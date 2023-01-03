# level plan

both tactics and map view rely on setting data during plan mode that is used during level
this is kind of like level delta, but we don't want it to reset on retry / level fail.
this suggests a third component of level state: a level plan.
the level plan is created when we open the level for the first time in planning.
it is modified by the planning menu:
    insertion point
    extraction point
    tactics
        disguise
        social engineering info
        fake id
        keycard
    
level state = template + delta + plan

level state is instantiated:
    ✓ level bootstrapper
    ✓ GameData.TestInitialData()
        ✓ level Bootstrapper x 3
        ✓ mission plan debug start
        ✓ load VR mission
    ✓ load VR mission
    GameManager.LoadMission(Template)
        ↳ Mission Fail Retry
        ↳ Mission Plan Controller

So there are only two true code paths through instantiating level state in the proper game.
We need to come up with a larger system for handling these two cases, the rest we can fake.

when we retry a failed mission:
    * level template never changes
    * level plan does not change
    * level delta should reset

when we start from mission plan:
    * level template never changes
    * level plan is created by / modified / provided by planner
    * level delta should reset

level plan should have a reasonable default
gamedata will include 


move disguise out of player state
    disguise is set in level plan
    disguise propagates to leve delta
        delta value is what is polled during level play
        delta value can change during level play
    we could potentially set gun templates in level plan instead of in player state.

when do we update gamedata plans?
on levelstart seems like a good idea.