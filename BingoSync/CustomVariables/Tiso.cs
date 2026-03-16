using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class Tiso
    {
        private static readonly string variableName = "tisoShieldHit";
        private static readonly string objectName = "Tiso Shield Bone";
        private static readonly string fsmName = "Head Control";
        private static readonly string hitStateName = "Hit Effects";

        public static void CreateTisoShieldTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasHitState = self.TryGetState(hitStateName, out FsmState hitState);
            if (self == null || self.FsmName != fsmName || !hasHitState) return;
            if (self.gameObject == null || self.gameObject.name != objectName) return;
            hitState.AddCustomAction(() => {
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
