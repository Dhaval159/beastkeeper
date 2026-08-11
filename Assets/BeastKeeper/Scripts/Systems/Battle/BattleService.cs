using UnityEngine;
using UnityEngine.SceneManagement;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Runtime implementation of the battle service, managing transitions and player state preservation.
    /// Scene names and fallback positions use named constants so they are not scattered string literals.
    /// </summary>
    public class BattleService : IBattleService
    {
        public const string BattleSceneName = "Prototype_Battle";
        public const string ExplorationSceneName = "Prototype_Exploration";
        private static readonly Vector3 VillageFallbackPosition = new Vector3(0f, -5f, 0f);

        private MonsterData activeEnemyData;
        private int activeEnemyLevel = 1;
        private Vector3 previousPlayerPosition = new Vector3(0f, -5f, 0f);
        private bool isBattleActive = false;
        private bool lastBattleWon = false;

        public MonsterData ActiveEnemyData => activeEnemyData;
        public int ActiveEnemyLevel => activeEnemyLevel;
        public bool IsBattleActive => isBattleActive;

        public void TriggerBattle(MonsterData enemyData, int level = 1)
        {
            if (isBattleActive) return;
            if (enemyData == null)
            {
                Debug.LogWarning("[BattleService] TriggerBattle called with null enemy data.");
                return;
            }

            activeEnemyData = enemyData;
            activeEnemyLevel = Mathf.Max(1, level);
            isBattleActive = true;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                previousPlayerPosition = player.transform.position;
            }

            SceneManager.LoadScene(BattleSceneName);
        }

        public void EndBattle(bool playerWon)
        {
            if (!isBattleActive) return;

            isBattleActive = false;
            lastBattleWon = playerWon;

            SceneManager.LoadScene(ExplorationSceneName);
            SceneManager.sceneLoaded += OnExplorationSceneLoaded;
        }

        private void OnExplorationSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnExplorationSceneLoaded;

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;

            if (lastBattleWon)
            {
                // Victory or escape: restore the position the player had before the battle.
                player.transform.position = previousPlayerPosition;
            }
            else
            {
                // Defeat fallback: teleport back to the village center.
                player.transform.position = VillageFallbackPosition;
            }

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = player.transform.position;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
