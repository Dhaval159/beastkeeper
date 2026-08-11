using System;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Contract for managing story progression and player experience/level stats.
    /// </summary>
    public interface IProgressionSystem : IGameService
    {
        event Action LeveledUp;

        ChapterData CurrentChapter { get; }
        int PlayerLevel { get; }
        int PlayerExperience { get; }

        void AdvanceChapter(ChapterData nextChapter);
        void AddExperience(int amount);
        bool CheckLevelUp();
    }
}
