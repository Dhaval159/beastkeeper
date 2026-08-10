using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeastKeeper.Data;
using BeastKeeper.Systems;

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

    /// <summary>
    /// Class representing a combatant's statistics during battle.
    /// </summary>
    public class BattleUnit
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int MaxHp { get; set; }
        public int CurrentHp { get; set; }
        public int BaseAttack { get; set; }
        public int Attack { get; set; } // Current attack (may be debuffed)
        public int Defense { get; set; }
        public int Speed { get; set; }
        public bool IsPlayer { get; set; }
        public List<MonsterAbility> Abilities { get; set; } = new List<MonsterAbility>();
    }

    /// <summary>
    /// Controller managing turn-based battle flow, state transitions, AI decisions, and stat resolution.
    /// </summary>
    public class BattleController : MonoBehaviour
    {
        [SerializeField] private BattleUI battleUI;

        private BattleState currentState;
        private BattleUnit playerUnit;
        private BattleUnit enemyUnit;
        private bool playerFirstInRound = true;
        private bool secondTurnExecutedInRound = false;

        public BattleState CurrentState => currentState;
        public BattleUnit PlayerUnit => playerUnit;
        public BattleUnit EnemyUnit => enemyUnit;

        private void Start()
        {
            InitializeBattle();
        }

        private void InitializeBattle()
        {
            currentState = BattleState.Start;
            
            // Get active enemy data from the BattleService
            MonsterData enemyData = BattleServiceManager.Get().ActiveEnemyData;

            // Fallback for direct testing in editor
            if (enemyData == null)
            {
                // Create a temporary dummy Mossfang if not set
                enemyData = ScriptableObject.CreateInstance<MonsterData>();
                enemyData.name = "Mossfang";
                // Let's set some dummy values
                var serializedObj = new UnityEditor.SerializedObject(enemyData);
                serializedObj.FindProperty("baseHp").intValue = 50;
                serializedObj.FindProperty("baseSpeed").intValue = 8;
                serializedObj.FindProperty("baseAttack").intValue = 12;
                serializedObj.FindProperty("baseDefense").intValue = 4;
                serializedObj.ApplyModifiedProperties();
            }

            // Setup Player Unit
            playerUnit = new BattleUnit
            {
                Name = "Companion",
                Level = 5,
                MaxHp = 100,
                CurrentHp = 100,
                BaseAttack = 15,
                Attack = 15,
                Defense = 5,
                Speed = 10,
                IsPlayer = true
            };

            // Set up Player default ability: Bite (created programmatically or dynamically)
            MonsterAbility biteAbility = ScriptableObject.CreateInstance<MonsterAbility>();
            var biteSerialized = new UnityEditor.SerializedObject(biteAbility);
            biteSerialized.FindProperty("basePower").intValue = 15;
            biteSerialized.FindProperty("effectType").intValue = (int)AbilityEffectType.Damage;
            biteSerialized.ApplyModifiedProperties();
            biteAbility.name = "Bite";
            playerUnit.Abilities.Add(biteAbility);

            // Setup Enemy Unit
            enemyUnit = new BattleUnit
            {
                Name = enemyData.name,
                Level = 4,
                MaxHp = enemyData.BaseHp,
                CurrentHp = enemyData.BaseHp,
                BaseAttack = enemyData.BaseAttack,
                Attack = enemyData.BaseAttack,
                Defense = enemyData.BaseDefense,
                Speed = enemyData.BaseSpeed,
                IsPlayer = false
            };

            // Assign enemy abilities from MonsterData
            if (enemyData.LearnableAbilities != null && enemyData.LearnableAbilities.Count > 0)
            {
                foreach (var ability in enemyData.LearnableAbilities)
                {
                    enemyUnit.Abilities.Add(ability);
                }
            }
            else
            {
                // Enemy default abilities fallback
                MonsterAbility enemyBite = ScriptableObject.CreateInstance<MonsterAbility>();
                var eBiteSerialized = new UnityEditor.SerializedObject(enemyBite);
                eBiteSerialized.FindProperty("basePower").intValue = 15;
                eBiteSerialized.FindProperty("effectType").intValue = (int)AbilityEffectType.Damage;
                eBiteSerialized.ApplyModifiedProperties();
                enemyBite.name = "Bite";

                MonsterAbility enemyGrowl = ScriptableObject.CreateInstance<MonsterAbility>();
                var eGrowlSerialized = new UnityEditor.SerializedObject(enemyGrowl);
                eGrowlSerialized.FindProperty("basePower").intValue = 0;
                eGrowlSerialized.FindProperty("effectType").intValue = (int)AbilityEffectType.ReduceAttack;
                eGrowlSerialized.FindProperty("effectValue").intValue = 2;
                eGrowlSerialized.ApplyModifiedProperties();
                enemyGrowl.name = "Growl";

                enemyUnit.Abilities.Add(enemyBite);
                enemyUnit.Abilities.Add(enemyGrowl);
            }

            // Sync UI
            if (battleUI != null)
            {
                battleUI.SetupUI(this);
            }

            StartCoroutine(BattleStartSequence());
        }

        private IEnumerator BattleStartSequence()
        {
            if (battleUI != null)
            {
                battleUI.LogMessage($"A wild {enemyUnit.Name} appeared!");
                battleUI.SetButtonsInteractable(false);
            }
            yield return new WaitForSeconds(1.5f);
            
            StartRound();
        }

        private void StartRound()
        {
            playerFirstInRound = playerUnit.Speed >= enemyUnit.Speed;
            secondTurnExecutedInRound = false;

            if (playerFirstInRound)
            {
                TransitionToState(BattleState.PlayerTurn);
            }
            else
            {
                TransitionToState(BattleState.EnemyTurn);
            }
        }

        private void TransitionToState(BattleState newState)
        {
            currentState = newState;
            
            if (battleUI != null)
            {
                battleUI.UpdateHPDisplay();
            }

            switch (currentState)
            {
                case BattleState.PlayerTurn:
                    if (battleUI != null)
                    {
                        battleUI.LogMessage($"It's your turn! Select an action.");
                        battleUI.SetButtonsInteractable(true);
                    }
                    break;
                case BattleState.EnemyTurn:
                    if (battleUI != null)
                    {
                        battleUI.SetButtonsInteractable(false);
                    }
                    StartCoroutine(EnemyTurnSequence());
                    break;
                case BattleState.Victory:
                    StartCoroutine(BattleEndSequence("Victory! You defeated " + enemyUnit.Name, true));
                    break;
                case BattleState.Defeat:
                    StartCoroutine(BattleEndSequence("Defeat! You were knocked out...", false));
                    break;
                case BattleState.Escape:
                    StartCoroutine(BattleEndSequence("Escaped safely!", true)); // Escape is treated as safe return
                    break;
            }
        }

        public void OnPlayerAttack()
        {
            if (currentState != BattleState.PlayerTurn) return;
            StartCoroutine(PlayerAttackSequence());
        }

        private IEnumerator PlayerAttackSequence()
        {
            if (battleUI != null) battleUI.SetButtonsInteractable(false);

            MonsterAbility ability = playerUnit.Abilities[0]; // Bite
            int damage = Mathf.Max(1, playerUnit.Attack + ability.BasePower - enemyUnit.Defense);
            enemyUnit.CurrentHp = Mathf.Max(0, enemyUnit.CurrentHp - damage);

            if (battleUI != null)
            {
                battleUI.LogMessage($"{playerUnit.Name} used {ability.name}!");
                battleUI.UpdateHPDisplay();
            }
            yield return new WaitForSeconds(1f);

            if (battleUI != null)
            {
                battleUI.LogMessage($"{enemyUnit.Name} took {damage} damage.");
                battleUI.UpdateHPDisplay();
            }
            yield return new WaitForSeconds(1f);

            // Check victory
            if (enemyUnit.CurrentHp <= 0)
            {
                TransitionToState(BattleState.Victory);
                yield break;
            }

            // Transition turn
            AdvanceRoundFlow(playerActsNext: false);
        }

        public void OnPlayerObserve()
        {
            if (currentState != BattleState.PlayerTurn) return;
            
            // Displays statistics in log but DOES NOT consume player turn
            if (battleUI != null)
            {
                battleUI.LogMessage($"{enemyUnit.Name} - HP: {enemyUnit.CurrentHp}/{enemyUnit.MaxHp} | ATK: {enemyUnit.Attack} | DEF: {enemyUnit.Defense} | SPD: {enemyUnit.Speed}");
            }
        }

        public void OnPlayerRun()
        {
            if (currentState != BattleState.PlayerTurn) return;
            TransitionToState(BattleState.Escape);
        }

        private IEnumerator EnemyTurnSequence()
        {
            if (battleUI != null)
            {
                battleUI.LogMessage($"{enemyUnit.Name} is thinking...");
            }
            yield return new WaitForSeconds(1.2f);

            // Randomly select between Bite and Growl
            MonsterAbility ability = enemyUnit.Abilities[UnityEngine.Random.Range(0, enemyUnit.Abilities.Count)];

            if (ability.EffectType == AbilityEffectType.Damage)
            {
                int damage = Mathf.Max(1, enemyUnit.Attack + ability.BasePower - playerUnit.Defense);
                playerUnit.CurrentHp = Mathf.Max(0, playerUnit.CurrentHp - damage);

                if (battleUI != null)
                {
                    battleUI.LogMessage($"{enemyUnit.Name} used {ability.name}!");
                    battleUI.UpdateHPDisplay();
                }
                yield return new WaitForSeconds(1f);

                if (battleUI != null)
                {
                    battleUI.LogMessage($"{playerUnit.Name} took {damage} damage.");
                    battleUI.UpdateHPDisplay();
                }
                yield return new WaitForSeconds(1f);
            }
            else if (ability.EffectType == AbilityEffectType.ReduceAttack)
            {
                playerUnit.Attack = Mathf.Max(1, playerUnit.Attack - ability.EffectValue);

                if (battleUI != null)
                {
                    battleUI.LogMessage($"{enemyUnit.Name} used {ability.name}!");
                    battleUI.UpdateHPDisplay();
                }
                yield return new WaitForSeconds(1f);

                if (battleUI != null)
                {
                    battleUI.LogMessage($"{playerUnit.Name}'s Attack fell by {ability.EffectValue}!");
                }
                yield return new WaitForSeconds(1f);
            }

            // Check defeat
            if (playerUnit.CurrentHp <= 0)
            {
                TransitionToState(BattleState.Defeat);
                yield break;
            }

            // Transition turn
            AdvanceRoundFlow(playerActsNext: true);
        }

        private void AdvanceRoundFlow(bool playerActsNext)
        {
            if (!secondTurnExecutedInRound)
            {
                secondTurnExecutedInRound = true;
                if (playerActsNext)
                {
                    TransitionToState(BattleState.PlayerTurn);
                }
                else
                {
                    TransitionToState(BattleState.EnemyTurn);
                }
            }
            else
            {
                // Round is finished, start new round
                StartRound();
            }
        }

        private IEnumerator BattleEndSequence(string endMessage, bool playerWon)
        {
            if (battleUI != null)
            {
                battleUI.LogMessage(endMessage);
                battleUI.LogMessage("Press any button or click action to exit battle...");
                battleUI.SetButtonsInteractable(false);
            }
            
            // Let the player click or press a key to exit
            bool exitTriggered = false;
            float timer = 0f;
            while (!exitTriggered && timer < 5f)
            {
                timer += Time.deltaTime;
                if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                {
                    exitTriggered = true;
                }
                yield return null;
            }

            BattleServiceManager.Get().EndBattle(playerWon);
        }
    }
}
