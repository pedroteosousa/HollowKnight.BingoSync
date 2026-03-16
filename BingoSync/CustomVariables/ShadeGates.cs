using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class ShadeGates
    {
        private static readonly string variableName = "shadeGatesHit";
        private static readonly string objectName = "Slash Effect";
        private static readonly string fsmName = "Control";
        private static readonly string hitStateName = "Pause";

        public static void CreateShadeGateTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasHitStateName = self.TryGetState(hitStateName, out FsmState hitState);
            if (self == null || self.FsmName != fsmName || !hasHitStateName) return;
            if (self.gameObject == null || !self.gameObject.name.StartsWith(objectName)) return;
            hitState.AddCustomAction(() => {
                string uniqueVariableName = $"hitShadeGate_{self.gameObject.scene.name}_{self.gameObject.GetPath()}";
                var alreadyHit = GoalCompletionTracker.GetBoolean(uniqueVariableName);
                if (alreadyHit)
                    return;
                GoalCompletionTracker.UpdateBoolean(uniqueVariableName, true);
                var shadeGatesHit = GoalCompletionTracker.GetInteger(variableName) + 1;
                GoalCompletionTracker.UpdateInteger(variableName, shadeGatesHit);
            });
        }
    }
}
