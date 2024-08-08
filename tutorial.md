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

"we should be able to tap in from here. get out your cyberdeck"
[HOLD X to open the inventory menu]


instead of picking lock on fence, use fence cutter
more cyber network focus
    before approaching the building, use a separate network to scout
    "before we go in there, let's scout the network a bit to find our target first."
    external news kiosk
        cyberdeck -> kiosk -> main trunk
        hop to "main trunk" (1) and scan
        from there hop to building router (2) and scan
        "that's as far as we can get from this 
        
when mousing over in cyber view, highlight path as dotted line, highlight start and end points, show hops in big number over target node
make info panel much smaller.
make unreachable nodes a different color



