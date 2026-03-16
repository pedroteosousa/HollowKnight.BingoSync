using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class SpaGladiator
    {
        private static readonly string variableName = "spaGladiatorSplashed";
        private static readonly string objectName = "Spa Gladiator";
        private static readonly string fsmName = "Control";
        private static readonly string splashedStateName = "Splashed";

        public static void CreateSplashedTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasSplashedState = self.TryGetState(splashedStateName, out FsmState splashedState);
            if (self == null || self.FsmName != fsmName || !hasSplashedState) return;
            if (self.gameObject == null || self.gameObject.name != objectName) return;
            splashedState.AddCustomAction(() => {
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
