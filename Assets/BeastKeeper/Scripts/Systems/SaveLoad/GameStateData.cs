using System;
using System.Collections.Generic;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Serializable runtime save-state foundation. No disk I/O, slots, or cloud — these models
    /// are consumed by systems via IDataPersistence and are ready for a future ISaveLoadSystem.
    /// </summary>
    [Serializable]
    public class GameStateData
    {
        public PlayerProgressionState Progression = new PlayerProgressionState();
        public InventoryState Inventory = new InventoryState();
        public List<QuestState> Quests = new List<QuestState>();
    }

    [Serializable]
    public class PlayerProgressionState
    {
        public int Level = 1;
        public int Experience;
    }

    [Serializable]
    public class InventoryState
    {
        public List<ItemStackState> Items = new List<ItemStackState>();
    }

    [Serializable]
    public class ItemStackState
    {
        public string ItemId;
        public int Quantity;
    }

    [Serializable]
    public class QuestState
    {
        public string QuestId;
        public string Status;
        public List<ObjectiveState> Objectives = new List<ObjectiveState>();
    }

    [Serializable]
    public class ObjectiveState
    {
        public string Id;
        public int Current;
    }
}
