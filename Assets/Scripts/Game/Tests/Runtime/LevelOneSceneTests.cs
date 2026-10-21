using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
    public sealed class LevelOneSceneTests : EconomyTestFixture
    {
        /// <summary>Exercises real scene bindings, rendered Blender art, stable cargo return and restart.</summary>
        [UnityTest]
        public IEnumerator ClickingCarriesDeliversAndRestarts()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var controller = Object.FindObjectOfType<LevelOneController>();
            Assert.IsNotNull(controller);
            var app = BeeHarvestApp.Interface;
            var model = app.GetModel<HarvestModel>();
            var board = controller.GetComponent<HarvestBoardView>();
            var bees = controller.GetComponent<BeeFlightView>();
            var hud = controller.Hud;
            Assert.AreEqual(1784, model.Remaining);
            Assert.AreEqual(1784, Object.FindObjectsOfType<MeshRenderer>().Count(r => r.name.StartsWith("FlowerV2_Voxel_") && r.enabled));
            var marker = Object.FindObjectsOfType<Transform>().First(t => t.name=="Large_Hive_Collector_Mouth");
            Assert.Less(Vector3.Distance(marker.position, board.Deposit), .001f);
            yield return HiveInputFixture.Click(hud,0);
            Assert.AreEqual(1, model.Hives.Count(h => h.Started));
            float deadline = Time.realtimeSinceStartup + 6f;
            while (model.InTransit == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.Greater(model.InTransit, 0, "Clicking a hive must produce pickup in actual frames");
            app.SendCommand(new PauseHarvestCommand(true));
            yield return null;
            var flight = model.Flights.First(f => f.Stage == 1);
            var actor = GameObject.Find("BlenderBeeCarrier_" + flight.Id);
            Assert.IsNotNull(actor);
            var cargo = actor.transform.Find("CarriedVoxel");
            Assert.IsTrue(cargo.gameObject.activeSelf);
            Assert.Greater(cargo.GetComponent<Renderer>().bounds.size.magnitude, .2f);
            var before = actor.transform.position;
            GameObject.Find("Flower(Clone)").transform.Rotate(0, 70, 0);
            yield return null;
            yield return null;
            Assert.Less(Vector3.Distance(before, actor.transform.position), .001f, "Detached cargo must not follow flower rotation");
            app.SendCommand(new PauseHarvestCommand(false));
            for (int c = 1; c < model.ColumnCount; c++) yield return HiveInputFixture.Click(hud,c);
            deadline = Time.realtimeSinceStartup + 6f;
            while (model.Deposited == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.Greater(model.Deposited, 0, "Actual frames must deliver to the collector");
            board.PublishCameraView();
            // This source puzzle requires choices; bounded real-board coverage proves transport and conservation.
            int delivered=model.Deposited;
            for (int i=0;i<120;i++){board.TickAutoRotation(.2f);board.PublishCameraView();BeeSceneSteps.Advance(.2f);}
            yield return null;
            Assert.Greater(model.Deposited,delivered);
            Assert.AreEqual(1784,model.Remaining+model.InTransit+model.Deposited);
            Assert.AreEqual(model.Remaining,Object.FindObjectsOfType<MeshRenderer>().Count(r=>r.name.StartsWith("FlowerV2_Voxel_")&&r.enabled));
            HiveInputFixture.RetryLevelForTest();
            yield return null;
            Assert.AreEqual(1784, model.Remaining);
            Assert.AreEqual(0, model.Deposited);
            Assert.IsFalse(model.Won);
            Assert.IsTrue(model.Slots.All(s => s < 0));
            Assert.AreEqual(1784, Object.FindObjectsOfType<MeshRenderer>().Count(r => r.name.StartsWith("FlowerV2_Voxel_") && r.enabled));
        }
    }
}
