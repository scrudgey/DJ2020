# gun shop

Basic requirements:

1. show guns for purchase
2. inspect a single gun for purchase
    details: 
        image
        description
        weapon stats
3. show guns in inventory
4. compare gun from inventory to gun for purchase
5. purchase gun
6. sell guns from inventory
show credits
show shopowner
show shopowner dialogue

buy/sell could be different modes.
guns for purchase is a clickable list.

comparing is difficult: what are we comparing to?
    in DJ1, pistol to pistol, there's only ever one.
in DJ2, you can have an arbitrary number of each type of gun:
    multiple pistols
    multiple smgs
    0 pistols and 3 smgs
    2 copies of the same smg

buy mode:
    show credits
    show list
    click a gun from the list
        populates the description
        grey out button if too expensive
    click buy
        added to your inventory, removed from store inventory
        credits deducted
sell mode:
    populate player inventory list
    click an item
        display stats
    click sell
        removed from inventory, credits added

dialogue can have gun shop and player portraits
+ dialogue describes weapon and allows flavor
+ player can provide feedback when gun is bought or sold.
+ shop / player on left / right can be used to differentiate buy / sell

initialize all fields properly
    initialize stats empty 
    * clear compare value
show gun image in selector buttons
button colors are bad
include bars
    bars lerp toward values
    bars show +/- versus compare
separate gun lists by gun type with header
"Buy" button is styled different from bottom butsons
sfx

* seller wares should be bigger
* spacing
test with rifle, shotgun, smg, with spacers
fix inventory 

update stats when compare gun is clicked
why are there two recoils and no weight?