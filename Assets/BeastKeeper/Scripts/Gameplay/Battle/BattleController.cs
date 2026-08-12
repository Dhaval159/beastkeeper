using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastKeeper.Data;
using BeastKeeper.Systems;

namespace BeastKeeper.Gameplay.Battle
{
    /// <summary>
    /// Monobehaviour that owns the battle turn flow: it drives a BattleSession,
    /// adds pacing, wires UI/input, and handles the end-of-battle exit.
    /// </summary>
    public class BattleController : MonoBehaviour
    {
        [SerializeField] private BattleUI battleUI;
        [SerializeField] private MonsterData playerMonsterData;
        [SerializeField] private SpriteRenderer enemySpriteRenderer;
        [SerializeField] private SpriteRenderer playerSpriteRenderer;

        [Header("Flow Timing")]
        [SerializeField] private float startSequenceDelay = 0.2f;
        [SerializeField] private float enemyThinkDelay = 0.3f;
        [SerializeField] private float actionPacingDelay = 0.3f;
        [SerializeField] private float endSequenceTimeout = 5f;

        private BattleSession session;
        private BattleAction pendingAction = BattleAction.None;
        private bool battleServiceEnded;

        public BattleState CurrentState => session != null ? session.CurrentState : BattleState.Start;
        public BattleUnit PlayerUnit => session != null ? session.PlayerUnit : null;
        public BattleUnit EnemyUnit => session != null ? session.EnemyUnit : null;

        private void Start()
        {
            BattleEventSystemBootstrapper.EnsureEventSystem(transform);
            InitializeBattle();
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        private void InitializeBattle()
        {
            IProgressionSystem progression = ProgressionServiceManager.Get();
            session = new BattleSession();
            session.Log += LogMessage;
            session.StateChanged += OnSessionStateChanged;

            IBattleService battleService = BattleServiceManager.Get();

            MonsterData enemyData = battleService.ActiveEnemyData;
            if (enemyData == null)
            {
                Debug.LogWarning("[BattleController] No active enemy data found; using a fallback enemy.");
                enemyData = CreateFallbackEnemyData();
            }

            MonsterData playerData = playerMonsterData != null ? playerMonsterData : CreateFallbackPlayerData();

            BattleUnit playerUnit = BattleUnit.FromMonsterData(playerData, Mathf.Max(1, progression.PlayerLevel));
            playerUnit.IsPlayer = true;
            BattleUnit enemyUnit = BattleUnit.FromMonsterData(enemyData, Mathf.Max(1, battleService.ActiveEnemyLevel));

            session.Initialize(playerUnit, enemyUnit);

            if (battleUI != null)
            {
                battleUI.SetupUI(this);
            }

            UpdateCombatantSprites();
            StartCoroutine(BattleStartSequence());
        }

        private void UpdateCombatantSprites()
        {
            if (session == null) return;

            var enemySr = enemySpriteRenderer != null ? enemySpriteRenderer : GameObject.Find("EnemyBeast")?.GetComponent<SpriteRenderer>();
            if (enemySr != null && session.EnemyUnit != null && session.EnemyUnit.BattleSprite != null)
            {
                enemySr.sprite = session.EnemyUnit.BattleSprite;
            }

            var playerSr = playerSpriteRenderer != null ? playerSpriteRenderer : GameObject.Find("PlayerBeast")?.GetComponent<SpriteRenderer>();
            if (playerSr != null && session.PlayerUnit != null && session.PlayerUnit.BattleSprite != null)
            {
                playerSr.sprite = session.PlayerUnit.BattleSprite;
            }
        }

        private static MonsterData CreateFallbackEnemyData()
        {
            var bite = MonsterAbility.CreateRuntime("bite", "Bite", 15, 0, AbilityEffectType.Damage);
            var growl = MonsterAbility.CreateRuntime("growl", "Growl", 0, 0, AbilityEffectType.ReduceAttack, 2);
            return MonsterData.CreateRuntime("mossfang", "Mossfang", 50, 8, 12, 4, bite, growl);
        }

        private static MonsterData CreateFallbackPlayerData()
        {
            var bite = MonsterAbility.CreateRuntime("bite", "Bite", 15, 0, AbilityEffectType.Damage);
            return MonsterData.CreateRuntime("temp_test_beast", "TEMPORARY TEST BEAST", 55, 9, 14, 6, bite);
        }

        private IEnumerator BattleStartSequence()
        {
            LogMessage($"A wild {session.EnemyUnit.Name} appeared!");
            SetButtonsInteractable(false);
            yield return new WaitForSeconds(startSequenceDelay);

            session.StartRound();
            StartCoroutine(BattleFlowRoutine());
        }

        private IEnumerator BattleFlowRoutine()
        {
            while (!session.IsBattleEnded)
            {
                switch (session.CurrentState)
                {
                    case BattleState.PlayerTurn:
                        if (pendingAction != BattleAction.None)
                        {
                            BattleAction action = pendingAction;
                            pendingAction = BattleAction.None;
                            session.ExecuteAction(action);
                            yield return new WaitForSeconds(actionPacingDelay);
                            if (session.CurrentState == BattleState.PlayerTurn)
                            {
                                SetButtonsInteractable(true);
                            }
                        }
                        else
                        {
                            yield return null;
                        }
                        break;

                    case BattleState.EnemyTurn:
                        LogMessage($"{session.EnemyUnit.Name} is thinking...");
                        yield return new WaitForSeconds(enemyThinkDelay);
                        session.ExecuteEnemyTurn();
                        break;

                    default:
                        yield return null;
                        break;
                }
            }

            StartCoroutine(BattleEndSequence());
        }

        private IEnumerator BattleEndSequence()
        {
            SetButtonsInteractable(false);

            string endMessage;
            switch (session.CurrentState)
            {
                case BattleState.Victory: endMessage = $"Victory! You defeated {session.EnemyUnit.Name}"; break;
                case BattleState.Defeat: endMessage = "Defeat! You were knocked out..."; break;
                case BattleState.Escape: endMessage = "Escaped safely!"; break;
                default: endMessage = "Battle ended."; break;
            }

            LogMessage(endMessage);
            LogMessage("Press Enter, Space or Escape to exit battle...");

            float timer = 0f;
            while (timer < endSequenceTimeout)
            {
                timer += Time.deltaTime;
                if (BattleKeyboardInput.IsEndSequenceAdvancePressed(Keyboard.current)) break;
                yield return null;
            }

            EndBattleNow();
        }

        /// <summary>
        /// UI buttons and keyboard both funnel through these methods.
        /// </summary>
        public void OnPlayerAttack() => DispatchToSession(BattleAction.Attack);
        public void OnPlayerObserve() => DispatchToSession(BattleAction.Observe);
        public void OnPlayerItem() => DispatchToSession(BattleAction.Item);
        public void OnPlayerRun() => DispatchToSession(BattleAction.Run);

        private void DispatchToSession(BattleAction action)
        {
            if (session == null || session.IsBattleEnded) return;
            if (!session.CanAcceptAction) return;

            pendingAction = action;
            SetButtonsInteractable(false);
        }

        private void HandleKeyboardInput()
        {
            if (session == null) return;

            if (session.IsBattleEnded)
            {
                if (BattleKeyboardInput.IsEndSequenceAdvancePressed(Keyboard.current))
                {
                    EndBattleNow();
                }
                return;
            }

            BattleAction action = BattleKeyboardInput.ReadAction(Keyboard.current);
            if (action != BattleAction.None)
            {
                DispatchToSession(action);
            }
        }

        private void EndBattleNow()
        {
            if (session == null || !session.IsBattleEnded || battleServiceEnded) return;
            battleServiceEnded = true;
            BattleServiceManager.Get().EndBattle(session.PlayerWon);
        }

        private void OnSessionStateChanged()
        {
            if (battleUI == null) return;
            battleUI.UpdateHPDisplay();
            SetButtonsInteractable(session.CanAcceptAction);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (battleUI != null) battleUI.SetButtonsInteractable(interactable);
        }

        private void LogMessage(string message)
        {
            if (battleUI != null) battleUI.LogMessage(message);
            else Debug.Log($"[Battle] {message}");
        }
    }
}
