using System;
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
    /// Objectives are advanced by string id (e.g. "talk:old_keeper", "enter:forest",
    /// "defeat:mossfang", "win_battle") so events drive progress without hard references.
    /// </summary>
    public interface IQuestSystem : IGameService
    {
        event Action<QuestData> QuestStarted;
        event Action<QuestData, QuestObjective> ObjectiveProgressed;
        event Action<QuestData> QuestCompleted;

        void RegisterQuest(QuestData quest);
        void StartQuest(QuestData quest);
        void CompleteQuest(QuestData quest);
        void AdvanceObjective(string objectiveId, int amount = 1);

        bool IsQuestActive(QuestData quest);
        bool IsQuestCompleted(QuestData quest);
        QuestStatus GetQuestStatus(QuestData quest);
        IReadOnlyList<QuestData> GetActiveQuests();
        IReadOnlyList<QuestObjective> GetQuestProgress(QuestData quest);
        QuestData GetQuestById(string questId);
    }
}
