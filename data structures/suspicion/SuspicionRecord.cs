using System.Collections.Generic;
using System.Linq;
using Nimrod;
using UnityEngine;
[System.Serializable]
public class SuspicionDialogueParameters {
    public string challenge;
    public string pastTenseChallenge;
    public List<DialogueTactic> tactics;
}


[System.Serializable]
public class DialogueTactic {
    public DialogueTacticType tacticType;
    public string content;
    public string successResponse;
    public string failResponse;
    public Grammar grammar;
    public string getContent() {
        return grammar.Parse(content);
    }
    public string getSuccessResponse() {
        return grammar.Parse(successResponse);
    }
    public string getFailResponse() {
        return grammar.Parse(failResponse);
    }
}

public enum DialogueTacticType { none, lie, deny, bluff, redirect, challenge, item, escape }

public class SuspicionRecord {
    public string content;
    public string stickyContent;
    public float lifetime;
    public float maxLifetime;
    public Suspiciousness suspiciousness;
    public SuspicionDialogueParameters dialogue;
    public bool allowIDCardResponse;
    public bool allowDataResponse;
    public List<string> grammarFiles = new List<string>();
    public int challengeValue = (int)Toolbox.RandomGaussian(30, 80);

    public bool stickyable;
    public bool stickied;
    public bool stickiedThisFrame;

    public void Update(float deltaTime) {
        lifetime -= deltaTime;
    }
    public bool IsTimed() {
        return maxLifetime > 0;
    }
    public void MakeSticky() {
        if (!stickied)
            stickiedThisFrame = true;
        maxLifetime = -1f;
        lifetime = 1f;
        stickied = true;
        Debug.Log("making sticky suspicion record: " + content);
    }
    public DialogueTactic getResponse(DialogueTacticType tacticType) {
        DialogueTactic tactic;
        List<DialogueTactic> viableTactics = dialogue.tactics.Where(tactic => tactic.tacticType == tacticType).ToList();
        // TODO: cache the grammar somewhere
        Grammar grammar = new Grammar();
        grammar.Load("suspicion");
        foreach (string grammarFile in grammarFiles) {
            grammar.Load(grammarFile);
        }
        if (viableTactics.Count > 0) {
            tactic = Toolbox.RandomFromList(viableTactics);
        } else {
            List<DialogueTactic> viableRandomTactics = dialogue.tactics.Where(tactic => tactic.tacticType != DialogueTacticType.item).ToList();
            tactic = Toolbox.RandomFromList(viableRandomTactics);
            Debug.LogError($"missing tactic type {tacticType} for {content}");
        }
        tactic.grammar = grammar;
        return tactic;
    }

    public static SuspicionRecord identitySuspicion(DialogueInput input) {
        SuspicionRecord record = new SuspicionRecord {
            content = "suspicious identity",
            maxLifetime = 120,
            lifetime = 120,
            suspiciousness = Suspiciousness.suspicious,
            allowIDCardResponse = true,
            dialogue = new SuspicionDialogueParameters {
                challenge = "You there, stop! You're not authorized to be in this area! Show me your identification!",
                tactics = new List<DialogueTactic>{
                        new DialogueTactic {
                            tacticType = DialogueTacticType.lie,
                            content = "I am {fakename}, security inspector.",
                            successResponse = "Yes, that sounds right. Ok then.",
                            failResponse = "I don't think so."
                        },
                        new DialogueTactic {
                            tacticType = DialogueTacticType.deny,
                            content = "I am authorized to be here.",
                            successResponse = "Okay then.",
                            failResponse = "Clearly you aren't."
                        },
                        new DialogueTactic {
                            tacticType = DialogueTacticType.redirect,
                            content = "No time for that! Have you seen my cat? She's around here somewhere!",
                            successResponse = "What does she look like?",
                            failResponse = "I'll put you down as \"unidentified stranger\"."
                        },
                        new DialogueTactic {
                            tacticType = DialogueTacticType.challenge,
                            content = "How dare you! Do you have any idea who I am?",
                            successResponse = "My apologies, sir.",
                            failResponse = "That's what I'm trying to figure out, jackass."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.bluff,
                            content = "Rockwell isn't going to be very happy if you delay our meeting!",
                            successResponse = "I'm sorry, I didn't mean to intrude. Carry on.",
                            failResponse = "Rockwell, eh? Let's see what he has to say about it."
                        },
                        new DialogueTactic {
                            tacticType = DialogueTacticType.item,
                            content = "Sure, check my ID card.",
                            successResponse = "I see.",
                            failResponse = "Where did you get this?"
                        }
                }
            }
        };

        return record;
    }
    public static SuspicionRecord gunshotsHeard() => new SuspicionRecord {
        content = "gunshots reported",
        maxLifetime = 120,
        lifetime = 120,
        suspiciousness = Suspiciousness.suspicious,
        dialogue = new SuspicionDialogueParameters {
            challenge = "What do you know about those gunshots?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "Those weren't gunshots, that was a car backfiring.",
                            successResponse = "Oh, well then.",
                            failResponse = "I don't think so."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "I heard them! Why were you shooting at me?",
                            successResponse = "I wasn't shooting at you, don't be ridiculous.",
                            failResponse = "I wasn't, yet."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "I didn't hear any gunshots.",
                            successResponse = "Well, you're lucky you weren't around when the shooting happened.",
                            failResponse = "Really? They were pretty loud."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.bluff,
                            content = "I'm the tile inspector.",
                            successResponse = "I don't mean to get in your way, sir. Carry on.",
                            failResponse = "Tile inspector? Yeah right."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "I'm not lying on the ground, you're lying on the ground.",
                            successResponse = "I'm sorry, I guess I was confused.",
                            failResponse = "How stupid do you think I am?"
                        }
                    }
        }
    };

    public static SuspicionRecord crawlingSuspicion() => new SuspicionRecord {
        content = "crawling on the floor",
        suspiciousness = Suspiciousness.suspicious,
        allowDataResponse = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "What are you doing down there on the ground?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "I'm looking for my contact lens.",
                            successResponse = "You probably don't want it after it's been rolling around in the dirt.",
                            failResponse = "Contact lens, huh? Yeah right."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.bluff,
                            content = "I'm the tile inspector.",
                            successResponse = "I don't mean to get in your way, sir. Carry on.",
                            failResponse = "Tile inspector? Yeah right."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "I'm not lying on the ground.",
                            successResponse = "I'm sorry, I guess I was confused.",
                            failResponse = "How stupid do you think I am?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "I'm not lying on the ground, you're lying on the ground.",
                            successResponse = "I'm sorry, I guess I was confused.",
                            failResponse = "How stupid do you think I am?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "Come down here and I'll show you, prick!",
                            successResponse = "Hey, I meant no offense.",
                            failResponse = "Oh, Mr. toughguy over here!"
                        },
                        new DialogueTactic {
                            tacticType = DialogueTacticType.item,
                            content = "I'm {malename}, personal assistant to {name}, and they asked me to inspect this floor for bugs.",
                            successResponse = "Carry on.",
                            failResponse = "Let's  give him a call."
                        }
                    }
        }
    };

    public static SuspicionRecord c4Suspicion() => new SuspicionRecord {
        content = "seen placing infernal device",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 1f,
        maxLifetime = 1f,
        allowDataResponse = true,
        stickyable = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "What was that object you just placed?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "It was a flowerpot.",
                            successResponse = "I guess it does look like a flowerpot. Ok.",
                            failResponse = "How stupid do you think I am?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "I didn't put anything anywhere.",
                            successResponse = "I could have sworn I saw something. Huh.",
                            failResponse = "How stupid do you think I am?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "It was a rat trap. Didn't you see that giant rat that's running around?",
                            successResponse = "A rat? Where?",
                            failResponse = "That didn't look like a rat trap to me."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "Hey, a little privacy please!",
                            successResponse = "I'm sorry, I don't mean to intrude.",
                            failResponse = "It's my job to ask questions."
                        },
                        new DialogueTactic {
                            tacticType = DialogueTacticType.item,
                            content = "You were notified of contractors working on premises this week to install {installations}.",
                            successResponse = "Carry on.",
                            failResponse = "Something's not adding up here."
                        }
                    }
        }
    };

    public static SuspicionRecord robRegisterSuspicion() => new SuspicionRecord {
        content = "robbed a cash register",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 160f,
        maxLifetime = 160f,
        allowDataResponse = true,
        stickyable = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "Why did you rob the cash register?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "I thought it was a big pez dispenser!",
                            successResponse = "Well, don't do it again.",
                            failResponse = "How stupid do you think I am?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "I didn't rob the register.",
                            successResponse = "I could have sworn I saw something. Huh.",
                            failResponse = "How stupid do you think I am?"
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "Listen, did you hear the way the cashier was talking to me? Go arrest him!",
                            successResponse = "Paying customers deserve to be treated with respect.",
                            failResponse = "That is beside the point. You robbed him."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "Why, do you want a cut? No deal, pal!",
                            successResponse = "I do not take a \"cut\" of ill-gotten gains.",
                            failResponse = "I am insulted by the very suggestion."
                        },
                        new DialogueTactic {
                            tacticType = DialogueTacticType.item,
                            content = "Check with {name}, I'm the new bookkeeping depositor that started on Monday.",
                            successResponse = "Carry on.",
                            failResponse = "Something's not adding up here."
                        }
                    }
        }
    };

    public static SuspicionRecord bodySuspicion() => new SuspicionRecord {
        content = "A body was discovered",
        maxLifetime = 120,
        lifetime = 120,
        suspiciousness = Suspiciousness.suspicious,
        allowDataResponse = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "Do you know anything about the dead body that was found?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "A body? Oh my gosh, no.",
                            successResponse = "Well, be careful. We're not sure what's going on.",
                            failResponse = "Hmm. I think you know more than you're letting on."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "A dead body? Yeah right!",
                            successResponse = "It's true. We're trying to find the responsible party.",
                            failResponse = "Yes, a dead body. And right now you're the prime suspect."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "A dead body? Is that the most important thing right now?",
                            successResponse = "Well, now that you mention it...",
                            failResponse = "I would think so, yes."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "Dead body? How do I know you didn't kill this person?",
                            successResponse = "Obviously I had nothing to do with it.",
                            failResponse = "I don't know but you seem awfully suspicious to me."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = "{firstname} from accounting sent me to rectify the org chart. The body will be cleaned up by the end of the day.",
                            successResponse = "Tell Jan I said hi.",
                            failResponse = "You don't look like an accountant to me."
                        }
                    }
        }
    };

    public static SuspicionRecord fledSuspicion() => new SuspicionRecord() {
        content = "fled from questioning",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 60f,
        maxLifetime = 60f,
        allowDataResponse = true,
        stickyable = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "Why did you run away?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "I thought I saw my runaway dog.",
                            successResponse = "Oh, I didn't realize you were searching for your pet.",
                            failResponse = "That doesn't track."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "I didn't run away.",
                            successResponse = "That's odd. I wonder who I saw then.",
                            failResponse = "You absolutely did run away."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "No time for that now! There's some sort of terrible crocodile chasing me!",
                            successResponse = "What? Where!?",
                            failResponse = "Your lies could use some work."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "They came at me with a gun!",
                            successResponse = "Understandable. We just want to ask you some questions.",
                            failResponse = "You should expect that."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = "{name} will have my ass on a pike if I don't get him these {reports} on time!.",
                            successResponse = "Don't let me interfere.",
                            failResponse = "You don't look like an accountant to me."
                        }
                    }
        }
    };

    public static SuspicionRecord shotSuspicion() => new SuspicionRecord {
        content = "someone was shot",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 120f,
        maxLifetime = 120f,
        allowDataResponse = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "Someone was shot here. Do you know anything about that?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "No, how terrible!",
                            successResponse = "We're trying to find who did it.",
                            failResponse = "I think you were the shooter."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "Yes, I saw a guy run through here with a gun.",
                            successResponse = "Don't worry, we'll catch him.",
                            failResponse = "Why didn't you raise the alarm?"
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "Yes, it was probably the same person who tried to shoot me!",
                            successResponse = "Are you okay?",
                            failResponse = "I'm more worried about the person who actually was shot."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "Who has time to keep track of every little shooting that happens here?",
                            successResponse = "We realize you might be stressed in this difficult time.",
                            failResponse = "We will compel your cooperation."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = "I'm {fakename} with HR. I need to interview the person who was shot for insurance reasons.",
                            successResponse = "I'll let them know.",
                            failResponse = "I don't remember you being with HR..."
                        }
                    }
        }
    };

    public static SuspicionRecord noiseSuspicion() => new SuspicionRecord {
        content = "a suspicious noise was heard",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 10f,
        maxLifetime = 10f,
        allowDataResponse = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "Did you hear that strange sound?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "No, I don't pay attention to sounds.",
                            successResponse = "My apologies.",
                            failResponse = "I think you're up to something."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "Yeah, I thought it sounded like a malfunctioning maintenance bot.",
                            successResponse = "Oh, that makes sense.",
                            failResponse = "There are no maintenance bots on this floor."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "Look behind you, a three-headed monkey!",
                            successResponse = "Where?!",
                            failResponse = "You think I'd fall for that?"
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "I hear a lot of strange sounds coming from you! They sound like accusations!!",
                            successResponse = "We're just trying to get to the bottom of this.",
                            failResponse = "It's my job to ask questions."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = "You should know that contractor electricians will be working on-site for the next two months. That's probably what you heard.",
                            successResponse = "I'll watch out for them, thanks.",
                            failResponse = "It's my job to ask questions."
                        }
                    }
        }
    };

    public static SuspicionRecord explosionSuspicion() => new SuspicionRecord {
        content = "an explosion was heard",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 60f,
        maxLifetime = 60f,
        allowDataResponse = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "Did you hear that explosion?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "I didn't hear any explosion.",
                            successResponse = "Well, pay more attention, then. It was pretty loud.",
                            failResponse = "How could you not have heard that explosion?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "Yeah, I think there was a car crash!",
                            successResponse = "We'll check it out.",
                            failResponse = "If that was a car crash then I'm a three-headed monkey."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "I think a water main burst somewhere around here.",
                            successResponse = "I'll look into it.",
                            failResponse = "I didn't hear anything about a water main."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "I hear lots of things. What's it to you?",
                            successResponse = "I think we might be under attack.",
                            failResponse = "You're defensive, aren't you?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = "{name} from facilities sent me to investigate a boiler that's giving us trouble. That's probably what you heard.",
                            successResponse = "Okay, be careful then.",
                            failResponse = "It's my job to ask questions."
                        },
                    }
        }
    };

    public static SuspicionRecord brandishingSuspicion() => new SuspicionRecord {
        content = "brandishing weapon",
        suspiciousness = Suspiciousness.suspicious,
        allowDataResponse = true,
        stickyable = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "Why are you waving that gun around?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "I'm authorized to carry a weapon on this job.",
                            successResponse = "Make sure you clear it with central next time.",
                            failResponse = "I don't think so."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "This isn't a gun. It's a rotisserie spear.",
                            successResponse = "Oh, how odd. Okay then.",
                            failResponse = "You think I'm an idiot, don't you?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "You put your gun away and I'll put my gun away.",
                            successResponse = "No need. Carry on.",
                            failResponse = "How about I make you put your gun away?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "I don't feel safe around here.",
                            successResponse = "Let security handle it next time.",
                            failResponse = "I can see why."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = "I'm {fakename}, the new security intern. Check with {nane} in HR, they're still working on my firearm badge.",
                            successResponse = "Well just be careful then, you don't want to get in the middle of a real firefight.",
                            failResponse = "I think I'd know if there was a new security intern."
                        },
                    }
        }
    };
    public static SuspicionRecord trippedSensor(string sensorName) => new SuspicionRecord {
        content = "tripped an alarm sensor",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 3f,
        maxLifetime = 3f,
        allowDataResponse = true,
        stickyable = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = $"Do you realize you triggered a {sensorName} sensor?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = $"Of course. I'm the {sensorName} inspector. I was testing it.",
                            successResponse = "Carry on.",
                            failResponse = "You must take me for an idiot."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = $"Are you sure? I haven't been near any {sensorName}.",
                            successResponse = "",
                            failResponse = ""
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = $"I don't have time to pay attention to every little {sensorName} around here! Do you know how much I make?",
                            successResponse = "My apologies, sir.",
                            failResponse = "I'm not buying it, punk."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "That wasn't me, I saw a guy with a moustache running through here!",
                            successResponse = "Where?!",
                            failResponse = "You honestly think I'd fall for that?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = "Yes, I'm {fakename} from facilities, we've been getting reports about faulty sensors in this region. Just ask {firstname}.",
                            successResponse = "Okay, try to take care of it before morning then.",
                            failResponse = "These sensors were just inspected last week, something's not adding up here."
                        },
                    }
        }
    };
    public static SuspicionRecord shotsFiredSuspicion() => new SuspicionRecord {
        content = "shooting gun",
        suspiciousness = Suspiciousness.aggressive,
        maxLifetime = 1f,
        lifetime = 1f,
        stickyable = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = "Stand down! Why are you shooting!?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "I was trying to kill a large rat.",
                            successResponse = "Next time let security handle it.",
                            failResponse = "A likely story."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "I'm not shooting.",
                            successResponse = "Well who is shooting then?",
                            failResponse = "I think you were."
                        }, new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "I think you're jealous of my carefree, can-do attitude!",
                            successResponse = "I respect your right to carry firearms, sir.",
                            failResponse = "You are reckless!"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "You were shooting at me!",
                            successResponse = "A classic wild west mix-up, eh? Let's call it even.",
                            failResponse = "Yes, as well I should."
                        }
                    }
        }
    };

    public static SuspicionRecord tamperingSuspicion(BurgleTargetData data) => new SuspicionRecord {
        content = "tampering with equipment",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 3f,
        maxLifetime = 3f,
        allowDataResponse = true,
        stickyable = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = $"What are you doing with that {data.target.niceName}?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = $"I'm the {data.target.niceName} inspector.",
                            successResponse = "Carry on.",
                            failResponse = "Oh yeah? You don't look like one."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "I'm not doing anything.",
                            successResponse = "I could have sworn I saw something.",
                            failResponse = "You're up to something, I know it."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = "Listen, do you want to tell my boss that this job won't get finished?",
                            successResponse = "I don't mean to interfere.",
                            failResponse = "Get your boss on the phone and we'll talk."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "Look behind you, a three-headed monkey!",
                            successResponse = "Where?!",
                            failResponse = "You honestly think I'd fall for that?"
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = $"Yes, {{name}} in facilities has authorized me to conduct a full audit of the {data.target.niceName}.",
                            successResponse = "Okay, just be quick about it.",
                            failResponse = "You honestly think I'd fall for that?"
                        },
                    }
        }
    };
    public static SuspicionRecord tamperEvidenceSuspicion(string targetName) => new SuspicionRecord {
        content = "equipment was tampered with",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 120f,
        maxLifetime = 120f,
        allowDataResponse = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = $"Do you know anything about the {targetName} that was tampered with?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic {
                            tacticType = DialogueTacticType.lie,
                            content = $"I'm the {targetName} manager and I have seen nothing amiss.",
                            successResponse = "Understood.",
                            failResponse = "Oh yeah? You don't look like one."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = $"No clue.",
                            successResponse = "Roger that. Keep your eyes peeled.",
                            failResponse = "Are you sure about that?"
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = $"Yes, there is a rogue robot around here that is messing with the {targetName}.",
                            successResponse = "I'll tell central about it.",
                            failResponse = "That seems unlikely."
                        },
                        new DialogueTactic {
                            tacticType = DialogueTacticType.challenge,
                            content = $"What do I look like, the {targetName} inspector?",
                            successResponse = "Well, I have to ask.",
                            failResponse = $"You look like someone who might mess with a {targetName}."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = $"Yes, {{name}} has been on my butt all week to check that thing for wiretaps, I'll be done in a few hours.",
                            successResponse = "Okay, just be quick about it.",
                            failResponse = "You honestly think I'd fall for that?"
                        },
                    }
        }
    };

    public static SuspicionRecord lootSuspicion(string lootName) => new SuspicionRecord {
        content = "stealing things",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 2f,
        maxLifetime = 2f,
        allowDataResponse = true,
        stickyable = true,
        dialogue = new SuspicionDialogueParameters {
            challenge = $"What are you doing with that {lootName}?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = $"I'm the {lootName} inspector.",
                            successResponse = "Carry on.",
                            failResponse = "Oh yeah? You don't look like one."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "It's mine. I don't know what you're talking about.",
                            successResponse = "I'll have to double check my reports then.",
                            failResponse = "You seem awfully suspicious to me."
                        },
                         new DialogueTactic{
                            tacticType = DialogueTacticType.redirect,
                            content = "I bet you want it for yourself!",
                            successResponse = "I would never take possession of suspected stolen property.",
                            failResponse = "I resent the accusation."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.challenge,
                            content = $"What, is this your {lootName}?",
                            successResponse = "I don't mean to interfere.",
                            failResponse = "I know it isn't yours."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.item,
                            content = $"I guess you didn't get the memo that was sent on Friday, I'm here to inspect the {lootName}.",
                            successResponse = "Okay, just be quick about it.",
                            failResponse = "You honestly think I'd fall for that?"
                        },
                    }
        }
    };

    // public static SuspicionRecord tamperingSuspicion() => new SuspicionRecord {
    //     content = "tampering with equipment",
    //     suspiciousness = Suspiciousness.suspicious,
    //     lifetime = 3f,
    //     maxLifetime = 3f,
    //     stickyable = true,
    //     dialogue = new SuspicionDialogueParameters {
    //         challenge = $"",
    //         tactics = new List<DialogueTactic>{
    //                     new DialogueTactic{
    //                         tacticType = DialogueTacticType.lie,
    //                         content = $"",
    //                         successResponse = "",
    //                         failResponse = ""
    //                     },
    //                      new DialogueTactic{
    //                         tacticType = DialogueTacticType.deny,
    //                         content = "",
    //                         successResponse = "",
    //                         failResponse = ""
    //                     },
    //                     new DialogueTactic{
    //                         tacticType = DialogueTacticType.challenge,
    //                         content = "",
    //                         successResponse = "",
    //                         failResponse = ""
    //                     },
    //                     new DialogueTactic{
    //                         tacticType = DialogueTacticType.redirect,
    //                         content = "",
    //                         successResponse = "",
    //                         failResponse = ""
    //                     }
    //                 }
    //     }
    // };
}

