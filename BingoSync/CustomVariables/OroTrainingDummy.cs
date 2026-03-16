using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class OroTrainingDummy
    {
        private static readonly string variableName = "oroTrainingDummyTriggered";
        private static readonly string objectName = "Training Dummy";
        private static readonly string fsmName = "Hit";
        private static readonly string summonStateName = "Summon?";

        public static void CreateOroTrainingDummyTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasSummonState = self.TryGetState(summonStateName, out FsmState summonState);
            if (self == null || self.FsmName != fsmName || !hasSummonState) return;
            if (self.gameObject == null || self.gameObject.name != objectName) return;
            summonState.AddCustomAction(() => {
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
