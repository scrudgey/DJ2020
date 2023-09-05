using System.Collections;
using System.Collections.Generic;
using AI;
using Nimrod;
using UnityEngine;

namespace AI {

    public class TaskSocialize : TaskNode {
        SpeechTextController speechTextController;
        SocialGroup socialGroup;
        float headSwivelOffset;
        Vector3 baseLookDirection;
        Grammar grammar;
        public TaskSocialize(SpeechTextController speechTextController, SocialGroup socialGroup) : base() {
            this.socialGroup = socialGroup;
            this.speechTextController = speechTextController;
            baseLookDirection = speechTextController.transform.forward;
            grammar = new Grammar();
            grammar.Load("socialize");
        }
        public override TaskState DoEvaluate(ref PlayerInput input) {
            headSwivelOffset = 45f * Mathf.Sin(Time.time);

            Vector3 lookDirection = baseLookDirection;
            lookDirection = Quaternion.AngleAxis(headSwivelOffset, Vector3.up) * lookDirection;
            input.lookAtDirection = lookDirection;

            if (speechTextController.IsSpeaking()) {

            } else if (socialGroup == null) {
                speechTextController.Say("hi");
            } else {
                if (socialGroup.currentSpeaker == speechTextController) {
                    socialGroup.DeregisterSpeaker();
                } else {
                    bool shouldISpeak = socialGroup.ShouldISpeak(speechTextController);
                    if (shouldISpeak) {
                        socialGroup.RegisterSpeaker(speechTextController);
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