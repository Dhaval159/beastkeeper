using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using BeastKeeper.Data;
using BeastKeeper.Gameplay;
using BeastKeeper.Gameplay.Battle;
using BeastKeeper.Systems;

namespace BeastKeeper.Editor
{
    /// <summary>
    /// Editor-only validation for the milestone 0.5 gameplay loop. Read-only: it never edits
    /// scenes, prefabs, or assets. It reports missing wiring, including the known gap that the
    /// exploration scene has no EncounterZone yet.
    /// </summary>
    public static class GameplayLoopValidator
    {
        public const string ExplorationScenePath = "Assets/BeastKeeper/Scenes/Prototype_Exploration.unity";
        public const string BattleScenePath = "Assets/BeastKeeper/Scenes/Prototype_Battle.unity";
        public const string NpcPrefabPath = "Assets/BeastKeeper/Prefabs/Characters/NPC.prefab";
        public const string PlayerPrefabPath = "Assets/BeastKeeper/Prefabs/Characters/Player.prefab";
        public const string MossfangPath = "Assets/BeastKeeper/Data/Monsters/Mossfang.asset";
        public const string PotionPath = "Assets/BeastKeeper/Data/Items/Potion.asset";
        public const string FirstStepsPath = "Assets/BeastKeeper/Data/Quest/quest_first_steps.asset";
        public const string ChapterOnePath = "Assets/BeastKeeper/Data/Quest/chapter_first_keeper.asset";
        public const string OldKeeperDialoguePath = "Assets/BeastKeeper/Data/Dialogue/OldKeeperIntroDialogue.asset";

        public const string FirstStepsId = "quest_first_steps";
        public const string ChapterOneId = "chapter_first_keeper";
        public const string OldKeeperDialogueId = "old_keeper_intro";
        public const string NpcOldKeeperId = "old_keeper";
        public const string ForestAreaId = "forest";
        public const string PotionItemId = "potion";
        public const string MossfangMonsterId = "mossfang";

        private static readonly List<string> Errors = new List<string>();
        private static readonly List<string> Warnings = new List<string>();
        private static readonly List<string> Info = new List<string>();

        [MenuItem("Beast Keeper/Validate Gameplay Loop")]
        public static void ValidateGameplayLoop()
        {
            Errors.Clear();
            Warnings.Clear();
            Info.Clear();

            ValidateScenes();
            ValidatePrefabs();
            ValidateMonsterData();
            ValidateItemData();
            ValidateQuestData();
            ValidateChapterData();
            ValidateDialogueData();
            ValidateSceneContents();

            string report = BuildReport();
            Debug.Log($"[GameplayLoopValidator]\n{report}");
            EditorUtility.DisplayDialog("Beast Keeper - Gameplay Loop Validation", report, "OK");
        }

        private static void ValidateScenes()
        {
            RequireAsset(ExplorationScenePath, "Exploration scene");
            RequireAsset(BattleScenePath, "Battle scene");
        }

        private static void ValidatePrefabs()
        {
            RequireAsset(NpcPrefabPath, "NPC prefab");
            RequireAsset(PlayerPrefabPath, "Player prefab");

            GameObject npcPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NpcPrefabPath);
            if (npcPrefab == null) return;

            InteractableNPC npc = npcPrefab.GetComponent<InteractableNPC>();
            if (npc == null)
            {
                Error("NPC prefab has no InteractableNPC component.");
                return;
            }

            string npcId = npc.NpcId;
            if (npcId != NpcOldKeeperId)
            {
                Error($"NPC prefab NpcId is '{npcId}', expected '{NpcOldKeeperId}' (Old Keeper) for quest 'talk:{NpcOldKeeperId}'.");
            }
            if (npc.DialogueData == null)
            {
                Error($"NPC prefab has no DialogueData assigned (expected {OldKeeperDialoguePath}).");
            }
            else if (npc.DialogueData.IdOrAssetName != OldKeeperDialogueId)
            {
                Error($"NPC prefab dialogue id is '{npc.DialogueData.IdOrAssetName}', expected '{OldKeeperDialogueId}'.");
            }
        }

        private static void ValidateMonsterData()
        {
            MonsterData mossfang = AssetDatabase.LoadAssetAtPath<MonsterData>(MossfangPath);
            if (mossfang == null)
            {
                Error($"Mossfang asset missing: {MossfangPath}");
                return;
            }

            if (mossfang.Id != MossfangMonsterId)
            {
                Error($"Mossfang id is '{mossfang.Id}', expected '{MossfangMonsterId}' (quest objective 'defeat:{MossfangMonsterId}').");
            }

            bool hasBite = false;
            bool hasGrowl = false;
            if (mossfang.LearnableAbilities != null)
            {
                foreach (MonsterAbility ability in mossfang.LearnableAbilities)
                {
                    if (ability == null) continue;
                    if (ability.Id == "bite" && ability.EffectType == AbilityEffectType.Damage) hasBite = true;
                    if (ability.Id == "growl" && ability.EffectType == AbilityEffectType.ReduceAttack) hasGrowl = true;
                }
            }

            if (!hasBite) Error("Mossfang is missing its 'bite' (Damage) ability.");
            if (!hasGrowl) Error("Mossfang is missing its 'growl' (ReduceAttack) ability.");
        }

        private static void ValidateItemData()
        {
            ItemData potion = AssetDatabase.LoadAssetAtPath<ItemData>(PotionPath);
            if (potion == null)
            {
                Error($"Potion item asset missing: {PotionPath}");
                return;
            }

            if (potion.Id != PotionItemId) Error($"Potion id is '{potion.Id}', expected '{PotionItemId}'.");
            if (potion.Type != ItemType.Consumable) Error("Potion must be a Consumable item.");
            if (potion.Value != BattleSession.PotionHealAmount)
            {
                Error($"Potion heal value is {potion.Value}, expected {BattleSession.PotionHealAmount} (BattleSession.PotionHealAmount).");
            }
        }

        private static void ValidateQuestData()
        {
            QuestData firstSteps = AssetDatabase.LoadAssetAtPath<QuestData>(FirstStepsPath);
            if (firstSteps == null)
            {
                Error($"First Steps quest asset missing: {FirstStepsPath}");
                return;
            }

            if (firstSteps.Id != FirstStepsId) Error($"First Steps id is '{firstSteps.Id}', expected '{FirstStepsId}'.");

            int expectedObjectives = 4;
            if (firstSteps.Objectives == null || firstSteps.Objectives.Count != expectedObjectives)
            {
                Error($"First Steps must have exactly {expectedObjectives} objectives (found {firstSteps.Objectives?.Count ?? 0}).");
            }
            else
            {
                CheckObjective(firstSteps, 0, $"talk:{NpcOldKeeperId}");
                CheckObjective(firstSteps, 1, $"enter:{ForestAreaId}");
                CheckObjective(firstSteps, 2, $"defeat:{MossfangMonsterId}");
                CheckObjective(firstSteps, 3, $"talk:{NpcOldKeeperId}:return");
            }

            if (firstSteps.ExperienceReward != 50)
            {
                Error($"First Steps XP reward is {firstSteps.ExperienceReward}, expected 50.");
            }

            bool hasPotionReward = false;
            if (firstSteps.ItemRewards != null)
            {
                foreach (QuestReward reward in firstSteps.ItemRewards)
                {
                    if (reward != null && reward.Item != null && reward.Item.Id == PotionItemId && reward.Quantity > 0)
                    {
                        hasPotionReward = true;
                    }
                }
            }
            if (!hasPotionReward) Error("First Steps must grant a Potion item reward.");

            if (firstSteps.StartAfterDialogueId != OldKeeperDialogueId)
            {
                Error($"First Steps StartAfterDialogueId is '{firstSteps.StartAfterDialogueId}', expected '{OldKeeperDialogueId}'.");
            }
            if (firstSteps.StartObjectiveId != $"talk:{NpcOldKeeperId}")
            {
                Error($"First Steps StartObjectiveId is '{firstSteps.StartObjectiveId}', expected 'talk:{NpcOldKeeperId}'.");
            }
        }

        private static void CheckObjective(QuestData quest, int index, string expectedId)
        {
            QuestObjective objective = quest.Objectives[index];
            if (objective.Id != expectedId)
            {
                Error($"First Steps objective[{index}] id is '{objective.Id}', expected '{expectedId}'.");
            }
            if (objective.Required <= 0)
            {
                Error($"First Steps objective '{objective.Id}' has non-positive required count.");
            }
        }

        private static void ValidateChapterData()
        {
            ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(ChapterOnePath);
            if (chapter == null)
            {
                Error($"Chapter 1 asset missing: {ChapterOnePath}");
                return;
            }

            if (chapter.Id != ChapterOneId) Error($"Chapter 1 id is '{chapter.Id}', expected '{ChapterOneId}'.");
            if (chapter.ChapterNumber != 1) Error($"Chapter 1 number is {chapter.ChapterNumber}, expected 1.");

            QuestData firstSteps = AssetDatabase.LoadAssetAtPath<QuestData>(FirstStepsPath);
            bool referencesFirstSteps = chapter.Quests != null && firstSteps != null && chapter.Quests.Contains(firstSteps);
            if (!referencesFirstSteps) Error("Chapter 1 must reference the First Steps quest.");
        }

        private static void ValidateDialogueData()
        {
            DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(OldKeeperDialoguePath);
            if (dialogue == null)
            {
                Error($"Old Keeper dialogue asset missing: {OldKeeperDialoguePath}");
                return;
            }

            if (dialogue.IdOrAssetName != OldKeeperDialogueId)
            {
                Error($"Old Keeper dialogue id is '{dialogue.IdOrAssetName}', expected '{OldKeeperDialogueId}'.");
            }
        }

        private static void ValidateSceneContents()
        {
            RequireComponentInScene(ExplorationScenePath, "Assets/BeastKeeper/Scripts/Systems/Quest/QuestDatabase.cs", "QuestDatabase");
            RequireComponentInScene(ExplorationScenePath, "Assets/BeastKeeper/Scripts/Systems/Dialogue/DialogueSystem.cs", "DialogueSystem");
            RequireComponentInScene(ExplorationScenePath, "Assets/BeastKeeper/Scripts/Gameplay/InteractableNPC.cs", "InteractableNPC (Old Keeper)");
            RequireComponentInScene(ExplorationScenePath, "Assets/BeastKeeper/Scripts/Gameplay/Encounter/EncounterZone.cs", "EncounterZone");
            RequireComponentInScene(ExplorationScenePath, "Assets/BeastKeeper/Scripts/Gameplay/AreaBoundsTrigger.cs", "AreaBoundsTrigger (forest)");
            RequireComponentInScene(BattleScenePath, "Assets/BeastKeeper/Scripts/Gameplay/Battle/BattleController.cs", "BattleController");
        }

        private static void RequireComponentInScene(string scenePath, string scriptPath, string label)
        {
            string guid = AssetDatabase.AssetPathToGUID(scriptPath);
            if (string.IsNullOrEmpty(guid))
            {
                Error($"Required script missing: {scriptPath}");
                return;
            }

            if (!File.Exists(scenePath))
            {
                return;
            }

            bool present = File.ReadAllText(scenePath).Contains($"m_Script: {{fileID: 11500000, guid: {guid}");
            if (!present)
            {
                Warning($"'{label}' is not present in {scenePath}. This must be added in the Editor " +
                        "(e.g. via 'Beast Keeper/Setup Gameplay Loop'); the scene was not modified.");
            }
        }

        private static void RequireAsset(string assetPath, string label)
        {
            if (!File.Exists(assetPath))
            {
                Error($"{label} missing: {assetPath}");
            }
            else
            {
                Info.Add($"'{label}' found: {assetPath}");
            }
        }

        private static void Error(string message) => Errors.Add(message);
        private static void Warning(string message) => Warnings.Add(message);

        private static string BuildReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Beast Keeper - Gameplay Loop Validation ===");
            sb.AppendLine();
            sb.AppendLine("--- Checks performed ---");
            AppendList(sb, Info);
            sb.AppendLine();
            sb.AppendLine("--- Errors ---");
            AppendList(sb, Errors, "No errors.");
            sb.AppendLine();
            sb.AppendLine("--- Warnings (gaps) ---");
            AppendList(sb, Warnings, "No warnings.");
            sb.AppendLine();
            sb.AppendLine(Errors.Count == 0
                ? "Result: PASS (warnings are scene wiring gaps to fix in the Editor)."
                : $"Result: FAIL ({Errors.Count} error(s)).");
            return sb.ToString();
        }

        private static void AppendList(StringBuilder sb, List<string> items, string emptyText = null)
        {
            if (items.Count == 0)
            {
                if (!string.IsNullOrEmpty(emptyText)) sb.AppendLine("  " + emptyText);
                return;
            }
            foreach (string item in items)
            {
                sb.AppendLine("  - " + item);
            }
        }
    }
}
