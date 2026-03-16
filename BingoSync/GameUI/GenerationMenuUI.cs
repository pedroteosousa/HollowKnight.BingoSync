using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BingoSync.CustomGoals;

namespace BingoSync.GameUI
{
    static class GenerationMenuUI
    {
        private static Action<string> Log;
        private static readonly TextureLoader Loader = new(Assembly.GetExecutingAssembly(), "BingoSync.Resources.Images");

        private static LayoutRoot layoutRoot;
        private static StackLayout generationMenu;

        private static TextInput profileNameInput;
        private static Button acceptProfileNameButton;
        private static int currentProfileScreen = 0;
        private static int _profilesPerScreen = 15;
        public static bool BottomBarShown
        {
            get
            {
                return _profilesPerScreen == 15;
            }
            set
            {
                _profilesPerScreen = value ? 12 : 15;
                Controller.RegenerateGameModeButtons();
            }
        }
        private static Button previousProfileButton;
        private static Button nextProfileButton;

        private static TextInput generationSeedInput;
        private static Button generateBoardButton;
        private static readonly List<Button> gameModeButtons = [];
        private static ToggleButton lockoutToggleButton;

        public static bool TextBoxActive { get; private set; } = false;

        public static void Setup(Action<string> log)
        {
            Log = log;
            Loader.Preload();
            SharedUIPage page = SharedUIManager.RequestUIPage("BingoSync");
            layoutRoot = page.Root;
            CreateGenerationMenu();
            page.AddContent(generationMenu);
        }

        public static void CreateGenerationMenu()
        {
            bool lockoutButtonIsLockout = lockoutToggleButton?.IsOn ?? false;

            profileNameInput?.Destroy();
            acceptProfileNameButton?.Destroy();
            generationSeedInput?.Destroy();
            generateBoardButton?.Destroy();
            lockoutToggleButton?.Destroy();

            generationMenu?.Children.Clear();

            generationMenu ??= new(layoutRoot)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Spacing = 15,
                    Orientation = Orientation.Vertical,
                    Padding = new Padding(0, 50, 20, 15),
                };

            profileNameInput = new(layoutRoot, "profileName")
            {
                FontSize = MenuUI.fontSize,
                MinWidth = MenuUI.profileNameFieldWidth,
                Placeholder = "Profile Name",
            };
            profileNameInput.OnHover += HoverTextInput;
            profileNameInput.OnUnhover += UnhoverTextInput;
            acceptProfileNameButton = new(layoutRoot, "acceptProfileNameButton")
            {
                Content = "Set Name",
                FontSize = 15,
                Margin = 20,
                MinWidth = MenuUI.acceptProfileNameButtonWidth,
                MinHeight = 25,
            };

            generationSeedInput = new(layoutRoot, "Seed")
            {
                FontSize = MenuUI.fontSize,
                MinWidth = MenuUI.seedFieldWidth,
                Placeholder = "Seed",
            };
            generationSeedInput.OnHover += HoverTextInput;
            generationSeedInput.OnUnhover += UnhoverTextInput;
            generateBoardButton = new(layoutRoot, "generateBoardButton")
            {
                Content = "Generate Board",
                FontSize = MenuUI.fontSize,
                Margin = 20,
                MinWidth = MenuUI.generateButtonWidth,
                MinHeight = 50,
                Enabled = false,
            };

            Sprite lockoutSprite = Loader.GetTexture("BingoSync Lockout Icon.png").ToSprite();
            Sprite nonLockoutSprite = Loader.GetTexture("BingoSync Non-Lockout Icon.png").ToSprite();

            lockoutToggleButton = new(layoutRoot, lockoutSprite, nonLockoutSprite, _ => { }, "Lockout Toggle");
            Button lockoutButton = new(layoutRoot, "lockoutToggleButton")
            {
                MinWidth = MenuUI.lockoutButtonWidth,
                MinHeight = MenuUI.lockoutButtonWidth,
            };
            lockoutToggleButton.SetButton(lockoutButton);

            if (lockoutButtonIsLockout)
            {
                lockoutToggleButton.Toggle(lockoutButton);
            }

            SetupRenameProfileRow();
            SetupGenerateRow();
        }

        private static void HoverTextInput(TextInput _)
        {
            TextBoxActive = true;
        }
        private static void UnhoverTextInput(TextInput _)
        {
            // note: this also gets called if the textbox becomes inactive by hiding the menu
            TextBoxActive = false;
        }

        private static void SetupRenameProfileRow()
        {
            acceptProfileNameButton.Click += AcceptProfileNameButtonClicked;

            StackLayout profileRenameRow = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            profileRenameRow.Children.Add(profileNameInput);
            profileRenameRow.Children.Add(acceptProfileNameButton);

            Sprite leftArrowSprite = Loader.GetTexture("BingoSync Left Arrow.png").ToSprite();
            Sprite rightArrowSprite = Loader.GetTexture("BingoSync Right Arrow.png").ToSprite();

            Panel leftArrowPanel = new(layoutRoot, leftArrowSprite, "leftArrowPanel")
            {
                MinHeight = MenuUI.profileScreenArrowButtonWidth,
                MinWidth = MenuUI.profileScreenArrowButtonWidth,
            };
            Panel rightArrowPanel = new(layoutRoot, rightArrowSprite, "rightArrowPanel")
            {
                MinHeight = MenuUI.profileScreenArrowButtonWidth,
                MinWidth = MenuUI.profileScreenArrowButtonWidth,
            };

            previousProfileButton = new(layoutRoot, "leftArrowButton")
            {
                MinHeight = MenuUI.profileScreenArrowButtonWidth,
                MinWidth = MenuUI.profileScreenArrowButtonWidth,
            };
            nextProfileButton = new(layoutRoot, "rightArrowButton")
            {
                MinHeight = MenuUI.profileScreenArrowButtonWidth,
                MinWidth = MenuUI.profileScreenArrowButtonWidth,
            };

            previousProfileButton.Click += PreviousProfileButtonClicked;
            nextProfileButton.Click += NextProfileButtonClicked;

            leftArrowPanel.Child = previousProfileButton;
            rightArrowPanel.Child = nextProfileButton;

            profileRenameRow.Children.Add(leftArrowPanel);
            profileRenameRow.Children.Add(rightArrowPanel);

            generationMenu.Children.Add(profileRenameRow);
        }

        private static void SetupGenerateRow()
        {
            generateBoardButton.Click += Controller.GenerateButtonClicked;

            StackLayout bottomRow = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            bottomRow.Children.Add(generateBoardButton);
            bottomRow.Children.Add(generationSeedInput);
            bottomRow.Children.Add(lockoutToggleButton);

            generationMenu.Children.Add(bottomRow);
        }

        public static void SetupGameModeButtons()
        {
            SetupProfileSelection();

            StackLayout buttonLayout = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Vertical,
            };

            StackLayout row = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            foreach (var button in gameModeButtons)
            {
                button.Destroy();
            }
            gameModeButtons.Clear();

            List<string> gameModes = GameModesManager.GameModeNames();
            int gameModeCountOnScreen = Math.Min(_profilesPerScreen, gameModes.Count - currentProfileScreen*_profilesPerScreen);
            List<string> gameModesOnScreen = gameModes.GetRange(currentProfileScreen * _profilesPerScreen, gameModeCountOnScreen);

            foreach (string gameMode in gameModesOnScreen)
            {
                if (row.Children.Count >= 3)
                {
                    buttonLayout.Children.Insert(0, row);
                    row = new(layoutRoot)
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Spacing = 10,
                        Orientation = Orientation.Horizontal,
                    };
                }
                Button gameModeButton = CreateGameModeButton(gameMode);
                gameModeButtons.Add(gameModeButton);
                row.Children.Add(gameModeButton);
            }
            buttonLayout.Children.Insert(0, row);
            generationMenu.Children.Insert(0, buttonLayout);
            SelectGameMode(gameModeButtons[0]);
        }

        public static Button CreateGameModeButton(string name)
        {
            Button button = new(layoutRoot, name)
            {
                Content = name,
                FontSize = 15,
                Margin = 20,
                MinWidth = MenuUI.gameModeButtonWidth,
            };
            button.Click += SelectGameMode;
            return button;
        }

        public static (int, bool) GetSeed()
        {
            string inputStr = generationSeedInput.Text;
            int seed = unchecked(DateTime.Now.Ticks.GetHashCode());
            bool isCustom = false;
            if (inputStr != string.Empty)
            {
                isCustom = true;
                bool isNumeric = int.TryParse(inputStr, out seed);
                if (!isNumeric)
                {
                    seed = inputStr.GetHashCode();
                }
            }
            return (seed, isCustom);
        }

        public static bool LockoutToggleButtonIsOn()
        {
            return lockoutToggleButton.IsOn;
        }

        private static void SelectGameMode(Button sender)
        {
            string gameModeName = sender.Content;
            Controller.ActiveGameMode = gameModeName;
            profileNameInput.Text = gameModeName;
            if (gameModeName.EndsWith("*"))
            {
                profileNameInput.Text = gameModeName.Remove(gameModeName.Count() - 1, 1);
            }
            foreach (Button gameMode in gameModeButtons)
            {
                gameMode.BorderColor = Color.white;
            }
            sender.BorderColor = Color.red;
            bool isCustom = Controller.IsCustomGameMode(gameModeName);
            profileNameInput.Enabled = isCustom;
            acceptProfileNameButton.Enabled = isCustom;
        }

        public static void SetGenerationButtonEnabled(bool enabled)
        {
            generateBoardButton.Enabled = enabled;
        }

        private static void AcceptProfileNameButtonClicked(Button _)
        {
            string rawName = profileNameInput.Text;
            string displayName = rawName + "*";
            if(rawName == string.Empty)
            {
                Log($"A name must be given to rename a gamemode");
                return;
            }
            if (gameModeButtons.FindIndex(gameMode => gameMode.Content == displayName) != -1)
            {
                Log($"Cannot rename gamemode to {displayName}, that name already exists");
                return;
            }
            string oldName = Controller.ActiveGameMode;
            bool success = Controller.RenameActiveGameModeTo(rawName);
            if (success)
            {
                gameModeButtons.Find(button => button.Content == oldName).Content = displayName;
                Controller.RefreshMenu();
            }
        }

        private static void PreviousProfileButtonClicked(Button _)
        {
            if (currentProfileScreen > 0) --currentProfileScreen;
            Controller.RegenerateGameModeButtons();
        }

        private static void NextProfileButtonClicked(Button _)
        {
            int profilesCount = GameModesManager.GameModeNames().Count;
            if (currentProfileScreen < (profilesCount / _profilesPerScreen)) ++currentProfileScreen;
            Controller.RegenerateGameModeButtons();
        }

        public static void SetupProfileSelection()
        {
            int profilesCount = GameModesManager.GameModeNames().Count;
            previousProfileButton.Enabled = (currentProfileScreen > 0);
            nextProfileButton.Enabled = (profilesCount > _profilesPerScreen * (currentProfileScreen+1));
            while(profilesCount < _profilesPerScreen * currentProfileScreen + 1)
            {
                --currentProfileScreen;
            }
        }
    }
}
