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