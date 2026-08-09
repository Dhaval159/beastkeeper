using UnityEngine;

namespace BeastKeeper.Data
{
    /// <summary>
    /// ScriptableObject representing an ability/skill that a monster can learn and use in battle.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbilityData", menuName = "Beast Keeper/Data/Monster/Ability")]
    public class MonsterAbility : EntityData
    {
        [SerializeField] private int basePower;
        [SerializeField] private int energyCost;

        public int BasePower => basePower;
        public int EnergyCost => energyCost;
    }
}
