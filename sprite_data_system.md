# sprite data system



conclusion: the main reason the editor doesn't edit existing sprite data is that 
nothing connected to that actually modifies the spriteDataSystem.
the only way to modify sprite data is to add a new one.
so:
    1. add an "update" button to sprite data system editor.
    2. allows leg sprites to store a torso offset, just as torso sprites have a head offset
    3. apply torso offset at runtime
    indicate when head is overridden
        if head is not overridden, 

how does leg sprite behave?
currently what we call "spriteData" is torso sprite data
    it contains:
    regular torso
    pistol
    shotgun
    smg
    etc.
it contains information about head because:
    position of head depends on torso sprite.
but now:
    position of torso depends on leg sprite.
so we need a separate LegSpriteData.

there is a 1-1 map between torso sprites and torso sprite data.
when we write torso sprite data:
    write it to the index in the array given by the torso index.

adjust code for switching sprite data.
switching torso index is switching torso data.
allow changes to go both ways:
    change torso index -> set current properties to match
    change current properties -> modify current torso data

now we have torso index -> change properties
now we want change properties -> change data

1. set properties -> data
2. set data -> properties

if we change properties, this will overrite data correctly.
but if we change data, it will override the new data with the old settings


we need special rules then.?
if we change torso index -> change active data -> set all current properties from data.
then proceed as normal.

how to initialize data? 
we assume that the data array will contain one data per 

to load the sprites is to load the sprite data.
if no sprite data is found, we initialize the array.

ensure 1-1 torso sprite data with torso sprites, same for leg sprites.
    on load: 
        if the number of sprite data < number of torso sprites, append to the list.
        it will get stored on next save.
        if the number of sprite data > number of torso sprites, throw a warning.
        manual intervention only can save this situation.

we can remove torso sprite index value?