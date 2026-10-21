using System.Collections;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
    public sealed class ColoredHiveChainTests : EconomyTestFixture
    {
        [TearDown]
        public void CancelUnfinishedHopBeforeTheNextScene()
        {
            var hop = Object.FindObjectOfType<HiveHopController>();
            if (hop != null) hop.Cancel();
        }

        static IEnumerator LoadPair()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var level = Object.FindObjectOfType<LevelOneController>();
            level.enabled = false;
            var source = BeeHarvestApp.Interface.GetUtility<LevelOneAssets>().Level(1);
            var data = JsonUtility.FromJson<LevelData>(JsonUtility.ToJson(source));
            foreach (var hive in data.hives) hive.connected = new int[0];
            // Existing quotas and cube colors remain valid: magenta 0, green 2.
            data.hives[2].column = 1;
            data.hives[0].connected = new[] { 2 };
            data.hives[2].connected = new[] { 0 };
            BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(data));
            level.Hud.Sync();
            level.GetComponent<HiveModelView>().Sync();
        }

        static MeshRenderer Rod()
        {
            var root = GameObject.Find("HiveChainLink_0_2");
            Assert.IsNotNull(root, "Visible linked endpoints must have a rod");
            var renderer = root.GetComponent<MeshRenderer>();
            Assert.IsNotNull(renderer, "A shaded capsule mesh replaces the gold line");
            return renderer;
        }

        [UnityTest]
        public IEnumerator EachHalfUsesItsEndpointPaletteAndKeepsOneSharedMaterial()
        {
            yield return LoadPair();
            var rod = Rod();
            var properties = new MaterialPropertyBlock();
            rod.GetPropertyBlock(properties);
            Color magenta, green;
            ColorUtility.TryParseHtmlString("#D80868", out magenta);
            ColorUtility.TryParseHtmlString("#187808", out green);
            Assert.Less(Vector4.Distance(magenta, properties.GetColor("_ColorA")), .00001f);
            Assert.Less(Vector4.Distance(green, properties.GetColor("_ColorB")), .00001f);
            Assert.AreEqual("BeeHarvest/HiveRoundedChain", rod.sharedMaterial.shader.name);
            Assert.IsTrue(rod.sharedMaterial.shader.isSupported);
            var mesh = rod.GetComponent<MeshFilter>().sharedMesh;
            Assert.Greater(mesh.vertexCount, 4, "Rod must bend without disconnecting its caps");
            Assert.IsNull(rod.GetComponent<Collider>());
            var material = rod.sharedMaterial;
            var level = Object.FindObjectOfType<LevelOneController>();
            level.GetComponent<HiveHopController>().ClickWaiting(0);
            level.GetComponent<HiveHopController>().Tick(.12f);
            Assert.AreSame(material, Rod().sharedMaterial);
            Assert.AreSame(mesh, Rod().GetComponent<MeshFilter>().sharedMesh);
        }

        static Vector3 Middle(MeshRenderer rod)
        {
            var vertices = rod.GetComponent<MeshFilter>().sharedMesh.vertices;
            int pair = (vertices.Length / 2) / 2;
            return Camera.main.WorldToScreenPoint(rod.transform.TransformPoint((vertices[pair * 2] + vertices[pair * 2 + 1]) * .5f));
        }

        static Vector3 AnchorMidpoint()
        {
            var camera = Camera.main;
            var a = camera.WorldToScreenPoint(GameObject.Find("SmallHive_0").transform.position);
            var b = camera.WorldToScreenPoint(GameObject.Find("SmallHive_2").transform.position);
            var middle = (a + b) * .5f;
            middle.y += camera.pixelHeight * .014f;
            return middle;
        }

        [UnityTest]
        public IEnumerator HopBendsSlightlyThenLandingAndCancellationRestoreTheAttachedStraightRod()
        {
            yield return LoadPair();
            var level = Object.FindObjectOfType<LevelOneController>();
            var hop = level.GetComponent<HiveHopController>();
            Assert.Less(Vector2.Distance(Middle(Rod()), AnchorMidpoint()), 1);
            hop.ClickWaiting(0);
            hop.Tick(.12f);
            float bend = Vector2.Distance(Middle(Rod()), AnchorMidpoint());
            Assert.Greater(bend, 1, "Hop introduces a small elastic bend");
            Assert.Less(bend, Camera.main.pixelHeight * .01f, "Motion stays subtle");
            hop.Tick(.23f);
            Assert.Less(Vector2.Distance(Middle(Rod()), AnchorMidpoint()), 1);
            var state = BeeHarvestApp.Interface.GetModel<HarvestModel>();
            Assert.IsTrue(state.Hives[0].Started);
            Assert.IsTrue(state.Hives[2].Started);
            yield return LoadPair();
            level = Object.FindObjectOfType<LevelOneController>();
            hop = level.GetComponent<HiveHopController>();
            hop.ClickWaiting(0);
            hop.Tick(.1f);
            BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(true));
            hop.Tick(.01f);
            Assert.Less(Vector2.Distance(Middle(Rod()), AnchorMidpoint()), 1);
            Assert.IsFalse(BeeHarvestApp.Interface.GetModel<HarvestModel>().Hives[0].Started);
        }

        [UnityTest]
        public IEnumerator ANewGenerationHidesOldGeometryImmediatelyBeforeDeferredDestruction()
        {
            yield return LoadPair();
            var oldRod = Rod();
            var oldMesh = oldRod.GetComponent<MeshFilter>().sharedMesh;
            BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(BeeHarvestApp.Interface.GetUtility<LevelOneAssets>().Level(1)));
            var level = Object.FindObjectOfType<LevelOneController>();
            level.Hud.Sync();
            level.GetComponent<HiveModelView>().Sync();
            Assert.IsFalse(oldRod.gameObject.activeSelf, "Deferred Destroy must not show a stale rod for the rest of this frame");
            yield return null;
            Assert.IsTrue(oldMesh == null);
        }

        [UnityTest]
        public IEnumerator MissingEndpointHidesTheRodAndSceneExitReleasesItsMesh()
        {
            yield return LoadPair();
            var rod = Rod();
            var mesh = rod.GetComponent<MeshFilter>().sharedMesh;
            GameObject.Find("SmallHive_2").SetActive(false);
            Object.FindObjectOfType<HiveModelView>().ShowHop(0);
            Assert.IsFalse(rod.gameObject.activeSelf, "A hidden endpoint must not leave a dangling link");
            SceneManager.LoadScene("MainMenu");
            yield return null;
            yield return null;
            Assert.IsTrue(mesh == null, "Scene-local dynamic geometry must be disposed");
        }
    }
}
