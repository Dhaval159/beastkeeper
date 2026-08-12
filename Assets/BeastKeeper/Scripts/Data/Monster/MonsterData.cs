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

        /// <summary>
        /// Creates an in-memory MonsterData for runtime fallback or testing without touching Unity Editor serialization.
        /// </summary>
        public static MonsterData CreateRuntime(string id, string displayName, int baseHp, int baseSpeed, int baseAttack, int baseDefense, params MonsterAbility[] abilities)
        {
            var data = CreateInstance<MonsterData>();
            data.name = displayName;
            data.InitializeBase(id, displayName);
            data.baseHp = Mathf.Max(0, baseHp);
            data.baseSpeed = Mathf.Max(0, baseSpeed);
            data.baseAttack = Mathf.Max(0, baseAttack);
            data.baseDefense = Mathf.Max(0, baseDefense);
            data.learnableAbilities = new List<MonsterAbility>();
            if (abilities != null)
            {
                foreach (var ability in abilities)
                {
                    if (ability != null) data.learnableAbilities.Add(ability);
                }
            }
            return data;
        }
    }
}
