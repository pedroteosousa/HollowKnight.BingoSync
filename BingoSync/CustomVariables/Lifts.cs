namespace BingoSync.CustomVariables
{
    internal static class Lifts
    {
        private static readonly string[] liftRoomPrefixes = {"Crossroads_49", "Ruins2_10"};

        public static bool CheckIfLiftWasUsed(string name, bool orig)
        {
            if (name != nameof(PlayerData.instance.liftArrival))
                return orig;

            var scene = GameManager.instance.GetSceneNameString();
            for (int i = 0; i < liftRoomPrefixes.Length; i++) {
                if (!scene.StartsWith(liftRoomPrefixes[i])) continue;
                string variableName = $"usedLift{i + 1}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            }

            return orig;
        }
    }
}
