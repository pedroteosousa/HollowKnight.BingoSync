namespace BingoSync.Clients.EventInfoObjects
{
    public class RoomSettings
    {
        public bool HideCard { get; set; }
        public bool IsLockout { get; set; }
        public string GameName { get; set; }
        public int GameId { get; set; }
        public string VariantName { get; set; }
        public int VariantId { get; set; }
        public int Seed { get; set; }
    }
}
