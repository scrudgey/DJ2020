----

toolbox methods

gun instance 
    in clip, in chamber
    silencer attached
    buffs

project gun tip (!) to 3D world coordinates. it can be done.
standardize audio setup
    * mixer
    * standard rolloff settings

enter wall state when pressing at the normal, but don't release until pressing stops.

start pressing on wall:
    start incrementing counter
    immediately change player material and flatten on wall
if we release at all from wall (+/- 0.75f) before timer is full
    change material back
    start decrementing counter
if timer becomes full
    change mode to wallpress
    change material back to billboard

also, Q/E breaks mode

separate torso and legs animation
    leave bob up to torso animation, to dynamically adjust height: this will greatly simplify all torso graphics.
    requires a slightly more complicated walk animation
    or more triggers in animation and let code handle it <- preferred
    separate leg and torso sprite renderers
unarmed walking animation
unarmed idle
pistol idle
pistol shoot
pistol reload

smg idle
smg shoot
smg reload

how will it work?
    two sprite renderers under regular animation
        * easy to manage walking animation
        * complicates gun logic
            * one code path for unarmed, separate code path for gun handling
    two animators: torso and legs
        * allows gun logic to be managed readily
        * a special case for unarmed
            * torso modes all have idle, some have walking animations


lift up the leg a bit more on walk left/right
downright walk needs a straighter leg

torso graphics cannot be tied to gun!
systematize resource loading
    directory structure
separate unarmed from armed graphics.
    legs
    torso
    pistol
    smg
    shotgun
    rifle
instead:
    a skin object
        unarmed idle / walk
        pistol idle / shoot
        smg idle / shoot
        shotgun idle / shoot
        rifle idle / shoot

new material using texture
    set texture to cutout or transparent
import objects
    set material

bake lightmap with readable texture
    have at least one baked light with shadows turned on
    have at least one mesh renderer
        receive shadows
        contribute to global illumination
        receive global illumination
    generate light data
at least one light marked static / contribute to GI
at least one light with mode Mixed (not baked)
select the generated texture
    read/write enabled, apply
    '


potential strategies
a collider zone based approach
    put floor 1, floor 2 etc. under different roots
    place zones that turn floors invisible
    cons: requires good practices when desiging level.

programmatic raytrace approach
    disable anything above the player
        kind of works maybe
        what about light fixtures?
        performance hit
            implement threaded approach
        what about approaching a hill from below?
        what about moving up a floor, and the above is now all disabled

objects in front, but not above
    raytrace to camera
    change material to be partly transparent?
ghengis dong, khan of the dongolians


changes to wall mode
    different sprite flipping in wall mode
        when i move left, flip x. don't flip again until i move right.
        requires: send input direction together with player direction.
            when in wall press mode, set orientation direction away from wall. (? already done ?)



layer weirdness
    trigger zone is tagged glass so it doesn't block bullets
    shell has its own layer


wall press: how do i implement the next version?
it seems it relies on fine-tuned collision detection.
ray 1: the condition for wall press to hold at all is that there is a wall directly behind the player.
ray 2, 3: the condition for detecting edges is if there is a wall to the left or right of ray 1.


stop before i walk off the edge
    ray 2, 3
use a collider to detect if i'm pressing against a wall for real, and break the wall ratchet if not.
    ray 1
different y offset depending on if crouched or not.
focus camera on nearby enemy if applicable
overall: rely less on wallNormal for state, it is more of an exposed parameter

smoothing camera transitions:
unified code for determining target values, 
unified code for lerping to target values,

two functions each for determining position, rotation, camera values
calculate them both and then lerp between the two values using the easing function


CONTROL QUESTIONS
use a controller, and change the way wall press works to hold instead of toggle
    analog input makes it easier to slide
toggle crouch instead of hold

jumping / highjump / vault



skybox:
possibilities:
    move the camera like before.
        this means there is more detail on distant objects, and not a fixed pixel scale 
        can involve real 3 dimensionality, rotation handled
        Q: what does the final skybox look like?
            concentric squares? 
            concentric cylinders?
            a bunch of sprites
        Q: how to make concentric cylinders that work?
            1. invert normals with shader

            2. cylinder from...... Crocotile??
                if i do this, then i might as well render the whole thing in Crocotile.
                    its image editing is a pain, so i would import texture
                    but then scale of image is tied to the scale of the cylinder. 
    camera fixed, move pixel layers independently
        this is consistent with how 2D parallax works. somewhat simpler
        how to handle rotation?
        Q: 


To make your skybox more believable split your image to 3 different level textures and you can map them on 3 different radius rings 
(make sure your materials are transparent/alpha cutout). This give you fast and nice parralax effect without much effort.

jump mode:

hold jump to start timer
    (this part is unclear exactly)
        crouch down
        prevent movement
        release before timer full: regular jump

    timer full:
        change display:
            linerenderer shows parabolic arc
            input now moves the parabolic arc 
            so at this point, we must be in a different state
        on release:
            super jump

p direction is a culprit,
magnitude is a culprit.

p issue is orientation-dependent.
right now we are calculating in local coordinates.
we need to apply velocity in world coordinates.
inputs are relative to camera orientation



1. rotation of character is affecting the tragectory.
    fixed (0, 0, 1) tragectory faces forward
    player rotates as indicator moves, moving the tragectory to coincidentally match the indicator.
    therefore, the tragectory needs to de-rotate (?)
2. z=1 is forward
3. line renderer and indicator position are local