using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class DreamNailDialogue
    {
        private static readonly string fsmName = "npc_dream_dialogue";
        private static readonly string hitStateName = "Take Control";

        public static void CreateDreamNailDialogueTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasHitState = self.TryGetState(hitStateName, out FsmState hitState);
            if (self == null || self.FsmName != fsmName || self.gameObject == null || !hasHitState) return;
            hitState.AddCustomAction(() => {
                string variableName = $"dreamDialogue_{self.gameObject.scene.name}_{self.gameObject.name}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
