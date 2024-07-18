# keys and passwords

key system:
    more detailed key usage enabling keyed alike attacks 
    use passwords in analogous way
    password data unlocks passwords
        as is, this doesn't make much sense:
        if password data can unlock a node, then you have spent an unlock to get an unlock.
        if password can unlock 1 or mode nodes, maybe, then it is different?



the simplest example beyond having a single key pickup is to have multiple keys that unlock different doors
it's obviously realistic, but does this enable interesting gameplay?

possible gamplay mechanics
    1. multiple keys
    2. keyed-alike keys
    3. master keys
    4. key cutter / key copy

* physical keys
* passwords
* rfid keys
* keycards


moving to multiple keys enables keyed-alike attacks
the main gameplay example:
    user acquires a key and it unlocks only some offices, not the stairwell.
    they need to acquire a different key, in some way, to open the stairwell
pros:
    there are benefits to not opening the entire map to the player all at once
    some strategy is created
    and 
    some interesting puzzles could be created (key behind locked door, etc.)
    these are good reasons
cons:
    hard to know what keys are needed where, and where to find them
        this is probably desireable / realistic
        the user doesn't know they have a key for the door util they try their keys, just like real life
        it takes time
    UI is a challenge
        burglar vs. normal view



## UI challenges

if the user is to cycle through multiple keys, it must be quick and easy to do.
ideally, click once on the door, click again to try the next key
once the correct key is known, use it always
allow the user to try a different key if they think they know the right one

## possible solution


## physical keys
### normal view
1. user right clicks on a locked door. a context dropdown appears
    icon of key, key id
2. the first untried key is right below the cursor
3. user left clicks to try key
    a. if it works, the key is now known to be correct and immediately highlighted every time
    b. if it fails, the context dropdown changes to mark the key incorrect and put it last
        the next untried key is right below the curosr

### burglar view

same idea but with key tool
key tool might indicate presence of more keys on key ring

### passwords

password data unlocks passwords. as is, this doesn't make much sense:
    if password data can unlock a node, then you have spent an unlock to get an unlock.
    if password can unlock 1 or mode nodes, maybe, then it is different?

    if you can get password data from somewhere else (papers, wastebasket, datacube) then it is good!
    maybe you downloaded a file without knowing what it is- you get a free try on some other machine.
    maybe a password can be used on multiple nodes, like the physical keys.

### rfid

stolen via rfid sniffer, replayed via cloner?

### keycard





# keys v2

interacting with a door just trys to open/close
    if unlocked, behave as normal
    if locked, do not open
unify the iconography with hack interface "password" button
unify this system with burglar tools interface?
    is there any reason to allow keys and keycards in this interface?
        yes: not all locks are obvious from world

* don't show key popup when key unlocked
* we should unify keydata at the gamedata level.
* hide key menu when appropriate
* have more than one key
* effect for try / success / fail
    * door locked: show lock popup
    * key fail: blink key and red x
    * keys success: blink key and green ✓
* indicate failed keys in menu
    * red x, button not usable
? how to handle when no keys?
* sound effects for using key
. wave arm


randomize door keys


fix up key info pane
    dividers between sections
    smaller icon
    better description
    better image?