using GlobalEnums;

namespace BingoSync.CustomVariables
{
    internal static class Jiji
    {
        private static readonly string variableName = "killedShadeJiji";
        public static bool CheckIfKilledShadeInJijis(string name, bool orig)
        {
            if (name != nameof(PlayerData.instance.soulLimited) || orig)
                return orig;

            var zone = GameManager.instance.sm.mapZone;
            if (zone != MapZone.TOWN)
                return orig;

            GoalCompletionTracker.UpdateBoolean(variableName, true);
            return orig;
        }
    }
}
