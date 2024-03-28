# montage

the basic idea is that after completing a mission and before returning home, the player gets a choice of location to visit
the location will give a random bonus unlock

1. restaurant
    this is an opportunity to showcase lots of unhinged future cuisine ideas
    probably gives bonus health or something
2. bar
    this is where you make new contacts
        tactics unlock
        black market loot shops reachable by phone
    you are presented with a few contacts and choose between them, like master of orion leaders
3. nightclub?
4. hacker space
    here you can learn new software effects (is this the only way?)

the whole thing to be styled like a kairosoft cutscene

question: when / how to place it?
if we do it as part of after action report, what happens if user force quits after mission complete but before montage?
one way or another we have to save in between:

1. mission
2. after action / montage
3. apartment

so we just need a way of saving/loading at montage point


save points currently:
    scene exit (moving between places)
    main menu save / save and quit
    mission plan cancel (to persist the plan / credits spent)
    start new day
        new game
        end of after action report

this means that:
    currently, player could lose state after mission complete and before end of after action report.
    * we must save immediately after apply reward.
        * change mode to after-action
    * then save after finishing after-action
        * change mode back to normal
    save after applying montage

on gamemanager LoadGame
    change what we jump to 

# restaurant

jack walks into a restaurant

options:
?

outcome:
permanent health boost -> health refill?

# bar

jack walks into a bar

"you enter the bar"

options:
tactics contacts

# club

options:
unlock a fence



* jack walks into safehouse
* open dialogue to map select
* walk cutscene and first text
* jack arrives and sits
* prompt text: choose an option
* selection is made
* write the bar sequence
* faster fade down to montage
* it jumped straight to completion
* remove some objectives from test level
* change caption text "Jack visits the bar'
* no descriptions on characters
* reaction image and reaction text
* conclusion button
* close dialogue
* blit texts
* tactic specifies response text
* draw map
* draw chibi walk
* pause the subway bounce too
* stop animation during montage
* apply unlock mechanism
* determine tactics from state
* this list is used in every mission planning, but those not available for those missions are greyed out
* handle unavailable tactic callback
* draw reaction image
* faster chibi walk animation
* pause the swinging handrails
* skipping reward screws up the balance text
* restaurant scene
* randomize restaurant name
? buttons: clicking/mousing over and then confirming?
* map highlights locations
* fix chibijack walk cycle
* restaurant reaction image
* apply bonus health / health refill
* npcs
* response to unlock contact
    * contact button shows fence 
    * club button mouseover
* nightclub
    * animate lasers
    * fix up response text
    * fill in all fence data
* food react image: noodle bar
    * make head smaller?


food options:
    +10 max HP
    +1 skill point

fence react image

does it make sense to have separate skill points and bonus hp when skill point can be used for bonus hp?
    the strategy is to get bonus skill point and buy the bonus hp to level up before you get the bonus hp.
anything else the player could get?