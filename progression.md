# progression mechanics

the basic idea is that progression will be handled in the after-action report sequence.

after-action report will change from a static readout to more of a cutscene, with text overlaid.

## cutscene

the player character riding in a subway car, through a cityscape, over water, etc.

question: how to handle aspect ratio and all that?

answer: develop the custscene in 16:9 

1920x1080
960x540
480x270

## Report

fade ins can be preempted by player input.

fade in in order:

1. mission name
2. objectives:
    * objective A: COMPLETE
    * objective B: COMPLETE
    * optional objectives:
        * C : INCOMPLETE
        * D : COMPLETE

pause, wait for player to continue

3. reward:

    credits

    new balance

    optional objective rewards
        (favors, credits, anything else)

4. response text

    "good work, etc."

ease in all segments.
wait for player to click CONTINUE

## progression

### purposes

ensure the player doesn't:
    1. spend all their money on advanced upgrades to start (limit their spending in early game)
    2. spend all their skills on advanced skill to start (limit their early development to useful skills)

#1: there are other ways to solve 1, and we might just relax this constraint entirely.
DJ1 allowed yu to buy 10 different dermal upgrades- how about only two?
if we allow that cyber upgrades, rifles, etc. are a whole order of magnitude greater in cost, it naturally limits things
while keeping things flat.
also re-introduces the purpose of the doctor.
maybe perks unlock more cyber slots
maybe bonuses enlarge the cyber store
    a mission on behalf of the doctor?
 
credit balance is a whole other issue:
rifles can be an order of magnitude out of reach of early game, but shitty rifles can be cheaper
maybe there is a shitty rifle you can reasonably upgrade into something good- good design pattern

regarding money spend:
main problem is having every option available at the start, everything controlled by money, so player could
spend all their money on something extreme to begin with.

how much of a problem is it?

Q: are there stats and perks? is it too hard to keep to perks only?

Q: pure tree structure or bag structure?

Q: are different gun types different skill types? different tiers within gun skill?

accuracy is a multi-part perk. add pips to it: +25% per pip.

### design points
1. progression should be controlled by a resource / token and can be done later so that if player
    quits or crashes before progressing it is not lost.
    basically, progression is assured once player completes mission.
    this means there should be a way to trigger progression from inside apartment at least-
    and progression interface should be modular.

2. there should be a way to see all possible future progress, like a tech tree or perk menu
    this allows player to see possible future skills and plan / anticipate.

3. progress is broken into various skills
    different gun types

4. higher level progress in a skill is gated behind lower level progress
    to keep higher level skills for later.
    (this naturally incentivizes specialization)

5. some skill trees in general are gated behind an overall degree of progress 
    to prevent the player from attaining too early

6. shops progressively unlock more and more material
    to direct player expenditure of credits at early stages
    cyberware not available to purchase immediately

### possibility 1
requirement: there is a sense of how leveled we are per-skill, and how leveled overall (two axes)
skill category, category level, overall level
the only challenge is in representing overall system to the player

### possibility 2
entirely graph based.
to fulfill #5, just move higher order unlocks to some higher point of the tree.
this leans on a deep tree
problem with deep tree is that the deeper nodes are less likely to be reached

### possibility 3
micro-trees inside player level tier bags

### possibility 4: solution

divide into multiple skill panes.

each pane has skill progression on x axis
player progression on y axis

skills divided into bags gated by player and skill progression

some skills are multi-pip

is there a point to skill-gated stuff? or is it just tree progression?
    gated: cyberware (level)


cyberware starts with one slot and is expensive.
unlocking more slots is gated behind player progression

there can be basic pistol, smg in low player progression tier


## resource balance
credits:
    * items
    * weapons
    * weapon upgrades
    * software / hardware
    * cyber upgrades
    * tactics

skill points:
    * weapon skills: pistol, smg, rifle, shotgun, sword
    * speech skills
    * hacking? in some way?
        to what extent is hacking controlled by skills and by software / hardware?
    * burglary skill
    * health
    * cyberware slots

### possible perks

per gun type:
    faster reload
    
    greater accuracy- +25% +50%
    greater stability
        combine these two: pistols I, II, III, etc.: now we are back to skill points

    lock on to head
    bonus damage
gun knowledge: know what upgrades can work for each gun
disable engine
	with a high enough caliber weapon, fire two shots into an engine block to disable it
cyber sword
    hack lower-level things by stabbing with sword
door breach
reload two shells at a time for manual loading shotguns

better bargaining
    one more deal per day
    one more market condition per day
    generally better market conditions
dialogue skills
hacking
burglary
    better lockpicking
    visually identify weaknesses / identify types
    hotwire

minimap / radar, show enemies -> enemies use jammer

## interface 

possibility 4

manually arrange perk buttons that take a perk scriptable object.
on activation, communicate with state handler to remove a skill point and put the perk id into list.

## after  action flavor bonus

player chooses a location to visit and a random reward / choice of random rewards is offered.

unlock new contacts:
    * new fences
    * new tactics
    * new shop availability

buffs:
    * extra health
    * favor


# data structure

perks are defined in a scriptable object template
    * skill category: gun, hack, dialogue, body
    * skill level requirement
    * player level requirement
    * icon
    * some sort of pip spec
    * name
    * description

player state has a list of active perks
player state has counter: skill points
player state can provide a clean api to answer questions: getCurrentPistolAccuracyBonus() etc.


allow multi-stage perks
add more perks
indicate levels in perk descriptor
smarter "requirements" text
sfx


faster reload
gun nut
bring two explosives