using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class OkaSettingsTests : EconomyTestFixture {
  SettingsPreferences previousPrefs;SettingsModel previousModel;string prefix;
  [SetUp] public void IsolateSettings(){var app=BeeHarvestApp.Interface;previousPrefs=app.GetUtility<SettingsPreferences>();previousModel=app.GetModel<SettingsModel>();prefix="BeeHarvest.tests.oka."+Guid.NewGuid().ToString("N");app.RegisterUtility(new SettingsPreferences(prefix));app.RegisterModel(new SettingsModel());}
  [TearDown] public void RestoreSettings(){var app=BeeHarvestApp.Interface;app.RegisterUtility(previousPrefs);app.RegisterModel(previousModel);foreach(string key in new[]{"Music","Sound","Haptic"})PlayerPrefs.DeleteKey(prefix+"."+key);PlayerPrefs.Save();}
  static Button Control(MainMenuPage page,string name){var node=page.Tabs.GalleryContent.transform.Find("PreferencePanel/Artwork/"+name+"Toggle");Assert.IsNotNull(node,"Settings page requires "+name);return node.GetComponent<Button>();}
  static IEnumerator Menu(){SceneManager.LoadScene("MainMenu");yield return null;yield return null;}
  [UnityTest] public IEnumerator SettingsReplacesGalleryWithoutChangingEconomy(){
   yield return Menu();var page=UnityEngine.Object.FindObjectOfType<MainMenuPage>();page.GalleryButton.onClick.Invoke();yield return null;yield return null;
   Assert.AreEqual("Setting",page.GalleryButton.image.sprite.name);Assert.AreEqual(0,page.Tabs.GalleryContent.GetComponentsInChildren<RectTransform>(true).Count(t=>t.name.StartsWith("GalleryCard_")));
   var app=BeeHarvestApp.Interface;var model=app.GetModel<SettingsModel>();var economy=app.GetModel<EconomyModel>();int revision=model.Revision,coins=economy.Coins,stamina=economy.Stamina;
   Assert.AreEqual(3,page.Tabs.GalleryContent.GetComponentsInChildren<Button>(true).Length);Assert.AreEqual(revision,model.Revision);Assert.AreEqual(coins,economy.Coins);Assert.AreEqual(stamina,economy.Stamina);
   Assert.IsFalse(page.Economy.gameObject.activeInHierarchy);Assert.IsFalse(page.SettingsButton.gameObject.activeInHierarchy);
   page.HomeButton.onClick.Invoke();yield return null;yield return null;Assert.IsTrue(page.Economy.gameObject.activeInHierarchy);Assert.IsFalse(page.SettingsButton.gameObject.activeInHierarchy);
  }
  [UnityTest] public IEnumerator FirstThreeShareSavedStateWithPopupAndReopen(){
   yield return Menu();var page=UnityEngine.Object.FindObjectOfType<MainMenuPage>();page.GalleryButton.onClick.Invoke();yield return null;yield return null;
   var app=BeeHarvestApp.Interface;var model=app.GetModel<SettingsModel>();string[] names={"Music","Sound","Haptic"};
   for(int i=0;i<3;i++){bool before=model.IsEnabled((GamePreference)i);Control(page,names[i]).onClick.Invoke();Assert.AreEqual(!before,model.IsEnabled((GamePreference)i));Assert.AreEqual(!before,app.GetUtility<SettingsPreferences>().Read((GamePreference)i));}
   page.HomeButton.onClick.Invoke();yield return null;yield return null;page.SettingsButton.onClick.Invoke();yield return null;
   for(int i=0;i<3;i++)Assert.IsTrue(page.SettingsPopup.OffMarks[i].activeSelf);
   page.SettingsPopup.PreferenceButtons[0].onClick.Invoke();page.SettingsPopup.Close();yield return null;yield return null;page.GalleryButton.onClick.Invoke();yield return null;yield return null;
   Assert.IsFalse(Control(page,"Music").transform.Find("OffMark").gameObject.activeSelf);Assert.IsTrue(Control(page,"Sound").transform.Find("OffMark").gameObject.activeSelf);
  }
  [UnityTest] public IEnumerator SettingsPageRejectsOldTouchesAndBackgroundClicks(){
   yield return Menu();var page=UnityEngine.Object.FindObjectOfType<MainMenuPage>();var events=EventSystem.current;var module=events.GetComponent<StandaloneInputModule>();var fake=module.gameObject.AddComponent<SettingsTouchInput>();module.enabled=false;events.enabled=false;module.inputOverride=fake;
   try{fake.Count=1;page.GalleryButton.onClick.Invoke();yield return null;var model=BeeHarvestApp.Interface.GetModel<SettingsModel>();int revision=model.Revision;Control(page,"Music").onClick.Invoke();Assert.AreEqual(revision,model.Revision);fake.Count=0;yield return null;yield return null;Assert.IsTrue(Control(page,"Music").interactable);
    page.Tabs.SendMessage("OnApplicationPause",true);Control(page,"Music").onClick.Invoke();Assert.AreEqual(revision,model.Revision);page.Tabs.SendMessage("OnApplicationPause",false);fake.Count=1;yield return null;Control(page,"Music").onClick.Invoke();Assert.AreEqual(revision,model.Revision);fake.Count=0;yield return null;yield return null;Control(page,"Music").onClick.Invoke();Assert.AreEqual(revision+1,model.Revision);
   }finally{module.inputOverride=null;module.enabled=true;events.enabled=true;UnityEngine.Object.Destroy(fake);}
  }
  [UnityTest] public IEnumerator LevelCloseResumesAndQuitUsesExistingHomeFlow(){
   foreach(string name in new[]{"Level01","Level02","Level03","Level04","Level05"}){SceneManager.LoadScene(name);yield return null;yield return null;var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var popup=UnityEngine.Object.FindObjectOfType<SettingsPopupController>();int generation=model.Generation;Assert.AreEqual(popup,UnityEngine.Object.FindObjectOfType<VictoryPopupController>().SettingsPopup,"Victory must retain the current Settings modal reference");
    popup.OpenButton.onClick.Invoke();yield return null;Assert.IsNotNull(popup.RetryButton);Assert.IsNull(popup.ContinueButton);popup.CloseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.IsFalse(popup.IsOpen);Assert.IsFalse(model.Paused);Assert.AreEqual(generation,model.Generation);
    popup.OpenButton.onClick.Invoke();yield return null;int stamina=app.GetModel<EconomyModel>().Stamina;popup.HomeButton.onClick.Invoke();popup.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.IsTrue(app.GetModel<MenuNavigationModel>().IsHome);Assert.AreEqual(stamina-1,app.GetModel<EconomyModel>().Stamina);
   }
  }
 }
}
