using System.Collections.Generic;

[System.Serializable]
public class SuspicionDialogueParameters {
    public string challenge;
    public List<DialogueTactic> tactics;
}


[System.Serializable]
public class DialogueTactic {
    public DialogueTacticType tacticType;
    public string content;
    public string successResponse;
    public string failResponse;
}

public enum DialogueTacticType { none, lie, deny, bluff, redirect, challenge, item, escape }

public class SuspicionRecord {
    public string content;
    public float lifetime;
    public float maxLifetime;
    public Suspiciousness suspiciousness;
    public SuspicionDialogueParameters dialogue;
    public void Update(float deltaTime) {
        lifetime -= deltaTime;
    }
    public bool IsTimed() {
        return maxLifetime > 0;
    }

    public static SuspicionRecord identitySuspicion(DialogueInput input) {
        SuspicionRecord record = new SuspicionRecord {
            content = "suspicious identity",
            maxLifetime = 120,
            lifetime = 120,
            suspiciousness = Suspiciousness.suspicious,
            dialogue = new SuspicionDialogueParameters {
                challenge = "You there, stop! You're not authorized to be in this area! Show me your identification!",
                tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = "I am P.J. Pennypacker, security inspector.",
                            successResponse = "Yes, that sounds right. Ok then.",
                            failResponse = "I don't think so."
                        },
                        new DialogueTactic{
                            tacticType = DialogueTacticType.bluff,
                            content = "Rockwell isn't going to be very happy if you delay our meeting!",
                            successResponse = "I'm sorry, I didn't mean to intrude. Carry on.",
                            failResponse = "Rockwell, eh? Let's see what he has to say about it."
                        },

                    }
            }
        };

        if (input.playerHasID) {
            record.dialogue.tactics.Add(
                new DialogueTactic {
                    tacticType = DialogueTacticType.item,
                    content = "Sure, check my ID card.",
                    successResponse = "I see.",
                    failResponse = "Where did you get this?"
                });
        }
        // TODO: more checks

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
                        }
                    }
        }
    };

    public static SuspicionRecord crawlingSuspicion() => new SuspicionRecord {
        content = "crawling on the floor",
        suspiciousness = Suspiciousness.suspicious,
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
                        }
                    }
        }
    };

    public static SuspicionRecord c4Suspicion() => new SuspicionRecord {
        content = "seen placing infernal device",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 1f,
        maxLifetime = 1f,
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
                        }
                    }
        }
    };

    public static SuspicionRecord robRegisterSuspicion() => new SuspicionRecord {
        content = "robbed a cash register",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 160f,
        maxLifetime = 160f,
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
                        }
                    }
        }
    };

    public static SuspicionRecord bodySuspicion() => new SuspicionRecord {
        content = "A body was discovered",
        maxLifetime = 120,
        lifetime = 120,
        suspiciousness = Suspiciousness.suspicious,
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
                            tacticType = DialogueTacticType.challenge,
                            content = "Body? How do I know you didn't kill this person?",
                            successResponse = "Obviously I had nothing to do with it.",
                            failResponse = "I don't know but you seem awfully suspicious to me."
                        }
                    }
        }
    };

    public static SuspicionRecord fledSuspicion() => new SuspicionRecord() {
        content = "fled from questioning",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 60f,
        maxLifetime = 60f,
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
                            tacticType = DialogueTacticType.challenge,
                            content = "They came at me with a gun!",
                            successResponse = "Understandable. We just want to ask you some questions.",
                            failResponse = "You should expect that."
                        }
                    }
        }
    };

    public static SuspicionRecord shotSuspicion() => new SuspicionRecord {
        content = "someone was shot",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 120f,
        maxLifetime = 120f,
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
                            tacticType = DialogueTacticType.challenge,
                            content = "Who has time to keep track of every little shooting that happens here?",
                            successResponse = "We realize you might be stressed in this difficult time.",
                            failResponse = "We will compel your cooperation."
                        }
                    }
        }
    };

    public static SuspicionRecord noiseSuspicion() => new SuspicionRecord {
        content = "a suspicious noise was heard",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 10f,
        maxLifetime = 10f,
        dialogue = new SuspicionDialogueParameters {
            challenge = "Did you hear that strange sound?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.deny,
                            content = "No, I don't pay attention to sounds.",
                            successResponse = "Apologies.",
                            failResponse = "I think you were the shooter."
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
                        }
                    }
        }
    };

    public static SuspicionRecord explosionSuspicion() => new SuspicionRecord {
        content = "an explosion was heard",
        suspiciousness = Suspiciousness.aggressive,
        lifetime = 60f,
        maxLifetime = 60f,
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
                        }
                    }
        }
    };

    public static SuspicionRecord brandishingSuspicion() => new SuspicionRecord {
        content = "brandishing weapon",
        suspiciousness = Suspiciousness.suspicious,
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
                    }
        }
    };
    public static SuspicionRecord trippedSensor(string sensorName) => new SuspicionRecord {
        content = "tripped an alarm sensor",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 3f,
        maxLifetime = 3f,
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
                        }
                    }
        }
    };
    public static SuspicionRecord shotsFiredSuspicion() => new SuspicionRecord {
        content = "shooting gun",
        suspiciousness = Suspiciousness.aggressive,
        maxLifetime = 1f,
        lifetime = 1f,
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
                            tacticType = DialogueTacticType.redirect,
                            content = "You were shooting at me!",
                            successResponse = "A classic wild west mix-up, eh? Let's call it even.",
                            failResponse = "Yes, as well I should."
                        },
                    }
        }
    };

    public static SuspicionRecord tamperingSuspicion(BurgleTargetData data) => new SuspicionRecord {
        content = "tampering with equipment",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 3f,
        maxLifetime = 3f,
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
                        }
                    }
        }
    };
    public static SuspicionRecord tamperEvidenceSuspicion(TamperEvidence evidence) => new SuspicionRecord {
        content = "equipment was tampered with",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 120f,
        maxLifetime = 120f,
        dialogue = new SuspicionDialogueParameters {
            challenge = $"Do you know anything about the {evidence.targetName} that was tampered with?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic {
                            tacticType = DialogueTacticType.lie,
                            content = $"No clue.",
                            successResponse = "Understood.",
                            failResponse = "Oh yeah? You don't look like one."
                        },
                        new DialogueTactic {
                            tacticType = DialogueTacticType.challenge,
                            content = $"What do I look like, the {evidence.targetName} inspector?",
                            successResponse = "Well, I have to ask.",
                            failResponse = $"You look like someone who might mess with a {evidence.targetName}."
                        }
                    }
        }
    };

    public static SuspicionRecord lootSuspicion(string lootName) => new SuspicionRecord {
        content = "stealing things",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 2f,
        maxLifetime = 2f,
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
                            tacticType = DialogueTacticType.challenge,
                            content = $"What, is this your {lootName}?",
                            successResponse = "I don't mean to interfere.",
                            failResponse = "I know it isn't yours."
                        }
                    }
        }
    };

    public static SuspicionRecord tamperingSuspicion(HackData data) => new SuspicionRecord {
        content = "tampering with equipment",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 3f,
        maxLifetime = 3f,
        dialogue = new SuspicionDialogueParameters {
            challenge = $"What are you doing with that {data.node.nodeTitle}?",
            tactics = new List<DialogueTactic>{
                        new DialogueTactic{
                            tacticType = DialogueTacticType.lie,
                            content = $"I'm the {data.node.nodeTitle} inspector.",
                            successResponse = "Carry on.",
                            failResponse = "Oh yeah? You don't look like one."
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
                            failResponse = "You honestly ,think I'd fall for that?"
                        }
                    }
        }
    };
    public static SuspicionRecord tamperingSuspicion() => new SuspicionRecord {
        content = "tampering with equipment",
        suspiciousness = Suspiciousness.suspicious,
        lifetime = 3f,
        maxLifetime = 3f,

    };
}

