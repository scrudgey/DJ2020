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

show a handle with a keyhole in attack surface

click lockpick to select it

with lockpick selected, click on the keyhole to pick it

success causes the door to unlock

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




how will this work?

set up interactive elements in the attack surface

when opening the attack camera, find the interactive elements in the attack surface
find their bounds in the camera view
create elements and place them in the camera view accordingly

make ui elements buttons
register buttons with canvas controller
click buttons -> callback
    specific logic 
click to select tools
text indicates selected tool
cursor image for tool
