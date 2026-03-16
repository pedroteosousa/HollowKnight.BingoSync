using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    // This is needed because rando does not set the openedCityGate PlayerData variable
    internal static class CityGate
    {
        private static readonly string variableName = "openedCityGate";
        private static readonly string objectName = "City Gate Control";
        private static readonly string fsmName = "Conversation Control";
        private static readonly string openStateName = "Activate";

        public static void CreateCityGateOpenedTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasOpenState = self.TryGetState(openStateName, out FsmState openState);
            if (self == null || self.FsmName != fsmName || !hasOpenState) return;
            if (self.gameObject == null || self.gameObject.name != objectName) return;
            openState.AddCustomAction(() => {
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
