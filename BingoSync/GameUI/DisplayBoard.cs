using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using System.Collections.Generic;
using GridLayout = MagicUI.Elements.GridLayout;
using Satchel;
using static BingoSync.Settings.ModSettings;
using MagicUI.Graphics;
using System.Reflection;

namespace BingoSync.GameUI
{
    class DisplayBoard
    {
        internal class SquareLayoutObjects
        {
            public TextObject Text;
            public Dictionary<HighlightType, Image> Highlights;
            public Dictionary<string, Image> BackgroundColors;
            public Dictionary<string, Image> ColorsIcons;
        };
        private readonly LayoutRoot layoutRoot;
        private readonly StackLayout boardAndName;
        private readonly GridLayout gridLayout;
        public List<SquareLayoutObjects> bingoLayout;
        public readonly TextObject boardName;
        private bool opacityInitialized = false;
        private readonly Dictionary<string, List<Image>> backgroundImagesByColor = new()
        {
            ["orange"] = [],
            ["red"] = [],
            ["blue"] = [],
            ["green"] = [],
            ["purple"] = [],
            ["navy"] = [],
            ["teal"] = [],
            ["brown"] = [],
            ["pink"] = [],
            ["yellow"] = [],
            ["blank"] = []
        };

        private static readonly Dictionary<string, Sprite> colorIconSprites = [];

        private static readonly TextureLoader Loader = new(Assembly.GetExecutingAssembly(), "BingoSync.Resources.Images.ColorIcons");
        static DisplayBoard()
        {
            Loader.Preload();

            colorIconSprites["orange"] = Loader.GetTexture("BingoSync Color Icon Orange.png").ToSprite();
            colorIconSprites["red"] = Loader.GetTexture("BingoSync Color Icon Red.png").ToSprite();
            colorIconSprites["blue"] = Loader.GetTexture("BingoSync Color Icon Blue.png").ToSprite();
            colorIconSprites["green"] = Loader.GetTexture("BingoSync Color Icon Green.png").ToSprite();
            colorIconSprites["purple"] = Loader.GetTexture("BingoSync Color Icon Purple.png").ToSprite();
            colorIconSprites["navy"] = Loader.GetTexture("BingoSync Color Icon Navy.png").ToSprite();
            colorIconSprites["teal"] = Loader.GetTexture("BingoSync Color Icon Teal.png").ToSprite();
            colorIconSprites["brown"] = Loader.GetTexture("BingoSync Color Icon Brown.png").ToSprite();
            colorIconSprites["pink"] = Loader.GetTexture("BingoSync Color Icon Pink.png").ToSprite();
            colorIconSprites["yellow"] = Loader.GetTexture("BingoSync Color Icon Yellow.png").ToSprite();
        }

        public DisplayBoard(Sprite backgroundSprite, Dictionary<HighlightType, Sprite> highlightSprites)
        {
            layoutRoot = new(true, "BingoSync_BoardDisplayRoot");

            boardAndName = new(layoutRoot, "BingoSync_BoardDisplayStack")
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Visibility = Visibility.Visible,
            };

            gridLayout = new GridLayout(layoutRoot, "BingoSync_BoardDisplayGrid")
            {
                MinWidth = 600,
                MinHeight = 600,
                RowDefinitions =
                {
                    new GridDimension(1, GridUnit.Proportional),
                    new GridDimension(1, GridUnit.Proportional),
                    new GridDimension(1, GridUnit.Proportional),
                    new GridDimension(1, GridUnit.Proportional),
                    new GridDimension(1, GridUnit.Proportional),
                },
                ColumnDefinitions =
                {
                    new GridDimension(1, GridUnit.Proportional),
                    new GridDimension(1, GridUnit.Proportional),
                    new GridDimension(1, GridUnit.Proportional),
                    new GridDimension(1, GridUnit.Proportional),
                    new GridDimension(1, GridUnit.Proportional),
                },
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Visibility = Visibility.Visible,
            };

            boardName = new(layoutRoot, "BingoSync_BoardDisplayName")
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                TextAlignment = HorizontalAlignment.Left,
                Text = "Hello there",
                Visibility = Visibility.Visible,
                FontSize = 26,
                Padding = new Padding(5, 3),
            };

            boardAndName.Children.Add(gridLayout);
            boardAndName.Children.Add(boardName);

            CreateBaseLayout(backgroundSprite, highlightSprites);

            layoutRoot.VisibilityCondition = BoardShouldBeVisible;
        }

        private bool BoardShouldBeVisible()
        {
            bool shouldBeVisible = Controller.ActiveSession.ClientIsConnected() && Controller.BoardIsVisible && Controller.ActiveSession.Board.IsAvailable && Controller.ActiveSession.Board.IsRevealed;
            if (shouldBeVisible && !opacityInitialized)
            {
                opacityInitialized = true;
                SetAlpha(Controller.GlobalSettings.BoardAlpha);
            }
            return shouldBeVisible;
        }

        public void SetAlpha(float alpha)
        {
            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();
            if (objects == null)
            {
                return;
            }
            foreach (GameObject obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }
                string name = obj.GetName();
                if(name.Contains("BingoSync_BoardDisplay") && !name.Contains("text"))
                {
                    if (!name.Contains("icon") || Controller.GlobalSettings.AdaptIconOpcaity)
                    {
                        obj.GetComponent<CanvasRenderer>()?.SetAlpha(alpha);
                    }
                    else
                    {
                        obj.GetComponent<CanvasRenderer>()?.SetAlpha(1.0f);
                    }
                }
            }
        }

        private void CreateBaseLayout(Sprite backgroundSprite, Dictionary<HighlightType, Sprite> highlightSprites)
        {
            bingoLayout = [];
            for (int row = 0; row < 5; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    var (stack, images, icons) = GenerateSquareBackgroundImage(row, column, backgroundSprite);
                    gridLayout.Children.Add(stack.WithProp(GridLayout.Row, row).WithProp(GridLayout.Column, column));

                    TextObject textObject = new TextObject(layoutRoot, $"BingoSync_BoardDisplay_square_{row}_{column}_text")
                    {
                        FontSize = 12,
                        Text = "",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        MaxWidth = 100,
                        MaxHeight = 100,
                        Padding = new Padding(10),
                        ContentColor = Color.white,
                    }.WithProp(GridLayout.Row, row).WithProp(GridLayout.Column, column);
                    gridLayout.Children.Add(textObject);

                    Dictionary<HighlightType, Image> highlightImages = [];
                    foreach(KeyValuePair<HighlightType, Sprite> entry in highlightSprites)
                    {
                        Image highlightImage = new Image(layoutRoot, entry.Value, $"BingoSync_BoardDisplay_square_{row}_{column}_highlight_{entry.Key}")
                        {
                            Height = 110,
                            Width = 110,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        }.WithProp(GridLayout.Row, row).WithProp(GridLayout.Column, column);
                        gridLayout.Children.Add(highlightImage);
                        highlightImages[entry.Key] = highlightImage;
                    }

                    foreach(KeyValuePair<string, Image> entry in icons)
                    {
                        gridLayout.Children.Add(icons[entry.Key].WithProp(GridLayout.Row, row).WithProp(GridLayout.Column, column));
                    }

                    bingoLayout.Add(new SquareLayoutObjects
                    {
                        Text = textObject,
                        Highlights = highlightImages,
                        BackgroundColors = images,
                        ColorsIcons = icons,
                    });
                }
            }
        }

        private (StackLayout, Dictionary<string, Image>, Dictionary<string, Image>) GenerateSquareBackgroundImage(int row, int column, Sprite backgroundSprite)
        {
            StackLayout stack = new StackLayout(layoutRoot, $"BingoSync_BoardDisplay_background_{row}_{column}")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Orientation = Orientation.Vertical,
                Spacing = 0,
            }.WithProp(GridLayout.Row, row).WithProp(GridLayout.Column, column);

            List<string> colors = ColorExtensions.GetAllColorNames();
            Dictionary<string, Image> images = [];
            Dictionary<string, Image> icons = [];
            for (int brow = 0; brow < colors.Count; brow++)
            {
                Color tint = ColorExtensions.FromName(colors[brow]).GetColor();
                Image backgroundImage = new Image(layoutRoot, backgroundSprite, $"BingoSync_BoardDisplay_color_{brow}_{row}_{column}")
                {
                    Height = 0,
                    Width = 110,
                    Tint = tint,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                stack.Children.Add(backgroundImage);
                images.Add(colors[brow], backgroundImage);
                backgroundImagesByColor[colors[brow]].Add(backgroundImage);

                if(colors[brow] != "blank")
                {
                    Image colorIcon = new Image(layoutRoot, colorIconSprites[colors[brow]], $"BingoSync_BoardDisplay_icon_{brow}_{row}_{column}")
                    {
                        Height = 110,
                        Width = 110,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    icons.Add(colors[brow], colorIcon);
                }
            }

            return (stack, images, icons);
        }

        public void UpdateColorScheme()
        {
            foreach(string color in ColorExtensions.GetAllColorNames())
            {
                foreach(Image image in backgroundImagesByColor[color])
                {
                    image.Tint = ColorExtensions.FromName(color).GetColor();
                }
            }
        }
    }
}
