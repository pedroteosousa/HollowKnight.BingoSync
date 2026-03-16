using BingoSync.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BingoSync.CustomGoals
{
    [Serializable]
    [JsonObject("CustomGameMode")]
    public class CustomGameMode : GameMode
    {
        [JsonProperty("GameModeName")]
        public string InternalName
        {
            get
            {
                return base.GetDisplayName();
            }
            set
            {
                SetName(value);
            }
        }
        [JsonProperty("GoalGroups")]
        private readonly List<GoalGroup> goalSettings;

        public CustomGameMode(string name, Dictionary<string, BingoGoal> goals, List<GoalGroup> loadedGoalSettings = null) 
            : base(name, goals)
        {
            if (loadedGoalSettings != null)
            {
                goalSettings = loadedGoalSettings;
            }
            else
            {
                goalSettings = GameModesManager.CreateDefaultCustomSettings();
            }
        }

        public void AddGoalGroupToSettings(GoalGroup goalGroup)
        {
            goalSettings.Add(goalGroup);
        }

        public List<GoalGroup> GetGoalSettings()
        {
            return goalSettings;
        }

        private void SetGoalsFromSettings()
        {
            Dictionary<string, BingoGoal> goals = [];
            foreach (GoalGroup goalGroup in goalSettings)
            {
                if (!GameModesManager.GoalGroupExists(goalGroup.Name))
                {
                    Modding.Logger.Log($"Group \"{goalGroup.Name}\" is not registered, skipping");
                    continue;
                }
                List<string> activeGoals = goalGroup.GetActiveGoals();
                List<BingoGoal> activeBingoGoals = GameModesManager.GetGoalsFromNames(goalGroup.Name, activeGoals);
                foreach(BingoGoal goal in activeBingoGoals)
                {
                    if(goals.ContainsKey(goal.name))
                    {
                        goals[goal.name].exclusions.AddRange(goal.exclusions);
                    }
                    else
                    {
                        goals[goal.name] = goal;
                    }
                }
            }
            SetGoals(goals);
        }

        public override List<BingoGoal> GenerateBoard(int seed)
        {
            SetGoalsFromSettings();
            return base.GenerateBoard(seed);
        }

        public override string GetDisplayName()
        {
            return base.GetDisplayName() + "*";
        }
    }
}
