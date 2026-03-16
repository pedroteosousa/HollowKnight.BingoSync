using Satchel;
using HutongGames.PlayMaker;

namespace BingoSync.CustomVariables
{
    internal static class BreakableFloors
    {
        private static readonly string variableName = "floorsBroken";
        private static readonly string fsmName = "quake_floor";
        private static readonly string breakStateName = "Destroy";

        public static void CreateBreakableFloorsTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasBreakState = self.TryGetState(breakStateName, out FsmState breakState);
            if (self == null || self.FsmName != fsmName || self.gameObject == null || !hasBreakState) return;
            breakState.AddCustomAction(() => {
                var floorsBroken = GoalCompletionTracker.GetInteger(variableName);
                GoalCompletionTracker.UpdateInteger(variableName, floorsBroken + 1);
            });
        }
    }
}
