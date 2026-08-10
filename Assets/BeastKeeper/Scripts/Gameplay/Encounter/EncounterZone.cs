using UnityEngine;
using BeastKeeper.Data;
using BeastKeeper.Systems;

namespace BeastKeeper.Gameplay
{
    /// <summary>
    /// A zone that triggers a turn-based combat encounter when the player walks into it.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class EncounterZone : MonoBehaviour
    {
        [SerializeField] private MonsterData encounterMonster;

        public MonsterData EncounterMonster
        {
            get => encounterMonster;
            set => encounterMonster = value;
        }

        private void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && encounterMonster != null)
            {
                BattleServiceManager.Get().TriggerBattle(encounterMonster);
            }
        }
    }
}
