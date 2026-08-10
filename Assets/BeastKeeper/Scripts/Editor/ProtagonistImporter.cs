using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace BeastKeeper.Editor
{
    public static class ProtagonistImporter
    {
        [MenuItem("Beast Keeper/Process Protagonist Sprite Sheet")]
        public static void Process()
        {
            Debug.Log("[ProtagonistImporter] Starting sprite sheet processing...");

            string srcPath = @"C:\Users\Asus\.gemini\antigravity-ide\brain\3624a803-2e9f-42db-a03a-1281ef103912\media__1786360750957.png";
            string destDir = "Assets/BeastKeeper/Art/Sprites/Characters/Player";
            string destPath = Path.Combine(destDir, "Protagonist_Sheet.png");

            if (!File.Exists(srcPath))
            {
                Debug.LogError($"[ProtagonistImporter] Source image not found at {srcPath}");
                return;
            }

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // 1. Load Source Image
            byte[] bytes = File.ReadAllBytes(srcPath);
            Texture2D srcTex = new Texture2D(2, 2);
            srcTex.LoadImage(bytes);

            int srcW = srcTex.width;
            int srcH = srcTex.height;
            int cols = 8;
            int rows = 4;
            int cellW = srcW / cols; // 128
            int cellH = srcH / rows; // 142

            // 2. Create Target Texture (512x256, 8 columns of 64px, 4 rows of 64px)
            int targetCellW = 64;
            int targetCellH = 64;
            Texture2D destTex = new Texture2D(cols * targetCellW, rows * targetCellH, TextureFormat.RGBA32, false);
            destTex.filterMode = FilterMode.Point;

            // Initialize with transparency
            Color[] transparentPixels = new Color[destTex.width * destTex.height];
            for (int i = 0; i < transparentPixels.Length; i++) transparentPixels[i] = new Color(0, 0, 0, 0);
            destTex.SetPixels(transparentPixels);

            // Scale configuration
            float scale = 0.40f; // Scale down character height of ~120px to ~48px
            int newFeetY = 8; // Feet placement inside 64px height frame

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // In Unity texture coordinates, y=0 is bottom
                    int texRow = rows - 1 - r;
                    int startX = c * cellW;
                    
                    // Row Heights in source: Row 0, 1, 2 have 143px height, Row 3 has 142px
                    int startY = 0;
                    int currentCellH = cellH;
                    if (texRow == 3) { startY = 428; currentCellH = 143; }
                    else if (texRow == 2) { startY = 285; currentCellH = 143; }
                    else if (texRow == 1) { startY = 142; currentCellH = 143; }
                    else { startY = 0; currentCellH = 142; }

                    // Find non-transparent bounds in this cell
                    int minX = startX + cellW;
                    int maxX = startX;
                    int minY = startY + currentCellH;
                    int maxY = startY;
                    bool hasPixels = false;

                    for (int y = startY; y < startY + currentCellH; y++)
                    {
                        if (y >= srcH) continue;
                        for (int x = startX; x < startX + cellW; x++)
                        {
                            if (x >= srcW) continue;
                            Color col = srcTex.GetPixel(x, y);
                            if (col.a > 0.05f)
                            {
                                hasPixels = true;
                                if (x < minX) minX = x;
                                if (x > maxX) maxX = x;
                                if (y < minY) minY = y;
                                if (y > maxY) maxY = y;
                            }
                        }
                    }

                    if (!hasPixels) continue;

                    // Centering math
                    float centerX_src = (minX + maxX) / 2f;
                    float feetY_src = minY;

                    // Map pixels from target 64x64 cell back to source cell
                    int destStartX = c * targetCellW;
                    int destStartY = (rows - 1 - r) * targetCellH; // Align matching rows correctly

                    for (int ty = 0; ty < targetCellH; ty++)
                    {
                        for (int tx = 0; tx < targetCellW; tx++)
                        {
                            float srcX = centerX_src + (tx - targetCellW / 2f) / scale;
                            float srcY = feetY_src + (ty - newFeetY) / scale;

                            int sx = Mathf.RoundToInt(srcX);
                            int sy = Mathf.RoundToInt(srcY);

                            if (sx >= startX && sx < startX + cellW && sy >= startY && sy < startY + currentCellH)
                            {
                                destTex.SetPixel(destStartX + tx, destStartY + ty, srcTex.GetPixel(sx, sy));
                            }
                        }
                    }
                }
            }

            destTex.Apply();

            // 3. Save PNG to disk
            byte[] destBytes = destTex.EncodeToPNG();
            File.WriteAllBytes(destPath, destBytes);
            AssetDatabase.ImportAsset(destPath);

            // 4. Configure TextureImporter for Multiple Sprites
            TextureImporter importer = AssetImporter.GetAtPath(destPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit = 32;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.compressionQuality = 0;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.wrapMode = TextureWrapMode.Clamp;
                importer.SetTextureSettings(settings);

                // Define metadata for all 32 frames
                List<SpriteMetaData> metas = new List<SpriteMetaData>();
                string[] directions = new string[] { "Down", "Left", "Right", "Up" };

                for (int r = 0; r < rows; r++)
                {
                    string dir = directions[r];
                    for (int c = 0; c < cols; c++)
                    {
                        SpriteMetaData meta = new SpriteMetaData();
                        meta.rect = new Rect(c * targetCellW, (rows - 1 - r) * targetCellH, targetCellW, targetCellH);
                        meta.alignment = (int)SpriteAlignment.Center;
                        meta.name = $"Player_{dir}_{c}";
                        metas.Add(meta);
                    }
                }

                importer.spritesheet = metas.ToArray();
                importer.SaveAndReimport();
            }

            Debug.Log("[ProtagonistImporter] Sprite sheet successfully sliced.");

            // 5. Generate Animation Clips & Animator Controller
            GenerateAnimationsAndController(destPath);
        }

        private static void GenerateAnimationsAndController(string spriteSheetPath)
        {
            string animDir = "Assets/BeastKeeper/Art/Animations/Player";
            if (!Directory.Exists(animDir))
            {
                Directory.CreateDirectory(animDir);
            }

            // Load sliced sprites from the asset database
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath);
            Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();

            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    spriteDict[sprite.name] = sprite;
                }
            }

            string[] directions = new string[] { "Down", "Left", "Right", "Up" };
            Dictionary<string, AnimationClip> walkClips = new Dictionary<string, AnimationClip>();
            Dictionary<string, AnimationClip> idleClips = new Dictionary<string, AnimationClip>();

            // Generate Clips
            foreach (var dir in directions)
            {
                // Walk Animation Clip (8 frames, loops)
                string walkClipPath = Path.Combine(animDir, $"Player_Walk_{dir}.anim");
                AnimationClip walkClip = new AnimationClip();
                walkClip.frameRate = 10; // 10 FPS is clean for pixel walk
                
                var settings = AnimationUtility.GetAnimationClipSettings(walkClip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(walkClip, settings);

                EditorCurveBinding binding = new EditorCurveBinding();
                binding.type = typeof(SpriteRenderer);
                binding.path = "";
                binding.propertyName = "m_Sprite";

                ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[8];
                for (int i = 0; i < 8; i++)
                {
                    string spriteName = $"Player_{dir}_{i}";
                    if (spriteDict.TryGetValue(spriteName, out Sprite sp))
                    {
                        keyframes[i] = new ObjectReferenceKeyframe();
                        keyframes[i].time = i / 10f;
                        keyframes[i].value = sp;
                    }
                    else
                    {
                        Debug.LogError($"[ProtagonistImporter] Sliced sprite {spriteName} not found!");
                    }
                }
                AnimationUtility.SetObjectReferenceCurve(walkClip, binding, keyframes);
                AssetDatabase.CreateAsset(walkClip, walkClipPath);
                walkClips[dir] = walkClip;

                // Idle Animation Clip (1 frame, loops)
                string idleClipPath = Path.Combine(animDir, $"Player_Idle_{dir}.anim");
                AnimationClip idleClip = new AnimationClip();
                idleClip.frameRate = 1;
                
                var idleSettings = AnimationUtility.GetAnimationClipSettings(idleClip);
                idleSettings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(idleClip, idleSettings);

                ObjectReferenceKeyframe[] idleKeyframes = new ObjectReferenceKeyframe[1];
                string idleSpriteName = $"Player_{dir}_0"; // Frame 0 is neutral standing pose
                if (spriteDict.TryGetValue(idleSpriteName, out Sprite idleSp))
                {
                    idleKeyframes[0] = new ObjectReferenceKeyframe();
                    idleKeyframes[0].time = 0f;
                    idleKeyframes[0].value = idleSp;
                }
                AnimationUtility.SetObjectReferenceCurve(idleClip, binding, idleKeyframes);
                AssetDatabase.CreateAsset(idleClip, idleClipPath);
                idleClips[dir] = idleClip;
            }

            // Create Animator Controller
            string controllerPath = Path.Combine(animDir, "PlayerAnimatorController.controller");
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var rootStateMachine = controller.layers[0].stateMachine;

            // Add Animation States
            foreach (var dir in directions)
            {
                var idleState = rootStateMachine.AddState($"Idle_{dir}");
                idleState.motion = idleClips[dir];

                var walkState = rootStateMachine.AddState($"Walk_{dir}");
                walkState.motion = walkClips[dir];
            }

            Debug.Log($"[ProtagonistImporter] Animations and Controller successfully created at {animDir}.");
        }
    }
}
