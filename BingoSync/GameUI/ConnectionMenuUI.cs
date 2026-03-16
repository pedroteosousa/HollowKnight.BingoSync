using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using System.Collections.Generic;
using MagicUI.Graphics;
using System.Reflection;
using System.Linq;
using System;
using BingoSync.Sessions;
using InputField = UnityEngine.UI.InputField;

namespace BingoSync.GameUI
{
    static class ConnectionMenuUI
    {
        private static Action<string> Log;
        private static readonly TextureLoader Loader = new(Assembly.GetExecutingAssembly(), "BingoSync.Resources.Images");

        private static LayoutRoot layoutRoot;
        private static StackLayout connectionMenu;

        private static TextInput roomCodeInput;
        private static TextInput nicknameInput;
        private static TextInput passwordInput;
        private static List<Button> colorButtons;
        private static Button joinRoomButton;
        private static ToggleButton handModeToggleButton;

        public static bool TextBoxActive { get; private set; } = false;
        public static bool HandMode
        {
            get
            {
                return handModeToggleButton.IsOn;
            }
            set
            {
                if (handModeToggleButton.IsOn != value)
                {
                    handModeToggleButton.Toggle(null);
                }
            }
        }

        public static void Setup(Action<string> log, LayoutRoot layoutRoot)
        {
            Log = log;
            Loader.Preload();
            ConnectionMenuUI.layoutRoot = layoutRoot;

            SetupTextFields();
            SetupColorButtons();
            SetupConnectionButtons();

            LoadDefaults();
        }

        private static void SetupTextFields()
        {
            connectionMenu = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Vertical,
                Padding = new Padding(0, 50, 20, 0),
            };
            roomCodeInput = new(layoutRoot, "RoomCode")
            {
                FontSize = MenuUI.fontSize,
                MinWidth = MenuUI.textFieldWidth,
                Placeholder = "Room Link",
            };
            roomCodeInput.OnHover += HoverTextInput;
            roomCodeInput.OnUnhover += UnhoverTextInput;
            nicknameInput = new(layoutRoot, "NickName")
            {
                FontSize = MenuUI.fontSize,
                MinWidth = MenuUI.textFieldWidth,
                Placeholder = "Nickname",
            };
            nicknameInput.OnHover += HoverTextInput;
            nicknameInput.OnUnhover += UnhoverTextInput;
            passwordInput = new(layoutRoot, "Password")
            {
                FontSize = MenuUI.fontSize,
                MinWidth = MenuUI.textFieldWidth,
                Placeholder = "Password",
                ContentType = InputField.ContentType.Password,
            };
            passwordInput.OnHover += HoverTextInput;
            passwordInput.OnUnhover += UnhoverTextInput;

            connectionMenu.Children.Add(roomCodeInput);
            connectionMenu.Children.Add(nicknameInput);
            connectionMenu.Children.Add(passwordInput);
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

        private static void SetupColorButtons()
        {
            colorButtons =
            [
                CreateColorButton("Orange", Colors.Orange.GetColor()),
                CreateColorButton("Red", Colors.Red.GetColor()),
                CreateColorButton("Blue", Colors.Blue.GetColor()),
                CreateColorButton("Green", Colors.Green.GetColor()),
                CreateColorButton("Purple", Colors.Purple.GetColor()),
                CreateColorButton("Navy", Colors.Navy.GetColor()),
                CreateColorButton("Teal", Colors.Teal.GetColor()),
                CreateColorButton("Brown", Colors.Brown.GetColor()),
                CreateColorButton("Pink", Colors.Pink.GetColor()),
                CreateColorButton("Yellow", Colors.Yellow.GetColor())
            ];

            StackLayout colorButtonsLayout = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Vertical,
            };

            StackLayout row1 = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            StackLayout row2 = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            for (int i = 0; i < 5; ++i)
            {
                row1.Children.Add(colorButtons.ElementAt(i));
                row2.Children.Add(colorButtons.ElementAt(5 + i));
            }

            colorButtonsLayout.Children.Add(row1);
            colorButtonsLayout.Children.Add(row2);

            connectionMenu.Children.Add(colorButtonsLayout);
        }

        private static void SetupConnectionButtons()
        {
            joinRoomButton = new(layoutRoot, "roomButton")
            {
                Content = "Join Room",
                FontSize = MenuUI.fontSize,
                Margin = 20,
                MinWidth = MenuUI.joinRoomButtonWidth,
            };
            Sprite handModeSprite = Loader.GetTexture("BingoSync Hand Icon.png").ToSprite();
            Sprite nonHandModeSprite = Loader.GetTexture("BingoSync Eye Icon.png").ToSprite();

            handModeToggleButton = new(layoutRoot, handModeSprite, nonHandModeSprite, Controller.ToggleHandModeButtonClicked, "Hand Mode Toggle");
            Button handModeButton = new(layoutRoot, "handModeToggleButton")
            {
                MinWidth = MenuUI.handModeButtonWidth,
                MinHeight = MenuUI.handModeButtonWidth,
            };
            handModeToggleButton.SetButton(handModeButton);

            joinRoomButton.Click += ReadCurrentConnectionInfo;
            joinRoomButton.Click += Controller.JoinRoomButtonClicked;
            joinRoomButton.Click += Update;

            StackLayout bottomRow = new(layoutRoot)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                Spacing = 10,
                Orientation = Orientation.Horizontal,
            };

            bottomRow.Children.Add(joinRoomButton);
            bottomRow.Children.Add(handModeToggleButton);
            
            connectionMenu.Children.Add(bottomRow);
        }

        private static Button CreateColorButton(string text, Color color)
        {
            Button button = new(layoutRoot, text.ToLower())
            {
                Content = text,
                FontSize = 15,
                Margin = 20,
                BorderColor = color,
                ContentColor = color,
                MinWidth = MenuUI.colorButtonWidth,
            };
            button.Click += SelectColor;
            return button;
        }

        private static void SelectColor(Button sender)
        {
            Button previousSelectedColor = layoutRoot.GetElement<Button>(Controller.RoomColor);
            previousSelectedColor.BorderColor = previousSelectedColor.ContentColor;
            Controller.RoomColor = sender.Name;
            sender.BorderColor = Color.white;
        }

        private static string SanitizeRoomCode(string input)
        {
            return new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c)).ToArray())
            .Split('/').Last();
        }

        public static void ReadCurrentConnectionInfo(Button _ = null)
        {
            Controller.RoomCode = SanitizeRoomCode(roomCodeInput.Text);
            Controller.RoomNickname = nicknameInput.Text;
            Controller.RoomPassword = passwordInput.Text;
        }

        public static void SetConnectionInfoFromSession(Session session)
        {
            roomCodeInput.Text = session.RoomLink;
            nicknameInput.Text = session.RoomNickname;
            passwordInput.Text = session.RoomPassword;
            HandMode = session.HandMode;
            foreach(Button button in colorButtons)
            {
                if (button.Content.ToLower() == session.RoomColor.GetName())
                {
                    button.BorderColor = Color.white;
                }
                else
                {
                    button.BorderColor = button.ContentColor;
                }
            }
        }

        public static void Update(Button _ = null)
        {
            if (Controller.ActiveSession.ClientIsConnected())
            {
                joinRoomButton.Content = "Exit Room";
                joinRoomButton.Enabled = true;
                SetEnabled(false);
            }
            else if (Controller.ActiveSession.ClientIsConnecting())
            {
                joinRoomButton.Content = "Loading...";
                joinRoomButton.Enabled = false;
                SetEnabled(false);
            }
            else
            {
                joinRoomButton.Content = "Join Room";
                joinRoomButton.Enabled = true;
                SetEnabled(true);
            }
        }

        private static void SetEnabled(bool enabled)
        {
            roomCodeInput.Enabled = enabled;
            nicknameInput.Enabled = enabled;
            passwordInput.Enabled = enabled;
            colorButtons.ForEach(button =>
            {
                button.Enabled = enabled;
            });
        }

        public static void LoadDefaults()
        {
            if (nicknameInput != null)
                nicknameInput.Text = Controller.GlobalSettings.DefaultNickname;
            if (passwordInput != null)
                passwordInput.Text = Controller.GlobalSettings.DefaultPassword;
            Controller.RoomColor = Controller.GlobalSettings.DefaultColor;
            if (layoutRoot == null)
                return;
            Button selectedColorButton = layoutRoot.GetElement<Button>(Controller.RoomColor);
            if (selectedColorButton != null)
            {
                selectedColorButton.BorderColor = Color.white;
            }
        }

        public static void UpdateColorScheme()
        {
            foreach(Button button in colorButtons)
            {
                button.ContentColor = ColorExtensions.FromName(button.Content.ToLower()).GetColor();
                button.BorderColor = ColorExtensions.FromName(button.Content.ToLower()).GetColor();
            }
            layoutRoot.GetElement<Button>(Controller.RoomColor).BorderColor = Color.white;
        }

    }
}
