using System;
using System.Collections.Generic;
using System.Linq;

namespace BingoSync.CustomGoals
{
    public class GameMode(string name, Dictionary<string, BingoGoal> goals)
    {
        private string name = name;
        private Dictionary<string, BingoGoal> goals = goals;

        public Dictionary<string, BingoGoal> GetGoals()
        {
            return goals;
        }

        public void SetGoals(Dictionary<string, BingoGoal> goals)
        {
            this.goals = goals;
        }

        virtual public string GetDisplayName()
        {
            return name;
        }

        protected void SetName(string newName)
        {
            name = newName;
        }

        virtual public List<BingoGoal> GenerateBoard(int seed)
        {
            List<BingoGoal> board = [];
            List<BingoGoal> availableGoals = [.. goals.Values];
            Random r = new(seed);
            while (board.Count < 25)
            {
                if (availableGoals.Count == 0)
                {
                    Modding.Logger.Log("Could not generate board");
                    return GetErrorBoard();
                }
                int index = r.Next(availableGoals.Count);
                BingoGoal proposedGoal = availableGoals[index];
                bool valid = true;
                foreach (BingoGoal existing in board)
                {
                    if (existing.Excludes(proposedGoal) || proposedGoal.Excludes(existing))
                    {
                        valid = false;
                    }
                }
                if (valid)
                {
                    board.Add(proposedGoal);
                }
                availableGoals.Remove(proposedGoal);
            }

            return board;
        }

        public static List<BingoGoal> GetErrorBoard()
        {
            BingoGoal empty = new("-");
            List<BingoGoal> board = [new BingoGoal("Error generating board")];
            for(int i = 0; i < 24; ++i)
            {
                board.Add(empty);
            }
            return board;
        }
    }
}
