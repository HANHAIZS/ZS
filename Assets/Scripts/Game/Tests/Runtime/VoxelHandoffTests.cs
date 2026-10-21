using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
    public sealed class VoxelHandoffTests : EconomyTestFixture
    {
        static IEnumerator Load()
        {
            SceneManager.LoadScene("Level01"); yield return null; yield return null;
            Object.FindObjectOfType<LevelOneController>().enabled = false;
        }
        static void Draw()
        {
            Object.FindObjectOfType<HiveModelView>().Sync();
            Object.FindObjectOfType<HarvestBoardView>().Sync();
            Object.FindObjectOfType<BeeFlightView>().RenderFlights();
        }
        static void Tick(float dt) { Object.FindObjectOfType<HarvestBoardView>().PublishCameraView(); BeeSceneSteps.Advance(dt); Draw(); }
        static void Clear(Renderer cargo)
        {
            foreach (var body in cargo.transform.parent.GetComponentsInChildren<Renderer>())
                if (body != cargo && !body.name.Contains("_Grip_")) Assert.IsFalse(BeeGeometryAssertions.Intersects(cargo,body), "Handoff enters " + body.name);
        }

        [UnityTest]
        public IEnumerator HandoffStartsAtTheSourceVoxelAndMovesContinuouslyToTheBee()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new ActivateHiveCommand(0)); Draw();
            var m = app.GetModel<HarvestModel>();
            var source = GameObject.Find(m.Cubes[m.Flights[0].Cube].Data.name).GetComponent<Renderer>();
            var originalScreen = Camera.main.WorldToScreenPoint(source.bounds.center);
            var originalScale = source.transform.lossyScale;
            var originalRotation = source.transform.rotation;
            BeeSceneSteps.FirstPickup();
            var cargo = GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel").GetComponent<Renderer>();
            Assert.IsFalse(source.enabled);
            Assert.IsTrue(cargo.gameObject.activeSelf);
            Assert.Less(Vector2.Distance(originalScreen, Camera.main.WorldToScreenPoint(cargo.bounds.center)), .5f,
                "The visible block must start where it left the flower, not teleport underneath the bee");
            Assert.Less(Vector3.Distance(originalScale, cargo.transform.lossyScale), .001f, "No sudden resize at handoff");
            Assert.Less(Quaternion.Angle(originalRotation, cargo.transform.rotation), .01f);
            Assert.AreSame(source.sharedMaterial, cargo.sharedMaterial);
            var begin = cargo.transform.position;
            var prior = cargo.transform.position;
            for (int i = 0; i < 14; i++)
            {
                Tick(.02f);
                var now = cargo.transform.position;
                float bound=m.Flights[0].TransportPhase==0?.065f:.55f;
                Assert.Less(Vector3.Distance(prior,now),bound,"Extraction is slow; later loaded travel is faster but remains bounded and continuous");
                Clear(cargo); prior=now;
            }
            Assert.Greater(Vector3.Distance(begin, cargo.transform.position), .1f, "The block actually travels from flower to hand");
            Tick(Mathf.Max(.02f,AdvanceHarvestCommand.LiftSeconds-.28f+.02f));
            var body=cargo.transform.parent.Cast<Transform>().First(t=>t!=cargo.transform);var held=body.InverseTransformPoint(cargo.transform.position);
            Tick(.08f);
            Assert.Less(Vector3.Distance(held,body.InverseTransformPoint(cargo.transform.position)), .001f, "Cargo follows the belly through turning and sway");
        }

        [UnityTest]
        public IEnumerator HandoffPausesAndRetryRestoresAllSourceVoxels()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new ActivateHiveCommand(0)); Draw(); BeeSceneSteps.FirstPickup();Tick(.1f);
            var cargo = GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel");
            var position = cargo.position; var rotation = cargo.rotation; var scale = cargo.lossyScale;
            app.SendCommand(new PauseHarvestCommand(true));
            GameObject.Find("Flower(Clone)").transform.Rotate(0, 70, 0);
            yield return null; Tick(5);
            Assert.Less(Vector3.Distance(position, cargo.position), .001f);
            Assert.Less(Quaternion.Angle(rotation, cargo.rotation), .01f);
            Assert.Less(Vector3.Distance(scale, cargo.lossyScale), .001f);
            Object.FindObjectOfType<LevelOneController>().RestartLevel(); Draw();
            Assert.IsNull(GameObject.Find("BlenderBeeCarrier_0"));
            var m = app.GetModel<HarvestModel>();
            foreach (var cube in m.Cubes) Assert.IsTrue(GameObject.Find(cube.Data.name).GetComponent<Renderer>().enabled);
            app.SendCommand(new ActivateHiveCommand(0)); Draw();
            Assert.AreEqual(39, m.Hives[0].Remaining, "Retry resets the staggered departure clock");
            Assert.AreEqual(1, m.Flights.Count);
        }

        [UnityTest]
        public IEnumerator FirstRenderAtPickupStillStartsAtTheRotatedSourceVoxel()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            GameObject.Find("Flower(Clone)").transform.rotation = Quaternion.Euler(0, 48, 0);
            Object.FindObjectOfType<HarvestBoardView>().PublishCameraView();
            app.SendCommand(new ActivateHiveCommand(0));
            var m = app.GetModel<HarvestModel>();
            var source = GameObject.Find(m.Cubes[m.Flights[0].Cube].Data.name).GetComponent<Renderer>();
            var screen = Camera.main.WorldToScreenPoint(source.bounds.center);
            BeeSceneSteps.FirstPickup();
            var cargo = GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel");
            Assert.Less(Vector2.Distance(screen, Camera.main.WorldToScreenPoint(cargo.GetComponent<Renderer>().bounds.center)), .5f);
            Assert.Less(Quaternion.Angle(source.transform.rotation, cargo.rotation), .01f);
            Clear(cargo.GetComponent<Renderer>());
        }
        static Bounds Geometry(Transform root)
        {
            var parts = root.GetComponentsInChildren<Renderer>().Where(r => r.enabled).ToArray();
            var bounds = parts[0].bounds;
            foreach (var part in parts) bounds.Encapsulate(part.bounds);
            return bounds;
        }

        [UnityTest]
        public IEnumerator SimultaneousHandoffsHaveClearanceAtMultipleFlowerAngles()
        {
            yield return Load();
            var level = Object.FindObjectOfType<LevelOneController>();
            var app = BeeHarvestApp.Interface;
            foreach (int angle in new[] { 0, 45, 90, 135 })
            {
                level.RestartLevel();
                HiveInputFixture.UseFiveColumnGeometryFixture();
                GameObject.Find("Flower(Clone)").transform.rotation = Quaternion.Euler(0, angle, 0);
                Object.FindObjectOfType<HarvestBoardView>().PublishCameraView();
                foreach (int col in new[] { 0, 1, 2, 3, 4 }) app.SendCommand(new ActivateHiveCommand(col));
                Draw();var model=app.GetModel<HarvestModel>();var seen=new System.Collections.Generic.HashSet<int>();
                for(int frame=0;frame<1600&&seen.Count<5;frame++){
                    Tick(.02f);
                    foreach(var f in model.Flights){
                        if(f.Id>=5||f.Stage!=1||seen.Contains(f.Id))continue;
                        var cargo=GameObject.Find("BlenderBeeCarrier_"+f.Id).transform.Find("CarriedVoxel").GetComponent<Renderer>();
                        var source=GameObject.Find(model.Cubes[f.Cube].Data.name).GetComponent<Renderer>();
                        Assert.IsFalse(source.enabled);Assert.IsTrue(cargo.gameObject.activeSelf);
                        Assert.Less(Vector2.Distance(Camera.main.WorldToScreenPoint(source.bounds.center),Camera.main.WorldToScreenPoint(cargo.bounds.center)),1.5f,"Every staggered handoff starts at its own source at angle "+angle);seen.Add(f.Id);
                    }
                    var actors=Object.FindObjectsOfType<Transform>().Where(t=>t.name.StartsWith("BlenderBeeCarrier_")).ToArray();
                    foreach(var actor in actors){var cargo=actor.Find("CarriedVoxel").GetComponent<Renderer>();if(cargo.gameObject.activeSelf)Clear(cargo);}
                    foreach(var actor in actors.Where(t=>t.Find("CarriedVoxel").gameObject.activeSelf))Assert.IsFalse(Geometry(actor).Intersects(Geometry(GameObject.Find("Flower(Clone)").transform)),"Loaded handoff enters model at angle "+angle+" frame "+frame);
                }
                Assert.AreEqual(5,seen.Count,"Exercise five independently scheduled pickups at each model angle");
            }
        }
        [UnityTest]
        public IEnumerator RotatingSourceFlowerKeepsEveryCarrierInsideCameraClipRange()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            var model = app.GetModel<HarvestModel>();
            var camera = Camera.main;
            foreach (int col in new[] { 0, 1, 2, 3, 4 })
            {
                app.SendCommand(new ActivateHiveCommand(col)); Tick(.07f);
            }
            for (int step = 0; step < 360 && !model.Won; step++)
            {
                if (step % 73 == 0) GameObject.Find("Flower(Clone)").transform.rotation = Quaternion.Euler(0, step * 11 % 180, 0);
                for(int col=0;col<5;col++)app.SendCommand(new ActivateHiveCommand(col));
                Tick(.1f);
                foreach (var actor in Object.FindObjectsOfType<Transform>().Where(t => t.name.StartsWith("BlenderBeeCarrier_")))
                {
                    var bounds = Geometry(actor);
                    float center = Vector3.Dot(bounds.center - camera.transform.position, camera.transform.forward);
                    float extent = Vector3.Dot(bounds.extents, new Vector3(Mathf.Abs(camera.transform.forward.x),
                        Mathf.Abs(camera.transform.forward.y), Mathf.Abs(camera.transform.forward.z)));
                    Assert.Greater(center - extent, camera.nearClipPlane, "Carrier enters near clipping plane at step " + step + " " + actor.name);
                    Assert.Less(center + extent, camera.farClipPlane);
                }
            }
            Assert.Greater(model.Deposited,0);
            Assert.AreEqual(1784,model.Remaining+model.InTransit+model.Deposited);
        }
    }
}
