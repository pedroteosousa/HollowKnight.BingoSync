using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class Telescope
    {
        private static readonly string variableName = "telescopeInteract";
        private static readonly string objectName = "Telescope Inspect";
        private static readonly string fsmName = "Conversation Control";
        private static readonly string interactStateName = "Fade";

        public static void CreateTelescopeTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasInteractState = self.TryGetState(interactStateName, out FsmState interactState);
            if (self == null || self.FsmName != fsmName || !hasInteractState) return;
            if (self.gameObject == null || self.gameObject.name != objectName) return;
            interactState.AddCustomAction(() => {
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
