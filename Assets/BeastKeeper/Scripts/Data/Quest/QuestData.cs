using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeastKeeper.Data
{
    /// <summary>
    /// A single quest objective. The definition fields (id/description/required) live on the
    /// ScriptableObject asset; Current is runtime state tracked by the QuestSystem in cloned
    /// instances so the asset is never mutated during play.
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        [SerializeField] private string id;
        [SerializeField] private string description;
        [SerializeField] private int required = 1;
        [SerializeField] private string requiresObjectiveId;

        public string Id => id;
        public string Description => description;
        public int Required => required;
        public string RequiresObjectiveId => requiresObjectiveId;
        public int Current { get; private set; }
        public bool IsComplete => Current >= required;

        public QuestObjective()
        {
        }

        public QuestObjective(string id, string description, int required = 1, string requiresObjectiveId = null)
        {
            this.id = id;
            this.description = description;
            this.required = Mathf.Max(1, required);
            this.requiresObjectiveId = requiresObjectiveId;
        }

        /// <summary>
        /// Advances progress, capping at the required amount. No-op once complete.
        /// </summary>
        public void Progress(int amount)
        {
            if (IsComplete || amount <= 0) return;
            Current = Mathf.Min(required, Current + amount);
        }

        /// <summary>
        /// Overwrites runtime progress (used when loading saved state).
        /// </summary>
        public void SetProgress(int current)
        {
            Current = Mathf.Clamp(current, 0, required);
        }

        /// <summary>
        /// Returns a fresh runtime copy of this objective definition.
        /// </summary>
        public QuestObjective Clone()
        {
            var clone = new QuestObjective
            {
                id = id,
                description = description,
                required = required,
                requiresObjectiveId = requiresObjectiveId
            };
            return clone;
        }
    }

    /// <summary>
    /// An item granted when a quest completes.
    /// </summary>
    [Serializable]
    public class QuestReward
    {
        [SerializeField] private ItemData item;
        [SerializeField] private int quantity = 1;

        public ItemData Item => item;
        public int Quantity => quantity;

        public QuestReward()
        {
        }

        public QuestReward(ItemData item, int quantity = 1)
        {
            this.item = item;
            this.quantity = Mathf.Max(1, quantity);
        }
    }

    /// <summary>
    /// ScriptableObject representing a quest, which contains multiple objectives.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuestData", menuName = "Beast Keeper/Data/Quest/Quest")]
    public class QuestData : EntityData
    {
        [SerializeField] private List<QuestObjective> objectives;
        [SerializeField] private int experienceReward;
        [SerializeField] private List<QuestReward> itemRewards;
        [SerializeField] private string startAfterDialogueId;
        [SerializeField] private string startObjectiveId;

        public IReadOnlyList<QuestObjective> Objectives => objectives;
        public int ExperienceReward => experienceReward;
        public IReadOnlyList<QuestReward> ItemRewards => itemRewards;

        /// <summary>
        /// Dialogue id (EntityData id or asset name) whose completion starts this quest.
        /// </summary>
        public string StartAfterDialogueId => startAfterDialogueId;

        /// <summary>
        /// Optional objective id considered fulfilled by the conversation that starts the quest.
        /// </summary>
        public string StartObjectiveId => startObjectiveId;

        /// <summary>
        /// Creates an in-memory QuestData for runtime fallback or testing.
        /// </summary>
        public static QuestData CreateRuntime(string id, string displayName, int experienceReward = 0,
            string startAfterDialogueId = null, string startObjectiveId = null,
            QuestReward[] itemRewards = null, params QuestObjective[] objectives)
        {
            var quest = CreateInstance<QuestData>();
            quest.name = displayName;
            quest.InitializeBase(id, displayName);
            quest.experienceReward = Mathf.Max(0, experienceReward);
            quest.startAfterDialogueId = startAfterDialogueId;
            quest.startObjectiveId = startObjectiveId;
            quest.objectives = new List<QuestObjective>();
            if (objectives != null)
            {
                foreach (var objective in objectives)
                {
                    if (objective != null) quest.objectives.Add(objective);
                }
            }
            quest.itemRewards = new List<QuestReward>();
            if (itemRewards != null)
            {
                foreach (var reward in itemRewards)
                {
                    if (reward != null) quest.itemRewards.Add(reward);
                }
            }
            return quest;
        }
    }
}
