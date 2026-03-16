using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class GiantGeoEgg
    {
        private static readonly string variableName = "destroyedGiantGeoEgg";
        private static readonly string objectName = "Giant Geo Egg";
        private static readonly string fsmName = "Geo Rock";
        private static readonly string destroyedStateName = "Destroy";

        public static void CreateGiantGeoRockTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasDestroyedState = self.TryGetState(destroyedStateName, out FsmState destroyedState);
            if (self == null || self.FsmName != fsmName || !hasDestroyedState) return;
            if (self.gameObject == null || self.gameObject.name != objectName) return;
            destroyedState.AddCustomAction(() => GoalCompletionTracker.UpdateBoolean(variableName, true));
        }
    }
}
