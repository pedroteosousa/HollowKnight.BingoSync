using BingoSync.GameUI;
using Satchel.BetterMenus;
using System;
using System.Collections.Generic;
using static BingoSync.Settings.ModSettings;

namespace BingoSync.ModMenu
{
    static class BoardSettingsMenu
    {
        private static Menu _BoardSettingsMenu;
        private static List<CustomSlider> _Sliders;

        public static MenuScreen CreateMenuScreen(MenuScreen parentMenu)
        {
            int alphaCount = Controller.GlobalSettings.BoardAlphas.Count;
            // Alphas, Highlight Type, Notif Condition, Notif Clip, Notif Volume
            int elementCount = alphaCount + 4;
            Element[] elements = new Element[elementCount];

            int elementId = 0;
            elements[elementId] = new HorizontalOption(
                name: "Highlight Type",
                description: "Which sprite to use for square highlighting",
                values: Enum.GetNames(typeof(HighlightType)),
                applySetting: (index) =>
                {
                    Controller.GlobalSettings.SelectedHighlightSprite = (HighlightType)index;
                    Controller.BoardUpdate();
                },
                loadSetting: () => (int)Controller.GlobalSettings.SelectedHighlightSprite
            );
            ++elementId;

            _Sliders = [];
            for (int i = 0; i < alphaCount; ++i)
            {
                _Sliders.Add(new CustomSlider(
                    name: $"Opacity preset {i + 1}",
                    storeValue: MakeValueStoreAction(i),
                    loadValue: MakeValueLoadAction(i),
                    minValue: 0,
                    maxValue: 100,
                    wholeNumbers: true
                ));
                elements[elementId] = _Sliders[i];
                ++elementId;
            }

            elements[elementId] = new HorizontalOption(
                name: "Audio For",
                description: "When to play the selected sound",
                values: ["None", "Other Players", "Other Colors", "All Goals"],
                applySetting: (index) =>
                {
                    Controller.GlobalSettings.AudioNotificationOn = (AudioNotificationCondition)index;
                    Controller.DefaultSession.AudioNotificationOn = (AudioNotificationCondition)index;
                    Controller.BoardUpdate();
                },
                loadSetting: () => (int)Controller.GlobalSettings.AudioNotificationOn
            );
            ++elementId;

            elements[elementId] = new HorizontalOption(
                name: "Audio Clip",
                description: "What sound to play when a goal gets marked",
                values: Controller.Audio.ClipNames.ToArray(),
                applySetting: (index) =>
                {
                    Controller.GlobalSettings.AudioClipId = index;
                    Controller.Audio.Play(index);
                    Controller.BoardUpdate();
                },
                loadSetting: () => Controller.GlobalSettings.AudioClipId
            );
            ++elementId;

            elements[elementId] = new CustomSlider(
                name: "Audio Volume",
                storeValue: value => Controller.GlobalSettings.AudioClipVolume = value,
                loadValue: () =>
                {
                    return Controller.GlobalSettings.AudioClipVolume;
                },
                minValue: 0f,
                maxValue: 3f
            );
            ++elementId;

            _BoardSettingsMenu = new Menu("BingoSync", elements);
            // do not call RefreshMenu here, Menu.Update does not work before Menu.GetMenuScreen is called first
            for (int i = 0; i < _Sliders.Count; ++i)
            {
                _Sliders[i].Name = $"Opacity preset {i + 1}" + (i == Controller.GlobalSettings.BoardAlphaIndex ? " (active)" : "");
            }
            return _BoardSettingsMenu.GetMenuScreen(parentMenu);
        }

        public static Action<float> MakeValueStoreAction(int id)
        {
            return (val) =>
            {
                Controller.GlobalSettings.BoardAlphas[id] = val / 100;
                Controller.RefreshBoardOpacity();
            };
        }

        public static Func<float> MakeValueLoadAction(int id)
        {
            return () =>
            {
                return Controller.GlobalSettings.BoardAlphas[id] * 100;
            };
        }

        public static void RefreshMenu()
        {
            if(_Sliders == null || _BoardSettingsMenu == null)
            {
                return;
            }
            for (int i = 0; i < _Sliders.Count; ++i)
            {
                _Sliders[i].Name = $"Opacity preset {i + 1}" + (i == Controller.GlobalSettings.BoardAlphaIndex ? " (active)" : "");
            }
            _BoardSettingsMenu.Update();
        }
    }
}
