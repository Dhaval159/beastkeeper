using System;
using System.Linq;
using NUnit.Framework;
using BeastKeeper.Core;
using BeastKeeper.Data;
using BeastKeeper.Systems;
using BeastKeeper.Gameplay.Battle;

namespace BeastKeeper.Tests.EditMode
{
    /// <summary>
    /// EditMode tests for milestone 0.5: quest lifecycle, event-driven objective progress,
    /// rewards (exactly-once), inventory, progression bridge, and save-state models.
    /// </summary>
    public class QuestLoopTests
    {
        private static ItemData PotionItem()
        {
            return ItemData.CreateRuntime("potion", "Potion", ItemType.Consumable, BattleSession.PotionHealAmount);
        }

        private static MonsterData BuildMossfang()
        {
            return MonsterData.CreateRuntime("mossfang", "Mossfang", 50, 8, 12, 4,
                MonsterAbility.CreateRuntime("bite", "Bite", 15, 0, AbilityEffectType.Damage),
                MonsterAbility.CreateRuntime("growl", "Growl", 0, 0, AbilityEffectType.ReduceAttack, 2));
        }

        private static BattleUnit PlayerUnit()
        {
            var unit = BattleUnit.FromMonsterData(
                MonsterData.CreateRuntime("companion", "Companion", 50, 10, 15, 5,
                    MonsterAbility.CreateRuntime("bite", "Bite", 15, 0, AbilityEffectType.Damage)), 1);
            unit.IsPlayer = true;
            return unit;
        }

        private static QuestData BuildFirstSteps()
        {
            return QuestData.CreateRuntime("quest_first_steps", "First Steps", 50,
                null, null, null,
                new QuestObjective("talk:old_keeper", "Talk to the Old Keeper."),
                new QuestObjective("enter:forest", "Enter the forest.", 1, "talk:old_keeper"),
                new QuestObjective("defeat:mossfang", "Defeat a Mossfang.", 1, "enter:forest"),
                new QuestObjective("talk:old_keeper:return", "Return to the Old Keeper.", 1, "defeat:mossfang"));
        }

        private static QuestData BuildFirstStepsWithStartTrigger()
        {
            return QuestData.CreateRuntime("quest_first_steps", "First Steps", 50,
                "old_keeper_intro", "talk:old_keeper",
                new QuestReward[] { new QuestReward(PotionItem(), 1) },
                new QuestObjective("talk:old_keeper", "Talk to the Old Keeper."),
                new QuestObjective("enter:forest", "Enter the forest.", 1, "talk:old_keeper"),
                new QuestObjective("defeat:mossfang", "Defeat a Mossfang.", 1, "enter:forest"),
                new QuestObjective("talk:old_keeper:return", "Return to the Old Keeper.", 1, "defeat:mossfang"));
        }

        [Test]
        public void QuestStartsAndBecomesActive()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);

                Assert.That(questSystem.IsQuestActive(quest), Is.True);
                Assert.That(questSystem.GetQuestStatus(quest), Is.EqualTo(QuestStatus.Active));
                Assert.That(questSystem.GetActiveQuests(), Does.Contain(quest));
                Assert.That(questSystem.GetQuestProgress(quest).Count, Is.EqualTo(4));
            }
        }

        [Test]
        public void QuestDoesNotStartTwice()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                questSystem.StartQuest(quest);

                Assert.That(questSystem.GetActiveQuests().Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void ObjectiveAdvancesAndCapsAtRequired()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                var progress = questSystem.GetQuestProgress(quest);

                questSystem.AdvanceObjective("talk:old_keeper", 5);

                Assert.That(progress[0].Current, Is.EqualTo(1), "Progress must cap at required.");
                Assert.That(progress[0].IsComplete, Is.True);
                Assert.That(progress[1].IsComplete, Is.False, "Objectives must not complete before their prerequisite.");
            }
        }

        [Test]
        public void UnknownObjectiveIsIgnoredSafely()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);

                questSystem.AdvanceObjective("bogus:target");
                questSystem.AdvanceObjective("");

                Assert.That(questSystem.IsQuestActive(quest), Is.True);
                Assert.That(questSystem.GetQuestProgress(quest)[0].IsComplete, Is.False);
            }
        }

        [Test]
        public void QuestCompletesWhenAllObjectivesComplete()
        {
            var quest = BuildFirstSteps();
            bool completed = false;
            using (var questSystem = new QuestSystem())
            {
                questSystem.QuestCompleted += q => completed = true;
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);

                questSystem.AdvanceObjective("talk:old_keeper");
                questSystem.AdvanceObjective("enter:forest");
                questSystem.AdvanceObjective("defeat:mossfang");
                questSystem.AdvanceObjective("talk:old_keeper");

                Assert.That(questSystem.IsQuestCompleted(quest), Is.True);
                Assert.That(questSystem.GetQuestStatus(quest), Is.EqualTo(QuestStatus.Completed));
                Assert.That(questSystem.GetActiveQuests(), Is.Empty);
                Assert.That(completed, Is.True);
            }
        }

        [Test]
        public void CompletedQuestDoesNotRestart()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                questSystem.AdvanceObjective("talk:old_keeper");
                questSystem.AdvanceObjective("enter:forest");
                questSystem.AdvanceObjective("defeat:mossfang");
                questSystem.AdvanceObjective("talk:old_keeper");

                questSystem.StartQuest(quest);
                questSystem.AdvanceObjective("talk:old_keeper");

                Assert.That(questSystem.GetQuestStatus(quest), Is.EqualTo(QuestStatus.Completed));
                Assert.That(questSystem.GetActiveQuests(), Is.Empty);
            }
        }

        [Test]
        public void NpcInteractionProgressesTalkObjective()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);

                EventBus.Raise(new NPCInteractionEvent { NpcId = "old_keeper" });

                Assert.That(questSystem.GetQuestProgress(quest)[0].IsComplete, Is.True);
            }
        }

        [Test]
        public void NpcInteractionForWrongNpcDoesNotProgress()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);

                EventBus.Raise(new NPCInteractionEvent { NpcId = "villager" });

                Assert.That(questSystem.GetQuestProgress(quest)[0].IsComplete, Is.False);
            }
        }

        [Test]
        public void AreaEnteredProgressesEnterObjective()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                questSystem.AdvanceObjective("talk:old_keeper");

                EventBus.Raise(new AreaEnteredEvent { AreaId = "forest" });

                Assert.That(questSystem.GetQuestProgress(quest)[1].IsComplete, Is.True);
            }
        }

        [Test]
        public void MonsterDefeatedProgressesCorrectMonsterObjective()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                questSystem.AdvanceObjective("talk:old_keeper");
                questSystem.AdvanceObjective("enter:forest");

                EventBus.Raise(new MonsterDefeatedEvent { MonsterId = "mossfang", Level = 1 });

                Assert.That(questSystem.GetQuestProgress(quest)[2].IsComplete, Is.True);
            }
        }

        [Test]
        public void MonsterDefeatedForWrongMonsterDoesNotProgress()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                questSystem.AdvanceObjective("talk:old_keeper");
                questSystem.AdvanceObjective("enter:forest");

                EventBus.Raise(new MonsterDefeatedEvent { MonsterId = "wolf", Level = 1 });

                Assert.That(questSystem.GetQuestProgress(quest)[2].IsComplete, Is.False);
            }
        }

        [Test]
        public void BattleVictoryProgressesWinBattleObjective()
        {
            var quest = QuestData.CreateRuntime("quest_win", "Win Battle", 0,
                null, null, null,
                new QuestObjective("win_battle", "Win any battle."));

            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                var progress = questSystem.GetQuestProgress(quest);

                EventBus.Raise(new BattleVictoryEvent { EnemyId = "mossfang", EnemyLevel = 1, ExperienceAwarded = 25 });

                Assert.That(progress[0].IsComplete, Is.True);
                Assert.That(questSystem.IsQuestCompleted(quest), Is.True);
            }
        }

        [Test]
        public void ReturnObjectiveIsGatedByDefeatPrerequisite()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                var progress = questSystem.GetQuestProgress(quest);

                EventBus.Raise(new NPCInteractionEvent { NpcId = "old_keeper" });
                EventBus.Raise(new NPCInteractionEvent { NpcId = "old_keeper" });

                Assert.That(progress[0].IsComplete, Is.True);
                Assert.That(progress[3].IsComplete, Is.False, "Return objective must wait for the defeat objective.");
                Assert.That(questSystem.IsQuestCompleted(quest), Is.False);
            }
        }

        [Test]
        public void DialogueCompletionStartsMatchingQuest()
        {
            var quest = BuildFirstStepsWithStartTrigger();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);

                EventBus.Raise(new DialogueCompletedEvent { DialogueId = "old_keeper_intro" });

                Assert.That(questSystem.IsQuestActive(quest), Is.True);
            }
        }

        [Test]
        public void DialogueCompletionFulfillsStartObjective()
        {
            var quest = BuildFirstStepsWithStartTrigger();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);

                EventBus.Raise(new DialogueCompletedEvent { DialogueId = "old_keeper_intro" });
                var progress = questSystem.GetQuestProgress(quest);

                Assert.That(progress[0].IsComplete, Is.True, "The starting conversation counts as the first talk.");
                Assert.That(progress[1].IsComplete, Is.False);
            }
        }

        [Test]
        public void QuestCompletionGrantsXpRewardExactlyOnce()
        {
            var progression = new PlayerProgression();
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem(progression, null))
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                questSystem.AdvanceObjective("talk:old_keeper");
                questSystem.AdvanceObjective("enter:forest");
                questSystem.AdvanceObjective("defeat:mossfang");
                questSystem.AdvanceObjective("talk:old_keeper");

                Assert.That(progression.PlayerExperience, Is.EqualTo(50));

                questSystem.CompleteQuest(quest);
                questSystem.CompleteQuest(quest);

                Assert.That(progression.PlayerExperience, Is.EqualTo(50), "XP reward must be granted exactly once.");
            }
        }

        [Test]
        public void QuestCompletionGrantsItemRewardExactlyOnce()
        {
            var inventory = new InventorySystem();
            var potion = PotionItem();
            var quest = QuestData.CreateRuntime("quest_item", "Item Reward", 0,
                null, null,
                new QuestReward[] { new QuestReward(potion, 2) },
                new QuestObjective("win_battle", "Win a battle."));

            using (var questSystem = new QuestSystem(null, inventory))
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);

                EventBus.Raise(new BattleVictoryEvent { EnemyId = "mossfang", EnemyLevel = 1, ExperienceAwarded = 25 });
                Assert.That(inventory.GetItemCount("potion"), Is.EqualTo(2));

                EventBus.Raise(new BattleVictoryEvent { EnemyId = "mossfang", EnemyLevel = 1, ExperienceAwarded = 25 });
                Assert.That(inventory.GetItemCount("potion"), Is.EqualTo(2), "Duplicate event must not duplicate rewards.");
            }
        }

        [Test]
        public void LevelUpEventFiresOncePerCheckLevelUp()
        {
            var progression = new PlayerProgression();
            int levelUps = 0;
            progression.LeveledUp += () => levelUps++;

            progression.AddExperience(99);
            Assert.That(progression.CheckLevelUp(), Is.False);
            Assert.That(levelUps, Is.Zero);

            progression.AddExperience(1);
            Assert.That(progression.CheckLevelUp(), Is.True);
            Assert.That(levelUps, Is.EqualTo(1));
            Assert.That(progression.PlayerLevel, Is.EqualTo(2));
        }

        [Test]
        public void InventoryAddRemoveHasAndOverRemoveGuard()
        {
            var inventory = new InventorySystem();
            var potion = PotionItem();

            Assert.That(inventory.AddItem(potion, 3), Is.True);
            Assert.That(inventory.GetItemCount("potion"), Is.EqualTo(3));
            Assert.That(inventory.HasItem("potion", 2), Is.True);
            Assert.That(inventory.HasItem("potion", 4), Is.False);

            Assert.That(inventory.RemoveItem("potion", 1), Is.True);
            Assert.That(inventory.GetItemCount("potion"), Is.EqualTo(2));

            Assert.That(inventory.RemoveItem("potion", 99), Is.False, "Over-removal must be rejected.");
            Assert.That(inventory.GetItemCount("potion"), Is.EqualTo(2));

            Assert.That(inventory.RemoveItem("potion", 2), Is.True);
            Assert.That(inventory.GetItemCount("potion"), Is.Zero);
            Assert.That(inventory.HasItem(potion), Is.False);
        }

        [Test]
        public void InventoryIsKeyedByStableItemId()
        {
            var inventory = new InventorySystem();
            var potionA = ItemData.CreateRuntime("potion", "Potion A", ItemType.Consumable, 20);
            var potionB = ItemData.CreateRuntime("potion", "Potion B", ItemType.Consumable, 20);

            inventory.AddItem(potionA, 2);

            Assert.That(inventory.GetItemCount(potionB), Is.EqualTo(2));
            Assert.That(inventory.HasItem(potionB), Is.True);
            Assert.That(inventory.GetItems().Keys, Does.Contain("potion"));
        }

        [Test]
        public void QuestLookupById()
        {
            var quest = BuildFirstSteps();
            using (var questSystem = new QuestSystem())
            {
                questSystem.RegisterQuest(quest);

                Assert.That(questSystem.GetQuestById("quest_first_steps"), Is.SameAs(quest));
                Assert.That(questSystem.GetQuestById("missing_quest"), Is.Null);
            }
        }

        [Test]
        public void SaveStateRoundTripsAllSystems()
        {
            var progression = new PlayerProgression();
            var inventory = new InventorySystem();
            var quest = BuildFirstSteps();

            object gameData = null;
            using (var questSystem = new QuestSystem(progression, inventory))
            {
                questSystem.RegisterQuest(quest);
                questSystem.StartQuest(quest);
                questSystem.AdvanceObjective("talk:old_keeper");
                questSystem.AdvanceObjective("enter:forest");
                inventory.AddItem("potion", 2);
                progression.AddExperience(25);

                progression.SaveData(ref gameData);
                inventory.SaveData(ref gameData);
                questSystem.SaveData(ref gameData);
            }

            var gd = gameData as GameStateData;
            Assert.That(gd, Is.Not.Null);
            Assert.That(gd.Progression.Experience, Is.EqualTo(25));
            Assert.That(gd.Inventory.Items.Count, Is.EqualTo(1));

            var progression2 = new PlayerProgression();
            var inventory2 = new InventorySystem();
            using (var questSystem2 = new QuestSystem(progression2, inventory2))
            {
                questSystem2.RegisterQuest(quest);
                progression2.LoadData(gd);
                inventory2.LoadData(gd);
                questSystem2.LoadData(gd);

                Assert.That(progression2.PlayerExperience, Is.EqualTo(25));
                Assert.That(inventory2.GetItemCount("potion"), Is.EqualTo(2));
                Assert.That(questSystem2.IsQuestActive(quest), Is.True);
                var progress = questSystem2.GetQuestProgress(quest);
                Assert.That(progress[0].IsComplete, Is.True);
                Assert.That(progress[1].IsComplete, Is.True);
                Assert.That(progress[2].IsComplete, Is.False);
            }
        }

        [Test]
        public void FullTargetLoopCompletesFirstStepsQuest()
        {
            var progression = new PlayerProgression();
            var inventory = new InventorySystem();
            var quest = BuildFirstStepsWithStartTrigger();

            using (var questSystem = new QuestSystem(progression, inventory))
            using (var bridge = new ProgressionEventBridge(progression))
            {
                questSystem.RegisterQuest(quest);

                // 1. Old Keeper dialogue completes -> quest starts, first talk counts.
                EventBus.Raise(new DialogueCompletedEvent { DialogueId = "old_keeper_intro" });
                Assert.That(questSystem.IsQuestActive(quest), Is.True);

                // 2. Enter forest.
                EventBus.Raise(new AreaEnteredEvent { AreaId = "forest" });

                // 3. Defeat Mossfang in a real battle session.
                var session = new BattleSession();
                var enemy = BattleUnit.FromMonsterData(BuildMossfang(), 1);
                enemy.CurrentHp = 1;
                session.Initialize(PlayerUnit(), enemy);
                session.StartRound();
                session.PlayerAttack();
                Assert.That(session.CurrentState, Is.EqualTo(BattleState.Victory));

                // 4. Return to Old Keeper -> quest completes, rewards granted once.
                EventBus.Raise(new NPCInteractionEvent { NpcId = "old_keeper" });

                Assert.That(questSystem.IsQuestCompleted(quest), Is.True);
                Assert.That(progression.PlayerExperience, Is.EqualTo(25 + 50), "Battle XP (25) + quest reward (50).");
                Assert.That(inventory.GetItemCount("potion"), Is.EqualTo(1));

                EventBus.Raise(new NPCInteractionEvent { NpcId = "old_keeper" });
                Assert.That(progression.PlayerExperience, Is.EqualTo(25 + 50), "Rewards must not be granted twice.");
            }
        }

        [Test]
        public void RuntimeAssembliesForQuestLoopDoNotReferenceUnityEditor()
        {
            var runtimeAssemblies = new[]
            {
                typeof(QuestSystem).Assembly,
                typeof(InventorySystem).Assembly,
                typeof(PlayerProgression).Assembly,
                typeof(GameStateData).Assembly,
                typeof(QuestData).Assembly,
                typeof(EventBus).Assembly
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
    }
}
