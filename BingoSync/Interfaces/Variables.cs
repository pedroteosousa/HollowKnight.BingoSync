using System;
using System.Collections.Generic;

namespace BingoSync
{
    public static class Variables
    {
        private static Action<string> Log;
        private static readonly HashSet<string> trackedVariables = [];

        public static void Setup(Action<string> log)
        {
            Log = log;
        }

        /// <summary>
        /// Starts tracking the given variable. Any access to that variable through the Variables interface will be logged.
        /// </summary>
        /// <param name="variableName"></param>
        public static void Track(string variableName)
        {
            trackedVariables.Add(variableName);
        }

        /// <summary>
        /// Stops tracking the given variable.
        /// </summary>
        /// <param name="variableName"></param>
        public static void Untrack(string variableName)
        {
            trackedVariables.Remove(variableName);
        }

        /// <summary>
        /// Reads the value of a variable of type int from the goal-progress-tracker.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static int GetInteger(string variableName)
        {
            int value = GoalCompletionTracker.GetInteger(variableName);
            if (trackedVariables.Contains(variableName))
            {
                Log($"GetInteger: {variableName} = {value}");
            }
            return value;
        }

        /// <summary>
        /// Sets the value of a variable of type int from the goal-progress-tracker.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void UpdateInteger(string variableName, int value)
        {
            if (trackedVariables.Contains(variableName))
            {
                Log($"UpdateInteger: {variableName} = {value}");
            }
            GoalCompletionTracker.UpdateInteger(variableName, value);
        }

        /// <summary>
        /// Alias for UpdateInteger.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        public static void SetInteger(string variableName, int value)
        {
            UpdateInteger(variableName, value);
        }

        /// <summary>
        /// Changes the value of a variable of type int by some amount.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="amount"></param>
        public static void Increment(string variableName, int amount = 1)
        {
            SetInteger(variableName, GetInteger(variableName) + amount);
        }

        /// <summary>
        /// Reads the value of a variable of type bool from the goal-progress-tracker.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static bool GetBoolean(string variableName)
        {
            bool value = GoalCompletionTracker.GetBoolean(variableName);
            if (trackedVariables.Contains(variableName))
            {
                Log($"GetBoolean: {variableName} = {value}");
            }
            return value;
        }

        /// <summary>
        /// Sets the value of a variable of type bool from the goal-progress-tracker.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        public static void UpdateBoolean(string variableName, bool value)
        {
            if (trackedVariables.Contains(variableName))
            {
                Log($"UpdateBoolean: {variableName} = {value}");
            }
            GoalCompletionTracker.UpdateBoolean(variableName, value);
        }

        /// <summary>
        /// Alias for UpdateBoolean.
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        public static void SetBoolean(string variableName, bool value)
        {
            UpdateBoolean(variableName, value);
        }
    }
}
