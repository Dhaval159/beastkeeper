using System.Collections.Generic;
using UnityEngine;

namespace BeastKeeper.Data
{
    /// <summary>
    /// ScriptableObject representing a monster type's base statistics, assets, and learnable abilities.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMonsterData", menuName = "Beast Keeper/Data/Monster/Monster")]
    public class MonsterData : EntityData
    {
        [Header("Stats")]
        [SerializeField] private int baseHp;
        [SerializeField] private int baseSpeed;
        [SerializeField] private int baseAttack;
        [SerializeField] private int baseDefense;

        [Header("Assets")]
        [SerializeField] private Sprite battleSprite;
        [SerializeField] private Sprite overworldSprite;

        [Header("Abilities")]
        [SerializeField] private List<MonsterAbility> learnableAbilities;

        public int BaseHp => baseHp;
        public int BaseSpeed => baseSpeed;
        public int BaseAttack => baseAttack;
        public int BaseDefense => baseDefense;

        public Sprite BattleSprite => battleSprite;
        public Sprite OverworldSprite => overworldSprite;
        public IReadOnlyList<MonsterAbility> LearnableAbilities => learnableAbilities;
    }
}
