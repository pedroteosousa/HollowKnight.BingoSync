using BingoSync.CustomGoals;
using BingoSync.GameUI;
using BingoSync.Interfaces;
using BingoSync.ModMenu;
using BingoSync.Settings;
using Modding;
using Newtonsoft.Json;

namespace BingoSync
{
    public class BingoSync : Mod, ILocalSettings<SaveSettings>, IGlobalSettings<ModSettings>, ICustomMenuMod
    {
        new public string GetName() => "BingoSync";

        public static string version = "1.4.4.5";
        public override string GetVersion() => version;

        private static readonly string DefaultSaveSettings = JsonConvert.SerializeObject(new SaveSettings());

        public override void Initialize()
        {
            OrderedLoader.Setup(Log);
            OrderedLoader.LoadInternal();
        }

        public static void ShowMenu()
        {
            Controller.MenuIsVisible = true;
        }

        public static void HideMenu()
        {
            Controller.MenuIsVisible = false;
        }

        public void OnLoadLocal(SaveSettings s)
        {
            GoalCompletionTracker.Variables = s;
            if(JsonConvert.SerializeObject(s) == DefaultSaveSettings)
            {
                return;
            }
            if (Controller.GlobalSettings.MarkCompletedGoalsOnLoadSavefile)
            {
                GoalCompletionTracker.ClearFinishedGoals();
                GoalCompletionTracker.BroadcastAllGoalStates();
            }
        }

        public SaveSettings OnSaveLocal()
        {
            return GoalCompletionTracker.Variables;
        }

        public void OnLoadGlobal(ModSettings s)
        {
            Controller.GlobalSettings = s;

            GameModesManager.LoadCustomGameModesFromFiles();
            MenuUI.LoadDefaults();
            MainMenu.RefreshMenu();
        }

        public ModSettings OnSaveGlobal()
        {
            GameModesManager.SaveCustomGameModesToFiles();
            return Controller.GlobalSettings;
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) {
            MenuScreen menu = MainMenu.CreateMenuScreen(modListMenu);
            MainMenu.RefreshMenu();
            return menu;
        }

        public bool ToggleButtonInsideMenu => false;
    }
}