using UnityEngine;

namespace BeastKeeper.Data
{
    /// <summary>
    /// Abstract base class for all data-driven game entities (monsters, items, quests, etc.).
    /// </summary>
    public abstract class EntityData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(3, 5)] private string description;

        /// <summary>
        /// Unique identifier for this entity.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// Display name shown in-game.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Localized or raw description.
        /// </summary>
        public string Description => description;
    }
}
