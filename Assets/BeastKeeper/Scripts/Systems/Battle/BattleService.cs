using UnityEngine;
using UnityEngine.SceneManagement;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Systems
{
    /// <summary>
    /// Runtime implementation of the battle service, managing transitions and player state preservation.
    /// </summary>
    public class BattleService : IBattleService
    {
        private MonsterData activeEnemyData;
        private string previousScenePath;
        private Vector3 previousPlayerPosition = new Vector3(0f, -5f, 0f);
        private bool isBattleActive = false;
        private bool lastBattleWon = false;

        public MonsterData ActiveEnemyData => activeEnemyData;
        public bool IsBattleActive => isBattleActive;

        public void TriggerBattle(MonsterData enemyData)
        {
            if (isBattleActive) return;

            activeEnemyData = enemyData;
            isBattleActive = true;
            previousScenePath = SceneManager.GetActiveScene().path;

            // Find player position in the scene to preserve it
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                previousPlayerPosition = player.transform.position;
            }

            SceneManager.LoadScene("Prototype_Battle");
        }

        public void EndBattle(bool playerWon)
        {
            if (!isBattleActive) return;

            isBattleActive = false;
            lastBattleWon = playerWon;

            SceneManager.LoadScene("Prototype_Exploration");
            SceneManager.sceneLoaded += OnExplorationSceneLoaded;
        }

        private void OnExplorationSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnExplorationSceneLoaded;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                if (lastBattleWon)
                {
                    player.transform.position = previousPlayerPosition;
                }
                else
                {
                    // Defeat fallback - teleport back to the village center
                    player.transform.position = new Vector3(0f, -5f, 0f);
                }

                // Sync Rigidbody2D position
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position = player.transform.position;
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }
    }
}
