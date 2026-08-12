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

        /// <summary>
        /// Creates an in-memory MonsterAbility for runtime fallback or testing without touching Unity Editor serialization.
        /// </summary>
        public static MonsterAbility CreateRuntime(string id, string displayName, int basePower, int energyCost, AbilityEffectType effectType, int effectValue = 0)
        {
            var ability = CreateInstance<MonsterAbility>();
            ability.name = displayName;
            ability.InitializeBase(id, displayName);
            ability.basePower = Mathf.Max(0, basePower);
            ability.energyCost = Mathf.Max(0, energyCost);
            ability.effectType = effectType;
            ability.effectValue = effectValue;
            return ability;
        }
    }
}
