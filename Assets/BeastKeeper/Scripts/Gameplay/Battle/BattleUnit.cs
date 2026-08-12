using System.Collections.Generic;
using UnityEngine;
using BeastKeeper.Data;

namespace BeastKeeper.Gameplay.Battle
{
    /// <summary>
    /// Runtime state of a single combatant during battle. Built from a MonsterData definition.
    /// </summary>
    public class BattleUnit
    {
        public const int HpPerLevel = 5;
        public const int StatPerLevel = 2;
        public const int SpeedPerLevel = 1;

        public string Name { get; set; }
        public string DataId { get; set; }
        public int Level { get; set; }
        public int MaxHp { get; set; }
        public int CurrentHp { get; set; }
        public int BaseAttack { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public bool IsPlayer { get; set; }
        public Sprite BattleSprite { get; set; }
        public List<MonsterAbility> Abilities { get; set; } = new List<MonsterAbility>();

        public bool IsDefeated => CurrentHp <= 0;

        /// <summary>
        /// Builds a combatant from a MonsterData definition with deterministic per-level stat scaling.
        /// </summary>
        public static BattleUnit FromMonsterData(MonsterData data, int level)
        {
            if (data == null)
            {
                Debug.LogWarning("[BattleUnit] Cannot build a unit from null MonsterData; using fallback stats.");
                data = MonsterData.CreateRuntime("fallback", "Fallback", 50, 8, 12, 4);
            }

            BattleDataValidator.ValidateMonster(data);

            int lvl = Mathf.Max(1, level);
            int maxHp = Mathf.Max(1, data.BaseHp + (lvl - 1) * HpPerLevel);
            int baseAttack = data.BaseAttack + (lvl - 1) * StatPerLevel;

            var unit = new BattleUnit
            {
                Name = data.DisplayNameOrAssetName,
                DataId = data.Id,
                Level = lvl,
                MaxHp = maxHp,
                CurrentHp = maxHp,
                BaseAttack = baseAttack,
                Attack = baseAttack,
                Defense = data.BaseDefense + (lvl - 1) * StatPerLevel,
                Speed = data.BaseSpeed + (lvl - 1) * SpeedPerLevel,
                IsPlayer = false,
                BattleSprite = data.BattleSprite
            };

            if (data.LearnableAbilities != null)
            {
                foreach (var ability in data.LearnableAbilities)
                {
                    if (ability != null) unit.Abilities.Add(ability);
                }
            }

            return unit;
        }

        /// <summary>
        /// Applies damage, flooring current HP at zero.
        /// </summary>
        public void ApplyDamage(int amount)
        {
            CurrentHp = Mathf.Max(0, CurrentHp - Mathf.Max(0, amount));
        }

        /// <summary>
        /// Restores HP up to MaxHp. Returns the actual amount healed.
        /// </summary>
        public int Heal(int amount)
        {
            int before = CurrentHp;
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + Mathf.Max(0, amount));
            return CurrentHp - before;
        }

        /// <summary>
        /// Returns the first available damaging ability, or null if none exists.
        /// </summary>
        public MonsterAbility GetFirstDamagingAbility()
        {
            if (Abilities == null) return null;
            foreach (var ability in Abilities)
            {
                if (ability != null && ability.EffectType == AbilityEffectType.Damage && ability.BasePower > 0)
                {
                    return ability;
                }
            }
            return null;
        }
    }
}
