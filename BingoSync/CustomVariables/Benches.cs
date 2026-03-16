using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class Benches
    {
        private static readonly string fsmName = "Bench Control";
        private static readonly string startRestStateName = "Start Rest";
        private static readonly string restingStateName = "Init Resting";

        public static void CreateBenchTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasStartRestState = self.TryGetState(startRestStateName, out FsmState startRestState);
            if (self == null || self.FsmName != fsmName || !hasStartRestState) return;
            startRestState.AddCustomAction(() => {
                string variableName = $"bench_{GameManager.instance.GetSceneNameString()}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
            bool hasRestingState = self.TryGetState(restingStateName, out FsmState restingState);
            if (!hasRestingState) return;
            restingState.AddCustomAction(() => {
                string variableName = $"bench_{GameManager.instance.GetSceneNameString()}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
