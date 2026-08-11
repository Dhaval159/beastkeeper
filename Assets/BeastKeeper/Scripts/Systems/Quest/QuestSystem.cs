using System;
using System.Collections.Generic;
using UnityEngine;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Runtime quest tracking. Keeps objective progress in cloned runtime copies so quest
    /// ScriptableObject assets are never mutated. Listens to the EventBus for the events that
    /// advance objectives; quest completion logic lives entirely in quest data (objective ids),
    /// never in gameplay controllers.
    /// </summary>
    public class QuestSystem : IQuestSystem, IDisposable, IDataPersistence
    {
        private readonly List<QuestData> activeOrder = new List<QuestData>();
        private readonly Dictionary<QuestData, List<QuestObjective>> activeProgress = new Dictionary<QuestData, List<QuestObjective>>();
        private readonly HashSet<QuestData> completedQuests = new HashSet<QuestData>();
        private readonly Dictionary<string, QuestData> questsById = new Dictionary<string, QuestData>();
        private readonly Dictionary<string, QuestData> questsByDialogueTrigger = new Dictionary<string, QuestData>();
        private readonly IProgressionSystem progression;
        private readonly IInventorySystem inventory;

        public event Action<QuestData> QuestStarted;
        public event Action<QuestData, QuestObjective> ObjectiveProgressed;
        public event Action<QuestData> QuestCompleted;

        public QuestSystem()
            : this(null, null)
        {
        }

        /// <summary>
        /// Services may be injected for testing; at runtime the ServiceLocator managers are used.
        /// </summary>
        public QuestSystem(IProgressionSystem progression, IInventorySystem inventory)
        {
            this.progression = progression;
            this.inventory = inventory;
            Subscribe();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            EventBus.Subscribe<DialogueCompletedEvent>(OnDialogueCompleted);
            EventBus.Subscribe<NPCInteractionEvent>(OnNpcInteraction);
            EventBus.Subscribe<AreaEnteredEvent>(OnAreaEntered);
            EventBus.Subscribe<MonsterDefeatedEvent>(OnMonsterDefeated);
            EventBus.Subscribe<BattleVictoryEvent>(OnBattleVictory);
            EventBus.Subscribe<CollectItemEvent>(OnItemCollected);
        }

        private void Unsubscribe()
        {
            EventBus.Unsubscribe<DialogueCompletedEvent>(OnDialogueCompleted);
            EventBus.Unsubscribe<NPCInteractionEvent>(OnNpcInteraction);
            EventBus.Unsubscribe<AreaEnteredEvent>(OnAreaEntered);
            EventBus.Unsubscribe<MonsterDefeatedEvent>(OnMonsterDefeated);
            EventBus.Unsubscribe<BattleVictoryEvent>(OnBattleVictory);
            EventBus.Unsubscribe<CollectItemEvent>(OnItemCollected);
        }

        public void RegisterQuest(QuestData quest)
        {
            if (quest == null) return;
            questsById[quest.IdOrAssetName] = quest;
            if (!string.IsNullOrEmpty(quest.StartAfterDialogueId))
            {
                questsByDialogueTrigger[quest.StartAfterDialogueId] = quest;
            }
        }

        public void StartQuest(QuestData quest)
        {
            if (quest == null || IsQuestActive(quest) || IsQuestCompleted(quest)) return;

            RegisterQuest(quest);
            activeOrder.Add(quest);
            activeProgress[quest] = CloneObjectives(quest);

            EventBus.Raise(new QuestStartedEvent { QuestId = quest.IdOrAssetName });
            QuestStarted?.Invoke(quest);

            // A conversation can count as the first talk objective when configured in data.
            if (!string.IsNullOrEmpty(quest.StartObjectiveId))
            {
                ProgressObjectiveInQuest(quest, quest.StartObjectiveId, 1);
            }
        }

        public void CompleteQuest(QuestData quest)
        {
            if (quest == null || IsQuestCompleted(quest)) return;
            if (!activeProgress.TryGetValue(quest, out _)) return;

            activeOrder.Remove(quest);
            activeProgress.Remove(quest);
            completedQuests.Add(quest);

            GrantRewards(quest);

            EventBus.Raise(new QuestCompletedEvent { QuestId = quest.IdOrAssetName });
            QuestCompleted?.Invoke(quest);
        }

        public void AdvanceObjective(string objectiveId, int amount = 1)
        {
            if (string.IsNullOrEmpty(objectiveId) || amount <= 0) return;

            // Backward iteration so CompleteQuest() can safely remove the current quest.
            for (int i = activeOrder.Count - 1; i >= 0; i--)
            {
                QuestData quest = activeOrder[i];
                List<QuestObjective> objectives = activeProgress[quest];
                bool anyProgress = false;

                for (int j = 0; j < objectives.Count; j++)
                {
                    QuestObjective objective = objectives[j];
                    if (objective.IsComplete) continue;
                    if (!ObjectiveMatches(objective.Id, objectiveId)) continue;

                    QuestObjective prereq = FindObjective(objectives, objective.RequiresObjectiveId);
                    if (prereq != null && !prereq.IsComplete) continue;

                    objective.Progress(amount);
                    anyProgress = true;
                    RaiseObjectiveProgressed(quest, objective);
                }

                if (anyProgress && AllComplete(objectives))
                {
                    CompleteQuest(quest);
                }
            }
        }

        public bool IsQuestActive(QuestData quest)
        {
            return quest != null && activeProgress.ContainsKey(quest);
        }

        public bool IsQuestCompleted(QuestData quest)
        {
            return quest != null && completedQuests.Contains(quest);
        }

        public QuestStatus GetQuestStatus(QuestData quest)
        {
            if (IsQuestCompleted(quest)) return QuestStatus.Completed;
            if (IsQuestActive(quest)) return QuestStatus.Active;
            return QuestStatus.NotStarted;
        }

        public IReadOnlyList<QuestData> GetActiveQuests()
        {
            return activeOrder;
        }

        public IReadOnlyList<QuestObjective> GetQuestProgress(QuestData quest)
        {
            return activeProgress.TryGetValue(quest, out List<QuestObjective> objectives) ? objectives : null;
        }

        public QuestData GetQuestById(string questId)
        {
            if (string.IsNullOrEmpty(questId)) return null;
            return questsById.TryGetValue(questId, out QuestData quest) ? quest : null;
        }

        private void ProgressObjectiveInQuest(QuestData quest, string objectiveId, int amount)
        {
            if (!activeProgress.TryGetValue(quest, out List<QuestObjective> objectives)) return;
            for (int i = 0; i < objectives.Count; i++)
            {
                QuestObjective objective = objectives[i];
                if (objective.IsComplete || objective.Id != objectiveId) continue;
                objective.Progress(amount);
                RaiseObjectiveProgressed(quest, objective);
            }

            if (activeProgress.TryGetValue(quest, out List<QuestObjective> updated) && AllComplete(updated))
            {
                CompleteQuest(quest);
            }
        }

        private void RaiseObjectiveProgressed(QuestData quest, QuestObjective objective)
        {
            EventBus.Raise(new QuestObjectiveProgressedEvent
            {
                QuestId = quest.IdOrAssetName,
                ObjectiveId = objective.Id,
                Current = objective.Current,
                Required = objective.Required,
                Complete = objective.IsComplete
            });
            ObjectiveProgressed?.Invoke(quest, objective);
        }

        private void GrantRewards(QuestData quest)
        {
            IProgressionSystem rewardProgression = progression != null ? progression : ProgressionServiceManager.Get();
            if (quest.ExperienceReward > 0)
            {
                rewardProgression.AddExperience(quest.ExperienceReward);
                rewardProgression.CheckLevelUp();
            }

            IInventorySystem rewardInventory = inventory != null ? inventory : InventoryServiceManager.Get();
            if (quest.ItemRewards != null)
            {
                foreach (QuestReward reward in quest.ItemRewards)
                {
                    if (reward == null || reward.Item == null) continue;
                    rewardInventory.AddItem(reward.Item, Mathf.Max(1, reward.Quantity));
                }
            }
        }

        private static List<QuestObjective> CloneObjectives(QuestData quest)
        {
            var clones = new List<QuestObjective>();
            if (quest.Objectives != null)
            {
                foreach (QuestObjective objective in quest.Objectives)
                {
                    if (objective != null) clones.Add(objective.Clone());
                }
            }
            return clones;
        }

        private static bool AllComplete(List<QuestObjective> objectives)
        {
            for (int i = 0; i < objectives.Count; i++)
            {
                if (!objectives[i].IsComplete) return false;
            }
            return true;
        }

        private static QuestObjective FindObjective(List<QuestObjective> objectives, string objectiveId)
        {
            if (string.IsNullOrEmpty(objectiveId)) return null;
            for (int i = 0; i < objectives.Count; i++)
            {
                if (objectives[i].Id == objectiveId) return objectives[i];
            }
            return null;
        }

        /// <summary>
        /// Matches an objective id against an event key. Supports exact ids ("talk:old_keeper")
        /// and variant ids for repeat interactions ("talk:old_keeper:return").
        /// </summary>
        private static bool ObjectiveMatches(string objectiveId, string eventKey)
        {
            if (objectiveId == eventKey) return true;
            return objectiveId.Length > eventKey.Length
                && objectiveId.StartsWith(eventKey, StringComparison.Ordinal)
                && objectiveId[eventKey.Length] == ':';
        }

        private void OnDialogueCompleted(DialogueCompletedEvent e)
        {
            if (string.IsNullOrEmpty(e.DialogueId)) return;
            if (questsByDialogueTrigger.TryGetValue(e.DialogueId, out QuestData quest))
            {
                StartQuest(quest);
            }
        }

        private void OnNpcInteraction(NPCInteractionEvent e)
        {
            AdvanceObjective($"talk:{e.NpcId}");
        }

        private void OnAreaEntered(AreaEnteredEvent e)
        {
            AdvanceObjective($"enter:{e.AreaId}");
        }

        private void OnMonsterDefeated(MonsterDefeatedEvent e)
        {
            AdvanceObjective($"defeat:{e.MonsterId}");
        }

        private void OnBattleVictory(BattleVictoryEvent e)
        {
            AdvanceObjective("win_battle");
        }

        private void OnItemCollected(CollectItemEvent e)
        {
            AdvanceObjective($"collect:{e.ItemId}");
        }

        public void SaveData(ref object gameData)
        {
            GameStateData gd = gameData as GameStateData ?? new GameStateData();
            gd.Quests = new List<QuestState>();

            foreach (QuestData quest in activeOrder)
            {
                var state = new QuestState { QuestId = quest.IdOrAssetName, Status = QuestStatus.Active.ToString() };
                if (activeProgress.TryGetValue(quest, out List<QuestObjective> objectives))
                {
                    foreach (QuestObjective objective in objectives)
                    {
                        state.Objectives.Add(new ObjectiveState { Id = objective.Id, Current = objective.Current });
                    }
                }
                gd.Quests.Add(state);
            }

            foreach (QuestData quest in completedQuests)
            {
                gd.Quests.Add(new QuestState { QuestId = quest.IdOrAssetName, Status = QuestStatus.Completed.ToString() });
            }

            gameData = gd;
        }

        public void LoadData(object gameData)
        {
            if (!(gameData is GameStateData gd) || gd.Quests == null) return;

            foreach (QuestState state in gd.Quests)
            {
                QuestData quest = GetQuestById(state.QuestId);
                if (quest == null) continue;

                if (state.Status == QuestStatus.Active.ToString())
                {
                    StartQuest(quest);
                    if (activeProgress.TryGetValue(quest, out List<QuestObjective> objectives) && state.Objectives != null)
                    {
                        for (int i = 0; i < objectives.Count && i < state.Objectives.Count; i++)
                        {
                            objectives[i].SetProgress(state.Objectives[i].Current);
                        }
                    }
                }
                else if (state.Status == QuestStatus.Completed.ToString())
                {
                    if (!completedQuests.Contains(quest)) completedQuests.Add(quest);
                }
            }
        }
    }
}
