using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
    public sealed class HivePresentationTests : EconomyTestFixture
    {
        [UnityTest]
        public IEnumerator ImmediateSlotReuseKeepsBothExitOrigins()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var app = BeeHarvestApp.Interface;
            var original = app.GetUtility<LevelOneAssets>().Level();
            var hives = original.hives.Select(h => new HiveData
            {
                column = h.column, color = h.color, quota = h.quota
            }).ToArray();
            int combined = hives[0].quota + hives[1].quota;
            hives[0].quota = 1;
            hives[1].quota = combined - 1;
            hives[1].column = 4;
            app.SendCommand(new StartLevelCommand(new LevelData
            {
                cubes = original.cubes,
                hives = hives
            }));
            app.SendCommand(new ActivateHiveCommand(0));
            app.SendCommand(new ActivateHiveCommand(4));
            Object.FindObjectOfType<LevelOneController>().enabled=false;BeeSceneSteps.Until(()=>app.GetModel<HarvestModel>().Flights.Count>=2);
            app.SendCommand(new PauseHarvestCommand(true));
            yield return null;
            var view = Object.FindObjectOfType<HiveModelView>();
            Assert.AreEqual(1, app.GetModel<HarvestModel>().Slots[0]);
            Assert.Less(Vector3.Distance(view.Source(0), view.Source(1)), .005f,
                "An old tail flight uses the stable slot center, not the new count label position");
            Assert.Greater(Vector3.Distance(GameObject.Find("BlenderBeeCarrier_0").transform.position,
                GameObject.Find("BlenderBeeCarrier_1").transform.position), .1f, "Flights from an immediately reused slot need separate visual clearance");
            var revision = app.GetModel<HarvestModel>().Revision;
            Camera.main.orthographicSize *= 1.1f;
            view.Sync();
            Object.FindObjectOfType<BeeFlightView>().RenderFlights();
            Assert.AreEqual(revision, app.GetModel<HarvestModel>().Revision);
            Assert.Less(Vector3.Distance(view.Source(0), view.Source(1)), .005f,
                "Viewport changes also move the exhausted hive's pending departure origin");
            Assert.Less(GameObject.Find("BlenderBeeCarrier_0").transform.position.z, view.Source(0).z - .5f,
                "A carrier projects its stable exit onto the safe foreground flight plane");
        }

        [UnityTest]
        public IEnumerator BlenderHiveClickDepartureAndRestart()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var controller = Object.FindObjectOfType<LevelOneController>();
            var hud = controller.Hud;
            var app = BeeHarvestApp.Interface;
            var model = app.GetModel<HarvestModel>();
            var hive = GameObject.Find("SmallHive_0");
            Assert.IsNotNull(hive, "The queue must display a Blender hive mesh");
            Assert.AreEqual(2, hive.GetComponentsInChildren<MeshRenderer>().Length, "The optimized Blender hive uses one body and one collar mesh");
            int identity = hive.GetInstanceID();
            Assert.AreEqual(0f, hud.QueueImages[0].color.a, "The old color block is no longer drawn");
            var hits = new List<RaycastResult>();
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, hud.QueueButtons[0].transform.position)
            };
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.IsTrue(hits.Any(h => h.gameObject == hud.QueueButtons[0].gameObject), "The transparent button remains clickable");
            yield return HiveInputFixture.Click(hud,0);
            Assert.AreEqual(39, model.Hives[0].Remaining);
            Assert.AreEqual(1, model.Flights.Count);
            app.SendCommand(new PauseHarvestCommand(true));
            yield return null;
            Assert.AreEqual("39", hud.SlotLabels[0].text);
            Assert.AreEqual(1784, model.Remaining);
            Assert.AreEqual(0, model.InTransit);
            var exit = hive.GetComponentsInChildren<Transform>().First(t => t.name.EndsWith("_Mouth"));
            var actor = GameObject.Find("BlenderBeeCarrier_0");
            Assert.IsNotNull(actor);
            Assert.Less(actor.transform.position.z, exit.position.z - .5f, "Bee exits in front of hive walls");
            Assert.Less(Vector2.Distance(Camera.main.WorldToScreenPoint(exit.position),
                Camera.main.WorldToScreenPoint(actor.transform.position)), 1f, "First departing bee remains aligned with the real exit projection");
            var plaque = hive.GetComponentsInChildren<Transform>().First(t => t.name.EndsWith("_NumberAnchor"));
            var label = RectTransformUtility.WorldToScreenPoint(null, hud.SlotLabels[0].transform.position);
            var expected = Camera.main.WorldToScreenPoint(plaque.position);
            Assert.Less(Vector2.Distance(label, new Vector2(expected.x, expected.y)), 1f);
            BeeSceneSteps.Advance(1f);
            Assert.AreEqual(39, model.Hives[0].Remaining);
            app.SendCommand(new PauseHarvestCommand(false));
            BeeSceneSteps.Advance(AdvanceHarvestCommand.OutboundSeconds + .05f);
            yield return null;
            Assert.AreEqual(model.Hives[0].Data.quota-model.Flights.Count(f=>f.Hive==0),model.Hives[0].Remaining,"Every independent departure spends exactly one quota; pickup never spends twice");Assert.Greater(model.Flights.Count,3);
            BeeSceneSteps.Until(()=>model.InTransit>0);Assert.GreaterOrEqual(model.InTransit,1);
            HiveInputFixture.RetryLevelForTest();
            yield return null;
            Assert.AreEqual(40, model.Hives[0].Remaining);
            Assert.AreEqual("40", hud.QueueLabels[0].text);
            Assert.AreEqual(identity, GameObject.Find("SmallHive_0").GetInstanceID(), "Restart reuses the hive model");
            Assert.IsTrue(model.Slots.All(s => s < 0));
        }
    }
}
