using BingoSync.CustomGoals;
using Modding.Converters;
using Newtonsoft.Json;
using System.Collections.Generic;
using BingoSync.GameUI;

namespace BingoSync.Settings
{
    public class ModSettings
    {
        public enum HighlightType
        {
            Border,
            Star,
        }
        public enum ItemSyncMarkDelay
        {
            None = 0,
            Delay = 1,
            NoMark = 2,
        }

        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds Keybinds = new();
        public bool RevealCardOnGameStart = false;
        public bool RevealCardWhenOthersReveal = false;
        public bool UnmarkGoals = false;
        public string DefaultNickname = "";
        public string DefaultPassword = "";
        public string DefaultColor = "red";
        public bool DebugMode = false;

        public int BoardAlphaIndex = 0;
        public List<float> BoardAlphas = [0.135f, 0.5f, 1f];
        public float BoardAlpha {
            get
            {
                return BoardAlphas[BoardAlphaIndex];
            }
            set {}
        }
        public HighlightType SelectedHighlightSprite = HighlightType.Border;

        public AudioNotificationCondition AudioNotificationOn = AudioNotificationCondition.None;
        public int AudioClipId = 0;
        public float AudioClipVolume = 1.0f;

        public int ColorScheme = 0;
        public bool UseShapesForColors = false;
        public bool AdaptIconOpcaity = false;

        public ItemSyncMarkDelay ItemSyncMarkSetting = ItemSyncMarkDelay.Delay;
        public int ItemSyncMarkDelayMilliseconds = 1000;

        public bool MarkCompletedGoalsOnLoadSavefile = true;
        public bool MarkCompletedGoalsOnNewCardReceived = true;
    }
}
