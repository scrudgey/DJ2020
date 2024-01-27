# level plan

both tactics and map view rely on setting data during plan mode that is used during level
this is kind of like level delta, but we don't want it to reset on retry / level fail.
this suggests a third component of level state: a level plan.
the level plan is created when we open the level for the first time in planning.
it is modified by the planning menu:
    insertion point
    extraction point
    tactics
        disguise
        social engineering info
        fake id
        keycard
    
level state = template + delta + plan

level state is instantiated:
    ✓ level bootstrapper
    ✓ GameData.TestInitialData()
        ✓ level Bootstrapper x 3
        ✓ mission plan debug start
        ✓ load VR mission
    ✓ load VR mission
    GameManager.LoadMission(Template)
        ↳ Mission Fail Retry
        ↳ Mission Plan Controller

So there are only two true code paths through instantiating level state in the proper game.
We need to come up with a larger system for handling these two cases, the rest we can fake.

when we retry a failed mission:
    * level template never changes
    * level plan does not change
    * level delta should reset

when we start from mission plan:
    * level template never changes
    * level plan is created by / modified / provided by planner
    * level delta should reset

level plan should have a reasonable default
gamedata will include 


move disguise out of player state
    disguise is set in level plan
    disguise propagates to leve delta
        delta value is what is polled during level play
        delta value can change during level play
    we could potentially set gun templates in level plan instead of in player state.

when do we update gamedata plans?
on levelstart seems like a good idea.











# how to handle knowledge state between template, plan, and gameplay

objective location:
    may be known from template (always known)
        player knows data node from intel
        player knows objective location directly from intel
    may be known when player purchases intel (known conditionally during planning)
        player discovers data node from intel
        player discovers objective location directly from intel
    may be discovered during gameplay (conditionally during mission)
        player discovers data node during scan
        player encounters the object / target

objective location knowledge is checked:
    during level planning:
        check template
        check plan
    during gameplahy:
        check template
        check plan
        check delta

 
networks:
    node visibility may be known from template (always known)
    node visibility may be discovered during planning (player purchases intel)
    node visibility may be discovered during gameplay (player scans node)

    edge visibility may be known from template (always known)
    edge visibility may be discovered during planning (player purchases intel)
    edge visibility may be discovered during gameplay (player scans node)

connection between objectives and networks:
    if a data objective location is known, its node is known
    if a node is known, the objective location is known

objective location may be randomized:
    if the objective location is not known at start of gameplay:
        location is decided between several possibilities defined in tempalte
        move / instantiate the gameobject at the decided location
    if the objective location is discovered before gameplay (template or plan):
        discovered location is decided between several possibilities defined in template
        discovered location is persisted in the plan- persistent between retries now
        move / instantiate the gameobject at the decided location



# case study: objective get loot


    objectives can be:
        data
        get loot
        use object
    planned:
        assassinate target


the only condition for this objective is that the player gets a loot with the correct name.
the location of that loot is currently controlled by the scene.
    the mcguffin is placed in a particular spot.

this means:
    1. no way to know the location of the mcguffin during level plan.
    2. no way to know the location of the mcguffin during gameplay map screen
    3. no way to randomize it.

solution:
    location of the mcguffin must be under template control.
        template defines a list of possible mcguffin spawns
    then during planning:
        the location might be unknown: location not fixed by template or plan
            in which case, objective location is not shown on map
            a player can purchase intel
                in which case, a choice of random location is fixed and persisted in level plan
        the location might be known:
            could be set by template (unlikely) or fixed by intel purchase
            in which case, objetive location is shown on map
    then during gameplay:
        the location might be unknown: location not fixed by template or plan
            a location is chosen from the random selection
            level is initialized accordingly (target is instantiated / moved / whatever)
            player does not know the location on the map
            the location might be discovered:
                player knows the location on the map
        the location might be known: location is fixed by template or plan
            level is initialized accordingly (target is instantiated / moved / whatever)
            player knows the location on the map

templates / scriptable objects cannot reference gameobjects in the scene.
but objects can reference scriptable objects.

so how does the solution work?
    1. per objective, the template knows of a set of possible locations.
        template ∋ objectives, but since objective (scriptable object) cannot reference gameobjects, instead we must have a set of spawn points that reference the objective.
        but then this cannot be known during mission planning.
        so it goes:
            scene -> object(s) -> reference to objective --(during level gen)-> reference to object(s) stored per objective
    2. when level gen is activated, we now enumerate a number of spawnpoints for the objective
        and presumably, the objective stores a prefab as well
        there is also stored a chosen location
    3. when in mission planning:
        if chosen location == null
            offer tactic
            if player buys a tactic,  
                choose a location from list
                <chosen location stored in plan?>
        if chosen location != null
            show location on map
    4. when mission load:
        if chosen location == null
            choose a location from list
            location not discovered <----- THIS PART IS WEIRD / INTERESTING
        spawn objective prefab at chosen location

what is the state and where does it live?
    objective template vs. objective plan vs. objective delta

    objective template: must be serializable as scriptable object per objective:
        serializable map:
            string idn -> position

    level plan: per objective:
        chosen position: string

    level delta: per objective:
        objective status
        visibility
        chosen objectiveDataProvider (?) / position provider


try a different angle:
    when we hit level gen util:
        we can save idns, but not gameobjects.
        we can save it on the level template, or directly on the objective scriptable objects.

        objectives store a list of idns.
        objectives store a selected idn. <- to be set in level gen util only when list of idns == 0

        take all objectives on the template:
            clear their data
        find all objective data providers:
            set the uuid
            take all objectives on the data provider:
                add this uuid to the objective list of positions
 
    in plan mode:
        template -> objectives -> list of positions and selected position.
        plan -> per objective -> list of selected positions
        if purchase info:
            plan selected position [objective] = random selection
        to determine what objectives to show:
            check if objective location known in template
            check if objective location known in plan

    in gameplay:
        instantiate an objective delta:
            if a location is chosen in plan:
                objective delta visibility = known
            if a location is chosen in template:
                objective delta visibility = unknown
            if a location is not chosen:
                choose a location
                objective delta visibility = unknown
            grab the data provider by idn
                use the data provider to instantiate the object
            set objective incomplete
            position provider:
                send selected position
        viewing map:
            
it is mostly clear except for plan state.
template: set objectives state in level gen
gameplay: set objective delta in level initialize.

how does the solution work for different objective types? 
    objective data
        during cybergraph.apply(plan) we can adjust visibility of nodes accordingly.
        we can also modify the plan objective data when intel unlocks data node.
    objective assassination:
        in theory, data provider can be the same as loot provider. just needs a position.
        during gameplay, the objective delta binds to the instantiated moving target.
        objective position() can provide that information on request.






# todo / implementation

* start by moving 3d map into planning mode.
* fix: load graphs during runtime
* don't show player location in mission planning
* instantiate objective delta inside level delta, use to track objective state
* figure out how to make map mode work in plan and gameplay both
* extend this model to all other objective types.
* load graphs during mission planning with applied visibility
* use deltas in gameplay to determine objective status

"spawn points" is actually more generic than that- but not sure what to call it

decide on escape menu: does it have selectable markers like planning mode? 
    might as well redesign plan mode tools




## bugs
handle mission failed for objective data
data objectives can select the same target node
partial visibility cyber graph doesn't work in mission plan mode
optional objectives - after action report
discover objective data node - discover objective




allow plan mode to modify objective deltas, visibility, etc
    the basic idea here: 
    objectives may have one or more potential locations to spawn from
    objective may have a single location but be unknown
        objective loot: potentialSpawnPoints
        objective data: cyber data nodes
    in the plan, we will purchase a tactic. this state will be reflected in plan.
    the plan will have to store information per objective.
        the information specifics will depend on objective type
            objective data: idn of a specific data node
            objective loot: idn of a specific potentialSpawnPoints idn
                (potentialSpawnPoints is also used by objective use item to identify the interactive )
    if information is set for the objective, its visibility is known as well.
        template visibility might be unknown, plan visibility is known -> objective delta visibility is known.

implement:
    * tactic can select and place idn in mission plan per objective
    * when creating delta for objective loot, data, use mission plan information
    * set visibility
    3. map view should take plan visibility into account

can't we use cyber node  locations for spawnpointlocations on objective data type? cleaner