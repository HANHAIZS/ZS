using System.Collections;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class HomeBeeLevelTests : EconomyTestFixture {
  static TMP_Text Label(MainMenuPage page){return page.StartGameButton.GetComponentInChildren<TMP_Text>(true);}
  [UnityTest] public IEnumerator LaunchButtonKeepsLevelOneAcrossTabsAndStartsItOnce(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   var page=Object.FindObjectOfType<MainMenuPage>();Assert.AreEqual("level 1",Label(page).text);
   page.ShopButton.onClick.Invoke();yield return null;yield return null;Assert.IsFalse(page.StartGameButton.gameObject.activeInHierarchy);
   page.GalleryButton.onClick.Invoke();yield return null;yield return null;Assert.IsFalse(page.StartGameButton.gameObject.activeInHierarchy);
   page.HomeButton.onClick.Invoke();yield return null;yield return null;Assert.AreEqual("level 1",Label(page).text);Assert.IsTrue(page.StartGameButton.interactable);
   page.StartGameButton.onClick.Invoke();page.StartGameButton.onClick.Invoke();float limit=Time.realtimeSinceStartup+4;
   while(SceneManager.GetActiveScene().name!="Level01"&&Time.realtimeSinceStartup<limit)yield return null;
   yield return null;yield return null;Assert.AreEqual("Level01",SceneManager.GetActiveScene().name);Assert.AreEqual(1,Object.FindObjectOfType<LevelOneController>().LevelNumber);
  }
  [UnityTest] public IEnumerator AllLevelsReturnTheirOwnButtonLabelAndStartTheResetGame(){
   var scenes=new[]{"Level01","Level02","Level03","Level04","Level05"};var labels=new[]{"level 1","level 2","level 3","level 4","level 5"};
   for(int i=0;i<scenes.Length;i++){
    SceneManager.LoadScene(scenes[i]);yield return null;yield return null;
    var model=BeeHarvestApp.Interface.GetModel<HarvestModel>();if(BeeHarvestApp.Interface.GetModel<EconomyModel>().Stamina<=1)BeeHarvestApp.Interface.SendCommand(new AddStaminaCommand());int generation=model.Generation,remaining=model.Remaining;
    var popup=Object.FindObjectOfType<SettingsPopupController>();popup.OpenButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();popup.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;
    var page=Object.FindObjectOfType<MainMenuPage>();Assert.AreEqual(labels[i],Label(page).text);
    Assert.AreEqual(generation+1,model.Generation);Assert.AreEqual(remaining,model.Remaining);Assert.IsTrue(model.Paused);
    page.StartGameButton.onClick.Invoke();yield return null;yield return null;
    Assert.AreEqual(scenes[i],SceneManager.GetActiveScene().name);Assert.AreEqual(generation+1,model.Generation);Assert.AreEqual(remaining,model.Remaining);Assert.IsFalse(page.gameObject.activeInHierarchy);
   }
  }
  [UnityTest] public IEnumerator FinalVictoryHomeButtonShowsLevelOneAndLoadsLevelOne(){
   SceneManager.LoadScene(LevelOneAssets.SceneName(LevelOneAssets.LastLevel));yield return null;yield return null;
   var app=BeeHarvestApp.Interface;var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;
   BeeHarvestApp.Interface.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
            app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+AdvanceHarvestCommand.LoadedSeconds+.01f));Assert.IsTrue(app.GetModel<HarvestModel>().Won);
   yield return null;var popup=Object.FindObjectOfType<VictoryPopupController>();float limit=Time.realtimeSinceStartup+3;
   while(!popup.ContinueButton.interactable&&Time.realtimeSinceStartup<limit)yield return null;
   Assert.IsTrue(popup.ContinueButton.interactable);popup.ContinueButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;
   var page=Object.FindObjectOfType<MainMenuPage>();Assert.AreEqual("level 1",Label(page).text);
   page.StartGameButton.onClick.Invoke();limit=Time.realtimeSinceStartup+4;
   while(SceneManager.GetActiveScene().name!="Level01"&&Time.realtimeSinceStartup<limit)yield return null;
   yield return null;yield return null;Assert.AreEqual("Level01",SceneManager.GetActiveScene().name);Assert.AreEqual(1,Object.FindObjectOfType<LevelOneController>().LevelNumber);
  }
 }
}
