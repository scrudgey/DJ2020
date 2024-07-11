using System;
using System.Collections.Generic;
using System.Linq;
using Nimrod;
using UnityEngine;
public enum DialogueTacticType { none, lie, deny, bluff, redirect, challenge, item, escape }

public class SuspicionRecord {
    public string content;
    public string pastTenseContent;
    public string stickyContent;
    public float lifetime;
    public float maxLifetime;
    public Suspiciousness suspiciousness;
    // public SuspicionDialogueParameters dialogue;
    public Func<NPCDialogueParameters, SuspicionDialogueParameters> getDialogue;
    public bool allowIDCardResponse;
    public bool allowDataResponse;
    public List<string> grammarFiles = new List<string>();
    public Dictionary<string, List<string>> grammarSymbols = new Dictionary<string, List<string>>();
    public int challengeValue = (int)Toolbox.RandomGaussian(30, 80);

    public bool stickyable;
    public bool stickied;
    public bool stickiedThisFrame;
    public bool pastTense;
    Grammar grammar;
    bool grammarInitialized = false;

    public void Update(float deltaTime) {
        lifetime -= deltaTime;
    }
    public bool IsTimed() {
        return maxLifetime > 0;
    }
    public void MakeSticky() {
        if (!stickied)
            stickiedThisFrame = true;
        maxLifetime = -1f;
        lifetime = 1f;
        stickied = true;
        // Debug.Log("making sticky suspicion record: " + content);
    }
    void InitializeGrammar() {
        if (grammarInitialized) return;
        grammar = new Grammar();

        grammar.Load("suspicion");
        foreach (string grammarFile in grammarFiles) {
            grammar.Load(grammarFile);
        }
        grammar.AddSymbols(grammarSymbols);
    }
    public DialogueTactic getTactic(NPCDialogueParameters nPCDialogueParameters, DialogueTacticType tacticType) {
        SuspicionDialogueParameters dialogue = getDialogue(nPCDialogueParameters);
        DialogueTactic tactic = tacticType switch {
            DialogueTacticType.bluff => dialogue.tacticBluff,
            DialogueTacticType.challenge => dialogue.tacticChallenge,
            DialogueTacticType.deny => dialogue.tacticDeny,
            DialogueTacticType.item => dialogue.tacticItem,
            DialogueTacticType.lie => dialogue.tacticLie,
            DialogueTacticType.redirect => dialogue.tacticRedirect
            // DialogueTacticType.escape 
        };
        return tactic;
    }

    public string GetChallenge(NPCDialogueParameters nPCDialogueParameters) {
        InitializeGrammar();
        SuspicionDialogueParameters dialogue = getDialogue(nPCDialogueParameters);
        if (pastTense) {
            return grammar.Parse(dialogue.pastTenseChallenge);
        } else {
            return grammar.Parse(dialogue.challenge);
        }
    }
    public string GetTacticContent(NPCDialogueParameters nPCDialogueParameters, DialogueTacticType tacticType) {
        InitializeGrammar();
        DialogueTactic tactic = getTactic(nPCDialogueParameters, tacticType);
        return grammar.Parse(tactic.content);
    }
    public string GetTacticSuccessResponse(NPCDialogueParameters nPCDialogueParameters, DialogueTacticType tacticType) {
        InitializeGrammar();
        DialogueTactic tactic = getTactic(nPCDialogueParameters, tacticType);
        return grammar.Parse(tactic.successResponse);
    }
    public string GetTacticFailResponse(NPCDialogueParameters nPCDialogueParameters, DialogueTacticType tacticType) {
        InitializeGrammar();
        DialogueTactic tactic = getTactic(nPCDialogueParameters, tacticType);
        return grammar.Parse(tactic.failResponse);
    }

    public static SuspicionRecord identitySuspicion(DialogueInput input) {
        SuspicionRecord record = new SuspicionRecord {
            content = "suspicious identity",
            maxLifetime = 120,
            lifetime = 120,
            suspiciousness = Suspiciousness.suspicious,
            allowIDCardResponse = true,
            getDialogue = parameters => parameters.IdentitySuspicionParams
        };

        return record;
    }
    public static SuspicionRecord gunshotsHeard() => new SuspicionRecord {
        content = "gunshots reported",
        maxLifetime = 120,
        lifetime = 120,
        suspiciousness = Suspiciousness.suspicious,
        getDialogue = parameters => parameters.GunshotHeardParams

    };

    public static SuspicionRecord crawlingSuspicion() => new SuspicionRecord {
        content = "crawling on the floor",
        suspiciousness = Suspiciousness.suspicious,
        allowDataResponse = true,
        getDialogue = parameters => parameters.CrawlingParams

    };

    public static SuspicionRecord c4Suspicion() => new SuspicionRecord {
        content = "seen placing infernal device",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 1f,
        maxLifetime = 1f,
        allowDataResponse = true,
        stickyable = true,
        getDialogue = parameters => parameters.PlacedSuspiciousObjectParams

    };

    public static SuspicionRecord robRegisterSuspicion() => new SuspicionRecord {
        content = "robbed a cash register",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 5f,
        maxLifetime = 5f,
        allowDataResponse = true,
        stickyable = true,
        getDialogue = parameters => parameters.RobbedRegisterParams
    };

    public static SuspicionRecord bodySuspicion() => new SuspicionRecord {
        content = "A body was discovered",
        maxLifetime = 120,
        lifetime = 120,
        suspiciousness = Suspiciousness.suspicious,
        allowDataResponse = true,
        getDialogue = parameters => parameters.BodyFoundParams

    };

    public static SuspicionRecord fledSuspicion() => new SuspicionRecord() {
        content = "fled from questioning",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 60f,
        maxLifetime = 60f,
        allowDataResponse = true,
        stickyable = true,
        getDialogue = parameters => parameters.FledQuestioningParams

    };

    public static SuspicionRecord shotSuspicion() => new SuspicionRecord {
        content = "someone was shot",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 120f,
        maxLifetime = 120f,
        allowDataResponse = true,
        getDialogue = parameters => parameters.SomeoneWasShotParams

    };

    public static SuspicionRecord noiseSuspicion() => new SuspicionRecord {
        content = "a suspicious noise was heard",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 10f,
        maxLifetime = 10f,
        allowDataResponse = true,
        getDialogue = parameters => parameters.SuspiciousNoiseParams

    };

    public static SuspicionRecord explosionSuspicion() => new SuspicionRecord {
        content = "an explosion was heard",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 60f,
        maxLifetime = 60f,
        allowDataResponse = true,
        getDialogue = parameters => parameters.ExplosionParams

    };

    public static SuspicionRecord brandishingSuspicion() => new SuspicionRecord {
        content = "brandishing weapon",
        suspiciousness = Suspiciousness.suspicious,
        allowDataResponse = true,
        stickyable = true,
        getDialogue = parameters => parameters.BrandishingWeaponParams

    };
    public static SuspicionRecord trippedSensor(string sensorName) => new SuspicionRecord {
        content = "tripped an alarm sensor",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 3f,
        maxLifetime = 3f,
        allowDataResponse = true,
        stickyable = true,
        getDialogue = parameters => parameters.TrippedAlarmParams,
        grammarSymbols = new Dictionary<string, List<string>>{
            {"sensorName", new List<string>{sensorName}}
        }
    };
    public static SuspicionRecord shotsFiredSuspicion() => new SuspicionRecord {
        content = "shooting gun",
        suspiciousness = Suspiciousness.aggressive,
        maxLifetime = 1f,
        lifetime = 1f,
        stickyable = true,
        getDialogue = parameters => parameters.ShotsFiredParams

    };

    public static SuspicionRecord tamperingSuspicion(BurgleTargetData data) => new SuspicionRecord {
        content = "tampering with equipment",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 3f,
        maxLifetime = 3f,
        allowDataResponse = true,
        stickyable = true,
        getDialogue = parameters => parameters.TamperingParams,
        grammarSymbols = new Dictionary<string, List<string>>{
            {"targetName", new List<string>{data.target.niceName}}
        }
    };
    public static SuspicionRecord tamperEvidenceSuspicion(string targetName) => new SuspicionRecord {
        content = "equipment was tampered with",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 120f,
        maxLifetime = 120f,
        allowDataResponse = true,
        getDialogue = parameters => parameters.TamperEvidenceParams,
        grammarSymbols = new Dictionary<string, List<string>>{
            {"targetName", new List<string>{targetName}}
        }
    };

    public static SuspicionRecord lootSuspicion(string lootName) => new SuspicionRecord {
        content = "stealing things",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 2f,
        maxLifetime = 2f,
        allowDataResponse = true,
        stickyable = true,
        getDialogue = parameters => parameters.LootParams,
        grammarSymbols = new Dictionary<string, List<string>>{
            {"lootName", new List<string>{lootName}}
        }
    };

    public static SuspicionRecord snoopingSuspicion() => new SuspicionRecord() {
        content = "suspicious snooping",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 60f,
        maxLifetime = -1f,
        allowDataResponse = true,
        stickyable = true,
        getDialogue = parameters => parameters.FledQuestioningParams,
        stickied = true
    };
}

