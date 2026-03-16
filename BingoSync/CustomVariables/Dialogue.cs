namespace BingoSync.CustomVariables
{
    internal static class Dialogue
    {
        public static void StartConversation(On.DialogueBox.orig_StartConversation orig, DialogueBox self, string convName, string sheetName)
        {
            orig(self, convName, sheetName);
            // Lemm with Crest
            if (convName == "RELICDEALER_DUNG") {
                GoalCompletionTracker.UpdateBoolean("metLemmWithCrest", true);
            }
            // Fluke Hermit
            if (convName.StartsWith("FLUKE_HERMIT")) {
                GoalCompletionTracker.UpdateBoolean("metFlukeHermit", true);
            }
            // Cornifer
            if (sheetName == "Cornifer") {
                var scene = GameManager.instance.GetSceneNameString();
                var variableName = $"cornifer_{scene}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            }
            // Mr Mushroom
            if (convName.StartsWith("MR_MUSHROOM")) {
                GoalCompletionTracker.UpdateBoolean("metMrMushroom", true);
            }
            // Hornet at Fountain
            if (convName.StartsWith("HORNET_FOUNTAIN"))
            {
                GoalCompletionTracker.UpdateBoolean("talkedToHornetAtCoTFountain", true);
            }
            // Hornet at Beast's Den
            if (convName.StartsWith("HORNET_SPIDER_TOWN")) {
                GoalCompletionTracker.UpdateBoolean("metHornetBeastsDen", true);
            }
            // Salubra overcharmed
            if (convName.StartsWith("CHARMSLUG") && PlayerData.instance.overcharmed) {
                GoalCompletionTracker.UpdateBoolean("talkedToSalubraOvercharmed", true);
            }
        }
    }
}
