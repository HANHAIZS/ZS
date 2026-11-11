using System.Collections;
using System.Linq;
using QFramework;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
    public sealed class ExpandedMidgamePresentationTests : EconomyTestFixture
    {
        internal static Vector4 RotationEnvelope(Transform root, Camera camera)
        {
            Assert.IsTrue(camera.orthographic);
            var envelope = new Vector4(float.PositiveInfinity, float.NegativeInfinity,
                float.PositiveInfinity, float.NegativeInfinity);
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
            {
                var bounds = filter.sharedMesh.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    var local = bounds.center + Vector3.Scale(bounds.extents,
                        new Vector3((corner & 1) == 0 ? -1 : 1,
                            (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                    var relative = Vector3.Scale(root.InverseTransformPoint(filter.transform.TransformPoint(local)), root.lossyScale);
                    var center = root.position + Vector3.up * relative.y;
                    var origin = camera.WorldToViewportPoint(center);
                    var cosine = camera.WorldToViewportPoint(center + new Vector3(relative.x, 0, relative.z)) - origin;
                    var sine = camera.WorldToViewportPoint(center + new Vector3(relative.z, 0, -relative.x)) - origin;
                    float width = Mathf.Sqrt(cosine.x * cosine.x + sine.x * sine.x);
                    float height = Mathf.Sqrt(cosine.y * cosine.y + sine.y * sine.y);
                    envelope.x = Mathf.Min(envelope.x, origin.x - width);
                    envelope.y = Mathf.Max(envelope.y, origin.x + width);
                    envelope.z = Mathf.Min(envelope.z, origin.y - height);
                    envelope.w = Mathf.Max(envelope.w, origin.y + height);
                }
            }
            return envelope;
        }

        internal static float SlotCeiling(Camera camera)
        {
            float ceiling = 0;
            for (int slot = 0; slot < 5; slot++)
            {
                var filter = GameObject.Find("HiveSlotTile_" + slot).GetComponent<MeshFilter>();
                var bounds = filter.sharedMesh.bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    var point = filter.transform.TransformPoint(bounds.center + Vector3.Scale(bounds.extents,
                        new Vector3((corner & 1) == 0 ? -1 : 1,
                            (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1)));
                    ceiling = Mathf.Max(ceiling, camera.WorldToViewportPoint(point).y);
                }
            }
            return ceiling + .015f;
        }

        internal struct Orbit
        {
            public Vector2 Center, Radius, Padding;
        }

        internal static Orbit[] Orbits(Transform root, Camera camera)
        {
            Assert.Less(Mathf.Abs(camera.transform.right.y), .0001f);
            return root.GetComponentsInChildren<Renderer>().Select(renderer =>
            {
                var relative = renderer.bounds.center - root.position;
                var center = root.position + Vector3.up * relative.y;
                Vector2 origin = camera.WorldToViewportPoint(center);
                Vector2 cosine = (Vector2)camera.WorldToViewportPoint(center + new Vector3(relative.x, 0, relative.z)) - origin;
                Vector2 sine = (Vector2)camera.WorldToViewportPoint(center + new Vector3(relative.z, 0, -relative.x)) - origin;
                float sphere = renderer.bounds.extents.magnitude / (2 * camera.orthographicSize);
                return new Orbit
                {
                    Center = origin,
                    Radius = new Vector2(Mathf.Sqrt(cosine.x * cosine.x + sine.x * sine.x),
                        Mathf.Sqrt(cosine.y * cosine.y + sine.y * sine.y)),
                    Padding = new Vector2(sphere / camera.aspect, sphere)
                };
            }).ToArray();
        }

        internal static Rect ExtraSlot(Camera camera, LevelOneController level)
        {
            var tile = level.GetComponentsInChildren<Renderer>(true).Single(r => r.name == "HiveSlotTile_5");
            var filter = tile.GetComponent<MeshFilter>();
            var bounds = filter.sharedMesh.bounds;
            Vector2 min = Vector2.one * float.PositiveInfinity;
            Vector2 max = Vector2.one * float.NegativeInfinity;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector2 point = camera.WorldToViewportPoint(filter.transform.TransformPoint(bounds.center + Vector3.Scale(bounds.extents,
                    new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1))));
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            return Rect.MinMaxRect(min.x - .005f, min.y - .005f, max.x + .005f, max.y + .005f);
        }

        internal static bool HitsSlot(Orbit[] orbits, Rect slot, float factor, Vector2 center)
        {
            foreach (var orbit in orbits)
            {
                var origin = center + (orbit.Center - center) * factor;
                var radius = orbit.Radius * factor;
                var padding = orbit.Padding * factor;
                float left = slot.xMin - padding.x - origin.x, right = slot.xMax + padding.x - origin.x;
                float bottom = slot.yMin - padding.y - origin.y, top = slot.yMax + padding.y - origin.y;
                if (radius.x < .000001f)
                {
                    if (left <= 0 && right >= 0 && bottom <= radius.y && top >= -radius.y) return true;
                    continue;
                }
                if (radius.y < .000001f)
                {
                    if (bottom <= 0 && top >= 0 && left <= radius.x && right >= -radius.x) return true;
                    continue;
                }
                left /= radius.x; right /= radius.x; bottom /= radius.y; top /= radius.y;
                float nearX = left > 0 ? left : right < 0 ? right : 0;
                float nearY = bottom > 0 ? bottom : top < 0 ? top : 0;
                float farX = Mathf.Max(Mathf.Abs(left), Mathf.Abs(right));
                float farY = Mathf.Max(Mathf.Abs(bottom), Mathf.Abs(top));
                if (nearX * nearX + nearY * nearY <= 1 && farX * farX + farY * farY >= 1) return true;
            }
            return false;
        }

        [UnityTest]
        public IEnumerator MidgameLightingAndExpandedModelsFitThePortraitPlayArea()
        {
            LevelOneAssets.ReloadPresentationCatalog();
            for (int number = 8; number <= 96; number++)
            {
                LevelSceneTestLoader.Load(LevelOneAssets.LevelKey(number));
                yield return null;
                yield return null;
                var level = Object.FindObjectOfType<LevelOneController>();
                level.enabled = false;
                var board = level.GetComponent<HarvestBoardView>();
                board.AutoRotate = false;
                board.ResetRotation();
                var profile = LevelOneAssets.Presentation(number);
                var root = GameObject.Find(profile.modelKey + "(Clone)").transform;
                var renderers = root.GetComponentsInChildren<Renderer>();
                var light = GameObject.Find("KeyLight").GetComponent<Light>();
                Assert.AreEqual(new Color(.48f, .52f, .63f), RenderSettings.ambientLight, "Environment at " + number);
                Assert.AreEqual(LightShadows.Soft, light.shadows, "Soft shadow at " + number);
                Assert.AreEqual(.30f, light.shadowStrength);
                Assert.AreEqual(.04f, light.shadowBias);
                Assert.AreEqual(.20f, light.shadowNormalBias);
                Assert.IsTrue(renderers.All(r => r.sharedMaterial.shader.name == "BeeHarvest/VoxelCavity"));
                Assert.IsTrue(renderers.All(r => r.sharedMaterial.GetFloat("_EdgeWidth") == 0));

                if (number >= 9 && number <= 12)
                {
                    var originals = Resources.Load<GameObject>("Prefabs/Gameplay/" + profile.modelKey)
                        .GetComponentsInChildren<Renderer>().ToDictionary(r => r.name);
                    foreach (var renderer in renderers)
                    {
                        var source = originals[renderer.name].sharedMaterial;
                        var actual = renderer.sharedMaterial;
                        Assert.AreSame(source.GetTexture("_BaseMap"), actual.GetTexture("_BaseMap"));
                        Assert.AreSame(source.GetTexture("_BumpMap"), actual.GetTexture("_BumpMap"));
                        Assert.AreEqual(source.GetTextureScale("_BaseMap"), actual.GetTextureScale("_BaseMap"));
                        Assert.AreEqual(source.GetTextureOffset("_BaseMap"), actual.GetTextureOffset("_BaseMap"));
                        Assert.AreEqual(source.GetFloat("_BumpScale"), actual.GetFloat("_BumpScale"));
                        Assert.IsTrue(actual.IsKeywordEnabled("_VOXEL_SURFACE"));
                        Assert.IsTrue(actual.IsKeywordEnabled("_NORMALMAP"));
                    }
                }

                if (number < 13 || number > 95) continue;
                Assert.Greater(profile.modelScaleMultiplier, 1, "Model enlarged at " + number);
                Assert.LessOrEqual(profile.modelScaleMultiplier, 1.33f);
                var camera = Camera.main;
                foreach (float aspect in new[] { 9f / 16, 9f / 20 })
                {
                    camera.aspect = aspect;
                    level.RestartLevel();
                    level.Hud.Sync();
                    level.GetComponent<HiveModelView>().Sync();
                    board.Sync();
                    var fiveSlotScale = root.localScale;
                    CheckBounds(root, camera, number);
                    var app = BeeHarvestApp.Interface;
                    app.SendCommand(new RefillHarvestBoosterCommand(0));
                    app.SendCommand(new AddHiveSlotCommand());
                    level.Hud.Sync();
                    level.GetComponent<HiveModelView>().Sync();
                    board.Sync();
                    CheckBounds(root, camera, number);
                    var center = new Vector2(.5f, profile.modelCenterY + board.LayoutViewportYOffset);
                    Assert.IsFalse(HitsSlot(Orbits(root, camera), ExtraSlot(camera, level), 1, center),
                        "The model must clear the sixth slot throughout rotation at " + number);
                    level.RestartLevel();
                    level.Hud.Sync();
                    level.GetComponent<HiveModelView>().Sync();
                    board.Sync();
                    Assert.Less(Vector3.Distance(fiveSlotScale, root.localScale), .0001f,
                        "Retry restores the five-slot model size at " + number);
                }
            }
        }

        static void CheckBounds(Transform root, Camera camera, int number)
        {
            var envelope = RotationEnvelope(root, camera);
            float bottom = SlotCeiling(camera);
            Assert.Greater(envelope.x, .0499f, "Left rotation boundary at " + number);
            Assert.Less(envelope.y, .9501f, "Right rotation boundary at " + number);
            Assert.Greater(envelope.z, bottom - .0001f, "Model stays above slots at " + number);
            Assert.Less(envelope.w, .8501f, "Model stays below header at " + number);
            TestContext.WriteLine("Level " + number + " aspect=" + camera.aspect +
                " scale=" + root.localScale.x + " bounds=" + envelope.ToString("F5"));
        }

        [UnityTest]
        public IEnumerator CameraZoomAndMovedExtraSlotRefreshTheFit()
        {
            LevelSceneTestLoader.Load("Level69");
            yield return null;
            yield return null;
            var level = Object.FindObjectOfType<LevelOneController>();
            level.enabled = false;
            var board = level.GetComponent<HarvestBoardView>();
            board.AutoRotate = false;
            var camera = Camera.main;
            camera.aspect = 9f / 16;
            var root = GameObject.Find(LevelOneAssets.ModelKey(69) + "(Clone)").transform;
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new RefillHarvestBoosterCommand(0));
            app.SendCommand(new AddHiveSlotCommand());
            level.Hud.Sync();
            level.GetComponent<HiveModelView>().Sync();
            board.Sync();
            camera.orthographicSize *= 1.1f;
            level.GetComponent<HiveModelView>().Sync();
            board.Sync();
            CheckBounds(root, camera, 69);
            Assert.IsFalse(HitsSlot(Orbits(root, camera), ExtraSlot(camera, level), 1, Vector2.zero));
            var previous = root.localScale;
            var sixth = level.Hud.SlotLabels[5].rectTransform;
            sixth.anchorMin += Vector2.right * .12f;
            sixth.anchorMax += Vector2.right * .12f;
            app.SendCommand(new PauseHarvestCommand(true));
            level.GetComponent<HiveModelView>().Sync();
            board.Sync();
            Assert.Less(root.localScale.x, previous.x, "A moved slot invalidates the cached fit");
            CheckBounds(root, camera, 69);
            Assert.IsFalse(HitsSlot(Orbits(root, camera), ExtraSlot(camera, level), 1, Vector2.zero));
        }

        [UnityTest]
        public IEnumerator CavityMaterialRendersTheExistingSurfaceAndNormalMaps()
        {
            LevelSceneTestLoader.Load("Level09");
            yield return null;
            yield return null;
            Object.FindObjectOfType<LevelOneController>().enabled = false;
            var source = GameObject.Find(LevelOneAssets.ModelKey(9) + "(Clone)")
                .GetComponentsInChildren<Renderer>()
                .OrderByDescending(r => r.sharedMaterial.GetColor("_BaseColor").grayscale).First();
            var material = new Material(source.sharedMaterial);
            material.SetFloat("_CavityStrength", 0);
            var cube = new GameObject("CavitySurfaceProbe");
            var cameraObject = new GameObject("CavitySurfaceCamera");
            var target = new RenderTexture(128, 128, 24);
            var pixels = new Texture2D(128, 128, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                cube.layer = 30;
                cube.transform.position = new Vector3(1000, 1000, 1000);
                cube.AddComponent<MeshFilter>().sharedMesh = source.GetComponent<MeshFilter>().sharedMesh;
                cube.AddComponent<MeshRenderer>().sharedMaterial = material;
                var camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                camera.orthographic = true;
                camera.orthographicSize = .64f;
                camera.nearClipPlane = .1f;
                camera.farClipPlane = 10;
                camera.cullingMask = 1 << 30;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.blue;
                camera.transform.position = cube.transform.position + Vector3.back * 3;
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
                pixels.Apply();
                var textured = pixels.GetPixels();
                material.SetFloat("_BumpScale", 0);
                camera.Render();
                pixels.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
                pixels.Apply();
                var noNormal = pixels.GetPixels();
                material.SetTexture("_BaseMap", null);
                camera.Render();
                pixels.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
                pixels.Apply();
                var plain = pixels.GetPixels();
                int surfaceChanged = 0, normalChanged = 0;
                for (int pixel = 0; pixel < plain.Length; pixel++)
                {
                    if (Vector4.Distance(noNormal[pixel], plain[pixel]) > .01f) surfaceChanged++;
                    if (Vector4.Distance(textured[pixel], noNormal[pixel]) > .001f) normalChanged++;
                }
                TestContext.WriteLine("Cavity surface pixels=" + surfaceChanged + "; normal pixels=" + normalChanged);
                Assert.Greater(surfaceChanged, 100, "The original surface atlas must affect rendered pixels");
                Assert.Greater(normalChanged, 10, "The original normal map must affect rendered pixels");
            }
            finally
            {
                RenderTexture.active = previous;
                Object.Destroy(cube);
                Object.Destroy(cameraObject);
                Object.Destroy(material);
                Object.Destroy(pixels);
                target.Release();
                Object.Destroy(target);
            }
        }
    }
}
