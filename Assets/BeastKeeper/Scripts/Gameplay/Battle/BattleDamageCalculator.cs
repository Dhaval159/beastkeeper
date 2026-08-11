using UnityEngine;
using BeastKeeper.Data;

namespace BeastKeeper.Gameplay.Battle
{
    /// <summary>
    /// Centralized damage calculation. Structured so future crit/type modifiers can be added here.
    /// </summary>
    public static class BattleDamageCalculator
    {
        /// <summary>
        /// dmg = attacker.Attack + ability.BasePower - defender.Defense, minimum 1.
        /// A null ability acts as a basic strike with no bonus power.
        /// </summary>
        public static int CalculateDamage(BattleUnit attacker, MonsterAbility ability, BattleUnit defender)
        {
            if (attacker == null || defender == null)
            {
                Debug.LogWarning("[Battle] Cannot calculate damage with a null combatant.");
                return 1;
            }

            int power = ability != null ? Mathf.Max(0, ability.BasePower) : 0;
            return Mathf.Max(1, attacker.Attack + power - defender.Defense);
        }
    }
}
