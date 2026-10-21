using System.Collections;
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
 public sealed class MainMenuTabsTests : EconomyTestFixture {
  static Image Background(MainMenuPage page) => page.transform.Find("Content/Background").GetComponent<Image>();
  static Transform Gallery(MainMenuPage page) => page.transform.Find("Content/SafeArea/GalleryContent");
  static Button Pager(MainMenuPage page,string name) => page.GetComponentsInChildren<Button>(true).Single(b=>b.name==name);
  static void Swipe(MainMenuPage page,Vector2 from,Vector2 to){
   var data=new PointerEventData(EventSystem.current){position=from,pressPosition=from};
   ExecuteEvents.Execute(page.gameObject,data,ExecuteEvents.beginDragHandler);
   data.position=to; ExecuteEvents.Execute(page.gameObject,data,ExecuteEvents.endDragHandler);
  }

  [UnityTest] public IEnumerator TabsChangePresentationAndHomeResumesTheLiveLevel(){
   SceneManager.LoadScene("Level01"); yield return null; yield return null;
   var app=BeeHarvestApp.Interface; var harvest=app.GetModel<HarvestModel>();
   app.SendCommand(new ReturnHomeCommand(false)); yield return null;
   var page=Object.FindObjectOfType<MainMenuPage>(); var homeSprite=Background(page).sprite;
   int generation=harvest.Generation, remaining=harvest.Remaining;
   Assert.IsTrue(page.ShopButton.interactable,"Shop is now a working tab");
   page.ShopButton.onClick.Invoke();yield return null;yield return null;
   Assert.AreEqual("ShopBackground",Background(page).sprite.name);Assert.IsFalse(page.StartGameButton.gameObject.activeSelf);
   page.GalleryButton.onClick.Invoke();yield return null;yield return null;
   Assert.AreEqual(page.Tabs.GalleryBackground,Background(page).sprite);
   Assert.AreEqual(1,page.Tabs.GalleryContent.GetComponentsInChildren<SettingsPageView>(true).Length);
   Assert.AreEqual(0,page.Tabs.GalleryContent.GetComponentsInChildren<RectTransform>(true).Count(t=>t.name.StartsWith("GalleryCard_")));
   Assert.IsTrue(harvest.Paused);Assert.AreEqual(generation,harvest.Generation);Assert.AreEqual(remaining,harvest.Remaining);
   page.HomeButton.onClick.Invoke();yield return null;yield return null;
   Assert.AreEqual(homeSprite,Background(page).sprite);Assert.IsTrue(page.StartGameButton.gameObject.activeSelf);
   page.StartGameButton.onClick.Invoke();yield return null;yield return null;
   Assert.IsFalse(app.GetModel<MenuNavigationModel>().IsHome);Assert.AreEqual(generation,harvest.Generation);Assert.AreEqual(remaining,harvest.Remaining);
  }

  [UnityTest] public IEnumerator SettingsHasNoPagingAndSwipesDoNotChangePreferences(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   var page=Object.FindObjectOfType<MainMenuPage>();page.GalleryButton.onClick.Invoke();yield return null;yield return null;
   Assert.AreEqual(0,page.GetComponentsInChildren<Button>(true).Count(b=>b.name.StartsWith("GalleryPrevious")||b.name.StartsWith("GalleryNext")));
   int revision=BeeHarvestApp.Interface.GetModel<SettingsModel>().Revision;
   Canvas.ForceUpdateCanvases();var center=RectTransformUtility.WorldToScreenPoint(null,Gallery(page).position);
   Swipe(page,center,center+Vector2.left*300);yield return null;Swipe(page,center,center+Vector2.right*300);yield return null;
   Assert.AreEqual(revision,BeeHarvestApp.Interface.GetModel<SettingsModel>().Revision);Assert.IsTrue(page.Tabs.GalleryContent.activeInHierarchy);
  }

  [UnityTest] public IEnumerator SettingsBackgroundAndOldPointersBlockTabInput(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   var page=Object.FindObjectOfType<MainMenuPage>();Assert.IsTrue(page.ShopButton.interactable);
   var events=EventSystem.current;var module=events.GetComponent<StandaloneInputModule>();
   var input=module.gameObject.AddComponent<SettingsTouchInput>();module.enabled=false;events.enabled=false;module.inputOverride=input;
   try{
    var original=Background(page).sprite;input.Count=1;page.SettingsButton.onClick.Invoke();
    Assert.IsFalse(page.ShopButton.interactable);page.ShopButton.onClick.Invoke();Assert.AreEqual(original,Background(page).sprite);
    page.SettingsPopup.Close();yield return null;Assert.IsFalse(page.GalleryButton.interactable);
    page.GalleryButton.onClick.Invoke();Assert.AreEqual(original,Background(page).sprite);
    input.Count=0;yield return null;Assert.IsTrue(page.GalleryButton.interactable);
    input.Count=1;page.GalleryButton.onClick.Invoke();yield return null;Assert.AreEqual(page.Tabs.GalleryBackground,Background(page).sprite);
    page.ShopButton.onClick.Invoke();Assert.AreEqual(page.Tabs.GalleryBackground,Background(page).sprite);
    input.Count=0;yield return null;yield return null;
    var tabs=page.GetComponents<MonoBehaviour>().Single(c=>c.GetType().Name=="MainMenuTabsController");
    tabs.SendMessage("OnApplicationPause",true);Assert.IsFalse(page.ShopButton.interactable);
    tabs.SendMessage("OnApplicationPause",false);input.Count=1;yield return null;Assert.IsFalse(page.ShopButton.interactable);
    input.Count=0;yield return null;page.ShopButton.onClick.Invoke();yield return null;Assert.AreEqual("ShopBackground",Background(page).sprite.name);
   }finally{module.inputOverride=null;module.enabled=true;events.enabled=true;Object.Destroy(input);}
  }

  [UnityTest] public IEnumerator ReturningHomeWaitsForTheOtherHeldFinger(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;
   var navigation=Object.FindObjectOfType<MainMenuNavigationController>();var page=navigation.Page;
   var popup=Object.FindObjectOfType<SettingsPopupController>();
   var events=EventSystem.current;var module=events.GetComponent<StandaloneInputModule>();
   var input=module.gameObject.AddComponent<SettingsTouchInput>();module.enabled=false;module.inputOverride=input;
   try{
    popup.OpenButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();input.Count=1;popup.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
    Assert.IsTrue(page.gameObject.activeInHierarchy);Assert.IsFalse(page.ShopButton.interactable,"Home entry must wait for the other finger to lift");
    Assert.IsFalse(page.StartGameButton.interactable);Assert.IsFalse(page.SettingsButton.interactable);
    var home=Background(page).sprite;page.ShopButton.onClick.Invoke();yield return null;
    Assert.AreEqual(home,Background(page).sprite);Assert.IsFalse(page.GalleryButton.interactable);
    input.Count=0;yield return null;Assert.IsTrue(page.ShopButton.interactable);
    page.ShopButton.onClick.Invoke();yield return null;Assert.AreEqual("ShopBackground",Background(page).sprite.name);
   }finally{module.inputOverride=null;module.enabled=true;events.enabled=true;Object.Destroy(input);}
  }

  [UnityTest] public IEnumerator SceneReloadKeepsOneSettingsPageAndPopupStillOpens(){
   for(int reload=0;reload<2;reload++){
    SceneManager.LoadScene("MainMenu");yield return null;yield return null;
    var page=Object.FindObjectOfType<MainMenuPage>();Assert.IsTrue(page.GalleryButton.interactable);
    page.GalleryButton.onClick.Invoke();yield return null;yield return null;
    Assert.AreEqual(1,page.GetComponentsInChildren<SettingsPageView>(true).Length);
    page.HomeButton.onClick.Invoke();yield return null;yield return null;
    Assert.AreEqual(1,page.GetComponents<MonoBehaviour>().Count(c=>c.GetType().Name=="MainMenuTabsController"));
    page.SettingsButton.onClick.Invoke();yield return null;Assert.IsTrue(page.SettingsPopup.IsOpen);
    var sprite=Background(page).sprite;page.ShopButton.onClick.Invoke();Assert.AreEqual(sprite,Background(page).sprite);
    page.SettingsPopup.Close();yield return null;yield return null;page.HomeButton.onClick.Invoke();yield return null;yield return null;
    Assert.IsTrue(page.StartGameButton.gameObject.activeSelf);
   }
  }
 }
}
