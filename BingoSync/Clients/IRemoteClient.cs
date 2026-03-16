using BingoSync.Clients.EventInfoObjects;
using BingoSync.CustomGoals;
using BingoSync.Sessions;
using System;
using System.Collections.Generic;

namespace BingoSync.Clients
{
    public interface IRemoteClient
    {
        public event EventHandler<CardRevealedEventInfo> CardRevealedBroadcastReceived;
        public event EventHandler<ChatMessageEventInfo> ChatMessageReceived;
        public event EventHandler<GoalUpdateEventInfo> GoalUpdateReceived;
        public event EventHandler<NewCardEventInfo> NewCardReceived;
        public event EventHandler<PlayerColorChangeEventInfo> PlayerColorChangeReceived;
        public event EventHandler<PlayerConnectionEventInfo> PlayerConnectedBroadcastReceived;
        public event EventHandler<RoomSettings> RoomSettingsReceived;
        public event EventHandler<ClientStateUpdateInfo> ConnectionStateChanged;

        public event EventHandler<ClientBoardUpdateInfo> NeedBoardUpdate;

        public string PlayerUUID { get; }

        public void SetBoard(BingoBoard board);
        public void DumpDebugInfo();
        public void Update();
        public ClientState GetState();
        public void JoinRoom(string roomID, string nickname, string password, Colors color, Action<Exception> callback);
        public void NewCard(List<BingoGoal> board, bool lockout = true, bool hideCard = true);
        public void RevealCard();
        public void SendChatMessage(string text);
        public void SelectSlot(int slot, Colors color, Action errorCallback, bool clear = false);
        public void ExitRoom(Action callback);
        public void ProcessRoomHistory(Action<List<RoomEventInfo>> callback, Action errorCallback);
    }
}
