using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class LevelLayoutTests : EconomyTestFixture {
  static IEnumerator Load(int level){SceneManager.LoadScene("Level0"+level);yield return null;yield return null;Canvas.ForceUpdateCanvases();}
  static Bounds Geometry(Renderer[] renderers){var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);return bounds;}
  static Vector2 Extent(Bounds bounds){float lo=float.MaxValue,hi=float.MinValue;for(int i=0;i<8;i++){var p=Camera.main.WorldToScreenPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));lo=Mathf.Min(lo,p.y);hi=Mathf.Max(hi,p.y);}return new Vector2(lo,hi);}
  static Vector2 Extent(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);return new Vector2(corners[0].y,corners[1].y);}
  [UnityTest] public IEnumerator FlowerHasThreePlanningRowsWithConservedQuotas(){
   yield return Load(1);var model=BeeHarvestApp.Interface.GetModel<HarvestModel>();var hud=Object.FindObjectOfType<LevelOneHud>();
   Assert.AreEqual(3,hud.VisibleRows);Assert.AreEqual(30,model.Hives.Count);Assert.AreEqual(1784,model.Cubes.Count);
   foreach(var color in model.Cubes.GroupBy(c=>c.Data.color))Assert.AreEqual(color.Count(),model.Hives.Where(h=>h.Data.color==color.Key).Sum(h=>h.Data.quota),color.Key);
   for(int c=0;c<3;c++){Assert.AreEqual(new[]{11,8,11}[c],model.WaitingColumn(c).Count);int third=hud.DisplayedHive(c,2);Assert.IsNotNull(GameObject.Find("SmallHive_"+third));Assert.AreEqual(model.Hives[third].Remaining.ToString(),hud.ThirdLabels[c].text);Assert.IsFalse(hud.ThirdButtons[c].interactable);}
  }
  [UnityTest] public IEnumerator ThirdRowOverlapsButtonsWhileBackgroundStaysBelowHives(){
   foreach(int level in new[]{1,2}){
    yield return Load(level);var hud=Object.FindObjectOfType<LevelOneHud>();Assert.AreEqual(3,hud.VisibleRows);
    var app=BeeHarvestApp.Interface;app.SendCommand(new RefillHarvestBoosterCommand(0));app.SendCommand(new AddHiveSlotCommand());app.SendCommand(new RefillHarvestBoosterCommand(3));app.SendCommand(new UseFreeHiveSelectionCommand());yield return null;
    var hive=GameObject.Find("SmallHive_"+hud.DisplayedHive(0,2));var shape=Geometry(hive.GetComponentsInChildren<Renderer>());var hiveY=Extent(shape);
    var background=(RectTransform)hud.Boosters.transform.Find("ToolbarBase");var baseY=Extent(background);var button=(RectTransform)hud.Boosters.Buttons[0].transform;var buttonY=Extent(button);
    Assert.Less(baseY.y,hiveY.x,"Blue background must end below the entire third-row hive");
    Assert.AreEqual(button.position.y,baseY.y,Camera.main.pixelHeight*.01f,"Background rim passes through the button midline");
    Assert.Greater(buttonY.y,hiveY.x);Assert.Less(buttonY.y,hiveY.y,"Button upper half partially covers the hive");
    var quota=hud.ThirdLabels[0];Assert.Greater(quota.rectTransform.position.y-quota.fontSize*quota.GetComponentInParent<Canvas>().scaleFactor*.5f,buttonY.y,"Quota stays above the covering button");
    var point=new Vector2(Camera.main.WorldToScreenPoint(shape.center).x,buttonY.y-5);
    var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
    Assert.IsNotEmpty(hits);Assert.IsTrue(hits[0].gameObject.transform.IsChildOf(hud.Boosters.transform),"Covered hive area is owned by a booster button; actual hit: "+hits[0].gameObject.name);
   }
  }
  [UnityTest] public IEnumerator CollectorAndSettingsStayAtTopWhileModelIsCenteredAboveSlots(){
   foreach(int level in new[]{1,2}){
    yield return Load(level);var hud=Object.FindObjectOfType<LevelOneHud>();var board=Object.FindObjectOfType<HarvestBoardView>();
    // Collector is the separately spawned art root, not the small-hive view.
    var collectorRoot=board.transform.Cast<Transform>().First(t=>t.name.StartsWith("Collector"));var collectorY=Extent(Geometry(collectorRoot.GetComponentsInChildren<Renderer>()));
    var gear=(RectTransform)Object.FindObjectOfType<SettingsPopupController>().OpenButton.transform;Assert.AreEqual(1,gear.anchorMin.y,.001f);Assert.AreEqual(1,gear.anchorMax.y,.001f);
    Assert.Greater((collectorY.x+collectorY.y)*.5f/Camera.main.pixelHeight,.925f,"Collector stays in the top band");
    var modelY=Extent(Geometry(board.GetComponentsInChildren<Renderer>().Where(r=>r.name.StartsWith(LevelOneAssets.ModelKey(level)+"V2_Voxel_")).ToArray()));var slotY=Extent(hud.SlotTargets[2]);
    Assert.Greater(modelY.x,slotY.y);Assert.Less(modelY.y,collectorY.x);
    Assert.AreEqual((slotY.y+collectorY.x)*.5f,(modelY.x+modelY.y)*.5f,Camera.main.pixelHeight*.025f,"Model is centered between collector and slots");
   }
  }
 }
}
