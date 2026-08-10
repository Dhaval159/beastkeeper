using UnityEngine;

namespace BeastKeeper.Data
{
    public enum AbilityEffectType
    {
        Damage,
        ReduceAttack
    }

    /// <summary>
    /// ScriptableObject representing an ability/skill that a monster can learn and use in battle.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbilityData", menuName = "Beast Keeper/Data/Monster/Ability")]
    public class MonsterAbility : EntityData
    {
        [SerializeField] private int basePower;
        [SerializeField] private int energyCost;
        [SerializeField] private AbilityEffectType effectType;
        [SerializeField] private int effectValue;

        public int BasePower => basePower;
        public int EnergyCost => energyCost;
        public AbilityEffectType EffectType => effectType;
        public int EffectValue => effectValue;
    }
}
