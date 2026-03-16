using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class Switches
    {
        private static readonly string fsmName = "Switch Control";
        private static readonly string openStateName = "Open";

        public static void CreateSwitchOpenTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasOpenState = self.TryGetState(openStateName, out FsmState openState);
            if (self == null || self.FsmName != fsmName || !hasOpenState) return;
            openState.AddCustomAction(() => {
                string variableName = $"switchOpen_{GameManager.instance.GetSceneNameString()}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
