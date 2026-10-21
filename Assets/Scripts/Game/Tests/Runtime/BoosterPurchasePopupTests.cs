using System.Collections;
using System.Linq;
using System.Reflection;
using GameFoundation.UI;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class BoosterPurchasePopupTests : EconomyTestFixture {
  int[] previousStock;
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoosterBar Bar=>Object.FindObjectOfType<HarvestBoosterBar>();
  static int[] Stock=>(int[])typeof(HarvestModel).GetField("boosterCounts",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Model);
  [SetUp] public void SaveInventory(){previousStock=(int[])Stock.Clone();}
  [TearDown] public void RestoreInventory(){previousStock.CopyTo(Stock,0);}
  static IEnumerator Load(string scene){SceneManager.LoadScene(scene);yield return null;yield return null;System.Array.Clear(Stock,0,4);Bar.SetInputEnabled(true);yield return null;}
  static void Coins(int value){typeof(EconomyModel).GetProperty("Coins").SetValue(BeeHarvestApp.Interface.GetModel<EconomyModel>(),value);}
  [UnityTest] public IEnumerator PlusOpensThePurchaseDialogWithoutGrantingStockOrSpending(){
   yield return Load("Level01");var app=BeeHarvestApp.Interface;var popup=Bar.PurchasePopup;int generation=Model.Generation;
   var titles=new[]{"Extra Slot","Speed Up","Shuffle","Pick Any"};var descriptions=new[]{"Add 1 extra slot","Speed up all bee carry speeds","Shuffle all single hive positions","Pick any Hive from queue"};
   for(int i=0;i<4;i++){
    Bar.RefillButtons[i].onClick.Invoke();yield return null;
    Assert.AreEqual(0,Model.BoosterCount(i),"Plus opens confirmation without a free grant");Assert.AreEqual(1600,app.GetModel<EconomyModel>().Coins);Assert.IsTrue(Model.Paused);Assert.IsTrue(popup.IsOpen);Assert.AreEqual(i,popup.SelectedBooster);Assert.AreEqual(titles[i],popup.TitleLabel.text);Assert.AreEqual(descriptions[i],popup.DescriptionLabel.text);Assert.AreEqual("1600",popup.PriceLabel.text);Assert.AreEqual("×3",popup.QuantityLabel.text);Assert.AreSame(popup.Icons[i],popup.BoosterIcon.sprite);Assert.AreSame(popup,app.GetUtility<UiPopupRouter>().ActiveView);Assert.IsFalse(Bar.RefillButtons[i].interactable);
    var oldRevision=Model.Revision;Bar.RefillButtons[(i+1)%4].onClick.Invoke();Bar.Buttons[i].onClick.Invoke();Assert.AreEqual(i,popup.SelectedBooster);Assert.AreEqual(oldRevision,Model.Revision);
    popup.CloseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;Assert.IsFalse(popup.IsOpen);Assert.IsFalse(Model.Paused);Assert.AreEqual(generation,Model.Generation);Assert.AreEqual(1600,app.GetModel<EconomyModel>().Coins);Assert.AreEqual(0,Model.BoosterCount(i));Assert.IsNull(app.GetUtility<UiPopupRouter>().ActiveView);
   }
  }
  [UnityTest] public IEnumerator EveryLevelAndItemBuysThreeOnceWithoutUsingIt(){
   for(int number=1;number<=5;number++){
    yield return Load("Level"+number.ToString("00"));var popup=Bar.PurchasePopup;int generation=Model.Generation;
    for(int i=0;i<4;i++){
     Coins(1600);Bar.RefillButtons[i].onClick.Invoke();Assert.IsTrue(popup.IsOpen);Assert.IsTrue(popup.PurchaseButton.interactable);popup.PurchaseButton.onClick.Invoke();popup.PurchaseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;
     Assert.IsFalse(popup.IsOpen);Assert.AreEqual(0,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);Assert.AreEqual(3,Model.BoosterCount(i));Assert.AreEqual("3",Bar.QuantityLabels[i].text);Assert.IsFalse(Bar.RefillButtons[i].gameObject.activeSelf);Assert.IsTrue(Bar.QuantityBadges[i].activeSelf);Assert.AreEqual(generation,Model.Generation);Assert.IsFalse(Model.Paused);Assert.AreEqual(5,Model.Slots.Count);Assert.IsFalse(Model.SpeedBoost);Assert.IsFalse(Model.FreeSelection);Assert.IsTrue(Model.Hives.All(h=>!h.Started));
    }
    Assert.AreEqual(1,Object.FindObjectsOfType<BoosterPurchasePopupController>().Length,"One cached popup per scene");
   }
  }
  [UnityTest] public IEnumerator InsufficientCoinsRejectPurchaseThenBalanceChangeEnablesIt(){
   yield return Load("Level02");Coins(1599);Bar.RefillButtons[1].onClick.Invoke();var popup=Bar.PurchasePopup;Assert.IsFalse(popup.PurchaseButton.interactable);popup.PurchaseButton.onClick.Invoke();Assert.AreEqual(1599,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);Assert.AreEqual(0,Model.BoosterCount(1));Assert.IsTrue(popup.IsOpen);
   BeeHarvestApp.Interface.SendCommand(new AddCoinsCommand());yield return null;Assert.IsTrue(popup.PurchaseButton.interactable);popup.PurchaseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;Assert.AreEqual(499,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);Assert.AreEqual(3,Model.BoosterCount(1));Assert.IsFalse(Model.SpeedBoost);Bar.Buttons[1].onClick.Invoke();Assert.AreEqual(2,Model.BoosterCount(1));Assert.IsTrue(Model.SpeedBoost);popup.Show(1);Assert.IsFalse(popup.IsOpen,"A stocked item cannot reopen a zero-stock offer");
  }
  [UnityTest] public IEnumerator DisabledAndStaleGenerationCallbacksCannotPurchase(){
   yield return Load("Level03");var popup=Bar.PurchasePopup;var level=Object.FindObjectOfType<LevelOneController>();Bar.RefillButtons[2].onClick.Invoke();popup.enabled=false;popup.PurchaseButton.onClick.Invoke();Assert.IsFalse(popup.IsOpen);Assert.AreEqual(1600,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);Assert.AreEqual(0,Model.BoosterCount(2));Assert.IsNull(BeeHarvestApp.Interface.GetUtility<UiPopupRouter>().ActiveView);Assert.IsFalse(Model.Paused);
   popup.enabled=true;yield return null;yield return null;Bar.RefillButtons[2].onClick.Invoke();Assert.IsTrue(popup.IsOpen);level.RestartLevel();popup.PurchaseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.IsFalse(popup.IsOpen);Assert.AreEqual(1600,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);Assert.AreEqual(0,Model.BoosterCount(2));
  }
  [UnityTest] public IEnumerator HomeAndSceneUnloadReleaseTheModalWithoutUnpausingHome(){
   yield return Load("Level04");var cachedPopup=Bar.PurchasePopup;Bar.RefillButtons[0].onClick.Invoke();Assert.IsTrue(cachedPopup.IsOpen);BeeHarvestApp.Interface.SendCommand(new ReturnHomeCommand(false));yield return null;Assert.IsTrue(Model.Paused);Assert.IsFalse(cachedPopup.IsOpen);Assert.IsNull(BeeHarvestApp.Interface.GetUtility<UiPopupRouter>().ActiveView);Assert.AreEqual(1600,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);
   yield return Load("Level05");Bar.RefillButtons[3].onClick.Invoke();Assert.IsTrue(Bar.PurchasePopup.IsOpen);yield return Load("Level01");Assert.AreEqual(1,Object.FindObjectsOfType<BoosterPurchasePopupController>().Length);Assert.IsNull(BeeHarvestApp.Interface.GetUtility<UiPopupRouter>().ActiveView);Bar.RefillButtons[3].onClick.Invoke();Assert.IsTrue(Bar.PurchasePopup.IsOpen);
  }
  [UnityTest] public IEnumerator RealTouchCancelAndPurchaseRejectTheEntryAndBackgroundPointer(){
   yield return Load("Level01");var popup=Bar.PurchasePopup;var level=Object.FindObjectOfType<LevelOneController>();var events=EventSystem.current;var module=events.GetComponent<StandaloneInputModule>();module.enabled=false;var fake=module.gameObject.AddComponent<SettingsTouchInput>();module.inputOverride=fake;
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;var read=typeof(PointerInputModule).GetMethod("GetTouchPointerEventData",flags);var process=typeof(StandaloneInputModule).GetMethod("ProcessTouchPress",flags);
   System.Action<Button,TouchPhase> touch=(button,phase)=>{Canvas.ForceUpdateCanvases();var point=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);var args=new object[]{new Touch{fingerId=71,position=point,phase=phase},false,false};var data=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{data,args[1],args[2]});if(phase==TouchPhase.Began)Assert.AreEqual(button.gameObject,data.pointerCurrentRaycast.gameObject,"Visible modal/plus must own its raycast");};
   try{
    fake.Count=1;touch(Bar.RefillButtons[1],TouchPhase.Began);touch(Bar.RefillButtons[1],TouchPhase.Ended);yield return null;Assert.IsTrue(popup.IsOpen);touch(popup.PurchaseButton,TouchPhase.Began);touch(popup.PurchaseButton,TouchPhase.Ended);Assert.AreEqual(0,Model.BoosterCount(1));Assert.AreEqual(1600,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);
    fake.Count=0;yield return null;yield return null;fake.Count=1;touch(popup.CloseButton,TouchPhase.Began);touch(popup.CloseButton,TouchPhase.Ended);yield return PopupMotionTestWait.Close();Assert.IsFalse(popup.IsOpen);fake.Count=0;yield return null;yield return null;
    fake.Count=1;touch(Bar.RefillButtons[1],TouchPhase.Began);touch(Bar.RefillButtons[1],TouchPhase.Ended);fake.Count=0;yield return null;yield return null;fake.Count=1;touch(popup.PurchaseButton,TouchPhase.Began);level.SendMessage("OnApplicationPause",true);popup.SendMessage("OnApplicationPause",true);level.SendMessage("OnApplicationPause",false);popup.SendMessage("OnApplicationPause",false);touch(popup.PurchaseButton,TouchPhase.Stationary);touch(popup.PurchaseButton,TouchPhase.Ended);Assert.AreEqual(0,Model.BoosterCount(1));Assert.IsTrue(popup.IsOpen);
    fake.Count=0;yield return null;yield return null;fake.Count=1;touch(popup.PurchaseButton,TouchPhase.Began);touch(popup.PurchaseButton,TouchPhase.Ended);fake.Count=0;yield return PopupMotionTestWait.Close();Assert.IsFalse(popup.IsOpen);Assert.AreEqual(3,Model.BoosterCount(1));Assert.AreEqual(0,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);Assert.IsFalse(Model.SpeedBoost);Assert.IsFalse(Model.Paused);
   }finally{module.inputOverride=null;module.enabled=true;events.enabled=true;Object.Destroy(fake);}
  }
 }
}
