using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;
using GameFoundation.UI;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class LeavingDialogTests : EconomyTestFixture {
  static IEnumerator Load(string scene){SceneManager.LoadScene(scene);yield return null;yield return null;}
  static T Field<T>(SettingsPopupController popup,string name) where T:class {var f=typeof(SettingsPopupController).GetField(name);Assert.IsNotNull(f,"Leaving view binding: "+name);var value=f.GetValue(popup) as T;Assert.IsNotNull(value);return value;}
  static bool Leaving(SettingsPopupController popup){var p=typeof(SettingsPopupController).GetProperty("IsLeaving");Assert.IsNotNull(p);return (bool)p.GetValue(popup);}
  [UnityTest] public IEnumerator QuitShowsConfirmationAndCancelPreservesProgressAndPauseOwnership(){
   yield return Load("Level03");var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var economy=app.GetModel<EconomyModel>();var popup=Object.FindObjectOfType<SettingsPopupController>();
   yield return HiveInputFixture.Click(popup.Level.Hud,0);int generation=model.Generation,remaining=model.Remaining,stamina=economy.Stamina;
   popup.OpenButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();yield return null;
   Assert.IsFalse(app.GetModel<MenuNavigationModel>().IsHome,"Quit must ask before leaving or spending stamina");Assert.IsTrue(popup.IsOpen);Assert.IsTrue(Leaving(popup));Assert.IsTrue(model.Paused);Assert.IsFalse(popup.PopupContent.activeSelf);Assert.IsTrue(Field<GameObject>(popup,"LeavingContent").activeSelf);Assert.AreEqual(stamina,economy.Stamina);Assert.AreEqual(generation,model.Generation);Assert.AreEqual(remaining,model.Remaining);Assert.AreSame(popup,app.GetUtility<UiPopupRouter>().ActiveView);
   popup.RetryButton.onClick.Invoke();popup.CloseButton.onClick.Invoke();Assert.IsTrue(Leaving(popup));Assert.AreEqual(generation,model.Generation);
   Field<Button>(popup,"CancelLeaveButton").onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.IsFalse(Leaving(popup));Assert.IsTrue(popup.PopupContent.activeSelf);Assert.IsTrue(model.Paused);Assert.AreEqual(stamina,economy.Stamina);Assert.AreEqual(generation,model.Generation);Assert.AreEqual(remaining,model.Remaining);Assert.IsTrue(model.Hives[0].Started);
   popup.CloseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.IsFalse(model.Paused);Assert.IsNull(app.GetUtility<UiPopupRouter>().ActiveView);
   app.SendCommand(new PauseHarvestCommand(true));popup.OpenButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();Field<Button>(popup,"CancelLeaveButton").onClick.Invoke();popup.CloseButton.onClick.Invoke();Assert.IsTrue(model.Paused,"Cancellation must preserve a preexisting manual pause");
  }
  [UnityTest] public IEnumerator LeaveResetsEachCurrentLevelOnceAndStartEntersItsBeginning(){
   int[] totals={1784,2190,812,1105,2840};
   for(int number=1;number<=5;number++){
    yield return Load("Level"+number.ToString("00"));var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var economy=app.GetModel<EconomyModel>();var popup=Object.FindObjectOfType<SettingsPopupController>();while(economy.Stamina<2)app.SendCommand(new AddStaminaCommand());
    yield return HiveInputFixture.Click(popup.Level.Hud,0);int generation=model.Generation,stamina=economy.Stamina;
    popup.OpenButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();var leave=Field<Button>(popup,"LeaveButton");leave.onClick.Invoke();leave.onClick.Invoke();yield return PopupMotionTestWait.Close();
    Assert.IsFalse(popup.IsOpen);Assert.IsFalse(Leaving(popup));Assert.IsFalse(Field<GameObject>(popup,"LeavingContent").activeSelf);Assert.IsNull(app.GetUtility<UiPopupRouter>().ActiveView);Assert.IsTrue(app.GetModel<MenuNavigationModel>().IsHome);Assert.AreEqual(stamina-1,economy.Stamina);Assert.AreEqual(number,app.GetModel<MenuNavigationModel>().CurrentLevel);Assert.AreEqual(generation+1,model.Generation);Assert.AreEqual(totals[number-1],model.Remaining);Assert.AreEqual(0,model.Deposited);Assert.AreEqual(0,model.Flights.Count);Assert.IsTrue(model.Hives.All(h=>!h.Started));Assert.IsTrue(model.Slots.All(s=>s<0));
    var page=Object.FindObjectOfType<MainMenuPage>();page.StartGameButton.onClick.Invoke();yield return null;yield return null;Assert.IsFalse(model.Paused);Assert.IsFalse(app.GetModel<MenuNavigationModel>().IsHome);Assert.AreEqual(generation+1,model.Generation);Assert.AreEqual(totals[number-1],model.Remaining);Assert.AreEqual("Level"+number.ToString("00"),SceneManager.GetActiveScene().name);yield return HiveInputFixture.Click(popup.Level.Hud,0);Assert.IsTrue(model.Hives[0].Started,"Fresh level must accept new input");
   }
  }
  [UnityTest] public IEnumerator RealTouchesCancelAndConfirmWhileOldBackgroundTouchCannotSpend(){
   yield return Load("Level01");var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var economy=app.GetModel<EconomyModel>();var popup=Object.FindObjectOfType<SettingsPopupController>();int generation=model.Generation,stamina=economy.Stamina;
   popup.OpenButton.onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();
   var events=EventSystem.current;var module=events.GetComponent<StandaloneInputModule>();module.enabled=false;var fake=module.gameObject.AddComponent<SettingsTouchInput>();module.inputOverride=fake;
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;var read=typeof(PointerInputModule).GetMethod("GetTouchPointerEventData",flags);var process=typeof(StandaloneInputModule).GetMethod("ProcessTouchPress",flags);
   System.Action<Button,TouchPhase> touch=(button,phase)=>{Canvas.ForceUpdateCanvases();var point=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);var args=new object[]{new Touch{fingerId=61,position=point,phase=phase},false,false};var data=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{data,args[1],args[2]});if(phase==TouchPhase.Began)Assert.AreEqual(button.gameObject,data.pointerCurrentRaycast.gameObject,"Topmost raycast must be the visible modal button");};
   try {
    fake.Count=1;touch(popup.HomeButton,TouchPhase.Began);touch(popup.HomeButton,TouchPhase.Ended);Assert.IsTrue(popup.IsLeaving);yield return null;
    var leave=Field<Button>(popup,"LeaveButton");touch(leave,TouchPhase.Began);touch(leave,TouchPhase.Ended);Assert.AreEqual(stamina,economy.Stamina,"The previous view's pointer session cannot confirm");Assert.IsFalse(app.GetModel<MenuNavigationModel>().IsHome);
    fake.Count=0;yield return null;yield return null;fake.Count=1;var cancel=Field<Button>(popup,"CancelLeaveButton");touch(cancel,TouchPhase.Began);touch(cancel,TouchPhase.Ended);Assert.IsFalse(popup.IsLeaving);Assert.IsTrue(popup.IsOpen);Assert.AreEqual(generation,model.Generation);
    fake.Count=0;yield return null;yield return null;fake.Count=1;touch(popup.HomeButton,TouchPhase.Began);touch(popup.HomeButton,TouchPhase.Ended);fake.Count=0;yield return null;yield return null;
    fake.Count=1;touch(leave,TouchPhase.Began);popup.Level.SendMessage("OnApplicationPause",true);popup.SendMessage("OnApplicationPause",true);popup.Level.SendMessage("OnApplicationPause",false);popup.SendMessage("OnApplicationPause",false);touch(leave,TouchPhase.Stationary);touch(leave,TouchPhase.Ended);Assert.AreEqual(stamina,economy.Stamina);Assert.IsTrue(popup.IsLeaving);
    fake.Count=0;yield return null;yield return null;fake.Count=1;touch(leave,TouchPhase.Began);touch(leave,TouchPhase.Ended);fake.Count=0;yield return PopupMotionTestWait.Close();Assert.IsTrue(app.GetModel<MenuNavigationModel>().IsHome);Assert.AreEqual(stamina-1,economy.Stamina);Assert.AreEqual(generation+1,model.Generation);
   } finally {module.inputOverride=null;module.enabled=true;events.enabled=true;Object.Destroy(fake);}
  }
  [UnityTest] public IEnumerator BackgroundAndDisableCannotConfirmOrSpend(){
   yield return Load("Level02");var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var economy=app.GetModel<EconomyModel>();var popup=Object.FindObjectOfType<SettingsPopupController>();int generation=model.Generation,stamina=economy.Stamina;
   popup.OpenButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();var leave=Field<Button>(popup,"LeaveButton");popup.Level.SendMessage("OnApplicationPause",true);popup.SendMessage("OnApplicationPause",true);leave.onClick.Invoke();Field<Button>(popup,"CancelLeaveButton").onClick.Invoke();Assert.IsTrue(Leaving(popup));Assert.AreEqual(stamina,economy.Stamina);Assert.AreEqual(generation,model.Generation);
   popup.Level.SendMessage("OnApplicationPause",false);popup.SendMessage("OnApplicationPause",false);yield return null;yield return null;popup.enabled=false;leave.onClick.Invoke();Assert.IsFalse(popup.IsOpen);Assert.IsFalse(Field<GameObject>(popup,"LeavingContent").activeSelf);Assert.AreEqual(stamina,economy.Stamina);Assert.AreEqual(generation,model.Generation);Assert.IsNull(app.GetUtility<UiPopupRouter>().ActiveView);
   popup.enabled=true;popup.OpenButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();leave.onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.IsTrue(app.GetModel<MenuNavigationModel>().IsHome);Assert.AreEqual(stamina-1,economy.Stamina);
  }
 }
}
