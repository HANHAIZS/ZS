using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class HarvestInventoryUiTests : EconomyTestFixture {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoosterBar Bar=>UnityEngine.Object.FindObjectOfType<HarvestBoosterBar>();
  static int Count(int i){var m=typeof(HarvestModel).GetMethod("BoosterCount");Assert.IsNotNull(m);return (int)m.Invoke(Model,new object[]{i});}
  static float Seconds(){var p=typeof(HarvestModel).GetProperty("SpeedSeconds");Assert.IsNotNull(p);return (float)p.GetValue(Model);}
  static Transform Badge(int i,string name){var t=Bar.Buttons[i].transform.Find(name);Assert.IsNotNull(t,"Missing corner badge: "+name);return t;}
  static void Restock(){var stock=(int[])typeof(HarvestModel).GetField("boosterCounts",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Model);for(int i=0;i<4;i++)stock[i]=1;Bar.SetInputEnabled(true);}
  static IEnumerator Load(){SceneManager.LoadScene("Level01");yield return null;yield return null;Badge(0,"QuantityBadge");Restock();}
  static IEnumerator Tap(Transform target){
   yield return null;Canvas.ForceUpdateCanvases();
   var e=EventSystem.current;var module=(StandaloneInputModule)e.currentInputModule;module.enabled=false;e.enabled=false;
   var fake=module.gameObject.AddComponent<SettingsTouchInput>();module.inputOverride=fake;fake.Count=1;
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;var read=typeof(PointerInputModule).GetMethod("GetTouchPointerEventData",flags);var process=typeof(StandaloneInputModule).GetMethod("ProcessTouchPress",flags);
   var point=RectTransformUtility.WorldToScreenPoint(null,target.position);
   foreach(var phase in new[]{TouchPhase.Began,TouchPhase.Ended}){var args=new object[]{new Touch{fingerId=37,position=point,phase=phase},false,false};var p=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{p,args[1],args[2]});if(phase==TouchPhase.Began)Assert.AreEqual(target.gameObject,p.pointerClick,"Touch raycast must reach the standalone refill button at "+point+", hit "+(p.pointerCurrentRaycast.gameObject==null?"none":p.pointerCurrentRaycast.gameObject.name));}
   fake.Count=0;module.inputOverride=null;module.enabled=true;e.enabled=true;UnityEngine.Object.Destroy(fake);yield return null;
  }
  [UnityTest] public IEnumerator CornerPlusAsksForPurchaseAndThreeUsePackDoesNotUseTheItem(){
   yield return Load();for(int i=0;i<4;i++){Assert.IsTrue(Badge(i,"QuantityBadge").gameObject.activeSelf);Assert.AreEqual("1",Badge(i,"QuantityBadge").Find("Count").GetComponent<TMP_Text>().text);Assert.IsFalse(Badge(i,"RefillBadge").gameObject.activeSelf);}
   Bar.Buttons[1].onClick.Invoke();Assert.AreEqual(0,Count(1));Assert.IsFalse(Badge(1,"QuantityBadge").gameObject.activeSelf);Assert.IsTrue(Badge(1,"RefillBadge").gameObject.activeSelf);Assert.IsFalse(Bar.Buttons[1].interactable);
   float before=Seconds();yield return Tap(Badge(1,"RefillBadge"));Assert.AreEqual(0,Count(1));Assert.IsTrue(Bar.PurchasePopup.IsOpen);yield return Tap(Bar.PurchasePopup.PurchaseButton.transform);Assert.AreEqual(3,Count(1));Assert.LessOrEqual(Seconds(),before,"Refill must not refresh/consume speed");Assert.IsTrue(Badge(1,"QuantityBadge").gameObject.activeSelf);
   Assert.AreEqual("FredokaBoldSDF",Badge(1,"QuantityBadge").Find("Count").GetComponent<TMP_Text>().font.name);
  }
  [UnityTest] public IEnumerator CountdownUpdatesWithoutAnyActiveHives(){
   yield return Load();Bar.Buttons[1].onClick.Invoke();BeeSceneSteps.Advance(1.2f);yield return null;
   StringAssert.Contains("2:59",Bar.StateLabels[1].text,"Timer rendering must update even without harvest revisions");
  }
  [UnityTest] public IEnumerator SettingsFreezesTimerLeaveAndRetryResetEffectsButKeepInventory(){
   yield return Load();Bar.Buttons[1].onClick.Invoke();Bar.Buttons[3].onClick.Invoke();var settings=UnityEngine.Object.FindObjectOfType<SettingsPopupController>();settings.OpenButton.onClick.Invoke();yield return null;
   float before=Seconds();BeeSceneSteps.Advance(200);Assert.AreEqual(before,Seconds());Badge(1,"RefillBadge").GetComponent<Button>().onClick.Invoke();Assert.AreEqual(0,Count(1));
   settings.HomeButton.onClick.Invoke();settings.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();BeeSceneSteps.Advance(200);Assert.AreEqual(0,Seconds());
   UnityEngine.Object.FindObjectOfType<MainMenuNavigationController>().Page.StartGameButton.onClick.Invoke();yield return null;yield return null;Assert.IsFalse(Model.SpeedBoost);Assert.IsFalse(Model.FreeSelection);Assert.AreEqual(0,Count(1));
   HiveInputFixture.RetryLevelForTest();yield return null;yield return null;Assert.AreEqual(0,Count(1));Assert.AreEqual(0,Count(3));Assert.AreEqual(0,Seconds());Assert.IsFalse(Model.FreeSelection);
  }
  [UnityTest] public IEnumerator CancelledRearHopKeepsSingleChoiceWithoutTogglingOrSpendingAgain(){
   yield return Load();var hud=UnityEngine.Object.FindObjectOfType<LevelOneHud>();
   Bar.Buttons[3].onClick.Invoke();Assert.AreEqual(0,Count(3));Assert.IsTrue(Model.FreeSelection);
   Bar.RefillButtons[3].onClick.Invoke();Bar.PurchasePopup.PurchaseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;Bar.Buttons[3].onClick.Invoke();
   Assert.AreEqual(3,Count(3));Assert.IsTrue(Model.FreeSelection);Assert.AreEqual("",Bar.StateLabels[3].text);
   int rear=hud.DisplayedHive(1,true);hud.RearButtons[1].onClick.Invoke();
   var settings=UnityEngine.Object.FindObjectOfType<SettingsPopupController>();settings.OpenButton.onClick.Invoke();yield return null;
   Assert.IsFalse(Model.Hives[rear].Started);Assert.IsTrue(Model.FreeSelection);Assert.AreEqual(3,Count(3));
   settings.CloseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;
   Assert.IsTrue(hud.RearButtons[1].interactable);hud.RearButtons[1].onClick.Invoke();
   float deadline=Time.realtimeSinceStartup+2;while(!Model.Hives[rear].Started&&Time.realtimeSinceStartup<deadline)yield return null;
   Assert.IsTrue(Model.Hives[rear].Started);Assert.IsFalse(Model.FreeSelection);Assert.AreEqual(3,Count(3));Assert.IsTrue(Bar.Buttons[3].interactable);
  }
  [UnityTest] public IEnumerator BackgroundResumeRejectsOldUseAndRefillTouches(){
   foreach(bool refill in new[]{false,true}){
    yield return Load();if(refill)Bar.Buttons[1].onClick.Invoke();yield return null;Canvas.ForceUpdateCanvases();
    var target=refill?Badge(1,"RefillBadge"):Bar.Buttons[1].transform;int before=Count(1);var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();
    var e=EventSystem.current;var module=(StandaloneInputModule)e.currentInputModule;module.enabled=false;e.enabled=false;var fake=module.gameObject.AddComponent<SettingsTouchInput>();module.inputOverride=fake;fake.Count=1;
    var flags=BindingFlags.Instance|BindingFlags.NonPublic;var read=typeof(PointerInputModule).GetMethod("GetTouchPointerEventData",flags);var process=typeof(StandaloneInputModule).GetMethod("ProcessTouchPress",flags);var point=RectTransformUtility.WorldToScreenPoint(null,target.position);
    var args=new object[]{new Touch{fingerId=43,position=point,phase=TouchPhase.Began},false,false};var pointer=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{pointer,args[1],args[2]});Assert.AreEqual(target.gameObject,pointer.pointerClick);
    level.SendMessage("OnApplicationPause",true);level.SendMessage("OnApplicationPause",false);
    args=new object[]{new Touch{fingerId=43,position=point,phase=TouchPhase.Stationary},false,false};pointer=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{pointer,args[1],args[2]});yield return null;
    args=new object[]{new Touch{fingerId=43,position=point,phase=TouchPhase.Ended},false,false};pointer=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{pointer,args[1],args[2]});fake.Count=0;yield return null;
    Assert.AreEqual(before,Count(1),"Old background touch must not "+(refill?"grant":"consume")+" an item");
    module.inputOverride=null;module.enabled=true;e.enabled=true;UnityEngine.Object.Destroy(fake);yield return null;yield return Tap(target);if(refill){Assert.AreEqual(0,Count(1));Assert.IsTrue(Bar.PurchasePopup.IsOpen);yield return Tap(Bar.PurchasePopup.PurchaseButton.transform);}Assert.AreEqual(refill?3:0,Count(1),"Fresh post-resume touch still works");
   }
  }
  [UnityTest] public IEnumerator HeldRefillPressDuringHopCannotGrantAfterLanding(){
   yield return Load();Bar.Buttons[1].onClick.Invoke();var hud=UnityEngine.Object.FindObjectOfType<LevelOneHud>();int hive=hud.DisplayedHive(2,false);
   var e=EventSystem.current;var module=(StandaloneInputModule)e.currentInputModule;module.enabled=false;e.enabled=false;var fake=module.gameObject.AddComponent<SettingsTouchInput>();module.inputOverride=fake;
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;var read=typeof(PointerInputModule).GetMethod("GetTouchPointerEventData",flags);var process=typeof(StandaloneInputModule).GetMethod("ProcessTouchPress",flags);
   hud.QueueButtons[2].onClick.Invoke();yield return new WaitForSeconds(.08f);var target=Badge(1,"RefillBadge");var point=RectTransformUtility.WorldToScreenPoint(null,target.position);fake.Count=1;
   var args=new object[]{new Touch{fingerId=39,position=point,phase=TouchPhase.Began},false,false};var p=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{p,args[1],args[2]});
   yield return new WaitForSeconds(.4f);Assert.IsTrue(Model.Hives[hive].Started);
   args=new object[]{new Touch{fingerId=39,position=point,phase=TouchPhase.Ended},false,false};p=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{p,args[1],args[2]});fake.Count=0;yield return null;Assert.AreEqual(0,Count(1));
   module.inputOverride=null;module.enabled=true;e.enabled=true;UnityEngine.Object.Destroy(fake);yield return Tap(target);Assert.AreEqual(0,Count(1));Assert.IsTrue(Bar.PurchasePopup.IsOpen);yield return Tap(Bar.PurchasePopup.PurchaseButton.transform);Assert.AreEqual(3,Count(1));
  }
 }
}
