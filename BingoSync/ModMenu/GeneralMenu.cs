using Satchel.BetterMenus;
using static BingoSync.Settings.ModSettings;

namespace BingoSync.ModMenu
{
    internal static class GeneralMenu
    {
        private static HorizontalOption revealCardOnStartSelector;
        private static HorizontalOption revealCardOnOthersRevealSelector;
        private static HorizontalOption markCompletedOnNewCardSelector;
        private static HorizontalOption markCompletedOnLoadSavefileSelector;
        private static HorizontalOption unmarkGoalsSelector;
        private static HorizontalOption itemSyncSelector;
        private static CustomSlider itemSyncDelay;

        private static Menu _TogglesMenu;

        public static MenuScreen CreateMenuScreen(MenuScreen parentMenu)
        {
            revealCardOnStartSelector = new HorizontalOption(
                name: "Reveal Card On Start",
                description: "Reveal Card when starting a new savefile",
                values: ["No", "Yes"],
                applySetting: (index) => Controller.GlobalSettings.RevealCardOnGameStart = (index == 1),
                loadSetting: () => Controller.GlobalSettings.RevealCardOnGameStart ? 1 : 0
            );

            revealCardOnOthersRevealSelector = new HorizontalOption(
                name: "Reveal With Others",
                description: "Reveal the card, when notified that another player did",
                values: ["No", "Yes"],
                applySetting: (index) => Controller.GlobalSettings.RevealCardWhenOthersReveal = (index == 1),
                loadSetting: () => Controller.GlobalSettings.RevealCardWhenOthersReveal ? 1 : 0
            );

            markCompletedOnNewCardSelector = new HorizontalOption(
                name: "Mark Goals On New Card",
                description: "Mark all completed goals when a new card is received/revealed",
                values: ["No", "Yes"],
                applySetting: (index) => Controller.GlobalSettings.MarkCompletedGoalsOnNewCardReceived = (index == 1),
                loadSetting: () => Controller.GlobalSettings.MarkCompletedGoalsOnNewCardReceived ? 1 : 0
            );

            markCompletedOnLoadSavefileSelector = new HorizontalOption(
                name: "Mark Goals On Load",
                description: "Mark all completed goals when loading a savefile",
                values: ["No", "Yes"],
                applySetting: (index) => Controller.GlobalSettings.MarkCompletedGoalsOnLoadSavefile = (index == 1),
                loadSetting: () => Controller.GlobalSettings.MarkCompletedGoalsOnLoadSavefile ? 1 : 0
            );

            unmarkGoalsSelector = new HorizontalOption(
                name: "Unmark Goals",
                description: "Some goals will be unmarked if their conditions are no longer met. WARNING: Can cause board inconsistencies on rare situations",
                values: ["No", "Yes"],
                applySetting: (index) => Controller.GlobalSettings.UnmarkGoals = (index == 1),
                loadSetting: () => Controller.GlobalSettings.UnmarkGoals ? 1 : 0
            );

            itemSyncSelector = new HorizontalOption(
                name: "Marks from ItemSync",
                description: "What to do when a goal gets completed by something received through ItemSync.",
                values: ["Mark", "Delay", "Ignore"],
                applySetting: (index) => Controller.GlobalSettings.ItemSyncMarkSetting = (ItemSyncMarkDelay)index,
                loadSetting: () => (int)Controller.GlobalSettings.ItemSyncMarkSetting
            );

            itemSyncDelay = new CustomSlider(
                    name: "ItemSync Delay (seconds)",
                    storeValue: value => Controller.GlobalSettings.ItemSyncMarkDelayMilliseconds = (int) (value * 1000),
                    loadValue: () => (float) Controller.GlobalSettings.ItemSyncMarkDelayMilliseconds / 1000,
                    minValue: 0f,
                    maxValue: 2.5f,
                    wholeNumbers: false
                );

            Element[] elements =
            [
                revealCardOnStartSelector,
                revealCardOnOthersRevealSelector,
                markCompletedOnNewCardSelector,
                markCompletedOnLoadSavefileSelector,
                unmarkGoalsSelector,
                itemSyncSelector,
                itemSyncDelay,
            ];

            _TogglesMenu = new Menu("BingoSync", elements);
            return _TogglesMenu.GetMenuScreen(parentMenu);
        }

        public static void RefreshMenu()
        {
            revealCardOnStartSelector?.LoadSetting();
            revealCardOnOthersRevealSelector?.LoadSetting();
            unmarkGoalsSelector?.LoadSetting();
            itemSyncSelector?.LoadSetting();
            itemSyncDelay?.LoadValue();
        }
    }
}
