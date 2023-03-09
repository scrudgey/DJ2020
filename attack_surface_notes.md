# attack surfaces
locks
card readers
electrical systems
sensors

## tools
definite:
* power drill through lock
* keyed-alike
* wire cutter
* magnetic probe (detects trip sensors)
* sensor defeaters
* lock decoder

* shim
* bump keys
* fence cutter
* glass cutter
    to prevent bullet holes: fairly simple
    to allow player to pass through: hard
* under door attack?

## configuration

there will be one door prefab per type (single, double, sliding, electronic)

that door prefab will contain all possibilities.

possibilities are driven at runtime by configuration.

configuration contains a security rating:
    low security - medium security - high security

security setting plus a random seed will determine the configuration of the door attack surface
    1. key code : from configuration, not random
    2. latch present / not, vulnerable / not: based on security
        latch guard
    3. lock picking difficulty: easy , medium, hard, impossible: based on security
    deadbolt



##

maybe there is a difference in suspicion between different kinds of tools?
    certainly lockpicks are less suspicious than power drill

if everything is vulnerable to lock pick then why bother with other things?
    because not every door is vulnerable to every type of thing.
    easy doors vulnerable to simple attacks
    lockpicks for simple locks, but become impossible on hardened locks
    extremely high security doors will require elaborate processes to bypass, or may be too difficult?
if that is the case, how does this translate to a non-frustrating gameplay experience?
    knowledge skill explains in advance some things
    freeform experimentation is still 

there is some notion in which doors / things can have different security levels
    security level seems important to telegraph to the player what to expect on a mission
    low: easily bypassed
        locks are pickable
        latches are bypassable
        no sensors
    high security / hardened: difficult bypass
        lockpicks might not work
        probe doesn't work
        sensors not easily disabled
if there are a plethora of tools / attacks and security levels, it makes sense that the character can have a skill to 
    recognize and assess different types of locks / systems.
    Q: where is the hook / thrill / game in it?
    A: a door is not vulnerable to the few easy attacks you try: they're clever. but you're more clever.

* hide tool from toolbar when in use
* click down effect?
* tool click sfx
* tool over element clicky sound
* click on empty to put tool away
* draw latch guard
* deadbolt
* redraw baseplate
* randomizing door attack surface:
    * latch vulnerable / not
    * latch visible / not
    * latch guard in place
    * lockpick difficulty
    * deadbolt present / not present 
    * deadbolt difficulty
    * auto close / not
* latch guard enable / disable
* latch guard screws / noscrews
* deadbolt
* disable attack surface UI element when latch cover is removed / screw is removed

configuration of keyid and vulnerabilities = a serial number?
show serial numbers for different elements
show vulnerabilities of different elements
    callout icons?

wirecutter
magnetic probe
magnet / trip sensor disable
REX sensor
skeleton key
trip sensor
electronic access:
    door open without badge -> alarm
    REX sensor
show tension wrench in keyway
handle manipulating unlocked lock
feedback when element is not vulnerable

lockpick difficulty:
    * random setbacks
    * reset stages when lock loses focus / lockpick is changed
    number of stages, setbacks are controlled by lock difficulty
