using System.Collections.Generic;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    /// <summary>
    /// Contract for the quest tracking and progression manager.
    /// </summary>
    public interface IQuestSystem : IGameService
    {
        void AcceptQuest(QuestData quest);
        void AdvanceObjective(QuestData quest, int objectiveIndex);
        void CompleteQuest(QuestData quest);
        QuestStatus GetQuestStatus(QuestData quest);
        IReadOnlyList<QuestData> GetActiveQuests();
    }
}
