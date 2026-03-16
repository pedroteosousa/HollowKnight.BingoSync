using Satchel;
using System.Collections;
using UnityEngine;

namespace BingoSync.CustomVariables
{
    internal static class Revek
    {
        private static readonly string variableName = "parryRevekConsecutive";
        private static readonly string revekScene = "RestingGrounds_08";

        public static IEnumerator EnterRoom(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            GoalCompletionTracker.UpdateInteger(variableName, 0);
            return orig(self, enterGate, delayBeforeEnter);
        }

        public static void CheckParry(On.HeroController.orig_NailParry orig, HeroController self)
        {
            orig(self);
            if (GameManager.instance.GetSceneNameString() != revekScene) {
                return;
            }

            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            if (objects == null)
            {
                return;
            }

            foreach (GameObject obj in objects)
            {
                if (obj.GetName() != "Hollow Shade(Clone)")
                {
                    continue;
                }
                foreach (PlayMakerFSM fsm in obj.GetComponents<PlayMakerFSM>())
                {
                    if (fsm.FsmName == "Shade Control" && fsm.ActiveStateName == "Slash")
                    {
                        return;
                    }
                }
            }

            var consecutiveParries = GoalCompletionTracker.GetInteger(variableName) + 1;
            GoalCompletionTracker.UpdateInteger(variableName, consecutiveParries);
        }
    }
}
