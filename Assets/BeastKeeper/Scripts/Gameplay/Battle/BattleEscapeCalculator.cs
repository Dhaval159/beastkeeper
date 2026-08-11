using UnityEngine;

namespace BeastKeeper.Gameplay.Battle
{
    /// <summary>
    /// Computes the escape chance: clamp(0.5 + (playerSpeed - enemySpeed) * 0.05, 0.2, 0.95).
    /// </summary>
    public static class BattleEscapeCalculator
    {
        public const float MinChance = 0.2f;
        public const float MaxChance = 0.95f;
        public const float SpeedFactor = 0.05f;

        public static float GetEscapeChance(BattleUnit player, BattleUnit enemy)
        {
            if (player == null || enemy == null) return 0.5f;
            float chance = 0.5f + (player.Speed - enemy.Speed) * SpeedFactor;
            return Mathf.Clamp(chance, MinChance, MaxChance);
        }
    }
}
