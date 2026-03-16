using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class VoidPool
    {
        private static readonly string variableName = "voidPoolSwim";
        private static readonly string sceneNamePrefix = "Abyss";
        private static readonly string fsmName = "Surface Water Region";
        private static readonly string poolEnterStateName = "In";

        public static void CreateVoidPoolTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasPoolEnterState = self.TryGetState(poolEnterStateName, out FsmState poolEnterState);
            if (self == null || self.FsmName != fsmName || !hasPoolEnterState) return;
            if (self.gameObject == null || !self.gameObject.scene.name.StartsWith(sceneNamePrefix)) return;
            poolEnterState.AddCustomAction(() => {
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
