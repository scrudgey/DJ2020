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




# ducking and sidechaining


The issue is that I'm finding it hard to balance the Background Music (bgm) with sound effects (sfx). It seems that one is always drowning out the other. I've separated the two onto their own channels in the mixer and I've taken also steps with the sfx to avoid clipping. Steps like: limiting the number of active sfx voices, ducking on the sfx channel, and even some normalization and compression. However, I'm having a lot of trouble keeping the bgm channel clear when a lot of sfx are playing at once and if I simply bump up the bgm volume then it almost certainly drowns out the sfx at some point, even when using normalisation.
Normally clarity would be achieved via ducking or side chain compression - I'm completely new to Unity so have no clue how you would implement this within Unity, but in the world of audio (my own background) the BGM level would be dropped - by at least 6dB - 12dB - every time there's a SFX hit, it really is as simple as that, the level drop should be rapid, with a pretty fast recovery too, you don't need to let the SFX finish before returning the BGM to its original level, psychoacoustically the attack of the SFX is much more important than the decay/sustain.

Sluggy said: ↑
I've started playing with the idea of using parametric EQ to sort of 'cut holes' in the sfx and bgm channels so that they don't blow out on any given frequency but I'm having limited success.

There really shouldn't be a need to 'pocket' the SFX to the BGM, this is something you might do on a static mix, but doing it on a dynamically changing mix means both the BGM and SFX are compromised, as they are EQ'd to allow other sounds to cut through, sounds that won't be playing 98% of the time.

Sluggy said: ↑
Or maybe just point me to a good beginner tutorial on mixing in a dynamic environment like video games?

Ducking/Sidechaning really is the secret here, you need to be more aggressive than you might think, I've suggested a 6dB - 12dB cut, but that's just as a minimum, you can often get away with near silence (on the BGM) for fractions of a second - without the BGM music appearing to lose its flow.





https://forum.unity.com/threads/duck-volume-sidechain-audio-from-another-mixer-in-the-hierarchy.352033/