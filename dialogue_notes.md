investigate dialogue
    the player is prompted
        the prompt should reflect the state of the level and any recent suspicion records.
            "why are you carrying a weapon? Identify yourself!"
            "are you authorized to be in this area?"
            "Red alert! Identify yourself or !"
    the player has multiple options to respond
        the outcome should depend on player skills
            charisma roll?
            etiquette?
            cyber?
        the outcome should depend on level sensitivity
        the outcome should depend on guard stats
        the outcome should depend on active suspicion records
            strikes against
    options to respond:
        LIE: plainly pit skill against interrogator
        BLUFF: relies on intel gathered in advanced or on-site
        ITEM: use an ID card, real or fake
        ESCAPE: distract the guard to run away
    outcome:
        success: player is cleared by NPC. guard will not bother them for the next 5 minutes, unless a new suspicion record is entered
        fail: player is identified as enemy, an aggressive suspicion record is added
        bonus: ?

factors in play:
    player skill (+)
    level sensitivity (+/-)
    guard stats (-)
    active suspicion records (-)
should be displayed visually, and consequences should be visually clear.

what is the mechanic? a simple probability roll? that's kind of boring and invites avoidance?
    when a 32% roll fails, it feels like there was nothing i could do?
minigame: bad idea
better idea: being dealt a random assortment and picking what you think has the best chance of working
crazy: assessing the guard's gullibility, alertness, etc.

I like the idea of picking one option out of three, three times, to concoct a lie, and doing this strategically, and connecting it with the text generator
1. there's some randomness to it that can be juiced by skills
2. some strategy develops as early choices are locked-in, trying to choose the best possible 
if aspects of a lie are being chosen draft-style, they can have discrete attributes (+) that counter some of the guard (-), then
    success is driven by clear interactions, not by a probability draw.

When formulating a reply, we choose two randomized elements from randomized list.
with a certain perk, we get three.
we are essentially choosing a card from three possibilities.
this really only makes sense if there are multiple "types" of (+) and (-) effects. but I dont want to introduce such complexity just yet.

    i remember what this is reminding me of: civilization
    there, you are stacking bonuses. here, we are stacking one against the other.
    can we break suspicion into some categories?
        * charm
        * believable
        * 

more (!):
    sensitivity:
    * (-) public property
    * (0) semiprivate property
    * (!) private property
    * (!!) restricted access
    records:
    * (!!) gunshots heard recently
    * (!) suspicious appearance: you are holding a gun
    guard:
    * (!) alert
    * (-) lazy
    * (-) drugs
more (-):
    * (-) etiquette: corporate
    * (-) etiquette: gang
    * (--) speech skill 2
    * (-) disguise


speech skill: does this introduce a skill system?
what did we have before:
    gun skills
    cyber implants
    hacking software 
    speech skills (?)
if instead of gun / speech skills, what about a perk that unlocks after each successful mission
there will be skill trees for different types of weapon, speech, barter, hack perks (cyber sword) etc
    armors, healths, throwing skill

progression mechanics:
    skill tree
    cyber implants
    cyberdeck + hack software
    guns & equipment.

in DJ1:
    cyber implants
    cyberdeck
    hack software
    $ -> better weapons, equipment, upgrades.

so speech skill like gun skill will be discrete integers.
includes etiquette
how is it calculated in the end though?
    simplest: p = 0.5 * exp(net)
one problem with these approaches is that it puts all effects on the same level. but this is probably deisreable to reduce complexity ^ confusion
it might work to arrange this all and see how it works.

probability is definitely involved. if it is deterministic, too easy / too easy to game. you know in advance if you can bluff your way.
however, if bluffing is not reliable, you can't really plan on it or use it. only for emergency last-ditch effort to get out of trouble.
something like:
    pass your first check that's at least 60%.

i like the idea: 
    * display portraits on left and right
        level (!) under NPC portrait
        player appearance under player potrait
        player skill (+) under player portrait
    iconography for relevant effects
    something showing the net effect. there's a balance - / 0 / +
                                        which creates a probability 20% - 50% - 80%

if we are showing both portraits, we need a way to distinguish who is talking.
    |-> |  text  | <-|

maybe, dynamically add / remove points on both sides with successful dialogue checks?
handle initializing the dialogue box:

1. set portraits
2. set status effects
    sensitivity
    records
    guard status
    player skill
    player etiquette
3. set text options

input: 
    npc state
    player state
    level state


* use alarm status
* use disguise status
* summarize appearance
* use dice roll skill check

* blink-emphasize result
* fix success size
* pulse-color doubt
* hang on result a bit
* progres
* progres...
* upon opening:
    * lerp percentage up from 0 -> target
    * move arrow indicator accordingly
    * blink percentage
    * start skill check bar
* apply results
* repeated dialogues from NPC
* turn to face each other
* adjust speed / lerping
