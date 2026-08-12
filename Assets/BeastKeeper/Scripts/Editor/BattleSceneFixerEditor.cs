using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BeastKeeper.Data;
using BeastKeeper.Gameplay.Battle;

namespace BeastKeeper.Editor
{
    public static class BattleSceneFixerEditor
    {
        private const string SpritesPath = "Assets/BeastKeeper/Art/Sprites";

        [MenuItem("Beast Keeper/Fix Battle Scene Layout")]
        public static void FixBattleScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.name.Contains("Battle"))
            {
                EditorSceneManager.OpenScene("Assets/BeastKeeper/Scenes/Prototype_Battle.unity");
            }

            // 1. 2D World Background & Combatant Beasts
            Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Battle/BattleBackground_Forest.png");
            Sprite mossfangSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Monsters/Mossfang_Battle.png");
            Sprite leaflingSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Monsters/Leafling_Battle.png");

            GameObject worldBg = GameObject.Find("BattleBackground_2D");
            if (worldBg == null) worldBg = new GameObject("BattleBackground_2D");
            worldBg.transform.position = new Vector3(0, 0, 5);
            SpriteRenderer bgSr = worldBg.GetComponent<SpriteRenderer>();
            if (bgSr == null) bgSr = worldBg.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -10;
            worldBg.transform.localScale = new Vector3(0.035f, 0.035f, 1f);

            // Enemy Beast Visual
            GameObject enemyBeast = GameObject.Find("EnemyBeast");
            if (enemyBeast == null) enemyBeast = new GameObject("EnemyBeast");
            enemyBeast.transform.position = new Vector3(2.5f, 0.8f, 0);
            enemyBeast.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            SpriteRenderer enemySr = enemyBeast.GetComponent<SpriteRenderer>();
            if (enemySr == null) enemySr = enemyBeast.AddComponent<SpriteRenderer>();
            enemySr.sprite = mossfangSprite;
            enemySr.sortingOrder = 2;

            // Player Beast Visual
            GameObject playerBeast = GameObject.Find("PlayerBeast");
            if (playerBeast == null) playerBeast = new GameObject("PlayerBeast");
            playerBeast.transform.position = new Vector3(-2.5f, -0.6f, 0);
            playerBeast.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
            SpriteRenderer playerSr = playerBeast.GetComponent<SpriteRenderer>();
            if (playerSr == null) playerSr = playerBeast.AddComponent<SpriteRenderer>();
            playerSr.sprite = leaflingSprite;
            playerSr.sortingOrder = 2;

            // 2. Fix Canvas & UI Overlay
            GameObject bgUI = GameObject.Find("BattleBackground");
            if (bgUI != null)
            {
                Image bgImg = bgUI.GetComponent<Image>();
                if (bgImg != null) bgImg.color = new Color(0, 0, 0, 0); // Transparent so world sprites show!
                RectTransform bgRt = bgUI.GetComponent<RectTransform>();
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.anchoredPosition = Vector2.zero;
                bgRt.sizeDelta = Vector2.zero;
            }

            // Player Panel
            GameObject playerPanel = GameObject.Find("PlayerPanel");
            if (playerPanel != null)
            {
                RectTransform rt = playerPanel.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.75f);
                rt.anchorMax = new Vector2(0.42f, 0.93f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
                Image img = playerPanel.GetComponent<Image>();
                if (img == null) img = playerPanel.AddComponent<Image>();
                img.color = new Color(0.1f, 0.12f, 0.15f, 0.75f);

                FixText(playerPanel, "PlayerName", "Player Beast", new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.95f), TextAlignmentOptions.Left, 22);
                FixText(playerPanel, "PlayerHPText", "HP: 100/100", new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.45f), TextAlignmentOptions.Left, 18);
                FixSlider(playerPanel, "PlayerHPSlider", new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.25f));
            }

            // Enemy Panel
            GameObject enemyPanel = GameObject.Find("EnemyPanel");
            if (enemyPanel != null)
            {
                RectTransform rt = enemyPanel.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.58f, 0.75f);
                rt.anchorMax = new Vector2(0.95f, 0.93f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
                Image img = enemyPanel.GetComponent<Image>();
                if (img == null) img = enemyPanel.AddComponent<Image>();
                img.color = new Color(0.15f, 0.1f, 0.1f, 0.75f);

                FixText(enemyPanel, "EnemyName", "Wild Mossfang", new Vector2(0.05f, 0.55f), new Vector2(0.95f, 0.95f), TextAlignmentOptions.Left, 22);
                FixText(enemyPanel, "EnemyHPText", "HP: 50/50", new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.45f), TextAlignmentOptions.Left, 18);
                FixSlider(enemyPanel, "EnemyHPSlider", new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.25f));
            }

            // Log Area
            GameObject logArea = GameObject.Find("LogArea");
            if (logArea != null)
            {
                RectTransform rt = logArea.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.18f);
                rt.anchorMax = new Vector2(0.95f, 0.32f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
                Image img = logArea.GetComponent<Image>();
                if (img == null) img = logArea.AddComponent<Image>();
                img.color = new Color(0.05f, 0.05f, 0.08f, 0.85f);

                FixText(logArea, "LogText", "A wild Mossfang appeared!", new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.98f), TextAlignmentOptions.Center, 20);
            }

            // Buttons Panel
            GameObject btnPanel = GameObject.Find("ButtonsPanel");
            if (btnPanel != null)
            {
                RectTransform rt = btnPanel.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.05f, 0.03f);
                rt.anchorMax = new Vector2(0.95f, 0.15f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;

                FixButton(btnPanel, "AttackButton", "ATTACK", 0);
                FixButton(btnPanel, "ObserveButton", "OBSERVE", 1);
                FixButton(btnPanel, "ItemButton", "ITEM", 2);
                FixButton(btnPanel, "RunButton", "RUN", 3);
            }

            // 3. Wire BattleSystem references
            GameObject battleSys = GameObject.Find("BattleSystem");
            if (battleSys != null)
            {
                BattleController controller = battleSys.GetComponent<BattleController>();
                if (controller == null) controller = battleSys.AddComponent<BattleController>();

                BattleUI ui = battleSys.GetComponent<BattleUI>();
                if (ui == null) ui = battleSys.AddComponent<BattleUI>();

                SerializedObject controllerSO = new SerializedObject(controller);
                controllerSO.FindProperty("battleUI").objectReferenceValue = ui;
                MonsterData playerBeastData = AssetDatabase.LoadAssetAtPath<MonsterData>("Assets/BeastKeeper/Data/Monsters/TempTestBeast.asset");
                controllerSO.FindProperty("playerMonsterData").objectReferenceValue = playerBeastData;
                controllerSO.FindProperty("enemySpriteRenderer").objectReferenceValue = enemySr;
                controllerSO.FindProperty("playerSpriteRenderer").objectReferenceValue = playerSr;
                controllerSO.FindProperty("startSequenceDelay").floatValue = 0.2f;
                controllerSO.FindProperty("enemyThinkDelay").floatValue = 0.3f;
                controllerSO.FindProperty("actionPacingDelay").floatValue = 0.3f;
                controllerSO.ApplyModifiedProperties();

                SerializedObject uiSO = new SerializedObject(ui);
                uiSO.FindProperty("playerNameText").objectReferenceValue = GameObject.Find("PlayerName")?.GetComponent<TMP_Text>();
                uiSO.FindProperty("playerHpText").objectReferenceValue = GameObject.Find("PlayerHPText")?.GetComponent<TMP_Text>();
                uiSO.FindProperty("playerHpSlider").objectReferenceValue = GameObject.Find("PlayerHPSlider")?.GetComponent<Slider>();

                uiSO.FindProperty("enemyNameText").objectReferenceValue = GameObject.Find("EnemyName")?.GetComponent<TMP_Text>();
                uiSO.FindProperty("enemyHpText").objectReferenceValue = GameObject.Find("EnemyHPText")?.GetComponent<TMP_Text>();
                uiSO.FindProperty("enemyHpSlider").objectReferenceValue = GameObject.Find("EnemyHPSlider")?.GetComponent<Slider>();

                uiSO.FindProperty("attackButton").objectReferenceValue = GameObject.Find("AttackButton")?.GetComponent<Button>();
                uiSO.FindProperty("observeButton").objectReferenceValue = GameObject.Find("ObserveButton")?.GetComponent<Button>();
                uiSO.FindProperty("itemButton").objectReferenceValue = GameObject.Find("ItemButton")?.GetComponent<Button>();
                uiSO.FindProperty("runButton").objectReferenceValue = GameObject.Find("RunButton")?.GetComponent<Button>();

                GameObject logTextObj = GameObject.Find("LogText");
                uiSO.FindProperty("logText").objectReferenceValue = logTextObj?.GetComponent<TMP_Text>();
                uiSO.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[BattleSceneFixerEditor] Battle scene layout fixed and saved successfully!");
        }

        private static void FixText(GameObject parent, string name, string defaultText, Vector2 min, Vector2 max, TextAlignmentOptions align, float fontSize)
        {
            Transform t = parent.transform.Find(name);
            GameObject obj = t != null ? t.gameObject : new GameObject(name);
            obj.transform.SetParent(parent.transform);

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null) rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            TMP_Text tmp = obj.GetComponent<TMP_Text>();
            if (tmp == null) tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.alignment = align;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
        }

        private static void FixSlider(GameObject parent, string name, Vector2 min, Vector2 max)
        {
            Transform t = parent.transform.Find(name);
            if (t == null) return;
            RectTransform rt = t.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = min;
                rt.anchorMax = max;
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
            }
        }

        private static void FixButton(GameObject parent, string name, string label, int index)
        {
            Transform t = parent.transform.Find(name);
            if (t == null) return;
            GameObject obj = t.gameObject;

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                float step = 1.0f / 4.0f;
                rt.anchorMin = new Vector2(index * step + 0.02f, 0.1f);
                rt.anchorMax = new Vector2((index + 1) * step - 0.02f, 0.9f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = Vector2.zero;
            }

            TMP_Text txt = obj.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                txt.text = label;
                txt.alignment = TextAlignmentOptions.Center;
            }
        }
    }
}
