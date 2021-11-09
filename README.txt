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


what does robust binding UI look like?

1. on player change (or whatever) gamemanager publishes a static action OnTargetChanged
2. UIController: 
    start: 
        listens to OnTargetChanged.
        OnTargetChanged(GameManager.target)

    OnTargetChanged: 
        gun display bind to new target.getcomponent<gun>
        health display bind to new target.getcomponent<health>

3. Binding: 
    fetch target component IBindable
    target?.onValueChange -= HandleValueChange
    this.target = target
    if target != null:
        set up display
        target.onValueChange += HandleValueChange
        HandleValueChange(target)
    
    HandleValueChange:
        update view

    it desubscribes frow whatever it is listening to and subscribes to the new component's OnValueChanged
4. When OnValueChanged, update our view.

let's trace out how console works as an example.
gamemanager listens to ~ input action.
on ~, transition to state inMenu.
    either we communicate with UIController, or we load a new scene
    OnUIInput(UIInput)
        -> UIController.handleUIInput()
    Showmenu(Menutype.console)
        scenemanager.loadscene("console")


when leaving state gameplay, set time delta to 0.
on player input, check game state.
when transitioning back to gameplay, set time delta to 0.

UIController: 
    handling callbacks from control elements
    eventually, something 


TODO: how can we use event callbacks while maintaining split state?
problem: two state machines that communicate by binding. instead one state should be subordinate to the other.
the problem is: 

GameManager can tell UI to open menu, and to close menu. Game State enters a different state.
UI should be able to close its own menu. easy for it to encapsulate its own responsibility, but when it closes its menu, gamestate also needs to change.

state tracking (gameplay, which menu) should all belong to a single class. UI just needs to react to the opening and closing of menu state.



damage decals
can we apply a decal to a specific face?
    either this requires instancing of materials, or spawning a new textured object on the face.
    decal material can prevent z fighting.
        1. get triangles
can we modify vertex colors?
    yes, provided:
        1. material property block with vertex color is used for instancing
        2. we duplicate the standard shader to support vertex coloring from material property block

damage decal system
    1. set up the monobehavior on each damageable object.
        initialize vertices, etc. here.
    2. track which faces have been hit. don't double-apply.
    3. use object pool
    4. expose a method for damaging the struck face
    5. configurable damage decal
    6. proper geometry for damage decal


glass shard uses sprites-default
leaf uses octo
magazine uses shadowspriteused
bullethole uses shadowspriteused
damagedecal uses decalMaterial

lit/unlit
billboard/normal

normal unlit = sprites-default
billboard unlit = ???
normal lit = shadowspriteused
billboard lit = octo

decalMaterial : overlay

combine decal material and shado
apply z ordering to shadowspriteused

big things looming:
NPCs
game system

abstract bullet impact subscription
allow bullet impacts to apply physics

set layer
make soda / snack bouncy
impact sounds
stop rolling


1. disable ceiling hide unless player is obscured
2. 

hide whole ceilings at once?
don't mess with transparent materials
    what is happening? zmash gets written at 2500


geometry    2000
zmask       2010
masked      2020
alphatest   2450
transparent 3000

mask masks everything > 

UI
items
explosives
explosions
destructible walls


pool: problem: now tht we have abstracted the pools,
how do we address them?

1. use enums
2. attach a component to pooled objects, to identify their pool
    pooled object component can handle setup / teardown

even abstract the pool management part: 
Initialize the pool on Get if it doesn't exist. classes can either initialize a pool on start or let it wait for runtime.

when the pool object is created / initialized, add to it 

her solar panels drank in the moonlight of secunda II

unified damage and destructible model
divide explodable from destrucible wall?

destroy walls

unify takedamage
Damage class
unify gibs and glass shards

1. review how it is done in YC3.
review what we like & don't like.


1. interface with takedamage and implementation for destroy
in this case, each component will track its health independently?

Explosion gibs:
    position random
    force directed by explosion
Glass gibs:
    position partly determined
    force directed by damage direction
destructible object gibs:
    ??
    does this handle a particular type of damage?
    what happens if glass and destructible on same object? they should not be.
    then gibs will be generated more like explosion


susceptible
takedamage
dotakedamage
destruct
    so maybe glass is just different from gibs?


split IDamageable and IDestructable
Susceptible vs. pattern matching?
Susceptible can determine if we should react
    but then, can we at least remove DamageType?
is there a way to abstract this pattern?
1. register Action<Damage> types? 


cars driving by ambient noise interval random
tree, bush, lanterns / things connected to random wind gusts
crickets




layers

shell: collides with geometry, nothing else
object: collides with geometry and object
    player character
    required to have a third thing that can collide with self and not geometry
skybox: camera
shadowprobe: camera

proposal:
    new layer:
    bulletPassThrough: collides like default


bullet raycast: 
    layer mask: include object, default

cursorToTarget raycast: 
    layer mask: include object

clearsighter overlap:
clearsighter raycast:
question: is there a better way to handle these things?

problem:
    we need bulletPassThrough tag because that's how we handle bullet holes

pistol, rifle, shotgun, smg skills
