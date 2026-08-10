using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;
using BeastKeeper.Gameplay;
using BeastKeeper.Data;
using BeastKeeper.Systems;

namespace BeastKeeper.Editor
{
    /// <summary>
    /// Editor script to automate setting up the exploration prototype scene, tilemaps, prefabs, and settings.
    /// </summary>
    public static class SetupPrototype
    {
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

            // 3. Create Placeholder Sprites (32x32 for Tilemap blocks)
            Sprite villageGrassSprite = CreateProceduralTile("Assets/BeastKeeper/Art/Sprites/VillageGrass.png", new Color(0.45f, 0.75f, 0.35f, 1f), new Color(0.35f, 0.65f, 0.25f, 1f), "grass");
            Sprite forestGrassSprite = CreateProceduralTile("Assets/BeastKeeper/Art/Sprites/ForestGrass.png", new Color(0.15f, 0.45f, 0.25f, 1f), new Color(0.1f, 0.35f, 0.2f, 1f), "grass");
            Sprite dirtPathSprite = CreateProceduralTile("Assets/BeastKeeper/Art/Sprites/DirtPath.png", new Color(0.8f, 0.65f, 0.45f, 1f), new Color(0.65f, 0.5f, 0.35f, 1f), "dirt");
            Sprite waterSprite = CreateProceduralTile("Assets/BeastKeeper/Art/Sprites/Water.png", new Color(0.2f, 0.5f, 0.8f, 1f), new Color(0.3f, 0.6f, 0.9f, 1f), "water");
            Sprite wallSprite = CreateProceduralTile("Assets/BeastKeeper/Art/Sprites/Wall.png", new Color(0.6f, 0.4f, 0.3f, 1f), new Color(0.45f, 0.3f, 0.2f, 1f), "brick");
            Sprite fenceSprite = CreateProceduralTile("Assets/BeastKeeper/Art/Sprites/Fence.png", new Color(0.7f, 0.55f, 0.4f, 1f), new Color(0.5f, 0.35f, 0.2f, 1f), "fence");
            Sprite treeSprite = CreateProceduralTile("Assets/BeastKeeper/Art/Sprites/Tree.png", new Color(0.2f, 0.6f, 0.3f, 1f), new Color(0.4f, 0.25f, 0.15f, 1f), "tree");
            Sprite wellSprite = CreateProceduralTile("Assets/BeastKeeper/Art/Sprites/Well.png", new Color(0.5f, 0.5f, 0.5f, 1f), new Color(0.3f, 0.3f, 0.3f, 1f), "well");

            // Also keep standard placeholders for player/NPC if they don't exist
            Sprite playerSprite = CreateSolidColorSprite("Assets/BeastKeeper/Art/Sprites/Placeholder_Player.png", Color.blue);
            Sprite npcSprite = CreateSolidColorSprite("Assets/BeastKeeper/Art/Sprites/Placeholder_NPC.png", Color.green);

            // 4. Create Tile Assets
            Tile villageGrassTile = CreateTileAsset(villageGrassSprite, "VillageGrass");
            Tile forestGrassTile = CreateTileAsset(forestGrassSprite, "ForestGrass");
            Tile dirtPathTile = CreateTileAsset(dirtPathSprite, "DirtPath");
            Tile waterTile = CreateTileAsset(waterSprite, "Water");
            Tile wallTile = CreateTileAsset(wallSprite, "Wall");
            Tile fenceTile = CreateTileAsset(fenceSprite, "Fence");
            Tile treeTile = CreateTileAsset(treeSprite, "Tree");
            Tile wellTile = CreateTileAsset(wellSprite, "Well");

            // 5. Create NPC Prefab
            GameObject npcPrefab = CreateNpcPrefab(npcSprite);

            // 6. Create Player Prefab
            GameObject playerPrefab = CreatePlayerPrefab(playerSprite, inputAsset);

            // 7. Create Dialogue Assets
            DialogueData oldKeeperDialogue = CreateOldKeeperDialogue();
            DialogueData shopkeeperDialogue = CreateSimpleDialogue("ShopkeeperDialogue", "Shopkeeper", "Welcome to the shop! I don't have items to sell yet.");
            DialogueData villager1Dialogue = CreateSimpleDialogue("Villager1Dialogue", "Villager Leo", "The forest to the north is beautiful, but be careful!");
            DialogueData villager2Dialogue = CreateSimpleDialogue("Villager2Dialogue", "Villager Mia", "My grandfather says there used to be monsters in the forest.");
            DialogueData villager3Dialogue = CreateSimpleDialogue("Villager3Dialogue", "Villager Bob", "The well in the center of the village has the freshest water.");

            // 8. Create Scene
            CreateScene(
                villageGrassTile, 
                forestGrassTile, 
                dirtPathTile, 
                waterTile, 
                wallTile, 
                fenceTile, 
                treeTile, 
                wellTile, 
                playerPrefab, 
                npcPrefab,
                oldKeeperDialogue,
                shopkeeperDialogue,
                villager1Dialogue,
                villager2Dialogue,
                villager3Dialogue);

            AssetDatabase.SaveAssets();
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

        private static Sprite CreateProceduralTile(string path, Color baseColor, Color detailColor, string pattern)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            if (File.Exists(path))
            {
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            Texture2D tex = new Texture2D(32, 32);
            tex.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[32 * 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    Color col = baseColor;
                    
                    if (pattern == "checker")
                    {
                        if ((x / 4 + y / 4) % 2 == 0)
                        {
                            col = Color.Lerp(baseColor, detailColor, 0.15f);
                        }
                    }
                    else if (pattern == "grass")
                    {
                        if ((x * 7 + y * 13) % 29 == 0 || (x * 3 + y * 17) % 31 == 0)
                        {
                            col = detailColor;
                        }
                    }
                    else if (pattern == "dirt")
                    {
                        if ((x * 11 + y * 7) % 19 == 0)
                        {
                            col = detailColor;
                        }
                    }
                    else if (pattern == "water")
                    {
                        if (y % 8 == 0 && (x > 4 && x < 12 || x > 18 && x < 28))
                        {
                            col = detailColor;
                        }
                    }
                    else if (pattern == "brick")
                    {
                        if (y % 8 == 0 || (x + (y / 8) * 8) % 16 == 0)
                        {
                            col = detailColor;
                        }
                    }
                    else if (pattern == "fence")
                    {
                        if (x % 16 < 4 || y == 12 || y == 20)
                        {
                            col = detailColor;
                        }
                    }
                    else if (pattern == "tree")
                    {
                        if (y < 8)
                        {
                            if (x >= 12 && x <= 20) col = detailColor;
                            else col = new Color(0, 0, 0, 0);
                        }
                        else
                        {
                            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 20));
                            if (dist < 12)
                            {
                                col = baseColor;
                                if ((x + y) % 4 == 0) col = detailColor;
                            }
                            else
                            {
                                col = new Color(0, 0, 0, 0);
                            }
                        }
                    }
                    else if (pattern == "well")
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                        if (dist < 14 && dist > 4)
                        {
                            col = baseColor;
                            if ((x * 3 + y * 7) % 5 == 0) col = detailColor;
                        }
                        else if (dist <= 4)
                        {
                            col = new Color(0.2f, 0.2f, 0.4f, 1f);
                        }
                        else
                        {
                            col = new Color(0, 0, 0, 0);
                        }
                    }
                    
                    pixels[y * 32 + x] = col;
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.spriteImportMode = SpriteImportMode.Single;
                if (pattern == "tree" || pattern == "fence" || pattern == "well")
                {
                    importer.alphaIsTransparency = true;
                }
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Tile CreateTileAsset(Sprite sprite, string name)
        {
            string path = $"Assets/BeastKeeper/Art/Tiles/{name}.asset";
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            Tile tile = null;
            bool exists = File.Exists(path);
            if (exists)
            {
                tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            }

            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
            }

            tile.sprite = sprite;
            
            if (name.Contains("Wall") || name.Contains("Fence") || name.Contains("Tree") || name.Contains("Water") || name.Contains("Well"))
            {
                tile.colliderType = Tile.ColliderType.Grid;
            }
            else
            {
                tile.colliderType = Tile.ColliderType.None;
            }

            if (!exists)
            {
                AssetDatabase.CreateAsset(tile, path);
            }
            else
            {
                EditorUtility.SetDirty(tile);
            }
            return tile;
        }

        private static DialogueData CreateOldKeeperDialogue()
        {
            string assetPath = "Assets/BeastKeeper/Data/Dialogue/OldKeeperIntroDialogue.asset";
            string dir = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            DialogueData data = null;
            bool exists = File.Exists(assetPath);
            if (exists)
            {
                data = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);
            }

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<DialogueData>();
            }

            var idField = typeof(EntityData).GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nameField = typeof(EntityData).GetField("displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            idField.SetValue(data, "old_keeper_intro");
            nameField.SetValue(data, "Old Keeper Intro");

            var nodeList = new List<DialogueNode>();
            nodeList.Add(CreateDialogueNode("Old Keeper", "Ah, there you are.", null));
            nodeList.Add(CreateDialogueNode("Old Keeper", "I was beginning to wonder when you'd return.", null));
            nodeList.Add(CreateDialogueNode("Old Keeper", "Be careful beyond the village gates.", null));

            var nodesField = typeof(DialogueData).GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nodesField.SetValue(data, nodeList);

            if (!exists)
            {
                AssetDatabase.CreateAsset(data, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(data);
            }

            return data;
        }

        private static DialogueData CreateSimpleDialogue(string filename, string speakerName, string text)
        {
            string assetPath = $"Assets/BeastKeeper/Data/Dialogue/{filename}.asset";
            string dir = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            DialogueData data = null;
            bool exists = File.Exists(assetPath);
            if (exists)
            {
                data = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);
            }

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<DialogueData>();
            }

            var idField = typeof(EntityData).GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nameField = typeof(EntityData).GetField("displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            idField.SetValue(data, filename);
            nameField.SetValue(data, speakerName + " Dialogue");

            var nodeList = new List<DialogueNode>();
            nodeList.Add(CreateDialogueNode(speakerName, text, null));

            var nodesField = typeof(DialogueData).GetField("nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nodesField.SetValue(data, nodeList);

            if (!exists)
            {
                AssetDatabase.CreateAsset(data, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(data);
            }

            return data;
        }

        private static DialogueNode CreateDialogueNode(string speaker, string text, List<DialogueChoice> choices)
        {
            DialogueNode node = new DialogueNode();
            
            var speakerField = typeof(DialogueNode).GetField("speakerName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var textField = typeof(DialogueNode).GetField("text", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var choicesField = typeof(DialogueNode).GetField("choices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            speakerField.SetValue(node, speaker);
            textField.SetValue(node, text);
            choicesField.SetValue(node, choices ?? new List<DialogueChoice>());

            return node;
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
            col.size = new Vector2(0.9f, 0.9f);

            PlayerInput input = go.AddComponent<PlayerInput>();
            input.actions = inputAsset;
            input.defaultActionMap = "Player";

            PlayerController controller = go.AddComponent<PlayerController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("moveSpeed").floatValue = 5.0f;
            serializedController.FindProperty("interactionRadius").floatValue = 1.2f;
            serializedController.FindProperty("interactableLayer").intValue = 1 << LayerMask.NameToLayer("Interactable");
            serializedController.ApplyModifiedProperties();

            // Configure Rigidbody2D on Player Prefab
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = go.AddComponent<Rigidbody2D>();
            }
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void CreateScene(
            Tile villageGrassTile, 
            Tile forestGrassTile, 
            Tile dirtPathTile, 
            Tile waterTile, 
            Tile wallTile, 
            Tile fenceTile, 
            Tile treeTile, 
            Tile wellTile, 
            GameObject playerPrefab, 
            GameObject npcPrefab,
            DialogueData oldKeeperDialogue,
            DialogueData shopkeeperDialogue,
            DialogueData villager1Dialogue,
            DialogueData villager2Dialogue,
            DialogueData villager3Dialogue)
        {
            string scenePath = "Assets/BeastKeeper/Scenes/Prototype_Exploration.unity";
            string dir = Path.GetDirectoryName(scenePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 1. Set Up World and Grid
            GameObject worldGo = new GameObject("World");
            GameObject gridGo = new GameObject("Grid");
            gridGo.transform.parent = worldGo.transform;
            Grid grid = gridGo.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // 2. Add Tilemaps
            GameObject groundGo = new GameObject("Ground");
            groundGo.transform.parent = gridGo.transform;
            Tilemap groundMap = groundGo.AddComponent<Tilemap>();
            TilemapRenderer groundRenderer = groundGo.AddComponent<TilemapRenderer>();
            groundRenderer.sortingLayerName = "Ground";

            GameObject pathsGo = new GameObject("Paths");
            pathsGo.transform.parent = gridGo.transform;
            Tilemap pathsMap = pathsGo.AddComponent<Tilemap>();
            TilemapRenderer pathsRenderer = pathsGo.AddComponent<TilemapRenderer>();
            pathsRenderer.sortingLayerName = "Ground";
            pathsRenderer.sortingOrder = 1;

            GameObject decorationGo = new GameObject("Decoration");
            decorationGo.transform.parent = gridGo.transform;
            Tilemap decorationMap = decorationGo.AddComponent<Tilemap>();
            TilemapRenderer decorationRenderer = decorationGo.AddComponent<TilemapRenderer>();
            decorationRenderer.sortingLayerName = "Obstacles";

            GameObject collisionGo = new GameObject("Collision");
            collisionGo.transform.parent = gridGo.transform;
            Tilemap collisionMap = collisionGo.AddComponent<Tilemap>();
            TilemapRenderer collisionRenderer = collisionGo.AddComponent<TilemapRenderer>();
            collisionRenderer.sortingLayerName = "Obstacles";
            collisionRenderer.sortingOrder = 1;
            
            // Add physics collision to Collision Map
            TilemapCollider2D tilemapCollider = collisionGo.AddComponent<TilemapCollider2D>();
            Rigidbody2D collisionRb = collisionGo.AddComponent<Rigidbody2D>();
            collisionRb.bodyType = RigidbodyType2D.Static;
            CompositeCollider2D composite = collisionGo.AddComponent<CompositeCollider2D>();
            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            // 3. Paint Village & Forest Ground
            for (int x = -15; x <= 14; x++)
            {
                for (int y = -15; y <= 14; y++)
                {
                    groundMap.SetTile(new Vector3Int(x, y, 0), villageGrassTile);
                }
            }
            for (int x = -20; x <= 19; x++)
            {
                for (int y = 15; y <= 54; y++)
                {
                    groundMap.SetTile(new Vector3Int(x, y, 0), forestGrassTile);
                }
            }

            // 4. Paint Paths
            for (int x = -2; x <= 1; x++)
            {
                for (int y = -10; y <= 50; y++)
                {
                    pathsMap.SetTile(new Vector3Int(x, y, 0), dirtPathTile);
                }
            }
            for (int x = -12; x <= 12; x++)
            {
                for (int y = 0; y <= 1; y++)
                {
                    pathsMap.SetTile(new Vector3Int(x, y, 0), dirtPathTile);
                }
            }
            for (int x = 2; x <= 15; x++)
            {
                for (int y = 24; y <= 25; y++)
                {
                    pathsMap.SetTile(new Vector3Int(x, y, 0), dirtPathTile);
                }
            }
            for (int x = -15; x <= -3; x++)
            {
                for (int y = 39; y <= 40; y++)
                {
                    pathsMap.SetTile(new Vector3Int(x, y, 0), dirtPathTile);
                }
            }
            for (int y = 41; y <= 47; y++)
            {
                for (int x = -13; x <= -11; x++)
                {
                    pathsMap.SetTile(new Vector3Int(x, y, 0), dirtPathTile);
                }
            }
            for (int x = -15; x <= -10; x++)
            {
                for (int y = 47; y <= 49; y++)
                {
                    pathsMap.SetTile(new Vector3Int(x, y, 0), dirtPathTile);
                }
            }

            // 5. Paint Boundary Obstacles
            for (int x = -15; x <= 14; x++) collisionMap.SetTile(new Vector3Int(x, -15, 0), wallTile);
            for (int y = -15; y <= 14; y++) collisionMap.SetTile(new Vector3Int(-15, y, 0), wallTile);
            for (int y = -15; y <= 14; y++) collisionMap.SetTile(new Vector3Int(14, y, 0), wallTile);
            for (int x = -15; x <= 14; x++)
            {
                if (x < -2 || x > 1)
                {
                    collisionMap.SetTile(new Vector3Int(x, 14, 0), wallTile);
                }
            }
            for (int x = -20; x <= 19; x++) collisionMap.SetTile(new Vector3Int(x, 54, 0), wallTile);
            for (int y = 15; y <= 54; y++) collisionMap.SetTile(new Vector3Int(-20, y, 0), wallTile);
            for (int y = 15; y <= 54; y++) collisionMap.SetTile(new Vector3Int(19, y, 0), wallTile);
            for (int x = -20; x <= -16; x++) collisionMap.SetTile(new Vector3Int(x, 15, 0), wallTile);
            for (int x = 15; x <= 19; x++) collisionMap.SetTile(new Vector3Int(x, 15, 0), wallTile);

            // 6. Paint Village Structures & Collision
            collisionMap.SetTile(new Vector3Int(0, 0, 0), wellTile);
            collisionMap.SetTile(new Vector3Int(0, 1, 0), wellTile);
            collisionMap.SetTile(new Vector3Int(1, 0, 0), wellTile);
            collisionMap.SetTile(new Vector3Int(1, 1, 0), wellTile);

            PaintBuilding(collisionMap, wallTile, -10, 4, 4, 3);
            PaintBuilding(collisionMap, wallTile, 7, 4, 4, 3);
            PaintBuilding(collisionMap, wallTile, -10, -7, 4, 3);
            PaintBuilding(collisionMap, wallTile, 7, -7, 4, 3);
            
            for (int x = -13; x <= -4; x++) collisionMap.SetTile(new Vector3Int(x, 2, 0), fenceTile);
            for (int x = 4; x <= 13; x++) collisionMap.SetTile(new Vector3Int(x, 2, 0), fenceTile);

            // 7. Paint Forest Obstacles
            for (int x = 8; x <= 13; x++)
            {
                for (int y = 30; y <= 34; y++)
                {
                    collisionMap.SetTile(new Vector3Int(x, y, 0), waterTile);
                }
            }
            for (int x = -19; x <= 18; x++)
            {
                for (int y = 16; y <= 53; y++)
                {
                    if (pathsMap.HasTile(new Vector3Int(x, y, 0)) || collisionMap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        continue;
                    }
                    if ((x * 17 + y * 23) % 5 < 3)
                    {
                        collisionMap.SetTile(new Vector3Int(x, y, 0), treeTile);
                    }
                }
            }
            for (int x = -14; x <= 13; x++)
            {
                for (int y = -14; y <= 13; y++)
                {
                    if (pathsMap.HasTile(new Vector3Int(x, y, 0)) || collisionMap.HasTile(new Vector3Int(x, y, 0)))
                    {
                        continue;
                    }
                    if (x <= -12 || x >= 11 || y <= -12 || y >= 11)
                    {
                        if ((x * 71 + y * 97) % 7 < 3)
                        {
                            collisionMap.SetTile(new Vector3Int(x, y, 0), treeTile);
                        }
                    }
                }
            }

            // 8. Instantiate Area Bounds Triggers
            GameObject triggersGo = new GameObject("AreaTransitions");
            triggersGo.transform.parent = worldGo.transform;

            GameObject villageTrigger = new GameObject("VillageBoundsTrigger");
            villageTrigger.transform.parent = triggersGo.transform;
            villageTrigger.transform.position = new Vector3(-0.5f, -0.5f, 0f);
            BoxCollider2D villageCol = villageTrigger.AddComponent<BoxCollider2D>();
            villageCol.size = new Vector2(30f, 30f);
            AreaBoundsTrigger villageBounds = villageTrigger.AddComponent<AreaBoundsTrigger>();
            
            SerializedObject sVillage = new SerializedObject(villageBounds);
            sVillage.FindProperty("areaName").stringValue = "Village";
            sVillage.FindProperty("minBounds").vector2Value = new Vector2(-15f, -15f);
            sVillage.FindProperty("maxBounds").vector2Value = new Vector2(15f, 15f);
            sVillage.ApplyModifiedProperties();

            GameObject forestTrigger = new GameObject("ForestBoundsTrigger");
            forestTrigger.transform.parent = triggersGo.transform;
            forestTrigger.transform.position = new Vector3(-0.5f, 34.5f, 0f);
            BoxCollider2D forestCol = forestTrigger.AddComponent<BoxCollider2D>();
            forestCol.size = new Vector2(40f, 40f);
            AreaBoundsTrigger forestBounds = forestTrigger.AddComponent<AreaBoundsTrigger>();
            
            SerializedObject sForest = new SerializedObject(forestBounds);
            sForest.FindProperty("areaName").stringValue = "Forest";
            sForest.FindProperty("minBounds").vector2Value = new Vector2(-20f, 15f);
            sForest.FindProperty("maxBounds").vector2Value = new Vector2(20f, 55f);
            sForest.ApplyModifiedProperties();

            // 9. Instantiate NPCs
            GameObject npc1 = PrefabUtility.InstantiatePrefab(npcPrefab) as GameObject;
            npc1.name = "Old Keeper";
            npc1.transform.position = new Vector3(3f, 1f, 0f);
            SetNpcProperties(npc1, "Old Keeper", oldKeeperDialogue);
            
            GameObject npc2 = PrefabUtility.InstantiatePrefab(npcPrefab) as GameObject;
            npc2.name = "Shopkeeper Placeholder";
            npc2.transform.position = new Vector3(6f, 3f, 0f);
            SetNpcProperties(npc2, "Shopkeeper", shopkeeperDialogue);

            GameObject npc3 = PrefabUtility.InstantiatePrefab(npcPrefab) as GameObject;
            npc3.name = "Villager 1";
            npc3.transform.position = new Vector3(-6f, -1f, 0f);
            SetNpcProperties(npc3, "Villager Leo", villager1Dialogue);

            GameObject npc4 = PrefabUtility.InstantiatePrefab(npcPrefab) as GameObject;
            npc4.name = "Villager 2";
            npc4.transform.position = new Vector3(-5f, 3f, 0f);
            SetNpcProperties(npc4, "Villager Mia", villager2Dialogue);

            GameObject npc5 = PrefabUtility.InstantiatePrefab(npcPrefab) as GameObject;
            npc5.name = "Villager 3";
            npc5.transform.position = new Vector3(5f, -3f, 0f);
            SetNpcProperties(npc5, "Villager Bob", villager3Dialogue);

            // 10. Instantiate Player
            GameObject playerInstance = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            playerInstance.transform.position = new Vector3(0f, -5f, 0f);

            // 11. Setup Camera bounds dynamically
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = 5f;
                CameraController camController = mainCam.gameObject.GetComponent<CameraController>();
                if (camController == null)
                {
                    camController = mainCam.gameObject.AddComponent<CameraController>();
                }
                
                SerializedObject serializedCam = new SerializedObject(camController);
                serializedCam.FindProperty("target").objectReferenceValue = playerInstance.transform;
                serializedCam.FindProperty("followSpeed").floatValue = 5.0f;
                serializedCam.FindProperty("useBounds").boolValue = true;
                serializedCam.FindProperty("minBounds").vector2Value = new Vector2(-15f, -15f);
                serializedCam.FindProperty("maxBounds").vector2Value = new Vector2(15f, 15f);
                serializedCam.ApplyModifiedProperties();
            }

            // 12. Create DialogueSystem Service
            GameObject dialogueSysGo = new GameObject("DialogueSystem");
            dialogueSysGo.transform.parent = worldGo.transform;
            dialogueSysGo.AddComponent<DialogueSystem>();

            // 13. Create Canvas and Dialogue UI
            GameObject canvasGo = new GameObject("DialogueCanvas");
            canvasGo.transform.parent = worldGo.transform;
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            // Dialogue Panel
            GameObject panelGo = new GameObject("DialoguePanel");
            panelGo.transform.parent = canvasGo.transform;
            Image panelImage = panelGo.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            
            RectTransform panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 20f);
            panelRect.sizeDelta = new Vector2(-40f, 160f);

            // Speaker Text
            GameObject speakerGo = new GameObject("SpeakerName");
            speakerGo.transform.parent = panelGo.transform;
            TextMeshProUGUI speakerText = speakerGo.AddComponent<TextMeshProUGUI>();
            speakerText.fontSize = 24;
            speakerText.fontStyle = FontStyles.Bold;
            speakerText.color = Color.yellow;
            speakerText.text = "Speaker";
            
            RectTransform speakerRect = speakerGo.GetComponent<RectTransform>();
            speakerRect.anchorMin = new Vector2(0f, 1f);
            speakerRect.anchorMax = new Vector2(0f, 1f);
            speakerRect.pivot = new Vector2(0f, 1f);
            speakerRect.anchoredPosition = new Vector2(20f, -15f);
            speakerRect.sizeDelta = new Vector2(400f, 40f);

            // Dialogue Text
            GameObject textGo = new GameObject("DialogueText");
            textGo.transform.parent = panelGo.transform;
            TextMeshProUGUI mainText = textGo.AddComponent<TextMeshProUGUI>();
            mainText.fontSize = 20;
            mainText.color = Color.white;
            mainText.text = "Dialogue content...";
            
            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, -20f);
            textRect.sizeDelta = new Vector2(-40f, -60f);

            // Continue Indicator
            GameObject indicatorGo = new GameObject("ContinueIndicator");
            indicatorGo.transform.parent = panelGo.transform;
            TextMeshProUGUI indicatorText = indicatorGo.AddComponent<TextMeshProUGUI>();
            indicatorText.fontSize = 16;
            indicatorText.color = Color.gray;
            indicatorText.text = "Press E/Space to continue...";
            indicatorText.alignment = TextAlignmentOptions.BottomRight;
            
            RectTransform indicatorRect = indicatorGo.GetComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(1f, 0f);
            indicatorRect.anchorMax = new Vector2(1f, 0f);
            indicatorRect.pivot = new Vector2(1f, 0f);
            indicatorRect.anchoredPosition = new Vector2(-20f, 10f);
            indicatorRect.sizeDelta = new Vector2(250f, 30f);

            // Choices Container
            GameObject choicesGo = new GameObject("ChoicesContainer");
            choicesGo.transform.parent = panelGo.transform;
            VerticalLayoutGroup vlg = choicesGo.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleRight;
            vlg.childControlHeight = false;
            vlg.childControlWidth = false;
            vlg.spacing = 5f;
            
            RectTransform choicesRect = choicesGo.GetComponent<RectTransform>();
            choicesRect.anchorMin = new Vector2(1f, 0.5f);
            choicesRect.anchorMax = new Vector2(1f, 0.5f);
            choicesRect.pivot = new Vector2(1f, 0.5f);
            choicesRect.anchoredPosition = new Vector2(-20f, 0f);
            choicesRect.sizeDelta = new Vector2(250f, 120f);

            // Button Template
            GameObject buttonTemplate = new GameObject("ChoiceButtonTemplate");
            buttonTemplate.transform.parent = canvasGo.transform;
            Image btnImage = buttonTemplate.AddComponent<Image>();
            btnImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            Button btn = buttonTemplate.AddComponent<Button>();
            
            GameObject btnTextGo = new GameObject("Text");
            btnTextGo.transform.parent = buttonTemplate.transform;
            TextMeshProUGUI btnText = btnTextGo.AddComponent<TextMeshProUGUI>();
            btnText.fontSize = 16;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.text = "Option";
            
            RectTransform btnTextRect = btnTextGo.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;
            
            RectTransform btnRect = buttonTemplate.GetComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(240f, 30f);
            buttonTemplate.SetActive(false); // Hide template

            // Configure DialogueUI
            DialogueUI uiComponent = canvasGo.AddComponent<DialogueUI>();
            SerializedObject sUi = new SerializedObject(uiComponent);
            sUi.FindProperty("dialoguePanel").objectReferenceValue = panelGo;
            sUi.FindProperty("speakerNameText").objectReferenceValue = speakerText;
            sUi.FindProperty("dialogueText").objectReferenceValue = mainText;
            sUi.FindProperty("continueIndicator").objectReferenceValue = indicatorGo;
            sUi.FindProperty("choicesContainer").objectReferenceValue = choicesRect;
            sUi.FindProperty("choiceButtonPrefab").objectReferenceValue = buttonTemplate;
            sUi.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static void PaintBuilding(Tilemap map, Tile tile, int startX, int startY, int width, int height)
        {
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY; y < startY + height; y++)
                {
                    map.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        private static void SetNpcProperties(GameObject npcInstance, string name, DialogueData dialogue)
        {
            InteractableNPC npc = npcInstance.GetComponent<InteractableNPC>();
            if (npc != null)
            {
                SerializedObject sNpc = new SerializedObject(npc);
                sNpc.FindProperty("npcName").stringValue = name;
                sNpc.FindProperty("dialogueData").objectReferenceValue = dialogue;
                sNpc.ApplyModifiedProperties();
            }
        }
    }
}
