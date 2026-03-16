using System.Collections.Generic;
using UnityEngine;

namespace BingoSync.CustomVariables
{
    internal static class NailHit
    {
        private static readonly List<NailTarget> NailTargets = new List<NailTarget>
        {
            new NailTarget
            {
                objectName = "god shrine stuff",
                variableSuffix = "TrilobiteStatue"
            }
        };

        public static void ProcessNailHit(Collider2D otherCollider, GameObject slash)
        {
            NailTargets.ForEach(target =>
            {
                if (otherCollider.gameObject == null || otherCollider.gameObject.name != target.objectName) return;
                var variableName = $"nailHit{target.variableSuffix}";
                GoalCompletionTracker.UpdateBoolean(variableName, true);
            });
        }
    }

    internal class NailTarget
    {
        public string objectName;
        public string variableSuffix;
    };
}
