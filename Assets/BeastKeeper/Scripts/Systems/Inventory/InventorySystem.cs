using System.Collections.Generic;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Minimal runtime inventory keyed by stable item id. No UI, no slots.
    /// </summary>
    public class InventorySystem : IInventorySystem, IDataPersistence
    {
        private readonly Dictionary<string, int> items = new Dictionary<string, int>();

        public bool AddItem(ItemData item, int quantity = 1)
        {
            if (item == null) return false;
            return AddItem(item.IdOrAssetName, quantity);
        }

        public bool AddItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;

            items.TryGetValue(itemId, out int current);
            items[itemId] = current + quantity;
            EventBus.Raise(new CollectItemEvent { ItemId = itemId, Amount = quantity });
            return true;
        }

        public bool RemoveItem(ItemData item, int quantity = 1)
        {
            if (item == null) return false;
            return RemoveItem(item.IdOrAssetName, quantity);
        }

        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;
            if (!items.TryGetValue(itemId, out int current) || current < quantity) return false;

            int remaining = current - quantity;
            if (remaining <= 0) items.Remove(itemId);
            else items[itemId] = remaining;
            return true;
        }

        public int GetItemCount(ItemData item)
        {
            return item == null ? 0 : GetItemCount(item.IdOrAssetName);
        }

        public int GetItemCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;
            return items.TryGetValue(itemId, out int count) ? count : 0;
        }

        public bool HasItem(ItemData item, int quantity = 1)
        {
            return GetItemCount(item) >= quantity;
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            return GetItemCount(itemId) >= quantity;
        }

        public IReadOnlyDictionary<string, int> GetItems()
        {
            return items;
        }

        public void SaveData(ref object gameData)
        {
            GameStateData gd = gameData as GameStateData ?? new GameStateData();
            gd.Inventory = CreateSaveData();
            gameData = gd;
        }

        public InventoryState CreateSaveData()
        {
            var state = new InventoryState();
            foreach (KeyValuePair<string, int> pair in items)
            {
                state.Items.Add(new ItemStackState { ItemId = pair.Key, Quantity = pair.Value });
            }
            return state;
        }

        public void LoadData(object gameData)
        {
            if (gameData is GameStateData gd && gd.Inventory != null)
            {
                LoadSaveData(gd.Inventory);
            }
        }

        public void LoadSaveData(InventoryState state)
        {
            items.Clear();
            if (state == null || state.Items == null) return;
            foreach (ItemStackState stack in state.Items)
            {
                if (stack == null || string.IsNullOrEmpty(stack.ItemId) || stack.Quantity <= 0) continue;
                items[stack.ItemId] = stack.Quantity;
            }
        }
    }
}
