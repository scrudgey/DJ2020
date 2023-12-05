# hacking v2

graph overlaid on top of world view- seamless integration with regular gameplay.
use hack tool to attach to nodes.

readability is achieved by highlighting node connections when node is selected:
    

## needs

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
    pay data
    objective
    personnel
    map
    password

nodes start locked by password

locked nodes cannot be interacted with (except to unlock)

locked nodes have less visibility

## visibility
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

