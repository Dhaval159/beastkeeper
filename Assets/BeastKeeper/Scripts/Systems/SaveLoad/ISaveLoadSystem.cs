using BeastKeeper.Core;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Contract for the game's save/load manager.
    /// </summary>
    public interface ISaveLoadSystem : IGameService
    {
        void SaveGame();
        void LoadGame();
        bool HasSaveFile();
        void RegisterPersistence(IDataPersistence persistence);
        void UnregisterPersistence(IDataPersistence persistence);
    }
}
