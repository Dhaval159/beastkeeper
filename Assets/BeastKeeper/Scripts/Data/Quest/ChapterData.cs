using System.Collections.Generic;
using UnityEngine;

namespace BeastKeeper.Data
{
    /// <summary>
    /// ScriptableObject representing a storyline chapter, which consists of several quests.
    /// </summary>
    [CreateAssetMenu(fileName = "NewChapterData", menuName = "Beast Keeper/Data/Quest/Chapter")]
    public class ChapterData : EntityData
    {
        [SerializeField] private int chapterNumber;
        [SerializeField] private List<QuestData> quests;

        public int ChapterNumber => chapterNumber;
        public IReadOnlyList<QuestData> Quests => quests;
    }
}
