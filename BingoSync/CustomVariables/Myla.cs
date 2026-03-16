using GlobalEnums;

namespace BingoSync.CustomVariables
{
    internal static class Myla
    {
        private static readonly string variableName = "killedMyla";
        public static int CheckIfMylaWasKilled(string name, int orig)
        {
            if (name != nameof(PlayerData.instance.killsZombieMiner))
                return orig;

            var zone = GameManager.instance.sm.mapZone;
            if (zone != MapZone.CROSSROADS)
                return orig;

            GoalCompletionTracker.UpdateBoolean(variableName, true);
            return orig;
        }
    }
}
