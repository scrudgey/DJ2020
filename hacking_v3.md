# hacking v3


fixed number of slots in cyberdeck for consumable software
software has a number of charges, but default is 1
make entire cyberdeck pane fill right side of screen
configure cyberdeck before mission


what parameters of software?
    type: targeted, virus
    charges: number, max, unlimited
    upload time
    effects:
        reveal node type
        reveal node edges
        reveal datafile
        crack password
        exploit
        disable utility
        "cover trakcs"
    conditions:
        limit on node type          (likely for exploits?)
        limit on node encryption    (likely for crack?)
        limit on hops
    timed vs. permanent



## the terminal paradigm

all info and interface is shown through a terminal
user controls via buttons that "type commands into the terminal" with results shown.

*the key purposes*:
1. the terminal instantly evokes the expected flavor and setting
2. the information to be conveyed is immediately idiomatic in the terminal
    ("enter password:", "access denied", "access granted, choose a menu option", etc.)

overwhelmingly 

## information that must be displayed as part of overall graph interface

node name & type
node icon
neighbors

## information that is unique to cyber node interface

1. locked / unlocked
    a. password prompt
2. compromised / uncompromised
3. utility to activate / deactivate
    a. menu options for unlocked node
4. data to download
    a. menu options for unlock

## software interface

a number of buttons of your consumable software
information is shown in a modal dialogue
    (effects, etc. to be tied to software crafting)
deploy action is controlled in modal dialogue

downloads / uploads in progress are shown in the terminal
and iconographically in the graph overlay.

software can be gated behind the modal dialogue

### when locked

prompt reads: enter password
player options:
    1. enter password from keyset
    2. use crack software

### when unlocked

prompt reads: make a choice from menu:
player options:
    1. download data
    2. use exploit software

software:
    is it all shown, all the time in that box? even things we can't use?
    how does it comingle with menu choices?


## the problem of inaccessible data

somehow, you must always be able to retrieve target data.
    you are given key software before the mission that will always unlock the target.

## the problem of failed attacks

if software is consumable, it has finite uses; 
but not all software can be used on all targets?

## changes to the software paradigm

download is no longer a software, it is a menu option on an unlocked computer.
    this is probably okay.
    but we will want more types of software, maybe they can be specific to types of node as always imagined.

mostly, you will be deploying a password or a cracker to gain access: bypass the "access denied" prompt.

then you have access: you can download and choose menu options. but the terminal is not yet compromised.

you deploy an exploit: now the terminal is fully cracked and you can use it to launch attacks on other nodes <- this part is still pretty poorly conveyed.

how does scanning work?

scanning reveals:
    type of node            -- node info
    neighbors of node       -- node info
    contents of datanode    -- could be shown in terminal in some way.

## unlocked versus cracked

perhaps it should be clear in all cases where we are launching an attack from










# gamplay sketch

you approach a computer with your cyberdeck.
it detects the computer and places a node on your overlay, but unknown
you click the node, opening the overlay display.
the node info slides in, then the cyberdeck / terminal interface
the node is unknown, so the node info display is mostly empty:
    icon is ?
    title / type is blank
    neighbors unknown
your main point of interface with the node is the terminal.
in this state, the terminal starts with the automatic command:
    > ping 127.0.5.10
    PING localhost (127.0.0.1): 56 data bytes
    64 bytes from 127.0.0.1: icmp_seq=0 ttl=64 time=0.118 ms

your options at this point:
    1. log in
    2. hack software... -> scan

you choose to scan. the terminal displays:
    > nmap -v -sS -O 127.0.5.10
    Starting Nmap 7.94 ( https://nmap.org ) at 2024-02-16 20:44 PST
    Initiating SYN Stealth Scan at 20:44
    Scanning localhost (127.0.0.1) [1000 ports]
    Discovered open port 49152/tcp on 127.0.0.1
    Discovered open port 5000/tcp on 127.0.0.1
    Discovered open port 7000/tcp on 127.0.0.1
    Completed SYN Stealth Scan at 20:44, 0.01s elapsed (1000 total ports)
    Initiating OS detection (try #1) against localhost (127.0.0.1)
    Retrying OS detection (try #2) against localhost (127.0.0.1)
    WARNING: OS didn't match until try #2
    Nmap scan report for localhost (127.0.0.1)
    Host is up (0.00028s latency).
    Other addresses for localhost (not scanned): ::1
    Not shown: 997 closed tcp ports (reset)
    PORT      STATE SERVICE
    5000/tcp  open  upnp
    7000/tcp  open  afs3-fileserver
    49152/tcp open  unknown
    Device type: general purpose
    Running: Apple macOS 11.X
    OS details: Apple macOS 11 (Big Sur) (Darwin 20.5.0)
    Uptime guess: 0.000 days (since Fri Feb 16 20:44:57 2024)
    Network Distance: 0 hops
    TCP Sequence Prediction: Difficulty=263 (Good luck!)
    IP ID Sequence Generation: All zeros

    Read data files from: /opt/homebrew/bin/../share/nmap
    OS detection performed. Please report any incorrect results at https://nmap.org/submit/ .
    Nmap done: 1 IP address (1 host up) scanned in 3.63 seconds
            Raw packets sent: 1044 (47.556KB) | Rcvd: 2087 (89.532KB)

now the node has greater visibility:
    1. neighbors
    2. type / title
    3. data file
    etc.


your options at this point:
    1. log in

you choose to log in. the terminal displays:
    > ssh 127.0.5.10
    * welcome to city sense/net! * 
    new users must register with sysadmin.

    enter password:█

your options at this point:
    1. enter known password
    2. hack software... -> crack

you can enter a known password from a selection menu:
    list all known passwords and indicate if they have been tried / failed
    try a password, it gets typed in and the screen reads
    ACCESS DENIED or ACCESS GRANTED

if you apply crack:
    the screen fills with randomized gibberish for a time, until the process is complete,
    then: ACCESS GRANTED

the terminal displays:
    ACCESS GRANTED
    * welcome to city sense/net! * 
    make a selection from the menu below:

    1. access data file
    2. system operations
choices:
    1. download data
    2. sysops
    3. software ... -> exploit




is it all too cumbersome?
it's just pushing buttons, as before- the only difference is hiding software behind a modal dialogue.

some subtle differences:
1. user must select "log in" before trying password or crack software
2. compromised vs. unlocked is still unclear:
    you can only launch attack software against a neighbor of a compromised machine
    this is kind of hard to see in the current setup.
    i.e. you can still ping around and map the network without a compromised machine

process should look like:
    1. mapping the network / discovery
        finding nodes
        finding connections to nother nodes
        identifying node types
        identifying node datafiles
    2. identify a weak point in the network
        an unpatched printer has vulnerability to software you have
        you crack the printer
    3. ???
    4. download data / turn off cameras / etc.
    5. cover your tracks

i really like the progression 1 -> 2, and it sets up specific compromising software
but what is the path 2 -> 3?

in real life, you compromise a node to get deeper access / visibility
    (presumably, here you already have visibility, don't need access to printer, you need access to datastore)
lateral movement across the network
    your hack software can't get through the firewalls & etc.
it is easy enough to enforce the neighbor condition of hacking, but it is unclear to the player
    1. i can try to log in to any machine from anywhere, right?
    2. i can scan to my heart's content, until i run out of charges maybe
    3. so how do i know when i have an attacker machine?
        terminal always visible, log / scan always visible, but
            this doesn't work if scan is hacksoft
            but if scan isn't hacksoft, then what is it and how is it ever limited?
            limited hops? stopped at firewalls?

move terminal to node info, have attacker machine on bottom only visible when thing is vulnerable
and identify attacking node (cyberdeck, etc.)

bigger problem: how do i do #2 (compromise a printer inside the network) if i have to be adjacent to it?
    i can eliminate the neighbor condition, but then why compromise the printer at all?
without the neighbor condition, maybe there is a benefit to having a number of compromised machines on the network
    that seems too complicated. i.e. you can hack a level 3 encryption with level 1 software if you have 2 compromised neighbors to join in etc. 


# v3 paradigm
move terminal to node info, have attacker machine on bottom only visible when thing is vulnerable
terminal always visible, login / scan always visible
scanning is limited to a number of hops from a compromised machine
so you start by scanning around, identify a weak link, compromise it, scan some more
you can't hack a machine you haven't scanned, or if there is no link to a compromised machine
firewalls stop a scan hop as well.
    does this work?
    1. performed by scanning from WAN or whatever terminal you connect to
        you find many computers, lots of them locked higher than you can break
        you find one unpatched printer at the edge of your visibility
    2. you can login with stolen creds or use crack, enabling more hops, more discovery
    3.a. you go more hops, and identify the datastore
    3.b. you login to the data node with stolen creds or use crack
    4. download data
    5. cover you tracks- WIP

communication of the hops to the player might be a challenge, but it is doable.
software usage paradigm:
1. select a node then the usable software is displayed
2. select a software and click on a node
    this can provide a different kind of feedback, but it's a little clunkier.


## terminal and interface in v3 paradigm gameplay sketch

you are approaching your target, an office building.
you locate the underground internet trunk connection and attempt to connect through that terminal.
- graph view
- click on WAN icon
- node info for WAN
    terminal: (node is discovered and is range) 
        > nmap -v -sS -O 127.0.5.10
        Starting Nmap 7.94 ( https://nmap.org ) at 2024-02-16 20:44 PST
        Initiating SYN Stealth Scan at 20:44
        Scanning localhost (127.0.0.1) [1000 ports]
        Device type: WAN trunk
        Running: Apple macOS 11.X
    terminal options:
        <none>
    hack options:
        scan

you hit scan and it maps the neighbors of the WAN
multiple new nodes are visible on the overlay, but they are unmapped- ? marks
you identify the node that is likely the entry to the office network and click on it
- node info view for unknown
    terminal: (node is unmapped and is range)
        > ping 127.0.5.10
        PING localhost (127.0.0.1): 56 data bytes
        64 bytes from 127.0.0.1: icmp_seq=0 ttl=64 time=0.118 ms
    terminal options:
        <none>
    hack options:
        scan

you scan the node and learn that it is the firewall
    terminal: (node is discovered and is range) 
        > nmap -v -sS -O 127.0.5.10
        Starting Nmap 7.94 ( https://nmap.org ) at 2024-02-16 20:44 PST
        Initiating SYN Stealth Scan at 20:44
        Scanning localhost (127.0.0.1) [1000 ports]
        Device type: firewall
        Running: Apple macOS 11.X
    terminal options:
        log in
    hack options:
        <none>
your progress in scanning / mapping further is halted by the firewall unless you can break in and disable it- you can't.
you move on

you work your way to the back of the building, pick the lock on a rear door, and explore the back rooms and hallways
you find an ungaurded network room
inside is a terminal- you connect here. 
the node is mapped because you physically connect from your cyberdeck.
- graph view
- click on terminal node 
- node info for terminal
    terminal: (node is discovered and is range) 
        > nmap -v -sS -O 127.0.5.10
        Starting Nmap 7.94 ( https://nmap.org ) at 2024-02-16 20:44 PST
        Initiating SYN Stealth Scan at 20:44
        Scanning localhost (127.0.0.1) [1000 ports]
        Device type: terminal
        Running: Apple macOS 11.X
    terminal options:
        log in
    hack options:
        scan
        crack

you don't want to waste your crack software on this terminal just yet. 
you want to map more of the inside of the network, so you use scan
this maps the neighbor connections from this node, revealing more nodes.
- click on unknown neighbor
- node info view for unknown
    terminal: (node is unmapped and is range)
        > ping 127.0.5.10
        PING localhost (127.0.0.1): 56 data bytes
        64 bytes from 127.0.0.1: icmp_seq=0 ttl=64 time=0.118 ms
    terminal options:
        log in ?
    hack options:
        scan

the node probably isn't a data node like you're searching for, but you're getting closer.
you hit scan again- you can scan nodes up to 2 hops away (1 hop = terminal, 2 hops = this unknown node)
the scan reveals several more unknown nodes
you scan each of them in turn, revealing their statuses:
    1. highly protected security nodes
    2. highly secure other nodes
    3. a vulnerable printer node
- click on printer node
- node info for terminal
    terminal: (node is discovered and is range) 
        > nmap -v -sS -O 127.0.5.10
        Starting Nmap 7.94 ( https://nmap.org ) at 2024-02-16 20:44 PST
        Initiating SYN Stealth Scan at 20:44
        Scanning localhost (127.0.0.1) [1000 ports]
        Device type: printer
        Running: Apple macOS 11.X
    terminal options:
        log in
    hack options:
        crack

you run the crack software
    terminal:
        please log in.
        password: 
        <hacking gibberish>
the printer changes to compromised state.
now more nodes are in range- compromised node provides instant visibility into neighbors
you scan from the neighbors and locate target
scan target
log in
terminal: (node is discovered and is range) 
        > ssh 127.0.5.10
        password:
    terminal options:
        password
    hack options:
        crack
you gain access through crack
terminal: (node is discovered and is range) 
        > ssh 127.0.5.10
        password:
        welcome to sense net!
        *-------*
        | file1.dat
        *-----
        etc.
    terminal options:
        download
    hack options:
        exploit

summary:
    node info unchanged.
    terminal is flavor depending on node visibility and state.
    terminal UI exposes what used to be utility toggle, and download software.
        addition of login prompt- it can be automatic
        none, log in, download, toggle
    this could be implemented today.

i think it's clear how it progresses, but it hinges on one thing:
question: you can always hack anything you can reach?
    scans only reaching 2 hops is just like cracks only reaching 1 hop
        if you can convey one, you can convey the other.
    i still like restriting cracks to 1 hop
        possibility of discovering data node but you still need to access it physically to download because you lack an outpost near it
    kind of weird if scans are limited but cracks are not
        it also might make it very easy- you can hop through the network until you find your target?
        and nothing can stop you then?
        you're still limited by software charges.
    the key is making number of hops intuitive.
        to make number of hops intuitive, also clarify originating point obvious.
        this would be the case if you first select compromised node, then select hack software
            how would the player ever know to click on the cyberdeck node?
            make a big obvious button on compromised nodes?
            clicking away from your selected node to bring up hack options feels wrong
            somehow, both must be selected at the same time.
    in fact, i've glossed over the process of choosing an origin when multiple compromised nodes exist
        i blithely assumed it'd be the closest compromised node.

    place software buttons on selected nodes:
        easy to show what can apply, directly
        hard to show where it is coming from, and limitations
        hard to show if a software cannot be used, why (too far awaym etc.)
    place software buttons on compromised nodes:
        easy to show full set of software that is available
        hard to show what each thing could be used on (printer hacking software, but no printers present?)
        hard to show if a software cannot be used, why (too far awaym etc.)

    both approaches fail to really explain why software can be used in what cases.

selecting a node for information and a node for hacking are two different things.
in cyber overlay, you can browse around and view node info for all the cyber nodes on the map.
compromised nodes and the cyberdeck have a big red "start hack" button
when you click the "start hack" button, the terminal (previously cyberdeck interface) pops up
a "stop hack" button appears
terminal shows: X -> connecting to Y
the path from X to Y is highlighted
here, the terminal shows just as if you were at X interfacing to Y over network (ping, nmap, ssh, etc.)
it can be clearly explained now, origin, destination, hops, software eligibility, etc.




# todo

* when a node is compromised, show hack button
* when hack button is clicked, set hack origin
* when hack origin is set: 
    * show indicator
    * show terminal
    * show terminal conditionally
    * highlight connection from attacker to target
        * update this path when attacker or target changes
    * show "stop hack" button when hack origin selected
    * maybe don't show hack button unless a node is already selected
    * ease in terminal
    * set terminal text
    * set terminal buttons
* animate path from hack origin to target
    * when path changes and is not empty
    * start from origin, adjust line render gradient origin -> node1
* points not deterministic: probably a node order issue. fix by sorting
* now points are correct but start and end are occasionally flipped.
    * we need to adjust _Start or _End depending on case
* hide terminal if no path between attacker and target
* don't reveal terminal until line is completed
* compromised nodes have skull & crossbones icon, except WAN
* show attacker above terminal i guess
* implement software template -> software state, player state
* software scriptable template -> templates -> state in default state
* wire up modal dialogue, make it usable
* modal dialogue -> terminal, take effect (playable)
* modal software dialogue
* melee ui starts open
* cyberdeck ui controller should not be visible
* "hack" button colors are bad
* view scroll view should clamp etc.
* terminal state gets weird
* update terminal when node status updates
* ui elements over software modal
* allow download
* allow toggle utility
* disable utility buttons
* change neighbor button colors
* show unknown edges in neighbor dialogue?
* must click software button agian before it will work
* node colors no longer make sense
* when clicking too far away, end hack state- this is an overall limit on attack distance?
* handle downloads
* display max hops in terminal
* hack button should hide when selected target is too far away
* apply conditions
* apply charges
* explain effects
* explain conditions
* explain charges
* explain disabled/non
* show status bar in overlay
* separate scan from scan edges from scan datafile
* software selector mouseover too close to disabled
* even when node edges are known, cyber info shows edges as unknowns unless node is known
* even when data file is known, data indicator not shown unless node is known
* smaller terminal window
* hacking from cyberdeck
* color of text in cybernode info panel
* "player" should be "cyberdeck"
* remove edge line render when edge is removed
* cybernodeinfo overlaps item view
* player -> node edge should not have elbowing
* should not mouseover cyberdeck indicator
* red lines don't hide / don't go away
* do something about terminal jumping when indicator revealed
* player node position
* only show player node if cyber deck is out
* overlay box should have fixed sizes
* button colors
* shouldn't have to hack a compromised node to download file...??/
* sfx
    * network action in progress
    replacement for the thing that sounds too like stickykeys
    * deploy sounds
    * start hack path
    * click node
    * mouseover node
    * typing command in terminal
    * show terminal
    * select hack origin
* adjust other info panes to match cyber
? delay after enter -> output
* download button colors
* network action sound is still old?
* effect when revealing nodes
* iconography
* modular view
    * display template mode
* populate slots from inventory
* configures level plan
* slotcolor = green or something
* level plan should take effect
    * plan templates -> player state analogous to items
* deploy buttons during gameplay
* populate slots with default
* check all instances when node visibility changes
* first screen is list of software with inspector
    * software view
    * delete modal
    * a button opens the craft interface
* craft interface:
* wire up
* design payload picker
    * payload picker button
* close initial dialogue
* number icon on effect selector
* move design points / cost
* add software view close button
* fix "set name" warning
 *type, 
* design conditionals system / picker / options
* hide virus params when exploit is selected
* conditions per effect?
* prevent double-adding effects
* modify view to show size
* change how close mission select works
* change costs and sizes
* icons
* fix name input
* allow edit template
* fix software loadout
    * when clicking a slot, if empty, show selector
    * if not empty, populate software view
    * allow permanent software on cyberdeck
    * create cyberdeck template
    * display for storage capacity
    clear slot button?
* fix software view
* charges not consumed
    * state needs to be more 
    * requirements not working
    * could deploy when requirement not met
    * could deploy crack_inf at distance
    * cyber graph inspection should use new clearsighter
* fix hacking interface
    * failed to connect cyberdeck -> camera after a few scans
* dont show mb size in hack view
* software view
    * separate requirements, description, charges
    * effects should show icons
* cancel edit does not cancel



# load up cyberdeck during mission planning

cyberdeck stats
    software slots, or size?
    slots: immediately intuitive, cleverly limiting
    sizes: 
        idiomatic
        more complicated
        more possibilities
        more customization to software
start with slots

come up with overall design:
    clear slots
    draw image of sorts

# craft software between missions

what parameters of software?
    type: targeted, virus
    charges: number, max, unlimited
    upload time ?
    effects:
        reveal node type
        reveal node edges
        reveal datafile
        crack password
        exploit
        disable utility
        "cover trakcs"
    conditions:
        limit on node type          (likely for exploits?)
        limit on node encryption    (likely for crack?)
        limit on hops (part of cyberdeck?)
    timed vs. permanent ?

# enemy counterattacks

hide node / hide edges
make node unknown again
repair compromised nodes

# sketch out

1. what does software inventory look like?
2. what does cyberdeck load interface look like?
3. what does cyberdeck use interface look like?


## purpose of conditonals vs. design 

originally i made the conditionals to give direction to the player and prevent
them from wasting charges.
1. can't unlock an already unlocked node
2. cant scan an already scanned node
3. cant compromise a locked node
4. cant scan file on a node that isnt a datanode

this style of constraint can be determined based on payload. it is bedrock limitation.
we can make it transitive:
1. exploit -> requirement: node is unlocked
2. exploit + unlock -> requirement: none


the idea for design constraints was a way to recoup design points.
are there examples of other constraints we could voluntarily apply:
    1. node type:
        camera, datanode, router, alarm, etc.
    2. node levels ?
    3. time limit?


// if exploit, require unlocked     (lockLevel)
// if unlock, require discovered ?? (nodevis > mystery)
// if scan, require not discovered
// if explot + unlock

crack:
    node is locked
    node is known?
crack_inf: 
    node is locked
    manual hack
exploit:
    node is unlocked
    node is not compromised
scan:
    node is mystery
scanData:
    nodetype is datanode
    data is not known
scanEdges:
    node edges are not known
scanNode:
    node is mystery







# crafting interface

firewalls
password entry
disconnecting / closing: how? when? escape key?
effect when revealing edges
disabled alarm graph icon should be more blatant

conditions a little weird still?
    i had a compromised node but with unknown edges- i wanted to scan it but couldn't because the node type was known.
    overall modifiers?
implement edges condition
deploy sounds - can be tied to icon 

 

skill tree
implement viruses