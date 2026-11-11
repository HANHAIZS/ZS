using System.Collections;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BeeHarvest.Tests {
 public sealed class PlaytestLevelSelectorTests : EconomyTestFixture {
  IArchitecture App=>BeeHarvestApp.Interface;
  MenuNavigationModel Nav=>App.GetModel<MenuNavigationModel>();
  HarvestModel Harvest=>App.GetModel<HarvestModel>();
  static Button FindButton(string name){var found=GameObject.Find(name);return found==null?null:found.GetComponent<Button>();}
  static IEnumerator Click(string name){
   Button b=null;float until=Time.realtimeSinceStartup+8;
   while(Time.realtimeSinceStartup<until){
    b=FindButton(name);if(b!=null&&b.IsInteractable())break;yield return null;
   }
   Assert.IsNotNull(b,name);Assert.IsTrue(b.IsInteractable(),name);b.onClick.Invoke();
  }
  static IEnumerator WaitForLevel(int number,LevelOneController old=null){
   float until=Time.realtimeSinceStartup+8;
   while(Time.realtimeSinceStartup<until){
    var level=Object.FindObjectOfType<LevelOneController>();
    if(level!=null&&!ReferenceEquals(old,level)&&level.LevelNumber==number&&Object.FindObjectOfType<MainMenuNavigationController>().Page!=null){yield return null;yield break;}
    yield return null;
   }
   Assert.Fail("Expected fresh level "+number);
  }
  IEnumerator StartSelector(){
   App.SendCommand(new ResetMenuNavigationSessionCommand());
   App.SendCommand(new InitializeMenuNavigationCommand(true,6));
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   yield return Click("LevelSelectButton");yield return null;
  }
  static void SetJump(int number){
   var go=GameObject.Find("LevelNumber");Assert.IsNotNull(go);go.GetComponent<TMP_InputField>().text=number.ToString();
  }
  static SettingsPopupController LevelSettings(){
   foreach(var popup in Object.FindObjectsOfType<SettingsPopupController>(true))if(popup.Level!=null)return popup;
   Assert.Fail("Level settings must exist");return null;
  }
  [UnityTest] public IEnumerator NumberEntryAndPauseReturnReselectTheSameLevelFresh(){
   yield return StartSelector();
   SetJump(0);yield return Click("JumpToLevel");Assert.IsTrue(Nav.LevelSelectOpen);
   SetJump(106);yield return Click("JumpToLevel");yield return WaitForLevel(106);
   var old=Object.FindObjectOfType<LevelOneController>();int generation=Harvest.Generation;
   var pause=LevelSettings();pause.OpenButton.onClick.Invoke();yield return null;
   Assert.IsTrue(pause.IsOpen);pause.HomeButton.onClick.Invoke();
   float until=Time.realtimeSinceStartup+5;
   while(!Nav.LevelSelectOpen&&Time.realtimeSinceStartup<until)yield return null;
   Assert.IsTrue(Nav.LevelSelectOpen);Assert.AreEqual(106,Nav.LastPlaytestLevel);
   Assert.AreEqual("106",FindButton("LevelCell16").GetComponentInChildren<TMP_Text>().text);
   yield return Click("LevelCell16");yield return WaitForLevel(106,old);
   Assert.Greater(Harvest.Generation,generation);
  }
  void CompleteOneCube(){
   var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;
   App.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
   App.SendCommand(new StartLevelCommand(new LevelData{
    cubes=new[]{new CubeData{id=0,x=0,y=0,z=0,color="red"}},
    hives=new[]{new HiveData{column=0,color="red",quota=1}}
   }));
   App.SendCommand(new ActivateHiveCommand(0));
   App.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+.01f));
   App.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.LoadedSeconds+.01f));
   Assert.IsTrue(Harvest.Won);
  }
  [UnityTest] public IEnumerator SettlementReplaysAdvancesAndReturnsWithoutWrappingLastLevel(){
   yield return StartSelector();
   int final=LevelOneAssets.LastLevel;
   SetJump(final-1);yield return Click("JumpToLevel");yield return WaitForLevel(final-1);
   CompleteOneCube();int generation=Harvest.Generation;
   yield return Click("PlaytestReplay");
   float until=Time.realtimeSinceStartup+5;
   while(Harvest.Generation==generation&&Time.realtimeSinceStartup<until)yield return null;
   Assert.Greater(Harvest.Generation,generation);Assert.IsFalse(Harvest.Won);
   CompleteOneCube();yield return Click("PlaytestNextLevel");yield return WaitForLevel(final);
   CompleteOneCube();yield return null;yield return null;
   Assert.IsNull(FindButton("PlaytestNextLevel"));
   yield return Click("PlaytestSelectLevel");yield return Click("CloseLevelSelect");
   Assert.IsFalse(Nav.IsPlaytest);Assert.IsFalse(Nav.LevelSelectOpen);Assert.AreEqual(6,Nav.CurrentLevel);
  }
  [TearDown] public void ResetSelection(){App.SendCommand(new ResetMenuNavigationSessionCommand());}
 }
}
