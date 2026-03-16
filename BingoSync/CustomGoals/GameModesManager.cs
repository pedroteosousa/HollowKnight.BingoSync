using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BingoSync.Settings;
using Newtonsoft.Json;
using UnityEngine;

namespace BingoSync.CustomGoals
{
    internal static class GameModesManager
    {
        private static readonly string CustomGameModesPath = Path.Combine(Path.Combine(Application.persistentDataPath, "BingoSync"), "CustomProfiles");
        private static Action<string> Log;
        private static readonly List<GameMode> gameModes = [];
        private static readonly Dictionary<string, BingoGoal> vanillaGoals = [];
        private static readonly Dictionary<string, BingoGoal> itemRandoGoals = [];
        private static readonly Dictionary<string, List<BingoGoal>> goalGroupDefinitions = [];

        public static readonly List<CustomGameMode> CustomGameModes = [];

        public static void DumpDebugInfo()
        {
            Log($"GameModesManager");
            foreach (string groupname in goalGroupDefinitions.Select(group => group.Key))
            {
                Log($"\tGoalGroup \"{groupname}\"");
            };
            foreach (string gamemode in gameModes.Select(gamemode => gamemode.GetDisplayName()))
            {
                Log($"\tGameMode \"{gamemode}\"");
            };
        }

        public static void Setup(Action<string> log)
        {
            Log = log;
            SetupVanillaGoals();
            SetupItemRandoGoals();
            gameModes.Add(new GameMode("Vanilla", vanillaGoals));
            gameModes.Add(new GameMode("Item Rando", itemRandoGoals));
            RegisterGoalsForCustom("Vanilla", vanillaGoals);
            RegisterGoalsForCustom("Item Rando", itemRandoGoals);
        }

        public static void AddGameMode(GameMode gameMode)
        {
            gameModes.Add(gameMode);
        }

        public static GameMode FindGameModeByDisplayName(string name)
        {
            return gameModes.Find(gameMode => gameMode.GetDisplayName() == name);
        }

        public static Dictionary<string, BingoGoal> GetGoalsByGroupName(string groupName)
        {
            Dictionary<string, BingoGoal> goals = [];
            if (goalGroupDefinitions.ContainsKey(groupName))
            {
                foreach(BingoGoal goal in goalGroupDefinitions[groupName])
                {
                    goals[goal.name] = goal;
                }
            }
            return goals;
        }

        public static Dictionary<string, BingoGoal> GetVanillaGoals()
        {
            return GetGoalsByGroupName("Vanilla");
        }

        public static Dictionary<string, BingoGoal> GetItemRandoGoals()
        {
            return GetGoalsByGroupName("Item Rando");
        }

        public static void RefreshCustomGameModes()
        {
            gameModes.RemoveAll(gameMode => gameMode.GetType() == typeof(CustomGameMode));
            foreach (CustomGameMode gameMode in CustomGameModes)
            {
                SychronizeGoalGroups(gameMode);
                AddGameMode(gameMode);
            }
        }

        public static void RenameGameModeFile(string oldName, string newName)
        {
            if(!File.Exists(MakeFilepathForGameModeName(oldName)) || File.Exists(MakeFilepathForGameModeName(newName)))
            {
                return;
            }
            File.Move(MakeFilepathForGameModeName(oldName), MakeFilepathForGameModeName(newName));
        }

        public static void DeleteGameModeFile(string name)
        {
            if (!File.Exists(MakeFilepathForGameModeName(name)))
            {
                return;
            }
            File.Delete(MakeFilepathForGameModeName(name));
        }

        public static void SaveCustomGameModesToFiles()
        {
            CreateFolderIfMissing();
            foreach (CustomGameMode gameMode in CustomGameModes)
            {
                File.WriteAllText(MakeFilepathForGameModeName(gameMode.InternalName), JsonConvert.SerializeObject(gameMode));
            }
        }

        public static void LoadCustomGameModesFromFiles()
        {
            CreateFolderIfMissing();
            string[] paths = Directory.GetFiles(CustomGameModesPath, "*.json");
            foreach (string path in paths)
            {
                CustomGameMode gameMode = LoadCustomGameModeFromFile(path);
                if (gameMode != null)
                {
                    CustomGameModes.Add(gameMode);
                }
            }
        }

        private static CustomGameMode LoadCustomGameModeFromFile(string filepath)
        {
            if (filepath == null || !File.Exists(filepath))
            {
                return null;
            }
            CustomGameMode gameMode = JsonConvert.DeserializeObject<CustomGameMode>(File.ReadAllText(filepath));
            File.Move(filepath, MakeFilepathForGameModeName(gameMode.InternalName));
            return gameMode;
        }

        private static string MakeFilepathForGameModeName(string gameModeName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                gameModeName = gameModeName.Replace(c, '_');
            }
            return Path.Combine(CustomGameModesPath, gameModeName + ".json");
        }

        private static void CreateFolderIfMissing()
        {
            if (!Directory.Exists(CustomGameModesPath))
            {
                Directory.CreateDirectory(CustomGameModesPath);
            }
        }

        private static void SychronizeGoalGroups(CustomGameMode gameMode)
        {
            foreach (var group in goalGroupDefinitions)
            {
                string groupName = group.Key;
                if(gameMode.GetGoalSettings().FindIndex(goalGroup => goalGroup.Name == groupName) == -1)
                {
                    gameMode.AddGoalGroupToSettings(CreateDefaultSettingsForGroup(groupName));
                }
            }
        }

        public static void RegisterGoalsForCustom(string groupName, Dictionary<string, BingoGoal> goals)
        {
            goalGroupDefinitions[groupName] = [.. goals.Values];
        }

        internal static List<BingoGoal> GetGoalsFromNames(string groupName, List<string> goalNames)
        {
            List<BingoGoal> goals = [];
            if (GoalGroupExists(groupName))
            {
                goals = goalGroupDefinitions[groupName].FindAll(goal => goalNames.Contains(goal.name));
            }
            return goals;
        }

        public static List<GoalGroup> CreateDefaultCustomSettings()
        {
            List<GoalGroup> defaultSettings = [];
            foreach(var group in goalGroupDefinitions)
            {
                defaultSettings.Add(CreateDefaultSettingsForGroup(group.Key));
            }
            return defaultSettings;
        }

        private static GoalGroup CreateDefaultSettingsForGroup(string groupName)
        {
            if (!goalGroupDefinitions.ContainsKey(groupName))
            {
                Log($"Couldn't create default settings for unknown group \"{groupName}\"");
                return new GoalGroup("Unknown Group", []);
            }
            return new GoalGroup(groupName, [.. goalGroupDefinitions[groupName].Select(goal => goal.name)]);
        }

        public static bool GoalGroupExists(string groupName)
        {
            return goalGroupDefinitions.ContainsKey(groupName);
        }

        public static List<string> GameModeNames()
        {
            List<string> names = [];
            foreach (GameMode gameMode in gameModes)
            {
                names.Add(gameMode.GetDisplayName());
            }
            return names;
        }

        public static void Generate()
        {
            (int seed, bool isCustomSeed) = Controller.GetCurrentSeed();
            string lockoutString = Controller.MenuIsLockout ? "lockout" : "non-lockout";
            string isCustomSeedString = isCustomSeed ? "set" : "random";
            Controller.ActiveSession.SendChatMessage($"Generating {Anify(Controller.ActiveGameMode)} board in {lockoutString} mode with {isCustomSeedString} seed {seed}");
            List<BingoGoal> board = GameMode.GetErrorBoard();
            if (Controller.ActiveGameMode != string.Empty)
            {
                board = FindGameModeByDisplayName(Controller.ActiveGameMode).GenerateBoard(seed);
            }
            Controller.ActiveSession.NewCard(board, Controller.MenuIsLockout);
        }

        private static void SetupVanillaGoals()
        {
            List<string> goals = ["Abyss Shriek", "All Grubs: Greenpath (4) + Fungal (2)", "All Grubs: Xroads (5) + Fog Canyon (1)", "Break the 420 geo rock in Kingdom's Edge", "Broken Vessel", "Buy 6 map pins from Iselda (All but two)", "Buy 6 maps", "Buy 8 map pins from Iselda (All)", "Collect 1 Arcane Egg", "Collect 3 King's Idols", "Collect 500 essence", "Collector", "Colosseum 1", "Complete 4 full dream trees", "Crystal Guardian 1", "Crystal Guardian 2", "Crystal Heart", "Cyclone Slash", "Dash Slash", "Deep Focus + Quick Focus", "Defeat Colosseum Zote", "Descending Dark", "Desolate Dive", "Dream Gate", "Dream Nail", "Dream Wielder", "Dung Defender", "Elder Hu", "Failed Champion", "False Knight + Brooding Mawlek", "Flukemarm", "Flukenest", "Fragile Heart, Greed, and Strength", "Galien", "Give Flower to Elderbug", "Glowing Womb + Grimmchild", "Goam and Garpede Journal Entries", "Gorb", "Great Slash", "Grubsong", "Have 1500 geo in the bank", "Have 2 Pale Ore", "Have 4 Rancid Eggs", "Have 5 Hallownest Seals", "Have 5 Wanderer's Journals", "Heavy Blow + Steady Body", "Herrah", "Hive Knight", "Hiveblood", "Hornet 2", "Howling Wraiths", "Interact with 5 Cornifer locations", "Isma's Tear", "Kill 2 Soul Warriors", "Kill 4 Mimics", "Kill 6 different Stalking Devouts", "Kill Myla", "Kill your shade in Jiji's Hut", "Lifeblood Heart + Joni's Blessing", "Longnail + MoP", "Lost Kin", "Lumafly Lantern", "Lurien", "Mantis Lords", "Markoth", "Marmu", "Mask Shard  in the Hive", "Monarch Wings", "Monomon", "Nail 2", "Nail 3", "No Eyes", "Nosk", "Obtain 1 extra mask", "Obtain 1 extra soul vessel", "Obtain 2 extra masks", "Obtain 3 extra notches", "Obtain fountain vessel fragment", "Pale Lurker", "Parry Revek 3 times without dying (Glade of Hope Guard)", "Pay for 6 tolls", "Pick up the Love Key", "Quick Slash", "Rescue Bretta + Sly", "Rescue Zote in Deepnest", "Save 15 grubs", "Save 20 grubs", "Save the 3 grubs in Queen's Garden", "Save the 3 grubs in Waterways", "Save the 5 grubs in CoT", "Save the 5 grubs in Deepnest", "Save the 7 grubs in Crystal Peak", "Shade Cloak", "Shade Soul", "Shape of Unn", "Sharp Shadow", "Soul Master", "Soul Tyrant", "Spell Twister + Shaman Stone", "Spend 3000 geo", "Spend 4000 geo", "Spend 5000 geo", "Sprintmaster + Dashmaster", "Stag Nest vessel fragment", "Take a bath in all 4 Hot Springs", "Talk to Bardoon", "Talk to Emilitia (shortcut out of sewers)", "Talk to Hornet at CoT Statue + Herrah", "Talk to Lemm with Crest Equipped", "Talk to Mask Maker", "Talk to Midwife", "Talk to the Fluke Hermit", "Thorns of agony + Baldur Shell + Spore Shroom", "Traitor Lord", "Tram Pass + Visit all 5 Tram Stations", "Unlock Deepnest Stag", "Unlock Hidden Stag Station", "Unlock Queen's Garden Stag", "Unlock Queen's Stag + King's Stag Stations", "Upgrade Grimmchild once", "Use 2 Simple Keys", "Use City Crest + Ride both CoT large elevators", "Uumuu", "Vengefly King + Massive Moss Charger", "Void Tendrils Journal Entry", "Watch Cloth Die", "Watcher Knights", "Weaversong", "Xero"];
            foreach (string goal in goals)
            {
                vanillaGoals.Add(goal, new(goal));
            }
            vanillaGoals["Break the 420 geo rock in Kingdom's Edge"].exclusions = ["Quick Slash"];
            vanillaGoals["Broken Vessel"].exclusions = ["Monarch Wings"];
            vanillaGoals["Buy 6 map pins from Iselda (All but two)"].exclusions = ["Buy 8 map pins from Iselda (All)"];
            vanillaGoals["Buy 8 map pins from Iselda (All)"].exclusions = ["Buy 6 map pins from Iselda (All but two)"];
            vanillaGoals["Collect 1 Arcane Egg"].exclusions = ["Shade Cloak", "Void Tendrils Journal Entry"];
            vanillaGoals["Collect 500 essence"].exclusions = ["Dream Wielder"];
            vanillaGoals["Colosseum 1"].exclusions = ["Defeat Colosseum Zote"];
            vanillaGoals["Defeat Colosseum Zote"].exclusions = ["Colosseum 1", "Rescue Zote in Deepnest"];
            vanillaGoals["Desolate Dive"].exclusions = ["Soul Master"];
            vanillaGoals["Dream Nail"].exclusions = ["Xero"];
            vanillaGoals["Dream Wielder"].exclusions = ["Collect 500 essence"];
            vanillaGoals["Dung Defender"].exclusions = ["Talk to Lemm with Crest Equipped"];
            vanillaGoals["Flukemarm"].exclusions = ["Flukenest"];
            vanillaGoals["Flukenest"].exclusions = ["Flukemarm"];
            vanillaGoals["Have 2 Pale Ore"].exclusions = ["Nail 3"];
            vanillaGoals["Herrah"].exclusions = ["Talk to Hornet at CoT Statue + Herrah"];
            vanillaGoals["Hive Knight"].exclusions = ["Hiveblood", "Mask Shard  in the Hive", "Tram Pass + Visit all 5 Tram Stations"];
            vanillaGoals["Hiveblood"].exclusions = ["Hive Knight", "Mask Shard  in the Hive", "Tram Pass + Visit all 5 Tram Stations"];
            vanillaGoals["Isma's Tear"].exclusions = ["Talk to Emilitia (shortcut out of sewers)"];
            vanillaGoals["Kill 4 Mimics"].exclusions = ["Save the 5 grubs in Deepnest", "Save the 7 grubs in Crystal Peak"];
            vanillaGoals["Longnail + MoP"].exclusions = ["Mantis Lords"];
            vanillaGoals["Mantis Lords"].exclusions = ["Longnail + MoP"];
            vanillaGoals["Mask Shard  in the Hive"].exclusions = ["Hive Knight", "Hiveblood", "Tram Pass + Visit all 5 Tram Stations"];
            vanillaGoals["Monarch Wings"].exclusions = ["Broken Vessel"];
            vanillaGoals["Nail 3"].exclusions = ["Have 2 Pale Ore"];
            vanillaGoals["Obtain fountain vessel fragment"].exclusions = ["Spend 3000 geo", "Spend 4000 geo"];
            vanillaGoals["Quick Slash"].exclusions = ["Break the 420 geo rock in Kingdom's Edge"];
            vanillaGoals["Rescue Zote in Deepnest"].exclusions = ["Defeat Colosseum Zote"];
            vanillaGoals["Save the 5 grubs in Deepnest"].exclusions = ["Kill 4 Mimics"];
            vanillaGoals["Save the 7 grubs in Crystal Peak"].exclusions = ["Kill 4 Mimics"];
            vanillaGoals["Shade Cloak"].exclusions = ["Collect 1 Arcane Egg", "Void Tendrils Journal Entry"];
            vanillaGoals["Soul Master"].exclusions = ["Desolate Dive"];
            vanillaGoals["Spend 3000 geo"].exclusions = ["Obtain fountain vessel fragment", "Spend 4000 geo"];
            vanillaGoals["Spend 4000 geo"].exclusions = ["Obtain fountain vessel fragment", "Spend 3000 geo", "Spend 5000 geo"];
            vanillaGoals["Spend 5000 geo"].exclusions = ["Obtain fountain vessel fragment", "Spend 4000 geo"];
            vanillaGoals["Talk to Emilitia (shortcut out of sewers)"].exclusions = ["Isma's Tear"];
            vanillaGoals["Talk to Hornet at CoT Statue + Herrah"].exclusions = ["Herrah"];
            vanillaGoals["Talk to Lemm with Crest Equipped"].exclusions = ["Dung Defender"];
            vanillaGoals["Talk to Mask Maker"].exclusions = ["Talk to Midwife"];
            vanillaGoals["Talk to Midwife"].exclusions = ["Talk to Mask Maker"];
            vanillaGoals["Traitor Lord"].exclusions = ["Watch Cloth Die"];
            vanillaGoals["Tram Pass + Visit all 5 Tram Stations"].exclusions = ["Hive Knight", "Hiveblood", "Mask Shard  in the Hive"];
            vanillaGoals["Void Tendrils Journal Entry"].exclusions = ["Collect 1 Arcane Egg", "Shade Cloak"];
            vanillaGoals["Watch Cloth Die"].exclusions = ["Traitor Lord"];
            vanillaGoals["Xero"].exclusions = ["Dream Nail"];
        }

        private static void SetupItemRandoGoals()
        {
            List<string> goals = ["10 Lifeblood masks at the same time", "Bow to Moss Prophet, dead or alive", "Bow to the Fungal Core Elder", "Break 3 floors using Dive", "Break the 420 geo rock in Kingdom's Edge", "Buy 6 map pins from Iselda (All but two)", "Buy 8 map pins from Iselda (All)", "Buy out Leg Eater", "Buy out Salubra", "Buy the Basin fountain check", "Check 2 Nailmasters", "Check Crystal Heart", "Check Deep Focus", "Check Glowing Womb", "Check Isma's Tear", "Check Joni's Blessing", "Check Love Key", "Check Shade Soul", "Check Shape of Unn", "Check Sheo", "Check Void Heart", "Check the Charged Lumafly Journal Entry", "Check the Crystal Crawler Journal Entry", "Check the Goam and Garpede Journal Entries", "Check the Hallownest Crown", "Check the Hive Mask Shard", "Check the Stag Nest vessel fragment", "Check the Void Tendrils Journal Entry", "Check the journal above Mantis Village", "Check the journal below Stone Sanctuary", "Check three different spell locations", "Check/Free all grubs in Ancient Basin (2)", "Check/Free all grubs in City of Tears (5)", "Check/Free all grubs in Crossroads (5) + Fog Canyon (1)", "Check/Free all grubs in Crystal Peak (7)", "Check/Free all grubs in Deepnest (5)", "Check/Free all grubs in Greenpath (4) and in Fungal (2)", "Check/Free all grubs in Queen's Gardens (3)", "Check/Free all grubs in Waterways (3)", "Check/Kill 4 Mimics", "Check/Read 3 lore tablets in Teacher's Archives", "Check/Read both Pilgrim's Way lore tablets", "Check/Read both lore tablets in Mantis Village", "Check/Read both lore tablets in Soul Sanctum", "Check/Read the Dung Defender sign before Isma's Grove", "Check/Read the lore tablet in Ancient Basin", "Check/Read the lore tablet in Howling Cliffs", "Check/Read the lore tablet in Kingdom's Edge (requires Spore Shroom)", "Check/Read three lore tablets in Greenpath", "Check/Read two lore tablets in City of Tears proper (No sub areas)", "Collect 500 essence", "Colosseum 1", "Complete 4 full dream trees", "Complete Path of Pain", "Complete either ending of the Cloth questline", "Complete the Greenpath Root", "Complete the Kingdom's Edge Root", "Defeat Broken Vessel", "Defeat Collector", "Defeat Colosseum Zote", "Defeat Crystal Guardian 1", "Defeat Crystal Guardian 2", "Defeat Dung Defender", "Defeat Elder Hu", "Defeat Failed Champion", "Defeat False Knight + Brooding Mawlek", "Defeat Flukemarm", "Defeat Galien", "Defeat Gorb", "Defeat Hive Knight", "Defeat Hornet 2", "Defeat Lost Kin", "Defeat Mantis Lords", "Defeat Markoth", "Defeat Marmu", "Defeat Nightmare King Grimm", "Defeat No Eyes", "Defeat Nosk", "Defeat Pale Lurker", "Defeat Soul Master", "Defeat Soul Tyrant", "Defeat Traitor Lord", "Defeat Troupe Master Grimm", "Defeat Uumuu", "Defeat Vengefly King + Massive Moss Charger", "Defeat Watcher Knights", "Defeat White Defender", "Defeat Xero", "Defeat any one Radiant Boss", "Defeat two Dream Bosses", "Defeat two dream warriors", "Dream Nail Marissa", "Dream Nail White Lady", "Dream Nail Willoh's meal", "Enter Godhome", "Enter the Lifeblood Core room without wearing any Lifeblood charms", "Equip 5 Charms at the same time", "Eternal Ordeal: 20 Zotes", "Get 2 Dreamer's checks (Requires Dream nail)", "Get Brumm's flame", "Get all the Grubfather checks", "Get all the Seer checks", "Get the Abyss Shriek check", "Get two Pale Ore checks (Grubs / Essence excluded)", "Give Flower to Elderbug", "Have 1500 geo in the bank", "Have 3 different maps not counting Dirtmouth or Hive", "Have 5 or more Charms", "Have 6 Charm Notches total", "Hit the Oro scarecrow up until the hoppers spawn", "Interact with 3 Cornifer locations", "Interact with Mr. Mushroom once (Does not require Spore Shroom)", "Kill 3 Oomas using a minion charm", "Kill 6 different Stalking Devouts", "Kill Gorgeous Husk", "Kill Myla", "Kill a Durandoo", "Kill a Great Hopper", "Kill a Gulka with its own projectile", "Kill a Kingsmould", "Kill a Lightseed", "Kill an Aluba", "Kill three different Great Husk Sentries", "Kill two Soul Warriors", "Kill two different Alubas", "Kill two different Maggots", "Look through Lurien's telescope", "Nail 2", "Nail 3", "Obtain 1 Arcane Egg", "Obtain 1 extra mask", "Obtain 1 extra soul vessel", "Obtain 15 grubs", "Obtain 2 Nail Arts", "Obtain 2 Pale Ore", "Obtain 3 King's Idols", "Obtain 4 Rancid Eggs", "Obtain 5 Hallownest Seals", "Obtain 5 Wanderer's Journals", "Obtain Abyss Shriek", "Obtain Carefree Melody", "Obtain Collector's Map", "Obtain Descending Dark", "Obtain Dream Gate", "Obtain Dream Nail", "Obtain Dream Wielder or Dreamshield", "Obtain Flukenest or Fury of the Fallen", "Obtain Glowing Womb or Weaversong", "Obtain Godtuner", "Obtain Grubsong or Grubberfly's Elegy", "Obtain Heavy Blow or Steady Body", "Obtain Herrah", "Obtain Hiveblood or Sharp Shadow", "Obtain Howling Wraiths", "Obtain Isma's Tear", "Obtain Longnail or Mark of Pride", "Obtain Lumafly Lantern", "Obtain Lurien", "Obtain Monomon", "Obtain Quick Focus or Deep Focus", "Obtain Quick Slash or Nailmaster's Glory", "Obtain Shade Soul", "Obtain Shaman Stone or Spell Twister", "Obtain Shape of Unn or Baldur Shell", "Obtain Soul Eater or Soul Catcher", "Obtain Sprintmaster or Dashmaster", "Obtain Thorns of Agony or Stalwart Shell", "Obtain Tram Pass", "Obtain Vengeful Spirit", "Obtain Wayward Compass or Gathering Swarm", "Obtain World Sense", "Obtain all three Fragile charms", "Obtain the Love Key", "Obtain two Lifeblood charms", "Open Jiji's Hut and buy out Jiji", "Open the Crystal Peak chest", "Open the Dirtmouth / Crystal Peak elevator", "Parry Revek 3 times without dying (Spirit's Glade Guard)", "Rescue Bretta + Sly", "Rescue Zote in Deepnest", "Ride the stag to Distant Village", "Ride the stag to Hidden Station", "Ride the stag to King's Station", "Ride the stag to Queen's Gardens", "Ride the stag to Queen's Station", "Sit down in Hidden Station", "Sit on the City of Tears Quirrel bench", "Slash Millibelle in Pleasure House", "Slash the Beast's Den Trilobite", "Slash two Shade Gates", "Spend 3000 geo", "Spend 4000 geo", "Spend 5000 geo", "Splash the NPC in the Colosseum's hot spring", "Swat Tiso's shield away from his corpse", "Swim in a Void Pool", "Take a bath in 4 different Hot Springs", "Talk to Bardoon", "Talk to Cloth", "Talk to Emilitia (shortcut out of sewers)", "Talk to Lemm in his shop with Defender's Crest equipped", "Talk to Mask Maker", "Talk to Midwife", "Talk to Salubra while overcharmed", "Talk to Tuk", "Talk to the Fluke Hermit", "Use 2 Simple Keys", "Use City Crest + Ride both CoT large elevators", "Use a Nail Art in its vanilla Nailmaster's Hut", "Visit Distant Village or Hive", "Visit Lake of Unn or Blue Lake", "Visit Overgrown Mound or Crystalised Mound (Crystalised requires dive)", "Visit Queen's Gardens or Cast Off Shell", "Visit Shrine of Believers", "Visit Soul Sanctum or Royal Waterways", "Visit Tower of Love (Love Key not required)", "Visit all 4 shops (Sly, Iselda, Salubra and Leg Eater)"];
            foreach (string goal in goals)
            {
                itemRandoGoals.Add(goal, new(goal));
            }
            itemRandoGoals["Obtain 15 grubs"].exclusions = [];
            itemRandoGoals["Use 2 Simple Keys"].exclusions = ["Open Jiji's Hut and buy out Jiji", "Dream Nail Marissa"];
            itemRandoGoals["Obtain 2 Pale Ore"].exclusions = ["Nail 3"];
            itemRandoGoals["Kill two Soul Warriors"].exclusions = ["Check Shade Soul"];
            itemRandoGoals["Spend 3000 geo"].exclusions = ["Buy the Basin fountain check", "Spend 4000 geo"];
            itemRandoGoals["Break 3 floors using Dive"].exclusions = [];
            itemRandoGoals["Have 3 different maps not counting Dirtmouth or Hive"].exclusions = ["Interact with 3 Cornifer locations"];
            itemRandoGoals["Kill three different Great Husk Sentries"].exclusions = ["Kill Gorgeous Husk"];
            itemRandoGoals["Spend 4000 geo"].exclusions = ["Buy the Basin fountain check", "Spend 3000 geo", "Spend 5000 geo"];
            itemRandoGoals["Spend 5000 geo"].exclusions = ["Spend 4000 geo"];
            itemRandoGoals["Have 1500 geo in the bank"].exclusions = ["Slash Millibelle in Pleasure House"];
            itemRandoGoals["Get Brumm's flame"].exclusions = ["Obtain Carefree Melody"];
            itemRandoGoals["Defeat Broken Vessel"].exclusions = ["Defeat Lost Kin"];
            itemRandoGoals["Obtain Carefree Melody"].exclusions = ["Defeat Nightmare King Grimm", "Get Brumm's flame"];
            itemRandoGoals["Defeat Crystal Guardian 1"].exclusions = ["Defeat Crystal Guardian 2"];
            itemRandoGoals["Defeat Crystal Guardian 2"].exclusions = ["Defeat Crystal Guardian 1"];
            itemRandoGoals["Talk to Cloth"].exclusions = ["Visit all 4 shops (Sly, Iselda, Salubra and Leg Eater)"];
            itemRandoGoals["Complete either ending of the Cloth questline"].exclusions = ["Defeat Traitor Lord", "Dream Nail White Lady"];
            itemRandoGoals["Defeat Collector"].exclusions = [];
            itemRandoGoals["Colosseum 1"].exclusions = ["Defeat Colosseum Zote"];
            itemRandoGoals["Defeat Colosseum Zote"].exclusions = ["Colosseum 1"];
            itemRandoGoals["Interact with 3 Cornifer locations"].exclusions = ["Have 3 different maps not counting Dirtmouth or Hive"];
            itemRandoGoals["Defeat Dung Defender"].exclusions = ["Defeat White Defender"];
            itemRandoGoals["Rescue Zote in Deepnest"].exclusions = [];
            itemRandoGoals["Ride the stag to Distant Village"].exclusions = ["Talk to Midwife", "Visit Distant Village or Hive"];
            itemRandoGoals["Get 2 Dreamer's checks (Requires Dream nail)"].exclusions = ["Defeat Uumuu", "Defeat Watcher Knights", "Visit Distant Village or Hive"];
            itemRandoGoals["Kill a Durandoo"].exclusions = ["Kill a Gulka with its own projectile"];
            itemRandoGoals["Buy the Basin fountain check"].exclusions = ["Spend 3000 geo", "Spend 4000 geo"];
            itemRandoGoals["Kill Gorgeous Husk"].exclusions = ["Kill three different Great Husk Sentries"];
            itemRandoGoals["Enter Godhome"].exclusions = ["Eternal Ordeal: 20 Zotes", "Defeat any one Radiant Boss"];
            itemRandoGoals["Kill a Gulka with its own projectile"].exclusions = ["Kill a Durandoo"];
            itemRandoGoals["Ride the stag to Hidden Station"].exclusions = ["Sit down in Hidden Station"];
            itemRandoGoals["Defeat Hive Knight"].exclusions = ["Check the Hive Mask Shard"];
            itemRandoGoals["Check the Hive Mask Shard"].exclusions = ["Defeat Hive Knight"];
            itemRandoGoals["Defeat Hornet 2"].exclusions = ["Visit Queen's Gardens or Cast Off Shell"];
            itemRandoGoals["Check Joni's Blessing"].exclusions = ["Obtain Lumafly Lantern"];
            itemRandoGoals["Kill a Kingsmould"].exclusions = ["Complete Path of Pain"];
            itemRandoGoals["Obtain Lumafly Lantern"].exclusions = ["Check Joni's Blessing"];
            itemRandoGoals["Defeat Lost Kin"].exclusions = ["Defeat Broken Vessel"];
            itemRandoGoals["Obtain the Love Key"].exclusions = [];
            itemRandoGoals["Check Love Key"].exclusions = ["Obtain Isma's Tear"];
            itemRandoGoals["Dream Nail Marissa"].exclusions = ["Take a bath in 4 different Hot Springs", "Use 2 Simple Keys"];
            itemRandoGoals["Talk to Midwife"].exclusions = ["Ride the stag to Distant Village", "Visit Distant Village or Hive"];
            itemRandoGoals["Slash Millibelle in Pleasure House"].exclusions = ["Have 1500 geo in the bank"];
            itemRandoGoals["Nail 3"].exclusions = ["Obtain 2 Pale Ore"];
            itemRandoGoals["Defeat Nightmare King Grimm"].exclusions = ["Obtain Carefree Melody"];
            itemRandoGoals["Defeat Nosk"].exclusions = ["Get two Pale Ore checks (Grubs / Essence excluded)"];
            itemRandoGoals["Eternal Ordeal: 20 Zotes"].exclusions = ["Enter Godhome", "Defeat any one Radiant Boss"];
            itemRandoGoals["Get two Pale Ore checks (Grubs / Essence excluded)"].exclusions = ["Defeat Nosk"];
            itemRandoGoals["Buy 6 map pins from Iselda (All but two)"].exclusions = ["Buy 8 map pins from Iselda (All)"];
            itemRandoGoals["Buy 8 map pins from Iselda (All)"].exclusions = ["Buy 6 map pins from Iselda (All but two)", "Obtain Tram Pass"];
            itemRandoGoals["Complete Path of Pain"].exclusions = ["Kill a Kingsmould"];
            itemRandoGoals["Defeat any one Radiant Boss"].exclusions = ["Enter Godhome", "Eternal Ordeal: 20 Zotes"];
            itemRandoGoals["Parry Revek 3 times without dying (Spirit's Glade Guard)"].exclusions = ["Visit Shrine of Believers"];
            itemRandoGoals["Visit all 4 shops (Sly, Iselda, Salubra and Leg Eater)"].exclusions = ["Talk to Cloth"];
            itemRandoGoals["Visit Shrine of Believers"].exclusions = ["Parry Revek 3 times without dying (Spirit's Glade Guard)"];
            itemRandoGoals["Defeat Soul Master"].exclusions = ["Defeat Soul Tyrant"];
            itemRandoGoals["Defeat Soul Tyrant"].exclusions = ["Defeat Soul Master"];
            itemRandoGoals["Take a bath in 4 different Hot Springs"].exclusions = ["Dream Nail Marissa"];
            itemRandoGoals["Check Shade Soul"].exclusions = ["Kill two Soul Warriors"];
            itemRandoGoals["Obtain Isma's Tear"].exclusions = ["Check Shape of Unn", "Check Love Key"];
            itemRandoGoals["Defeat Traitor Lord"].exclusions = ["Dream Nail White Lady", "Complete either ending of the Cloth questline"];
            itemRandoGoals["Obtain Tram Pass"].exclusions = ["Buy 8 map pins from Iselda (All)"];
            itemRandoGoals["Check Shape of Unn"].exclusions = ["Obtain Isma's Tear"];
            itemRandoGoals["Visit Distant Village or Hive"].exclusions = ["Obtain Tram Pass", "Ride the stag to Distant Village", "Talk to Midwife"];
            itemRandoGoals["Visit Queen's Gardens or Cast Off Shell"].exclusions = ["Defeat Hornet 2"];
            itemRandoGoals["Defeat White Defender"].exclusions = ["Defeat Dung Defender", "Interact with Mr. Mushroom once (Does not require Spore Shroom)"];
            itemRandoGoals["Dream Nail White Lady"].exclusions = ["Defeat Traitor Lord", "Complete either ending of the Cloth questline"];
            itemRandoGoals["Sit down in Hidden Station"].exclusions = ["Ride the stag to Hidden Station"];
        }

        private static string Anify(string word)
        {
            if (new List<string> { "a", "e", "i", "o", "u" }.Contains(word.Substring(0, 1).ToLower()))
            {
                return "an " + word;
            }
            return "a " + word;
        }
    }
}
