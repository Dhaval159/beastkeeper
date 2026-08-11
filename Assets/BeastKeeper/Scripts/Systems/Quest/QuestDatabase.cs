using System.Collections.Generic;
using UnityEngine;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Scene component that registers quest assets with the QuestSystem so quests can be
    /// located by id and started via dialogue-complete triggers.
    /// </summary>
    public class QuestDatabase : MonoBehaviour
    {
        [SerializeField] private List<QuestData> quests = new List<QuestData>();

        public IReadOnlyList<QuestData> Quests => quests;

        private void Awake()
        {
            IQuestSystem questSystem = QuestServiceManager.Get();
            foreach (QuestData quest in quests)
            {
                if (quest != null) questSystem.RegisterQuest(quest);
            }
        }
    }
}
