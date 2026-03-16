using System.Collections.Generic;
using HutongGames.PlayMaker;
using Satchel;

namespace BingoSync.CustomVariables
{
    internal static class LoreTablets
    {
        private static readonly List<string> fsmNames = new List<string>{ "inspect_region", "Inspection" };
        private static readonly string readStateName = "Take Control";

        public static void MarkLoreTabletAsRead(string roomName, string objectName)
        {
            string variableName = $"readLoreTablet_{roomName}_{objectName}";
            GoalCompletionTracker.UpdateBoolean(variableName, true);
        }

        public static void CreateLoreTabletTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            bool hasReadState = self.TryGetState(readStateName, out FsmState readState);
            if (self == null || !fsmNames.Contains(self.FsmName) || self.gameObject == null || !hasReadState) return;
            readState.AddCustomAction(() => {
                MarkLoreTabletAsRead(self.gameObject.scene.name, self.gameObject.name);
            });
        }
    }
}
