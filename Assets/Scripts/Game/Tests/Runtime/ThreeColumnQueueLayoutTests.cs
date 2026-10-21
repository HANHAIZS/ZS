using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class ThreeColumnQueueLayoutTests:EconomyTestFixture {
  static IEnumerator Load(int number){SceneManager.LoadScene(LevelOneAssets.SceneName(number));yield return null;yield return null;Canvas.ForceUpdateCanvases();}
  static void CheckLayout(LevelOneHud hud){
   var app=BeeHarvestApp.Interface;Assert.AreEqual(3,app.GetModel<HarvestModel>().ColumnCount);Assert.AreEqual(5,app.GetModel<HarvestModel>().Slots.Count);Assert.AreEqual(3,hud.VisibleRows);
   var edges=new Vector3[4];((RectTransform)hud.transform).GetWorldCorners(edges);float width=edges[3].x-edges[0].x;float[] centers={.20f,.50f,.80f};
   for(int c=0;c<3;c++)for(int row=0;row<3;row++){
    var control=hud.RowControl(c,row);control.GetWorldCorners(edges);var safe=new Vector3[4];((RectTransform)hud.transform).GetWorldCorners(safe);Assert.AreEqual(centers[c],(control.anchorMin.x+control.anchorMax.x)*.5f,.002f,"All row anchors follow the column layout");int id=hud.DisplayedHive(c,row);if(id>=0){float x=Camera.main.WorldToScreenPoint(GameObject.Find("SmallHive_"+id).transform.position).x;Assert.AreEqual(centers[c],(x-safe[0].x)/width,.002f,"The hive itself is centered; its quota label follows the original plaque offset");}Assert.AreEqual(.17f,(edges[3].x-edges[0].x)/width,.002f,"Closing the gaps must preserve hive size");
   }
   foreach(int c in new[]{3,4}){Assert.IsFalse(hud.QueueButtons[c].gameObject.activeSelf);Assert.IsFalse(hud.NextLabels[c].gameObject.activeSelf);Assert.IsFalse(hud.ThirdLabels[c].gameObject.activeSelf);}
  }
  [UnityTest] public IEnumerator ThreeColumnsAreCloserWhilePointerTargetsAndSlotLayoutRemainIntact(){
   yield return Load(1);var level=Object.FindObjectOfType<LevelOneController>();var hud=level.Hud;CheckLayout(hud);
   for(int c=0;c<3;c++){
    var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,hud.QueueButtons[c].transform.position)};var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
    Assert.IsTrue(hits.Any(h=>h.gameObject==hud.QueueButtons[c].gameObject||h.gameObject.transform.IsChildOf(hud.QueueButtons[c].transform)),"Each visible hive retains its own clickable column");
    foreach(int other in Enumerable.Range(0,3).Where(i=>i!=c))Assert.IsFalse(RectTransformUtility.RectangleContainsScreenPoint((RectTransform)hud.QueueButtons[other].transform,pointer.position,null),"Closing gaps must not overlap neighboring click areas");
   }
   level.RestartLevel();yield return null;CheckLayout(hud);
  }
  [UnityTest] public IEnumerator AllNinetyFiveScenesLoadThreePlanningColumnsAfterEachGeneration(){for(int n=1;n<=95;n++){yield return Load(n);CheckLayout(Object.FindObjectOfType<LevelOneController>().Hud);}}
 }
}
