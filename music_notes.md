# music notes

how is music supposed to work?

at the level of level template, we define the music track to use.

start on level start, stop on level end.
on level end, play conclusion ditty.

# during level play

1. load all subtracks associated with the track
2. set audioclips on all players, play
3. set volumes 0-1 accordingly. initially, all 0 except subtrack 0.

now, we adjust volumes according to player status.

status: ignore
    track 0: vol 1
    track 1: vol 0
    track 2: vol 0
    track 3: vol 0

status: suspicious
    track 0: vol 1
    track 1: vol 1
    track 2: vol 0
    track 3: vol 0

status: aggressive
    track 0: vol 1
    track 1: vol 1
    track 2: vol 1
    track 3: vol 0

status: gunfight
    track 0: vol 1
    track 1: vol 1
    track 2: vol 1
    track 3: vol 1