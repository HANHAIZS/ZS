using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
    public sealed class BeeClearanceTests : EconomyTestFixture
    {
        static IEnumerator Load()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            Object.FindObjectOfType<LevelOneController>().enabled = false;
            HiveInputFixture.UseFiveColumnGeometryFixture();
        }

        static void Draw()
        {
            Object.FindObjectOfType<HiveModelView>().Sync();
            Object.FindObjectOfType<HarvestBoardView>().Sync();
            Object.FindObjectOfType<BeeFlightView>().RenderFlights();
        }

        static Transform[] Actors()
        {
            return Object.FindObjectsOfType<Transform>()
                .Where(t => t.name.StartsWith("BlenderBeeCarrier_")).ToArray();
        }

        static Bounds Geometry(Transform root)
        {
            var rs = root.GetComponentsInChildren<Renderer>().Where(r => r.enabled).ToArray();
            Assert.IsNotEmpty(rs);
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }

        [UnityTest]
        public IEnumerator CarriedVoxelStaysOutsideEveryBeePart()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new ActivateHiveCommand(0));
            Draw();
            var seen=new System.Collections.Generic.HashSet<int>();
            for(int step=0;step<1000&&seen.Count<3;step++){
                BeeSceneSteps.Advance(.02f);Draw();
                foreach(var actor in Actors().Where(t=>t.Find("CarriedVoxel").gameObject.activeSelf)){
                    var cargo=actor.Find("CarriedVoxel").GetComponent<Renderer>();
                    foreach(var part in actor.GetComponentsInChildren<Renderer>())if(part!=cargo&&!part.name.Contains("_Grip_"))Assert.IsFalse(BeeGeometryAssertions.Intersects(cargo,part),"Carried voxel intersects "+part.name);
                    seen.Add(int.Parse(actor.name.Substring("BlenderBeeCarrier_".Length)));
                }
            }
            Assert.AreEqual(3,seen.Count,"Check three distinct staggered carriers");
        }

        [UnityTest]
        public IEnumerator ConcurrentCarriersHaveClearanceThroughoutBothFlightStages()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            foreach (int c in new[] { 0, 1, 2, 3, 4 }) app.SendCommand(new ActivateHiveCommand(c));
            Draw();
            BeeSceneSteps.Advance(.31f); Draw();
            BeeSceneSteps.Advance(.31f); Draw();
            Assert.Greater(Actors().Length,3,"Independent mouths and unlimited Busy allow a visible swarm");
            for (int step = 0; step < 500; step++)
            {
                var actors = Actors();
                foreach(var actor in actors.Where(t=>t.Find("CarriedVoxel").gameObject.activeSelf))
                    Assert.IsFalse(Geometry(actor).Intersects(Geometry(GameObject.Find("Flower(Clone)").transform)),"Loaded bee or cargo crosses the source model");
                Object.FindObjectOfType<HarvestBoardView>().PublishCameraView();
                BeeSceneSteps.Advance(.06f);
                Draw();
            }
            var model = app.GetModel<HarvestModel>();
            Assert.Greater(model.Deposited, 0);
            Assert.AreEqual(1784, model.Remaining + model.InTransit + model.Deposited);
            app.SendCommand(new PauseHarvestCommand(true));
            var before = Actors().ToDictionary(a => a.name, a => a.position);
            yield return null;
            Draw();
            foreach (var actor in Actors()) Assert.Less(Vector3.Distance(before[actor.name], actor.position), .001f);
            Object.FindObjectOfType<LevelOneController>().RestartLevel();
            Draw();
            Assert.IsEmpty(Actors());
            Assert.AreEqual(1784, model.Remaining);
        }

        [UnityTest]
        public IEnumerator ReplacementHiveCanOverlapRetiringBeesWithoutCollisions()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            var data = HiveInputFixture.FiveColumnGeometryData();
            int combined = data.hives[0].quota + data.hives[5].quota;
            data.hives[0].quota = 1; data.hives[5].quota = combined - 1;
            app.SendCommand(new StartLevelCommand(data));
            Object.FindObjectOfType<HarvestBoardView>().PublishCameraView();
            app.SendCommand(new ActivateHiveCommand(0));
            Draw(); BeeSceneSteps.Advance(.31f); Draw();
            BeeSceneSteps.Advance(.31f); Draw();
            Assert.AreEqual(-1, app.GetModel<HarvestModel>().Slots[0], "Final departure releases the slot before delivery");
            app.SendCommand(new ActivateHiveCommand(0)); Draw();
            Assert.GreaterOrEqual(Actors().Length,1,"Retiring task stays alive while its slot is reused");
            for (int step = 0; step < 500; step++)
            {
                var actors = Actors();
                foreach(var actor in actors.Where(t=>t.Find("CarriedVoxel").gameObject.activeSelf))
                    Assert.IsFalse(Geometry(actor).Intersects(Geometry(GameObject.Find("Flower(Clone)").transform)),"Replacement-hive cargo crosses the source model");
                BeeSceneSteps.Advance(.04f); Draw();
            }
            var model = app.GetModel<HarvestModel>();
            Assert.AreEqual(1784, model.Remaining + model.InTransit + model.Deposited);
        }

        [UnityTest]
        public IEnumerator ConcurrentReturnsConvergeAtCollectorEntrance()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new ActivateHiveCommand(0));
            Draw();
            var board = Object.FindObjectOfType<HarvestBoardView>();
            var destination = Camera.main.WorldToScreenPoint(board.Deposit);
            var model = app.GetModel<HarvestModel>();
            var seen = new System.Collections.Generic.HashSet<int>();
            var previousCargo=new System.Collections.Generic.Dictionary<int,Vector2>();
            for (int step = 0; step < 2500 && seen.Count < 3; step++)
            {
                BeeSceneSteps.Advance(.01f); Draw();
                foreach (var flight in model.Flights){
                    if(flight.Id>=3)continue;
                    var actor=GameObject.Find("BlenderBeeCarrier_"+flight.Id);
                    if(flight.Stage==1)previousCargo[flight.Id]=Camera.main.WorldToScreenPoint(actor.transform.Find("CarriedVoxel").position);
                    if(flight.Stage!=2||seen.Contains(flight.Id))continue;
                    Assert.IsNull(actor,"Bee and cargo retire together without an empty return");
                    Assert.IsTrue(previousCargo.ContainsKey(flight.Id));
                    Assert.Less(Vector2.Distance(destination,previousCargo[flight.Id]),10f,"Cargo actually reached the hive mouth before retirement");
                    seen.Add(flight.Id);
                }
            }
            Assert.AreEqual(3, seen.Count, "Check three staggered return arrivals separately");
        }

        [UnityTest]
        public IEnumerator RotatedFlowerAndCollectorNeverIntersectCarriers()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            foreach (int c in new[] { 0, 1, 2, 3, 4 }) app.SendCommand(new ActivateHiveCommand(c));
            var flower = GameObject.Find("Flower(Clone)").transform;
            var collector = GameObject.Find("Collector(Clone)").transform;
            Draw();
            for (int step = 0; step < 40; step++)
            {
                flower.rotation = Quaternion.Euler(0, step * 17, 0);
                Object.FindObjectOfType<HarvestBoardView>().PublishCameraView();
                BeeSceneSteps.Advance(.06f);
                Draw();
                foreach (var actor in Actors())
                {
                    var bounds = Geometry(actor);
                    Assert.IsFalse(bounds.Intersects(Geometry(flower)), "Carrier crosses rotated flower at step " + step);
                    Assert.IsFalse(bounds.Intersects(Geometry(collector)), "Carrier crosses collector at step " + step);
                }
            }
        }
    }
}
