using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nimrod;
using UnityEngine;

[System.Serializable]
public class SoftwareTemplate {
    public enum SoftwareType { exploit, virus }

    public string name;
    public SoftwareType softwareType;
    public SoftwareEffect.Type principalType;
    public List<SoftwareEffect> effects;
    public bool infiniteCharges;

    public int maxCharges;
    public int virusHops;
    public int virusDup;
    public float loiterTimeLow;
    public float transitTimeLow;
    public float timeSpread = 1.5f;

    public List<SoftwareCondition> conditions;

    [JsonConverter(typeof(ScriptableObjectJsonConverter<Sprite>))]
    public Sprite icon;

    [JsonConverter(typeof(ObjectListJsonConverter<AudioClip>))]
    public List<AudioClip> deploySounds;
    public bool nameHasBeenSet;

    public SoftwareTemplate(SoftwareTemplate other) {
        this.name = other.name;
        this.softwareType = other.softwareType;
        this.principalType = other.principalType;
        this.effects = new List<SoftwareEffect>(other.effects);
        this.infiniteCharges = other.infiniteCharges;
        this.maxCharges = other.maxCharges;
        this.virusHops = other.virusHops;
        this.virusDup = other.virusDup;
        this.conditions = new List<SoftwareCondition>(other.conditions);
        this.icon = other.icon;
        this.deploySounds = new List<AudioClip>(other.deploySounds);
        this.nameHasBeenSet = other.nameHasBeenSet;
    }

    public SoftwareTemplate() { }
    public static SoftwareTemplate Download() {
        return new SoftwareTemplate() {
            name = "download",
            principalType = SoftwareEffect.Type.download,
            effects = new List<SoftwareEffect>{
                new SoftwareEffect(){
                    type = SoftwareEffect.Type.download,
                }
            },
            maxCharges = 1,
            icon = null
        };
    }
    public NetworkAction ToNetworkAction(List<CyberNode> path, CyberNode target) {
        // float lifetime = effect.type switch {
        //     SoftwareEffect.Type.compromise => 10f,
        //     SoftwareEffect.Type.download => 10f,
        //     SoftwareEffect.Type.scan => 3f,
        //     // SoftwareEffect.Type.unlock => 5f,
        //     SoftwareEffect.Type.unlock => 6f,
        //     _ => 1f
        // };
        // TODO:
        float lifetime = 2.5f;

        NetworkAction networkAction = new NetworkAction() {
            title = $"uploading {name}...",
            softwareTemplate = this,
            lifetime = lifetime,
            toNode = path[path.Count - 1],
            timerRate = 1f,
            payData = target.payData,
            path = path,
        };

        return networkAction;
    }
    public VirusProgram ToVirusProgram(CyberNode target, CyberGraph graph) {
        return new VirusProgram() {
            graph = graph,
            position = target.position,
            currentNode = target,
            hops = virusHops,
            maxHops = virusHops,
            duplication = virusDup,
            timer = 0,
            effects = effects,
            waitAtNodeTimeRange = new LoHi(loiterTimeLow, loiterTimeLow * timeSpread),
            transitTimeRange = new LoHi(transitTimeLow, transitTimeLow * timeSpread),
        };
    }

    public static SoftwareTemplate Empty() {
        return new SoftwareTemplate() {
            name = "",
            principalType = SoftwareEffect.Type.scanAll,
            effects = new List<SoftwareEffect>(),
            conditions = new List<SoftwareCondition>(),
            maxCharges = 1,
            icon = null,
            nameHasBeenSet = false
        };
    }


    public int CalculateChargesCost() {
        return maxCharges * 2;
    }
    public int CalculateEffectsCost() {
        return effects.Select(effect => effect.CalculateDesignPoints()).Sum();
    }
    public int CalculateConditionsCost() {
        return conditions.Select(condition => condition.Cost()).Sum();
    }
    public int CalculateHopsCost() {
        return virusHops;
    }
    public int CalculateDupCost() {
        return virusDup;
    }
    public int CalculateTotalViralCost() {
        return CalculateHopsCost() + CalculateDupCost();
    }
    public int CalculateSize() {
        return maxCharges + effects.Select(effect => effect.CalculateSize()).Sum();
    }
    public int TotalDesignPointsCost() {
        int baseCost = CalculateChargesCost() + CalculateEffectsCost() + CalculateConditionsCost();
        if (softwareType == SoftwareType.virus) {
            baseCost += CalculateTotalViralCost();
        }
        if (softwareType == SoftwareType.virus) {
            baseCost += 10;
        }
        return baseCost;
    }

    public void SetRandomName() {
        Grammar grammar = new Grammar();
        grammar.Load("software");

        string key = Toolbox.RandomFromList(effects).type switch {
            SoftwareEffect.Type.compromise => "compromise",
            SoftwareEffect.Type.download => "download",
            SoftwareEffect.Type.scanAll => "scan",
            SoftwareEffect.Type.scanEdges => "scanEdge",
            SoftwareEffect.Type.scanFile => "scanFile",
            SoftwareEffect.Type.scanNode => "scan",
            SoftwareEffect.Type.unlock => "unlock",
            SoftwareEffect.Type.none => "compromise",
        };

        grammar.SetSymbol("category", key);
        name = grammar.Parse("{main}");
        nameHasBeenSet = true;
    }

    public SoftwareState toState() => new SoftwareState(this);


    // public static List<string> randomRestaurantNames = new List<string>{
    //     "Soy Beef Bucket",
    //     "tikka masala sushi",
    //     "empanada fusion",
    //     "samosa pizza",
    //     "Sizzling Lunch",
    //     "Dumpster soup",
    //     "Sizzling Hot\nSquid on a Stick",
    //     "3AM BEEF",
    //     "Angel Jesus's\nFresh Manpower Factory Outlet",
    //     "Baked Salad with Cheddar",
    //     "Laser-heated\nCurrywurst Supreme",
    //     "sushi burrito",
    //     "biryani bucket",
    //     "frozen hummus hut",
    //     "New York style sushi"
    // };
}