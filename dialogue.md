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
    objective
    visibility

hacking verbs:
    crack password
    reveal nodes
    scan data


## map

map now plays a role in discoverability
highlight objectives!


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

player statuses affect cards
npc and level statuses affect threshold

maybe the statuses don't appear on the portraits
    visually awkward
    what do they modify?
        suspicion threshold
        player card rating

start:
    initialize dialogue
    show initial challenge
    ease in suspicion bar
    set suspicion threshold
    show challenge level and indicate it on the suspicion bar
    ease in dialogue responses & challenge level
        lie
        bluff
        id card, etc
    dialogue responses start disabled
    activate response buttons

    when player mouses over response button:
        ease the card up
        indicate effect on suspicion bar
    
    when player clicks response button:
        activate card:
            move it out of stack
            apply card and challenge to suspicion bar
            ease in new card
        show dialogue
        show continue button
    
    when player clicks continue button:
        resolve effect on suspicion bar
        show dialogue response
        show continue button

    when player clicks continue button:


✔ separate dialogue management into its own class
update the UI
create the new UI:
    suspicionbar
    responses and challenge

why not just play best card always?
    the guard challenge is random- you don't want to waste a good card on a weak 
    challenge when you'll need it later.
    your best card might be a high scoring lie, but it'll dilute your other lie, when you could use the moderate scoring bluff instead
    mostly when you're trying to buffer for later challenges

    if you are holding a high 10 lie, and two 5 lies
    playing the 5 will dilute your 10.
    playing the 10 will dilute your 5s, 

    the penalty is for playing x lies in a row:
        now, you just played a 5 lie to preserve your 10
        and you get another 5- now worth 2.5
        do you play it and hope to get a bluff? preserving the 10?
    this only really works if there are multiple challenges-
        sometimes we see one, sometimes two
    but if the bullshit meter & cards carry over, then it adds to the strategy.

    since conditions change, you might start out the conversation
    in a deficit, you have to work it down a lot to get out ok

end conditions
    does not end when you're above threshold
    this gives you opportunity to cut & run

extra ways to pay down bullshit meter
    consumable skill

lie, deny, bluff, redirect, challenge




* card state belongs to level delta
* bullshhit level belongs to level delta
* show initial dialogue
* support threshold 
* set challenge text
* clicking a card triggers the correct text response
* overall flow control 
    * when challenge starts: 
        * npc dialogue happens
        * then apply bullshit meter (rising sound)
        * then enable cards
    * player chooses a card:
        * disable cards
        * card goes up and disappears
        * player dialogue happens
        * apply bullshit meter    (decreasing sound)
        * new card?
        ? continue button appears
    * player presses continue:
        * npc response happens
        * apply bullshit meter?
* audio comes in first?
* no sound for second bullshit move?
* fractional bs amout
* bullshit starts 55- should start 0, ease up to 55
* threshold starts 85- should start 0, ease up to 85
* setting bullshit to 0
* then updating to current bullshit (0)
* takes too long
* more delays between things
* draw new card
* card numbers generated correctly
* some delay between card action and responses
* blink final threshold text for emphasis
* no sound for threshold meter
* standardize card drawing
* disable and enable cards correctly
* ease in bullshit meter from the side
* challenges generated correctly
* show [LIE]
* allow status effects
    * correctly show base value
    * apply status effect
    * number of lies told
* 2 cards
* faster card play
* challenges correct
* conclusion
* conclusion:
* when end is pressed:
* denser dialogue segments
* correct ease out of dialogue 
* fix textblit to blit all of <>at once
* "told 5 denys"
* set names
* prevent clicking of multiple cards

blink "warning" when over bullshit threshold

escape
items
    ID card
    personnel data

when to handle success vs fail response?s
continue button in dialogue container?
support nimrod




mechanical questions:

    threshold correct
        take status effects
    bonus consumable
        double value
        bonus decrease
        
    flaw:
        if the threshold is high- you just need to hold a card for 100-threshold until the last challenge.
    challenge 60
    lie 30      -> 30, lie 24   (-6)
    lie 47      -> 13, lie 15   (-2)

    the main only reason to not play the high card is to save it for later potential
        -> but your strategy can be to just save one high card until you hit bullshit threshold?
    in effect, you want to match the played challenge.
    suppose you have two lies and the choice of playing a high card against a low challenge, or a low challenge
        play the high card and you waste it, play the low card and you sabotage the high card

    what about success/fail rolls?