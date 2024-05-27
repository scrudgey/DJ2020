# tools and equipment management

the basic problems:

1. if player uses plain credits to acquire equipment then they can get softlocked if they run out of money and don't have required equipment.
2. hard to know what equipment you need for a mission before you start it
3. if all equipment is available to purchase from one shop at the start, the player doesn't know which equipment to buy, which to bring, and what to use it on.
4. if the player has a stash of a lot of equipment, how do they know what to bring / what limits them from bringing everything?

## one solution

when the player encounters a new obstacle, they gain intel.
they can call someone and ask about this obstacle (how do i defeat a cylinder lock? how do i crack a safe? how do i get around the security door?)

the response they get can explain all the possible solutions
(disable the power, pick the lock, drill the lock, hit it with a crowbar, some cheap safes have a little latch you can spring with a shim)

for more complex stuff, or specialty equipment, they will point you to someone who knows- a new shop.

this shop has the equipment you need- a diamond tipped drill bit.
it also has a few other bits of equipment.

you can purchase it for a fixed price (100,000 credits) or a percentage cut of the mission you bring it on.
same with the other equipment.

## what it solves

0. player is never softlocked: only in debt
1. no tutorials: equipment and its usage is explained at the moment the player encounters the problem.
2. multiple solutions per obstacle
3. not all equipment is unlocked all at once: only a subset, most of which is relevant or interrelated.

## what it does not solve

problem 2
problem 4 (these are related)

if the player has to select equipment before the mission,
and they can't bring everything,
then they will encounter something during the mission that they are missing the tool for.

bringing equipment:
    you can bring more if you bring a backpack, duffel bag- makes you a little more suspicious (a neutral suspicion record)

## a possible bridge

there are burglar tools, and inventory items. 
burglar tools:
    acquired through non-monetary means
        the number of tools is limited and unlocking is perhaps arduous in some way but doesn't require money?
        at worst, a special mission that focuses on using the tool. 
    explained on-site via phone call:
        what the tool is used to attack
        how to use the tool to attack
        unlocks the tool, possibly
    you always bring every burglar tool

inventory equipment:
    purchased with money at a shop
    tools equiped during gameplay to make your life easier
    sub-weapons
    can apply backpack or duffel bag for larger equipment?

special purpose gear:
    diamond tipped drill bit
    nuclear material container





# idea

1. person teaches you how to use tools to attack specific security, then sends you on a mission featuring the tool
to unlock the tool
2. items have a similar thing, a test arena you can try
3. when encountering security feature, it is logged in some way. you can bring this log to the person and theyll teach
you.
4. missions that require a specific skill (safe cracking or whatever) can come with intel that unlocks the log.
5. introductory dialogue can introduce the tutorial mission
6. limit the tools to a bunch of general things that apply in many situations (lockpick, screwdriver, etc) and maybe the only unlock is upgrading them
7. maybe the system is more automated than that. you pass a mission (early mission, no impossible obstacles) then you get an email from your friend saying "hey i've come up with a method to open up that ATM you saw on that last mission", this prompts the player to go learn
8. the intel you collect on missions (basically, when you encounter a new thing like a camera or ATM) you can inquire about on the underground chat room ("anyone know how to deal with this ATM I saw?") then you'll get a half dozen responses from different characters explaining how they do it ("i blow it up with a little c4", "I found you can hotwire these things if you can open the rear panel", etc.)
9. bringing upgraded equipment to a mission might bring the same feeling as intricate plan
10. de-emphasize planning in advance. all tools are brought at all times. different ways of attacking a surface



what about things that have multiple attacks?
the basic idea behind a simulation driven game is that there are multiple solutions-
modular pieces that combine and interact
but security is the opposite: learning specific exploits, single-purpose tools

burglar tool view is one vector of attack. for a given obstacle:
    explode it
    disable the power
        on the object
        remote from the object
    disable the alarm system
        on the object
        remote from the object
    burglar attack
        destructive, non-destructive
        leaves evidence vs. undetectable
    
    1. power drill to lock
    2. pick the lock
    3. use a key (stolen- )
        keyed-alike



we need to emphasize modular composing components
e.g. two panels under screws that intercommunicate through wires with some alarm sensors
tools can be basic and self-explanatory if not multi-purpose


the guy at the bar says he has a mission for you
    "oh but do you know how to crack a safe?"


crack a safe -> safecracking tools
basic lockpicking -> 3 different types of lock, (padlock, tubular lock, regular lock)
disable a security camera -> different methods
cut a fence
bypass a keycard reader
spoof rfid
exploit a RTE sensor



* fix level load


# todo

fix up existing system
    redraw tools to have more shadow
    * snippable wires are broken in some cases
    remove cyber
    change lockpicking
add to existing system
    attack surface for card reader
determine loadout system
    tools that are added to burglar toolkit
    tools that are loaded prior to mission - item slot?
    duffel bag and backpack
add new tools
    keycard spoofer
    power drill
add tutorial mission guy in the bar
    interface
    some missions
determine how electronics hacking will work

tower map is messed up




## equipment selection considerations

regardless of how tools are introduced and taught, we have the problem of how tools are brought on a mission

several possible modes:

1. tools are automatically added to the burglar toolkit
    ensures that critical equipment (lockpick, screwdriver) cannot be skipped.
    small tools that fit in the kit
        lockpick, screwdriver, probe

2. tools are selected before mission with limited space
    allows interesting tradeoffs
    less critical tools that offer interesting approaches
        duct tape
        power drill

3. tools are actually items 
    item-like equipment is brought in item slots
        fence cutter
        glass cutter
        safe cracking equipment



sounds like:
    there are basic tools that are always brought.
    (the tools might be upgraded and *maybe* expanded upon but otherwise the basic capabilities are always there)

    there are extra tools that have to be selected
    you start with one slot. if you need extra slots you bring a backpack or duffel bag
    it is possible that duffel bag slots act as extra item slots too.


## how are tools used?

the other problem is, how does the player use the tool?
i.e. fence cutter is an item to be used in normal mode.
    tubular lockpick is an item to be used in burglar mode.
    power drill does not fit in burglar toolkit


## backpack and duffel bag
extra space for tools is provided by backpack or duffel bag
for this to be a possibility, there has to be selection (2, 3) and there has to be limited space (2, 3)

what is the upside / downside to having a duffel bag?
    1. you cannot use two-handed weapons while carrying duffel bag.
        you automatically drop it when pulling out a weapon
    2. it is extra suspicion
so you can bring critical tools in the mission 
    this really only ever becomes an issue if there's more than one "critical tool" that you feel like bringing- it would have to be pretty important

how do you use the tools in a duffel bag?





(tool) key duplicator
(tool) key card creator
    advantage: create a legit-seeming key card so you don't have to hack the reader

    somehow, you have to determine the correct key though. 
    get the correct key code

(tool) rfid sniffer
(tool) crowbar uses:
    open doors
    open elevator door
    open hatch
    open cars
    break windows
    melee weapon
(tool) power drill
    can destroy a lock, but is noisy
(tool) safe cracking equipment
    destructive / noisy vs. silent