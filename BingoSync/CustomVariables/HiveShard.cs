using GlobalEnums;

namespace BingoSync.CustomVariables
{
    internal static class HiveShard
    {
        private static readonly string variableName = "collectedHiveShard";
        public static bool CheckIfHiveShardWasCollected(string name, bool orig)
        {
            if (name != nameof(PlayerData.instance.heartPieceCollected) || !orig)
                return orig;

            var zone = GameManager.instance.sm.mapZone;
            if (zone != MapZone.HIVE)
                return orig;

            GoalCompletionTracker.UpdateBoolean(variableName, true);
            return orig;
        }
    }
}
