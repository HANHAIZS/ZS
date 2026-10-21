using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class AdditionalLevelsGameplayTests : EconomyTestFixture {
  static readonly string[] Names={"Chick","Sheep","Shark","Broccoli","Cactustoy","Carrot","CatHead","Snake","Onion","DodoBird"};
  static readonly int[] Counts={870,1030,1054,1084,1205,1244,1300,1595,1617,1622};
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static IEnumerator Load(int number){
   string scene="Level"+number.ToString("00");Assert.IsTrue(Application.CanStreamedLevelBeLoaded(scene),scene);
   SceneManager.LoadScene(scene);yield return null;yield return null;
  }
  [UnityTest] public IEnumerator AllTenScenesLoadTheirOwnModelAndDeliverCargoWithExistingUiAndAudio(){
   for(int i=0;i<10;i++){
    yield return Load(i+6);var level=Object.FindObjectOfType<LevelOneController>();var hud=level.Hud;
    Assert.AreEqual(i+6,level.LevelNumber);Assert.AreEqual("Level "+(i+6),hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);
    Assert.AreEqual(Counts[i],Model.Remaining);Assert.AreEqual(3,Model.ColumnCount);Assert.AreEqual(5,Model.Slots.Count);Assert.AreEqual(3,hud.VisibleRows);
    Assert.IsFalse(hud.QueueButtons[3].gameObject.activeSelf);Assert.IsFalse(hud.QueueButtons[4].gameObject.activeSelf);
    Assert.AreEqual(Counts[i],level.GetComponent<HarvestBoardView>().GetComponentsInChildren<Renderer>().Count(r=>r.name.StartsWith(Names[i]+"V2_Voxel_")));
    var audio=Object.FindObjectsOfType<GameAudioController>();Assert.AreEqual(1,audio.Length);Assert.AreEqual(7,audio[0].GetComponents<AudioSource>().Length);
    Assert.AreEqual("LevelMusic",audio[0].GetComponents<AudioSource>().Single(s=>s.loop).clip.name);
    yield return HiveInputFixture.Click(hud,0);BeeSceneSteps.Until(()=>Model.Deposited>0,30);
    Assert.Greater(Model.Deposited,0,Names[i]);Assert.AreEqual(Counts[i],Model.Remaining+Model.InTransit+Model.Deposited);
   }
  }
  [UnityTest] public IEnumerator EveryNewLevelLeavesToItsOwnHomeLabelAndRetriesItsOriginalData(){
   for(int i=0;i<10;i++){
    yield return Load(i+6);var app=BeeHarvestApp.Interface;var level=Object.FindObjectOfType<LevelOneController>();
    while(app.GetModel<EconomyModel>().Stamina<3)app.SendCommand(new AddStaminaCommand());
    yield return HiveInputFixture.Click(level.Hud,0);BeeSceneSteps.Until(()=>Model.Deposited>0,30);
    int generation=Model.Generation;var settings=Object.FindObjectOfType<SettingsPopupController>();
    settings.OpenButton.onClick.Invoke();settings.HomeButton.onClick.Invoke();settings.LeaveButton.onClick.Invoke();
    yield return PopupMotionTestWait.Close();yield return null;
    var page=Object.FindObjectOfType<MainMenuPage>();Assert.AreEqual("Level "+(i+6),page.StartGameButton.GetComponentInChildren<TMP_Text>(true).text);
    Assert.AreEqual(generation+1,Model.Generation);Assert.AreEqual(Counts[i],Model.Remaining);Assert.AreEqual(0,Model.Deposited);Assert.IsTrue(Model.Paused);
    page.StartGameButton.onClick.Invoke();yield return null;yield return null;
    Assert.AreEqual("Level"+(i+6).ToString("00"),SceneManager.GetActiveScene().name);Assert.IsFalse(Model.Paused);
    level.SendMessage("OnApplicationPause",true);Assert.IsTrue(level.IsApplicationPaused);
    level.SendMessage("OnApplicationPause",false);yield return null;Assert.IsFalse(level.IsApplicationPaused);
    HiveInputFixture.RetryLevelForTest();yield return null;
    Assert.AreEqual(i+6,level.LevelNumber);Assert.AreEqual(Counts[i],Model.Remaining);Assert.AreEqual(generation+2,Model.Generation);
   }
  }
  [UnityTest] public IEnumerator FifthThroughFourteenthVictoryLoadsTheFollowingSceneOnce(){
   for(int from=5;from<15;from++){
    yield return Load(from);var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var app=BeeHarvestApp.Interface;
    app.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
    app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));
    app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(Model.Won);yield return null;
    var popup=Object.FindObjectOfType<VictoryPopupController>();float limit=Time.realtimeSinceStartup+3;
    while(!popup.ContinueButton.interactable&&Time.realtimeSinceStartup<limit)yield return null;
    Assert.IsTrue(popup.ContinueButton.interactable);popup.ContinueButton.onClick.Invoke();popup.ContinueButton.onClick.Invoke();
    string next="Level"+(from+1).ToString("00");limit=Time.realtimeSinceStartup+5;
    while(SceneManager.GetActiveScene().name!=next&&Time.realtimeSinceStartup<limit)yield return null;
    yield return null;yield return null;
    Assert.AreEqual(next,SceneManager.GetActiveScene().name);Assert.AreEqual(Counts[from-5],Model.Remaining);Assert.IsFalse(Model.Won);
   }
  }
 }
}
