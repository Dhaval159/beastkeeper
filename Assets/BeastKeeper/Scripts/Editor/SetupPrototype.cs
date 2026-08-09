using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using BeastKeeper.Gameplay;

namespace BeastKeeper.Editor
{
    /// <summary>
    /// Editor script to automate setting up the exploration prototype scene, prefabs, and settings.
    /// </summary>
    [InitializeOnLoad]
    public static class SetupPrototype
    {
        static SetupPrototype()
        {
            string scenePath = "Assets/BeastKeeper/Scenes/Prototype_Exploration.unity";
            if (!File.Exists(scenePath))
            {
                EditorApplication.delayCall += () =>
                {
                    if (!File.Exists(scenePath))
                    {
                        RunSetup();
                    }
                };
            }
            else
            {
                EditorApplication.delayCall += () =>
                {
                    if (EditorSceneManager.activeScene.path != scenePath)
                    {
                        EditorSceneManager.OpenScene(scenePath);
                    }
                };
            }
        }

        public static void RunSetupFromCommandLine()
        {
            Debug.Log("[SetupPrototype] Running setup from command line...");
            RunSetup();
        }

        [MenuItem("Beast Keeper/Setup Prototype Scene")]
        public static void RunSetup()
        {
            Debug.Log("[SetupPrototype] Starting setup...");

            // 1. Setup Layers and Sorting Layers
            SetupLayers();
            SetupSortingLayers();

            // 2. Setup Input Action Asset
            InputActionAsset inputAsset = SetupInputAsset();

            // 3. Create Placeholder Sprites
            Sprite playerSprite = CreateSolidColorSprite("Assets/BeastKeeper/Art/Sprites/Placeholder_Player.png", Color.blue);
            Sprite npcSprite = CreateSolidColorSprite("Assets/BeastKeeper/Art/Sprites/Placeholder_NPC.png", Color.green);
            Sprite obstacleSprite = CreateSolidColorSprite("Assets/BeastKeeper/Art/Sprites/Placeholder_Obstacle.png", Color.red);
            Sprite groundSprite = CreateSolidColorSprite("Assets/BeastKeeper/Art/Sprites/Placeholder_Ground.png", Color.gray);

            // 4. Create NPC Prefab
            GameObject npcPrefab = CreateNpcPrefab(npcSprite);

            // 5. Create Player Prefab
            GameObject playerPrefab = CreatePlayerPrefab(playerSprite, inputAsset);

            // 6. Create Scene
            CreateScene(groundSprite, obstacleSprite, playerPrefab, npcPrefab);

            Debug.Log("[SetupPrototype] Setup completed successfully!");
        }

        private static void SetupLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            
            SetLayerName(layers, 6, "Player");
            SetLayerName(layers, 7, "Obstacle");
            SetLayerName(layers, 8, "Interactable");
            
            tagManager.ApplyModifiedProperties();
        }

        private static void SetLayerName(SerializedProperty layers, int index, string name)
        {
            if (index < layers.arraySize)
            {
                layers.GetArrayElementAtIndex(index).stringValue = name;
            }
        }

        private static void SetupSortingLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");
            
            AddSortingLayer(sortingLayers, "Ground");
            AddSortingLayer(sortingLayers, "Obstacles");
            AddSortingLayer(sortingLayers, "Entities");
            
            tagManager.ApplyModifiedProperties();
        }

        private static void AddSortingLayer(SerializedProperty sortingLayers, string name)
        {
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                if (sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name)
                    return;
            }

            sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
            SerializedProperty newLayer = sortingLayers.GetArrayElementAtIndex(sortingLayers.arraySize - 1);
            newLayer.FindPropertyRelative("name").stringValue = name;
            newLayer.FindPropertyRelative("uniqueID").intValue = name.GetHashCode();
        }

        private static InputActionAsset SetupInputAsset()
        {
            string srcPath = "Assets/Settings/InputSystem_Actions.inputactions";
            string destPath = "Assets/BeastKeeper/Settings/Input/BeastKeeperInputActions.inputactions";
            
            if (!File.Exists(destPath))
            {
                string dir = Path.GetDirectoryName(destPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                AssetDatabase.CopyAsset(srcPath, destPath);
                AssetDatabase.Refresh();
            }

            return AssetDatabase.LoadAssetAtPath<InputActionAsset>(destPath);
        }

        private static Sprite CreateSolidColorSprite(string path, Color color)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (File.Exists(path))
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            Texture2D tex = new Texture2D(16, 16);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreateNpcPrefab(Sprite npcSprite)
        {
            string prefabPath = "Assets/BeastKeeper/Prefabs/Characters/NPC.prefab";
            string dir = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            GameObject go = new GameObject("NPC");
            go.layer = LayerMask.NameToLayer("Interactable");

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = npcSprite;
            sr.sortingLayerName = "Entities";

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            InteractableNPC npc = go.AddComponent<InteractableNPC>();
            SerializedObject serializedNpc = new SerializedObject(npc);
            serializedNpc.FindProperty("npcName").stringValue = "Old Keeper";
            serializedNpc.FindProperty("interactionMessage").stringValue = "Hello, Beast Keeper. Be careful in the wild.";
            serializedNpc.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreatePlayerPrefab(Sprite playerSprite, InputActionAsset inputAsset)
        {
            string prefabPath = "Assets/BeastKeeper/Prefabs/Characters/Player.prefab";
            string dir = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            GameObject go = new GameObject("Player");
            go.tag = "Player";
            go.layer = LayerMask.NameToLayer("Player");

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = playerSprite;
            sr.sortingLayerName = "Entities";

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.9f, 0.9f); // Slightly smaller to ease grid movement

            PlayerInput input = go.AddComponent<PlayerInput>();
            input.actions = inputAsset;
            input.defaultActionMap = "Player";

            PlayerController controller = go.AddComponent<PlayerController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("moveSpeed").floatValue = 5.0f;
            serializedController.FindProperty("interactionRadius").floatValue = 1.2f;
            serializedController.FindProperty("interactableLayer").intValue = 1 << LayerMask.NameToLayer("Interactable");
            serializedController.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreateScene(Sprite groundSprite, Sprite obstacleSprite, GameObject playerPrefab, GameObject npcPrefab)
        {
            string scenePath = "Assets/BeastKeeper/Scenes/Prototype_Exploration.unity";
            string dir = Path.GetDirectoryName(scenePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 1. Set Up Ground Grid
            GameObject gridGo = new GameObject("GroundGrid");
            for (int x = -10; x <= 10; x++)
            {
                for (int y = -10; y <= 10; y++)
                {
                    GameObject tile = new GameObject($"Tile_{x}_{y}");
                    tile.transform.parent = gridGo.transform;
                    tile.transform.position = new Vector3(x, y, 0);
                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = groundSprite;
                    sr.sortingLayerName = "Ground";
                    
                    // Simple checkerboard tint
                    if (System.Math.Abs(x + y) % 2 == 0)
                    {
                        sr.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                    }
                }
            }

            // 2. Set Up Obstacles Group
            GameObject obstaclesGo = new GameObject("Obstacles");

            // Bounding Walls
            CreateWall(obstaclesGo, new Vector3(0, 11, 0), new Vector2(23, 1), obstacleSprite);
            CreateWall(obstaclesGo, new Vector3(0, -11, 0), new Vector2(23, 1), obstacleSprite);
            CreateWall(obstaclesGo, new Vector3(-11, 0, 0), new Vector2(1, 23), obstacleSprite);
            CreateWall(obstaclesGo, new Vector3(11, 0, 0), new Vector2(1, 23), obstacleSprite);

            // Village Houses (Placeholder Blocks)
            CreateHouseObstacle(obstaclesGo, new Vector3(-4, 4, 0), new Vector2(3, 3), "House 1", obstacleSprite);
            CreateHouseObstacle(obstaclesGo, new Vector3(4, 4, 0), new Vector2(3, 3), "House 2", obstacleSprite);

            // Obstacle Trees
            CreateTreeObstacle(obstaclesGo, new Vector3(-5, -3, 0), obstacleSprite);
            CreateTreeObstacle(obstaclesGo, new Vector3(5, -3, 0), obstacleSprite);
            CreateTreeObstacle(obstaclesGo, new Vector3(0, -6, 0), obstacleSprite);

            // 3. Instantiate NPC
            GameObject npcInstance = PrefabUtility.InstantiatePrefab(npcPrefab) as GameObject;
            npcInstance.transform.position = new Vector3(3, 1, 0);

            // 4. Instantiate Player
            GameObject playerInstance = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            playerInstance.transform.position = new Vector3(0, -2, 0);

            // 5. Setup Camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = 5f;
                CameraController camController = mainCam.gameObject.AddComponent<CameraController>();
                
                SerializedObject serializedCam = new SerializedObject(camController);
                serializedCam.FindProperty("target").objectReferenceValue = playerInstance.transform;
                serializedCam.FindProperty("followSpeed").floatValue = 5.0f;
                serializedCam.ApplyModifiedProperties();
            }

            // Save Scene
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void CreateWall(GameObject parent, Vector3 position, Vector2 size, Sprite sprite)
        {
            GameObject wall = new GameObject("BoundaryWall");
            wall.transform.parent = parent.transform;
            wall.transform.position = position;
            wall.layer = LayerMask.NameToLayer("Obstacle");

            SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.size = size;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.color = Color.black;
            sr.sortingLayerName = "Obstacles";

            BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private static void CreateHouseObstacle(GameObject parent, Vector3 position, Vector2 size, string name, Sprite sprite)
        {
            GameObject house = new GameObject(name);
            house.transform.parent = parent.transform;
            house.transform.position = position;
            house.layer = LayerMask.NameToLayer("Obstacle");

            SpriteRenderer sr = house.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.size = size;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brown
            sr.sortingLayerName = "Obstacles";

            BoxCollider2D col = house.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private static void CreateTreeObstacle(GameObject parent, Vector3 position, Sprite sprite)
        {
            GameObject tree = new GameObject("Tree");
            tree.transform.parent = parent.transform;
            tree.transform.position = position;
            tree.layer = LayerMask.NameToLayer("Obstacle");

            SpriteRenderer sr = tree.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
            sr.sortingLayerName = "Obstacles";

            BoxCollider2D col = tree.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }
    }
}
