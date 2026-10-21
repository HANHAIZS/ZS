using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BeeHarvest.Editor
{
    public static class LevelOneBackgroundSetup
    {
        const string Folder = "Assets/Sprites/Gameplay/Backgrounds";
        public const string TexturePath = Folder + "/Level01HoneyGarden.png";
        public const string MaterialPath = Folder + "/Level01HoneyGarden.mat";

        /// <summary>Applies only background art and foreground readability to the current level.</summary>
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Apply background in Edit Mode");
            var cameraObject = GameObject.Find("LevelOneCamera");
            if (cameraObject == null) throw new System.InvalidOperationException("LevelOneCamera is missing");
            var camera = cameraObject.GetComponent<Camera>();
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
            if (importer == null) throw new System.InvalidOperationException("Background PNG is missing");
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) throw new System.InvalidOperationException("URP Unlit shader is missing");
                material = new Material(shader) { name = "Level01HoneyGarden" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            var existing = camera.transform.Find("LevelBackground");
            var background = existing != null ? existing.gameObject : GameObject.CreatePrimitive(PrimitiveType.Quad);
            background.name = "LevelBackground";
            background.transform.SetParent(camera.transform, false);
            background.transform.localPosition = new Vector3(0f, 0f, 50f);
            background.transform.localRotation = Quaternion.identity;
            var collider = background.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            var renderer = background.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            var view = background.GetComponent<LevelBackgroundView>();
            if (view == null) view = background.AddComponent<LevelBackgroundView>();
            view.ViewCamera = camera;
            view.BackgroundTexture = texture;
            view.Fit();
            EditorUtility.SetDirty(view);
            camera.backgroundColor = new Color(1f, .95f, .82f);
            var hud = Object.FindObjectOfType<LevelOneHud>();
            if (hud != null)
            {
                foreach (var text in hud.GetComponentsInChildren<Text>(true))
                    text.color = hud.Restart != null && text.transform.IsChildOf(hud.Restart.transform)
                        ? Color.white : new Color(.27f, .15f, .065f);
            }
            EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
            AssetDatabase.SaveAssets();
        }
    }
}
