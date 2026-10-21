using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class PausedDialogTests : EconomyTestFixture {
  static IEnumerator Load(string scene){SceneManager.LoadScene(scene);yield return null;yield return null;}
  [UnityTest] public IEnumerator RestartResetsTheCurrentLevelAndChargesOnlyOnce(){
   foreach(var scene in new[]{"Level01","Level02","Level03","Level04","Level05"}) {
    yield return Load(scene);var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var level=Object.FindObjectOfType<LevelOneController>();var popup=Object.FindObjectOfType<SettingsPopupController>();
    Assert.IsNotNull(popup.RetryButton,"Paused requires a working Restart entry");Assert.IsNull(popup.ContinueButton);
    yield return HiveInputFixture.Click(level.Hud,0);Assert.IsTrue(model.Hives[0].Started);
    int generation=model.Generation,energy=app.GetModel<EconomyModel>().Stamina;
    popup.OpenButton.onClick.Invoke();yield return null;Assert.IsTrue(model.Paused);
    popup.RetryButton.onClick.Invoke();popup.RetryButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
    Assert.AreEqual(generation+1,model.Generation);Assert.AreEqual(new[]{1784,2190,812,1105,2840}[level.LevelNumber-1],model.Remaining);Assert.IsTrue(model.Slots.All(s=>s<0));Assert.AreEqual(0,model.Flights.Count);
    Assert.AreEqual(energy-1,app.GetModel<EconomyModel>().Stamina);Assert.IsFalse(popup.IsOpen);Assert.IsFalse(model.Paused);Assert.AreEqual(scene,SceneManager.GetActiveScene().name);
    Assert.AreEqual(popup,Object.FindObjectOfType<VictoryPopupController>().SettingsPopup);
   }
  }
  [UnityTest] public IEnumerator RestartAtZeroKeepsTheModalAndBecomesAvailableAfterRecovery(){
   yield return Load("Level02");var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var economy=app.GetModel<EconomyModel>();var popup=Object.FindObjectOfType<SettingsPopupController>();Assert.IsNotNull(popup.RetryButton);
   for(int i=0;i<5;i++)app.SendCommand(new SpendStaminaCommand());Assert.AreEqual(0,economy.Stamina);int generation=model.Generation;
   popup.OpenButton.onClick.Invoke();yield return null;Assert.IsFalse(popup.RetryButton.interactable);popup.RetryButton.onClick.Invoke();Assert.IsTrue(popup.IsOpen);Assert.AreEqual(generation,model.Generation);
   app.SendCommand(new AddStaminaCommand());yield return new WaitForSecondsRealtime(1.1f);Assert.IsTrue(popup.RetryButton.interactable);
   popup.RetryButton.onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.AreEqual(generation+1,model.Generation);Assert.AreEqual(0,economy.Stamina);Assert.IsFalse(popup.IsOpen);
  }
  [UnityTest] public IEnumerator BackgroundAndDisabledModalCannotRestartOrCharge(){
   yield return Load("Level01");var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var economy=app.GetModel<EconomyModel>();var popup=Object.FindObjectOfType<SettingsPopupController>();Assert.IsNotNull(popup.RetryButton);int generation=model.Generation,energy=economy.Stamina;
   popup.OpenButton.onClick.Invoke();yield return null;popup.Level.SendMessage("OnApplicationPause",true);popup.SendMessage("OnApplicationPause",true);
   popup.RetryButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();Assert.IsTrue(popup.IsOpen);Assert.AreEqual(generation,model.Generation);Assert.AreEqual(energy,economy.Stamina);
   popup.Level.SendMessage("OnApplicationPause",false);popup.SendMessage("OnApplicationPause",false);popup.enabled=false;popup.RetryButton.onClick.Invoke();Assert.AreEqual(generation,model.Generation);Assert.AreEqual(energy,economy.Stamina);
   popup.enabled=true;popup.OpenButton.onClick.Invoke();yield return null;popup.RetryButton.onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.AreEqual(generation+1,model.Generation);Assert.AreEqual(energy-1,economy.Stamina);
  }
  [UnityTest] public IEnumerator ReturningFromBackgroundRejectsOldTouchOnRestartAndQuit(){
   foreach(bool quit in new[]{false,true}) {
    yield return Load("Level02");var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var economy=app.GetModel<EconomyModel>();var popup=Object.FindObjectOfType<SettingsPopupController>();
    popup.OpenButton.onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();
    var events=EventSystem.current;var module=events.GetComponent<StandaloneInputModule>();var fake=module.gameObject.AddComponent<SettingsTouchInput>();module.enabled=false;module.inputOverride=fake; // Keep the real EventSystem active when Quit reveals the cached menu.
    try {
     var target=quit?popup.HomeButton:popup.RetryButton;var point=RectTransformUtility.WorldToScreenPoint(null,target.transform.position);
     var flags=BindingFlags.Instance|BindingFlags.NonPublic;var read=typeof(PointerInputModule).GetMethod("GetTouchPointerEventData",flags);var process=typeof(StandaloneInputModule).GetMethod("ProcessTouchPress",flags);
     System.Action<TouchPhase> touch=phase=>{var args=new object[]{new Touch{fingerId=27,position=point,phase=phase},false,false};var data=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{data,args[1],args[2]});};
     int generation=model.Generation,energy=economy.Stamina;fake.Count=1;touch(TouchPhase.Began);
     popup.Level.SendMessage("OnApplicationPause",true);popup.SendMessage("OnApplicationPause",true);popup.Level.SendMessage("OnApplicationPause",false);popup.SendMessage("OnApplicationPause",false);
     touch(TouchPhase.Stationary);touch(TouchPhase.Ended);
     Assert.IsTrue(popup.IsOpen,"Recreated old touch cannot invoke Restart or Quit");Assert.AreEqual(generation,model.Generation);Assert.AreEqual(energy,economy.Stamina);Assert.IsFalse(app.GetModel<MenuNavigationModel>().IsHome);
     fake.Count=0;yield return null;yield return null;fake.Count=1;touch(TouchPhase.Began);touch(TouchPhase.Ended);fake.Count=0;yield return null;
     if(quit){Assert.IsTrue(popup.IsLeaving);Assert.AreEqual(energy,economy.Stamina);yield return null;popup.LeaveButton.onClick.Invoke();}
     yield return PopupMotionTestWait.Close();Assert.IsFalse(popup.IsOpen);Assert.AreEqual(energy-1,economy.Stamina);
     if(quit)Assert.IsTrue(app.GetModel<MenuNavigationModel>().IsHome);else Assert.AreEqual(generation+1,model.Generation);
    } finally {module.inputOverride=null;module.enabled=true;events.enabled=true;Object.Destroy(fake);}
   }
  }
 }
}
