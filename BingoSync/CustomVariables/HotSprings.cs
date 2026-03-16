using BingoSync.Helpers;
using GlobalEnums;

namespace BingoSync.CustomVariables
{
    internal static class HotSprings
    {
        private static readonly int SpaEnvironmentType = 3;
        private static string GetHotSpringVariableName(MapZone zone)
        {
            return $"hotSpringBath_{zone}";
        }

        public static int CheckBath(string name, int orig)
        {
            if (name != nameof(PlayerData.instance.environmentType) || orig != SpaEnvironmentType)
            {
                return orig;
            }
            var zone = ZoneHelper.GreaterZone(GameManager.instance.sm.mapZone);
            var variableName = GetHotSpringVariableName(zone);
            GoalCompletionTracker.UpdateBoolean(variableName, true);
            return orig;
        }

    }
}
