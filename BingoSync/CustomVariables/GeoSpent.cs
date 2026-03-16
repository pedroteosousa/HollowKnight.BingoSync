using System.Reflection;

namespace BingoSync.CustomVariables
{
    internal static class GeoSpent {
        private static readonly string variableName = "geoSpent";
        private static readonly string bankSceneName = "Fungus3_35";
        private static readonly FieldInfo counterCurrent = typeof(GeoCounter).GetField("counterCurrent", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void UpdateGeoText(On.GeoCounter.orig_Update orig, GeoCounter self)
        {
            orig(self);
            var geoSpent = GoalCompletionTracker.GetInteger(variableName);
            self.geoTextMesh.text = $"{counterCurrent.GetValue(self)} ({geoSpent} Spent)";
        }

        public static void UpdateGeoSpent(On.GeoCounter.orig_TakeGeo orig, GeoCounter self, int geo)
        {
            orig(self, geo);
            if (GameManager.instance.GetSceneNameString() == bankSceneName && PlayerData.instance.bankerAccountPurchased)
            {
                return;
            }
            var geoSpent = GoalCompletionTracker.GetInteger(variableName);
            GoalCompletionTracker.UpdateInteger(variableName, geoSpent + geo);
        }
    }
}
