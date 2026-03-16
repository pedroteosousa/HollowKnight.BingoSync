using MagicUI.Core;
using System;

namespace BingoSync.GameUI
{
    internal static class MenuUI
    {
        private static Action<string> Log;

        public static readonly int gapWidth = 10;
        public static readonly int colorButtonWidth = 100;
        public static readonly int lockoutButtonWidth = 50;
        public static readonly int handModeButtonWidth = 43;
        // public static readonly int resetConnectionButtonWidth = 43;
        public static readonly int textFieldWidth = colorButtonWidth * 5 + gapWidth * 4;
        public static readonly int joinRoomButtonWidth = textFieldWidth - handModeButtonWidth - gapWidth;
        public static readonly int gameModeButtonWidth = (textFieldWidth - gapWidth * 2) / 3;
        public static readonly int generateButtonWidth = (int)(1.5 * gameModeButtonWidth) + gapWidth;
        public static readonly int seedFieldWidth = textFieldWidth - generateButtonWidth - lockoutButtonWidth - gapWidth * 2;
        public static readonly int profileScreenArrowButtonWidth = 35;
        public static readonly int acceptProfileNameButtonWidth = 120;
        public static readonly int profileNameFieldWidth = textFieldWidth - 2*profileScreenArrowButtonWidth - acceptProfileNameButtonWidth - gapWidth * 3;
        public static readonly int fontSize = 22;

        public static bool HandMode {
            get
            {
                return ConnectionMenuUI.HandMode;
            }
            set
            {
                ConnectionMenuUI.HandMode = value;
            }
        }

        private static readonly LayoutRoot layoutRoot = new(true, "Persistent layout")
        {
            // this is needed to hide the menu while the game is loading
            VisibilityCondition = () => false,
        };

        public static void Setup(Action<string> log)
        {
            Log = log;

            ConnectionMenuUI.Setup(Log, layoutRoot);
            GenerationMenuUI.Setup(Log);


            layoutRoot.VisibilityCondition = () => {
                Controller.ActiveSession.Update();
                ConnectionMenuUI.Update();
                return Controller.MenuIsVisible;
            };

            layoutRoot.ListenForPlayerAction(Controller.GlobalSettings.Keybinds.HideMenu, () => {
                if (GenerationMenuUI.TextBoxActive || ConnectionMenuUI.TextBoxActive)
                {
                    return;
                }
                Controller.MenuIsVisible = !Controller.MenuIsVisible;
            });
        }

        public static void LoadDefaults()
        {
            ConnectionMenuUI.LoadDefaults();
        }

        public static bool IsLockout()
        {
            return GenerationMenuUI.LockoutToggleButtonIsOn();
        }

        public static (int, bool) GetSeed()
        { 
            return GenerationMenuUI.GetSeed();
        }
        public static void SetupGameModeButtons()
        {
            GenerationMenuUI.SetupGameModeButtons();
        }
    }
}