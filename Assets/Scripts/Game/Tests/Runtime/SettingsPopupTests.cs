using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameFoundation.UI;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BeeHarvest.Tests
{
    public sealed class SettingsPopupTests : EconomyTestFixture
    {
        readonly string[] names = { "Music", "Sound", "Haptic" };
        readonly bool[] had = new bool[3];
        readonly int[] saved = new int[3];

        [SetUp]
        public void RememberPreferences()
        {
            for (int i = 0; i < 3; i++)
            {
                string key = "BeeHarvest.settings.v1." + names[i];
                had[i] = PlayerPrefs.HasKey(key);
                saved[i] = PlayerPrefs.GetInt(key, 1);
            }
        }

        [TearDown]
        public void RestorePreferences()
        {
            var asm = typeof(HarvestModel).Assembly;
            var commandType = asm.GetType("BeeHarvest.SetPreferenceCommand");
            var optionType = asm.GetType("BeeHarvest.GamePreference");
            for (int i = 0; i < 3; i++)
            {
                if (commandType != null)
                    BeeHarvestApp.Interface.SendCommand((ICommand)Activator.CreateInstance(commandType,
                        Enum.ToObject(optionType, i), saved[i] != 0));
                string key = "BeeHarvest.settings.v1." + names[i];
                if (had[i]) PlayerPrefs.SetInt(key, saved[i]); else PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
        }

        Button Gear()
        {
            var go = GameObject.Find("SettingsButton");
            Assert.IsNotNull(go, "The first level must have the supplied settings icon");
            return go.GetComponent<Button>();
        }

        [UnityTest]
        public IEnumerator OpensPausesBlocksTheLevelAndClosesWithoutLosingProgress()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var app = BeeHarvestApp.Interface;
            var m = app.GetModel<HarvestModel>();
            var hud = UnityEngine.Object.FindObjectOfType<LevelOneController>().Hud;
            Assert.IsNull(hud.Restart, "The obsolete restart entry remains absent");
            Assert.IsNull(hud.transform.Find("RestartLevel"));
            HiveInputFixture.Tap(hud, 0);
            yield return new WaitForSeconds(.08f);
            Gear().onClick.Invoke();
            yield return null;
            Assert.IsTrue(m.Paused);
            Assert.AreEqual(0, m.Flights.Count, "Opening settings cancels an unfinished hive hop");
            Assert.AreEqual(40, m.Hives[0].Remaining);
            var modal = GameObject.Find("SettingsPopup");
            Assert.IsNotNull(modal);
            Assert.IsNotNull(modal.transform.Find("PopupContent/TitleBanner").GetComponent<Image>().sprite);
            Assert.IsTrue(modal.GetComponentsInChildren<TMP_Text>().All(t => t.font != null));
            Assert.IsTrue(GameObject.Find("HomeButton").GetComponent<Button>().interactable);
            var p = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, hud.QueueButtons[0].transform.position)
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(p, hits);
            Assert.IsTrue(hits[0].gameObject.transform.IsChildOf(modal.transform), "The modal intercepts taps over the hive queue");
            int generation = m.Generation;
            yield return new WaitForSeconds(.45f);
            Assert.AreEqual(0, m.Flights.Count);
            GameObject.Find("CloseButton").GetComponent<Button>().onClick.Invoke();
            yield return PopupMotionTestWait.Close();
            Assert.IsFalse(m.Paused);
            Assert.AreEqual(generation, m.Generation);
            Assert.IsNull(app.GetUtility<UiPopupRouter>().ActiveView);
            yield return HiveInputFixture.Click(hud, 0);
            Assert.AreEqual(39, m.Hives[0].Remaining);
        }

        [UnityTest]
        public IEnumerator SwitchesPersistAndRepeatedOpenDoesNotDoubleToggle()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            for (int cycle = 0; cycle < 3; cycle++)
            {
                Gear().onClick.Invoke();
                yield return null;
                for (int i = 0; i < 3; i++)
                {
                    var toggle = GameObject.Find(names[i] + "Toggle").GetComponent<Button>();
                    var mark = toggle.transform.Find("OffMark").gameObject;
                    bool wasOff = mark.activeSelf;
                    toggle.onClick.Invoke();
                    Assert.AreEqual(!wasOff, mark.activeSelf, "One click must toggle exactly once");
                    Assert.AreEqual(wasOff ? 1 : 0, PlayerPrefs.GetInt("BeeHarvest.settings.v1." + names[i], -1));
                    toggle.onClick.Invoke();
                    Assert.AreEqual(wasOff, mark.activeSelf);
                }
                GameObject.Find("CloseButton").GetComponent<Button>().onClick.Invoke();
                yield return PopupMotionTestWait.Close();
            }
        }

        [UnityTest]
        public IEnumerator ClosePreservesTheLevelAndOldPauseIsPreservedOnClose()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var app = BeeHarvestApp.Interface;
            var m = app.GetModel<HarvestModel>();
            app.SendCommand(new PauseHarvestCommand(true));
            Gear().onClick.Invoke();
            yield return null;
            GameObject.Find("CloseButton").GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(m.Paused, "Closing settings cannot release a pause it did not acquire");
            yield return PopupMotionTestWait.Close();
            app.SendCommand(new PauseHarvestCommand(false));
            yield return null;
            var hud = UnityEngine.Object.FindObjectOfType<LevelOneController>().Hud;
            yield return HiveInputFixture.Click(hud, 0);
            int generation = m.Generation;
            Gear().onClick.Invoke();
            yield return null;
            GameObject.Find("CloseButton").GetComponent<Button>().onClick.Invoke();
            yield return PopupMotionTestWait.Close();
            Assert.AreEqual(generation, m.Generation);
            Assert.IsTrue(m.Hives[0].Started);
            Assert.IsFalse(m.Paused);
            Assert.IsNull(app.GetUtility<UiPopupRouter>().ActiveView);
        }

        [UnityTest]
        public IEnumerator HeldTouchCannotRepressQueueAfterThePointerCacheIsCleared()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var level = UnityEngine.Object.FindObjectOfType<LevelOneController>();
            var model = BeeHarvestApp.Interface.GetModel<HarvestModel>();
            var module = (StandaloneInputModule)EventSystem.current.currentInputModule;
            module.enabled = false;
            var eventSystem = EventSystem.current;
            eventSystem.enabled = false;
            var fake = module.gameObject.AddComponent<SettingsTouchInput>();
            module.inputOverride = fake;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var read = typeof(PointerInputModule).GetMethod("GetTouchPointerEventData", flags);
            var process = typeof(StandaloneInputModule).GetMethod("ProcessTouchPress", flags);
            var point = RectTransformUtility.WorldToScreenPoint(null, level.Hud.QueueButtons[0].transform.position);
            foreach (bool viaRestart in new[] { false, true })
            {
                fake.Count = 1;
                var args = new object[] { new Touch { fingerId = 17, position = point, phase = TouchPhase.Began }, false, false };
                var pointer = (PointerEventData)read.Invoke(module, args);
                process.Invoke(module, new object[] { pointer, args[1], args[2] });
                Gear().onClick.Invoke();
                if (viaRestart) GameObject.Find("RetryButton").GetComponent<Button>().onClick.Invoke();
                else GameObject.Find("CloseButton").GetComponent<Button>().onClick.Invoke();
                args = new object[] { new Touch { fingerId = 17, position = point, phase = TouchPhase.Stationary }, false, false };
                pointer = (PointerEventData)read.Invoke(module, args);
                Assert.IsTrue((bool)args[1], "Unity recreates a cleared stationary touch as a press");
                process.Invoke(module, new object[] { pointer, args[1], args[2] });
                yield return null;
                Assert.IsFalse(level.Hud.QueueButtons[0].interactable, "A held old touch must keep the queue blocked");
                args = new object[] { new Touch { fingerId = 17, position = point, phase = TouchPhase.Ended }, false, false };
                pointer = (PointerEventData)read.Invoke(module, args);
                process.Invoke(module, new object[] { pointer, args[1], args[2] });
                fake.Count = 0;
                yield return new WaitForSeconds(.42f);
                Assert.IsFalse(model.Hives[0].Started);
                Assert.AreEqual(40, model.Hives[0].Remaining);
            }
            module.inputOverride = null;
            module.enabled = true;
            eventSystem.enabled = true;
            yield return HiveInputFixture.Click(level.Hud, 0);
            Assert.IsTrue(model.Hives[0].Started);
        }

        [UnityTest]
        public IEnumerator PreModalPressCannotDispatchAfterCloseOrRestart()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var level = UnityEngine.Object.FindObjectOfType<LevelOneController>();
            var model = BeeHarvestApp.Interface.GetModel<HarvestModel>();
            var module = (StandaloneInputModule)EventSystem.current.currentInputModule;
            module.enabled = false; // Drive the installed module's real mouse processing deterministically.
            var method = typeof(StandaloneInputModule).GetMethod("ProcessMousePress", BindingFlags.Instance | BindingFlags.NonPublic);
            var cache = (Dictionary<int, PointerEventData>)typeof(PointerInputModule)
                .GetField("m_PointerData", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(module);
            foreach (bool viaRestart in new[] { false, true })
            {
                var target = level.Hud.QueueButtons[0].gameObject;
                var pointer = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = RectTransformUtility.WorldToScreenPoint(null, target.transform.position),
                    pointerCurrentRaycast = new RaycastResult { gameObject = target }
                };
                cache[-1] = pointer;
                method.Invoke(module, new object[] { new PointerInputModule.MouseButtonEventData
                    { buttonData = pointer, buttonState = PointerEventData.FramePressState.Pressed } });
                Assert.AreEqual(target, pointer.pointerClick, "The pre-modal press must first be cached by Unity");
                Gear().onClick.Invoke();
                if (viaRestart) GameObject.Find("RetryButton").GetComponent<Button>().onClick.Invoke();
                else GameObject.Find("CloseButton").GetComponent<Button>().onClick.Invoke();
                // A physical mouse-up uses the module's current pointer cache, not the stale object.
                if (!cache.TryGetValue(-1, out pointer))
                    pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left,
                        pointerCurrentRaycast = new RaycastResult { gameObject = target } };
                method.Invoke(module, new object[] { new PointerInputModule.MouseButtonEventData
                    { buttonData = pointer, buttonState = PointerEventData.FramePressState.Released } });
                yield return new WaitForSeconds(.42f);
                Assert.IsFalse(model.Hives[0].Started, "Releasing a pre-modal press cannot launch a hive");
                Assert.AreEqual(40, model.Hives[0].Remaining);
            }
            module.enabled = true;
            yield return HiveInputFixture.Click(level.Hud, 0);
            Assert.IsTrue(model.Hives[0].Started, "Fresh clicks remain usable");
        }

        [UnityTest]
        public IEnumerator DisabledModalCannotOpenAndReenableRestoresRegistration()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var popup = UnityEngine.Object.FindObjectOfType<SettingsPopupController>();
            var router = BeeHarvestApp.Interface.GetUtility<UiPopupRouter>();
            var model = BeeHarvestApp.Interface.GetModel<HarvestModel>();
            foreach (bool root in new[] { false, true })
            {
                if (root) popup.gameObject.SetActive(false); else popup.enabled = false;
                Gear().onClick.Invoke();
                Assert.IsFalse(popup.IsOpen);
                Assert.IsFalse(model.Paused);
                Assert.IsNull(router.ActiveView);
                Assert.IsFalse(router.Show(popup.PopupId, UiPopupOpenMode.Replace));
                if (root) popup.gameObject.SetActive(true); else popup.enabled = true;
                Gear().onClick.Invoke();
                Assert.IsTrue(popup.IsOpen);
                popup.Close();
                Assert.IsFalse(model.Paused);
            }
        }

        [UnityTest]
        public IEnumerator BackgroundAndSettingsReleaseTheirOwnPauses()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            var level = UnityEngine.Object.FindObjectOfType<LevelOneController>();
            var popup = UnityEngine.Object.FindObjectOfType<SettingsPopupController>();
            var model = BeeHarvestApp.Interface.GetModel<HarvestModel>();
            Gear().onClick.Invoke();
            level.SendMessage("OnApplicationPause", true);
            popup.Close();
            Assert.IsTrue(model.Paused, "Closing settings in background must defer resume");
            level.SendMessage("OnApplicationPause", false);
            Assert.IsFalse(model.Paused);
            Gear().onClick.Invoke();
            level.SendMessage("OnApplicationPause", true);
            level.SendMessage("OnApplicationPause", false);
            Assert.IsTrue(model.Paused, "Returning to the app cannot release the modal pause");
            popup.Close();
            Assert.IsFalse(model.Paused);
            level.SendMessage("OnApplicationPause", true);
            Gear().onClick.Invoke();
            Assert.IsFalse(popup.IsOpen, "A backgrounded level refuses modal entry");
            var router = BeeHarvestApp.Interface.GetUtility<UiPopupRouter>();
            router.Show(popup.PopupId, UiPopupOpenMode.Replace);
            Assert.IsNull(router.ActiveView, "A refused Router request cannot leave an invisible active view");
            level.SendMessage("OnApplicationPause", false);
            Assert.IsFalse(model.Paused);
        }

        [UnityTest]
        public IEnumerator SceneReloadUnregistersTheOldPopupAndKeepsASingleButton()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            Gear().onClick.Invoke();
            yield return null;
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            Assert.IsNull(BeeHarvestApp.Interface.GetUtility<UiPopupRouter>().ActiveView);
            Assert.AreEqual(1, UnityEngine.Object.FindObjectsOfType<Button>().Count(b => b.name == "SettingsButton"));
            Gear().onClick.Invoke();
            yield return null;
            Assert.IsNotNull(BeeHarvestApp.Interface.GetUtility<UiPopupRouter>().ActiveView);
        }
    }
}
