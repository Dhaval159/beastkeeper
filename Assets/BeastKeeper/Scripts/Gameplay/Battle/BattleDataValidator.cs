using System;
using UnityEngine;
using BeastKeeper.Data;

namespace BeastKeeper.Gameplay.Battle
{
    /// <summary>
    /// Safe data validation for battle-related assets. Warns on invalid data instead of failing hard.
    /// </summary>
    public static class BattleDataValidator
    {
        public static void ValidateMonster(MonsterData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[Battle] Attempted to validate a null MonsterData.");
                return;
            }

            if (string.IsNullOrEmpty(data.Id))
                Debug.LogWarning($"[Battle] Monster '{data.name}' has no Id set.");
            if (string.IsNullOrEmpty(data.DisplayName))
                Debug.LogWarning($"[Battle] Monster '{data.name}' has no display name set.");
            if (data.BaseHp <= 0)
                Debug.LogWarning($"[Battle] Monster '{data.name}' has HP <= 0 ({data.BaseHp}).");
            if (data.BaseAttack < 0)
                Debug.LogWarning($"[Battle] Monster '{data.name}' has negative base attack ({data.BaseAttack}).");
            if (data.BaseDefense < 0)
                Debug.LogWarning($"[Battle] Monster '{data.name}' has negative base defense ({data.BaseDefense}).");
            if (data.BaseSpeed < 0)
                Debug.LogWarning($"[Battle] Monster '{data.name}' has negative base speed ({data.BaseSpeed}).");

            if (data.LearnableAbilities == null || data.LearnableAbilities.Count == 0)
            {
                Debug.LogWarning($"[Battle] Monster '{data.name}' has no learnable abilities.");
            }
            else
            {
                foreach (var ability in data.LearnableAbilities)
                {
                    ValidateAbility(ability);
                }
            }
        }

        public static void ValidateAbility(MonsterAbility ability)
        {
            if (ability == null)
            {
                Debug.LogWarning("[Battle] Validated a null MonsterAbility.");
                return;
            }

            if (string.IsNullOrEmpty(ability.Id))
                Debug.LogWarning($"[Battle] Ability '{ability.name}' has no Id set.");
            if (string.IsNullOrEmpty(ability.DisplayName))
                Debug.LogWarning($"[Battle] Ability '{ability.name}' has no display name set.");

            if (!Enum.IsDefined(typeof(AbilityEffectType), ability.EffectType))
            {
                Debug.LogWarning($"[Battle] Ability '{ability.name}' has an invalid effect type ({ability.EffectType}).");
            }
            else if (ability.EffectType == AbilityEffectType.Damage && ability.BasePower <= 0)
            {
                Debug.LogWarning($"[Battle] Damage ability '{ability.name}' has zero or invalid power ({ability.BasePower}).");
            }
            else if (ability.EffectType == AbilityEffectType.ReduceAttack && ability.EffectValue <= 0)
            {
                Debug.LogWarning($"[Battle] ReduceAttack ability '{ability.name}' has invalid effect value ({ability.EffectValue}).");
            }
        }
    }
}
