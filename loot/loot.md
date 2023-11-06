# loot

loot has:
    * gameobject
    * portrait
    * value
    * description
    * name
    * categories
    optional modifiers
        extra value?


* wire up loot collection
* more grabbable loot
* show loot in popup
* show counts
* show total value
* show data too?
better reward sound
* crouch to pick up low items, wave arm to pick up high items

i think i'm gonna need more categories.
    nuclear
    military
    robotics
    hardware
    rare


## NPCs drop loot

NPCs have a loot collection

loot collection is a collection of loot sets with probability:
    1x valuable with prob 0.5
    2x low value with prob 0.7

## example loot 

credstick
gold bar
cryptocurrency stick

high value drug:
    * zyme
    * nuke
    * synthmesc
    drencrom
    tiger tea
    smart drugs
    betaphenethylamine
    consult shulgun
low value drug:
    caffeine powder
    alcohol - liquor
    40 oz

street:
    take out menu
    switchblade? a weapon is confusing
    jewelery
        ring
        chain
        watch
    easily removable cyberware
    sunglasses
    sneakers
    cyberdeck
    gift card
    spray paint
    marker
    burner phone
    cash
    lighter
    

commercial:
    data cube
    software disk
    telecom equipment
    radiation meter
    hardware scrambler
    widget
    AR glasses

medical
    syringes
    lab glassware
    lab coat
    goggles
    laboratory tools

high value industrial:
    tools
    thermal paste
    epoxy
    industrial lube
    raw materials
    radioactive fuel rod
    uranium ore
    plutonium
    roentgen meter
    red mercury
    radioscope
    radioactive waste / slag
    cesium ampoule
    antimatter vessel
    endohedral fullerenes
    nano particulate raw materials
    Utility foglet ampoule
    claytronic atoms
    nitinol wire
    servos
    High Voltage DC Power Supply, 25,000 volts
    Thermoelectric Coupler
    vacuum pump
    Uranium Acetate
    vacuum dessicator

asteroid minerals
pure space-grown crystal

low value industrial:
    solar cell
    2-Propanol-99%
    solvent
    Aluminum Powder
    Borated Paraffin
    Cadmium Metal, Rod
    carbon rod
    Deuterium Oxide
    Magnesium Metal Ribbon
    magnets
    batteries
    strontium carbonate

## locations 

* loot can be found on ground
loot can be found on shelves
loot can be found by searching drawers
loot is dropped when NPC dies

randomized loot placement

## iconography

breaks down into categories
    loot can have one or more categories
different shops will buy different loot at different rates
    drugs +
    hardware -
    industrial ++




# random loot drops

1. should allow multiple "drop elements" per loot dropper.
2. drop element has a probability of dropping or not
3. when drop element drops, it chooses an element from its list.
    elements can have weight
4. drop element can specify n drops.

4. people can drop keys
5. it is possible to set the level up so that one NPC will carry a key.
    do not hard code key drops to a specific spawn point.
6. drop elements should be scriptable objects.
7. probability of dropping drop element should belong to loot dropper, not drop element.

the only really tricky part is #5.
to do #5:
    this must exist outside of spawnpoint logic.
    a component (level initializer) has a list of spawn points and a list of required loot elements.
        it places the loot element on randomly selected spawn point
        when spawn point spawns, it places the loot element on the NPC lootdropper with probability 1.

problem: what happens when some spawn points don't spwan (i.e. randomization?)
    it seems like we need a two stage process:
    1. determine which spawn points are going to spawn
    2. determine which spawn points get the random item
    3. apply the random item
this logic can belong to level initializer pretty readily.
level initializer logic can be moved to level template.? perhaps not: level-specific references.


connect up lootdropper status with worker spawn points / npc spawn points