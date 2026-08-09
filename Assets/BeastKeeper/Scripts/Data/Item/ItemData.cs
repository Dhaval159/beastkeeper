using UnityEngine;

namespace BeastKeeper.Data
{
    public enum ItemType
    {
        Consumable,
        Quest,
        BondingUnit, // Creature bonding item
        Equipment
    }

    /// <summary>
    /// ScriptableObject representing items in the inventory database.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemData", menuName = "Beast Keeper/Data/Item")]
    public class ItemData : EntityData
    {
        [SerializeField] private ItemType type;
        [SerializeField] private Sprite icon;
        [SerializeField] private int value;

        public ItemType Type => type;
        public Sprite Icon => icon;
        public int Value => value;
    }
}
