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