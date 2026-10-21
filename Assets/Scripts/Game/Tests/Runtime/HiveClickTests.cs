using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
    public static class HiveInputFixture
    {
        // Geometry stress cases need five independent, immediately harvestable hives, separate from source puzzle ordering.
        public static LevelData FiveColumnGeometryData()
        {
            var app=BeeHarvestApp.Interface;
            var data=JsonUtility.FromJson<LevelData>(JsonUtility.ToJson(app.GetUtility<LevelOneAssets>().Level(1)));
            data.columns=5;foreach(var cube in data.cubes)cube.color="Palette17";
            for(int i=0;i<data.hives.Length;i++){data.hives[i].column=i%5;data.hives[i].connected=new int[0];data.hives[i].color="Palette17";}
            return data;
        }
        public static void UseFiveColumnGeometryFixture()
        {
            BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(FiveColumnGeometryData()));
            var level=Object.FindObjectOfType<LevelOneController>();level.Hud.Sync();level.GetComponent<HiveModelView>().Sync();
        }
        public static void RetryLevelForTest()
        {
            var level = Object.FindObjectOfType<LevelOneController>();
            Assert.IsTrue(level.TryRetryLevel(), "Internal retry remains available without a Settings UI entry");
        }
        public static void Tap(LevelOneHud hud, int column)
        {
            EventSystem.current.SendMessage("OnApplicationFocus", true);
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, hud.QueueButtons[column].transform.position)
            };
            ExecuteEvents.Execute(hud.QueueButtons[column].gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        public static string ObservedInputState(){
            var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();var nav=app.GetModel<MenuNavigationModel>();
            var l=Object.FindObjectOfType<LevelOneController>();var v=Object.FindObjectOfType<VictoryPopupController>(true);var h=l==null?null:l.GetComponent<HiveHopController>();
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var module=EventSystem.current==null?null:EventSystem.current.GetComponent<StandaloneInputModule>();
            return "scene="+SceneManager.GetActiveScene().name+" home="+nav.IsHome+" level="+nav.CurrentLevel+" paused="+m.Paused+" won="+m.Won
             +" controller="+(l==null?"null":(l.enabled+"/appPause="+l.IsApplicationPaused))
             +" hop="+(h==null?"null":h.GetType().GetField("hive",flags).GetValue(h)+"/modal="+h.GetType().GetField("modalInputBlocked",flags).GetValue(h))
             +" victory="+(v==null?"null":v.IsOpen+"/opening="+(PopupMotionTestWait.Motion(v)?.IsOpening??false)+"/fresh="+v.GetType().GetField("waitingForFreshPointer",flags).GetValue(v))
             +" input="+(module==null?"null":module.input.GetType().Name+"/touch="+module.input.touchCount+"/mouse="+module.input.GetMouseButton(0));
        }

        public static IEnumerator Click(LevelOneHud hud, int column)
        {
            // Let the regular HUD frame apply pause/restart state before synthetic input.
            yield return null;
            var app = BeeHarvestApp.Interface;
            var model = app.GetModel<HarvestModel>();
            int hive = app.GetSystem<HarvestSystem>().Front(column);
            if (hive < 0) yield break;
            Tap(hud, column);
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!model.Hives[hive].Started && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(model.Hives[hive].Started, "One click must finish the hop and activate the visible hive. "+ObservedInputState());
        }
    }

    public sealed class HiveClickTests : EconomyTestFixture
    {
        [UnityTest]
        public IEnumerator DisabledHopRejectsInputAndCanResumeWithANewClick()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var controller = Object.FindObjectOfType<LevelOneController>();
            var hop = controller.GetComponent<HiveHopController>();
            var model = BeeHarvestApp.Interface.GetModel<HarvestModel>();
            HiveInputFixture.Tap(controller.Hud, 0);
            yield return new WaitForSeconds(.08f);
            hop.enabled = false;
            HiveInputFixture.Tap(controller.Hud, 0);
            yield return new WaitForSeconds(.4f);
            Assert.IsFalse(model.Hives[0].Started, "Cached click/tick callbacks cannot bypass a disabled component");
            Assert.AreEqual(40, model.Hives[0].Remaining);
            Assert.AreEqual(0, model.Flights.Count);
            hop.enabled = true;
            yield return HiveInputFixture.Click(controller.Hud, 0);
            Assert.IsTrue(model.Hives[0].Started);
        }

        [UnityTest]
        public IEnumerator ClickHopsIntoFirstEmptySlotAndKeepsTheTile()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var hud = Object.FindObjectOfType<LevelOneController>().Hud;
            var app = BeeHarvestApp.Interface;
            var model = app.GetModel<HarvestModel>();
            var hive = GameObject.Find("SmallHive_0");
            int identity = hive.GetInstanceID();
            var before = Camera.main.WorldToScreenPoint(hive.transform.position);
            HiveInputFixture.Tap(hud, 0);
            Assert.IsFalse(model.Hives[0].Started, "Bees start only after the hive lands");
            yield return new WaitForSeconds(.12f);
            var moving = Camera.main.WorldToScreenPoint(hive.transform.position);
            Assert.Greater(moving.y - before.y, 20f, "The existing Blender model must visibly hop toward the slot");
            Assert.AreEqual(40, model.Hives[0].Remaining, "The animation cannot consume departure quota");
            Assert.Greater(Mathf.Abs(hive.transform.localScale.y / hive.transform.localScale.x - 1), .03f,
                "The hop includes a small stretch, rather than teleporting the model");
            var plaque = hive.GetComponentsInChildren<Transform>().First(t => t.name.EndsWith("_NumberAnchor"));
            var label = RectTransformUtility.WorldToScreenPoint(null, hud.QueueLabels[0].transform.position);
            Assert.Less(Vector2.Distance(label, Camera.main.WorldToScreenPoint(plaque.position)), 1f);
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!model.Hives[0].Started && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(model.Hives[0].Started);
            app.SendCommand(new PauseHarvestCommand(true));
            yield return null;
            Assert.AreEqual(0, model.Slots[0]);
            Assert.AreEqual(39, model.Hives[0].Remaining);
            Assert.AreEqual("39", hud.SlotLabels[0].text);
            Assert.AreEqual(identity, GameObject.Find("SmallHive_0").GetInstanceID());
            var tile = GameObject.Find("HiveSlotTile_0");
            Assert.IsNotNull(tile.GetComponent<Renderer>().sharedMaterial.mainTexture);
            Assert.Greater(Vector3.Dot(tile.transform.position - hive.transform.position, Camera.main.transform.forward), .5f);
            var center = Camera.main.WorldToViewportPoint(tile.transform.position);
            Assert.AreEqual(.11f, center.x, .003f);
            Assert.AreEqual(.3975f, center.y, .003f);
            Assert.IsTrue(hud.SlotLabels.Skip(1).All(t => t.text == ""));
            Assert.AreEqual(1f, hive.transform.localScale.y / hive.transform.localScale.x, .001f);
            Assert.IsFalse(ExecuteEvents.CanHandleEvent<IBeginDragHandler>(hud.QueueButtons[0].gameObject));
            Assert.IsFalse(ExecuteEvents.CanHandleEvent<IDragHandler>(hud.QueueButtons[0].gameObject));
        }

        [UnityTest]
        public IEnumerator RepeatedClicksDoNotActivateTheRearHiveOrAnotherColumn()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var hud = Object.FindObjectOfType<LevelOneController>().Hud;
            var model = BeeHarvestApp.Interface.GetModel<HarvestModel>();
            HiveInputFixture.Tap(hud, 0);
            Assert.IsFalse(hud.QueueButtons[0].interactable, "A moving hive cannot accept a new press");
            Assert.IsTrue(hud.QueueImages.All(i => !i.raycastTarget), "A press during the hop cannot queue a click for the rear hive");
            HiveInputFixture.Tap(hud, 0);
            HiveInputFixture.Tap(hud, 1);
            float deadline = Time.realtimeSinceStartup + 2f;
            while (!model.Hives[0].Started && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.AreEqual(1, model.Hives.Count(h => h.Started));
            Assert.IsFalse(model.Hives[1].Started);
            Assert.IsFalse(model.Hives[2].Started);
            yield return HiveInputFixture.Click(hud, 1);
            Assert.AreEqual(11, model.Slots[1], "The next click chooses the next free slot");
        }

        [UnityTest]
        public IEnumerator RestartPauseAndBackgroundCancelAnUnfinishedHop()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var controller = Object.FindObjectOfType<LevelOneController>();
            var hud = controller.Hud;
            var app = BeeHarvestApp.Interface;
            var model = app.GetModel<HarvestModel>();
            var home = GameObject.Find("SmallHive_0").transform.position;
            HiveInputFixture.Tap(hud, 0);
            yield return new WaitForSeconds(.08f);
            HiveInputFixture.RetryLevelForTest();
            yield return new WaitForSeconds(.4f);
            Assert.AreEqual(40, model.Hives[0].Remaining);
            Assert.AreEqual(0, model.Flights.Count);
            Assert.IsTrue(model.Slots.All(s => s < 0));
            Assert.Less(Vector3.Distance(home, GameObject.Find("SmallHive_0").transform.position), .005f);
            HiveInputFixture.Tap(hud, 0);
            yield return new WaitForSeconds(.08f);
            app.SendCommand(new PauseHarvestCommand(true));
            yield return null;
            app.SendCommand(new PauseHarvestCommand(false));
            yield return new WaitForSeconds(.4f);
            Assert.AreEqual(0, model.Flights.Count);
            HiveInputFixture.Tap(hud, 0);
            yield return new WaitForSeconds(.08f);
            controller.SendMessage("OnApplicationPause", true);
            controller.SendMessage("OnApplicationPause", false);
            yield return new WaitForSeconds(.4f);
            Assert.AreEqual(0, model.Flights.Count);
            yield return HiveInputFixture.Click(hud, 0);
            Assert.IsTrue(model.Hives[0].Started, "Cancellation releases the animation lock for another click");
        }

        [UnityTest]
        public IEnumerator FullSlotsRejectAClickWithoutMovingOrConsumingQuota()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var app = BeeHarvestApp.Interface;
            var original = app.GetUtility<LevelOneAssets>().Level();
            app.SendCommand(new StartLevelCommand(new LevelData
            {
                cubes = original.cubes,
                hives = original.hives
            }));
            for (int i = 0; i < 4; i++) app.SendCommand(new ActivateHiveCommand(1)); app.SendCommand(new ActivateHiveCommand(2));
            yield return null;
            var hud = Object.FindObjectOfType<LevelOneController>().Hud;
            var model = app.GetModel<HarvestModel>();
            var root = GameObject.Find("SmallHive_1");
            var before = root.transform.position;
            HiveInputFixture.Tap(hud, 0);
            yield return new WaitForSeconds(.4f);
            Assert.IsFalse(model.Hives[1].Started);
            Assert.AreEqual(80, model.Hives[1].Remaining);
            Assert.Less(Vector3.Distance(before, root.transform.position), .005f);
            Assert.IsTrue(model.Slots.All(s => s >= 0));
        }
    }
}
