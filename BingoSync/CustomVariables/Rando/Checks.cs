using System.Threading.Tasks;
using ItemChanger;

namespace BingoSync.CustomVariables.Rando
{
    internal static class Checks
    {
        public static void AfterGiveItem(ReadOnlyGiveEventArgs args)
        {
            var variableName = $"gotCheck_{args.Placement.Name}";
            GoalCompletionTracker.UpdateBoolean(variableName, true);

            if (args.Placement.AllObtained())
            {
                var allObtainedVariableName = $"allObtained_{args.Placement.Name}";
                GoalCompletionTracker.UpdateBoolean(allObtainedVariableName, true);
            }
        }

        public static void PlacementStateChange(VisitStateChangedEventArgs args)
        {
            if (args.NewFlags == VisitState.None) return;
            var variableName = $"checked_{args.Placement.Name}";
            GoalCompletionTracker.UpdateBoolean(variableName, true);
        }

        public static void GetRandomizedPlacements() {
            RetryHelper.RetryWithExponentialBackoff(() => {
                var placementKeys = ItemChanger.Internal.Ref.Settings.Placements.Keys;
                if (placementKeys.Count == 0) {
                    return Task.FromException(new System.Exception("no placement keys"));
                }
                foreach (var placement in placementKeys)
                {
                    var variableName = $"randomized_{placement}";
                    GoalCompletionTracker.UpdateBoolean(variableName, true);
                }
                return Task.CompletedTask;
            }, 10, nameof(GetRandomizedPlacements));
        }
    }
}

