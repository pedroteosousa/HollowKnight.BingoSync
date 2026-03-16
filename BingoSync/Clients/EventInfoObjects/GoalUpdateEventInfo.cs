namespace BingoSync.Clients.EventInfoObjects
{
    public class GoalUpdateEventInfo : RoomEventInfo
    {
        public Colors Color { get; set; }
        public string Goal { get; set; }
        public int Index {  get; set; } 
        public bool Unmarking { get; set; }
    }
}
