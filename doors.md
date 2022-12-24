# doors

key requirements

* door should open / close smoothly when interacted with
* play sounds on open, close
* play sound on try to open but can't
* we need a very satisfying unlock sound

* door can be auto-closing
    * open door closes when player moves away

* door can open one-way, or both ways
    * selectable in / out

* door can be pushed
    * only when unlatched

* slightly offset from hinge axis when open, to prevent z-fighting on flush jamb

* door can be locked / unlocked
    locking systems:
        * physical lock
        * latch
        numeric keypad
        electronic access control

doors can be unlocked with keys
    physical key
    keycard
    badge access
    numeric key

* locked door can be picked

* latch can be disabled
    * slip the latch while closed
    * push door slightly when unlatched
    disable latching entirely (tape latch? damage?)

* doors have knobs
    * handle
    knob
    push bar
    no knob
* turn handle when door is activated

* selectable parameters in editor

* indicate feedback to player?

* doubledoors

* physical keys
    * key can be picked up
    * keyring appears in burglar tools when player has more than one key
    * keep track of key ids
        * locked door has a key id
        * key pickup has a key id
    key can be copied from key code?
        impressioning / photographing keys?
        what would be the gameplay purpose here?
        IRL you want to steal a key without anyone noticing. but in game, you have only minutes
    try multiple keys in the door? realism
    key cutter -> generate master key? 
    standard key codes
    guards can drop keys


doubledoors can be pushed?

doors can be bashed

doors can have trip sensors
    can be disabled with magnets

door interaction with NPC
place doors in levels
maybe auto-use key in door when using door



# NPC interaction

## NPC should be able to move through unlocked door

Modify TaskMoveToKey to include logic for detecting when path passes through a door.

Then the question is: how to handle it?

Pause movement when I am in range. 
Open door. 
Wait until door is open.
Resume movement.

## NPC should navigate through only doors with access

* unlocked doors

* doors they have the key to

This involves manually setting navmesh areas and masking them out selectively.
Mapping from door keyId to navmesh area is tricky.
    fixed mapping.
    key1, key2: common keyed-alike.
function: NPC key state -> NavMeshQueryFilter
Better editor widgets can make the keyId obvious, and navmesh areas can be named after keyIds.

now the only challenge is setting up the navmesh properly.

## test environment

set up a test environment level.

spawn an NPC.

instruct the NPC to move into a room, path takes through door.
this could be a patrol route.



how does it work with keyed-alike?