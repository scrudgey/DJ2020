# overlay UI

* status right now: things are shown that don't need to be

* lock needs to be much clearer

* download button, utility button, password button on info panel

* download button, utility button behind lock

* file info outside lock

* should we be able to see download and utility buttons even when locked?

* don't show node name


0. there is no point is having a node be unknown; if it is discovered its type is known
1. when node is locked
    show "ACCESS DENIED" and password button
    lock/unlocked is always visible and prominent
    when locked: password entry box and password button (what if player has no password data?)
    when unlocked: ACCESS GRANTED
    when being hacked-unlocked: glitch the text entry
2. when node is datastore, show data box

* size of elements
* add scan data to debug loadout
* download button
    * extra ":" text
    . disable download button when download in progress
* wire up password data button
    * "no" sign when zero data
    * change display text to "find password data"
    * usable
* wire up toggle
    * disable toggle when node is locked
    * indicate lock
    * set checkbox on start?
* cyber toggle both enable and disable
* utility enable/disable:
    * allow different texts: cash register, e.g.
* charges larger
* effects smaller
* title/icon larger
* change mode when node changes
. change unknown data node to question file
* change software interface
* hack panel buttons don't make sense grouped with attacker and not on the target
* some text doesn't fit in node info
* neighbor buttons
* change hack panel
    * a collection of software buttons with charges / names visible
* prevent double-launching software
* better disconnect button
    * hide until there is a selection
* fix other network displays
* close hack inteface when disconnecting
* sfx for changing interface
* hack sfx louder

ideally: keep some notion of consistent node when changing overlays
make map data take effect

overlay polish:
    sfx / fx for exploit success
    sfx / fx for unlock
    when mousing over in cyber view, highlight path as dotted line, highlight start and end points, show hops in big number over target node
    change hops display to be more prominent- it is most important.
