using System.Collections;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class FastPlayPreviewTests : EconomyTestFixture {
  bool previousEnabled;EnterPlayModeOptions previousOptions;SceneSetup[] previous;
  [SetUp] public void CaptureEditor(){previousEnabled=EditorSettings.enterPlayModeOptionsEnabled;previousOptions=EditorSettings.enterPlayModeOptions;previous=EditorSceneManager.GetSceneManagerSetup();}
  [UnityTearDown] public IEnumerator RestoreEditor(){
   if(EditorApplication.isPlaying)yield return new ExitPlayMode();
   EditorSettings.enterPlayModeOptionsEnabled=previousEnabled;EditorSettings.enterPlayModeOptions=previousOptions;
   // Discard only this test's unsaved preview edits, then return to the user's saved scene setup.
   EditorSceneManager.OpenScene(LevelOneAssets.GameScenePath,OpenSceneMode.Single);EditorSceneManager.RestoreSceneManagerSetup(previous);
  }
  [UnityTest] public IEnumerator FastPlayEntryUsesInspectorInsteadOfRetainedLevel112OnEveryRun(){
   EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload|EnterPlayModeOptions.DisableSceneReload;
   EditorSceneManager.OpenScene(LevelOneAssets.GameScenePath,OpenSceneMode.Single);
   foreach(int number in new[]{73,35}){
    var level=Object.FindObjectOfType<LevelOneController>();level.PreviewLevel=number;
    var app=BeeHarvestApp.Interface;app.SendCommand(new InitializeMenuNavigationCommand(false,112));var retained=app.GetModel<MenuNavigationModel>();Assert.IsTrue(retained.Initialized);Assert.AreEqual(112,retained.CurrentLevel);
    yield return new EnterPlayMode(false);yield return null;yield return null;
    level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;Assert.AreSame(retained,app.GetModel<MenuNavigationModel>(),"This test must exercise no domain reload, retaining the same model object");
    TestContext.WriteLine("preview="+number+", actual="+level.LevelNumber+", navigation="+retained.CurrentLevel);
    Assert.AreEqual(number,level.LevelNumber,"A new Editor Play session must not consume last run's navigation selection");Assert.AreEqual(number,retained.CurrentLevel);Assert.IsFalse(retained.IsHome);Assert.AreEqual(new LevelOneAssets().Level(number).cubes.Length,app.GetModel<HarvestModel>().Cubes.Count);Assert.AreEqual("Level "+number,level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);
    yield return new ExitPlayMode();
   }
  }
 }
}
