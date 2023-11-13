# dialogue system

dialogue mechanic should:

1. invlove player making choices
2. choices should matter
3. mechanic should not be a simple dice roll

tactics are used up:
    pro: easy progression in increasing / unlocking tactics
    con: have to specify tactic in every dialogue challenge



can dialogue be used outside of challenge?
to extract information and locate objective?


# v2

1. consumable skill points influence the outcome, up to & including guaranteed success
2. balance of NPC negs and player positives influence outcome
3. show the threshold visually, and the effect of player choices visually, directly
    the threshold to pass should be set by player actions here.
4. fake id, data can provide guaranteed pass
    data is consumable resource, collected from datastore
5. we don't necessarily have to show the literal string response- leave it up to be generated just in time.
6. different response types could start the player at different thresholds; 50% vs 40% vs 60%
    and they can be shown as locked, player skill issue?

an amount of suspicion build up throughout the conversation. the guard has a threshold for when alert is triggered.
show in advance the different challenges you will face?
    pro: 
        event after the threshold is reached, the player can play their remaining cards to reduce the amount
        player can instead decide to cut and run when the conversation isn't going their way
        don't need to pass three separate checks; one cumulative check is easier to gauge
        show [easy] [suspicious] [warning!] [fail!]
    con:
        how to see the anticipated effect of the next roll?
            rolls are built in to the cards: randomness is on card draw
        how does success/fail of technique work?
            guard still needs to respond to player

don't need to show explicit dialogue choice
guard challenge comes with a number, player lies come with a number
modifiers affect the numbers
    decreasing effectiveness of reused techniques
    guard alertness
    level sensitivity
    alarm / global modifiers
the difference is applied to the suspicion meter
repeated uses of same tactic decrease effectiveness
    this is the only thing that makes variety of tactics worthwhile
strategy of holding good cards for a bad spot works mainly if there are repeated challenges
    we can always beef up the number of challenges
    or choosing when to cut & run
what is the benefit of cut & run?
    if done early, you get a head start on running
    if you wait too long, you lose the chance
drafting cards from a deck?
    you have three upcoming choices: decide to use them now or later, save the bad options for when you have a good play, good options for when you need them
consumable bonuses
consumable data
    example?
    What are you doing down there on the ground?
        Check your records. Joe Armitage ordered this floor to be checked for cyber bugs. Now buzz off.
fake id


how does guard threshold work?
con: unlike previous ideas, this idea abstracts all the negative/positive modifiers to the guard challenge number
    we could keep numbers constant and adjust threshold then: let the suspicion threshold visually reflect the modifiers.
con: how do we handle success/fail responses?
    if player number > guard number, play success. else, fail

## UI

the threshold is the key thing. 
it starts at 50% (say) and decreases for every negative pip,
increases for every positive pip.
display: consumable skill points, consumable data, choices
    color: free pass is always green

## skills

two, three, four options to choose from
greater variety of tactics
more free-play get out of jail cards
multiplier from speech skill
etiquettes?
    corporate, street, science?

## data interaction

data can come from datastore, wastebasket, printouts, desk drawer, filing cabinet, datacube
indicate data type on datastore visibilitty
UI / some way for player to know what data they have at a glance

visibility / discoverability
    can discover connected nodes when you connect to a node, up to your observability rating
    perk: penetrate locks to discover contents
    nodes are buttons: when unlocked, you can activate them
        download data from datastore
        deactivate actuator node
    a virus can hop the network, providing visibility

icons for node type: 
    datastore
    actuator (camera, door, laser grid, alarm grid, transformer, electronic access control)
node flavor?
    cyber, industrial, security, etc. ?
node can be locked / unlocked
    unlocking requires password data, or hacking software
icons for data type:
    paydata
    lie data
    password data
    objective / location
    visibility

hacking verbs:
    crack password
    reveal nodes
    scan data


## map

map now plays a role in discoverability
highlight objectives!
possibly use dialogue with civilian/worker to locate objectives


## status effects
    suspicion records   - number of challenges!
    speech skill        - affects card * 
    etiquette?
    alarm is active     - affects threshold
    in disguise         - affects card * 
    npc alertness       - affects threshold
    level sensitivity   - affects threshold
    tactic use          - affects card
    appearance          - affects card * 

lie, deny, bluff, redirect, challenge


## mechanical questions:
when to handle success vs fail response?s
    <color=#ff4757>[FAIL]</color>

# todo

* create perks
    * unlock redirect
    * unlock challenge
    * 3 cards in hand
    * speech skill
    * higher threshold
    * stall?
* populate perk menu
* 2x across the board
* trustworthy in two places?

wire up perks:
    * challenge
    * redirect
    * 3card
    * speech
    * threshold
    stall

test perks

numbers drawn from shuffled number deck
move card on mouseover
show amount of personnel data
show stack of played tactics
bonus consumable ? 

lock icon in skill menu
highlight unlocked zones in skill menu?



how does stall work?
    when stall is unlocked, show button
    clicking button goes into "stall mode"
        now clicking cards does not play them.
            instead marks them for discard
        cards wiggle to indicate stall mode
        show buttons "discard" and "cancel"
            discard is disabled until one card is selected for discard

