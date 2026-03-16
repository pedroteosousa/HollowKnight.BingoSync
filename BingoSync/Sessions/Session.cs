using BingoSync.Clients;
using BingoSync.Clients.EventInfoObjects;
using BingoSync.CustomGoals;
using BingoSync.GameUI;
using BingoSync.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static BingoSync.GoalCompletionTracker;
using static BingoSync.Settings.ModSettings;

namespace BingoSync.Sessions
{
    public class Session
    {
        private readonly IRemoteClient _client;
        private string _sessionName = "Default";
        public string SessionName {
            get
            {
                return _sessionName;
            }
            set
            {
                _sessionName = value;
                Controller.BoardUpdate();
            }
        }
        public bool IsAutoMarking { get; set; }
        public bool BoardIsVisible { get; set; } = true;
        private bool _handMode = false;
        public bool HandMode
        {
            get
            {
                return _handMode;
            }
            set
            {
                _handMode = value;
                if (Controller.ActiveSession == this)
                {
                    Controller.SetHandModeButtonState(value);
                }
            }
        }
        public AudioNotificationCondition AudioNotificationOn { get; set; } = AudioNotificationCondition.None;
        public bool HasCustomAudio { get; set; } = false;
        private int _customAudioClipId = 0;
        public int ActiveAudioId { 
            get
            {
                if (HasCustomAudio)
                {
                    return _customAudioClipId;
                }
                return Controller.GlobalSettings.AudioClipId;
            }
            set
            {
                HasCustomAudio = true;
                _customAudioClipId = value;
            }
        }
        public bool RoomIsLockout { get; set; } = false;
        public string RoomLink { get; set; } = string.Empty;
        public string RoomNickname { get; set; } = string.Empty;
        public string RoomPassword { get; set; } = string.Empty;
        public Colors RoomColor { get; set; } = Colors.Orange;
        public string RoomPlayerUUID { get
            {
                return _client.PlayerUUID;
            } 
        }
        public BingoBoard Board { get; } = new();
        public bool NonStandardBoardGeneration { get; set; } = false;

        #region Events

        public event EventHandler<CardRevealedEventInfo> OnCardRevealedBroadcastReceived;

        private void RefireCardRevealedBroadcast(object _, CardRevealedEventInfo broadcast)
        {
            OnCardRevealedBroadcastReceived?.Invoke(this, broadcast);
        }

        public event EventHandler<ChatMessageEventInfo> OnChatMessageReceived;

        private void RefireChatMessage(object _, ChatMessageEventInfo broadcast)
        {
            OnChatMessageReceived?.Invoke(this, broadcast);
        }

        public event EventHandler<GoalUpdateEventInfo> OnGoalUpdateReceived;

        private void RefireGoalUpdate(object _, GoalUpdateEventInfo broadcast)
        {
            OnGoalUpdateReceived?.Invoke(this, broadcast);
        }

        public event EventHandler<NewCardEventInfo> OnNewCardReceived;

        private void RefireNewCard(object _, NewCardEventInfo broadcast)
        {
            OnNewCardReceived?.Invoke(this, broadcast);
        }

        public event EventHandler<PlayerColorChangeEventInfo> OnPlayerColorChangeReceived;

        private void RefirePlayerColorChange(object _, PlayerColorChangeEventInfo broadcast)
        {
            OnPlayerColorChangeReceived?.Invoke(this, broadcast);
        }

        public event EventHandler<PlayerConnectionEventInfo> OnPlayerConnectedBroadcastReceived;

        private void RefirePlayerConnectedBroadcast(object _, PlayerConnectionEventInfo broadcast)
        {
            OnPlayerConnectedBroadcastReceived?.Invoke(this, broadcast);
        }

        public event EventHandler<RoomSettings> OnRoomSettingsReceived;

        private void RefireRoomSettings(object _, RoomSettings broadcast)
        {
            OnRoomSettingsReceived?.Invoke(this, broadcast);
        }

        public event EventHandler<ClientStateUpdateInfo> OnClientStateChanged;

        private void RefireClientState(object _, ClientStateUpdateInfo broadcast)
        {
            OnClientStateChanged?.Invoke(this, broadcast);
        }

        private void UnsubscribeEventRefires()
        {
            _client.CardRevealedBroadcastReceived -= RefireCardRevealedBroadcast;
            _client.ChatMessageReceived -= RefireChatMessage;
            _client.GoalUpdateReceived -= RefireGoalUpdate;
            _client.NewCardReceived -= RefireNewCard;
            _client.PlayerColorChangeReceived -= RefirePlayerColorChange;
            _client.PlayerConnectedBroadcastReceived -= RefirePlayerConnectedBroadcast;
            _client.RoomSettingsReceived -= RefireRoomSettings;
            _client.ConnectionStateChanged -= RefireClientState;
        }

        private void SubscribeEventRefires()
        {
            UnsubscribeEventRefires();
            _client.CardRevealedBroadcastReceived += RefireCardRevealedBroadcast;
            _client.ChatMessageReceived += RefireChatMessage;
            _client.GoalUpdateReceived += RefireGoalUpdate;
            _client.NewCardReceived += RefireNewCard;
            _client.PlayerColorChangeReceived += RefirePlayerColorChange;
            _client.PlayerConnectedBroadcastReceived += RefirePlayerConnectedBroadcast;
            _client.RoomSettingsReceived += RefireRoomSettings;
            _client.ConnectionStateChanged += RefireClientState;
        }

    #endregion

        public Session(string name, IRemoteClient client, bool markingClient)
        {
            SessionName = name;
            _client = client;
            SubscribeEventRefires();
            IsAutoMarking = markingClient;
            _client.SetBoard(Board);
            OnGoalUpdateReceived += DoAudioNotification;
            OnRoomSettingsReceived += ConsumeRoomSettings;
            OnCardRevealedBroadcastReceived += RevealOnOthersReveal;
            OnCardRevealedBroadcastReceived += MarkCompletedGoalsOnReveal;
            OnNewCardReceived += MarkCompletedGoalsOnNewCard;
            _client.NeedBoardUpdate += ClientTriggeredBoardUpdate;
            GoalCompletionTracker.OnGoalCompletionChanged += OnInternalGoalUpdate;
            ItemSyncInterop.AddSession(this);
        }

        public void LocalUpdate()
        {
            Controller.BoardUpdate();
        }

        private void ConsumeRoomSettings(object sender, RoomSettings settings)
        {
            RoomIsLockout = settings.IsLockout;
        }

        private void MarkCompletedGoalsOnNewCard(object sender, NewCardEventInfo newCardEvent)
        {
            MarkAllCompleted();
        }

        private void MarkCompletedGoalsOnReveal(object sender, CardRevealedEventInfo revealedEvent)
        {
            if (revealedEvent.Player.UUID != _client.PlayerUUID)
            {
                return;
            }
            MarkAllCompleted();
        }

        private void MarkAllCompleted()
        {
            if (GameManager.instance.IsMenuScene())
            {
                return;
            }
            if (!Controller.GlobalSettings.MarkCompletedGoalsOnNewCardReceived)
            {
                return;
            }
            if (!IsPlayable())
            {
                return;
            }
            int index = 0;
            foreach (Square square in Board.AllSquares)
            {
                if (GoalCompletionTracker.IsGoalMarkedByName(square.Name))
                {
                    SelectIndex(index, () => { });
                }
                ++index;
            }
        }

        public bool IsPlayable()
        {
            Update();
            if (!Board.IsAvailable || !Board.IsRevealed)
                return false;
            if (!ClientIsConnected())
                return false;
            return true;
        }

        public bool ClientIsConnected()
        {
            return GetClientState() == ClientState.Connected;
        }

        public bool ClientIsConnecting()
        {
            return GetClientState() == ClientState.Loading;
        }

        public void JoinRoom(string roomID, string nickname, string password, Action<Exception> callback)
        {
            if (roomID == null || roomID == string.Empty
                || nickname == null || nickname == string.Empty
                || password == null || password == string.Empty)
            {
                return;
            }

            _client.JoinRoom(roomID, nickname, password, ColorExtensions.FromName(Controller.RoomColor), callback);
            RoomNickname = nickname;
            RoomColor = ColorExtensions.FromName(Controller.RoomColor);
        }

        public void ExitRoom(Action callback)
        {
            _client.ExitRoom(callback);
        }

        public void Update()
        {
            _client.Update();
            Controller.BoardUpdate();
        }

        public ClientState GetClientState()
        {
            return _client.GetState();
        }

        public void NewCard(List<BingoGoal> board, bool lockout = true, bool hideCard = true)
        {
            _client.NewCard(board, lockout, hideCard);
        }

        public void RevealCard()
        {
            _client.RevealCard();
        }

        public void SendChatMessage(string text)
        {
            _client.SendChatMessage(text);
        }

        internal void OnInternalGoalUpdate(object sender, InternalGoalUpdate goalUpdate)
        {
            if(!IsPlayable() || !IsAutoMarking)
            {
                return;
            }
            int slot = 1;
            foreach (Square square in Board.AllSquares)
            {
                if (square.Name == goalUpdate.Name)
                {
                    UpdateGoalBySlot(slot, goalUpdate);
                }
                ++slot;
            }
        }

        private void UpdateGoalBySlot(int slot, InternalGoalUpdate goalUpdate)
        {
            Square square = Board.GetSlot(slot);
            if (!SquareNeedsUpdate(square, RoomColor, goalUpdate.Clear))
            {
                return;
            }
            Task.Run(() =>
            {
                ItemSyncMarkDelay setting = Controller.GlobalSettings.ItemSyncMarkSetting;
                if (setting == ItemSyncMarkDelay.NoMark && goalUpdate.IsItemSyncUpdate)
                {
                    return;
                }
                if (setting == ItemSyncMarkDelay.Delay && goalUpdate.IsItemSyncUpdate)
                {
                    Thread.Sleep(ItemSyncInterop.MarkDelay);
                    if (!SquareNeedsUpdate(square, RoomColor, goalUpdate.Clear))
                    {
                        return;
                    }
                }
                SelectSlot(slot, () => { }, goalUpdate.Clear);
            });
        }

        private bool SquareNeedsUpdate(Square square, Colors color, bool clear)
        {
            bool isMarked = square.MarkedBy.Contains(color);
            bool isBlank = square.MarkedBy.Contains(Colors.Blank);
            bool canMark = isBlank || (!isMarked && !RoomIsLockout);
            bool shouldMark = canMark && !clear;
            bool shouldUnmark = isMarked && clear;
            return shouldMark || shouldUnmark;
        }

        public void SelectIndex(int index, Action errorCallback, bool clear = false)
        {
            SelectSlot(index + 1, RoomColor, errorCallback, clear);
        }

        public void SelectSlot(int slot, Action errorCallback, bool clear = false)
        {
            SelectSlot(slot, RoomColor, errorCallback, clear);
        }

        public void SelectIndex(int index, Colors color, Action errorCallback, bool clear = false)
        {
            SelectSlot(index + 1, color, errorCallback, clear);
        }

        public void SelectSlot(int slot, Colors color, Action errorCallback, bool clear = false)
        {
            if (SquareNeedsUpdate(Board.GetIndex(slot - 1), color, clear))
            {
                _client.SelectSlot(slot, color, errorCallback, clear);
            }
        }

        public void ProcessRoomHistory(Action<List<RoomEventInfo>> callback, Action errorCallback)
        {
            _client.ProcessRoomHistory(callback, errorCallback);
        }

        public void DumpDebugInfo()
        {
            _client.DumpDebugInfo();
        }

        private void DoAudioNotification(object sender, GoalUpdateEventInfo goalUpdate)
        {
            if (goalUpdate.Unmarking || !Board.IsAvailable || !Board.IsRevealed)
            {
                return;
            }
            switch(AudioNotificationOn)
            {
                case AudioNotificationCondition.None:
                    break;

                case AudioNotificationCondition.OtherPlayers:
                    if(goalUpdate.Player.Name != RoomNickname)
                    {
                        Controller.Audio.Play(ActiveAudioId);
                    }
                    break;

                case AudioNotificationCondition.OtherColors:
                    if (goalUpdate.Player.Color != RoomColor)
                    {
                        Controller.Audio.Play(ActiveAudioId);
                    }
                    break;

                case AudioNotificationCondition.AllGoals:
                    Controller.Audio.Play(ActiveAudioId);
                    break;
            }
        }

        private void RevealOnOthersReveal(object sender, CardRevealedEventInfo revealedInfo)
        {
            if (Controller.GlobalSettings.RevealCardWhenOthersReveal)
            {
                Controller.RevealCard();
            }
        }

        private void ClientTriggeredBoardUpdate(object sender, ClientBoardUpdateInfo info)
        {
            Controller.BoardUpdate();
            if(info.NeedsConditionReset)
            {
                GoalCompletionTracker.ClearFinishedGoals();
            }
        }

        public void SetDisplaySquaresSelector(Func<List<Square>, List<Square>> selector)
        {
            Board.SetDisplaySquaresSelector(selector);
        }

        public void SetDefaultDisplaySquaresSelector()
        {
            Board.SetDefaultDisplaySquaresSelector();
        }
    }
}
