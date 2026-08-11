using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BeastKeeper.Data;
using BeastKeeper.Gameplay;
using BeastKeeper.Gameplay.Battle;
using BeastKeeper.Systems;

namespace BeastKeeper.Editor
{
    /// <summary>
    /// Editor-only, explicit, reversible setup for the milestone 0.5 gameplay loop. Idempotent:
    /// assets and scene wiring are only created when missing, and no duplicate EncounterZones or
    /// QuestDatabases are added. This tool is NOT executed automatically; run the menu items
    /// manually in the Editor.
    /// </summary>
    public static class GameplayLoopSetup
    {
        private const string ItemsDir = "Assets/BeastKeeper/Data/Items";
        private const string QuestDir = "Assets/BeastKeeper/Data/Quest";

        private static bool sceneDirty;

        [MenuItem("Beast Keeper/Setup Gameplay Loop")]
        public static void SetupGameplayLoop()
        {
            ItemData potion = CreatePotionAsset();
            QuestData firstSteps = CreateFirstStepsQuestAsset(potion);
            ChapterData chapter = CreateChapterOneAsset(firstSteps);
            WireNpcPrefab();

            EditorSceneManager.OpenScene(GameplayLoopValidator.ExplorationScenePath, OpenSceneMode.Single);
            sceneDirty = false;
            AddQuestDatabaseToScene(firstSteps, chapter);
            AddEncounterZoneToScene();
            if (sceneDirty) EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            EditorSceneManager.CloseScene(EditorSceneManager.GetActiveScene(), true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[GameplayLoopSetup] Setup complete. Potion, First Steps, Chapter 1 created; NPC prefab wired; " +
                      "QuestDatabase and EncounterZone ensured in Prototype_Exploration. Open 'Beast Keeper/Validate Gameplay Loop' to confirm.");
        }

        [MenuItem("Beast Keeper/Remove Gameplay Loop Wiring")]
        public static void RemoveGameplayLoopWiring()
        {
            if (!File.Exists(GameplayLoopValidator.ExplorationScenePath))
            {
                Debug.LogWarning("[GameplayLoopSetup] Exploration scene not found; nothing to remove.");
                return;
            }

            EditorSceneManager.OpenScene(GameplayLoopValidator.ExplorationScenePath, OpenSceneMode.Single);
            bool removed = false;

            foreach (EncounterZone zone in Object.FindObjectsByType<EncounterZone>(FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(zone.gameObject);
                removed = true;
            }

            foreach (QuestDatabase db in Object.FindObjectsByType<QuestDatabase>(FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(db.gameObject);
                removed = true;
            }

            if (removed)
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            EditorSceneManager.CloseScene(EditorSceneManager.GetActiveScene(), true);
            Debug.Log(removed
                ? "[GameplayLoopSetup] Removed EncounterZone and QuestDatabase wiring from Prototype_Exploration."
                : "[GameplayLoopSetup] No EncounterZone/QuestDatabase wiring found in Prototype_Exploration.");
        }

        private static ItemData CreatePotionAsset()
        {
            ItemData potion = AssetDatabase.LoadAssetAtPath<ItemData>(GameplayLoopValidator.PotionPath);
            if (potion != null) return potion;

            if (!Directory.Exists(ItemsDir)) Directory.CreateDirectory(ItemsDir);

            potion = ScriptableObject.CreateInstance<ItemData>();
            var so = new SerializedObject(potion);
            so.FindProperty("id").stringValue = GameplayLoopValidator.PotionItemId;
            so.FindProperty("displayName").stringValue = "Potion";
            so.FindProperty("description").stringValue = "Restores 20 HP in battle.";
            so.FindProperty("type").enumValueIndex = (int)ItemType.Consumable;
            so.FindProperty("value").intValue = BattleSession.PotionHealAmount;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(potion, GameplayLoopValidator.PotionPath);
            return potion;
        }

        private static QuestData CreateFirstStepsQuestAsset(ItemData potion)
        {
            QuestData quest = AssetDatabase.LoadAssetAtPath<QuestData>(GameplayLoopValidator.FirstStepsPath);
            if (quest != null) return quest;

            if (!Directory.Exists(QuestDir)) Directory.CreateDirectory(QuestDir);

            quest = ScriptableObject.CreateInstance<QuestData>();
            var so = new SerializedObject(quest);
            so.FindProperty("id").stringValue = GameplayLoopValidator.FirstStepsId;
            so.FindProperty("displayName").stringValue = "First Steps";
            so.FindProperty("description").stringValue = "Speak with the Old Keeper, explore the forest, and defeat a Mossfang.";
            so.FindProperty("experienceReward").intValue = 50;
            so.FindProperty("startAfterDialogueId").stringValue = GameplayLoopValidator.OldKeeperDialogueId;
            so.FindProperty("startObjectiveId").stringValue = $"talk:{GameplayLoopValidator.NpcOldKeeperId}";

            SerializedProperty objectives = so.FindProperty("objectives");
            objectives.ClearArray();
            SetObjective(objectives, 0, $"talk:{GameplayLoopValidator.NpcOldKeeperId}", "Talk to the Old Keeper.", 1, null);
            SetObjective(objectives, 1, $"enter:{GameplayLoopValidator.ForestAreaId}", "Enter the forest.", 1, $"talk:{GameplayLoopValidator.NpcOldKeeperId}");
            SetObjective(objectives, 2, $"defeat:{GameplayLoopValidator.MossfangMonsterId}", "Defeat a Mossfang.", 1, $"enter:{GameplayLoopValidator.ForestAreaId}");
            SetObjective(objectives, 3, $"talk:{GameplayLoopValidator.NpcOldKeeperId}:return", "Return to the Old Keeper.", 1, $"defeat:{GameplayLoopValidator.MossfangMonsterId}");

            SerializedProperty rewards = so.FindProperty("itemRewards");
            rewards.ClearArray();
            rewards.InsertArrayElementAtIndex(0);
            SerializedProperty reward = rewards.GetArrayElementAtIndex(0);
            reward.FindPropertyRelative("item").objectReferenceValue = potion;
            reward.FindPropertyRelative("quantity").intValue = 1;

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(quest, GameplayLoopValidator.FirstStepsPath);
            return quest;
        }

        private static void SetObjective(SerializedProperty objectives, int index, string id, string description, int required, string requiresObjectiveId)
        {
            objectives.InsertArrayElementAtIndex(index);
            SerializedProperty objective = objectives.GetArrayElementAtIndex(index);
            objective.FindPropertyRelative("id").stringValue = id;
            objective.FindPropertyRelative("description").stringValue = description;
            objective.FindPropertyRelative("required").intValue = required;
            objective.FindPropertyRelative("requiresObjectiveId").stringValue = requiresObjectiveId ?? string.Empty;
        }

        private static ChapterData CreateChapterOneAsset(QuestData firstSteps)
        {
            ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(GameplayLoopValidator.ChapterOnePath);
            if (chapter != null) return chapter;

            if (!Directory.Exists(QuestDir)) Directory.CreateDirectory(QuestDir);

            chapter = ScriptableObject.CreateInstance<ChapterData>();
            var so = new SerializedObject(chapter);
            so.FindProperty("id").stringValue = GameplayLoopValidator.ChapterOneId;
            so.FindProperty("displayName").stringValue = "The First Keeper";
            so.FindProperty("description").stringValue = "Begin your journey as a Beast Keeper.";
            so.FindProperty("chapterNumber").intValue = 1;

            SerializedProperty quests = so.FindProperty("quests");
            quests.ClearArray();
            quests.InsertArrayElementAtIndex(0);
            quests.GetArrayElementAtIndex(0).objectReferenceValue = firstSteps;

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(chapter, GameplayLoopValidator.ChapterOnePath);
            return chapter;
        }

        private static void WireNpcPrefab()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameplayLoopValidator.NpcPrefabPath);
            if (prefab == null) return;

            InteractableNPC npc = prefab.GetComponent<InteractableNPC>();
            if (npc == null) return;

            DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(GameplayLoopValidator.OldKeeperDialoguePath);

            var so = new SerializedObject(npc);
            so.FindProperty("npcName").stringValue = "Old Keeper";
            so.FindProperty("npcId").stringValue = GameplayLoopValidator.NpcOldKeeperId;
            if (dialogue != null) so.FindProperty("dialogueData").objectReferenceValue = dialogue;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(prefab);
        }

        private static void AddQuestDatabaseToScene(QuestData firstSteps, ChapterData chapter)
        {
            QuestDatabase[] existing = Object.FindObjectsByType<QuestDatabase>(FindObjectsSortMode.None);
            QuestDatabase database;
            if (existing.Length == 0)
            {
                var go = new GameObject("QuestDatabase");
                database = go.AddComponent<QuestDatabase>();
                sceneDirty = true;
            }
            else
            {
                database = existing[0];
            }

            var so = new SerializedObject(database);
            SerializedProperty quests = so.FindProperty("quests");
            quests.ClearArray();
            quests.InsertArrayElementAtIndex(0);
            quests.GetArrayElementAtIndex(0).objectReferenceValue = firstSteps;
            if (chapter != null)
            {
                quests.InsertArrayElementAtIndex(1);
                quests.GetArrayElementAtIndex(1).objectReferenceValue = chapter;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
            sceneDirty = true;
        }

        private static void AddEncounterZoneToScene()
        {
            EncounterZone[] existing = Object.FindObjectsByType<EncounterZone>(FindObjectsSortMode.None);
            if (existing.Length > 0)
            {
                return;
            }

            MonsterData mossfang = AssetDatabase.LoadAssetAtPath<MonsterData>(GameplayLoopValidator.MossfangPath);

            var go = new GameObject("EncounterZone_Mossfang");
            go.transform.position = new Vector3(0f, 21f, 0f);

            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(3f, 3f);
            collider.isTrigger = true;

            EncounterZone zone = go.AddComponent<EncounterZone>();
            var so = new SerializedObject(zone);
            if (mossfang != null) so.FindProperty("encounterMonster").objectReferenceValue = mossfang;
            so.FindProperty("encounterLevel").intValue = 1;
            so.FindProperty("reTriggerCooldown").floatValue = 1f;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(go);
            sceneDirty = true;
        }
    }
}
