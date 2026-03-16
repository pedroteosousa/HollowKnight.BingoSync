using System.Collections.Generic;

namespace BingoSync.CustomVariables
{
    internal static class NailArts
    {
        public static List<string> nailArtObjectNames = new List<string> { "Great Slash", "Dash Slash", "Cyclone Slash" };

        public static void CreateNailArtsTrigger(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
        {
            orig(self);
            if (self == null || self.gameObject == null || !nailArtObjectNames.Contains(self.gameObject.name)) return;
            var variableName = $"nailArtUsed_{self.gameObject.name}_{GameManager.instance.GetSceneNameString()}";
            GoalCompletionTracker.UpdateBoolean(variableName, true);
        }
    }
}
