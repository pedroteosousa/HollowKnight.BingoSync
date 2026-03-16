namespace BingoSync.CustomVariables
{
    internal class FountainFragment
    {
        private static readonly string variableName = "fountainVesselFragmentCollected";
        private static readonly string fountainSceneName = "Abyss_04";
        private static readonly string fragmentVariableName = "vesselFragments";

        public static int CheckCollected(string name, int orig)
        {
            if (name == fragmentVariableName && GameManager.instance.GetSceneNameString() == fountainSceneName)
            {
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            }
            return orig;
        }
    }
}
