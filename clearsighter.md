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


apply clearsightblocker to other windows
selectively clearsight based on shooting enemies as well / enemies
fix "isvisible" in gamemanager.level
"dont hide interloper" tag
interloper frustrum should be rotated at an angle consummate with isometric view