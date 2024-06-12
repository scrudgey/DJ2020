# burglar v2

## tools

traveler hook: should have a cutaway view like pick / require some interaction

key duplicator
key card creator
    advantage: create a legit-seeming key card so you don't have to hack the reader
    somehow, you have to determine the correct key though. 
    get the correct key code: decoder pick
        now, you can bypass this door and others without raising suspicion!

duct tape
    apply to door after opening- prevents latch
rfid sniffer
crowbar uses:
    open doors
    open elevator door
    open hatch
    open cars
    break windows?
    melee weapon?
power drill
    can destroy a lock, but is noisy
safe cracking equipment
    destructive / noisy vs. silent

shim
under-door tool
dot magnets
uv powder

## targets

combination lock
multiple varieties of padlock 
multiple varieties of tumbler locks
wafer lock
alarm sensors
remove hinges from door?
numpad door
rotary safe lock
    drilling
chainlink fence
camera


## attacks

combination lock
1. use the probe to decode the combination
2. ? shim
3. ? bolt cutter- destructive & leaves evidence

multiple varieties of padlock 
1. shim
2. pick
3. ? keyed-alike?

multiple varieties of tumbler locks
1. pick / picking variety / pick skill
2. decode the key and create a copy
3. pick that decodes at the same time
4. pick gun
5. ? keyed-alike?

wafer lock
1. lockick
2. ? keyed-alike?

alarm sensors that detect when door is opened
1. place a magnet on the sensor to prevent from tripping
2. disable alarm controller

? door hinges?
1. remove door from hinges?

keycard & keypad door lock
0. decode the key id:
    read key id from access controller
    read key id from electronics inside the keypad
    ? install a keycard data stealer and wait for someone to use the door
    scan someone with an rfid scanner / stealer
    place UV-reactive powder, wait for someone to use the door, and from that determine the 4 digits involved
1. decode the keycard id, then flash a keycard using a card maker tool or enter the code
2. decode the keypad id then enter it on the keypad
3. decode the keycard id, then use tool on electronics inside to enter the keycard id to unlock without a key
4. hack the access controller 
    learn the correct current keycard id and apply it as above
    enter a new keycard id of either a current card or one you can make
5. disable the power to the door disables the mag lock
6. use keycard scrambler


physical door lock
1. pick the lock- allows entrance
    there can be some variety in lock types and lockpicking based on how lockpicking mechanic works
2. bypass the latch - opens the door but remains locked
3. tape the latch - prevents latch from operating, but leaves evidence
4. decode the key - special tool or pick - allows to create a key and therefore not raise suspicion
    pak-a-punch
5. drill the lock out - permanently disables the lock but makes noise & leaves evidence

rotary safe lock
0. autodialer
1. x-ray
2. drill / physical attacks
3. thermite

chainlink fence
1. cut a hole with bolt cutters

camera
1. disable the motor while it is faced away
2. cyber hack it and disable
3. open a panel and disable it
4. shoot it
5. emp attack
6. spray paint

ATM


## electronics


every external cyber, power, alarm connection is represented on the edge of the board
    they are wires, connected to little screw terminals
    each wire can be cut with wire cutters
    one wire per graph connection.
connections are brought inward to circuit components in the center of the board
connections can be cut or manipulated
    example 1 (good): security camera 
        connections: power, cyber, and alarm
        components on board: 
            alarm source
            cyber component connects to the alarm source
            power component?
        cutting the cyber connection just prevents this from being hacked remotely- disconnects it from the network
        cutting the power connection powers down the whole camera. success- but somewhat destructive. central can notice it and send a tech
        cutting the alarm connection prevents alarm source from communicating with alarm central. but it can still alert nearby guards.
        attack alarm source: if there is a more detailed way to attack this, then this is the best attack. leave everything else intact, prevent it from alarming.
    example 2 (good): numeric keypad door lock / keycard lock
        connections: cyber connection, ?door?
        components on board: 
            RAM chip stores the access code and can be decoded with oscilloscope, connected to cyber
            keypad chip sends the keypad input to the access controller and can be spoofed with oscilloscope?
        potential attacks:
            decode the keycode from the ram chip
            reprogram the keycode via cyber attack
    example 3: power transformer
        connections: power connections out, mains connection in
        this might not fall under electronics hacking. it just requires heavy-duty insulated shears 
    example 4: laser grid
        connections: power, alarm
        components on board:
            alarm source
            power source
            laser controller
        cutting the power connection powers down the laser grid. success- but somewhat destructive. central can notice it and send a tech
        cutting the alarm connection prevents alarm source from communicating with alarm central.
        attack the laser controller and set it to blink or disable the lower laser!
        attack alarm source: if there is a more detailed way to attack this, then this is the best attack. leave everything else intact, prevent it from alarming.
    example 5: turret gun
        connections: power, cyber. ?alarm?
    example 6: alarm terminal
        connections: alarm 
        components on board: unclear
        cut the connections to prevent central alarm from activating
            is there any other meaningful attack on the alarm terminal? probably not. accessing it & disabling it is the main prize.
    example 7: ATM
        goal is to put it into servicing mode and open the cash drawer

still to figure out:
    attacking the alarm source on the board is important. it offers the best outcome (prevents alarms nondestructively) and should be gated.

upgradeable kit:
    1. lockpick -> decoding pick -> lockpick gun
    2. screwdriver -> powered screwdriver

specialized tools:
    1. key cutter                   - decoded physical locks                    - item slot
    2. reprogrammable keycard       - decoded keycard locks                     - item slot
    3. keycard scramble attacker    - keycard / keypad locks                    - burglar tool
    4. insulated bolt cutters       - transformer, chainlink fence              - burglar tool (attack transformer) / item slot (attack fences)
    5. drill                        - physical locks, safe, maybe others        - burglar tool
    6. rotary safe autodialer       - safe locks                                - burglar tool / could be item slot?
    7. shim?                        - padlocks                                  - burglar tool
    8. keyed-alike keys             - tumblers                                  - burglar tool
    9. duct tape? could be included in normal gear? - latches                   - burglar tool
    10. rfid scanner                - one route to decode keycard lock          - item slot
    11. UV-reactive powder and blacklight   - one route to decode keypad lock   - burglar tool / could be item slot?


## implementation
if we need different aspect ratios, we can always adjust the viewport rect dimensions relative to the game resolution
change how text / highlights work in camera image
1250 x 750
625 x 375
312.5 x 187.5
156.25 x 93.75









raycast can extend far?
clicking unzips
broken connection display in overlay
multitool stays present when leaving burglar view
remove attack surfaces from cyber objects
allow keycard reader to read card in burglar view
would be nice to zoom in to chip
randomize the key
error in permutations at index = 9?



draw
    jazz up multitool display
    draw more circuit board layouts

add circuit to more elements
    elevator keycard reader
    alarm panel
    create keypad lock

circuit components
    door controller- play keycode to open door
    alarm source
    an LED turns off when powered down

play sound on reveal






don't click to remove panel AND jump into circuit
change burglar tool captions etc.
draw
    draw ram chips
    draw alligator clip
    draw waveforms
    icons for codes in escape menu

* disable multitool buttons until in ram chip mode


































### brainstorming section

1. a chip on the board can contain a code that can be read out (door codes, keycard codes)
    differential power analysis: connect the oscilloscope and see the pulse code.
        then push buttons until you match the pulse code to get the numeric code. (i like this)
2. a separate chip is used for transmitting codes
    connect the tool and push buttons to send a code
3. a chip on the camera can be attacked for looping - less chance of discovery?
4. chips might connect the device up to alarm network- disable them to disable the alarming.
5. honeypot chips trigger negative effects?


what if it involves moving wires around to isolate elements, but the wires must be jumped to other locations
you do this to free up slots to attach your multitool
but in moving a wire you risk attaching a signal to something that might trigger an alarm or an otherwise bad effect?
what if there is no real gamey aspect of it, it just takes time
    there is oscilloscope decoding, is there anything else similar? signals processing?
attaching a looper chip requires placing the chip, attaching it to the signal processing unit, and attaching power
order of operations: cut any connection to alarm before messing with a thing

maybe we don't want electronics hacking to be too complicated.
    in lockpicking, simply discovering the order of pins is sufficient. it's not really a minigame but a mini-simulation
    we can see how oscilloscope works to decode a sequence
    these both work because they aren't minigames per se. they are simplified versions of how you'd use these tools in real life to accomplish meaningful goals.
    in minigames, there is only surface level resemblance to real life, and the goal is not meaningful.
    lock: unlock it
    keypad / card reader: get the code
    camera: loop it (connect buffer to input source?)
        disable it (disconnect input source)
        disable alarm (disconnect alarm circuit)
    alarm hub: disable it (snip wires)
    transformer: disable it (cut the power line)
    tamper switches

    realistically, to disable the camera, just disable the alarm connection. why bother looping?
        disable alarm connection: can still alarm on the camera, just can't activate central alarm
    realistically, to disable the alarm system, just cut all the wires
    there is nothing much of interest unless we add:
        some wires (?) connect up (?) tamper alarms, you must disable these before you tamper with the target
        so use an inspection lens to determine what is what, then cut wires, done
        this is almost all we need. the only problem is in the details.
            cutting a wire drops power- unintuitive for that to *cause* a negative effect
            maybe we are connecting wires? jumping wires?
                it doesn't make sense to jump a wire to disable the alarming system
    there is a circuit board and we are tampering with it in some way.
        cutting wires, adding wires
        swapping chips
        destroying / removing chips or something instead of wires
        uart port?
    i like the idea of standardizing connector ports
    what if you have a little info card on the right like "look for this port GND IN OUT VCC" and you have to find it
        or a little recipe card
            1. identify RC123
            2. identify JR4 interface
            3. connect RC123 to JR4
            4. disconnect VCC  
        this is interesting, but how does the honeypot work? just if you screw it up?
            we envisioned honeypots as something to avoid
    use multimeter to identify & tag components
 
        

    components on the board are connected to tamper sensors
        tamper sensors can be temporarily disabled, n exclusively at a time
            time element is good- adds some urgency / planning / skill
            but you still want to prevent the player from freezing every tamper sensor all the time. so only n at a time- exclusive

    some components are valuable and can be removed

good threads:
1. oscilloscope-style things
    decode numeric waveforms
2. inspection requires manually moving a view that lets you trace connections
    ICs reroute power
3. moving wire connections around
4. injecting fuzz
5. defeating guard circuits before you can attack a normal chip
6. freezing chips with liquid nitrogen?

goals:
1. capture a key
2. play a key
anything else?
3. loop a camera- special logic?

what is the challenge?
1. determine the nature and location of the chips
2. determine wiring patterns
3. figure out ways to reroute wires without tripping honeypots

targets:
1. keypad door
    no connection to access controller, code stored on device
2. ketcard reader 
    connection to access controller
3. camera
4. transformer
5. alarm system

suppose we get a good answer to electronic hacking. is that enough to go forward?
1. attacks are multi-faceted with multiple systems. some can be quick & easy, allowing you to continue on with gameplay.
2. lockpicking still unsure
    in lockpicking, simply discovering the order of pins is sufficient. it's not really a minigame but a mini-simulation
    you can have pins that reset everything when hit out of order, or just allow order to be discovered

3. gadgets- how?
    i like the idea that the player has a reprogramable key and keycard that integrates with the multitool.

* attack surface has a circuit
    * circuit has a camera
    * camera has a render texture
* when click on element (circuit board, etc) 
    * result: change render texture and current camera
    * use current camera for raycasting
    * a button on burglar view is activated, and on click it returns us to the main camera
    * raycast to skyboxnoshadow too
* draw circuit, wires
* snippable wires
* wire configuration controlled by graph edges
* snipping wires disables edges in graph
* refactor tool selectors:
    * bag starts out at top
    * cyberdeck starts hidden
    * when electronic panel selected, cyberdeck rises up too
* mode: tool is out, usb not selected: show prompt
* mode: usb selected: show "scanning..."
* mode: usb selected, mouse over: show info on connected node
* mouse exit not doing anything
* start from "none"
* when returning usb to anchor, reset to none
* allow click to attach
* announce decoded in some way
* easings / coroutines
    * ease in indicators
    * flash buttons on reveal
    * flash waveforms on correct
    * flash total waveform on match
* RAM chip- keycard and keypad doors
* implement power analysis mode
    * detect when entered code is correct  
        * store key entry
        * disable buttons
    * mouse over ram chip
* randomize waveforms per key
* mode: usb connected to ram chip: show decode mode
* randomized circuit layouts
* add circuit board randomization before we add to everything
    * laser component
        * bridge from laser unit to control unit
    * alarm central
    * router
    * transformer
    * keycard reader
        * move doorlock to keycard reader
        * create ram chip


