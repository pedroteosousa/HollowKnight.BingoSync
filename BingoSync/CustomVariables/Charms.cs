namespace BingoSync.CustomVariables
{
    internal static class Charms
    {
        public static string variablePrefix = "equippedCharm";
        public static string variableName = "charmsEquipped";

        public static bool CheckEquippedCharms(string name, bool orig)
        {
            if (!name.StartsWith(variablePrefix))
                return orig;

            var equippedCharms = PlayerData.instance.equippedCharms.Count;
            GoalCompletionTracker.UpdateInteger(variableName, equippedCharms);

            return orig;
        }
    }
}
