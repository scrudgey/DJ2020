# Save system

placeholder objects that load an NPC at runtime
spawnpoints that randomize the placeholder at runtime
placeholder for pickups
spawnpoint for pickups

these can be used together to design modular levels. more or less enemies, different types of enemies.
the state here:
    * skin
    * weapon loadout
    * patrol route


for levels with multiple screens loaded at runtime, we still need to persist state and load it at runtime
    * destructible walls
    * node components
this is different because we let unity serialize/deserialize the scene, but then we modify the scene state after loaded to reflect previous state.
if it's the first time loading the scene, use the unity state as-is.
    then, load placeholder objects.
if we've loaded the scene before on this mission, then we want to apply saved state.
we don't need to track which levels have been loaded, we just need to check for saved state.
    if it exists, load and apply the state.
how to maintain persistent ids?
the editor widget will find all persistent components and assign them a UUID.


LOAD:
    on scene load, check for saved state
    if it exists, load and apply the state.

SAVE:
    on scene exit, save the state.
    this applies to every 

if there are objects spawned at runtime, they won't be in the unity editor. this includes all pooled objects.
    part of loading the scene is initializing object pools where necessary.
    the save state must include data about object pool.
        when reinitializing an object pool, UUID won't match.
        so object pool save state includes UUIDs. after initializing the pool, we apply UUIDs to the pooled objects.
            but what about subcomponents of pooled objects?

here's the issue: state belongs to components, but objects as a whole are pooled.
we can apply UUIDs to instantiated pool objects, but then we need to know parenting relationship to apply UUID to their components.


originally the hope was that we could just save each component data and then apply it at load.
but not all components will be present if some were dynamically instantiated.
    if some were instantiated at runtime, then they must be instantiated at load.
    so now our trick of applying UUIDs via editor tool doesn't apply.
    some saved UUIDs will resolve to scene components, that's good.
    some saved UUIDs will not resolve to scene components because they were destroyed.
    the dynamically generated UUIDs won't resolve to anything. we need to know how to instantiate them at load.
        we instantiate gameobjects from prefab.
        then we 

so, we have to track the pooled objects, and save their state


loading screen / load async.
possibility 2: use level generator tool to serialize *everything* to a file, reconstruct it dynamically on load
this includes:
    instantiating all level geometry
    instantiating patrol paths
    instantiating lights

1. these are kept as prefabs with root savemarker
2. savemarker contains a reference to the prefab path
3. all components with volatile runtime state are ISaveable, allowing them to save/load from serializable state.
    now every gameobject and component has a UUID.
4. resolving gameobject or component references is a thing.

now at runtime, when you unload a scene, you save it first into its own temporary level state (new level object.)
then when returning to the scene, it is readily reconstructed.

enables future level editor <--- extremely important feature.

we only need to persist that which can change at runtime or references to something outside the prefab.
so the main difference here: this approach does not start by loading the scene.
* there is no difficulty in loading new objects, destorying old objects, everything is handled identically.
    no difficulty in resolving UUIDs. everything starts from state on disk!
* we just serialize and deserialize at the scene level.
* can still allow a bare gameobject persistent object, special case, if the components are important.
    bare gameobject with persistent object component; check special case box or smth

nonserialized on 711 scene and solution:
    skybox                      (loaded separately as level configuraton)
    ladder                      (prefab)
    storechime                  (prefab)
    doors                       (prefab)
    directional light           (persistent directional light)
    spot light                  (persistent directional light)
    mains (power source)        (prefab)
    wan (network source)        (prefab)
    environment sound (random source)   (prefab)
    point light                 (persistent directional light)
    park plants                 (prefab)
    ground plane                (config or level default)
    environment sound (crickets, fire)  (prefab)
    patrol route                (persistent patrol route)
    patrol zone                 (prefab / persistent component)

after loading, bake navigation mesh!
last resort: we can start by loading a speciality scene.

main challenge: apply persistent gameobject components to everything, and specify prefab paths.
 PrefabUtility.GetCorrespondingObjectFromSource()
 AssetDatabase.GetAssetPath(parentObject)

so how will this interact with VR levels?
1. VR mission dialogue will set up 
    level data
    player data
    VR mission data
        NPC data
        number of enemies
        time limit
        objectives
        respawning
load the level, which includes spawnpoints / placeholders.
loading should include initializing prefab pools
then resolve placeholders as part of post-load.
    resolving NPC placeholders means:
        0. determine which placeholders to use
        1. grab a prefab from the prefab pool
        2. load persistentObject state
    resolving player placeholder means:
        0. determine which placeholder to use
        1. spawn playerobject
        2. load player data


deep save: use inspection to determine every component, and every field of every component. use asset database to get path to all assets.
at runtime, create empty object, 
this is unrealistic for now, requires serializing too many built-in components (meshes, etc.)



public static string GetGameObjectPath(GameObject obj)
{
    string path = "/" + obj.name;
    while (obj.transform.parent != null)
    {
        obj = obj.transform.parent.gameObject;
        path = "/" + obj.name + path;
    }
    return path;
}



VR mission data
    NPC data
    number of enemies
    time limit
    objectives
    respawning


1. NPC data
2. player spawn point
    do not find player object on initialize. spawn player object according to playerdata.
3. NPC spawn point
4. async loading and loading screen









# fixing the save system

## problem statement 
Save system uses AssetDatabase, which is UnityEditor only.
Therefore we cannot build a standalone app.

Secondly, there are at least three separate ways that we are handling state:

three distinct approaches to serializing state
1. player state duplicates fields in template and in state. instantiating player state involves a lot of copying.
2. a state object with a template, and mutable fields
3. a state object with a template and a delta. i like this, keeps all cleanly separate.
modify playerstate, playertemplate, Default() player template, PlayerState Instantiate(PlayerTemplate template), PlayerState DefaultState(),
NPCSpawnPoint and what is IPlayerStateLoader ??

adding etiquette was not easy.
even after setting default player state, it didn't work for player
because we were loading from the saved VR template.
then getting NPC to work was difficult.
i had to modify NPCTemplateJSONConverter, even after modifying the scriptable objects.

1. take a principled stance on how to handle state
    ideal:
    State
        Delta
        Template
    template contains references to resources. delta contains mutable primitives.
    on save, save state: reference the template by string.
    this works when templates can be scriptable objects.
    but VR mission template wants to 
    
2. enumerate all current save related systems

it is okay to use asset serializer in templates that are scriptable objects! right?
NPCTemplateJsonConverter
    this one is good! it manually stores resource references as strings without using AssetDatabase!
ScriptableObjectJsonConverter
    is this the problem? only used in player template

problem: 
NPCTemplateJsonConverter uses Toolbox.AssetRelativePath on write.
PlayerTemplate uses ScriptableObjectJsonConverter on read/write.
therefore it can't write in runtime.
we need a different system.

the system needs to be able to resolve a Resources/ path given an asset.



how could we do something like AssetDatabase.GetAssetPath at runtime?
we would need to enumerate all resources on start, and build a dictionary.