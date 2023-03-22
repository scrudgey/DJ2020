using System.Collections;
using System.Collections.Generic;
using AI;
using Nimrod;
using UnityEngine;

namespace AI {

    public class TaskSocialize : TaskNode {
        WorldNPCAI ai;
        SpeechTextController speechTextController;
        SocialGroup socialGroup;
        float headSwivelOffset;
        Vector3 baseLookDirection;
        Grammar grammar;
        public TaskSocialize(WorldNPCAI ai, SpeechTextController speechTextController, SocialGroup socialGroup) : base() {
            this.ai = ai;
            this.socialGroup = socialGroup;
            this.speechTextController = speechTextController;
            baseLookDirection = ai.transform.forward;
            grammar = new Grammar();
            grammar.Load("socialize");
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            headSwivelOffset = 45f * Mathf.Sin(Time.time);

            Vector3 lookDirection = baseLookDirection;
            lookDirection = Quaternion.AngleAxis(headSwivelOffset, Vector3.up) * lookDirection;
            input.lookAtDirection = lookDirection;

            if (speechTextController.IsSpeaking()) {

            } else {
                if (socialGroup.currentSpeaker == ai) {
                    socialGroup.DeregisterSpeaker();
                } else {
                    bool shouldISpeak = socialGroup.ShouldISpeak(ai);
                    if (shouldISpeak) {
                        socialGroup.RegisterSpeaker(ai);
                        speechTextController.Say(GetPhrase());
                    }
                }
            }
            return TaskState.running;
        }

        string GetPhrase() {
            return grammar.Parse("{default}");
        }
    }
}