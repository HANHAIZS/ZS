using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace BeeHarvest.Editor
{
    public static class LevelOneTitleSetup
    {
        public const string FontPath = "Assets/Sprites/UI/Fonts/Coiny-Regular.ttf";
        public const string FontAssetPath = "Assets/Sprites/UI/Fonts/CoinyRoundedSDF.asset";

        /// <summary>Applies the specified title and removes only the three auxiliary labels.</summary>
        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before configuring the title.");
            var hud = UnityEngine.Object.FindObjectOfType<LevelOneHud>();
            if (hud == null) throw new InvalidOperationException("LevelOneHud was not found in the active scene.");
            var label = hud.transform.Find("LevelLabel");
            if (label == null) throw new InvalidOperationException("LevelLabel was not found.");
            var fontAsset = LoadFontAsset();
            Color face, outline;
            ColorUtility.TryParseHtmlString("#FFF6E5", out face);
            ColorUtility.TryParseHtmlString("#5A3A18", out outline);
            fontAsset.material.SetColor("_FaceColor", Color.white);
            fontAsset.material.SetColor("_OutlineColor", outline);
            fontAsset.material.SetFloat("_OutlineWidth", .2f);
            fontAsset.material.EnableKeyword("OUTLINE_ON");
            EditorUtility.SetDirty(fontAsset.material);

            var oldText = label.GetComponent<Text>();
            if (oldText != null) UnityEngine.Object.DestroyImmediate(oldText);
            var title = label.GetComponent<TextMeshProUGUI>();
            if (title == null) title = label.gameObject.AddComponent<TextMeshProUGUI>();
            title.text = "Level 1";
            title.font = fontAsset;
            title.fontSharedMaterial = fontAsset.material;
            title.fontSize = 36f * 1.15f;
            title.enableAutoSizing = false;
            title.fontStyle = FontStyles.Normal;
            title.alignment = TextAlignmentOptions.Center;
            title.color = face;
            title.raycastTarget = false;
            title.enableWordWrapping = false;
            title.UpdateMeshPadding();
            title.ForceMeshUpdate();
            EditorUtility.SetDirty(title);
            foreach (var name in new[] { "CollectedCount", "Instruction", "SlotTitle" })
            {
                var item = hud.transform.Find(name);
                if (item != null) UnityEngine.Object.DestroyImmediate(item.gameObject);
            }
            EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Bakes the title glyphs once, retaining the material and atlas as font subassets.</summary>
        static TMP_FontAsset LoadFontAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (asset != null) return asset;
            var source = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (source == null) throw new InvalidOperationException("Coiny source font is missing.");
            if (Shader.Find("TextMeshPro/Mobile/Distance Field") == null)
                throw new InvalidOperationException("TMP Essential Resources are missing.");
            asset = TMP_FontAsset.CreateFontAsset(source, 90, 12, GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic, false);
            if (asset == null || !asset.TryAddCharacters("Level 1_"))
                throw new InvalidOperationException("The title glyphs could not be baked.");
            asset.name = "CoinyRoundedSDF";
            asset.atlasPopulationMode = AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(asset, FontAssetPath);
            asset.material.name = "CoinyRoundedSDF Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            foreach (var atlas in asset.atlasTextures)
            {
                atlas.name = "CoinyRoundedSDF Atlas";
                AssetDatabase.AddObjectToAsset(atlas, asset);
            }
            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
