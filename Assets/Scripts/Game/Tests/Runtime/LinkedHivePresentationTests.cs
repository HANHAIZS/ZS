using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class LinkedHivePresentationTests : EconomyTestFixture {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static IEnumerator LoadPair(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;
   var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;
   // Preserve the real source geometry; move the existing second hive to column one for an immediate front pair.
   var data=JsonUtility.FromJson<LevelData>(JsonUtility.ToJson(BeeHarvestApp.Interface.GetUtility<LevelOneAssets>().Level(1)));
   foreach(var h in data.hives)h.connected=new int[0];data.hives[1].column=1;data.hives[0].connected=new[]{1};data.hives[1].connected=new[]{0};
   BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(data));level.Hud.Sync();level.GetComponent<HiveModelView>().Sync();
  }
  [UnityTest] public IEnumerator LinkedFrontsHopAndCommitTogetherOnlyOnce(){
   yield return LoadPair();var level=Object.FindObjectOfType<LevelOneController>();var hop=level.GetComponent<HiveHopController>();
   var a=GameObject.Find("SmallHive_0");var b=GameObject.Find("SmallHive_1");var x=a.transform.position;var y=b.transform.position;
   Assert.IsNotNull(GameObject.Find("HiveChainLink_0_1").GetComponent<MeshRenderer>());
   hop.ClickWaiting(0);hop.ClickWaiting(1);hop.Tick(.12f);
   Assert.Greater(Vector3.Distance(x,a.transform.position),.05f);Assert.Greater(Vector3.Distance(y,b.transform.position),.05f);
   Assert.IsTrue(Model.Slots.All(s=>s<0));Assert.IsFalse(Model.Hives[0].Started);Assert.IsFalse(Model.Hives[1].Started);
   hop.Tick(.23f);Assert.AreEqual(2,Model.Hives.Count(h=>h.Started));Assert.AreEqual(0,Model.Slots[0]);Assert.AreEqual(1,Model.Slots[1]);
   Assert.AreEqual(39,Model.Hives[0].Remaining);Assert.AreEqual(79,Model.Hives[1].Remaining,"Both linked hives dispatch immediately through their independent mouths");
   hop.Tick(1);Assert.AreEqual(2,Model.Hives.Count(h=>h.Started));
  }
  [UnityTest] public IEnumerator PauseBackgroundAndNewGenerationCancelBothMembers(){
   yield return LoadPair();var level=Object.FindObjectOfType<LevelOneController>();var hop=level.GetComponent<HiveHopController>();
   var before=new[]{GameObject.Find("SmallHive_0").transform.position,GameObject.Find("SmallHive_1").transform.position};
   for(int mode=0;mode<3;mode++){
    hop.ClickWaiting(0);hop.Tick(.1f);
    if(mode==0){BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(true));hop.Tick(.01f);BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(false));}
    else if(mode==1){hop.SendMessage("OnApplicationPause",true);hop.SendMessage("OnApplicationPause",false);}
    else {var data=JsonUtility.FromJson<LevelData>(JsonUtility.ToJson(BeeHarvestApp.Interface.GetUtility<LevelOneAssets>().Level(1)));BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(data));hop.Tick(.01f);}
    hop.Tick(1);Assert.IsTrue(Model.Hives.All(h=>!h.Started));Assert.IsTrue(Model.Slots.All(s=>s<0));Assert.AreEqual(0,Model.Flights.Count);
    Assert.Less(Vector3.Distance(before[0],GameObject.Find("SmallHive_0").transform.position),.005f);
    if(mode<2)Assert.Less(Vector3.Distance(before[1],GameObject.Find("SmallHive_1").transform.position),.005f);
   }
  }
  [UnityTest] public IEnumerator SourceThreeColumnsAlignAtEntryWithoutAnInitialClick(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;var level=Object.FindObjectOfType<LevelOneController>();var hud=level.Hud;
   Assert.AreEqual(3,Model.ColumnCount);
   for(int c=0;c<3;c++){
    var control=(RectTransform)hud.QueueButtons[c].transform;var root=GameObject.Find("SmallHive_"+hud.DisplayedHive(c,0));
    Assert.Greater(hud.QueueLabels[c].rectTransform.rect.width,50);Assert.Greater(control.rect.width,50);
    Assert.AreEqual(RectTransformUtility.WorldToScreenPoint(null,control.position).x,Camera.main.WorldToScreenPoint(root.transform.position).x,2f);
   }
   Assert.IsFalse(hud.QueueButtons[3].gameObject.activeInHierarchy);Assert.IsFalse(hud.QueueButtons[4].gameObject.activeInHierarchy);
  }
 }
}
