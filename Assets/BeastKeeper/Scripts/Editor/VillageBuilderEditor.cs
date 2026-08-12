using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BeastKeeper.Data;
using BeastKeeper.Gameplay;

namespace BeastKeeper.Editor
{
    public static class VillageBuilderEditor
    {
        private const string SpritesPath = "Assets/BeastKeeper/Art/Sprites";

        [MenuItem("Beast Keeper/Build Exploration Village")]
        public static void BuildVillage()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.name.Contains("Exploration"))
            {
                EditorSceneManager.OpenScene("Assets/BeastKeeper/Scenes/Prototype_Exploration.unity");
            }

            // Cleanup old redundant root objects
            GameObject existingWorld = GameObject.Find("World");
            if (existingWorld != null) Object.DestroyImmediate(existingWorld);

            GameObject oldGrid = GameObject.Find("GroundGrid");
            if (oldGrid != null) Object.DestroyImmediate(oldGrid);

            GameObject oldObstacles = GameObject.Find("Obstacles");
            if (oldObstacles != null) Object.DestroyImmediate(oldObstacles);

            // Create Master Root Hierarchy
            GameObject world = new GameObject("World");
            GameObject groundRoot = new GameObject("Ground"); groundRoot.transform.SetParent(world.transform);
            GameObject pathsRoot = new GameObject("Paths"); pathsRoot.transform.SetParent(world.transform);
            GameObject buildingsRoot = new GameObject("Buildings"); buildingsRoot.transform.SetParent(world.transform);
            GameObject propsRoot = new GameObject("Props"); propsRoot.transform.SetParent(world.transform);
            GameObject npcsRoot = new GameObject("NPCs"); npcsRoot.transform.SetParent(world.transform);
            GameObject triggersRoot = new GameObject("Triggers"); triggersRoot.transform.SetParent(world.transform);

            // Load Sprites
            Sprite villageGrass = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tiles/VillageGrass.png");
            Sprite villageGrassVar = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tiles/VillageGrassVariant.png");
            Sprite forestGrass = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tiles/ForestGrass.png");
            Sprite forestGrassVar = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tiles/ForestGrassVariant.png");
            if (forestGrass == null) forestGrass = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/ForestGrass.png");

            Sprite dirtSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tiles/DirtPath.png");
            Sprite dirtVarSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tiles/DirtPathVariant.png");
            if (dirtSprite == null) dirtSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/DirtPath.png");

            Sprite waterSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tiles/Water.png");
            if (waterSprite == null) waterSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Water.png");

            Sprite treeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Props/Tree.png");
            Sprite treeVarSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Props/TreeVariant.png");
            if (treeSprite == null) treeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Tree.png");

            Sprite bushSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Props/Bush.png");
            Sprite flowerSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Props/Flower.png");
            Sprite rockSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Props/Rock.png");
            Sprite wellSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Props/Well.png");
            Sprite fenceSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Props/Fence.png");
            Sprite signSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Props/Sign.png");

            Sprite houseWallSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Buildings/HouseWall.png");
            Sprite houseRoofSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Buildings/HouseRoof.png");
            Sprite houseDoorSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Buildings/HouseDoor.png");

            Sprite oldKeeperSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Characters/NPC_OldKeeper.png");
            Sprite shopkeeperSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Characters/NPC_Shopkeeper.png");
            Sprite leoSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Characters/NPC_Leo.png");
            Sprite miaSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Characters/NPC_Mia.png");
            Sprite bobSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesPath}/Characters/NPC_Bob.png");

            // 1. Build Ground Grid (Village: y = -15..12, Forest: y = 13..42)
            for (int x = -18; x <= 18; x++)
            {
                for (int y = -15; y <= 42; y++)
                {
                    bool isForest = y >= 13;
                    bool isPath = false;

                    if (!isForest)
                    {
                        // Village paths
                        isPath = (x >= -1 && x <= 1 && y >= -14 && y <= 13) ||
                                 (y >= -1 && y <= 1 && x >= -10 && x <= 10) ||
                                 (y >= -9 && y <= -7 && x >= -10 && x <= 10);
                    }
                    else
                    {
                        // Forest main winding path
                        if (y >= 13 && y <= 17 && Mathf.Abs(x) <= 1) isPath = true; // Gate to forest entrance
                        else if (y >= 18 && y <= 21 && x >= -4 && x <= -1) isPath = true; // Curve west
                        else if (y >= 22 && y <= 25 && x >= -2 && x <= 1) isPath = true; // Curve back central
                        else if (y >= 26 && y <= 35 && Mathf.Abs(x) <= 1) isPath = true; // Main path north

                        // East branch path (to pond)
                        else if (y >= 21 && y <= 23 && x >= 1 && x <= 12) isPath = true;

                        // West branch path (to secluded clearing)
                        else if (y >= 21 && y <= 23 && x >= -12 && x <= -4) isPath = true;
                    }

                    // Water pond check
                    bool isWater = isForest && (x >= 9 && x <= 13 && y >= 19 && y <= 23);

                    GameObject tile = new GameObject($"Tile_{x}_{y}");
                    tile.transform.position = new Vector3(x, y, 0);
                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

                    if (isWater)
                    {
                        tile.transform.SetParent(propsRoot.transform);
                        sr.sprite = waterSprite;
                        sr.sortingOrder = 1;
                        BoxCollider2D waterCol = tile.AddComponent<BoxCollider2D>();
                        waterCol.size = new Vector2(1f, 1f);
                    }
                    else if (isPath)
                    {
                        tile.transform.SetParent(pathsRoot.transform);
                        sr.sprite = ((x + y) % 3 == 0) ? dirtVarSprite : dirtSprite;
                        sr.sortingOrder = 0;
                    }
                    else
                    {
                        tile.transform.SetParent(groundRoot.transform);
                        if (isForest)
                        {
                            sr.sprite = ((x * 3 + y * 7) % 4 == 0) ? forestGrassVar : (forestGrass != null ? forestGrass : villageGrass);
                        }
                        else
                        {
                            sr.sprite = ((x * 3 + y * 7) % 5 == 0) ? villageGrassVar : villageGrass;
                        }
                        sr.sortingOrder = 0;
                    }
                }
            }

            // 2. Build Village Buildings
            CreateBuilding(buildingsRoot, "OldKeeperHouse", new Vector3(8, 2, 0), houseWallSprite, houseRoofSprite, houseDoorSprite);
            CreateBuilding(buildingsRoot, "Shop", new Vector3(-8, 2, 0), houseWallSprite, houseRoofSprite, houseDoorSprite, signSprite);
            CreateBuilding(buildingsRoot, "House3", new Vector3(-8, -8, 0), houseWallSprite, houseRoofSprite, houseDoorSprite);
            CreateBuilding(buildingsRoot, "House4", new Vector3(8, -8, 0), houseWallSprite, houseRoofSprite, houseDoorSprite);

            // 3. Build Village Props
            if (wellSprite != null)
            {
                GameObject wellObj = new GameObject("VillageWell");
                wellObj.transform.SetParent(propsRoot.transform);
                wellObj.transform.position = new Vector3(0, 0, 0);
                SpriteRenderer sr = wellObj.AddComponent<SpriteRenderer>();
                sr.sprite = wellSprite;
                sr.sortingOrder = 2;
                BoxCollider2D col = wellObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1.2f, 1.2f);
            }

            // Village Border Fences
            for (int x = -16; x <= 16; x += 2)
            {
                if (x >= -2 && x <= 2) continue;
                CreateProp(propsRoot, "Fence", new Vector3(x, -14, 0), fenceSprite, true);
            }

            // Village Outer Border Trees (East & West)
            for (int y = -14; y <= 11; y += 3)
            {
                CreateProp(propsRoot, "Tree", new Vector3(-15, y, 0), treeSprite, true);
                CreateProp(propsRoot, "Tree", new Vector3(15, y, 0), treeVarSprite, true);
            }

            // Northern Village Gate Boundary Trees
            for (int x = -15; x <= 15; x += 2)
            {
                if (x >= -3 && x <= 3) continue; // Northern path open!
                CreateProp(propsRoot, "Tree", new Vector3(x, 12, 0), treeSprite, true);
            }

            // Decorative Village Props
            CreateProp(propsRoot, "Bush", new Vector3(-3, 1, 0), bushSprite, false);
            CreateProp(propsRoot, "Bush", new Vector3(3, 1, 0), bushSprite, false);
            CreateProp(propsRoot, "Flower", new Vector3(-2, -3, 0), flowerSprite, false);
            CreateProp(propsRoot, "Flower", new Vector3(2, -3, 0), flowerSprite, false);
            CreateProp(propsRoot, "Rock", new Vector3(4, -4, 0), rockSprite, true);
            CreateProp(propsRoot, "Sign", new Vector3(2, 6, 0), signSprite, true);

            // 4. Build Forest Area Props & Boundaries (y = 13 to 42)
            // Outer Forest Perimeter Walls (East & West)
            for (int y = 13; y <= 42; y += 2)
            {
                CreateProp(propsRoot, "ForestTree", new Vector3(-17, y, 0), treeSprite, true);
                CreateProp(propsRoot, "ForestTree", new Vector3(-16, y + 1, 0), treeVarSprite, true);
                CreateProp(propsRoot, "ForestTree", new Vector3(17, y, 0), treeVarSprite, true);
                CreateProp(propsRoot, "ForestTree", new Vector3(16, y + 1, 0), treeSprite, true);
            }

            // Deep Forest Canopy Boundary (North: y = 36 to 42)
            for (int x = -17; x <= 17; x += 2)
            {
                for (int y = 36; y <= 42; y += 2)
                {
                    CreateProp(propsRoot, "DeepForestTree", new Vector3(x, y, 0), treeVarSprite, true);
                }
            }

            // Forest Interior Tree Clusters & Rocks
            // Cluster West
            CreateProp(propsRoot, "ForestTree", new Vector3(-8, 16, 0), treeSprite, true);
            CreateProp(propsRoot, "ForestTree", new Vector3(-10, 18, 0), treeVarSprite, true);
            CreateProp(propsRoot, "ForestRock", new Vector3(-7, 19, 0), rockSprite, true);
            CreateProp(propsRoot, "ForestBush", new Vector3(-6, 15, 0), bushSprite, false);

            // Cluster East
            CreateProp(propsRoot, "ForestTree", new Vector3(8, 16, 0), treeVarSprite, true);
            CreateProp(propsRoot, "ForestTree", new Vector3(10, 17, 0), treeSprite, true);
            CreateProp(propsRoot, "ForestRock", new Vector3(6, 18, 0), rockSprite, true);
            CreateProp(propsRoot, "ForestBush", new Vector3(5, 15, 0), bushSprite, false);

            // Pond Area Surroundings (East Clearing)
            CreateProp(propsRoot, "PondTree", new Vector3(14, 25, 0), treeSprite, true);
            CreateProp(propsRoot, "PondRock", new Vector3(8, 24, 0), rockSprite, true);
            CreateProp(propsRoot, "PondFlower", new Vector3(7, 21, 0), flowerSprite, false);

            // Secluded Area Props (West Clearing)
            CreateProp(propsRoot, "SecludedTree", new Vector3(-14, 25, 0), treeVarSprite, true);
            CreateProp(propsRoot, "SecludedRock", new Vector3(-8, 24, 0), rockSprite, true);
            CreateProp(propsRoot, "SecludedBush", new Vector3(-11, 20, 0), bushSprite, false);

            // Path Surroundings along main forest path
            CreateProp(propsRoot, "PathTree", new Vector3(3, 29, 0), treeSprite, true);
            CreateProp(propsRoot, "PathTree", new Vector3(-4, 31, 0), treeVarSprite, true);
            CreateProp(propsRoot, "PathRock", new Vector3(-3, 28, 0), rockSprite, true);
            CreateProp(propsRoot, "PathFlower", new Vector3(2, 27, 0), flowerSprite, false);

            // 5. Build NPCs
            DialogueData oldKeeperDialogue = AssetDatabase.LoadAssetAtPath<DialogueData>("Assets/BeastKeeper/Data/Dialogue/OldKeeperIntroDialogue.asset");
            DialogueData shopDialogue = AssetDatabase.LoadAssetAtPath<DialogueData>("Assets/BeastKeeper/Data/Dialogue/ShopkeeperDialogue.asset");
            DialogueData v1Dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>("Assets/BeastKeeper/Data/Dialogue/Villager1Dialogue.asset");
            DialogueData v2Dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>("Assets/BeastKeeper/Data/Dialogue/Villager2Dialogue.asset");
            DialogueData v3Dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>("Assets/BeastKeeper/Data/Dialogue/Villager3Dialogue.asset");

            CreateNpc(npcsRoot, "Old Keeper", "old_keeper", new Vector3(6, 0, 0), oldKeeperSprite, oldKeeperDialogue);
            CreateNpc(npcsRoot, "Shopkeeper", "shopkeeper", new Vector3(-6, 0, 0), shopkeeperSprite, shopDialogue);
            CreateNpc(npcsRoot, "Villager Leo", "leo", new Vector3(-4, -5, 0), leoSprite, v1Dialogue);
            CreateNpc(npcsRoot, "Villager Mia", "mia", new Vector3(4, -5, 0), miaSprite, v2Dialogue);
            CreateNpc(npcsRoot, "Villager Bob", "bob", new Vector3(0, 4, 0), bobSprite, v3Dialogue);

            // 6. Triggers & Bounds
            MonsterData mossfangData = AssetDatabase.LoadAssetAtPath<MonsterData>("Assets/BeastKeeper/Data/Monsters/Mossfang.asset");
            
            // Encounter Zone positioned inside Forest path
            GameObject encObj = GameObject.Find("EncounterZone_Mossfang");
            if (encObj == null)
            {
                encObj = new GameObject("EncounterZone_Mossfang");
                encObj.transform.SetParent(triggersRoot.transform);
            }
            encObj.transform.position = new Vector3(0, 26, 0);
            BoxCollider2D encCol = encObj.GetComponent<BoxCollider2D>();
            if (encCol == null) encCol = encObj.AddComponent<BoxCollider2D>();
            encCol.isTrigger = true;
            encCol.size = new Vector2(8f, 6f);

            EncounterZone ez = encObj.GetComponent<EncounterZone>();
            if (ez == null) ez = encObj.AddComponent<EncounterZone>();
            ez.EncounterMonster = mossfangData;

            // Forest Area Bounds Trigger
            GameObject forestAreaObj = GameObject.Find("AreaBoundsTrigger_Forest");
            if (forestAreaObj == null)
            {
                forestAreaObj = new GameObject("AreaBoundsTrigger_Forest");
                forestAreaObj.transform.SetParent(triggersRoot.transform);
            }
            forestAreaObj.transform.position = new Vector3(0, 14, 0);
            BoxCollider2D forestCol = forestAreaObj.GetComponent<BoxCollider2D>();
            if (forestCol == null) forestCol = forestAreaObj.AddComponent<BoxCollider2D>();
            forestCol.isTrigger = true;
            forestCol.size = new Vector2(10f, 2f);

            AreaBoundsTrigger abtForest = forestAreaObj.GetComponent<AreaBoundsTrigger>();
            if (abtForest == null) abtForest = forestAreaObj.AddComponent<AreaBoundsTrigger>();
            abtForest.Configure("Forest", "forest", new Vector2(-20f, 12f), new Vector2(20f, 45f));

            // Village Area Bounds Trigger
            GameObject villageAreaObj = GameObject.Find("AreaBoundsTrigger_Village");
            if (villageAreaObj == null)
            {
                villageAreaObj = new GameObject("AreaBoundsTrigger_Village");
                villageAreaObj.transform.SetParent(triggersRoot.transform);
            }
            villageAreaObj.transform.position = new Vector3(0, 0, 0);
            BoxCollider2D villageCol = villageAreaObj.GetComponent<BoxCollider2D>();
            if (villageCol == null) villageCol = villageAreaObj.AddComponent<BoxCollider2D>();
            villageCol.isTrigger = true;
            villageCol.size = new Vector2(28f, 24f);

            AreaBoundsTrigger abtVillage = villageAreaObj.GetComponent<AreaBoundsTrigger>();
            if (abtVillage == null) abtVillage = villageAreaObj.AddComponent<AreaBoundsTrigger>();
            abtVillage.Configure("Village", "village", new Vector2(-18f, -18f), new Vector2(18f, 12f));

            // 7. Ensure Player setup
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0, -5, 0);
                SpriteRenderer psr = player.GetComponent<SpriteRenderer>();
                if (psr != null)
                {
                    psr.sortingOrder = 3;
                }
            }

            // 8. Ensure Main Camera follows Player and has initial Village bounds
            GameObject camObj = GameObject.FindWithTag("MainCamera");
            if (camObj != null && player != null)
            {
                CameraController cc = camObj.GetComponent<CameraController>();
                if (cc != null)
                {
                    cc.SetTarget(player.transform);
                    cc.SetBounds(new Vector2(-18f, -18f), new Vector2(18f, 12f));
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[VillageBuilderEditor] Exploration World (Village & Forest) constructed and saved successfully!");
        }

        private static void CreateBuilding(GameObject parent, string name, Vector3 pos, Sprite wall, Sprite roof, Sprite door, Sprite sign = null)
        {
            GameObject bldg = new GameObject(name);
            bldg.transform.SetParent(parent.transform);
            bldg.transform.position = pos;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 0; y++)
                {
                    GameObject w = new GameObject("Wall");
                    w.transform.SetParent(bldg.transform);
                    w.transform.position = pos + new Vector3(x, y, 0);
                    SpriteRenderer sr = w.AddComponent<SpriteRenderer>();
                    sr.sprite = (x == 0 && y == -1) ? door : wall;
                    sr.sortingOrder = 1;
                }
            }
            for (int x = -1; x <= 1; x++)
            {
                GameObject r = new GameObject("Roof");
                r.transform.SetParent(bldg.transform);
                r.transform.position = pos + new Vector3(x, 1, 0);
                SpriteRenderer sr = r.AddComponent<SpriteRenderer>();
                sr.sprite = roof;
                sr.sortingOrder = 2;
            }

            if (sign != null)
            {
                GameObject s = new GameObject("Sign");
                s.transform.SetParent(bldg.transform);
                s.transform.position = pos + new Vector3(1.5f, -1f, 0);
                SpriteRenderer sr = s.AddComponent<SpriteRenderer>();
                sr.sprite = sign;
                sr.sortingOrder = 2;
            }

            BoxCollider2D col = bldg.AddComponent<BoxCollider2D>();
            col.offset = new Vector2(0, 0);
            col.size = new Vector2(3.2f, 3.2f);
        }

        private static void CreateProp(GameObject parent, string name, Vector3 pos, Sprite sprite, bool addCollider)
        {
            if (sprite == null) return;
            GameObject prop = new GameObject(name);
            prop.transform.SetParent(parent.transform);
            prop.transform.position = pos;
            SpriteRenderer sr = prop.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            if (addCollider)
            {
                BoxCollider2D col = prop.AddComponent<BoxCollider2D>();
                col.size = new Vector2(0.8f, 0.8f);
            }
        }

        private static void CreateNpc(GameObject parent, string name, string id, Vector3 pos, Sprite sprite, DialogueData dialogue)
        {
            GameObject npc = GameObject.Find(name);
            if (npc == null)
            {
                npc = new GameObject(name);
            }
            npc.name = name;
            npc.layer = 8;
            npc.transform.SetParent(parent.transform);
            npc.transform.position = pos;

            SpriteRenderer sr = npc.GetComponent<SpriteRenderer>();
            if (sr == null) sr = npc.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;

            BoxCollider2D col = npc.GetComponent<BoxCollider2D>();
            if (col == null) col = npc.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.9f, 0.9f);

            InteractableNPC interactable = npc.GetComponent<InteractableNPC>();
            if (interactable == null) interactable = npc.AddComponent<InteractableNPC>();
            interactable.Configure(id, name, dialogue);
        }
    }
}
