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
    objective 
    personnel - useful in dialogue
    map - adds visibility
    password - potential to unlock nodes
    none / empty - routers, etc.

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

SetNodeEnabled



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