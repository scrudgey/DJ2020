# tutorial / story intro mission

# goals to hit

movement
crawl
quick inventory menu
burglar view: lock picking
burglar view: cutting wires
security cameras
stealing data with 
third person mode?

# script

start
focus

player and mentor appear running down an alley
dialogue: "this is the place. follow my lead and we should be in and out with the score in minutes."
stick with me kid, this should be a milk run.

ease that transition from cutscene back to normal cam
* reduce the amount of UI on screen
    * remove appearance status
    * remove overlay bar
    * remove status bar
    * remove visibility
* navigation bit: movement, crawl, rotate camera, zoom in and out
    * UI text blits on screen and tells you controls, stays until the next trigger
    * go around a corner
    * crawl through a gap in the fence
* rain effect should travel with camera focus
    * ladder / interaction

* mentor runs up to locked chainlink fence
* dialogue: "come up over here and open this lock"
* leave focus
* focus on door (with arrow?)

* player opens burglar menu
* pause interactions on burglar canvas 
* dialogue: explains / walks through lockpicking
when door is open, "good work. it's unlocked"
* close burglar
instruction: click on door to open
* when door is opened, focus, mentor runs through
* control camera orientation when moving camera.
* disable overlay controls after getting right overlay


set zoom when panning back to character control.



* focus
* door opens
* mentor runs through, up to hide behind advance position of dumpster
* dialogue: come up here
* when player gets close, take focus, move player into position
* camera moves to look at camera pointed at door
* "that camera is guarding the door we want. let's scope it out before we make a move."
* arrow indicates to activate overlay to alarm
* "just as i thought- that camera is wired into the central alarm system directly. if it sees you, the whole building is gonna come down on us"
* "there might be an easy bypass- let's see."
* switch to power overlay
* "yes, right there. if we take out that power relay it should cut the power to the camera. remember to always look for the holes in security."

move up to the power relay and open burglar view
mentor walks you through cutting the power, and the camera goes down
"that's it, let's go."
the mentor runs through the door into the building (hallway), and you follow.
when you get inside, he goes through another door into the computer room
"okay, the data file we're being paid to recover is somewhere in this room"
walks you through taking out the cyberdeck, stealing the file
"great! what did i tell you kid,


# interdict bad controls
prevent closing the burglar menu
prevent running down the street wrong way
prevent taking gun out (no gun?)


# v2

player comes down the fence-
wait for trigger
 take control and walk up to the mentor
(location mentor_greet_location, player_greet_location)
swing camera down to show building
(cam location mentor_greet)
mentor: "okay, this is the place. before we go in there, let's scout the network a bit to find our target first."


mentor walks over to the kiosk
(location mentor_kiosk)
highlight kiosk location
wait for player to enter kiosk location

"we should be able to tap in from here. get out your cyberdeck"
[HOLD X to open the inventory menu]
input profile: disallow all inputs except weapon wheel


instead of picking lock on fence, use fence cutter
more cyber network focus
    before approaching the building, use a separate network to scout
    "before we go in there, let's scout the network a bit to find our target first."
    external news kiosk
        cyberdeck -> kiosk -> main trunk
        hop to "main trunk" (1) and scan
        from there hop to building router (2) and scan
        "that's as far as we can get from this 
        




tutorial:
    "quiet night. all the better for us."
    make mentor and player face building when they look
    * show / switch to cyber overlay?
    * allow zoom in/out during 
    * fix loadout
        * items, cyberware
    * hide kiosk highlight
    * change mentor instruction to cutscene text

pause when overlay is opened and walk through major parts of display
    first
        okay, we're looking at the network now. 
        this node here is your cyberdeck and the node it is connected to is the wallcom.
    indicate: click on the router node to connect to it.
    connected to router node
        alright, this node is probably connected to the target building. we need to scan it to reveal its connections.
        indicate: use 1/2 scan on the router
        "great, looks like we have a way in to the building. click on this node."
    connect to building router
        "looks like the lab. the data is probably in here some where. but we can't hack this node, we're too far away. we need to take control of a closer node"
    indicate: click on router
    connected to router
        "alright, this is going to be our operating base for scouting their data files. first we need to crack the password"
        indicate: use 1/1 crack on the router
        "now that it's opened up, we take control."
        indicate: use 1/1 exploit on the router
        "perfect. from here, the data nodes are within reach. now let's scan their files."
    indicate: click on datastore 1 2 or 3
    clicked on datastore 1 2 or 3
    indicate: use 1/3 scan file on the datastore
    repeat until target is found
        "there it is, the payload. we can't grab it over the network, the node is locked. we have to go in there. but now we know where we're going."
        "feel free to poke around the network some more if you want. when you're ready, hit the disconnect button"
    show disconnect button and indicate for 1 or 2 seconds

player disconnects
move to fence sequence


include node icon in description text
when mousing over in cyber view, highlight path as dotted line, highlight start and end points, show hops in big number over target node
make info panel much smaller.
make unreachable nodes a different color
