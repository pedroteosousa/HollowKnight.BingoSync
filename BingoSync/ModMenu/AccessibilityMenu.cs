using BingoSync.GameUI;
using Satchel.BetterMenus;

namespace BingoSync.ModMenu
{
    static class AccessibilityMenu
    {
        private static Menu _AccessibilityMenu;

        public static MenuScreen CreateMenuScreen(MenuScreen parentMenu)
        {
            int elementCount = 3;
            Element[] elements = new Element[elementCount];

            int elementId = 0;
            elements[elementId] = new HorizontalOption(
                name: "Color Scheme",
                description: "Different color schemes can be better for low opacity",
                values: ["Default", "Contrast", "High Contrast"],
                applySetting: (index) =>
                {
                    Controller.GlobalSettings.ColorScheme = index;
                    ConnectionMenuUI.UpdateColorScheme();
                    BingoBoardUI.UpdateColorScheme();
                    Controller.BoardUpdate();
                },
                loadSetting: () => Controller.GlobalSettings.ColorScheme
            );
            ++elementId;

            elements[elementId] = new HorizontalOption(
                name: "Color Icons",
                description: "Display small icons in addition to colors",
                values: ["Off", "On"],
                applySetting: (index) =>
                {
                    Controller.GlobalSettings.UseShapesForColors = index == 1;
                    Controller.BoardUpdate();
                },
                loadSetting: () => Controller.GlobalSettings.UseShapesForColors ? 1 : 0
            );
            ++elementId;

            elements[elementId] = new HorizontalOption(
                name: "Adapt Icon Opacity",
                description: "Small icons' opacity changes with the rest of the board",
                values: ["Off", "On"],
                applySetting: (index) =>
                {
                    Controller.GlobalSettings.AdaptIconOpcaity = index == 1;
                    Controller.RefreshBoardOpacity();
                    Controller.BoardUpdate();
                },
                loadSetting: () => Controller.GlobalSettings.AdaptIconOpcaity ? 1 : 0
            );
            ++elementId;

            _AccessibilityMenu = new Menu("BingoSync", elements);
            return _AccessibilityMenu.GetMenuScreen(parentMenu);
        }
    }
}
