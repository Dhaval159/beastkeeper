using System.Collections.Generic;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Contract for player inventory management.
    /// </summary>
    public interface IInventorySystem : IGameService
    {
        bool AddItem(ItemData item, int quantity = 1);
        bool RemoveItem(ItemData item, int quantity = 1);
        int GetItemCount(ItemData item);
        bool HasItem(ItemData item, int quantity = 1);
        IReadOnlyDictionary<ItemData, int> GetItems();
    }
}
