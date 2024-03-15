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
    we must save immediately after apply reward.
        change mode to after-action
    then save after finishing after-action
        change mode back to normal

on gamemanager LoadGame
    change what we jump to 