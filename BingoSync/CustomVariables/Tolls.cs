using System;

namespace BingoSync.CustomVariables
{
    internal static class Tolls {
        private static readonly string variableName = "paidTolls";
        private static readonly string[] tollScenes = {"Crossroads_47", "Crossroads_49b", "Mines_33", "Fungus1_31", "Fungus1_16_alt", "Fungus2_02", "Ruins1_29", "Ruins1_31", "Ruins2_08", "Abyss_18", "Abyss_22", "Fungus3_40", "Fungus3_50", "Deepnest_09"};
        private static readonly string cityBenchTollSceneName = "Ruins1_31";
        private static readonly int cityBenchTollPrice = 150;
        public static void UpdateTolls(On.GeoCounter.orig_TakeGeo orig, GeoCounter self, int geo)
        {
            orig(self, geo);
            var sceneName = GameManager.instance.GetSceneNameString();
            var isTollScene = Array.IndexOf(tollScenes, sceneName) >= 0;
            if (!isTollScene) return;
            if (sceneName == cityBenchTollSceneName && geo != cityBenchTollPrice) return;
            var paidTolls = GoalCompletionTracker.GetInteger(variableName) + 1;
            GoalCompletionTracker.UpdateInteger(variableName, paidTolls);
        }
    }
}
