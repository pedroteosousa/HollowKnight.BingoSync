using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class Shinies
    {
        private static readonly string fsmName = "Shiny Control";
        private static readonly string trinketStateName = "Trink Flash";

        private static string GetVariableName(int trinketNum)
        {
            var roomName = GameManager.instance.GetSceneNameString();
            return $"gotShiny_{trinketNum}_{roomName}";
        }

        public static void CreateTrinketTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasTrinketState = self.TryGetState(trinketStateName, out FsmState trinketState);
            if (self == null || self.FsmName != fsmName || !hasTrinketState) return;
            trinketState.AddCustomAction(() =>
            {
                var trinketNum = self.FsmVariables.GetFsmInt("Trinket Num").Value;
                var variableName = GetVariableName(trinketNum);
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }
}
