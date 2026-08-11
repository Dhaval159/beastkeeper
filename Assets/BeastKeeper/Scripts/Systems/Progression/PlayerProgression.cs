using System;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Minimal player progression used to track battle XP and levels.
    /// </summary>
    public class PlayerProgression : IProgressionSystem, IDataPersistence
    {
        public event Action LeveledUp;

        public int PlayerLevel { get; private set; } = 1;
        public int PlayerExperience { get; private set; }
        public ChapterData CurrentChapter { get; private set; }

        /// <summary>
        /// XP required for the next level, e.g. 100 * level.
        /// </summary>
        public int XpToNextLevel => PlayerLevel * 100;

        public void AddExperience(int amount)
        {
            if (amount <= 0) return;
            PlayerExperience += amount;
        }

        /// <summary>
        /// Applies any pending level ups and returns whether at least one level was gained.
        /// </summary>
        public bool CheckLevelUp()
        {
            bool leveledUp = false;
            while (PlayerExperience >= XpToNextLevel)
            {
                PlayerExperience -= XpToNextLevel;
                PlayerLevel++;
                leveledUp = true;
            }

            if (leveledUp) LeveledUp?.Invoke();
            return leveledUp;
        }

        public void AdvanceChapter(ChapterData nextChapter)
        {
            CurrentChapter = nextChapter;
        }

        public void SaveData(ref object gameData)
        {
            GameStateData gd = gameData as GameStateData ?? new GameStateData();
            gd.Progression = CreateSaveData();
            gameData = gd;
        }

        public PlayerProgressionState CreateSaveData()
        {
            return new PlayerProgressionState { Level = PlayerLevel, Experience = PlayerExperience };
        }

        public void LoadData(object gameData)
        {
            if (gameData is GameStateData gd && gd.Progression != null)
            {
                LoadSaveData(gd.Progression);
            }
        }

        public void LoadSaveData(PlayerProgressionState state)
        {
            if (state == null) return;
            PlayerLevel = Math.Max(1, state.Level);
            PlayerExperience = Math.Max(0, state.Experience);
        }
    }
}
