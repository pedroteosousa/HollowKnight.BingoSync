using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class Marissa
    {
        private static readonly string variableName = "killedMarissa";
        private static readonly string roomName = "Ruins_Bathhouse";
        private static readonly string objectName = "Ghost NPC";
        private static readonly string fsmName = "ghost_npc_death";
        private static readonly string killedStateName = "Kill";

        public static void CreateMarissaKilledTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasKilledState = self.TryGetState(killedStateName, out FsmState killedState);
            if (self == null || self.FsmName != fsmName || !hasKilledState) return;
            if (self.gameObject == null || self.gameObject.name != objectName) return;
            if (self.gameObject.scene.name != roomName) return;
            killedState.AddCustomAction(() => GoalCompletionTracker.UpdateBoolean(variableName, true));
        }
    }
}
