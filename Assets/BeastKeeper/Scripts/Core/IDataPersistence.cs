namespace BeastKeeper.Core
{
    /// <summary>
    /// Interface that systems implement to integrate with the save/load pipeline.
    /// </summary>
    public interface IDataPersistence
    {
        /// <summary>
        /// Called when the game state is being loaded.
        /// </summary>
        /// <param name="gameData">The game state container.</param>
        void LoadData(object gameData);

        /// <summary>
        /// Called when the game state is being saved.
        /// </summary>
        /// <param name="gameData">The game state container.</param>
        void SaveData(ref object gameData);
    }
}
