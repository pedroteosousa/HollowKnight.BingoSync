using MagicUI.Core;
using MagicUI.Elements;
using MagicUI.Graphics;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using Sprite = UnityEngine.Sprite;

namespace BingoSync.GameUI
{
    public class SharedUIPage
    {
        private const int ArrowButtonSize = 50;
        private const int FontSize = 30;

        private string _name;
        private readonly StackLayout _mainStack;
        private readonly GridLayout _pageSelection;
        private readonly TextObject _pageNameText;
        private readonly int _pageNr;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                _pageNameText.Text = value;
            }
        }
        public LayoutRoot Root { get; private set; }

        internal SharedUIPage(string name, int pageNr)
        {
            _name = name;
            _pageNr = pageNr;
            Root = new LayoutRoot(true, Name + "LayoutRoot")
            {
                VisibilityCondition = IsVisible,
            };
            _mainStack = new StackLayout(Root)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Padding = new Padding(2, 15),
            };
            _pageSelection = new(Root, Name + "BottomRow")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MinHeight = ArrowButtonSize,
                MinWidth = ArrowButtonSize,
                ColumnDefinitions =
                {
                    new GridDimension(ArrowButtonSize, GridUnit.AbsoluteMin),
                    new GridDimension(MenuUI.textFieldWidth - 2 * ArrowButtonSize, GridUnit.AbsoluteMin),
                    new GridDimension(ArrowButtonSize, GridUnit.AbsoluteMin),
                },
                Padding = new(20, 0),
                Visibility = Visibility.Visible,
            };
            _pageNameText = new(Root, Name + "PageName")
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = HorizontalAlignment.Center,
                FontSize = FontSize,
                Text = Name,
                MaxHeight = 50,
                Padding = new(0,10),
            };

            CreateBottomRow();
        }

        public bool AddContent(ArrangableElement content)
        {
            if (content == null || _mainStack.Children.Count >= 2)
            {
                return false;
            }
            _mainStack.Children.Insert(0, content);
            return true;
        }

        public void SetPageSelectionVisible(bool visible)
        {
            _pageSelection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool IsVisible()
        {
            return _pageNr == SharedUIManager.CurrentPage && Controller.MenuIsVisible;
        }

        private void CreateBottomRow()
        {
            Panel leftArrowPanel = new(Root, SharedUIManager.leftArrowSprite, Name + "LeftArrowPanel")
            {
                MinHeight = ArrowButtonSize,
                MinWidth = ArrowButtonSize,
            };
            Panel rightArrowPanel = new(Root, SharedUIManager.rightArrowSprite, Name + "RightArrowPanel")
            {
                MinHeight = ArrowButtonSize,
                MinWidth = ArrowButtonSize,
            };

            Button previousPageButton = new(Root, Name + "PreviousPageButton")
            {
                MinHeight = ArrowButtonSize,
                MinWidth = ArrowButtonSize,
            };
            Button nextPageButton = new(Root, Name + "NextPageButton")
            {
                MinHeight = ArrowButtonSize,
                MinWidth = ArrowButtonSize,
            };

            previousPageButton.Click += SharedUIManager.PreviousPageButtonClicked;
            nextPageButton.Click += SharedUIManager.NextPageButtonClicked;

            leftArrowPanel.Child = previousPageButton;
            rightArrowPanel.Child = nextPageButton;

            _pageSelection.Children.Add(leftArrowPanel);
            leftArrowPanel.WithProp(GridLayout.Row, 0).WithProp(GridLayout.Column, 0);

            _pageSelection.Children.Add(_pageNameText);
            _pageNameText.WithProp(GridLayout.Row, 0).WithProp(GridLayout.Column, 1);

            _pageSelection.Children.Add(rightArrowPanel);
            rightArrowPanel.WithProp(GridLayout.Row, 0).WithProp(GridLayout.Column, 2);

            _mainStack.Children.Add(_pageSelection);
        }
    }

    internal static class SharedUIManager
    {
        private static readonly TextureLoader Loader = new(Assembly.GetExecutingAssembly(), "BingoSync.Resources.Images");

        public static Sprite leftArrowSprite = Loader.GetTexture("BingoSync Left Arrow.png").ToSprite();
        public static Sprite rightArrowSprite = Loader.GetTexture("BingoSync Right Arrow.png").ToSprite();

        private static readonly List<SharedUIPage> Pages = [];
        internal static int CurrentPage = 0;

        public static SharedUIPage RequestUIPage(string name)
        {
            SharedUIPage newPage = new(name, Pages.Count);
            Pages.Add(newPage);
            if(Pages.Count > 1)
            {
                GenerationMenuUI.BottomBarShown = true;
            }
            foreach(SharedUIPage page in Pages)
            {
                page.SetPageSelectionVisible(Pages.Count > 1);
            }
            return newPage;
        }

        internal static void PreviousPageButtonClicked(Button _)
        {
            --CurrentPage;
            if (CurrentPage < 0)
            {
                CurrentPage = Pages.Count - 1;
            }
        }

        internal static void NextPageButtonClicked(Button _)
        {
            ++CurrentPage;
            if(CurrentPage >= Pages.Count)
            {
                CurrentPage = 0;
            }

        }
    }
}
