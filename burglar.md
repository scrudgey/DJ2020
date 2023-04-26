# burglar

## tools
tools
    finger: operate objects (how will this work?)
        turn handle
    lockpick
    shim / traveler's hook
    screwdriver
    wirecutter

from inside the camera view, we need to be able to click on specific elements.

## effects

somehow, interacting with things in the attack surface needs to cause state changes in connected
world elements.

## feedback

it might be desirable to give specific feedback to the player regarding the state of the object they are attacking.

door:
    latched / unlatched
    locked / unlocked

### first effort

* show a handle with a keyhole in attack surface

* click lockpick to select it

* with lockpick selected, click on the keyhole to pick it

* success causes the door to unlock

### numeric keypad

use finger to press buttons

enter the correct code to unlock the door

### wire attack

unscrew four screws on an access panel

this reveals wires underneath the panel

snip the wire to trigger something

## security levels

different doors are susceptible to different attacks.

lowest security:
    door latch is vulnerable to traveler's hook.
    key is vulnerable to basic lock pick.
    door can be propped open.

highest security:
    key is vulnerable to pick
    door auto closes

configuring attack surface:
needs a collider
    needs a rigidbody
    needs to be on Object layer
    needs to have tag system to stop interloper business

* set up interactive elements in the attack surface
* when opening the attack camera, find the interactive elements in the attack surface
* find their bounds in the camera view
* create elements and place them in the camera view accordingly
* make ui elements buttons
* register buttons with canvas controller
* click buttons -> callback
    * specific logic 
* click to select tools
* text indicates selected tool
* cursor image for tool
* tools follow the cursor around
* pointer or finger for interaction
* toolbox icons
* animate tool when in use?
    * only when over element
* jiggle door handle when locked
* door handle rotation is weird!
* lockpicking takes time
    * display timer
* highlight attack surfaces
* both sides of door
* auto-close burglar view when door is opened
* rattle grate sound

lock keyway turns independently of the handle?
hotkeys 1-9 for tools
lockpicking takes skill
    periodically reset timer
suspicion when attacking a door

screw removed sound







# burglar overhaul and unification with hack

open questions:
* key system
* interface for burglar tools
* interface for hack
* different camera, window

## key system

do we have a separate keyring?
how do we show keys when keys are picked up?

separate keys:
    you must try each key in the lock?
    you can decode, copy, cut new keys
    realistic approach to using keys?
    annoying?
    easier to integrate with keyed-alike

## interface for burglar 

the idea is:

1. upon opening interface, no tools are out.
    the player can somehow select burglar tools or hack device.

2. the player selects burglar tools
    the bag appears, unfurls? unzips? 
    tools are available to use
    sound effects, animation

3. player uses tools as needed

4. player puts burglar tools away

### top level selection

ideas: burglar bag, laptop peek on bottom of screen
click one to open it
how to close it?
    an "x" appears

button bar on right?
    not so great. 

how to make the hack tool fit with this?