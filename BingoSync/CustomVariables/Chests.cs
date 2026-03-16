using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class Chests
    {
        private static readonly string fsmName = "Chest Control";
        private static readonly string openStateName = "Opened";

        public static void CreateChestOpenTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasOpenState = self.TryGetState(openStateName, out FsmState openState);
            if (self == null || self.FsmName != fsmName || !hasOpenState) return;
            openState.AddCustomAction(() => {
                string variableName = $"chestOpen_{GameManager.instance.GetSceneNameString()}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
