using System.Reflection;
using UnityEngine;

namespace BingoSync.CustomVariables
{
    internal static class UniqueEnemies
    {
        public static void CheckIfUniqueEnemyWasKilled(EnemyDeathEffects enemyDeathEffects, bool _0, ref float? _1, ref bool _2, ref bool _3, ref bool _4)
        {
            var field = typeof(EnemyDeathEffects).GetField("playerDataName", BindingFlags.Instance | BindingFlags.NonPublic);
            var enemyName = field.GetValue(enemyDeathEffects);
            var scene = GameManager.instance.GetSceneNameString();
            var variableName = $"killed_{enemyDeathEffects.name}_{scene}";
            var alreadyKilled = GoalCompletionTracker.GetBoolean(variableName);
            GoalCompletionTracker.UpdateBoolean(variableName, true);
            if (alreadyKilled) {
                return;
            }
            var countVariableName = $"killedUnique_{enemyName}";
            var uniqueCount = GoalCompletionTracker.GetInteger(countVariableName);
            GoalCompletionTracker.UpdateInteger(countVariableName, uniqueCount + 1);
        }

        public static void HitLightseed(On.ScuttlerControl.orig_Hit orig, ScuttlerControl self, HitInstance damageInstance)
        {
            orig(self, damageInstance);
            if (!self.name.StartsWith("Orange Scuttler")) return;
            GoalCompletionTracker.UpdateBoolean("killedLightseed", true);
        }

        public static void KillGulkaWithSpikeBall(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);
            if (self.gameObject == null || !self.gameObject.name.StartsWith("Plant Turret")) return;
            if (hitInstance.Source == null || !hitInstance.Source.name.StartsWith("Spike Ball")) return;
            if (!self.GetIsDead()) return;
            GoalCompletionTracker.UpdateBoolean("killGulkaWithSpikeBall", true);
        }

        private static bool IsOoma(string objectName)
        {
            return objectName.StartsWith("Jellyfish") && !objectName.Contains("Baby");
        }

        private static void UpdateOomasKilledWithMinionCharm(string objectName)
        {
            var roomName = GameManager.instance.GetSceneNameString();
            var variableName = $"killedOomaWithMinionCharm_{objectName}_{roomName}";
            var alreadyKilled = GoalCompletionTracker.GetBoolean(variableName);
            GoalCompletionTracker.UpdateBoolean(variableName, true);
            if (alreadyKilled) {
                return;
            }
            var countVariableName = "killsOomaWithMinionCharm";
            var uniqueCount = GoalCompletionTracker.GetInteger(countVariableName);
            GoalCompletionTracker.UpdateInteger(countVariableName, uniqueCount + 1);
        }

        public static void OomasKilledWithMinions_GlowingWomb(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
        {
            orig(self, hitInstance);
            if (!IsOoma(self?.gameObject?.name)) return;
            if (hitInstance.Source == null || !hitInstance.Source.name.StartsWith("Damager")) return;
            if (!self.GetIsDead()) return;
            UpdateOomasKilledWithMinionCharm(self.gameObject.name);
        }

        // Weaversong and Grimmchild FSMs do not trigger the TakeDamage hook
        public static void OomasKilledWithMinions_Weaversong_Grimmchild(On.SetHP.orig_OnEnter orig, SetHP self)
        {
            orig(self);
            GameObject obj = self?.target.GetSafe(self);
            if (!IsOoma(obj?.name)) return;
            if (self?.Fsm?.Owner?.gameObject?.name != "Enemy Damager") return;
            if (self.hp.Value > 0) return;
            UpdateOomasKilledWithMinionCharm(obj?.name);
        }
    }
}
