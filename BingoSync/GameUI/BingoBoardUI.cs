using BingoSync.Sessions;
using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static BingoSync.Settings.ModSettings;

namespace BingoSync.GameUI
{
    internal static class BingoBoardUI
    {
        private static DisplayBoard board;

        private static readonly LayoutRoot commonRoot = new(true, "Persistent layout")
        {
            VisibilityCondition = () => false,
        };
        private static readonly Button revealCardButton = new(commonRoot, "revealCard")
        {
            Content = "Reveal Card",
            FontSize = 15,
            Margin = 20,
            BorderColor = Color.white,
            ContentColor = Color.white,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            Padding = new Padding(20),
            MinWidth = 200,
            Visibility = Visibility.Hidden,
        };
        private static readonly TextObject loadingText = new(commonRoot)
        {
            Text = "Loading...",
            FontSize = 15,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            MaxWidth = 200,
            Padding = new Padding(20),
            ContentColor = Color.white,
            Visibility = Visibility.Hidden,
        };
        
        private static Action<string> Log;
        private static readonly TextureLoader Loader = new(Assembly.GetExecutingAssembly(), "BingoSync.Resources.Images");

        public static void Setup(Action<string> log)
        {
            Log = log;

            commonRoot.VisibilityCondition = () => true;

            revealCardButton.Click += Controller.RevealButtonClicked;

            Loader.Preload();

            Sprite backgroundSprite = Loader.GetTexture("BingoSync Background.png").ToSprite();

            Dictionary<HighlightType, Sprite> highlights = [];
            highlights[HighlightType.Border] = Loader.GetTexture("BingoSync Highlight Border.png").ToSprite();
            highlights[HighlightType.Star] = Loader.GetTexture("BingoSync Highlight Star.png").ToSprite();

            board = new DisplayBoard(backgroundSprite, highlights);

            commonRoot.ListenForPlayerAction(Controller.GlobalSettings.Keybinds.ToggleBoard, Controller.ToggleBoardKeybindClicked);
            commonRoot.ListenForPlayerAction(Controller.GlobalSettings.Keybinds.RevealCard, Controller.RevealKeybindClicked);
            commonRoot.ListenForPlayerAction(Controller.GlobalSettings.Keybinds.CycleBoardOpacity, Controller.CycleBoardOpacity);
        }

        public static void UpdateColorScheme()
        {
            board.UpdateColorScheme();
        }

        public static void UpdateGrid()
        {
            loadingText.Visibility = (!Controller.ActiveSession.Board.IsAvailable && Controller.ActiveSession.ClientIsConnecting()) ? Visibility.Visible : Visibility.Hidden;
            revealCardButton.Visibility = (Controller.ActiveSession.ClientIsConnected() && Controller.ActiveSession.Board.IsAvailable && !Controller.ActiveSession.Board.IsRevealed) ? Visibility.Visible : Visibility.Hidden;

            if (!Controller.ActiveSession.Board.IsAvailable)
            {
                return;
            }

            int goalIndex = 0;
            foreach (Square square in Controller.ActiveSession.Board.SquaresToDisplay)
            {
                board.bingoLayout[goalIndex].Text.Text = square.Name;
                board.bingoLayout[goalIndex].BackgroundColors.Values.ToList().ForEach(img => img.Height = 0);
                board.bingoLayout[goalIndex].ColorsIcons.Values.ToList().ForEach(img => img.Visibility = Visibility.Hidden);
                foreach (Colors color in square.MarkedBy)
                {
                    board.bingoLayout[goalIndex].BackgroundColors[color.GetName()].Height = 110 / square.MarkedBy.Count;
                    if (Controller.GlobalSettings.UseShapesForColors && color != Colors.Blank)
                    {
                        board.bingoLayout[goalIndex].ColorsIcons[color.GetName()].Visibility = Visibility.Visible;
                    }
                }
                foreach(KeyValuePair<HighlightType, Image> entry in board.bingoLayout[goalIndex].Highlights)
                {
                    HighlightType sprite = entry.Key;
                    Image image = entry.Value;
                    image.Visibility = sprite == Controller.GlobalSettings.SelectedHighlightSprite && square.Highlighted ? Visibility.Visible : Visibility.Hidden;
                }
                ++goalIndex;
            }
        }

        public static void UpdateName()
        {
            board.boardName.Text = Controller.ActiveSession.SessionName;
            board.boardName.Visibility = Controller.ShowSessionName ? Visibility.Visible : Visibility.Hidden;
        }

        public static void SetBoardAlpha(float alpha)
        {
            board.SetAlpha(alpha);
        }
    }
}