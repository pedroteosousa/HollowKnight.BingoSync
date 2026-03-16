using BingoSync.CustomGoals;
using BingoSync.Helpers;
using BingoSync.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BingoSync
{
    internal class GoalCompletionTracker
    {
        private static readonly Dictionary<string, BingoSquare> AllKnownSquaresByName = [];
        private static readonly Dictionary<string, List<BingoSquare>> GoalsByVariable = [];
        private static readonly Dictionary<string, List<BingoSquare>> GoalsByRuleset = [];

        private static Action<string> Log;
        public static SaveSettings Variables { get; set; }

        public class InternalGoalUpdate
        {
            public string Name { get; set; }
            public bool Clear { get; set; }
            public bool IsItemSyncUpdate { get; set; }
        }

        public static event EventHandler<InternalGoalUpdate> OnGoalCompletionChanged;

        public static void Setup(Action<string> log)
        {
            Log = log;

            string[] resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (!resource.StartsWith("BingoSync.Resources.Squares"))
                {
                    continue;
                }
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
                using StreamReader reader = new(s);
                using JsonTextReader jsonReader = new(reader);
                JsonSerializer ser = new();
                List<BingoSquare> squares = ser.Deserialize<List<BingoSquare>>(jsonReader);
                foreach (BingoSquare square in squares)
                {
                    AllKnownSquaresByName[square.Name] = square;
                }
            }
        }

        public static bool IsGoalMarkedByName(string name)
        {
            if(!AllKnownSquaresByName.TryGetValue(name, out BingoSquare square))
            {
                return false;
            }
            return square.Condition.Solved;
        }

        internal static void SetupDictionaries()
        {
            foreach (BingoSquare square in AllKnownSquaresByName.Values)
            {
                AddAllVariablesToTrack(square, square.Condition);
                if(!GoalsByRuleset.ContainsKey(square.Ruleset))
                {
                    GoalsByRuleset[square.Ruleset] = [];
                }
                GoalsByRuleset[square.Ruleset].Add(square);
            }
        }

        private static void AddAllVariablesToTrack(BingoSquare square, Condition condition)
        {
            switch (condition.Type)
            {
                case ConditionType.Or:
                case ConditionType.And:
                case ConditionType.Some:
                    foreach (Condition subcondition in condition.Conditions)
                    {
                        AddAllVariablesToTrack(square, subcondition);
                    }
                    break;
                case ConditionType.Bool:
                case ConditionType.Int:
                default:
                    string variableName = condition.VariableName;
                    if (!GoalsByVariable.ContainsKey(variableName))
                    {
                        GoalsByVariable[variableName] = [];
                    }
                    if(!GoalsByVariable[variableName].Contains(square)) {
                        GoalsByVariable[variableName].Add(square);
                    }
                    break;
            }
        }

        public static Dictionary<string, BingoGoal> ProcessGoalsFile(string filepath)
        {
            using FileStream filestream = File.Open(filepath, FileMode.Open);
            return ProcessGoalsStream(filestream);
        }

        public static Dictionary<string, BingoGoal> ProcessGoalsStream(Stream goalstream)
        {
            Dictionary<string, BingoGoal> goals = [];
            List<BingoSquare> squares = [];

            using (StreamReader reader = new(goalstream))
            using (JsonTextReader jsonReader = new(reader))
            {
                JsonSerializer ser = new();
                var squaresSer = ser.Deserialize<List<BingoSquare>>(jsonReader);
                squares.AddRange(squaresSer);
            }

            foreach (BingoSquare square in squares)
            {
                goals.Add(square.Name, new BingoGoal(square.Name, []));
                AllKnownSquaresByName[square.Name] = square;
            }
            return goals;
        }

        public static bool GetBoolean(string name)
        {
            if (Variables == null)
            {
                return false;
            }

            if (Variables.Booleans.TryGetValue(name, out bool current))
            {
                return current;
            }
            return false;
        }

        public static void UpdateBoolean(string name, bool value)
        {
            Variables.Booleans[name] = value;
            AfterVariableUpdated(name);
        }

        public static int GetInteger(string name)
        {
            if (Variables == null)
            {
                return 0;
            }

            if (Variables.Integers.TryGetValue(name, out int current))
            {
                return current;
            }
            return 0;
        }

        public static void UpdateInteger(string name, int current)
        {
            if (Variables == null)
            {
                return;
            }

            if (!Variables.Integers.TryGetValue(name, out int previous))
            {
                previous = 0;
            }
            UpdateInteger(name, previous, current);
        }

        public static void UpdateInteger(string name, int previous, int current)
        {
            if (Variables == null)
            {
                return;
            }

            var added = Math.Max(0, current - previous);
            var removed = Math.Max(0, previous - current);
            if (!Variables.Integers.ContainsKey(name))
            {
                Variables.Integers.Add(name, current);
                Variables.IntegersTotalAdded.Add(name, added);
                Variables.IntegersTotalRemoved.Add(name, removed);
            }
            else
            {
                Variables.Integers[name] = current;
                Variables.IntegersTotalAdded[name] += added;
                Variables.IntegersTotalRemoved[name] += removed;
            }
            AfterVariableUpdated(name);
        }
        
        private static void AfterVariableUpdated(string variableName)
        {
            if(!GoalsByVariable.TryGetValue(variableName, out List<BingoSquare> squares))
            {
                return;
            }
            foreach (BingoSquare square in squares)
            {
                bool wasSolved = square.Condition.Solved;
                bool isSolved = IsSolved(square);
                bool shouldUnmark = !isSolved;
                if (wasSolved != isSolved)
                {
                    OnGoalCompletionChanged?.Invoke(null, new InternalGoalUpdate()
                    {
                        Name = square.Name,
                        Clear = shouldUnmark,
                        IsItemSyncUpdate = ItemSyncInterop.IsItemSyncUpdate,
                    });
                }
            }
        }

        internal static void BroadcastAllGoalStates()
        {
            foreach (BingoSquare square in AllKnownSquaresByName.Values)
            {
                OnGoalCompletionChanged?.Invoke(null, new InternalGoalUpdate()
                {
                    Name = square.Name,
                    Clear = !IsSolved(square),
                    IsItemSyncUpdate = ItemSyncInterop.IsItemSyncUpdate,
                });
            }
        }

        private static bool IsSolved(BingoSquare square)
        {
            if (square.Condition.Solved && (!Controller.GlobalSettings.UnmarkGoals || !square.CanUnmark))
                return square.Condition.Solved;
            UpdateCondition(square.Condition);
            return square.Condition.Solved;
        }

        private static void UpdateCondition(Condition condition)
        {
            condition.Solved = false;
            if (condition.Type == ConditionType.Bool)
            {
                Variables.Booleans.TryGetValue(condition.VariableName, out var value);
                if (value == condition.ExpectedValue)
                {
                    condition.Solved = true;
                }
            }
            else if (condition.Type == ConditionType.Int)
            {
                int quantity = 0;
                int current = -1;
                int added = -1;
                int removed = -1;
                if (!Variables.Integers.TryGetValue(condition.VariableName, out current)
                    || !Variables.IntegersTotalAdded.TryGetValue(condition.VariableName, out added)
                    || !Variables.IntegersTotalRemoved.TryGetValue(condition.VariableName, out removed))
                {
                    return;
                }
                switch (condition.State)
                {
                    case BingoRequirementState.Current:
                        quantity = current;
                        break;
                    case BingoRequirementState.Added:
                        quantity = added;
                        break;
                    case BingoRequirementState.Removed:
                        quantity = removed;
                        break;
                }
                if (quantity >= condition.ExpectedQuantity)
                {
                    condition.Solved = true;
                }
            }
            else {
                condition.Conditions.ForEach(UpdateCondition);
                if (condition.Type == ConditionType.Or) {
                    condition.Solved = condition.Conditions.Any(cond => cond.Solved);
                }
                else if (condition.Type == ConditionType.And) {
                    condition.Solved = condition.Conditions.All(cond => cond.Solved);
                }
                else if (condition.Type == ConditionType.Some) {
                    condition.Solved = condition.Conditions.FindAll(cond => cond.Solved).Count >= condition.Amount;
                }
            }
        }

        public static void ClearFinishedGoals() {
            foreach(KeyValuePair<string, BingoSquare> entry in AllKnownSquaresByName)
            {
                ClearCondition(entry.Value.Condition);
            }
        }

        public static void ClearCondition(Condition condition) {
            condition.Solved = false;
            condition.Conditions.ForEach(ClearCondition);
        }
    }

    class BingoSquare
    {
        public string Name = string.Empty;
        public Condition Condition = new();
        public bool CanUnmark = false;
        public string Ruleset = "Default";
    }

    class Condition
    {
        public ConditionType Type = ConditionType.And;
        public int Amount = 0;
        public bool Solved = false;
        public string VariableName = string.Empty;
        public BingoRequirementState State = BingoRequirementState.Current;
        public int ExpectedQuantity = 0;
        public bool ExpectedValue = false;
        public List<Condition> Conditions = [];
    }

    enum ConditionType
    {
        Bool,
        Int,
        Or,
        And,
        Some,
    }

    enum BingoRequirementState
    {
        Current,
        Added,
        Removed,
    }
}
