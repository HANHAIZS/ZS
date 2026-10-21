using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeeHarvest.Editor
{
    public static class LevelOneHiveClickSetup
    {
        public const string TexturePath = OkaUiRefreshSetup.SlotTexturePath;
        const string MaterialPath = "Assets/Sprites/UI/Gameplay/HiveSlotTile.mat";

        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before setting up hive click slots.");
            var controller = UnityEngine.Object.FindObjectOfType<LevelOneController>();
            if (controller == null || controller.Hud == null) throw new InvalidOperationException("The level HUD is required.");
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            if (importer == null) throw new InvalidOperationException("The slot tile PNG is missing.");
            importer.textureType = TextureImporterType.Default;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath));
            OkaUiRefreshSetup.ApplySlotMaterial(material);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Surface", 1);
            material.SetFloat("_ZWrite", 0);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_Cull", (float)CullMode.Off);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            var view = controller.GetComponent<HiveModelView>();
            view.SlotMaterial = material;
            if (controller.GetComponent<HiveHopController>() == null) controller.gameObject.AddComponent<HiveHopController>();
            var hud = controller.Hud;
            hud.SlotTargets = new RectTransform[5];
            for (int i = 0; i < 5; i++)
            {
                var existing = hud.transform.Find("HiveSlotTarget_" + i);
                var target = existing != null ? (RectTransform)existing : new GameObject("HiveSlotTarget_" + i, typeof(RectTransform)).GetComponent<RectTransform>();
                target.SetParent(hud.transform, false);
                var label = hud.SlotLabels[i].rectTransform;
                target.anchorMin = target.anchorMax = (label.anchorMin + label.anchorMax) * .5f;
                target.pivot = Vector2.one * .5f;
                target.anchoredPosition = Vector2.zero;
                target.sizeDelta = Vector2.one * 96;
                hud.SlotTargets[i] = target;
                hud.SlotLabels[i].text = "";
                EditorUtility.SetDirty(hud.SlotLabels[i]);
            }
            EditorUtility.SetDirty(view);
            EditorUtility.SetDirty(hud);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            AssetDatabase.SaveAssets();
        }
    }
}
