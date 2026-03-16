using System.Collections.Generic;

namespace BingoSync.Sessions
{
    public class Square
    {
        public string Name { get; set; }
        public HashSet<Colors> MarkedBy { get; set; }
        public bool Highlighted { get; set; }
        public int GoalIndex { get; set; }
    }
}
