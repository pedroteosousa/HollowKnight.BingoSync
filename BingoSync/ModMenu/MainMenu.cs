using Modding.Menu;
using Modding.Menu.Config;
using UnityEngine.UI;

namespace BingoSync.ModMenu
{
    internal static class MainMenu
    {
        private static MenuScreen _MainMenuScreen;
        private static MenuScreen _KeybindsScreen;
        private static MenuScreen _GeneralScreen;
        private static MenuScreen _DefaultsScreen;
        private static MenuScreen _BoardSettingsScreen;
        private static MenuScreen _ProfilesScreen;
        private static MenuScreen _AccessibilityScreen;

        public static MenuScreen CreateMenuScreen(MenuScreen parentMenu) {
            void ExitMenu(MenuSelectable _) => UIManager.instance.UIGoToDynamicMenu(parentMenu);
            void GoToKeybinds(MenuSelectable _) => UIManager.instance.UIGoToDynamicMenu(_KeybindsScreen);
            void GoToGeneral(MenuSelectable _) => UIManager.instance.UIGoToDynamicMenu(_GeneralScreen);
            void GoToDefaults(MenuSelectable _) => UIManager.instance.UIGoToDynamicMenu(_DefaultsScreen);
            void GoToBoardSettings(MenuSelectable _) => UIManager.instance.UIGoToDynamicMenu(_BoardSettingsScreen);
            void GoToProfiles(MenuSelectable _) {
                ProfilesManagementMenu.RefreshMenu();
                UIManager.instance.UIGoToDynamicMenu(_ProfilesScreen);
            };
            void GoToAccessibility(MenuSelectable _) => UIManager.instance.UIGoToDynamicMenu(_AccessibilityScreen);

            MenuBuilder mainMenuBuilder = MenuUtils.CreateMenuBuilderWithBackButton("BingoSync", parentMenu, out _);

            mainMenuBuilder.AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c =>
                    {
                        c.AddMenuButton("General", new MenuButtonConfig
                        {
                            Label = "General",
                            Proceed = true,
                            SubmitAction = GoToGeneral,
                            CancelAction = ExitMenu,
                        })
                        .AddMenuButton("Keybinds", new MenuButtonConfig
                        {
                            Label = "Keybinds",
                            Proceed = true,
                            SubmitAction = GoToKeybinds,
                            CancelAction = ExitMenu,
                        })
                        .AddMenuButton("Defaults", new MenuButtonConfig
                        {
                            Label = "Defaults",
                            Proceed = true,
                            SubmitAction = GoToDefaults,
                            CancelAction = ExitMenu,
                        })
                        .AddMenuButton("Board Settings", new MenuButtonConfig
                        {
                            Label = "Board Settings",
                            Proceed = true,
                            SubmitAction = GoToBoardSettings,
                            CancelAction = ExitMenu,
                        })
                        .AddMenuButton("Profiles", new MenuButtonConfig
                        {
                            Label = "Profiles",
                            Proceed = true,
                            SubmitAction = GoToProfiles,
                            CancelAction = ExitMenu,
                        })
                        .AddMenuButton("Accessibility", new MenuButtonConfig
                        {
                            Label = "Accessibility",
                            Proceed = true,
                            SubmitAction = GoToAccessibility,
                            CancelAction = ExitMenu,
                        })
                        .AddStaticPanel("Spacer", new RelVector2(new RelLength(1), new RelLength(1)), out _)
                        .AddMenuButton("Reset Active Connection", new MenuButtonConfig
                        {
                            Style = new MenuButtonStyle()
                            {
                                TextSize = 30,
                                Height = new RelLength(40f),
                            },
                            Label = "Reset Active Connection",
                            Proceed = false,
                            SubmitAction = ResetConnectionButtonClicked,
                            CancelAction = ExitMenu,
                        });
                    });

            _MainMenuScreen = mainMenuBuilder.Build();

            _KeybindsScreen = KeybindsMenu.CreateMenuScreen(_MainMenuScreen);
            _GeneralScreen = GeneralMenu.CreateMenuScreen(_MainMenuScreen);
            _DefaultsScreen = DefaultsMenu.CreateMenuScreen(_MainMenuScreen);
            _ProfilesScreen = ProfilesManagementMenu.CreateMenuScreen(_MainMenuScreen);
            _BoardSettingsScreen = BoardSettingsMenu.CreateMenuScreen(_MainMenuScreen);
            _AccessibilityScreen = AccessibilityMenu.CreateMenuScreen(_MainMenuScreen);

            return _MainMenuScreen;
        }

        public static void RefreshMenu()
        {
            KeybindsMenu.RefreshMenu();
            GeneralMenu.RefreshMenu();
            DefaultsMenu.RefreshMenu();
            ProfilesManagementMenu.RefreshMenu();
            BoardSettingsMenu.RefreshMenu();
        }

        public static void ResetConnectionButtonClicked(MenuButton _)
        {
            Controller.ResetConnectionButtonClicked();
        }
    }
}