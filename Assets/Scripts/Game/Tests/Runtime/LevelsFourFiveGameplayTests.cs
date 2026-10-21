using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class LevelsFourFiveGameplayTests : EconomyTestFixture {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static IEnumerator Load(int number){string scene="Level"+number.ToString("00");Assert.IsTrue(Application.CanStreamedLevelBeLoaded(scene),"New level scene must be playable");SceneManager.LoadScene(scene);yield return null;yield return null;}
  [UnityTest] public IEnumerator BothNewScenesUseTheirOwnModelAndActuallyDeliver(){
   var names=new[]{"Bee","Watermelon"};var counts=new[]{1105,2840};var hives=new[]{15,51};
   for(int i=0;i<2;i++){
    yield return Load(i+4);var level=Object.FindObjectOfType<LevelOneController>();var hud=level.Hud;Assert.AreEqual(i+4,level.LevelNumber);Assert.AreEqual("Level "+(i+4),hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);
    Assert.AreEqual(counts[i],Model.Remaining);Assert.AreEqual(hives[i],Model.Hives.Count);Assert.AreEqual(3,Model.ColumnCount);Assert.AreEqual(5,Model.Slots.Count);Assert.AreEqual(3,hud.VisibleRows);
    var parts=level.GetComponent<HarvestBoardView>().GetComponentsInChildren<Renderer>().Where(r=>r.name.StartsWith(names[i]+"V2_Voxel_")).ToArray();Assert.AreEqual(counts[i],parts.Length);Assert.AreEqual(counts[i],parts.Select(r=>r.name).Distinct().Count());Assert.IsNull(GameObject.Find("Banana(Clone)"));
    Assert.IsFalse(hud.QueueButtons[3].gameObject.activeSelf);Assert.IsFalse(hud.QueueButtons[4].gameObject.activeSelf);
    yield return HiveInputFixture.Click(hud,0);BeeSceneSteps.Until(()=>Model.Deposited>0,30);Assert.Greater(Model.Deposited,0);Assert.AreEqual(counts[i],Model.Remaining+Model.InTransit+Model.Deposited);
   }
  }
  [UnityTest] public IEnumerator BananaThenBeeVictoriesLoadTheFollowingSceneOnlyOnce(){
   for(int from=3;from<=4;from++){
    yield return Load(from);var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var app=BeeHarvestApp.Interface;app.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
    app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(Model.Won);yield return null;
    var popup=Object.FindObjectOfType<VictoryPopupController>();float deadline=Time.realtimeSinceStartup+3;while(!popup.ContinueButton.interactable&&Time.realtimeSinceStartup<deadline)yield return null;Assert.IsTrue(popup.ContinueButton.interactable);
    popup.ContinueButton.onClick.Invoke();popup.ContinueButton.onClick.Invoke();string next="Level"+(from+1).ToString("00");deadline=Time.realtimeSinceStartup+5;while(SceneManager.GetActiveScene().name!=next&&Time.realtimeSinceStartup<deadline)yield return null;yield return null;yield return null;
    Assert.AreEqual(next,SceneManager.GetActiveScene().name);Assert.AreEqual(from+1,Object.FindObjectOfType<LevelOneController>().LevelNumber);Assert.AreEqual(from==3?1105:2840,Model.Remaining);Assert.IsFalse(Model.Won);
   }
  }
  [UnityTest] public IEnumerator WatermelonLinkedPairWaitsForBothFrontsAndCommitsTogether(){
   yield return Load(5);var level=Object.FindObjectOfType<LevelOneController>();var hop=level.GetComponent<HiveHopController>();
   Assert.IsNotNull(GameObject.Find("HiveChainLink_1_20"));hop.ClickWaiting(20);hop.Tick(.5f);Assert.IsFalse(Model.Hives[1].Started);Assert.IsFalse(Model.Hives[20].Started);Assert.IsTrue(Model.Slots.All(s=>s<0));
   yield return HiveInputFixture.Click(level.Hud,0);
   hop.ClickWaiting(20);hop.Tick(.5f);Assert.IsFalse(Model.Hives[20].Started,"Hive 20 is still behind hives 18 and 19");
   yield return HiveInputFixture.Click(level.Hud,1);yield return HiveInputFixture.Click(level.Hud,1);
   Assert.AreEqual(20,BeeHarvestApp.Interface.GetSystem<HarvestSystem>().Front(1));level.enabled=false;
   hop.ClickWaiting(20);hop.ClickWaiting(1);hop.Tick(.12f);Assert.IsFalse(Model.Hives[1].Started);Assert.IsFalse(Model.Hives[20].Started);
   hop.Tick(.23f);Assert.IsTrue(Model.Hives[1].Started);Assert.IsTrue(Model.Hives[20].Started);Assert.AreEqual(5,Model.Hives.Count(h=>h.Started));Assert.AreNotEqual(Model.Hives[1].Slot,Model.Hives[20].Slot);
  }
  // Keep every source cube, quota and link; only promote the selected hives for isolated hop boundaries.
  static LevelData PromoteWatermelonHives(params int[] first){
   var source=JsonUtility.FromJson<LevelData>(JsonUtility.ToJson(BeeHarvestApp.Interface.GetUtility<LevelOneAssets>().Level(5)));
   var order=first.Concat(Enumerable.Range(0,source.hives.Length).Except(first)).ToArray();
   var remap=new int[order.Length];for(int i=0;i<order.Length;i++)remap[order[i]]=i;
   var reordered=order.Select(i=>source.hives[i]).ToArray();
   foreach(var hive in reordered)hive.connected=(hive.connected??new int[0]).Select(i=>remap[i]).ToArray();
   source.hives=reordered;return source;
  }
  [UnityTest] public IEnumerator WatermelonTripleRespectsCapacityAndCancelsAsOneOnPauseBackgroundAndRetry(){
   yield return Load(5);var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;
   var app=BeeHarvestApp.Interface;var hop=level.GetComponent<HiveHopController>();var view=level.GetComponent<HiveModelView>();
   app.SendCommand(new StartLevelCommand(PromoteWatermelonHives(0,18,36,6,24,39)));level.Hud.Sync();view.Sync();
   foreach(int id in new[]{0,1,2})app.SendCommand(new ActivateWaitingHiveCommand(id,id));
   hop.ClickWaiting(3);hop.Tick(.5f);Assert.AreEqual(3,Model.Hives.Count(h=>h.Started));
   Assert.IsTrue(new[]{3,4,5}.All(id=>!Model.Hives[id].Started),"Two free slots cannot accept a three-hive group");
   for(int mode=0;mode<3;mode++){
    app.SendCommand(new StartLevelCommand(PromoteWatermelonHives(6,24,39)));level.Hud.Sync();view.Sync();
    var before=Enumerable.Range(0,3).Select(id=>GameObject.Find("SmallHive_"+id).transform.position).ToArray();
    hop.ClickWaiting(1);hop.Tick(.1f);
    Assert.IsTrue(Enumerable.Range(0,3).All(id=>Vector3.Distance(before[id],GameObject.Find("SmallHive_"+id).transform.position)>.02f),"All three hive visuals must move in cancellation case "+mode);
    if(mode==0){app.SendCommand(new PauseHarvestCommand(true));hop.Tick(.01f);app.SendCommand(new PauseHarvestCommand(false));}
    else if(mode==1){hop.SendMessage("OnApplicationPause",true);hop.SendMessage("OnApplicationPause",false);level.SendMessage("LateUpdate");}
    else{int generation=Model.Generation;Assert.IsTrue(level.TryRetryLevel());Assert.Greater(Model.Generation,generation);hop.Tick(.01f);}
    hop.Tick(1);Assert.IsTrue(Model.Hives.All(h=>!h.Started));Assert.IsTrue(Model.Slots.All(s=>s<0));Assert.AreEqual(0,Model.Flights.Count);
    if(mode<2)for(int id=0;id<3;id++)Assert.Less(Vector3.Distance(before[id],GameObject.Find("SmallHive_"+id).transform.position),.005f);
   }
   app.SendCommand(new StartLevelCommand(PromoteWatermelonHives(6,24,39)));level.Hud.Sync();view.Sync();
   hop.ClickWaiting(2);hop.Tick(.35f);Assert.AreEqual(3,Model.Hives.Count(h=>h.Started));
   Assert.AreEqual(3,Enumerable.Range(0,3).Select(id=>Model.Hives[id].Slot).Distinct().Count());
  }

 }
}
