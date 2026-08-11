using System.Collections.Generic;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Contract for player inventory management, keyed by stable item id.
    /// </summary>
    public interface IInventorySystem : IGameService
    {
        bool AddItem(ItemData item, int quantity = 1);
        bool AddItem(string itemId, int quantity = 1);
        bool RemoveItem(ItemData item, int quantity = 1);
        bool RemoveItem(string itemId, int quantity = 1);
        int GetItemCount(ItemData item);
        int GetItemCount(string itemId);
        bool HasItem(ItemData item, int quantity = 1);
        bool HasItem(string itemId, int quantity = 1);
        IReadOnlyDictionary<string, int> GetItems();
    }
}
