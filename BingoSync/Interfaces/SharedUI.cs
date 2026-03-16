using BingoSync.GameUI;

namespace BingoSync.Interfaces
{
    public static class SharedUI
    {
        /// <summary>
        /// Get a separate page in the bottom right, with position and visibility managed by BingoSync.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SharedUIPage RequestUIPage(string name)
        {
            return SharedUIManager.RequestUIPage(name);
        }
    }
}
