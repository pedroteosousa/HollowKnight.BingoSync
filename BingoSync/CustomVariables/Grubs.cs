using BingoSync.Helpers;
using GlobalEnums;

namespace BingoSync.CustomVariables
{
    internal static class Grubs
    {
        private static string GetZoneGrubsVariableName(MapZone zone)
        {
            return $"grubsSaved_{zone}";
        }

        public static int CheckIfGrubWasSaved(string name, int orig)
        {
            if (name == nameof(PlayerData.grubsCollected))
            {
                GrubSaved(GameManager.instance.sm.mapZone);
            }
            return orig;
        }

        private static void GrubSaved(MapZone zone)
        {
            zone = ZoneHelper.GreaterZone(zone);
            var variableName = GetZoneGrubsVariableName(zone);
            var grubsSaveOnZone = GoalCompletionTracker.GetInteger(variableName) + 1;
            GoalCompletionTracker.UpdateInteger(variableName, grubsSaveOnZone);
        }
    }
}
