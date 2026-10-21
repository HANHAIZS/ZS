using System.Collections;
using System.Linq;
using GameFoundation.UI;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class MainMenuSettingsTests : EconomyTestFixture {
  readonly bool[] had=new bool[3],values=new bool[3];
  readonly string[] names={"Music","Sound","Haptic"};
  [SetUp] public void RememberPreferences(){
   for(int i=0;i<3;i++){string key="BeeHarvest.settings.v1."+names[i];had[i]=PlayerPrefs.HasKey(key);values[i]=PlayerPrefs.GetInt(key,1)!=0;}
  }
  [TearDown] public void RestorePreferences(){
   for(int i=0;i<3;i++){BeeHarvestApp.Interface.SendCommand(new SetPreferenceCommand((GamePreference)i,values[i]));if(!had[i])PlayerPrefs.DeleteKey("BeeHarvest.settings.v1."+names[i]);}PlayerPrefs.Save();
  }
  Button Gear(MainMenuPage page){
   var gear=page.GetComponentsInChildren<Button>(true).SingleOrDefault(b=>b.name=="SettingsButton");
   Assert.IsNotNull(gear,"Legacy modal retains its hidden opener reference");return gear;
  }
  [UnityTest] public IEnumerator MenuPopupBlocksStartAndSharesPreferencesWithLevel(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   var app=BeeHarvestApp.Interface;var settings=app.GetModel<SettingsModel>();var harvest=app.GetModel<HarvestModel>();
   var page=Object.FindObjectOfType<MainMenuPage>();var gear=Gear(page);gear.onClick.Invoke();yield return null;
   var popup=page.GetComponentInChildren<SettingsPopupController>(true);Assert.IsTrue(popup.IsOpen);
   Assert.IsNull(popup.RetryButton);Assert.IsNull(popup.HomeButton);
   Assert.IsFalse(popup.GetComponentsInChildren<Button>(true).Any(b=>b.name=="RetryButton"||b.name=="HomeButton"));
   Assert.IsFalse(page.StartGameButton.interactable);int generation=harvest.Generation,remaining=harvest.Remaining;bool paused=harvest.Paused;
   page.StartGameButton.onClick.Invoke();yield return null;yield return null;Assert.AreEqual("MainMenu",SceneManager.GetActiveScene().name);
   for(int cycle=0;cycle<3;cycle++){
    for(int i=0;i<3;i++){
     bool before=settings.IsEnabled((GamePreference)i);popup.PreferenceButtons[i].onClick.Invoke();
     Assert.AreEqual(!before,settings.IsEnabled((GamePreference)i),"Each menu click toggles exactly once");
     Assert.AreEqual(before,popup.OffMarks[i].activeSelf);Assert.AreEqual(before?0:1,PlayerPrefs.GetInt("BeeHarvest.settings.v1."+names[i],-1));
    }
    popup.CloseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
    Assert.IsNull(app.GetUtility<UiPopupRouter>().ActiveView);Assert.AreEqual(paused,harvest.Paused);
    Assert.AreEqual(generation,harvest.Generation);Assert.AreEqual(remaining,harvest.Remaining);
    if(cycle<2){gear.onClick.Invoke();yield return null;}
   }
   page.StartGameButton.onClick.Invoke();float limit=Time.realtimeSinceStartup+4;
   while(SceneManager.GetActiveScene().name!="Level01"&&Time.realtimeSinceStartup<limit)yield return null;
   yield return null;yield return null;
   var levelPopup=Object.FindObjectOfType<SettingsPopupController>();levelPopup.OpenButton.onClick.Invoke();yield return null;
   for(int i=0;i<3;i++)Assert.AreEqual(!settings.IsEnabled((GamePreference)i),levelPopup.OffMarks[i].activeSelf,"Menu choice survives entering level");
   Assert.IsNotNull(levelPopup.RetryButton);Assert.IsNull(levelPopup.ContinueButton);Assert.IsNotNull(levelPopup.HomeButton);
   levelPopup.HomeButton.onClick.Invoke();levelPopup.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
   page=Object.FindObjectOfType<MainMenuPage>();Gear(page).onClick.Invoke();yield return null;
   popup=page.GetComponentInChildren<SettingsPopupController>(true);Assert.IsTrue(popup.IsOpen);Assert.IsTrue(harvest.Paused);
   popup.CloseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();Assert.IsTrue(harvest.Paused,"Menu settings must keep retained level paused");
   Assert.AreEqual(1784,harvest.Remaining);Assert.AreEqual(1,page.GetComponentsInChildren<SettingsPopupController>(true).Length);
  }
  [UnityTest] public IEnumerator ClosingMenuSettingsWaitsForOldTouchBeforeStartCanLoad(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   var page=Object.FindObjectOfType<MainMenuPage>();var gear=Gear(page);
   var eventSystem=EventSystem.current;var module=eventSystem.GetComponent<StandaloneInputModule>();
   module.enabled=false;eventSystem.enabled=false;var fake=module.gameObject.AddComponent<SettingsTouchInput>();module.inputOverride=fake;
   try{
    fake.Count=1;gear.onClick.Invoke();var popup=page.GetComponentInChildren<SettingsPopupController>(true);popup.CloseButton.onClick.Invoke();
    yield return PopupMotionTestWait.Close();Assert.IsFalse(page.StartGameButton.interactable,"Old modal finger must still block Start");
    page.StartGameButton.onClick.Invoke();yield return null;Assert.AreEqual("MainMenu",SceneManager.GetActiveScene().name);
    fake.Count=0;yield return PopupMotionTestWait.Close();Assert.IsTrue(page.StartGameButton.interactable);Assert.IsTrue(gear.interactable);
   }finally{module.inputOverride=null;module.enabled=true;eventSystem.enabled=true;Object.Destroy(fake);}
  }
 }
}
