using System.Collections;
using GlobalEnums;

namespace BingoSync.CustomVariables
{
    internal static class Scenes
    {
        private static readonly string overgrownMoundRoomName = "Room_Fungus_Shaman";
        private static readonly string lifebloodCoreRoomName = "Abyss_08";

        public static IEnumerator EnterRoom(On.HeroController.orig_EnterScene orig, HeroController self, TransitionPoint enterGate, float delayBeforeEnter)
        {
            var zone = GameManager.instance.GetCurrentMapZone();
            var room = GameManager.instance.GetSceneNameString();

            // Overgrown Mound normally counts as royal gardens, which makes no sense.
            if (room == overgrownMoundRoomName)
            {
                zone = MapZone.OVERGROWN_MOUND.ToString();
            }

            CheckIfInLifebloodCoreRoom(room);

            string zoneVisitedVariableName = $"zoneVisited_{zone}";
            GoalCompletionTracker.UpdateBoolean(zoneVisitedVariableName, true);
            string roomVisitedVariableName = $"roomVisited_{room}";
            GoalCompletionTracker.UpdateBoolean(roomVisitedVariableName, true);
            return orig(self, enterGate, delayBeforeEnter);
        }

        public static void CheckIfInLifebloodCoreRoom(string roomName)
        {
            var variableName = "inLifebloodCoreRoom";
            GoalCompletionTracker.UpdateBoolean(variableName, roomName == lifebloodCoreRoomName);
        }
    }
}
