using System.IO;
using UnityEditor;
using UnityEngine;

namespace BeastKeeper.Editor
{
    public static class ProceduralPixelArtGenerator
    {
        private const string BasePath = "Assets/BeastKeeper/Art/Sprites";

        [MenuItem("Beast Keeper/Generate Pixel Art Assets")]
        public static void GenerateAllAssets()
        {
            EnsureDirectories();

            // Ground & Paths (32x32)
            GenerateSprite("Tiles/VillageGrass.png", CreateGrassTexture(new Color32(46, 125, 50, 255), new Color32(56, 142, 60, 255), new Color32(27, 94, 32, 255)));
            GenerateSprite("Tiles/VillageGrassVariant.png", CreateGrassTexture(new Color32(56, 142, 60, 255), new Color32(76, 175, 80, 255), new Color32(46, 125, 50, 255)));
            GenerateSprite("Tiles/DirtPath.png", CreateDirtTexture(new Color32(141, 110, 99, 255), new Color32(109, 76, 65, 255), new Color32(161, 136, 127, 255)));
            GenerateSprite("Tiles/DirtPathVariant.png", CreateDirtTexture(new Color32(121, 85, 72, 255), new Color32(93, 64, 55, 255), new Color32(141, 110, 99, 255)));

            // Props & Buildings (32x32 / 64x64)
            GenerateSprite("Props/Tree.png", CreateTreeTexture(false));
            GenerateSprite("Props/TreeVariant.png", CreateTreeTexture(true));
            GenerateSprite("Props/Bush.png", CreateBushTexture());
            GenerateSprite("Props/Flower.png", CreateFlowerTexture());
            GenerateSprite("Props/Rock.png", CreateRockTexture());
            GenerateSprite("Props/Well.png", CreateWellTexture());
            GenerateSprite("Props/Fence.png", CreateFenceTexture());
            GenerateSprite("Props/Sign.png", CreateSignTexture());

            GenerateSprite("Buildings/HouseWall.png", CreateHouseWallTexture());
            GenerateSprite("Buildings/HouseRoof.png", CreateHouseRoofTexture());
            GenerateSprite("Buildings/HouseDoor.png", CreateHouseDoorTexture());

            // NPCs (32x32)
            GenerateSprite("Characters/NPC_OldKeeper.png", CreateNpcTexture(new Color32(33, 150, 243, 255), new Color32(255, 255, 255, 255), true));
            GenerateSprite("Characters/NPC_Shopkeeper.png", CreateNpcTexture(new Color32(76, 175, 80, 255), new Color32(255, 193, 7, 255), false));
            GenerateSprite("Characters/NPC_Leo.png", CreateNpcTexture(new Color32(121, 85, 72, 255), new Color32(141, 110, 99, 255), false));
            GenerateSprite("Characters/NPC_Mia.png", CreateNpcTexture(new Color32(156, 39, 176, 255), new Color32(233, 30, 99, 255), false));
            GenerateSprite("Characters/NPC_Bob.png", CreateNpcTexture(new Color32(244, 67, 54, 255), new Color32(33, 33, 33, 255), false));

            // Monsters & Battle (64x64)
            GenerateSprite("Monsters/Mossfang_Overworld.png", CreateMossfangOverworldTexture());
            GenerateSprite("Monsters/Mossfang_Battle.png", CreateMossfangBattleTexture());
            GenerateSprite("Monsters/Leafling_Battle.png", CreateLeaflingBattleTexture());
            GenerateSprite("Battle/BattleBackground_Forest.png", CreateForestBattleBackgroundTexture());

            AssetDatabase.Refresh();
            Debug.Log("[ProceduralPixelArtGenerator] All pixel art assets generated successfully!");
        }

        private static void EnsureDirectories()
        {
            Directory.CreateDirectory($"{BasePath}/Tiles");
            Directory.CreateDirectory($"{BasePath}/Props");
            Directory.CreateDirectory($"{BasePath}/Buildings");
            Directory.CreateDirectory($"{BasePath}/Characters");
            Directory.CreateDirectory($"{BasePath}/Monsters");
            Directory.CreateDirectory($"{BasePath}/Battle");
        }

        private static void GenerateSprite(string relativePath, Texture2D tex)
        {
            string fullPath = $"{BasePath}/{relativePath}";
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit = 32;
                importer.SaveAndReimport();
            }
        }

        private static Texture2D CreateGrassTexture(Color baseColor, Color highlight, Color shadow)
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color c = baseColor;
                    if ((x * 7 + y * 13) % 11 == 0) c = highlight;
                    else if ((x * 17 + y * 3) % 13 == 0) c = shadow;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateDirtTexture(Color baseColor, Color shadow, Color highlight)
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color c = baseColor;
                    if ((x * 5 + y * 11) % 9 == 0) c = shadow;
                    else if ((x * 13 + y * 7) % 15 == 0) c = highlight;
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateTreeTexture(bool variant)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color trunk = new Color32(109, 76, 65, 255);
            Color trunkShadow = new Color32(78, 52, 46, 255);
            Color leafBase = variant ? new Color32(46, 125, 50, 255) : new Color32(56, 142, 60, 255);
            Color leafHighlight = variant ? new Color32(129, 199, 132, 255) : new Color32(165, 214, 167, 255);
            Color leafShadow = variant ? new Color32(27, 94, 32, 255) : new Color32(46, 125, 50, 255);

            // Trunk (bottom center)
            for (int y = 4; y < 24; y++)
            {
                for (int x = 26; x <= 37; x++)
                {
                    tex.SetPixel(x, y, x < 32 ? trunkShadow : trunk);
                }
            }

            // Canopy (circles/blobs)
            Vector2 center = new Vector2(32, 40);
            float radius = 22f;
            for (int y = 16; y < 60; y++)
            {
                for (int x = 10; x < 54; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        Color c = leafBase;
                        if (y > center.y + 6 && dist < radius - 3) c = leafHighlight;
                        else if (y < center.y - 6 || dist > radius - 3) c = leafShadow;
                        tex.SetPixel(x, y, c);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateBushTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color baseGreen = new Color32(46, 125, 50, 255);
            Color highlight = new Color32(129, 199, 132, 255);
            Color shadow = new Color32(27, 94, 32, 255);

            Vector2 center = new Vector2(16, 14);
            float radius = 12f;
            for (int y = 2; y < 28; y++)
            {
                for (int x = 4; x < 28; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        Color c = baseGreen;
                        if (y > 16) c = highlight;
                        else if (y < 8 || dist > radius - 2) c = shadow;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateFlowerTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color petalRed = new Color32(244, 67, 54, 255);
            Color petalYellow = new Color32(255, 235, 59, 255);
            Color centerYellow = new Color32(255, 193, 7, 255);
            Color stem = new Color32(76, 175, 80, 255);

            // Stems
            for (int y = 2; y < 14; y++)
            {
                tex.SetPixel(10, y, stem);
                tex.SetPixel(22, y, stem);
            }

            // Flower 1
            tex.SetPixel(10, 14, centerYellow);
            tex.SetPixel(9, 14, petalRed); tex.SetPixel(11, 14, petalRed);
            tex.SetPixel(10, 13, petalRed); tex.SetPixel(10, 15, petalRed);

            // Flower 2
            tex.SetPixel(22, 14, centerYellow);
            tex.SetPixel(21, 14, petalYellow); tex.SetPixel(23, 14, petalYellow);
            tex.SetPixel(22, 13, petalYellow); tex.SetPixel(22, 15, petalYellow);

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateRockTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color rockBase = new Color32(158, 158, 158, 255);
            Color rockLight = new Color32(224, 224, 224, 255);
            Color rockDark = new Color32(97, 97, 97, 255);

            for (int y = 6; y <= 24; y++)
            {
                for (int x = 6; x <= 26; x++)
                {
                    if ((x - 16) * (x - 16) / 100.0f + (y - 15) * (y - 15) / 81.0f <= 1.0f)
                    {
                        Color c = rockBase;
                        if (y > 18) c = rockLight;
                        else if (y < 10) c = rockDark;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateWellTexture()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color stone = new Color32(117, 117, 117, 255);
            Color stoneDark = new Color32(66, 66, 66, 255);
            Color wood = new Color32(141, 110, 99, 255);
            Color roof = new Color32(183, 28, 28, 255);
            Color water = new Color32(33, 150, 243, 255);

            // Roof
            for (int y = 44; y < 60; y++)
            {
                for (int x = 12; x < 52; x++)
                {
                    tex.SetPixel(x, y, roof);
                }
            }
            // Wooden posts
            for (int y = 24; y < 44; y++)
            {
                tex.SetPixel(16, y, wood); tex.SetPixel(17, y, wood);
                tex.SetPixel(46, y, wood); tex.SetPixel(47, y, wood);
            }
            // Circular Well Base
            for (int y = 4; y < 24; y++)
            {
                for (int x = 12; x < 52; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(32, 14));
                    if (dist <= 18)
                    {
                        if (dist <= 12 && y > 10) tex.SetPixel(x, y, water);
                        else tex.SetPixel(x, y, (x % 6 == 0 || y % 6 == 0) ? stoneDark : stone);
                    }
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateFenceTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color wood = new Color32(141, 110, 99, 255);
            Color woodDark = new Color32(93, 64, 55, 255);

            // Horizontal rails
            for (int x = 0; x < 32; x++)
            {
                for (int y = 8; y <= 11; y++) tex.SetPixel(x, y, wood);
                for (int y = 20; y <= 23; y++) tex.SetPixel(x, y, wood);
            }
            // Vertical posts
            for (int y = 2; y < 30; y++)
            {
                for (int x = 4; x <= 8; x++) tex.SetPixel(x, y, x == 8 ? woodDark : wood);
                for (int x = 23; x <= 27; x++) tex.SetPixel(x, y, x == 27 ? woodDark : wood);
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSignTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color wood = new Color32(161, 136, 127, 255);
            Color post = new Color32(109, 76, 65, 255);

            // Post
            for (int y = 2; y < 18; y++)
            {
                tex.SetPixel(15, y, post);
                tex.SetPixel(16, y, post);
            }
            // Sign Board
            for (int y = 14; y < 28; y++)
            {
                for (int x = 6; x <= 25; x++)
                {
                    tex.SetPixel(x, y, wood);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateHouseWallTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color wallBase = new Color32(238, 238, 238, 255);
            Color line = new Color32(189, 189, 189, 255);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, (x % 8 == 0 || y % 8 == 0) ? line : wallBase);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateHouseRoofTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color roofRed = new Color32(198, 40, 40, 255);
            Color line = new Color32(136, 14, 79, 255);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, (y % 4 == 0) ? line : roofRed);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateHouseDoorTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color wood = new Color32(121, 85, 72, 255);
            Color knob = new Color32(255, 215, 0, 255);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, wood);
                }
            }
            tex.SetPixel(24, 16, knob);
            tex.SetPixel(24, 17, knob);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateNpcTexture(Color robeColor, Color hairColor, bool isOldKeeper)
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color skin = new Color32(255, 204, 153, 255);

            // Head (y: 18 to 26, x: 11 to 20)
            for (int y = 18; y <= 26; y++)
            {
                for (int x = 11; x <= 20; x++)
                {
                    tex.SetPixel(x, y, skin);
                }
            }
            // Hair / Beard
            for (int x = 10; x <= 21; x++)
            {
                tex.SetPixel(x, 27, hairColor);
                tex.SetPixel(x, 28, hairColor);
            }
            if (isOldKeeper)
            {
                // Beard
                for (int y = 14; y <= 18; y++)
                {
                    for (int x = 11; x <= 20; x++)
                    {
                        tex.SetPixel(x, y, hairColor);
                    }
                }
            }
            // Eyes
            tex.SetPixel(13, 22, Color.black);
            tex.SetPixel(18, 22, Color.black);

            // Body / Robe (y: 2 to 17, x: 8 to 23)
            for (int y = 2; y <= 17; y++)
            {
                for (int x = 8; x <= 23; x++)
                {
                    tex.SetPixel(x, y, robeColor);
                }
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateMossfangOverworldTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color mossGreen = new Color32(46, 125, 50, 255);
            Color darkGreen = new Color32(27, 94, 32, 255);
            Color eyes = new Color32(255, 235, 59, 255);
            Color fangs = Color.white;

            // Body
            for (int y = 6; y <= 22; y++)
            {
                for (int x = 6; x <= 25; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 14));
                    if (dist <= 10)
                    {
                        tex.SetPixel(x, y, dist > 8 ? darkGreen : mossGreen);
                    }
                }
            }
            // Glowing eyes
            tex.SetPixel(12, 16, eyes); tex.SetPixel(13, 16, eyes);
            tex.SetPixel(19, 16, eyes); tex.SetPixel(20, 16, eyes);
            // Fangs
            tex.SetPixel(14, 12, fangs); tex.SetPixel(18, 12, fangs);

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateMossfangBattleTexture()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color mossGreen = new Color32(46, 125, 50, 255);
            Color highlight = new Color32(102, 187, 106, 255);
            Color darkGreen = new Color32(27, 94, 32, 255);
            Color eyeYellow = new Color32(255, 235, 59, 255);
            Color eyeRed = new Color32(244, 67, 54, 255);
            Color clawCol = new Color32(220, 220, 220, 255);

            // Large Beast Silhouette
            Vector2 bodyCenter = new Vector2(32, 28);
            for (int y = 8; y <= 52; y++)
            {
                for (int x = 8; x <= 56; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), bodyCenter);
                    if (dist <= 22)
                    {
                        Color c = mossGreen;
                        if (y > 34) c = highlight;
                        else if (y < 20 || dist > 18) c = darkGreen;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            // Horns / Ears
            for (int y = 45; y <= 58; y++)
            {
                tex.SetPixel(20, y, darkGreen); tex.SetPixel(21, y, highlight);
                tex.SetPixel(42, y, darkGreen); tex.SetPixel(43, y, highlight);
            }
            // Threatening Eyes
            for (int y = 32; y <= 35; y++)
            {
                for (int x = 22; x <= 26; x++) tex.SetPixel(x, y, eyeRed);
                for (int x = 37; x <= 41; x++) tex.SetPixel(x, y, eyeRed);
            }
            tex.SetPixel(24, 33, eyeYellow);
            tex.SetPixel(39, 33, eyeYellow);
            // Sharp Claws/Fangs
            for (int y = 14; y <= 20; y++)
            {
                tex.SetPixel(26, y, clawCol); tex.SetPixel(37, y, clawCol);
            }

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateLeaflingBattleTexture()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Color bodyLight = new Color32(165, 214, 167, 255);
            Color bodyBase = new Color32(76, 175, 80, 255);
            Color bellyCol = new Color32(255, 245, 157, 255);
            Color eyeBlue = new Color32(33, 150, 243, 255);

            // Starter Beast Body (Cute Leaf Creature)
            Vector2 bodyCenter = new Vector2(32, 28);
            for (int y = 8; y <= 48; y++)
            {
                for (int x = 12; x <= 52; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), bodyCenter);
                    if (dist <= 18)
                    {
                        Color c = bodyBase;
                        if (x > 24 && x < 40 && y < 30) c = bellyCol;
                        else if (y > 32) c = bodyLight;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            // Leaf Sprout on Head
            for (int y = 46; y <= 58; y++)
            {
                tex.SetPixel(31, y, bodyBase); tex.SetPixel(32, y, bodyLight);
            }

            // Friendly Large Eyes
            for (int y = 30; y <= 36; y++)
            {
                for (int x = 22; x <= 26; x++) tex.SetPixel(x, y, eyeBlue);
                for (int x = 38; x <= 42; x++) tex.SetPixel(x, y, eyeBlue);
            }
            tex.SetPixel(24, 34, Color.white);
            tex.SetPixel(40, 34, Color.white);

            tex.Apply();
            return tex;
        }

        private static Texture2D CreateForestBattleBackgroundTexture()
        {
            int width = 320;
            int height = 180;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color skyTop = new Color32(144, 202, 249, 255);
            Color skyBottom = new Color32(227, 242, 253, 255);
            Color forestFar = new Color32(46, 125, 50, 255);
            Color groundTop = new Color32(129, 199, 132, 255);
            Color groundBottom = new Color32(56, 142, 60, 255);
            Color arenaGround = new Color32(141, 110, 99, 255);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c;
                    if (y > 100)
                    {
                        // Sky
                        float t = (y - 100) / 80f;
                        c = Color.Lerp(skyBottom, skyTop, t);
                    }
                    else if (y > 70)
                    {
                        // Far forest line
                        c = forestFar;
                    }
                    else
                    {
                        // Battle arena ground
                        float t = y / 70f;
                        c = Color.Lerp(groundBottom, groundTop, t);
                        // Battle platforms/rings
                        float distEnemy = Vector2.Distance(new Vector2(x, y), new Vector2(230, 45));
                        float distPlayer = Vector2.Distance(new Vector2(x, y), new Vector2(90, 25));
                        if (distEnemy <= 35 || distPlayer <= 35)
                        {
                            c = arenaGround;
                        }
                    }
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        private static void ClearTexture(Texture2D tex)
        {
            Color clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, clear);
                }
            }
        }
    }
}
