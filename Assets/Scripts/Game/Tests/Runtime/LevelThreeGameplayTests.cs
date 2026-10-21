using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests {
 public sealed class LevelThreeGameplayTests : EconomyTestFixture {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static IEnumerator Load(){Assert.IsTrue(Application.CanStreamedLevelBeLoaded("Level03"),"Banana must be included in the playable scene list");SceneManager.LoadScene("Level03");yield return null;yield return null;}
  static void AssertBananaCurveVisible(){
   var parts=GameObject.Find("Banana(Clone)").GetComponentsInChildren<Renderer>();var bounds=parts[0].bounds;foreach(var r in parts)bounds.Encapsulate(r.bounds);
   float left=float.PositiveInfinity,right=float.NegativeInfinity,bottom=float.PositiveInfinity,top=float.NegativeInfinity;
   for(int i=0;i<8;i++){var corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));var screen=Camera.main.WorldToScreenPoint(corner);left=Mathf.Min(left,screen.x);right=Mathf.Max(right,screen.x);bottom=Mathf.Min(bottom,screen.y);top=Mathf.Max(top,screen.y);}
   Assert.Greater((right-left)/(top-bottom),.45f,"The initial view must show the banana's curved side rather than a narrow edge");
  }
  [UnityTest] public IEnumerator BananaDefaultViewKeepsItsReadableCurveAfterRestart(){
   yield return Load();AssertBananaCurveVisible();Object.FindObjectOfType<LevelOneController>().RestartLevel();yield return null;AssertBananaCurveVisible();
  }
  [UnityTest] public IEnumerator BananaSceneShowsItsOwnModelThreeColumnsAndLiveQuota(){
   yield return Load();var level=Object.FindObjectOfType<LevelOneController>();Assert.AreEqual(3,level.LevelNumber);
   Assert.AreEqual("Level 3",level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);Assert.AreEqual(812,Model.Remaining);Assert.AreEqual(20,Model.Hives.Count);
   var board=level.GetComponent<HarvestBoardView>();Assert.AreEqual(812,board.GetComponentsInChildren<Renderer>().Count(r=>r.name.StartsWith("BananaV2_Voxel_")));Assert.IsNull(GameObject.Find("Duck(Clone)"));
   Assert.AreEqual(3,Model.ColumnCount);Assert.AreEqual(5,Model.Slots.Count);Assert.AreEqual(3,level.Hud.VisibleRows);
   for(int c=0;c<3;c++){int id=level.Hud.DisplayedHive(c,0);Assert.GreaterOrEqual(id,0);Assert.AreEqual(Model.Hives[id].Remaining.ToString(),level.Hud.QueueLabels[c].text);}
   Assert.IsFalse(level.Hud.QueueButtons[3].gameObject.activeSelf);Assert.IsFalse(level.Hud.QueueButtons[4].gameObject.activeSelf);
   yield return HiveInputFixture.Click(level.Hud,0);BeeSceneSteps.Until(()=>Model.Deposited>0,30);
   Assert.Greater(Model.Deposited,0);Assert.AreEqual(812,Model.Remaining+Model.InTransit+Model.Deposited);Assert.Less(Model.Remaining,812);
  }
  [UnityTest] public IEnumerator BananaRetryAndLeaveKeepTheCorrectLevelAndInventory(){
   yield return Load();var app=BeeHarvestApp.Interface;var level=Object.FindObjectOfType<LevelOneController>();
   app.SendCommand(new RefillHarvestBoosterCommand(1));level.Hud.Boosters.Buttons[1].onClick.Invoke();int count=Model.BoosterCount(1);Assert.IsTrue(Model.SpeedBoost);
   Assert.IsTrue(level.TryRetryLevel());yield return null;yield return null;Assert.AreEqual(812,Model.Remaining);Assert.AreEqual(count,Model.BoosterCount(1));Assert.IsFalse(Model.SpeedBoost);
   yield return HiveInputFixture.Click(level.Hud,0);var settings=Object.FindObjectOfType<SettingsPopupController>();settings.OpenButton.onClick.Invoke();settings.HomeButton.onClick.Invoke();settings.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;
   int generation=Model.Generation,remaining=Model.Remaining;var page=Object.FindObjectOfType<MainMenuPage>();Assert.AreEqual("Level 3",page.StartGameButton.GetComponentInChildren<TMP_Text>().text);
   page.StartGameButton.onClick.Invoke();yield return null;yield return null;Assert.AreEqual("Level03",SceneManager.GetActiveScene().name);Assert.AreEqual(generation,Model.Generation);Assert.AreEqual(remaining,Model.Remaining);Assert.AreEqual(count,Model.BoosterCount(1));
  }
  [UnityTest] public IEnumerator SecondLevelVictoryActuallyLoadsBananaAndConsumesContinueOnce(){
   SceneManager.LoadScene("Level02");yield return null;yield return null;var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var app=BeeHarvestApp.Interface;
   app.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(Model.Won);yield return null;
   var popup=Object.FindObjectOfType<VictoryPopupController>();float deadline=Time.realtimeSinceStartup+3;while(!popup.ContinueButton.interactable&&Time.realtimeSinceStartup<deadline)yield return null;
   Assert.IsTrue(popup.ContinueButton.interactable,HiveInputFixture.ObservedInputState());popup.ContinueButton.onClick.Invoke();popup.ContinueButton.onClick.Invoke();deadline=Time.realtimeSinceStartup+5;
   while(SceneManager.GetActiveScene().name!="Level03"&&Time.realtimeSinceStartup<deadline)yield return null;yield return null;yield return null;
   Assert.AreEqual("Level03",SceneManager.GetActiveScene().name);Assert.AreEqual(3,Object.FindObjectOfType<LevelOneController>().LevelNumber);Assert.AreEqual(812,Model.Remaining);Assert.IsFalse(Model.Won);
  }
 }
}
