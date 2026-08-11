using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BeastKeeper.Gameplay.Battle
{
    /// <summary>
    /// UI Manager handling rendering of unit statistics and binding battle actions to interface buttons.
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        [Header("Player Fields")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text playerHpText;
        [SerializeField] private Slider playerHpSlider;

        [Header("Enemy Fields")]
        [SerializeField] private TMP_Text enemyNameText;
        [SerializeField] private TMP_Text enemyHpText;
        [SerializeField] private Slider enemyHpSlider;

        [Header("Action Buttons")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button observeButton;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button runButton;

        [Header("Log Console")]
        [SerializeField] private TMP_Text logText;

        private BattleController controller;

        public void SetupUI(BattleController battleController)
        {
            controller = battleController;

            if (playerNameText != null) playerNameText.text = $"{controller.PlayerUnit.Name} (Lv.{controller.PlayerUnit.Level})";
            if (enemyNameText != null) enemyNameText.text = $"{controller.EnemyUnit.Name} (Lv.{controller.EnemyUnit.Level})";

            if (attackButton != null)
            {
                attackButton.onClick.RemoveAllListeners();
                attackButton.onClick.AddListener(controller.OnPlayerAttack);
            }
            if (observeButton != null)
            {
                observeButton.onClick.RemoveAllListeners();
                observeButton.onClick.AddListener(controller.OnPlayerObserve);
            }
            if (itemButton != null)
            {
                itemButton.onClick.RemoveAllListeners();
                itemButton.onClick.AddListener(controller.OnPlayerItem);
            }
            if (runButton != null)
            {
                runButton.onClick.RemoveAllListeners();
                runButton.onClick.AddListener(controller.OnPlayerRun);
            }

            UpdateHPDisplay();
        }

        public void UpdateHPDisplay()
        {
            if (controller == null) return;

            if (playerHpText != null) playerHpText.text = $"HP: {controller.PlayerUnit.CurrentHp}/{controller.PlayerUnit.MaxHp}";
            if (playerHpSlider != null)
            {
                playerHpSlider.maxValue = controller.PlayerUnit.MaxHp;
                playerHpSlider.value = controller.PlayerUnit.CurrentHp;
            }

            if (enemyHpText != null) enemyHpText.text = $"HP: {controller.EnemyUnit.CurrentHp}/{controller.EnemyUnit.MaxHp}";
            if (enemyHpSlider != null)
            {
                enemyHpSlider.maxValue = controller.EnemyUnit.MaxHp;
                enemyHpSlider.value = controller.EnemyUnit.CurrentHp;
            }
        }

        public void SetButtonsInteractable(bool interactable)
        {
            if (attackButton != null) attackButton.interactable = interactable;
            if (observeButton != null) observeButton.interactable = interactable;
            if (itemButton != null) itemButton.interactable = interactable;
            if (runButton != null) runButton.interactable = interactable;
        }

        public void LogMessage(string message)
        {
            if (logText != null)
            {
                logText.text = message;
            }
            Debug.Log($"[BattleLog] {message}");
        }
    }
}
