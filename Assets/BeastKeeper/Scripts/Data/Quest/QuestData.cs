using System.Collections.Generic;
using UnityEngine;

namespace BeastKeeper.Data
{
    /// <summary>
    /// ScriptableObject representing a quest, which contains multiple objectives.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuestData", menuName = "Beast Keeper/Data/Quest/Quest")]
    public class QuestData : EntityData
    {
        [SerializeField] private List<string> objectives;
        [SerializeField] private int experienceReward;

        public IReadOnlyList<string> Objectives => objectives;
        public int ExperienceReward => experienceReward;
    }
}
