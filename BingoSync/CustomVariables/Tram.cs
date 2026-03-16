namespace BingoSync.CustomVariables
{
    internal static class Tram
    {
        private static readonly string lowerTramRoom = "Room_Tram";
        private static readonly string upperTramRoom = "Room_Tram_RG";

        public static int CheckIfStationWasVisited(string name, int orig)
        {
            var scene = GameManager.instance.GetSceneNameString();
            var lowerTramMoving = (name == nameof(PlayerData.instance.tramLowerPosition) && scene == lowerTramRoom);
            var upperTramMoving = (name == nameof(PlayerData.instance.tramRestingGroundsPosition) && scene == upperTramRoom);
            if (!lowerTramMoving && !upperTramMoving)
            {
                return orig;
            }
            var prev = PlayerData.instance.GetInt(name);
            MarkTramVisited(scene, prev);
            MarkTramVisited(scene, orig);
            return orig;
        }

        private static void MarkTramVisited(string scene, int location) {
            var variableName = $"visited_{scene}_{location}";
            GoalCompletionTracker.UpdateBoolean(variableName, true);
        }
    }
}
