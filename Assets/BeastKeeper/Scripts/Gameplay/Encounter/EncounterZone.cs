using UnityEngine;
using BeastKeeper.Data;
using BeastKeeper.Systems;

namespace BeastKeeper.Gameplay
{
    /// <summary>
    /// A zone that triggers a turn-based combat encounter when the player walks into it.
    /// Reusable: it is never destroyed and ignores re-triggers while a battle is active.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class EncounterZone : MonoBehaviour
    {
        [SerializeField] private MonsterData encounterMonster;
        [SerializeField] private int encounterLevel = 1;
        [SerializeField] private float reTriggerCooldown = 1f;

        private float lastTriggerTime = -100f;

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
            if (!other.CompareTag("Player") || encounterMonster == null) return;

            var battleService = BattleServiceManager.Get();
            if (battleService.IsBattleActive) return;
            if (Time.time - lastTriggerTime < reTriggerCooldown) return;

            lastTriggerTime = Time.time;
            battleService.TriggerBattle(encounterMonster, encounterLevel);
        }
    }
}
