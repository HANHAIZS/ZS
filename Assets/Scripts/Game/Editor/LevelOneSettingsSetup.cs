using System;
using System.Linq;
using GameFoundation.UI;
using GameFoundation.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace BeeHarvest.Editor
{
    public static class LevelOneSettingsSetup
    {
        public const string ArtRoot = "Assets/Sprites/UI/Settings/";
        public const string PrefabPath = "Assets/Sprites/Prefabs/UI/SettingsPopup.prefab";
        public const string FontPath = "Assets/Sprites/UI/Fonts/FredokaBoldSDF.asset";
        const string ProfilePath = "Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset";
        static Color cream = new Color(1, .965f, .898f, 1);
        static Color brown = new Color(.353f, .227f, .094f, 1);

        /// <summary>Adds only the settings controls, preserving the existing first-level scene.</summary>
        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before settings setup.");
            var level = UnityEngine.Object.FindObjectOfType<LevelOneController>();
            if (level == null || level.Hud == null) throw new InvalidOperationException("Level01 must be open.");
            var oldRestart = level.Hud.transform.Find("RestartLevel");
            if (oldRestart != null) UnityEngine.Object.DestroyImmediate(oldRestart.gameObject);
            level.Hud.Restart = null;
            EditorUtility.SetDirty(level.Hud);
            Ensure("Assets/Sprites/Prefabs/UI");
            var font = Font();
            var profile = AssetDatabase.LoadAssetAtPath<UiPrefabConventionProfile>(ProfilePath);
            if (profile == null) throw new InvalidOperationException("Project UI convention profile is missing.");
            var panel = Art("SettingsPanel", 1024, true);
            var gear = OkaUiRefreshSetup.Sprite("SettingsGear");
            var icons = new[] { Art("MusicIcon", 256, false), Art("SoundIcon", 256, false), Art("HapticIcon", 256, false) };
            var retry = Art("RetryButton", 512, false);
            var home = Art("HomeButton", 512, false);

            var root = Rect("SettingsPopup", null, Vector2.zero, Vector2.zero);
            Stretch(root);
            var controller = root.gameObject.AddComponent<SettingsPopupController>();
            var dim = Rect("DimBackground", root, Vector2.zero, Vector2.zero);
            Stretch(dim);
            var dimImage = dim.gameObject.AddComponent<Image>();
            dimImage.color = new Color(0, 0, 0, profile.DimBackgroundAlpha);
            dimImage.raycastTarget = true;
            var content = Rect("PopupContent", root, new Vector2(620, 580), new Vector2(0, 45));
            var background = content.gameObject.AddComponent<Image>();
            background.sprite = panel;
            background.type = Image.Type.Sliced;
            background.pixelsPerUnitMultiplier = 2;
            background.raycastTarget = true;
            Label(content, "Title", "setting", font, new Vector2(440, 92), new Vector2(0, 231), 66);

            var circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (circle == null) throw new InvalidOperationException("Unity built-in circle sprite is missing.");
            var close = Button(content, "CloseButton", circle, new Vector2(80, 80), new Vector2(266, 244));
            close.image.color = cream;
            var closeFace = Rect("RedFace", close.transform, new Vector2(68, 68), Vector2.zero);
            var face = closeFace.gameObject.AddComponent<Image>();
            face.sprite = circle;
            face.color = new Color(.92f, .16f, .20f);
            face.raycastTarget = false;
            Label(closeFace, "Label", "×", font, new Vector2(62, 62), new Vector2(0, 2), 54);
            controller.CloseButton = close;

            controller.PreferenceButtons = new Button[3];
            controller.PreferenceIcons = new Image[3];
            controller.OffMarks = new GameObject[3];
            string[] names = { "Music", "Sound", "Haptic" };
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 178;
                var button = Button(content, names[i] + "Toggle", icons[i], new Vector2(142, 142), new Vector2(x, 36));
                controller.PreferenceButtons[i] = button;
                controller.PreferenceIcons[i] = button.image;
                Label(button.transform, "Label", names[i], font, new Vector2(160, 48), new Vector2(0, 105), 34);
                var mark = Rect("OffMark", button.transform, new Vector2(120, 14), Vector2.zero);
                mark.localRotation = Quaternion.Euler(0, 0, -45);
                var markImage = mark.gameObject.AddComponent<Image>();
                markImage.color = new Color(.95f, .18f, .20f);
                markImage.raycastTarget = false;
                controller.OffMarks[i] = mark.gameObject;
                mark.gameObject.SetActive(false);
            }
            controller.RetryButton = Button(content, "RetryButton", retry, new Vector2(242, 87), new Vector2(-130, -167));
            Label(controller.RetryButton.transform, "Label", "Retry", font, new Vector2(215, 76), Vector2.zero, 43);
            controller.HomeButton = Button(content, "HomeButton", home, new Vector2(242, 87), new Vector2(130, -167));
            Label(controller.HomeButton.transform, "Label", "Home", font, new Vector2(215, 76), Vector2.zero, 43);
            var homeColors = controller.HomeButton.colors;
            homeColors.disabledColor = new Color(.62f, .62f, .62f, 1);
            controller.HomeButton.colors = homeColors;
            controller.HomeButton.interactable = false;
            dim.gameObject.SetActive(false);
            content.gameObject.SetActive(false);
            controller.DimBackground = dim.gameObject;
            controller.PopupContent = content.gameObject;
            OkaSettingsSetup.ImportArt();
            SettingsPopupStyleSetup.Build(controller,false);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root.gameObject);

            var canvas = level.Hud.GetComponentInParent<Canvas>();
            var old = canvas.transform.Find("SettingsPopup");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var oldGear = level.Hud.transform.Find("SettingsButton");
            if (oldGear != null) UnityEngine.Object.DestroyImmediate(oldGear.gameObject);
            var gearButton = Button(level.Hud.transform, "SettingsButton", gear, new Vector2(80, 80), new Vector2(-60, -60));
            var gearRect = (RectTransform)gearButton.transform;
            gearRect.anchorMin = gearRect.anchorMax = Vector2.one;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
            var view = instance.GetComponent<SettingsPopupController>();
            view.Level = level;
            view.OpenButton = gearButton;
            PrefabUtility.RecordPrefabInstancePropertyModifications(view);
            EditorUtility.SetDirty(view);
            instance.transform.SetAsLastSibling();
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(level.gameObject.scene);
            var report = UiPrefabValidator.Validate(PrefabPath, profile, UiPrefabKind.Popup);
            if (!report.IsValid) throw new InvalidOperationException(report.ToMultilineString());
            Debug.Log("SETTINGS_POPUP_READY: project popup validator passed.");
        }

        /// <summary>Imports transparent sprites and crops only their import rectangle; source pixels stay intact.</summary>
        static Sprite Art(string name, int maximum, bool sliced)
        {
            string path = ArtRoot + name + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Settings art missing: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.maxTextureSize = 8192;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var pixels = texture.GetPixels32();
            int x0 = texture.width, y0 = texture.height, x1 = 0, y1 = 0;
            for (int y = 0; y < texture.height; y++) for (int x = 0; x < texture.width; x++)
                if (pixels[y * texture.width + x].a > 64) { x0 = Mathf.Min(x0, x); y0 = Mathf.Min(y0, y); x1 = Mathf.Max(x1, x); y1 = Mathf.Max(y1, y); }
            if (x1 <= x0 || y1 <= y0) throw new InvalidOperationException("Settings sprite is empty: " + name);
            int pad = Mathf.CeilToInt(Mathf.Min(x1 - x0, y1 - y0) * .025f);
            x0 = Mathf.Max(0, x0 - pad); y0 = Mathf.Max(0, y0 - pad);
            x1 = Mathf.Min(texture.width, x1 + pad + 1); y1 = Mathf.Min(texture.height, y1 + pad + 1);
            var rect = new Rect(x0, y0, x1 - x0, y1 - y0);
            var border = sliced ? new Vector4(rect.width * .16f, rect.height * .16f, rect.width * .16f, rect.height * .23f) : Vector4.zero;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            // Unity 2022.3 project has no 2D Sprite provider package; its legacy importer still serializes these rectangles.
#pragma warning disable 0618
            importer.spritesheet = new[] { new SpriteMetaData { name = name, rect = rect, alignment = (int)SpriteAlignment.Center, pivot = Vector2.one * .5f, border = border } };
#pragma warning restore 0618
            importer.isReadable = false;
            importer.maxTextureSize = maximum;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single();
        }

        static TMP_FontAsset Font()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (asset == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/Sprites/UI/Fonts/Fredoka-Bold.ttf");
                if (source == null) throw new InvalidOperationException("Fredoka Bold source font is missing.");
                asset = TMP_FontAsset.CreateFontAsset(source, 90, 12, GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic, false);
                if (asset == null || !asset.TryAddCharacters("settingMusicSoundHapticRetryHome×_ ")) throw new InvalidOperationException("Settings glyphs failed to bake.");
                asset.name = "FredokaBoldSDF";
                asset.atlasPopulationMode = AtlasPopulationMode.Static;
                AssetDatabase.CreateAsset(asset, FontPath);
                asset.material.name = "FredokaBoldSDF Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
                foreach (var atlas in asset.atlasTextures) { atlas.name = "FredokaBoldSDF Atlas"; AssetDatabase.AddObjectToAsset(atlas, asset); }
            }
            if (!asset.characterLookupTable.ContainsKey('_'))
            {
                asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                if (!asset.TryAddCharacters("_ ")) throw new InvalidOperationException("TMP underline glyph failed to bake.");
                asset.atlasPopulationMode = AtlasPopulationMode.Static;
            }
            asset.material.SetColor("_OutlineColor", brown);
            asset.material.SetColor("_FaceColor", Color.white);
            asset.material.SetFloat("_OutlineWidth", .18f);
            asset.material.EnableKeyword("OUTLINE_ON");
            EditorUtility.SetDirty(asset);
            EditorUtility.SetDirty(asset.material);
            return asset;
        }

        static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = Vector2.one * .5f;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }
        static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        static Button Button(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 position)
        {
            var rect = Rect(name, parent, size, position);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.pressedColor = new Color(.82f, .82f, .82f, 1);
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            return button;
        }
        static void Label(Transform parent, string name, string text, TMP_FontAsset font, Vector2 size, Vector2 position, float pointSize)
        {
            var label = Rect(name, parent, size, position).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSharedMaterial = font.material;
            label.text = text;
            label.fontSize = pointSize;
            label.fontStyle = FontStyles.Normal;
            label.alignment = TextAlignmentOptions.Center;
            label.color = cream;
            label.enableAutoSizing = false;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            label.UpdateMeshPadding();
        }
        static void Ensure(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            Ensure(path.Substring(0, slash));
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
