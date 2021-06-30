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