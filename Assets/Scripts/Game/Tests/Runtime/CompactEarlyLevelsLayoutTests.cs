using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class CompactEarlyLevelsLayoutTests:EconomyTestFixture {
  static float X(RectTransform r)=>(r.anchorMin.x+r.anchorMax.x)*.5f;
  static float Y(RectTransform r)=>(r.anchorMin.y+r.anchorMax.y)*.5f;
  [UnityTest] public IEnumerator LevelsSevenThroughTwelveKeepCompactControlsAndLargerRotatingModelsClear(){
   var app=BeeHarvestApp.Interface;
   foreach(int n in Enumerable.Range(7,6)){
    LevelOneAssets.ReloadPresentationCatalog();LevelSceneTestLoader.Load(LevelOneAssets.LevelKey(n));yield return null;yield return null;
    var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var hud=level.Hud;var board=level.GetComponent<HarvestBoardView>();board.AutoRotate=false;board.ResetRotation();board.PublishCameraView();
    Assert.AreEqual(.3495f,Y(hud.SlotLabels[0].rectTransform),.0001f,"Lower slots at "+n);
    for(int c=0;c<3;c++)for(int row=0;row<3;row++){
     var control=hud.RowControl(c,row);Assert.AreEqual(.5f+(c-1)*.24f,X(control),.0001f);Assert.AreEqual(.274f-row*.07f,Y(control),.0001f);Assert.AreEqual(.17f,control.anchorMax.x-control.anchorMin.x,.0001f);
     if(row<2)Assert.Greater(control.anchorMin.y,hud.RowControl(c,row+1).anchorMax.y);if(c<2)Assert.Less(control.anchorMax.x,hud.RowControl(c+1,row).anchorMin.x);
    }
    for(int slot=0;slot<6;slot++){var control=hud.SlotLabels[slot].rectTransform;Assert.AreEqual(.5f+((slot<5?slot:0)-2)*.16f,X(control),.0001f);Assert.AreEqual(.17f,control.anchorMax.x-control.anchorMin.x,.0001f);}
    for(int c=0;c<3;c++){var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,hud.QueueButtons[c].transform.position)};var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Assert.IsTrue(hits.Any(h=>h.gameObject==hud.QueueButtons[c].gameObject||h.gameObject.transform.IsChildOf(hud.QueueButtons[c].transform)),"Front hive remains clickable at "+n);}
    var root=GameObject.Find(LevelOneAssets.ModelKey(n)+"(Clone)").transform;var rs=root.GetComponentsInChildren<Renderer>();var original=Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.ModelKey(n));var byName=original.GetComponentsInChildren<Renderer>().ToDictionary(r=>r.name);
    Assert.Greater(root.localScale.x,.86f);Assert.LessOrEqual(root.localScale.x,.86f*1.3312f+.00001f);foreach(var r in rs){Assert.AreEqual(byName[r.name].transform.localPosition,r.transform.localPosition);Assert.AreEqual(byName[r.name].transform.localScale,r.transform.localScale);Assert.AreEqual("BeeHarvest/VoxelCavity",r.sharedMaterial.shader.name);}
    float loX=1,hiX=0,loY=1,hiY=0;for(int turn=0;turn<16;turn++){root.rotation=Quaternion.Euler(0,turn*22.5f,0);foreach(var r in rs){var b=r.bounds;for(int corner=0;corner<8;corner++){var p=Camera.main.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1)));loX=Mathf.Min(loX,p.x);hiX=Mathf.Max(hiX,p.x);loY=Mathf.Min(loY,p.y);hiY=Mathf.Max(hiY,p.y);}}}
    TestContext.WriteLine("Level "+n+" scale="+root.localScale.x+" bounds="+loX+","+hiX+","+loY+","+hiY);Assert.Greater(loX,.05f);Assert.Less(hiX,.95f);Assert.Greater(loY,.43f);Assert.Less(hiY,.85f);root.rotation=Quaternion.identity;
    var model=app.GetModel<HarvestModel>();var rules=app.GetSystem<HarvestSystem>();int hive=Enumerable.Range(0,3).Select(c=>rules.Front(c)).First(h=>h>=0&&rules.CanSelectHive(h));var hop=level.GetComponent<HiveHopController>();hop.ClickWaiting(hive);hop.Tick(.341f);Assert.IsTrue(model.Hives[hive].Started);int destination=model.Hives[hive].Slot;Assert.Less(Vector2.Distance(Camera.main.WorldToScreenPoint(GameObject.Find("SmallHive_"+hive).transform.position),hud.SlotTargets[destination].position),1);
    app.SendCommand(new RefillHarvestBoosterCommand(0));app.SendCommand(new AddHiveSlotCommand());hud.Sync();level.GetComponent<HiveModelView>().Sync();Assert.AreEqual(6,model.Slots.Count);Assert.AreEqual(hud.SlotTargets[0].position.x,hud.SlotTargets[5].position.x,.1f);
    var anchors=hud.SlotLabels.Select(t=>t.rectTransform.anchorMin).ToArray();level.RestartLevel();yield return null;Assert.AreEqual(5,model.Slots.Count);for(int i=0;i<6;i++)Assert.AreEqual(anchors[i],hud.SlotLabels[i].rectTransform.anchorMin);Assert.AreEqual(.3495f,Y(hud.SlotLabels[0].rectTransform),.0001f);
   }
  }
 }
}
