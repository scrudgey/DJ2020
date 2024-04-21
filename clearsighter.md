# clearsighter v2

there are two main categories of things to hide: static geometry and dynamic.

static geometry can be pre-computed in a spatial query structure.

we will use an octree, this will allow us to rapidly discover which objects are in which locations.

there are two main strategies for handling static geometry:
1. clipping plane of camera.
    to leverage this, put the camera between the player and interloper geometry.
        you could put it right behind the player, except this would vanish walls & etc.
    secondly, adjust the clipping frustrum. don't move it too far forward
        problem: when on roof, this would eliminate the  view of the ground.

2. octree slicing.
    from a given player position, you can find all objects above, below, etc.

we could query the octree on every frame, but it is better if we query it only when the player moves signficantly (or every 1s, whatever comes first)

a repeating coroutine that updates geometry in chunks.

let's start with the simplest case: put all static geometry in a tree, and disable things above the player.

support disabling / show all for aim mode, etc.


# clearsighter v3

basically should be based around character line of sight
async batching of raycasts and calculations

a suggestion is that it could be based on horizontal raycasts to find nearest walls: but raycasts from where?

radar-like raycast sweep, and cull based on normal dot product with direction to camera.
    half of the radar is redundant?
    the long hallway problem

or raycasts from nodes on a grid toward the camera?
we need to know what nodes are visible to player, 

possibly: a grid of points that start by raycasting against static geometry in direction of camera (?) to know what occludes them
then raycast to each point and know what occludes them, disable those
this is potentially a very large grid

tune: 
    n rays
    n subrays
    n frames




* handle multiple hits on same object
* handle aboves
* raycast hits are hitting triggers
* can we simplify the next/previous batch collections?
* flickering
* some system for only updating active handlers
? we don't need to batch the locating- just the hanlder updates
* classic interlopers
* improve the classic interloper:
* handle windows
* check fps impact
* condition on dot of normal and dot of ray?
* still flickery wall
* SwapExposureBackBuffer could be a proper swap i think?
* support aim, wallpress
* support burglar
? only raycast in appropriate directions away from camera
* handle above properly
* handle non-cutaway
* better burglar solution (above only)
*    nearby to door disables wall
* junk still present in basement
* buildings
* ease disappearance, timed reappearance
* windows still not working
* cutaway renderer
* frustrum size? frustrum geometry?
* change hashsets of renderers to hashsets of transforms
* handle anchor
* clean up code
* sprites??
? interloper frustrum should be rotated at an angle consummate with isometric view
    rotation depends.
* fix "isvisible" in gamemanager.level
* "dont hide interloper" tag
* trees
* apply clearsightblocker to other windows
* aim mode not working
* doors hidden again
* alpha limit not working
* fade in quicker, fade out slowly
* above is sticky
* use alreadyhandled and currentbatch everywhere
* buildings a problem again


apply donthideinterloper to more objects
hvac still doesnt work with clearsight?
figure out a solution for jackthatdata
zwrite off when transparent?


selectively clearsight based on shooting enemies as well / enemies
    how would this work?
    some method to add/remove certain gameobjects from clearsighting
    then either raycast from them or frustrum
        when would we add?
        first guy to start shooting?
        guy who shoots & hits player- show him momentarily?

handle multiple floors
    how would this work?
    moving from floor 1 -> 2, given 1,2,3
        when in between floors, fade in 2. visible: 1,2
        when in between floors, disable upper floors still: visible: 1,2; hidden: 3
            we need interloper activity to carve out some visibility in floor 2. 
        
    moving from floor 2 -> 1, given 1,2,3
        when in between floors

    issues today:
        various objects appear at different times when moving between floors: much better if they all appear at once.




                // how to handle easing between states?
                // we tally transparent requests- if we just went transparent we don't go back to opaque right away
                // we also need to integrate a few transparent requests before we start fading.

                // since we need to change the requests and stayframes per frame, we need to subscribe to time updates.
                // just make sure we unsubscribe correctly!
                // what is the condition there?
                // it would mean currentRendererState == desiredRendererState and also the integrations are 0.

                // allow us to change desired state and subscribe- easy.
                // allow frames / requests to determine when we start to fade.
                //  put all that logic in time update
                // 
                // overall this means we need more state.
                // it means we can have a desired renderer state that is different from the current renderer state
                //  but not updating alpha yet.

                // the main problem with ubsubscribe when current == desired and frames are 0:
                // we will continue to request transparent constantly as object is in view.
                // therefore frames will never be 0 and we will never unsubscribe.

                // desired behavior:
                // state: normal    render: opaque
                // transparent request issued : do nothing
                // transparent request issued : do nothing
                // transparent request issued : do nothing
                // transparent request issued : desired state: transparent, subscrube
                // update: alpha
                // update: alpha
                // update: alpha
                // update: alpha
                // current state == transparent, ubsubscribe



# clearsighter v4

the emphasis here will be on total accuracy, polish, precision
nothing slow

the idea i had is to pre-compute culling of static geometry on a grid
then line-of-sight to each grid point and cull geometry accordingly.

1. placement of grid
    fixed points vs. points attached to geometry
    what if geometry overlaps points? inaccuracy?
2. efficient sampling of grid
    a. only test those points immediately around the player. 
        do not test distance to every single point on every single update.
    b. only recompute those points affected when the player moves.
        this means recomputing only points at the edge of the effective grid.
    c. raycast in a way that is blocked only by occluding geometry
        raycast from top of player to top of ceiling
3. precomputed data structure
    every static geometry gets a unique ID
    every point on the grid lists IDs occluding for each cardinal camera direction
4. define floor heights
5. handlers manage hiding/showing meshes

edge cases:
    1. decals on walls
    2. floors like jackthatdata
    3. moving between floors
    4. non-static geometry


suppose player is at (45, 60, 1). grid extends from [0, 100], [0, 100]. clearsight radius is 20
1. identify floor player is on, or if player is between floors
2. now we need to sample points from (25, 40, 1) to (65, 80, 1)
3. grid is a 2d array. suppose first index is x.
    we want to sample points grid[25] -> grid[65]
    within each grid entry are y entries
    grid[25] = grid[25][0]...grid[25][100]
        we can easily sample this to be between 40 and 80
    
    grid[25][40] ... grid[25][80]
    grid[26][40] ... grid[26][80]
    ...
    grid[65][40] ... grid[65][80]




* in editor mode, create grid of points, one grid Aper floor, bounded by the bounding box
* find all static geometry roots. create a new component on them that will manage culling.
    * give each culling component a UIID
* for each point, find all intersecting static geometry along each cardinal direction, store the UUIDs
* write the information to file.
* during gameplay:
    * on start:
        * load the data
        * find all culling components and store them by UUID key in a dictionary.
    * on update:
        * raycast efficiently from the player to every grid point on this floor within the culling radius
        * for every visible grid point, get the list of culled UUIDs along the cardinal direction
        * cull the geometry accordingly- let culling component handle it
* problem: all of the async raycast batch stuff is assuming a fixed buffer size
* handle aboves
    * load all culling components into an octree
* skinny objects: pad out with extra collider
* handle stuff in folders!
* build test level
* culling volume
    * better handling above/below etc
    * implement floor-based culling
        * dictionary maintains status of each floor (visible, invisible)
        * methods to detect when player leaves floor
        * coroutine for managing disabling floors
        * collections of culling components per floor
* handle different camera modes
    * revert floor visibility back when re-entering state
* exterior roof zones
* make rooftop zones
    * multiple colliders
    * colliders have a noninteracting layer that hits the raycast and a tag
* tag culling components with rooftop zone when computing
    * allow id -1
* track if grid raycasts hit rooftop zones
* track which zone the player is in
* track interloper state of rooftop zones
* floorstates must be nested under roof zones
* handle dynamic sprites
* separate the time updates between dynamic and static
* handle floor transitions
    * clearly we must detect when the player is in a buffer zone between floors and then change visibility accordingly
    * interloper logic will have to change: don't handle things as above in interloper.
        * only handle those things on the grid floor, and as interloper only.
* figure out why the floor has that final invisible region
* somehow, state is sticky when moving between floors



handle world levels
    culling data should be tied to scene perhaps







optimization: somehow remove points with no culling data?
    do not return these from the grid search
    set a flag during volume creation for easy check

if we need to fine tune dynamic culling, code can interact with culling component
    have a flag for allow culling
    when flag is turned on or off, reapply culling status

player shadow!
    duplicate sprites in shadows-only mode


fade?
scale test
    number of points (grid spacing)
    size of grid search

