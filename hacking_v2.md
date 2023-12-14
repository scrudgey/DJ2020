# hacking v2

graph overlaid on top of world view- seamless integration with regular gameplay.
use hack tool to attach to nodes.

readability is achieved by highlighting node connections when node is selected:
    

## needs

readibility: player should be able to see / navigate all nodes on the network and understand what they refer to in physical space
    selecting a node highlights the node and all connected to it.
    from there we need to be able to navigate to connected nodes.
    if the connected node is offscreen there still needs to be an onscreen indicator that allows us to navigate to it.
    selecting a node shows in in physical space- so we know what object (camera, datacenter) it refers to
        this also helps us locate the objective

what about hopping to a non-connected non-highlighted node?
    1: you can't select them: only mouseover them
    2: it's fine, the important thing is illustrating connectivity on mouseover.

grabbing data should be easy as clicking something on the overlay.
    should provide a gratifying experience

should be able to read off data in data nodes from the overlay directly
    allows player to locate specific data
    can perhaps also indicate value or a rough range of value

edges need to support multiple stylization
    deselected node -> deselected node
    selected node -> deselected node
    selected node -> mouseover node
    mouseover node -> deselected node
    compromised node ->

data can be located in physical object outside cyberspace
    waste paper basket
    paper on desk



## node model

different types of nodes
    pay data - yields $
    objective  data
    personnel data - useful in dialogue
    map data - adds visibility
    password data - potential to unlock nodes
    none / empty - routers, etc.
    utility - camera, alarm

iconography
    data node should be stacks

nodes start locked by password
unlocked nodes are compromised

locked nodes cannot be interacted with (except to unlock)

locked nodes have less visibility

nodes can be accessible or not
    accessible: connected to player cyberdeck in physical space, or connected to an unlocked/compromised node

### visibility
    nodes / locations
    node types
    node content
    node password level
    datastore content
    datastore content details (value, type)
    node connections

## verbs

discover
    discover nodes / locations
    discover node types
    discover node content
    discover node password level
    discover datastore content
    discover datastore content details (value, type)
    discover node connections

unlock
    hack password
    enter known password

exploit
    converts a vulnerable unlocked node into a compromised node

activate
    toggle security camera on/off
    enable/disable power substation
    lock/unlock door
    deactivate alarm

upload virus
    propagates between edges
    activate at each node
        unlock / discover / activate?

## software and hardware

goal is: hardware has some fixed slots for software (3 maybe) that you load up before the mission
you acquire this software on the black market
the software enables the different verbs
possibly single-use or x uses
possibly it can be used on locked nodes if it has the correct exploit
    i.e. bypasses encryption on level 2 security camera nodes

parameters:
    verb:   discover, unlock, activate
    uses: 1, 2, unlimited
    type: targeted, virus
    if virus: number of hops

like crafting a spell in morrowind, it requires a specialist and money
more money for higher level parameters

if the goal is to download data, we should ensure that the player can't get stuck because they used up all their software.
    there should be something like a backup

and we need a better name for software & using it
and hardware
reference gibson

virus takes time to propagate

hardware:
holds x software
download speed
space for paydata?


### using software

you can apply software to any node you are directly connected to (using cyberdeck in physical space)
or to any node you have access to on the net
    new concept: access / compromise





# milestone 1

readable network
    show connections on mouseover
    allow a node to be clicked / highlighted
    when a node is highlighted, slide out an info panel with info on the right

discoverability

cyberdeck tool item

# milestone 2

complete UI iconography

# milestone 3

software / hardware interaction

# milestone 4

discoverability in planning mode

somewhere in here: randomization of datanodes




# UI

various line conditions:

1. compromised edge
2. selected edge
3. mouseover edge
4. unselected unmoused edge


# readibility

the basic idea behind readibility is that we need to separate camera movement due to player
from camera movement to inspect graph.

questions:
how to allow graph inspection at a glance? being tied to player location is a problem.
how to handle overlapping nodes?
when a node is drawn on top of the object: what then?

1. navigate to center of mass of graph and adjust zoom
2. snap nodes to a grid, and add a small callout line to the object they control
3. allow nodes to interact via 2d physics and add callout line
4. click on a node: jump camera to that node.
    info pane contains buttons to jump to neighbors

free camera mode?
    WASD or some buttons on screen move camera?
        blinking -> arrows on edges to indicate?



# code refresh

node enabled/disabled
vs. compromised / not
enabled is general: take a node out of the network.
    happens when components are destroyed.
compromised is specific to hacked nodes.
similar in that they might both disable a node (is that true?)
different in that a disabled node won't allow hacks to pass through.

SetCyberNodeState:
    what "state"? compromised
    called by:
        VR node open
        WAN node start (this should be baked into node state)
        hack controller

trace out all code paths from:
load:
    initialize level
    find all cyber components and set cyberComponents[component.idn] = component
    RefreshCyberGraph
        transfer state data -> components: foreach id, node in the graph data:
            component[idn] state = data state
    OnCyberGraphChange?.Invoke
        update UI

(state change moves from graph data to components.)


a node is hacked
    hackcontroller magic
    hack updates-> completes
    SetCyberNodeState(data.node, true);
        node.compromised = state;
        RefreshCyberGraph
            transfer state data -> components: foreach id, node in the graph data:
                component[idn] state = data state

(state change moves from graph data to components.)

a cyber component is destroyed
    OnDestroy -> GameManager.I?.SetNodeEnabled
        this fetches component, sets enabled on component and node, refreshes graph as necessary.

Q: how are nodes linked?
Q: what possibility of linked / cascading updates?
Q: when do we drop all the stored state of the overlay?



we have now abstracted everything such that a clean slate approach can be attempted.

1. icons for each type of thing.
    data model support!
    node type
    perma-compromised node

1.5 locked / unlocked nodes
    affects visibility of node type

2. support discoverability of edges.

2.5 mouseover highlight edges

3. selection / navigation

update cyber component model:
* datastore
cash register
alarm component reference


            // Gradient newGradient = new Gradient();
            // GradientColorKey[] colorKey;
            // GradientAlphaKey[] alphaKey;
            // // Populate the color keys at the relative time 0 and 1 (0 and 100%)
            // colorKey = new GradientColorKey[2];
            // colorKey[0].color = indicator1.image.color;
            // colorKey[0].time = 0.0f;
            // colorKey[1].color = indicator2.image.color;
            // colorKey[1].time = 1.0f;

            // // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            // alphaKey = new GradientAlphaKey[2];
            // alphaKey[0].alpha = indicator1.image.color.a;
            // alphaKey[0].time = 0.0f;
            // alphaKey[1].alpha = indicator2.image.color.a;
            // alphaKey[1].time = 1.0f;

            // newGradient.SetKeys(colorKey, alphaKey);
            // renderer.colorGradient = newGradient;


because cyber node indicator needs to have information about data, it needs to configure with the component as well.
and eventually we will need to bring in levelplan / levelstate as well for discoverability.

what data information is shown on ui?
* type
* value: value is type?
- filename
what is shown when locked?
* type
* lock 

what is shown when locked?

### visibility
visibility -1:
    node not visible

visibility level 0:
    node visible, no information

visibility level 1: 
    node type is known
        data / normal / utility

visibility level 2:
    connections known

### data visibility
data visibility 0:
    data node is known

data visibility 1:
    data type is known

data visibility 2:
    data info is known
        value
        type / number of map data

### lock visibility
lock visibility:
    lock level: 0, 1, 2, 3

scenario: a player discovers a node (computer) while walking around
    (UI fx: node discovered)
the node is at visibility 0: no information, locked
lock level 1.0
user connects cyberdeck
    it is locked, but we can still run scanner v1.0
    to run others (datathief, compromise) we need to unlock it first.
user runs scanner v1.0 and discovers it is a datanode, containing $$ paydata.
    (visibility level 1, data visibility 2)
to access the data, we need to unlock the node.
user runs ice cutter v1.0 and it takes a few seconds to break the encryption
now user runs datathief, and the download begins
in a few seconds, the download completes.
the data now stolen, the UI reflects that the data node is now empty.


## node state
node type:
    normal, utility, datastore

node status (dynamic):
    invulnerable -> vulnerable -> unlocked -> compromised

visibility:
    unknown -> known -> identified -> mapped

lock level: 
    0, 1, 2, 3

data sink: yes/no

data stolen: yes/no

invulnerable: basic state of all nodes
vulnerable: connected to a compromised node
    cyberdeck
    WAN
    compromised node
unlocked: password level 0
compromised: user has deployed exploit

## software use patterns

scan:       deploy against any vulnerable node
unlock:     deploy against any vulnerable locked node
download:   deploy against any vulnerable unlocked node connected to player or WAN
exploit:    deploy against any vulnerable unlocked node

viruses are unique:
    deploy against any vulnerable node
    they hop and activate against all connected nodes

in order to run datathief:
    node needs to be unlocked
    node needs to be vulnerable
    download takes an amount of time, adjusted by cyberdeck speed
in order to run scanner:
    node needs to be vulnerable
in order to run compromise:
    node needs to be unlocked
in order to run password breaker:
    node needs to be vulnerable



because scan can be deployed against an invulnerable node, this means that all the node information 
type, content, links, etc.
can be displayed whether or not node is locked.
hence, lock is separate UI element.

questions:
    1. does unlocking the node increase visibility?
        maybe
    2. scanning doesn't require unlock, but other attacks do?
        yes
    3. what other attacks exist besides scan?
        scan, unlock, download, exploit
    4. state model for visibility
        it could go right on level delta graph
        but at least some of it comes from level planning phase too.
            in theory, player can view graph network on the map,
            unlock more graph visibility on the map by paying someone,
            and then construct their plan around the location of the objective.

            the plan needs to stay permanent- since the player paid for it.
            but anything not in the plan is randomized.
    5. scanner & visibility
        does visibility increment? or just set?
        is visibility capped by scanner version?
        does visibility target specific information? i.e. data type, connections, etc

UI goal 1:
    display nodes respecting visibility
    node icon just shows type: normal, utility, datastore, empty datastore
    all details go into info pane!
    color indicates node status
        invulnerable -> blue
        vulnerable -> yellow
        compromised -> red
    lock is indicated as well
    lock strength is indicated.

UI goal 2:
    info pane



state model:
plan info is persistent across runs.

during gameplay, at level load, we:
    1. load the graph
    2. apply the plan info
    3. randomize the rest as indicated by level.

during plan, we:
    1. load the graph
    2. apply plan info
    everything not in the plan is hidden/unknown

keep the graph just in delta: this doesn't work for plan.
keep the graph just in plan: too much persistence.
so we need a general model.

we either load the graph and apply plan,
or load the graph, apply plan, and apply randomization

applying plan + randomization means populating data files, a concept that does not exist at the level of graph today!
but since we want to represent this independent of the state of the level (maybe it isn't even loaded!)
that means we need to promote datafile to the level of the graph model.
    https://stackoverflow.com/questions/12237268/how-to-implement-xml-serialization-with-inherited-classes-in-c-sharp
    this makes it cleanest for sure:
        we will have the graph model and we will mutate it by applying plan and/or randomizer.
        then we can represent it in planning phase, in in-game map, in the level, etc.
        single source of truth for all state.
graphstate includes the graph template but also: 
    hacks in progress, downloads in progress, viruses, etc.
    visibility knowledge.
    node deltas. 
when we save the graph template from the editor, data files may or may not be set.
this means that in the case of (for example) planning mode, when we apply just the plan to the template, some (most) data nodes will not have data files.
    it also means in plan mode we want to somehow represent that data file info, info which previously was said to live in the info panel only!



plan could also be more general than just network: it could reveal the randomized location of other objectives for example.

graph template:
    nodes
    data node subclass
        data file may or may not be null
randomization:
    required data file
        acceptable nodes: any, subset
        data template
    infill data file
        data template:
            random types
            random values

details of randomization can wait-- just be aware of the potential downfalls and complications.
work it out later.

this is a lot of new ideas and information. how to proceed?
1. allow subclasses of nodes in graph.
    allow cyberdatanode to specify datafile.
    change therefore everything else around this.
    levelstate includes a graphstate.


right now i want to simplify and unify ApplyNodeState and OnStateChange

what is the point of graph node components at all?
1. to give nodes a location in space
2. when a gameobject is destroyed, to notify graph state
3. give level gen util something to key off of

then:
1. NO component should inherit from nodecomponent. keep it separate and top-level. one per gameobject.
2. when writing graph file, connect each child component via idn.
    after graph load, all each component to bind to node via idn lookup.
    when graph changes, component state changes to reflect.
    possibly eliminate transfernodeState functions.
    the support components (alarm button, alarm sensors, etc.) subscribe to changes
3. data store is a special type of cybernodecomponent.
    it is GraphNodeComponent<CyberDataComponent, CyberDataNode>
    the only question: is there any point in separating the monobehavior component stuff? no for now.
    eliminate paydata from here.
        (how to set specific datafile from editor? use randomizer interfaces.)
4. consider alarm sensors: frequently they use game level state to affect graph state.
    in this case we propagate from world model to data model.

can Node be record class? no: we prefer to store references.

when building graph, get all subcomponents that implement an interface NodeBinder and populate their idns
then at runtime after loading graph bind all NodeBinders
NodeBinder uses their Node reference directly to call setter methods that trigger graph updates
and graph updates cause the NodeBinder to recieve an update
and nothing in the NodeComponent is used.


* handle on component destroy -> disable node
* alarm component state
* powered component state
* handle attack surface wires
* datafile indicator is apparently incorrect
* wan icon is incorrect
* alarm central and laser grid should be utility
* camera -> utility
* node locations incorrect
* set all node types in editor
* implement first round of data model changes
* in cyber UI:
    * icons
    * locks
    * datafile present
    * color indicates status

* data indicator is hard to read
* locks:
* set lock and data color?
* line thickness & color is incorrect
* set line color:
    * from compromised -> vulnerable: orange
    * from compromised -> compromised: red


snap line points
basic selection / navigability
    -> needs to interact w camera
disable hacking as it works today



# navigability 

regular mode:
    input from input controller
    input -> character controller
    camera input from character controller
    camera input -> camera
overlay:
    input from input controller
    if overlay is active:
        input -> camera??? (something new here?)
    if a node is selected:
        camera input from overlay controller
        camera inpur -> camera ?

problem: WASD free movement conflicts when player takes out cyberdeck tool and cyber overlay comes up.
problem: when cyber overlay is active, player movement stops.
    only stop movement once a node is selected
    deselect node when:
        player clicks close button on info panel
        player closes cyber overlay
one possible solution:
    intermediate cyber overlay shows nodes and allows player to move around and connect to a node, but does not switch to 
    full cyber overlay mode until player clicks on a node.
        mouseover targets different nodes- controls like a gun - exclusive of guns
        nodes highlight in a way when player is in range- connects when mouseover- node changes color to indicate vulnerable
        click node-  drop into cyber overlay
first draft: ignore free movement for now.



* when a node is active: 
    * camera input comes from cyber overlay
    * player is not controlled.
* allow zooming in/out
* there is some sort of offset.
* show info pane
    * show close button
* configure info pane
    * title: name
    * icon
    * type
    * status
    * datafile
    * lock
* neighbors
    * one button per neighbor
    * scrollrect



indicator in overlay display
utility node
icons
paydata
dont zoom when mouse over neighbor scroll view
clear neighbor mouseover when jumping nodes



power node info pane
alarm node info pane

visibility








changing overlay
    preserve node if a corresponding node exists, otherwise deselect.
make nodes button-like
    mouseover effect indicates clickability
    cursor changes
    edges are highlighted