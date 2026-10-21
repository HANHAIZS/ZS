using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class MainMenuFlowTests : EconomyTestFixture {
  [UnityTest] public IEnumerator ConfirmedLeaveResetsAndStartUsesTheSameLevelScene(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();
   var level=Object.FindObjectOfType<LevelOneController>();var hud=level.Hud;
   yield return HiveInputFixture.Click(hud,0);yield return new WaitForSeconds(.5f);
   for(int cycle=0;cycle<3;cycle++){
    var settings=Object.FindObjectOfType<SettingsPopupController>();settings.OpenButton.onClick.Invoke();yield return null;
    Assert.IsTrue(settings.HomeButton.interactable,"Settings Home must now return to the supplied main menu");
    int generation=m.Generation,remaining=m.Remaining,deposited=m.Deposited,flightCount=m.Flights.Count;
    int quota=m.Hives[0].Remaining;float[] progress=m.Flights.Select(f=>f.Progress).ToArray();
    settings.HomeButton.onClick.Invoke();settings.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
    Assert.IsFalse(settings.IsOpen);Assert.IsTrue(m.Paused);Assert.IsFalse(hud.gameObject.activeSelf);
    var page=GameObject.Find("MainMenuPage");Assert.IsNotNull(page);
    var start=page.GetComponentsInChildren<Button>().Single(b=>b.name=="StartGameButton");
    Assert.AreEqual("Level 1",start.GetComponentInChildren<TMP_Text>().text);
    Assert.IsTrue(page.GetComponentsInChildren<TMP_Text>().All(t=>t.font.name=="FredokaBoldSDF"));
    var p=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,hud.QueueButtons[0].transform.position)};
    var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);
    Assert.IsTrue(hits.Count>0);Assert.IsFalse(hits.Any(h=>h.gameObject.name.StartsWith("HiveColumn_")),"Hidden level must not receive home taps");
    yield return new WaitForSeconds(.35f);
    Assert.AreEqual(generation+1,m.Generation);Assert.AreEqual(1784,m.Remaining);Assert.AreEqual(0,m.Deposited);
    Assert.AreEqual(40,m.Hives[0].Remaining);Assert.AreEqual(0,m.Flights.Count);Assert.IsTrue(m.Hives.All(h=>!h.Started));
    start.onClick.Invoke();Assert.IsFalse(m.Paused);Assert.IsTrue(hud.gameObject.activeSelf);
    Assert.AreEqual(generation+1,m.Generation);Assert.AreEqual(1784,m.Remaining);Assert.AreEqual(0,m.Flights.Count);
    Assert.IsFalse(page.activeSelf);yield return null;
   }
   HiveInputFixture.RetryLevelForTest();yield return null;
   Assert.AreEqual(1784,m.Remaining);Assert.IsTrue(hud.gameObject.activeSelf);Assert.IsFalse(m.Paused);
  }
  [UnityTest] public IEnumerator ConfirmedLeaveClearsManualPauseAndStartHonorsBackgroundPause(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();
   var level=Object.FindObjectOfType<LevelOneController>();var settings=Object.FindObjectOfType<SettingsPopupController>();
   app.SendCommand(new PauseHarvestCommand(true));settings.OpenButton.onClick.Invoke();
   Assert.IsTrue(settings.HomeButton.interactable);settings.HomeButton.onClick.Invoke();settings.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
   var start=GameObject.Find("StartGameButton").GetComponent<Button>();start.onClick.Invoke();yield return null;
   Assert.IsFalse(m.Paused,"Starting the reset level must clear the old manual pause");
   app.SendCommand(new PauseHarvestCommand(false));settings.OpenButton.onClick.Invoke();settings.HomeButton.onClick.Invoke();settings.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
   start=GameObject.Find("StartGameButton").GetComponent<Button>();
   level.SendMessage("OnApplicationPause",true);start.onClick.Invoke();yield return null;
   Assert.IsTrue(m.Paused);Assert.IsTrue(start.gameObject.activeInHierarchy);
   level.SendMessage("OnApplicationPause",false);yield return null;start.onClick.Invoke();yield return null;
   Assert.IsFalse(m.Paused);Assert.IsTrue(level.Hud.gameObject.activeSelf);
  }
  [UnityTest] public IEnumerator NewLaunchShowsHomeAndStartLoadsFirstLevelOnce(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   var page=GameObject.Find("MainMenuPage");Assert.IsNotNull(page);
   var start=page.GetComponentsInChildren<Button>().Single(b=>b.name=="StartGameButton");
   Assert.IsTrue(page.GetComponentsInChildren<TMP_Text>().All(t=>t.font.name=="FredokaBoldSDF"));
   start.onClick.Invoke();start.onClick.Invoke();Assert.IsFalse(start.interactable);
   float limit=Time.realtimeSinceStartup+4;
   while(SceneManager.GetActiveScene().name!="Level01"&&Time.realtimeSinceStartup<limit)yield return null;
   yield return null;yield return null;
   Assert.AreEqual("Level01",SceneManager.GetActiveScene().name);
   Assert.AreEqual(1,Object.FindObjectsOfType<LevelOneController>().Length);
   Assert.AreEqual(1784,BeeHarvestApp.Interface.GetModel<HarvestModel>().Remaining);
   Assert.IsNull(GameObject.Find("MainMenuPage"),"Page is cached but hidden in a new level");
  }

  [UnityTest] public IEnumerator HeldTouchAcrossStartCannotActivateWaitingHive(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;
   var level=Object.FindObjectOfType<LevelOneController>();var model=BeeHarvestApp.Interface.GetModel<HarvestModel>();
   var popup=Object.FindObjectOfType<SettingsPopupController>();popup.OpenButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();popup.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
   var eventSystem=EventSystem.current;var module=(StandaloneInputModule)eventSystem.currentInputModule;module.enabled=false;eventSystem.enabled=false;
   var fake=module.gameObject.AddComponent<SettingsTouchInput>();fake.Count=1;module.inputOverride=fake;
   GameObject.Find("StartGameButton").GetComponent<Button>().onClick.Invoke();yield return null;
   Assert.IsFalse(level.Hud.QueueButtons[0].interactable,"Old finger must keep the queue blocked after Start");
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;
   var read=typeof(PointerInputModule).GetMethod("GetTouchPointerEventData",flags);var process=typeof(StandaloneInputModule).GetMethod("ProcessTouchPress",flags);
   var point=RectTransformUtility.WorldToScreenPoint(null,level.Hud.QueueButtons[0].transform.position);
   var args=new object[]{new Touch{fingerId=19,position=point,phase=TouchPhase.Stationary},false,false};
   var pointer=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{pointer,args[1],args[2]});yield return null;
   args=new object[]{new Touch{fingerId=19,position=point,phase=TouchPhase.Ended},false,false};
   pointer=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{pointer,args[1],args[2]});fake.Count=0;
   yield return new WaitForSeconds(.4f);Assert.IsTrue(model.Hives.All(h=>!h.Started));Assert.AreEqual(40,model.Hives[0].Remaining);
   module.inputOverride=null;module.enabled=true;eventSystem.enabled=true;yield return HiveInputFixture.Click(level.Hud,0);Assert.IsTrue(model.Hives[0].Started);
  }

  [UnityTest] public IEnumerator FirstLaunchHeldTouchCannotActivateNewLevelHive(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   SettingsTouchInput fake=null;StandaloneInputModule module=null;
   UnityEngine.Events.UnityAction<Scene,LoadSceneMode> inject=(scene,mode)=>{
    if(scene.name!="Level01")return;
    module=Object.FindObjectOfType<StandaloneInputModule>();
    fake=module.gameObject.AddComponent<SettingsTouchInput>();fake.Count=1;
    Canvas.ForceUpdateCanvases();var level=Object.FindObjectOfType<LevelOneController>();
    fake.Value=new Touch{fingerId=27,position=RectTransformUtility.WorldToScreenPoint(null,level.Hud.QueueButtons[0].transform.position),phase=TouchPhase.Stationary};
    module.inputOverride=fake;
   };
   SceneManager.sceneLoaded+=inject;
   try{
    GameObject.Find("StartGameButton").GetComponent<Button>().onClick.Invoke();
    float limit=Time.realtimeSinceStartup+4;
    while(SceneManager.GetActiveScene().name!="Level01"&&Time.realtimeSinceStartup<limit)yield return null;
    yield return null;yield return null;
    Assert.IsNotNull(fake,"Inject held finger before the new level starts");
    var level=Object.FindObjectOfType<LevelOneController>();var model=BeeHarvestApp.Interface.GetModel<HarvestModel>();
    Assert.IsFalse(level.Hud.QueueButtons[0].interactable,"New level must block fingers carried across the scene load");
    fake.Value=new Touch{fingerId=27,position=fake.Value.position,phase=TouchPhase.Ended};yield return null;
    fake.Count=0;yield return new WaitForSeconds(.4f);
    Assert.IsTrue(model.Hives.All(h=>!h.Started));Assert.AreEqual(40,model.Hives[0].Remaining);
    module.inputOverride=null;yield return HiveInputFixture.Click(level.Hud,0);Assert.IsTrue(model.Hives[0].Started);
   }finally{
    SceneManager.sceneLoaded-=inject;if(module!=null)module.inputOverride=null;if(fake!=null)Object.Destroy(fake);
   }
  }

 }
}
