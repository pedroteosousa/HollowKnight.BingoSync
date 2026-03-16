using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class Stag
    {
        private static readonly string objectName = "Stag";
        private static readonly string fsmName = "Stag Control";
        private static readonly string travelStateName = "Go To Stag Cutscene";

        public static void CreateStagTravelTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasTravelState = self.TryGetState(travelStateName, out FsmState travelState);
            if (self == null || self.FsmName != fsmName || !hasTravelState) return;
            if (self.gameObject == null || self.gameObject.name != objectName) return;
            travelState.AddCustomAction(() => {
                var targetPos = self.FsmVariables.GetVariable("To Position");
                if (targetPos == null) return;
                string variableName = $"stagTravelTo_{targetPos.ToInt()}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
