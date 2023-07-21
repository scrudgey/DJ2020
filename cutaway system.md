# cutaway system

architectural cutaway

# solutioning

some possibilities:

1. a separate mesh for each wall type
2. wall type with dramatically smaller y scale
    (temporary POC)
3. custom shader?


## considerations

we need to leave the regular mesh intact to keep the shadows and lighting intact.
so this means whatever happens,
    1. main mesh becomes shadowsonly
    2. a second mesh is revealed (how do shadows work?)


how do we detect cutaway enabled meshes? a tag?
how do we avoid detecting on the cutaways? a tag?
    if they are generated after tree is initialized, then they wont be in the tree.

generating the mesh dynamically doesn't work because of batching. do it manually.
    then, detect the cutaway