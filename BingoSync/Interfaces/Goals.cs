using System.Collections.Generic;
using System.IO;
using BingoSync.CustomGoals;

namespace BingoSync
{
    public static class Goals
    {
        /// <summary>
        /// Generally recommended to use ProcessGoalsStream instead, reads a goals json, starts tracking and marking the goals, and also returns them for custom gamemodes.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static Dictionary<string, BingoGoal> ProcessGoalsFile(string filepath)
        {
            return GoalCompletionTracker.ProcessGoalsFile(filepath);
        }

        /// <summary>
        /// Reads a goals json, starts tracking and marking the goals, and also returns them for custom gamemodes.
        /// </summary>
        /// <param name="goalstream"></param>
        /// <returns></returns>
        public static Dictionary<string, BingoGoal> ProcessGoalsStream(Stream goalstream)
        {
            return GoalCompletionTracker.ProcessGoalsStream(goalstream);
        }

        /// <summary>
        /// Returns all the vanilla goals (with their exclusions set to be identiacal to bingosync.com) for use in custom gamemodes.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, BingoGoal> GetVanillaGoals()
        {
            return GameModesManager.GetVanillaGoals();
        }

        /// <summary>
        /// Returns all the item rando goals (with their exclusions set to be identiacal to bingosync.com) for use in custom gamemodes.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, BingoGoal> GetItemRandoGoals()
        {
            return GameModesManager.GetItemRandoGoals();
        }

        /// <summary>
        /// Returns the goals in the given goal group if it exists, an empty dictionary otherwise.
        /// </summary>
        /// <param name="groupName"></param>
        public static Dictionary<string, BingoGoal> GetGoalsByGroupName(string groupName)
        {
            return GameModesManager.GetGoalsByGroupName(groupName);
        }

        /// <summary>
        /// Allows players to select goals from this list for custom profiles.
        /// </summary>
        /// <param name="groupName">The name under which the goal group shows up in the settings</param>
        /// <param name="goals">The goals in the group</param>
        public static void RegisterGoalsForCustom(string groupName, Dictionary<string, BingoGoal> goals)
        {
            GameModesManager.RegisterGoalsForCustom(groupName, goals);
        }

        /// <summary>
        /// Registers a gamemode to be available on the generation UI.
        /// </summary>
        /// <param name="gameMode"></param>
        public static void AddGameMode(GameMode gameMode)
        {
            GameModesManager.AddGameMode(gameMode);
        }
    }
}
