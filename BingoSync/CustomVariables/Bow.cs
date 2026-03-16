using System.Collections.Generic;
using UnityEngine;

namespace BingoSync.CustomVariables
{
    internal static class Bow
    {
        private static readonly List<BowInfo> BowInfos = new List<BowInfo>()
        {
            new BowInfo
            {
                BowRect = new Rect(55, 20, 20, 5),
                roomName = "Fungus2_30",
                variableSuffix = "FungalCoreElder",
            },
            new BowInfo
            {
                // Enough for Moss Prophet to be entirely on screen if you are facing him
                BowRect = new Rect(69, 4, 26, 5),
                roomName = "Fungus3_39",
                variableSuffix = "MossProphet",
            }
        };

        public static void BowToNPC()
        {
            bool isBowing = GameManager.instance?.hero_ctrl?.cState?.lookingDownAnim ?? false;
            if (!isBowing) return;
            var pos = GameManager.instance.hero_ctrl.transform.position;
            BowInfos.ForEach(info =>
            {
                if (GameManager.instance.GetSceneNameString() != info.roomName) return;
                if (!info.BowRect.Contains(pos)) return;
                var variableName = $"bow{info.variableSuffix}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }

    internal class BowInfo
    {
        public Rect BowRect;
        public string roomName;
        public string variableSuffix;
    }
}
