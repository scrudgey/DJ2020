# serialization, mutable state, spec

## The problem:
Various data objects in the game seem to have fields that split between an immutable template or spec, and some mutable state.

Examples:
* hurtable state has both an immutable `fullHealth`, and mutable `health`.
* gun has both a set of stats, `Gun` with maxClipSize, and a mutable `GunInstance` with `clip`, `chamber`, etc.
* `VRMissionData` splits between immutable mission specifications like mission type and victory conditions, and `VRMissionMutableData` which tracks time played, NPCs killed, etc.

One problem has appeared when spec is a scriptable object and contains mutable data. 
e.g., `NPCState` includes `GunInstance`.
When this happens, changing state during gameplay changes the scriptable object.

Another problem is related to serialization.
We desire to serialize the entire state, but when that includes immutable scriptable objects, it means serializing and deserializing a scriptable object, which is not recommended.

### Resetting

All of these examples include the need to reset the mutable state, templating from the immutable spec.
* NPC hurtable needs to be reset when NPCs are pooled.
Player hurtable needs to be reset when starting mission.
* gun needs to be reset when NPCs are pooled.
* `MutableMissionData` needs to be reset when mission is started.

*Question: When do we need to do a reset?*

Answer: When starting a new mission, resetting a character, resetting global state.

When we do a reset, we need a referene to the spec. That should be stored as a stable, immutable, external asset.

## Critical serialization goals

* The entire game should be fully specified by a data object that can be serialized / deserialized in memory.
Serialization should (indeed, must) include the mutable state.
When we save and load a player, it comes with the state of the health.

* Game should also be moddable.

# possible solutions

## Gun Instance model

`GunInstance` handles this by having `GunInstance` with mutable state act as a wrapper around `Gun`, with a constructor that takes a `Gun` as argument.
It then stores a reference to the `Gun` as spec, so it can be reset by instantiating a new `GunInstance` from that.

*Problem:* 
Seriaizling `GunInstance` means serializing `Gun` which is a scriptable object.

## Ditch serialized object

This would be more in keeping with possible mods.

*Problem:*
Dealing with json files will be harder for finding assets, etc.

## Copy all fields from spec

Spec for hurtable has fullHealth.
State for hurtable has health and fullHealth.

In this solution, the state does not hold a reference to the spec.
It holds all the fields of the spec.

*Problem:*
Nothing in the type system or code will ensure that all fields added to spec exist in state.

*Solution:*
Both spec and state implement the same interface.

*Problem:*
Nothing guarantees that the values get set.

*Solution:*
Use reflection.

*Problem:*
Nothing guarantees that the spec values don't get modified.

*Solution:*
It might or might not, but it doesn't matter...?
When the object is reset, we will (use the original spec?)
But the object doesn't have the original spec?
We can store the spec, but not serialize it...?
But then when we load, we can...?

**Problem: how are we supposed to serialize / deserialize this state?**


## Use a copy method

```
class Gun : ScriptableObject
{
     public string name;
     int currAmmo;
     public int maxAmmo;
     public float fireRate;
     public Image icon;
 
     public GameObject gunObject;
   
     void OnAwake()
     {
          //also instantiate gunObject here
          currAmmo = maxAmmo;
     }
   
    Gun GetCopy()
    {
        return Instantiate(this);
    }
 
     void Shoot(){
          //do some shooting
         currAmmo -= 1;
     }
}
```

Here, we put both the template and the state in the same object.
We can load the Gun object, and grab a copy with GetCopy().

**Problem: how are we supposed to serialize / deserialize this state?**

Isn't that a general problem for any scheme where we plan to serialize state with a spec?
The spec might in general contain references to sound effects.

How could it be that

## Solution: do nothing

Use a handful of ad-hoc, different methods here and there on various classes without ever systematizing.


**Problem: how are we supposed to serialize / deserialize this state?**

We will still have the problem of serializing anything that contains a reference to a scriptableobject spec.

The real challenge is serializing state, i.e., `GunInstance`.
`Gun` contains references to audioclips, graphics, etc. 
This is used to define `GunInstance`. 
But we want to serialize `GunInstance`.

## Reference paths to data objects

```
String klaxonRootPath = AssetDatabase.GetAssetPath(networkUtil.klaxonSound);
String klaxonRelativePath = klaxonRootPath.Replace("Assets/Resources/", "");

int fileExtPos = klaxonRelativePath.LastIndexOf(".");
if (fileExtPos >= 0)
    klaxonRelativePath = klaxonRelativePath.Substring(0, fileExtPos);

networkUtil.levelData.alarmAudioClipPath = klaxonRelativePath;
```

This sounds bad but it might be the least bad.

We would want to systematize the process of serializing a path to the scriptableobject spec, and deserializing by loading the spec., and resetting from spec.

States can contain a reference to the spec.
When serialized, the spec is stored as a path to a data object.
When deserialized, the data object is loaded from resources.

When resetting, just take the base values from spec.

What about generalizing to other sources of spec (mods?)

Then we would have to store a path to the mod spec.

At the end of the day, if the spec is a separate template, we want to keep the reference to it stable even through serialization / deserialization.
In other words,

## Another Possibility

Forget scriptable objects entirely.
Keep all templating in google spreadsheets.
Define spreadsheet -> XML -> TemplateObject -> Instantiate (load resources)

All assets (sound effects, etc.) are referenced by Resource path.

If this is a burden, we can design a way to go from serialized object -> XML

What is the advantage?
Everything, in this case, is defined by pure JSON or XML serialize / deserialize.

But is that the case? We always need some post process to create gameobjects, load assets, etc.

# The crux of the problem

**Problem: how are we supposed to serialize / deserialize this state?**

We always run into the problem of serializing anything that contains a reference to a scriptableobject spec.
If it doesn't reference a scriptable object, then we have the problem of serializing something with references to assets.
The real problem is about how we handle templating, ser/de, and asset references.

**Question: how does this problem present today?**

Answer: So far we have avoided serializing `GameData`. But as soon as we do, we will encounter the above issue.

In `LevelData`, we do resource pathing / loading to store the klaxon sound.
In this case, we are serializing a path to a template asset.

In `NPCData`, a scriptable object template, we store instances of GunInstance.
In this case, a template asset is storing mutable data.

We are mixing both concepts, causing confusion.


The question of when and how we reset state based on spec is important, because it will affect decisions.

On the one hand, suppose we use the spec as a template when instantiating a gun.
We copy over all of the stats to fields on the gun instance.
Now we hold onto that instance for all time, until the gun is disposed of in the game (sold, lost, etc.)
Resetting can be accomplished at any point with the information available on the instance.

But what about when we go to save?
We can serialize the floats, ints, strings, modifications, etc.
When we deserialize, where will we get the asset references?

It is natural to store the asset references in a scriptable object, but neither they nor the SO are serializable.
So we store a reference to the SO, and on deserialize we load assets from there.
This is necessarily done whenever we are referencing assets.

In Yogurt Commercial we handled this by referencing prefabs by name.

# Solution

During serialization, we store the reference to the template.
We only save the mutable data.

During deserialization, we load the template from resources from the path.
Then we apply the saved state.

So in other words, we instantiate the object from template, and we load the object from template.

Two methods:

1. Instantiate(template)
    Used when we need a fresh instance of the object.
2. Instantiate(template, state)
    Used when we want to load an object defined by state.

Question: doesn't this all require special code in all cases?
Answer: We already plan on writing special serialization code at all levels.
We just need to be aware of these intricacies, and have a plan.


# The plan

Separate the template and the mutable state.
We save only the mutable state and a reference to the template.

1. All instances are instantiated from templates. Either:
    a. Instantiate(template)
    b. Instantiate(template, state)

2. When we save, we save a reference to the template, and we save the mutable state.

GunState:           Template:Gun                   Delta:GunInstance 
NPCState:           Template:NPCTemplate           Delta:NPCDelta
LevelState:         Template:LevelTemplate         Delta:LevelState          
VRMissionTemplate   VRMissionData                  Delta:VRMissionDelta

PlayerState?         

Question: how does this interact with save and load?

Answer: Clearly, the loading is done with mutable state.
Does the loader need access to the template?
Take GunHandler for example.

# Execution
Implement this for gun first.

-------------------------------------------------------------------------

GunTemplate: ScriptableObject
    * clipsize
    * fire rate
    * type
    * graphic

GunDelta: 
    * clip
    * chamber
    * cooldowntimer
    * JSON Save()
    * GunDelta Load(JSON)

GunState : State<GunTemplate, GunDelta>
    * template: GunTemplate
    * delta: GunDelta
    * Instantiate(GunTemplate)
    * Instantiate(GunTemplate, GunDelta)
    * JSON Save / Serialize
        save template path
        GunDelta.Save()
    * Load(JSON)
        template = resources.Load(JOSN.template path)
        delta = GunDelta.Load(JSON.delta)

-------------------------------------------------------------------------

vv state without a template
GunHandlerState: IGunHandlerTemplateLoader
    * GunState primary
    * GunState secondary
    * GunState tertiary
    * activegun
    * JSON Save()
        primary.Save
        secondary.Save
        save active gun
    * GunHandlerState Load(JSON)
        primary = GunState.Load(JSON)

vv this one loads a state without having a template.
GunHandler: Monobehavior
    GunHandlerState state;
    * LoadGunHandlerState(GunHandlerState delta) 
        state = GunHandlerState.LoadGunHandlerState(template, delta)
    * GunHandlerState SaveGunHandlerState()

-------------------------------------------------------------------------


NPCTemplate: ScriptableObject
    * HurtableTemplate hurtable
    * GunTemplate primary
    * GunTemplate secondary
    * GunTemplate tertiary

Instantiate: Template -> State
Serialize: State -> JSON
Deserialize: JSON -> State






VRMissionTemplate
    NPCTemplate npc1Template
    mission type
    max number NPCS

VRMissionDelta
    status
    number live NPCs
    number NPCs killed
    time played

VRMissionState
    VRMissionTemplate template
    VRMissionDelta delta
    * LoadVRMissionState(IGunHandlerTemplate template) 
        delta = VRMissionDelta.Instantiate(template)
    * LoadVRMissionState(IGunHandlerTemplate template, GunDelta delta ) 
         delta = VRMissionDelta.Instantiate(template.primary)
        secondary = GunState.Instantiate(template.secondary, delta)
    * XML? SaveGunHandlerState()

VRMissionController
    VRMissionState











vv there is no such thing.
NPCInstance: State<NPCTemplate, NPCDelta>
    * template: NPCTemplate : ScriptableObject
        * fullHealthAmount
        * Gun primary
        * Gun secondary
        * Gun tertiary
    * delta: NPCState : ISkinState, IGunHandlerState, ICharacterHurtableState
        * GunInstance primary
        * GunInstance secondary
        * GUnInstance tertiary
        * activegun
        * string legskin
        * string torsoskin
        * float health
    * Instantiate(Gun)
        this is used when creating the gun for the first time.
    * Instantiate(Gun, state)
        this is used when loading a gun
    * Save / Serialize
        this writes GunInstance:
            template: path
            state: serialized gunstate
            
i think maybe there just is no universal relation between these things.
* template is immutable
* delta is mutable
* NPCInstance combines multiple templates and deltas, but corresponds to no single monobehavior
* GunInstance can be instantiated from a GunTemplate and GunDelta
* GunInstance can be saved
* GunHandler has delta but no template

maybe all i need to do is standardize the usages and naming conventions and move on.

State is just a template + delta, that's all.
it is saveable and loadable.


<!-- IGunHandlerState : 
    * GunState primary
    * GunState secondary
    * GunState tertiary
    * activegun -->
