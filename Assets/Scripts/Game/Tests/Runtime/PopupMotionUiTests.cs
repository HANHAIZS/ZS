using System.Collections;
using System.Reflection;
using GameFoundation.UI;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public static class PopupMotionTestWait {
  public static PopupMotion Motion(object owner)=>(PopupMotion)owner.GetType().GetField("motion",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(owner);
  public static IEnumerator Close(){
   yield return null;
   float deadline=Time.realtimeSinceStartup+2;
   while(Time.realtimeSinceStartup<deadline){
    bool closing=false;
    foreach(var popup in Object.FindObjectsOfType<SettingsPopupController>())closing|=Motion(popup)?.IsClosing??false;
    foreach(var popup in Object.FindObjectsOfType<BoosterPurchasePopupController>())closing|=Motion(popup)?.IsClosing??false;
    foreach(var popup in Object.FindObjectsOfType<VictoryPopupController>())closing|=Motion(popup)?.IsClosing??false;
    if(!closing){yield return null;yield break;}yield return null;
   }
   Assert.Fail("Popup close must complete within its unscaled-time budget");
  }
  public static IEnumerator Open(PopupMotion motion){float deadline=Time.realtimeSinceStartup+2;while(motion.IsOpening&&Time.realtimeSinceStartup<deadline)yield return null;Assert.IsFalse(motion.IsOpening);}
 }
 public sealed class PopupMotionUiTests : EconomyTestFixture {
  static IEnumerator Load(){SceneManager.LoadScene("Level01");yield return null;yield return null;}
  [UnityTest] public IEnumerator PausedPopupAnimatesOnUnscaledTimeAndOwnsInputUntilClosed(){
   yield return Load();var popup=Object.FindObjectOfType<SettingsPopupController>();var model=BeeHarvestApp.Interface.GetModel<HarvestModel>();int generation=model.Generation;float oldScale=Time.timeScale;
   try {
    Time.timeScale=0;popup.OpenButton.onClick.Invoke();var motion=PopupMotionTestWait.Motion(popup);var content=popup.PopupContent.GetComponent<CanvasGroup>();var dim=popup.DimBackground.GetComponent<CanvasGroup>();
    Assert.AreEqual(0,content.alpha);Assert.AreEqual(0,dim.alpha);Assert.IsTrue(model.Paused);
    yield return PopupMotionTestWait.Open(motion);Assert.AreEqual(1,content.alpha);Assert.AreEqual(1,dim.alpha);
    popup.CloseButton.onClick.Invoke();popup.CloseButton.onClick.Invoke();Assert.IsTrue(popup.IsOpen);Assert.IsTrue(model.Paused);Assert.IsFalse(content.interactable);Assert.AreSame(popup,BeeHarvestApp.Interface.GetUtility<UiPopupRouter>().ActiveView);
    yield return PopupMotionTestWait.Close();Assert.IsFalse(popup.IsOpen);Assert.IsFalse(model.Paused);Assert.IsFalse(popup.DimBackground.activeSelf);Assert.AreEqual(generation,model.Generation);
   }finally{Time.timeScale=oldScale;}
  }
  [UnityTest] public IEnumerator BackgroundCancelsDeferredLeaveAndNewClickSpendsExactlyOnce(){
   yield return Load();var app=BeeHarvestApp.Interface;var popup=Object.FindObjectOfType<SettingsPopupController>();var model=app.GetModel<HarvestModel>();var economy=app.GetModel<EconomyModel>();int generation=model.Generation,stamina=economy.Stamina;
   popup.OpenButton.onClick.Invoke();yield return PopupMotionTestWait.Open(PopupMotionTestWait.Motion(popup));popup.HomeButton.onClick.Invoke();Assert.IsTrue(popup.IsLeaving);Assert.AreEqual(1,popup.DimBackground.GetComponent<CanvasGroup>().alpha);
   popup.LeaveButton.onClick.Invoke();popup.LeaveButton.onClick.Invoke();Assert.AreEqual(stamina,economy.Stamina);Assert.AreEqual(generation,model.Generation);Assert.IsFalse(app.GetModel<MenuNavigationModel>().IsHome);
   popup.Level.SendMessage("OnApplicationPause",true);popup.SendMessage("OnApplicationPause",true);popup.Level.SendMessage("OnApplicationPause",false);popup.SendMessage("OnApplicationPause",false);
   yield return PopupMotionTestWait.Open(PopupMotionTestWait.Motion(popup));yield return null;yield return null;
   Assert.IsTrue(popup.IsLeaving);Assert.AreEqual(stamina,economy.Stamina);Assert.AreEqual(generation,model.Generation);
   popup.LeaveButton.onClick.Invoke();popup.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.IsFalse(popup.IsOpen);Assert.IsTrue(app.GetModel<MenuNavigationModel>().IsHome);Assert.AreEqual(stamina-1,economy.Stamina);Assert.AreEqual(generation+1,model.Generation);
  }
  [UnityTest] public IEnumerator FourPurchaseVariantsUseMotionAndCancelWithoutSpending(){
   yield return Load();var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var stock=(int[])typeof(HarvestModel).GetField("boosterCounts",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(model);var original=(int[])stock.Clone();
   try {
    System.Array.Clear(stock,0,stock.Length);var bar=Object.FindObjectOfType<HarvestBoosterBar>();var popup=bar.PurchasePopup;bar.SetInputEnabled(true);
    for(int i=0;i<4;i++){
     bar.RefillButtons[i].onClick.Invoke();Assert.IsTrue(popup.IsOpen);Assert.AreEqual(0,popup.PopupContent.GetComponent<CanvasGroup>().alpha);
     yield return PopupMotionTestWait.Open(PopupMotionTestWait.Motion(popup));popup.CloseButton.onClick.Invoke();Assert.IsTrue(popup.IsOpen);Assert.IsTrue(model.Paused);
     yield return PopupMotionTestWait.Close();Assert.IsFalse(popup.IsOpen);Assert.IsFalse(model.Paused);Assert.AreEqual(1600,app.GetModel<EconomyModel>().Coins);Assert.AreEqual(0,model.BoosterCount(i));
    }
   }finally{original.CopyTo(stock,0);}
  }
  [UnityTest] public IEnumerator DisableCancelsCloseAndResponsiveFitComposesWithMotion(){
   yield return Load();var popup=Object.FindObjectOfType<SettingsPopupController>();popup.OpenButton.onClick.Invoke();popup.CloseButton.onClick.Invoke();popup.enabled=false;
   Assert.IsFalse(popup.IsOpen);Assert.IsFalse(popup.DimBackground.activeSelf);popup.enabled=true;popup.OpenButton.onClick.Invoke();Assert.IsTrue(popup.IsOpen);
   yield return PopupMotionTestWait.Open(PopupMotionTestWait.Motion(popup));Assert.AreEqual(1,popup.PopupContent.GetComponent<CanvasGroup>().alpha);Assert.IsTrue(popup.IsOpen);
   var root=new GameObject("FitFixture",typeof(RectTransform));var frame=new GameObject("Frame",typeof(RectTransform));frame.transform.SetParent(root.transform,false);
   try {
    ((RectTransform)root.transform).sizeDelta=new Vector2(500,1000);((RectTransform)frame.transform).sizeDelta=new Vector2(700,1000);var fit=frame.AddComponent<PausedDialogFit>();fit.SetMotionScale(.85f);Assert.AreEqual(.55f*.85f,frame.transform.localScale.x,.0001f);
    ((RectTransform)root.transform).sizeDelta=new Vector2(1000,2000);fit.SetMotionScale(.85f);Assert.AreEqual(.85f,frame.transform.localScale.x,.0001f);fit.SetMotionScale(1);Assert.AreEqual(1,frame.transform.localScale.x,.0001f);
   }finally{Object.Destroy(root);}
  }
  [UnityTest] public IEnumerator RouterReplacementAndGenerationChangeCancelPendingRetry(){
   yield return Load();var app=BeeHarvestApp.Interface;var popup=Object.FindObjectOfType<SettingsPopupController>();var model=app.GetModel<HarvestModel>();int generation=model.Generation,stamina=app.GetModel<EconomyModel>().Stamina;
   popup.OpenButton.onClick.Invoke();popup.RetryButton.onClick.Invoke();Assert.IsTrue(PopupMotionTestWait.Motion(popup).IsClosing);
   app.GetUtility<UiPopupRouter>().Show(popup.PopupId,UiPopupOpenMode.Replace);yield return PopupMotionTestWait.Open(PopupMotionTestWait.Motion(popup));
   Assert.IsTrue(popup.IsOpen);Assert.AreEqual(generation,model.Generation);Assert.AreEqual(stamina,app.GetModel<EconomyModel>().Stamina);Assert.IsTrue(model.Paused);
   popup.RetryButton.onClick.Invoke();popup.Level.RestartLevel();yield return PopupMotionTestWait.Close();Assert.IsFalse(popup.IsOpen);Assert.AreEqual(generation+1,model.Generation);Assert.AreEqual(stamina,app.GetModel<EconomyModel>().Stamina);
  }
 }
}
