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

* input: 
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




ID check:
    enable / disable ID card check based on inventory
    how does ID check work?
        ID class: stolen, fake
    does ID cancel out some negatives, i.e.?
    perhaps, apply information, ID card, and see target threshold decrease in real time?
implement bluff
    iconography for information: person, place, event
    bluff information exploit:
        description: "shareholder meeting happening tomorrow"
            bluff content: "I'm working on slides for the shareholder meeting"
            type: event (person, location)
data / config:
    probably, skill check input / results should be something like their own data structure, nodes, defined in configuration
    data-driven difficulty threshold
    failure leads to a secondary check
    automatic retry x2, x3

prevent kiting




ninja vanish!



-------------

for each strike against the player, we must pass a check.
you use up LIE, BLUFF, etc.

e.g.
    Who are you? Identify yourself
    * use ID card: automatic pass
    What's with the rocket launcher?

so it sounds like, for each kind of suspicion record:
    1. guard challenge
    2. player options
        LIE, CHALLENGE, REDIRECT


[CHALLENGE] what does it look like I'm doing?
[REDIRECT] Hi, I'm looking for my dog, have you seen him?
[REDIRECT] I'm with NE&T, could you point me to your main trunk router?
[LIE] I am P.J. Pennypacker, security inspector.
[BLUFF] Rockwell isn't going to be very happy if you delay our meeting!
[ITEM] Sure, check my ID card.
[ESCAPE] Excuse me, I think I left my identification in my car.
[ESCAPE] Ninja vanish!
[DENIAL] I don't have to tell you anything!

how does civilian NPC dialogue work?
    who are you? etc.
    faily: NPC tries to activate alarm
        easy: NPC runs away
        medium: NPC runs for alarm
        hard: NPC radios HQ

challenge and options must be associated with suspicion records.
    1. that's a lot of extra stuff to define in the parts of the code where i just care about adding a suspicion record
    2. you could move it to static methods, but now we're doing code as configuration
    3. we could move suspicion records to scriptable objects, but now they're hard to reference from code
    4. define suspicion records in scriptable objects, then have static methods using an enum to interface
    5. this makes them all very static- we probably want to inject specific information (door, location, type of object, etc.)
        this points back to static methods
if we don't associate challenge and options with suspicion record?
    we would need a function f(suspicion) -> challenge, doesnt make sense
just go with static factory methods for now, we can turn it into configuration later if need be.



when initializing dialogue menu, initialize all of the challenges
e.g.
    "what are you doing down there on the floor?"
    "why are you tampering with the air duct?"
do one challenge - response per suspicion record


is there an order to the challenges?
    identity comes first. it can be quickly neutralized with an ID badge


now that we must answer multiple challenges, how does it work?
multiple challenges are harder to pass than one challenge.
    in other words, this isn't multiple chances to pass, it's multiple chances to fail
do they all contribute to a single meter that fills over time?
    1. a meter that fills from 0 suspicion to 100. each challenge moves the bar a little. if it reaches 100%, you automatically fail.
        otherwise, you have to roll 0-100 and beat the bar.
        this plays well with running away: if you think you're unlikely to pass the next check, then bail out with a stun bonus
        this means that:
            the more challenges, the less likely you are to succeed in the end
    2. same idea but you have to beat the set threshold.
        if this is the case, rolls either can sometimes subtract, 
        or else once you hit the limit you're boned?
what makes it interesting? what makes a choice meaningful?
    choosing the right tactic for the right challenge: what is the challenge?
        like playing cards from a hand, in order
    does the player use up tactics?
        then there would have to be strategy to saving your better tactics for later
        there'd have to be some notion of better / worse tactics?
        there'd have to be some way of indicating total and used up tactics     
            this is hard: the tactic stays the same, but the text changes
    enumerate the elements:
        challenges
        tactics
    are tactics controlled by player speech skill?
        number of tactics:
            use them up and you have no choice but to run or deny
        effectiveness of tactics:
           then why not just use the best one only? 


how do we visually indicate the impact of different parameters on the thresholding?
    etiquette
    speech skill
    alertness
    alarm
    disguise
    restrictedness
    current total suspicion appearance
easiest way is to put numerical +/- percentages right next to the modifiers.
the modifiers can determine the initial %

if this gets too elaborate, it is annoying to have to do it over and over

beginning maternity dept.
800-272-3531


## proposal:

1. initialize the dialogue controller with suspicion records
    suspicion records come with challenges
    always starts with "identify yourself" challenge
2. initialize suspicion bar
    every modifier flies over to the bar and pushes the initial limit...?
3. player is presented with each challenge in order
    player starts with a number of tactics to use
    tactics get used up
    more speech skill = better tactics, more tactics
    speech skill 1: only one lie
    speech skill 2: lie, bluff
    bonus tactic from explotable information
    always get the [ESCAPE] and [DENY] tactics
4. player chooses a tactic to answer the challenge
    the effect is rolled
        highlight: [BAD!] [GOOD!]
    the effect is applied
        the suspicion meter number changes
5. when all challenges are exhausted, roll 
    if the roll is above the threshold, fail

nice features:
    strategy is involved in deciding to keep pressing on, or to cut losses and run
        as threshold increases, you risk hitting the 100% suspicion limit and losing automatically
        as threshold increases, odds of getting out successfulyl decrease
        in either case, you might have a better chance taking the stun + run
difficult features:
    * length
    * comes down to a random roll, you can still fail even with good initial results
    * every challenge needs to support every tactic (?)



# first modification
* one roll per lie. each lie increases the credibility threshold.
* start with identity challenge
* increase threshold as we go.
* give item option only when user has item
    * fake ID always works
* fail early


# second modification
show threshold constantly
    "suspicion"
    unify with skill check.
differentiate tactics
take into account skill & state (how?)
