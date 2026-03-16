using BingoSync.CustomVariables.Rando;
using BingoSync.CustomVariables;
using ItemChanger;
using Modding;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Threading.Tasks;
using MonoMod.Utils;
using System.Collections;
using UnityEngine;

namespace BingoSync
{
    internal class Hooks
    {
        public static void Setup()
        {
            // General
            ModHooks.SetPlayerBoolHook += UpdateBoolInternal;
            ModHooks.SetPlayerIntHook += UpdateIntInternal;

            // Fountain Fragment
            ModHooks.SetPlayerIntHook += FountainFragment.CheckCollected;

            // GeoSpent
            On.GeoCounter.TakeGeo += GeoSpent.UpdateGeoSpent;
            On.GeoCounter.Update += GeoSpent.UpdateGeoText;

            // Tolls
            On.GeoCounter.TakeGeo += Tolls.UpdateTolls;

            // Grubs
            ModHooks.SetPlayerIntHook += Grubs.CheckIfGrubWasSaved;

            // Myla
            ModHooks.SetPlayerIntHook += Myla.CheckIfMylaWasKilled;

            // Revek
            On.HeroController.NailParry += Revek.CheckParry;
            On.HeroController.EnterScene += Revek.EnterRoom;

            // Lifts
            ModHooks.SetPlayerBoolHook += Lifts.CheckIfLiftWasUsed;

            // Jiji
            ModHooks.SetPlayerBoolHook += Jiji.CheckIfKilledShadeInJijis;

            // Dialogue
            On.DialogueBox.StartConversation += Dialogue.StartConversation;

            // Hot Springs
            ModHooks.SetPlayerIntHook += HotSprings.CheckBath;

            // Hive Shard
            ModHooks.SetPlayerBoolHook += HiveShard.CheckIfHiveShardWasCollected;

            // Tram
            ModHooks.SetPlayerIntHook += Tram.CheckIfStationWasVisited;

            // Unique Enemies
            ModHooks.OnReceiveDeathEventHook += UniqueEnemies.CheckIfUniqueEnemyWasKilled;
            On.ScuttlerControl.Hit += UniqueEnemies.HitLightseed;
            On.HealthManager.TakeDamage += UniqueEnemies.KillGulkaWithSpikeBall;
            On.HealthManager.TakeDamage += UniqueEnemies.OomasKilledWithMinions_GlowingWomb;
            On.SetHP.OnEnter += UniqueEnemies.OomasKilledWithMinions_Weaversong_Grimmchild;

            // Giant Geo Egg
            On.PlayMakerFSM.OnEnable += GiantGeoEgg.CreateGiantGeoRockTrigger;

            // Marissa
            On.PlayMakerFSM.OnEnable += Marissa.CreateMarissaKilledTrigger;

            // Stag
            On.PlayMakerFSM.OnEnable += Stag.CreateStagTravelTrigger;

            // BreakableFloors
            On.PlayMakerFSM.OnEnable += BreakableFloors.CreateBreakableFloorsTrigger;

            // Oro Training Dummy
            On.PlayMakerFSM.OnEnable += OroTrainingDummy.CreateOroTrainingDummyTrigger;

            // Millibelle
            On.PlayMakerFSM.OnEnable += Millibelle.CreateMillibelleHitTrigger;

            // Chests
            On.PlayMakerFSM.OnEnable += Chests.CreateChestOpenTrigger;

            // Switches
            On.PlayMakerFSM.OnEnable += Switches.CreateSwitchOpenTrigger;

            // Benches
            On.PlayMakerFSM.OnEnable += Benches.CreateBenchTrigger;

            // Tiso
            On.PlayMakerFSM.OnEnable += Tiso.CreateTisoShieldTrigger;

            // Telescope
            On.PlayMakerFSM.OnEnable += Telescope.CreateTelescopeTrigger;

            // Shade Gates
            On.PlayMakerFSM.OnEnable += ShadeGates.CreateShadeGateTrigger;

            // Dream Nail Dialogue
            On.PlayMakerFSM.OnEnable += DreamNailDialogue.CreateDreamNailDialogueTrigger;

            // Lore Tablets
            On.PlayMakerFSM.OnEnable += LoreTablets.CreateLoreTabletTrigger;

            // Nail Arts
            On.PlayMakerFSM.OnEnable += NailArts.CreateNailArtsTrigger;

            // Spa Gladiator
            On.PlayMakerFSM.OnEnable += SpaGladiator.CreateSplashedTrigger;

            // Eternal Ordeal
            On.PlayMakerFSM.OnEnable += EternalOrdeal.CreateCounterTrigger;

            // Void Pool
            On.PlayMakerFSM.OnEnable += VoidPool.CreateVoidPoolTrigger;

            // Shinies
            On.PlayMakerFSM.OnEnable += Shinies.CreateTrinketTrigger;

            // City Gate
            On.PlayMakerFSM.OnEnable += CityGate.CreateCityGateOpenedTrigger;

            // Scenes
            On.HeroController.EnterScene += Scenes.EnterRoom;

            // Charms
            ModHooks.SetPlayerBoolHook += Charms.CheckEquippedCharms;

            // Bow
            ModHooks.HeroUpdateHook += Bow.BowToNPC;

            // NailHit
            ModHooks.SlashHitHook += NailHit.ProcessNailHit;

            // Rando
            AbstractItem.AfterGiveGlobal += Checks.AfterGiveItem;
            AbstractPlacement.OnVisitStateChangedGlobal += Checks.PlacementStateChange;

            // Menu
            On.UIManager.ContinueGame += ContinueGame;
            On.UIManager.StartNewGame += StartNewGame;
            On.UIManager.FadeInCanvasGroup += FadeIn;
            On.UIManager.FadeOutCanvasGroup += FadeOut;

            var _hook = new ILHook
            (
                typeof(DreamPlant).GetMethod("CheckOrbs", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(),
                DreamTrees.TrackDreamTrees
            );

        }

        private static bool UpdateBoolInternal(string name, bool orig)
        {
            GoalCompletionTracker.UpdateBoolean(name, orig);
            return orig;
        }

        private static int UpdateIntInternal(string name, int current)
        {
            var previous = PlayerData.instance.GetIntInternal(name);
            GoalCompletionTracker.UpdateInteger(name, previous, current);
            return current;
        }

        private static IEnumerator FadeOut(On.UIManager.orig_FadeOutCanvasGroup orig, UIManager self, CanvasGroup cg)
        {
            if (cg.name == "MainMenuScreen")
            {
                Controller.MenuIsVisible = false;
                Controller.IsOnMainMenu = false;
                Controller.RefreshGenerationButtonEnabled();
            }
            return orig(self, cg);
        }

        private static IEnumerator FadeIn(On.UIManager.orig_FadeInCanvasGroup orig, UIManager self, CanvasGroup cg)
        {
            if (cg.name == "MainMenuScreen")
            {
                Controller.MenuIsVisible = true;
                Controller.IsOnMainMenu = true;
                Controller.RefreshGenerationButtonEnabled();
            }
            return orig(self, cg);
        }

        private static void ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
        {
            Controller.MenuIsVisible = false;
            Controller.IsOnMainMenu = false;
            Controller.RefreshGenerationButtonEnabled();
            if (Controller.GlobalSettings.RevealCardOnGameStart)
            {
                Controller.RevealCard();
            }
            Task.Run(() => {
                Checks.GetRandomizedPlacements();
            });
            orig(self);
        }

        private static void StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            Controller.MenuIsVisible = false;
            Controller.IsOnMainMenu = false;
            Controller.RefreshGenerationButtonEnabled();
            if (Controller.GlobalSettings.RevealCardOnGameStart)
            {
                Controller.RevealCard();
            }
            Task.Run(() => {
                Checks.GetRandomizedPlacements();
            });
            orig(self, permaDeath, bossRush);
        }
    }
}
