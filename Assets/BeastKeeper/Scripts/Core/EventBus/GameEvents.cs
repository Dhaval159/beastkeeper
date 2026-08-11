namespace BeastKeeper.Core
{
    /// <summary>
    /// Raised when a quest transitions to the Active state.
    /// </summary>
    public struct QuestStartedEvent : IGameEvent
    {
        public string QuestId;
    }

    /// <summary>
    /// Raised whenever an objective of an active quest advances.
    /// </summary>
    public struct QuestObjectiveProgressedEvent : IGameEvent
    {
        public string QuestId;
        public string ObjectiveId;
        public int Current;
        public int Required;
        public bool Complete;
    }

    /// <summary>
    /// Raised when a quest completes and its rewards are granted.
    /// </summary>
    public struct QuestCompletedEvent : IGameEvent
    {
        public string QuestId;
    }

    /// <summary>
    /// Raised when the player wins a battle. Carries the enemy and the XP awarded.
    /// </summary>
    public struct BattleVictoryEvent : IGameEvent
    {
        public string EnemyId;
        public int EnemyLevel;
        public int ExperienceAwarded;
    }

    /// <summary>
    /// Raised when the player loses a battle.
    /// </summary>
    public struct BattleDefeatEvent : IGameEvent
    {
        public string EnemyId;
        public int EnemyLevel;
    }

    /// <summary>
    /// Raised when a specific monster is defeated in battle.
    /// </summary>
    public struct MonsterDefeatedEvent : IGameEvent
    {
        public string MonsterId;
        public int Level;
    }

    /// <summary>
    /// Raised when the player interacts with an NPC.
    /// </summary>
    public struct NPCInteractionEvent : IGameEvent
    {
        public string NpcId;
    }

    /// <summary>
    /// Raised when the player enters an area.
    /// </summary>
    public struct AreaEnteredEvent : IGameEvent
    {
        public string AreaId;
    }

    /// <summary>
    /// Raised when a dialogue finishes.
    /// </summary>
    public struct DialogueCompletedEvent : IGameEvent
    {
        public string DialogueId;
    }

    /// <summary>
    /// Raised when an item is added to the inventory.
    /// </summary>
    public struct CollectItemEvent : IGameEvent
    {
        public string ItemId;
        public int Amount;
    }
}
