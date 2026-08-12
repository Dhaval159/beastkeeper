using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using BeastKeeper.Core;
using BeastKeeper.Data;
using BeastKeeper.Systems;
using BeastKeeper.Gameplay.Battle;

namespace BeastKeeper.Tests.EditMode
{
    public class BattleVerticalSliceTests
    {
        private static MonsterData BuildMossfang()
        {
            return MonsterData.CreateRuntime("mossfang", "Mossfang", 50, 8, 12, 4,
                MonsterAbility.CreateRuntime("bite", "Bite", 15, 0, AbilityEffectType.Damage),
                MonsterAbility.CreateRuntime("growl", "Growl", 0, 0, AbilityEffectType.ReduceAttack, 2));
        }

        private static MonsterData BuildCompanion()
        {
            return MonsterData.CreateRuntime("companion", "Companion", 50, 10, 15, 5,
                MonsterAbility.CreateRuntime("bite", "Bite", 15, 0, AbilityEffectType.Damage));
        }

        private static BattleUnit PlayerUnit()
        {
            var unit = BattleUnit.FromMonsterData(BuildCompanion(), 1);
            unit.IsPlayer = true;
            return unit;
        }

        private static BattleUnit EnemyUnit()
        {
            return BattleUnit.FromMonsterData(BuildMossfang(), 1);
        }

        [Test]
        public void BattleInitializesWithBothUnitsAndStartState()
        {
            var session = new BattleSession();
            var player = PlayerUnit();
            var enemy = EnemyUnit();

            session.Initialize(player, enemy);
            session.StartRound();

            Assert.That(session.PlayerUnit, Is.SameAs(player));
            Assert.That(session.EnemyUnit, Is.SameAs(enemy));
            Assert.That(session.IsBattleEnded, Is.False);
            Assert.That(session.CurrentState, Is.EqualTo(BattleState.PlayerTurn));
        }

        [Test]
        public void FasterUnitActsFirst()
        {
            var fastEnemy = BattleUnit.FromMonsterData(
                MonsterData.CreateRuntime("fast", "Fast", 40, 12, 10, 4,
                    MonsterAbility.CreateRuntime("bite", "Bite", 10, 0, AbilityEffectType.Damage)), 1);

            var session = new BattleSession();
            session.Initialize(PlayerUnit(), fastEnemy);
            session.StartRound();

            Assert.That(session.CurrentState, Is.EqualTo(BattleState.EnemyTurn));
        }

        [Test]
        public void PlayerAttackDealsExpectedDamage()
        {
            var session = new BattleSession();
            var player = PlayerUnit();
            var enemy = EnemyUnit();
            session.Initialize(player, enemy);
            session.StartRound();

            int before = enemy.CurrentHp;
            session.PlayerAttack();
            int expected = Mathf.Max(1, player.Attack + player.Abilities[0].BasePower - enemy.Defense);

            Assert.That(before - enemy.CurrentHp, Is.EqualTo(expected));
        }

        [Test]
        public void DamageCannotReduceHpBelowZeroOnEitherSide()
        {
            var enemy = EnemyUnit();
            enemy.ApplyDamage(9999);
            Assert.That(enemy.CurrentHp, Is.Zero);

            var player = PlayerUnit();
            player.ApplyDamage(9999);
            Assert.That(player.CurrentHp, Is.Zero);
        }

        [Test]
        public void DamageNeverFallsBelowOne()
        {
            var attacker = BattleUnit.FromMonsterData(
                MonsterData.CreateRuntime("weak", "Weak", 50, 8, 1, 1,
                    MonsterAbility.CreateRuntime("bite", "Bite", 5, 0, AbilityEffectType.Damage)), 1);
            var defender = BattleUnit.FromMonsterData(
                MonsterData.CreateRuntime("tank", "Tank", 100, 1, 1, 50), 1);

            int damage = BattleDamageCalculator.CalculateDamage(attacker, attacker.Abilities[0], defender);
            Assert.That(damage, Is.EqualTo(1));
        }

        [Test]
        public void DefeatingEnemyEndsBattleInVictory()
        {
            var session = new BattleSession();
            var enemy = EnemyUnit();
            enemy.CurrentHp = 5;
            session.Initialize(PlayerUnit(), enemy);
            session.StartRound();

            session.PlayerAttack();

            Assert.That(session.CurrentState, Is.EqualTo(BattleState.Victory));
            Assert.That(session.IsBattleEnded, Is.True);
            Assert.That(session.PlayerWon, Is.True);
        }

        [Test]
        public void PlayerDefeatedEndsBattleInDefeat()
        {
            var session = new BattleSession();
            var player = PlayerUnit();
            player.CurrentHp = 3;
            session.Initialize(player, EnemyUnit());
            session.StartRound();

            session.PlayerAttack();
            Assert.That(session.CurrentState, Is.EqualTo(BattleState.EnemyTurn));

            session.ExecuteEnemyTurn();

            Assert.That(session.CurrentState, Is.EqualTo(BattleState.Defeat));
            Assert.That(session.IsBattleEnded, Is.True);
            Assert.That(session.PlayerWon, Is.False);
        }

        [Test]
        public void EnemyGrowlReducesPlayerAttack()
        {
            var strongPlayer = BattleUnit.FromMonsterData(
                MonsterData.CreateRuntime("strong", "Strong", 50, 10, 30, 5,
                    MonsterAbility.CreateRuntime("bite", "Bite", 15, 0, AbilityEffectType.Damage)), 1);
            strongPlayer.IsPlayer = true;

            var session = new BattleSession();
            session.Initialize(strongPlayer, EnemyUnit());
            session.StartRound();

            session.PlayerAttack();
            Assert.That(session.CurrentState, Is.EqualTo(BattleState.EnemyTurn));

            int attackBefore = strongPlayer.Attack;
            session.ExecuteEnemyTurn();

            Assert.That(strongPlayer.Attack, Is.EqualTo(attackBefore - 2));
            Assert.That(session.CurrentState, Is.EqualTo(BattleState.PlayerTurn));
        }

        [Test]
        public void EnemyPrefersBiteWhenPlayerAttackIsNotNotablyHigher()
        {
            var session = new BattleSession();
            var player = PlayerUnit();
            session.Initialize(player, EnemyUnit());
            session.StartRound();

            session.PlayerAttack();
            Assert.That(session.CurrentState, Is.EqualTo(BattleState.EnemyTurn));

            int playerHpBefore = player.CurrentHp;
            session.ExecuteEnemyTurn();

            Assert.That(player.CurrentHp, Is.LessThan(playerHpBefore));
        }

        [Test]
        public void ObserveConsumesTurn()
        {
            var session = new BattleSession();
            session.Initialize(PlayerUnit(), EnemyUnit());
            session.StartRound();
            Assert.That(session.CurrentState, Is.EqualTo(BattleState.PlayerTurn));

            session.PlayerObserve();

            Assert.That(session.CurrentState, Is.EqualTo(BattleState.EnemyTurn));
            Assert.That(session.IsBattleEnded, Is.False);
        }

        [Test]
        public void ItemHealsAndConsumesTurn()
        {
            var session = new BattleSession();
            var player = PlayerUnit();
            player.CurrentHp = 30;
            session.Initialize(player, EnemyUnit());
            session.StartRound();

            session.PlayerItem();

            Assert.That(player.CurrentHp, Is.EqualTo(50));
            Assert.That(session.PotionCount, Is.Zero);
            Assert.That(session.CurrentState, Is.EqualTo(BattleState.EnemyTurn));
        }

        [Test]
        public void MissingUsableItemsDoesNotConsumeTurnOrCrash()
        {
            var session = new BattleSession();
            session.Initialize(PlayerUnit(), EnemyUnit(), potionCount: 0);
            session.StartRound();

            session.PlayerItem();

            Assert.That(session.CurrentState, Is.EqualTo(BattleState.PlayerTurn));
            Assert.That(session.PotionCount, Is.Zero);
        }

        [Test]
        public void RunSucceedsWhenRollSucceeds()
        {
            var session = new BattleSession(() => 0.01f);
            session.Initialize(PlayerUnit(), EnemyUnit());
            session.StartRound();

            session.PlayerRun();

            Assert.That(session.CurrentState, Is.EqualTo(BattleState.Escape));
            Assert.That(session.IsBattleEnded, Is.True);
        }

        [Test]
        public void RunFailureHandsTurnToEnemy()
        {
            var session = new BattleSession(() => 0.99f);
            session.Initialize(PlayerUnit(), EnemyUnit());
            session.StartRound();

            session.PlayerRun();

            Assert.That(session.CurrentState, Is.EqualTo(BattleState.EnemyTurn));
            Assert.That(session.IsBattleEnded, Is.False);
        }

        [Test]
        public void EscapeChanceFollowsSpeedFormula()
        {
            var player = PlayerUnit();
            var enemy = EnemyUnit();
            Assert.That(BattleEscapeCalculator.GetEscapeChance(player, enemy), Is.EqualTo(0.6f).Within(0.001f));

            var verySlowPlayer = BattleUnit.FromMonsterData(
                MonsterData.CreateRuntime("slow", "Slow", 50, 1, 15, 5,
                    MonsterAbility.CreateRuntime("bite", "Bite", 15, 0, AbilityEffectType.Damage)), 1);
            verySlowPlayer.IsPlayer = true;
            Assert.That(BattleEscapeCalculator.GetEscapeChance(verySlowPlayer, enemy), Is.EqualTo(0.2f).Within(0.001f));
        }

        [Test]
        public void NoActionsAcceptedAfterBattleEnds()
        {
            var session = new BattleSession();
            var enemy = EnemyUnit();
            enemy.CurrentHp = 5;
            session.Initialize(PlayerUnit(), enemy);
            session.StartRound();

            session.PlayerAttack();
            Assert.That(session.IsBattleEnded, Is.True);

            int hpAfterVictory = enemy.CurrentHp;
            session.PlayerAttack();
            session.ExecuteAction(BattleAction.Observe);

            Assert.That(enemy.CurrentHp, Is.EqualTo(hpAfterVictory));
            Assert.That(session.CurrentState, Is.EqualTo(BattleState.Victory));
        }

        [Test]
        public void RepeatedAttackInputDoesNotCauseDoubleTurn()
        {
            var session = new BattleSession();
            var enemy = EnemyUnit();
            session.Initialize(PlayerUnit(), enemy);
            session.StartRound();

            int hpBefore = enemy.CurrentHp;
            session.PlayerAttack();
            int hpAfterFirst = enemy.CurrentHp;

            session.PlayerAttack();
            session.PlayerAttack();

            Assert.That(hpAfterFirst, Is.LessThan(hpBefore));
            Assert.That(enemy.CurrentHp, Is.EqualTo(hpAfterFirst));
        }

        [Test]
        public void KeyboardKeysMapToBattleActions()
        {
            Assert.That(BattleKeyboardInput.KeyToAction(Key.Digit1), Is.EqualTo(BattleAction.Attack));
            Assert.That(BattleKeyboardInput.KeyToAction(Key.Digit2), Is.EqualTo(BattleAction.Observe));
            Assert.That(BattleKeyboardInput.KeyToAction(Key.Digit3), Is.EqualTo(BattleAction.Item));
            Assert.That(BattleKeyboardInput.KeyToAction(Key.Digit4), Is.EqualTo(BattleAction.Run));
            Assert.That(BattleKeyboardInput.KeyToAction(Key.A), Is.EqualTo(BattleAction.None));
        }

        [Test]
        public void ExecuteActionFunnelsToSameMethodsAsDirectCalls()
        {
            var sessionA = new BattleSession();
            var enemyA = EnemyUnit();
            sessionA.Initialize(PlayerUnit(), enemyA);
            sessionA.StartRound();
            int hpA = enemyA.CurrentHp;
            sessionA.ExecuteAction(BattleAction.Attack);
            int damageA = hpA - enemyA.CurrentHp;

            var sessionB = new BattleSession();
            var enemyB = EnemyUnit();
            sessionB.Initialize(PlayerUnit(), enemyB);
            sessionB.StartRound();
            int hpB = enemyB.CurrentHp;
            sessionB.PlayerAttack();
            int damageB = hpB - enemyB.CurrentHp;

            Assert.That(damageA, Is.EqualTo(damageB));
        }

        [Test]
        public void FromMonsterDataScalesStatsWithLevel()
        {
            var lvl1 = BattleUnit.FromMonsterData(BuildMossfang(), 1);
            var lvl5 = BattleUnit.FromMonsterData(BuildMossfang(), 5);

            Assert.That(lvl5.MaxHp, Is.EqualTo(lvl1.MaxHp + 4 * BattleUnit.HpPerLevel));
            Assert.That(lvl5.Attack, Is.EqualTo(lvl1.Attack + 4 * BattleUnit.StatPerLevel));
            Assert.That(lvl5.Speed, Is.EqualTo(lvl1.Speed + 4 * BattleUnit.SpeedPerLevel));
            Assert.That(lvl5.Abilities, Has.Count.EqualTo(2));
        }

        [Test]
        public void VictoryEventDrivesXpAwardBasedOnEnemyLevel()
        {
            var progression = new PlayerProgression();
            using (var bridge = new ProgressionEventBridge(progression))
            {
                var session = new BattleSession();
                var enemy = BattleUnit.FromMonsterData(BuildMossfang(), 3);
                enemy.CurrentHp = 1;
                session.Initialize(PlayerUnit(), enemy);
                session.StartRound();

                session.PlayerAttack();

                Assert.That(session.CurrentState, Is.EqualTo(BattleState.Victory));
                Assert.That(progression.PlayerExperience, Is.EqualTo(3 * BattleSession.VictoryXpPerEnemyLevel));
            }
        }

        [Test]
        public void VictoryDoesNotLevelUpWhenXpInsufficient()
        {
            var progression = new PlayerProgression();
            using (var bridge = new ProgressionEventBridge(progression))
            {
                var session = new BattleSession();
                var enemy = EnemyUnit();
                enemy.CurrentHp = 1;
                session.Initialize(PlayerUnit(), enemy);
                session.StartRound();

                session.PlayerAttack();

                Assert.That(progression.PlayerExperience, Is.EqualTo(25));
                Assert.That(progression.PlayerLevel, Is.EqualTo(1));
            }
        }

        [Test]
        public void LevelUpTriggersCorrectly()
        {
            var progression = new PlayerProgression();

            progression.AddExperience(99);
            Assert.That(progression.CheckLevelUp(), Is.False);
            Assert.That(progression.PlayerLevel, Is.EqualTo(1));

            progression.AddExperience(1);
            Assert.That(progression.CheckLevelUp(), Is.True);
            Assert.That(progression.PlayerLevel, Is.EqualTo(2));
            Assert.That(progression.PlayerExperience, Is.Zero);
            Assert.That(progression.XpToNextLevel, Is.EqualTo(200));
        }

        [Test]
        public void EventSystemIsBootstrappedOnlyOnce()
        {
            var parent = new GameObject("TestParent").transform;
            try
            {
                EventSystem first = BattleEventSystemBootstrapper.EnsureEventSystem(parent);
                Assert.That(first, Is.Not.Null);
                Assert.That(first.GetComponent<InputSystemUIInputModule>(), Is.Not.Null);

                EventSystem second = BattleEventSystemBootstrapper.EnsureEventSystem(parent);
                Assert.That(second, Is.SameAs(first));

                EventSystem[] all = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
                Assert.That(all.Length, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
        }

        [Test]
        public void RuntimeAssembliesDoNotReferenceUnityEditor()
        {
            var runtimeAssemblies = new[]
            {
                typeof(BattleController).Assembly,
                typeof(BattleUnit).Assembly,
                typeof(BattleSession).Assembly,
                typeof(PlayerProgression).Assembly,
                typeof(MonsterData).Assembly,
                typeof(BeastKeeper.Core.ServiceLocator).Assembly
            };

            foreach (var assembly in runtimeAssemblies)
            {
                var references = assembly.GetReferencedAssemblies();
                Assert.That(
                    references.Any(r => r.Name.StartsWith("UnityEditor")),
                    Is.False,
                    $"{assembly.FullName} references UnityEditor");
            }
        }

        [Test]
        public void BattleVictoryRaisesEventsExactlyOnce()
        {
            int victoryCount = 0;
            int defeatedCount = 0;
            Action<BattleVictoryEvent> onVictory = e => victoryCount++;
            Action<MonsterDefeatedEvent> onDefeated = e => defeatedCount++;

            EventBus.Subscribe(onVictory);
            EventBus.Subscribe(onDefeated);
            try
            {
                var session = new BattleSession();
                var enemy = EnemyUnit();
                enemy.CurrentHp = 5;
                session.Initialize(PlayerUnit(), enemy);
                session.StartRound();

                session.PlayerAttack();
                Assert.That(session.CurrentState, Is.EqualTo(BattleState.Victory));
                Assert.That(victoryCount, Is.EqualTo(1));
                Assert.That(defeatedCount, Is.EqualTo(1));

                session.PlayerAttack();
                session.ExecuteAction(BattleAction.Observe);
                Assert.That(victoryCount, Is.EqualTo(1), "Duplicate victory event must not be raised.");
                Assert.That(defeatedCount, Is.EqualTo(1));
            }
            finally
            {
                EventBus.Unsubscribe(onVictory);
                EventBus.Unsubscribe(onDefeated);
            }
        }

        [Test]
        public void BattleDefeatRaisesDefeatEvent()
        {
            int defeatCount = 0;
            Action<BattleDefeatEvent> onDefeat = e =>
            {
                defeatCount++;
                Assert.That(e.EnemyId, Is.EqualTo("mossfang"));
                Assert.That(e.EnemyLevel, Is.EqualTo(1));
            };

            EventBus.Subscribe(onDefeat);
            try
            {
                var session = new BattleSession();
                var player = PlayerUnit();
                player.CurrentHp = 3;
                session.Initialize(player, EnemyUnit());
                session.StartRound();

                session.PlayerAttack();
                session.ExecuteEnemyTurn();

                Assert.That(session.CurrentState, Is.EqualTo(BattleState.Defeat));
                Assert.That(defeatCount, Is.EqualTo(1));
            }
            finally
            {
                EventBus.Unsubscribe(onDefeat);
            }
        }
    }
}
