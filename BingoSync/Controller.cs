using BingoSync.Clients;
using BingoSync.CustomGoals;
using BingoSync.GameUI;
using BingoSync.Interfaces;
using BingoSync.ModMenu;
using BingoSync.Sessions;
using BingoSync.Settings;
using MagicUI.Elements;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BingoSync
{
    internal static class Controller
    {
        public static ModSettings GlobalSettings { get; set; } = new ModSettings();

        public static AudioPlayer Audio { get; set; } = new AudioPlayer();

        public static Session DefaultSession { get; set; }
        private static Session _activeSession;
        public static Session ActiveSession { 
            get {
                return _activeSession;
            }
            set {
                Session previous = _activeSession;
                _activeSession = value;
                if (previous == null) return;
                SessionManager.SessionChanged(previous);
            }
        }
        public static bool IsOnMainMenu { get; set; } = true;
        public static bool MenuIsVisible { get; set; } = true;
        public static bool BoardIsVisible
        {
            get
            {
                return ActiveSession?.BoardIsVisible ?? false;
            }
            set
            {
                if (ActiveSession != null)
                {
                    ActiveSession.BoardIsVisible = value;
                }
            }
        }
        public static string ActiveGameMode { get; set; } = string.Empty;
        public static bool MenuIsLockout
        {
            get
            {
                return MenuUI.IsLockout();
            }
            private set { }
        }
        public static bool HandMode
        {
            get
            {
                return ActiveSession?.HandMode ?? false; // MenuUI.IsHandMode();
            }
            set
            {
                if (ActiveSession != null)
                {
                    ActiveSession.HandMode = value;
                    MenuUI.HandMode = value;
                }
            }
        }

        public static string RoomCode {
            get
            {
                return ActiveSession?.RoomLink;
            } 
            set
            {
                ActiveSession.RoomLink = value;
            }
        }
        public static string RoomPassword
        {
            get
            {
                return ActiveSession?.RoomPassword;
            }
            set
            {
                ActiveSession.RoomPassword = value;
            }
        }
        public static string RoomNickname
        {
            get
            {
                return ActiveSession?.RoomNickname;
            }
            set
            {
                ActiveSession.RoomNickname = value;
            }
        }
        public static string RoomColor
        {
            get
            {
                return ActiveSession?.RoomColor.GetName();
            }
            set
            {
                if (ActiveSession != null)
                {
                    ActiveSession.RoomColor = ColorExtensions.FromName(value);
                }
            }
        }

        public static bool ShowSessionName { 
            get
            {
                return SessionManager.ShowSessionName;
            }
            private set { }
        }

        public static bool IsDebugMode
        {
            get
            {
                return GlobalSettings.DebugMode;
            }
            private set { }
        }

        public static event Action OnBoardUpdate;

        private static Action<string> Log;
        private static readonly Stopwatch timer = new();
        private static readonly TimeSpan showBoardButtonTimeout = new(days: 0, hours: 0, minutes: 0, seconds: 0, milliseconds: 300);
        private static int showBoardClickCount = 0;

        public static void Setup(Action<string> log)
        {
            Log = log;
            DefaultSession = new Session("Default", new BingoSyncClient(log), true)
            {
                AudioNotificationOn = GlobalSettings.AudioNotificationOn
            };
            ActiveSession = DefaultSession;
            OnBoardUpdate += BingoBoardUI.UpdateGrid;
            OnBoardUpdate += BingoBoardUI.UpdateName;
            OnBoardUpdate += ConfirmTopLeftOnReveal;
            OnBoardUpdate += RefreshGenerationButtonEnabled;
            SessionManager.OnSessionChanged += OnSessionChanged;
        }

        public static void BoardUpdate()
        {
            OnBoardUpdate?.Invoke();
        }

        private static void OnSessionChanged(object _, Session previous)
        {
            RefreshGenerationButtonEnabled();
            RefreshUIWithSession(ActiveSession);
        }

        public static void RefreshUIWithSession(Session session)
        {
            ConnectionMenuUI.SetConnectionInfoFromSession(session);
            BoardUpdate();
        }

        public static void SetHandModeButtonState(bool handMode)
        {
            MenuUI.HandMode = handMode;
        }

        public static void ToggleBoardKeybindClicked()
        {
            if (!ActiveSession.Board.IsAvailable)
            {
                return;
            }
            if (!HandMode)
            {
                BoardIsVisible = !BoardIsVisible;
                return;
            }
            if (timer.Elapsed < showBoardButtonTimeout)
            {
                ++showBoardClickCount;
            }
            else
            {
                showBoardClickCount = 1;
            }
            if (showBoardClickCount > 2 || BoardIsVisible)
            {
                showBoardClickCount = 0;
                BoardIsVisible = !BoardIsVisible;
            }
            timer.Restart();
        }

        public static void GenerateButtonClicked(Button _)
        {
            GameModesManager.Generate();
            Task resetBoardVisibility = new(() =>
            {
                Thread.Sleep(300);
                BoardIsVisible = true;
            });
            resetBoardVisibility.Start();
        }

        public static void ConfirmTopLeftOnReveal()
        {
            if (!HandMode)
            {
                return;
            }
            if (!ActiveSession.Board.IsAvailable || !ActiveSession.Board.IsRevealed || ActiveSession.Board.IsConfirmed)
            {
                return;
            }
            ActiveSession.Board.IsConfirmed = true;
            string message = $"Revealed my card in hand-mode, my top-left goal is \"{ActiveSession.Board.GetIndex(0).Name}\"";
            ActiveSession.SendChatMessage(message);
        }

        public static void RefreshDefaultsFromUI()
        {
            ConnectionMenuUI.ReadCurrentConnectionInfo();
        }

        public static void CycleBoardOpacity()
        {
            if (!ActiveSession.Board.IsAvailable)
            {
                return;
            }
            GlobalSettings.BoardAlphaIndex += 1;
            GlobalSettings.BoardAlphaIndex %= GlobalSettings.BoardAlphas.Count();
            RefreshBoardOpacity();
            RefreshMenu();
        }

        public static void RefreshBoardOpacity()
        {
            BingoBoardUI.SetBoardAlpha(GlobalSettings.BoardAlpha);
        }

        public static void RevealButtonClicked(Button _)
        {
            RevealCard();
        }

        public static void RevealKeybindClicked()
        {
            RevealCard();
        }

        public static void JoinRoomButtonClicked(Button _)
        {
            if (!ActiveSession.ClientIsConnected())
            {
                ActiveSession.JoinRoom(RoomCode, RoomNickname, RoomPassword, (ex) => {
                    ConnectionMenuUI.Update();
                    RefreshGenerationButtonEnabled();
                });
            }
            else
            {
                ActiveSession.ExitRoom(() => {
                    ConnectionMenuUI.Update();
                    RefreshGenerationButtonEnabled();
                });
            }
        }

        public static void ToggleHandModeButtonClicked(Button _)
        {
            HandMode = MenuUI.HandMode;
        }

        public static void ResetConnectionButtonClicked()
        {
            new Task(() =>
            {
                ActiveSession.ExitRoom(() =>
                {
                    ConnectionMenuUI.Update();
                    RefreshGenerationButtonEnabled();
                });
                Thread.Sleep(250);
                ActiveSession.JoinRoom(RoomCode, RoomNickname, RoomPassword, (ex) =>
                {
                    ConnectionMenuUI.Update();
                    RefreshGenerationButtonEnabled();
                });
            }).Start();
        }

        public static void RevealCard()
        {
            if (ActiveSession.Board.IsRevealed)
            {
                return;
            }
            ActiveSession.Board.IsConfirmed = false;
            ActiveSession.RevealCard();
            if (HandMode)
            {
                BoardIsVisible = false;
            }
        }

        public static (int, bool) GetCurrentSeed()
        {
            return MenuUI.GetSeed();
        }

        public static void RegenerateGameModeButtons()
        {
            GenerationMenuUI.SetupProfileSelection();
            GameModesManager.RefreshCustomGameModes();
            GenerationMenuUI.CreateGenerationMenu();
            GenerationMenuUI.SetupGameModeButtons();
        }

        public static void DumpDebugInfo()
        {
        }

        public static bool RenameActiveGameModeTo(string newName)
        {
            GameMode gameMode = GameModesManager.FindGameModeByDisplayName(ActiveGameMode);
            if(gameMode == null || gameMode.GetType() != typeof(CustomGameMode))
            {
                Log($"Cannot rename non-custom gamemode {ActiveGameMode}");
                return false;
            }
            CustomGameMode customGameMode = (CustomGameMode)gameMode;
            string oldName = customGameMode.InternalName;
            customGameMode.InternalName = newName;
            GameModesManager.RenameGameModeFile(oldName, newName);
            ActiveGameMode = customGameMode.GetDisplayName();
            return true;
        }

        public static bool IsCustomGameMode(string name)
        {
            return GameModesManager.FindGameModeByDisplayName(name).GetType() == typeof(CustomGameMode);
        }

        public static void SetGenerationButtonEnabled(bool enabled)
        {
            GenerationMenuUI.SetGenerationButtonEnabled(enabled);
        }

        public static void RefreshGenerationButtonEnabled()
        {
            bool clientConnected = (ActiveSession.ClientIsConnected() || ActiveSession.ClientIsConnecting());
            bool standardGeneration = !ActiveSession.NonStandardBoardGeneration;
            SetGenerationButtonEnabled(clientConnected && IsOnMainMenu && standardGeneration);
        }

        public static void RefreshMenu()
        {
            MainMenu.RefreshMenu();
        }
    }
}
