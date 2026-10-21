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
    public sealed class VictorySettlementTests : EconomyTestFixture
    {
        static HarvestModel Model => BeeHarvestApp.Interface.GetModel<HarvestModel>();
        static Image Artwork() => Object.FindObjectsOfType<Image>(true).SingleOrDefault(i => i.name == "VictoryArtwork");
        static Button ContinueButton() => Object.FindObjectOfType<VictoryPopupController>(true).ContinueButton;
        static MonoBehaviour Popup() => Object.FindObjectsOfType<MonoBehaviour>(true).Single(c => c.GetType().Name == "VictoryPopupController");

        static IEnumerator WaitForSecondLevel()
        {
            float limit=Time.realtimeSinceStartup+5;
            while(SceneManager.GetActiveScene().name!="Level02"&&Time.realtimeSinceStartup<limit)yield return null;
            yield return null;yield return null;
            Assert.AreEqual("Level02",SceneManager.GetActiveScene().name);
        }

        // Actual Commands drive the model; no direct Won assignment or fake popup.
        static void StartOneCube(LevelOneController level)
        {
            level.enabled = false;
            BeeHarvestApp.Interface.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
            BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(new LevelData {
                cubes = new[] { new CubeData { id = 0, x = 0, y = 0, z = 0, color = "red" } },
                hives = new[] { new HiveData { column = 0, color = "red", quota = 1 } }
            }));
            BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));
            BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+.01f));
            Assert.AreEqual(0, Model.Remaining);
            Assert.AreEqual(1, Model.InTransit);
            Assert.IsFalse(Model.Won);
        }

        static void DeliverLastCube()
        {
            BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.LoadedSeconds+.01f));
            Assert.IsTrue(Model.Won);
            Assert.AreEqual(0, Model.InTransit);
        }

        static PointerEventData ProcessTouch(StandaloneInputModule module, Vector2 position, TouchPhase phase, int id)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var args = new object[] { new Touch { fingerId = id, position = position, phase = phase }, false, false };
            var pointer = (PointerEventData)typeof(PointerInputModule).GetMethod("GetTouchPointerEventData", flags).Invoke(module, args);
            typeof(StandaloneInputModule).GetMethod("ProcessTouchPress", flags).Invoke(module, new object[] { pointer, args[1], args[2] });
            return pointer;
        }

        [UnityTest]
        public IEnumerator EntryAndBackgroundTouchesMustEndBeforeContinueAcceptsAFreshPress()
        {
            foreach (bool background in new[] { false, true }) {
                SceneManager.LoadScene("Level01"); yield return null; yield return null;
                var level = Object.FindObjectOfType<LevelOneController>();
                StartOneCube(level); DeliverLastCube(); yield return null; yield return null;
                Assert.IsNotNull(Artwork(), "Winning supplies a real Continue button");
                var events = EventSystem.current; var module = events.GetComponent<StandaloneInputModule>();
                var input = module.gameObject.AddComponent<SettingsTouchInput>();
                module.enabled = false; events.enabled = false; module.inputOverride = input;
                int generation = Model.Generation;
                try {
                    if (background) {
                        yield return new WaitForSecondsRealtime(.65f);
                        level.SendMessage("OnApplicationPause", true); Popup().SendMessage("OnApplicationPause", true);
                    }
                    input.Count = 1;
                    if (background) {
                        level.SendMessage("OnApplicationPause", false); Popup().SendMessage("OnApplicationPause", false);
                    }
                    Canvas.ForceUpdateCanvases();
                    var point = RectTransformUtility.WorldToScreenPoint(null, ContinueButton().transform.position);
                    var pointer = ProcessTouch(module, point, TouchPhase.Stationary, 31);
                    Assert.AreEqual(ContinueButton().gameObject, pointer.pointerPress, "uGUI caches a stationary/entry press even while the button is disabled");
                    yield return new WaitForSecondsRealtime(.65f);
                    Assert.IsFalse(ContinueButton().interactable, "Entry or background finger must end before Continue is enabled");
                    ProcessTouch(module, point, TouchPhase.Ended, 31);
                    Assert.AreEqual(generation, Model.Generation, "Releasing that old finger must not restart");
                    input.Count = 0;
                    float freshDeadline=Time.realtimeSinceStartup+3;
                    while(!ContinueButton().interactable&&Time.realtimeSinceStartup<freshDeadline)yield return null;
                    Assert.IsTrue(ContinueButton().interactable,"Fresh pointers are accepted after entry animation finishes");
                    ProcessTouch(module, point, TouchPhase.Began, 32);
                    ProcessTouch(module, point, TouchPhase.Ended, 32);
                    yield return PopupMotionTestWait.Close();
                    Assert.AreEqual(2,BeeHarvestApp.Interface.GetModel<MenuNavigationModel>().CurrentLevel,"Only a subsequent fresh press advances");
                } finally { module.inputOverride = null; module.enabled = true; events.enabled = true; Object.Destroy(input); }
                yield return WaitForSecondLevel();
                Assert.AreEqual(generation+1,Model.Generation);
            }
        }

        [UnityTest]
        public IEnumerator VictoryReplacesSettingsOpenedInTheFinalDeliveryFrame()
        {
            SceneManager.LoadScene("Level01"); yield return null; yield return null;
            var level = Object.FindObjectOfType<LevelOneController>();
            StartOneCube(level); DeliverLastCube();
            var settings = Object.FindObjectOfType<SettingsPopupController>();
            settings.OpenButton.onClick.Invoke(); Assert.IsTrue(settings.IsOpen);
            yield return null; yield return null;
            Assert.IsNotNull(Artwork(), "Winning supplies the settlement");
            Assert.IsFalse(settings.IsOpen, "Victory must release the previous settings modal before owning input");
            Assert.IsNull(BeeHarvestApp.Interface.GetUtility<UiPopupRouter>().ActiveView);
            yield return new WaitForSecondsRealtime(.65f);
            ContinueButton().onClick.Invoke(); yield return WaitForSecondLevel();
            settings=Object.FindObjectOfType<SettingsPopupController>();
            Assert.IsFalse(settings.IsOpen); Assert.IsFalse(Model.Paused);
            Assert.IsTrue(settings.OpenButton.interactable);
            settings.OpenButton.onClick.Invoke(); Assert.IsTrue(settings.IsOpen); Assert.IsTrue(Model.Paused);
        }

        [UnityTest]
        public IEnumerator SettlementWaitsForFinalDeliveryThenAnimatesAndBlocksLevel()
        {
            SceneManager.LoadScene("Level01"); yield return null; yield return null;
            var level = Object.FindObjectOfType<LevelOneController>();
            StartOneCube(level); yield return null;
            Assert.IsTrue(Artwork() == null || !Artwork().gameObject.activeInHierarchy, "No victory while the final bee is still carrying");
            DeliverLastCube(); yield return null;
            Assert.IsNotNull(Artwork(), "Winning must display the supplied victory artwork");
            Assert.IsTrue(Artwork().gameObject.activeInHierarchy);
            var content = Artwork().GetComponentInParent<CanvasGroup>();
            Assert.Less(content.alpha, 1f, "Entry starts with a fade instead of a jump");
            Assert.IsFalse(ContinueButton().interactable, "Entry ignores stale/accidental Continue taps");
            var settings = Object.FindObjectOfType<SettingsPopupController>();
            // Newly enabled uGUI graphics receive their render depth at the end of this frame.
            yield return null;
            Canvas.ForceUpdateCanvases();
            var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, level.Hud.QueueButtons[0].transform.position) };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Assert.Greater(hits.Count, 0, "Active settlement graphics must participate in the next frame's raycast");
            Assert.IsTrue(hits[0].gameObject.transform.IsChildOf(Popup().transform), "The victory overlay owns taps outside the content too");
            // Animation caps its frame delta; wall time alone is insufficient on a busy editor.
            float entryDeadline=Time.realtimeSinceStartup+3;
            while(!ContinueButton().interactable&&Time.realtimeSinceStartup<entryDeadline)yield return null;
            Assert.AreEqual(1f, content.alpha, .001f);
            Assert.IsTrue(ContinueButton().interactable);
            Assert.AreEqual("continue", ContinueButton().GetComponentInChildren<TMP_Text>().text);
            Assert.AreEqual("FredokaBoldSDF", ContinueButton().GetComponentInChildren<TMP_Text>().font.name);
            var dim = Popup().GetComponentsInChildren<Image>().Single(i => i.name == "DimBackground");
            Assert.That(dim.color.a, Is.InRange(.60f, .70f), "Black overlay must preserve the level behind it");
            Assert.IsFalse(settings.OpenButton.interactable);
        }

        [UnityTest]
        public IEnumerator ContinueEntersSecondLevelOncePreservesInventoryAndCanShowTheNextWin()
        {
            SceneManager.LoadScene("Level01"); yield return null; yield return null;
            var level = Object.FindObjectOfType<LevelOneController>();
            StartOneCube(level);
            BeeHarvestApp.Interface.SendCommand(new RefillHarvestBoosterCommand(1));
            BeeHarvestApp.Interface.SendCommand(new UseHarvestSpeedCommand());
            int[] inventory = Enumerable.Range(0, 4).Select(Model.BoosterCount).ToArray();
            DeliverLastCube(); yield return null;
            Assert.IsNotNull(Artwork(), "Victory supplies a Continue action");
            yield return new WaitForSecondsRealtime(.65f);
            int generation = Model.Generation;
            var button = ContinueButton();
            button.onClick.Invoke(); button.onClick.Invoke();
            float limit = Time.realtimeSinceStartup + 4;
            while (SceneManager.GetActiveScene().name != "Level02" && Time.realtimeSinceStartup < limit) yield return null;
            yield return null; yield return null;
            Assert.AreEqual("Level02", SceneManager.GetActiveScene().name);
            level = Object.FindObjectOfType<LevelOneController>();
            Assert.AreEqual(generation + 1, Model.Generation, "Double taps cannot advance twice");
            Assert.AreEqual(2190, Model.Remaining); Assert.AreEqual(0, Model.Deposited);
            Assert.IsFalse(Model.Won); Assert.IsFalse(Artwork().gameObject.activeInHierarchy);
            CollectionAssert.AreEqual(inventory, Enumerable.Range(0, 4).Select(Model.BoosterCount).ToArray());
            Assert.AreEqual(0f, Model.SpeedSeconds);
            Assert.IsFalse(BeeHarvestApp.Interface.GetModel<MenuNavigationModel>().IsHome);
            Assert.IsTrue(Object.FindObjectOfType<SettingsPopupController>().OpenButton.interactable);
            StartOneCube(level); DeliverLastCube(); yield return null;
            Assert.IsTrue(Artwork().gameObject.activeInHierarchy, "A new generation must show its own settlement");
        }

        [UnityTest]
        public IEnumerator ContinueKeepsOldSettlementClosedDuringAsyncLoad()
        {
            SceneManager.LoadScene("Level01");yield return null;yield return null;
            var level=Object.FindObjectOfType<LevelOneController>();StartOneCube(level);DeliverLastCube();yield return null;
            var popup=Object.FindObjectOfType<VictoryPopupController>();float limit=Time.realtimeSinceStartup+3;
            while(!popup.ContinueButton.interactable&&Time.realtimeSinceStartup<limit)yield return null;
            Assert.IsTrue(popup.ContinueButton.interactable);popup.ContinueButton.onClick.Invoke();
            Assert.IsTrue(popup.IsOpen,"The closing animation still owns the modal");
            yield return PopupMotionTestWait.Close();
            Assert.IsFalse(popup.IsOpen);
            popup.SendMessage("Update"); // Old Canvas sibling can update before the deferred scene load.
            Assert.IsFalse(popup.IsOpen,"A consumed victory must stay closed until scene teardown");
            yield return WaitForSecondLevel();
        }

        [UnityTest]
        public IEnumerator BackgroundPausesEntryAndRefusesContinue()
        {
            SceneManager.LoadScene("Level01"); yield return null; yield return null;
            var level = Object.FindObjectOfType<LevelOneController>();
            StartOneCube(level); DeliverLastCube(); yield return null;
            Assert.IsNotNull(Artwork(), "Winning creates the animated modal");
            var popup = Popup(); var content = Artwork().GetComponentInParent<CanvasGroup>();
            level.SendMessage("OnApplicationPause", true); popup.SendMessage("OnApplicationPause", true);
            float alpha = content.alpha; int generation = Model.Generation;
            yield return new WaitForSecondsRealtime(.65f);
            Assert.AreEqual(alpha, content.alpha, .001f);
            ContinueButton().onClick.Invoke(); Assert.AreEqual(generation, Model.Generation);
            level.SendMessage("OnApplicationPause", false); popup.SendMessage("OnApplicationPause", false);
            yield return new WaitForSecondsRealtime(.65f);
            Assert.IsTrue(ContinueButton().interactable);
        }

        [UnityTest]
        public IEnumerator RetryAndSceneReloadRemoveOldSettlement()
        {
            SceneManager.LoadScene("Level01"); yield return null; yield return null;
            var level = Object.FindObjectOfType<LevelOneController>();
            StartOneCube(level); DeliverLastCube(); yield return null;
            Assert.IsNotNull(Artwork(), "Winning creates the modal");
            level.RestartLevel(); yield return null;
            Assert.IsFalse(Artwork().gameObject.activeInHierarchy);
            StartOneCube(level); DeliverLastCube(); yield return null;
            SceneManager.LoadScene("Level01"); yield return null; yield return null;
            Assert.IsFalse(Artwork().gameObject.activeInHierarchy);
            Assert.AreEqual(1, Object.FindObjectOfType<VictoryPopupController>(true).GetComponentsInChildren<Button>(true).Count(b => b.name == "ContinueButton"));
            Assert.IsTrue(Object.FindObjectOfType<LevelOneController>().Hud.QueueButtons[0].interactable);
        }

        [UnityTest]
        public IEnumerator HeldTouchAfterContinueCannotActivateAHive()
        {
            SceneManager.LoadScene("Level01"); yield return null; yield return null;
            var level=Object.FindObjectOfType<LevelOneController>();
            StartOneCube(level);DeliverLastCube();yield return null;yield return new WaitForSecondsRealtime(.65f);
            SettingsTouchInput input=null;StandaloneInputModule module=null;
            UnityEngine.Events.UnityAction<Scene,LoadSceneMode> inject=(scene,mode)=>{
                if(scene.name!="Level02")return;
                module=Object.FindObjectOfType<StandaloneInputModule>();
                input=module.gameObject.AddComponent<SettingsTouchInput>();input.Count=1;
                input.Value=new Touch{fingerId=51,phase=TouchPhase.Stationary,position=new Vector2(10,10)};
                module.inputOverride=input;
            };
            SceneManager.sceneLoaded+=inject;
            try{
                ContinueButton().onClick.Invoke();yield return WaitForSecondLevel();
                level=Object.FindObjectOfType<LevelOneController>();
                Assert.IsNotNull(input);Assert.IsFalse(level.Hud.QueueButtons[0].interactable,"Old finger holds the new scene input gate");
                var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,level.Hud.QueueButtons[0].transform.position)};
                ExecuteEvents.Execute(level.Hud.QueueButtons[0].gameObject,pointer,ExecuteEvents.pointerClickHandler);
                yield return new WaitForSeconds(.4f);Assert.IsTrue(Model.Hives.All(h=>!h.Started));
                input.Value=new Touch{fingerId=51,phase=TouchPhase.Ended,position=input.Value.position};yield return null;
                input.Count=0;yield return null;yield return null;
                Assert.IsTrue(level.Hud.QueueButtons[0].interactable,"A fresh press can resume after old pointers finish");
            }finally{SceneManager.sceneLoaded-=inject;if(module!=null)module.inputOverride=null;if(input!=null)Object.Destroy(input);}
        }
        [UnityTest] public IEnumerator PendingContinueIsCancelledByDisableBackgroundAndNewGeneration()
        {
            SceneManager.LoadScene("Level01");yield return null;yield return null;
            var level=Object.FindObjectOfType<LevelOneController>();StartOneCube(level);DeliverLastCube();yield return null;
            var popup=Object.FindObjectOfType<VictoryPopupController>();yield return PopupMotionTestWait.Open(PopupMotionTestWait.Motion(popup));yield return null;
            int generation=Model.Generation;popup.ContinueButton.onClick.Invoke();Assert.IsTrue(PopupMotionTestWait.Motion(popup).IsClosing);popup.enabled=false;
            yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(generation,Model.Generation);Assert.AreEqual("Level01",SceneManager.GetActiveScene().name);
            popup.enabled=true;yield return null;yield return PopupMotionTestWait.Open(PopupMotionTestWait.Motion(popup));yield return null;
            popup.ContinueButton.onClick.Invoke();level.SendMessage("OnApplicationPause",true);popup.SendMessage("OnApplicationPause",true);
            yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(generation,Model.Generation);Assert.IsTrue(popup.IsOpen);
            level.SendMessage("OnApplicationPause",false);popup.SendMessage("OnApplicationPause",false);yield return PopupMotionTestWait.Open(PopupMotionTestWait.Motion(popup));yield return null;
            Assert.IsTrue(popup.ContinueButton.interactable);popup.ContinueButton.onClick.Invoke();level.RestartLevel();yield return new WaitForSecondsRealtime(.2f);
            Assert.AreEqual(generation+1,Model.Generation);Assert.IsFalse(popup.IsOpen);Assert.AreEqual("Level01",SceneManager.GetActiveScene().name);
        }
    }
}
