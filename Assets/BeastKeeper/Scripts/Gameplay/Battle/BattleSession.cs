using System;
using UnityEngine;
using BeastKeeper.Core;
using BeastKeeper.Data;

namespace BeastKeeper.Gameplay.Battle
{
    public enum BattleState
    {
        Start,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat,
        Escape
    }

    public enum BattleAction
    {
        None,
        Attack,
        Observe,
        Item,
        Run
    }

    /// <summary>
    /// Pure turn-based battle state machine. Synchronous by design so it can be unit tested
    /// without scenes or coroutines; the BattleController provides pacing and presentation.
    /// </summary>
    public class BattleSession
    {
        public const int PotionHealAmount = 20;
        public const int VictoryXpPerEnemyLevel = 25;
        private const float EnemyAiAttackAdvantageThreshold = 5f;

        public event Action<string> Log;
        private void LogMessage(string msg) { Log?.Invoke(msg); }
        public event Action StateChanged;

        public BattleUnit PlayerUnit { get; private set; }
        public BattleUnit EnemyUnit { get; private set; }
        public BattleState CurrentState { get; private set; }
        public bool ActionInProgress { get; private set; }
        public bool IsBattleEnded { get; private set; }
        public bool PlayerWon { get; private set; }
        public int PotionCount { get; private set; }

        private ItemData potionItem;
        private bool playerActedThisRound;
        private bool enemyActedThisRound;
        private bool victoryRewardsAwarded;
        private readonly Func<float> randomProvider;

        /// <summary>
        /// Only CurrentState == PlayerTurn && !ActionInProgress && !IsBattleEnded may accept input.
        /// </summary>
        public bool CanAcceptAction => CurrentState == BattleState.PlayerTurn && !ActionInProgress && !IsBattleEnded;

        public BattleSession()
            : this(null)
        {
        }

        public BattleSession(Func<float> randomProvider)
        {
            this.randomProvider = randomProvider ?? (() => UnityEngine.Random.value);
        }

        /// <summary>
        /// Sets up both units and resets to the Start state. Call StartRound() to begin the flow.
        /// </summary>
        public void Initialize(BattleUnit player, BattleUnit enemy, ItemData potionItem = null, int potionCount = 1)
        {
            if (player == null || enemy == null)
            {
                Debug.LogError("[BattleSession] Cannot initialize a session without both units.");
                return;
            }

            PlayerUnit = player;
            EnemyUnit = enemy;
            this.potionItem = potionItem ?? ItemData.CreateRuntime("potion", "Potion", ItemType.Consumable, PotionHealAmount);
            PotionCount = Mathf.Max(0, potionCount);
            playerActedThisRound = false;
            enemyActedThisRound = false;
            victoryRewardsAwarded = false;
            ActionInProgress = false;
            IsBattleEnded = false;
            PlayerWon = false;
            CurrentState = BattleState.Start;
        }

        /// <summary>
        /// Begins a fresh round, ordering actors by speed (ties favor the player).
        /// </summary>
        public void StartRound()
        {
            if (IsBattleEnded || PlayerUnit == null || EnemyUnit == null) return;

            playerActedThisRound = false;
            enemyActedThisRound = false;
            bool playerActsFirst = PlayerUnit.Speed >= EnemyUnit.Speed;
            TransitionToState(playerActsFirst ? BattleState.PlayerTurn : BattleState.EnemyTurn);
        }

        public void ExecuteAction(BattleAction action)
        {
            if (!CanAcceptAction) return;

            switch (action)
            {
                case BattleAction.Attack: PlayerAttack(); break;
                case BattleAction.Observe: PlayerObserve(); break;
                case BattleAction.Item: PlayerItem(); break;
                case BattleAction.Run: PlayerRun(); break;
                default: LogMessage("Unknown action."); break;
            }
        }

        public void PlayerAttack()
        {
            if (!CanAcceptAction) return;

            ActionInProgress = true;
            var ability = PlayerUnit.GetFirstDamagingAbility();
            if (ability == null)
            {
                Debug.LogWarning("[BattleSession] Player has no damaging ability; using a basic strike.");
            }

            int damage = BattleDamageCalculator.CalculateDamage(PlayerUnit, ability, EnemyUnit);
            EnemyUnit.ApplyDamage(damage);
            LogMessage($"{PlayerUnit.Name} used {(ability != null ? ability.DisplayNameOrAssetName : "a basic strike")}!");
            LogMessage($"{EnemyUnit.Name} took {damage} damage.");
            RaiseStateChanged();

            if (EnemyUnit.IsDefeated)
            {
                TransitionToState(BattleState.Victory);
                ActionInProgress = false;
                return;
            }

            AdvanceAfterPlayerAction();
            ActionInProgress = false;
        }

        public void PlayerObserve()
        {
            if (!CanAcceptAction) return;

            ActionInProgress = true;
            var enemy = EnemyUnit;
            LogMessage($"{enemy.Name} (Lv.{enemy.Level}) - HP: {enemy.CurrentHp}/{enemy.MaxHp} | ATK: {enemy.Attack} | DEF: {enemy.Defense} | SPD: {enemy.Speed}");
            AdvanceAfterPlayerAction();
            ActionInProgress = false;
        }

        public void PlayerItem()
        {
            if (!CanAcceptAction) return;

            if (potionItem == null || PotionCount <= 0)
            {
                LogMessage("No usable items.");
                return;
            }

            ActionInProgress = true;
            int healed = PlayerUnit.Heal(potionItem.Value);
            PotionCount--;
            LogMessage($"Used {potionItem.DisplayNameOrAssetName}! Recovered {healed} HP.");
            RaiseStateChanged();
            AdvanceAfterPlayerAction();
            ActionInProgress = false;
        }

        public void PlayerRun()
        {
            if (!CanAcceptAction) return;

            ActionInProgress = true;
            float chance = BattleEscapeCalculator.GetEscapeChance(PlayerUnit, EnemyUnit);
            bool escaped = randomProvider() < chance;
            if (escaped)
            {
                TransitionToState(BattleState.Escape);
                ActionInProgress = false;
                return;
            }

            LogMessage("Couldn't escape!");
            AdvanceAfterPlayerAction();
            ActionInProgress = false;
        }

        /// <summary>
        /// Resolves the enemy's turn using simple rule-based AI, then advances the flow.
        /// </summary>
        public void ExecuteEnemyTurn()
        {
            if (IsBattleEnded || CurrentState != BattleState.EnemyTurn) return;

            ActionInProgress = true;
            var ability = SelectEnemyAbility();

            if (ability != null && ability.EffectType == AbilityEffectType.ReduceAttack)
            {
                int reduction = Mathf.Max(1, ability.EffectValue);
                PlayerUnit.Attack = Mathf.Max(1, PlayerUnit.Attack - reduction);
                LogMessage($"{EnemyUnit.Name} used {ability.DisplayNameOrAssetName}!");
                LogMessage($"{PlayerUnit.Name}'s Attack fell by {reduction}!");
            }
            else
            {
                int damage = BattleDamageCalculator.CalculateDamage(EnemyUnit, ability, PlayerUnit);
                PlayerUnit.ApplyDamage(damage);
                LogMessage($"{EnemyUnit.Name} used {(ability != null ? ability.DisplayNameOrAssetName : "a basic strike")}!");
                LogMessage($"{PlayerUnit.Name} took {damage} damage.");
            }

            RaiseStateChanged();

            if (PlayerUnit.IsDefeated)
            {
                TransitionToState(BattleState.Defeat);
                ActionInProgress = false;
                return;
            }

            AdvanceAfterEnemyAction();
            ActionInProgress = false;
        }

        /// <summary>
        /// Simple rule-based enemy AI: prefer Growl when the player's attack notably exceeds the enemy's
        /// base attack, otherwise prefer a damaging ability. Falls back safely when abilities are missing.
        /// </summary>
        private MonsterAbility SelectEnemyAbility()
        {
            bool preferGrowl = PlayerUnit.Attack > EnemyUnit.BaseAttack + EnemyAiAttackAdvantageThreshold;

            if (preferGrowl)
            {
                var growl = FindAbility(EnemyUnit, AbilityEffectType.ReduceAttack);
                if (growl != null) return growl;
            }

            var damaging = EnemyUnit.GetFirstDamagingAbility();
            if (damaging != null) return damaging;

            if (preferGrowl)
            {
                Debug.LogWarning("[BattleSession] Enemy has no Growl ability; using a basic strike instead.");
                return null;
            }

            var reduce = FindAbility(EnemyUnit, AbilityEffectType.ReduceAttack);
            if (reduce != null) return reduce;

            Debug.LogWarning("[BattleSession] Enemy has no usable abilities; using a basic strike.");
            return null;
        }

        private static MonsterAbility FindAbility(BattleUnit unit, AbilityEffectType effectType)
        {
            if (unit == null || unit.Abilities == null) return null;
            foreach (var ability in unit.Abilities)
            {
                if (ability != null && ability.EffectType == effectType) return ability;
            }
            return null;
        }

        private void AdvanceAfterPlayerAction()
        {
            playerActedThisRound = true;
            if (enemyActedThisRound) StartRound();
            else TransitionToState(BattleState.EnemyTurn);
        }

        private void AdvanceAfterEnemyAction()
        {
            enemyActedThisRound = true;
            if (playerActedThisRound) StartRound();
            else TransitionToState(BattleState.PlayerTurn);
        }

        private void TransitionToState(BattleState newState)
        {
            CurrentState = newState;

            switch (newState)
            {
                case BattleState.Victory:
                    IsBattleEnded = true;
                    PlayerWon = true;
                    LogMessage($"{EnemyUnit.Name} defeated!");
                    AwardVictoryRewards();
                    break;
                case BattleState.Defeat:
                    IsBattleEnded = true;
                    PlayerWon = false;
                    LogMessage("You were defeated...");
                    EventBus.Raise(new BattleDefeatEvent { EnemyId = EnemyUnit.DataId, EnemyLevel = EnemyUnit.Level });
                    break;
                case BattleState.Escape:
                    IsBattleEnded = true;
                    PlayerWon = true;
                    LogMessage("Escaped safely!");
                    break;
            }

            RaiseStateChanged();
        }

        /// <summary>
        /// Publishes victory events exactly once per battle. XP is awarded by subscribers
        /// (ProgressionEventBridge), never directly by the battle session.
        /// </summary>
        private void AwardVictoryRewards()
        {
            if (victoryRewardsAwarded) return;
            victoryRewardsAwarded = true;

            int xp = Mathf.Max(1, EnemyUnit.Level) * VictoryXpPerEnemyLevel;
            LogMessage($"+{xp} XP");

            EventBus.Raise(new MonsterDefeatedEvent { MonsterId = EnemyUnit.DataId, Level = EnemyUnit.Level });
            EventBus.Raise(new BattleVictoryEvent
            {
                EnemyId = EnemyUnit.DataId,
                EnemyLevel = EnemyUnit.Level,
                ExperienceAwarded = xp
            });
        }

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke();
        }
    }
}
