using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class SelectedSourceHandoffTests : EconomyTestFixture {
  static LevelOneController Level=>Object.FindObjectOfType<LevelOneController>();
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Level.GetComponent<HarvestBoardView>();
  static BeeFlightView Bees=>Level.GetComponent<BeeFlightView>();
  static IEnumerator Load(int n){LevelOneAssets.ReloadPresentationCatalog();LevelSceneTestLoader.Load(LevelOneAssets.LevelKey(n));yield return null;yield return null;Level.enabled=false;Board.AutoRotate=false;Board.ResetRotation();Board.PublishCameraView();}
  static FlightState Select(){var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();var root=GameObject.Find(LevelOneAssets.ModelKey(Level.LevelNumber)+"(Clone)").transform;for(int turn=0;turn<8;turn++){Board.PublishCameraView();for(int c=0;c<3;c++){int h=rules.Front(c);if(h>=0&&rules.CanSelectHive(h)&&rules.FindTarget(Model.Hives[h].Data.color)>=0){app.SendCommand(new ActivateHiveCommand(c));Level.GetComponent<HiveModelView>().Sync();Board.Sync();Bees.RenderFlights();return Model.Flights[0];}}root.Rotate(0,45,0);}Assert.Fail("Visible selectable hive required at "+Level.LevelNumber);return null;}
  [UnityTest] public IEnumerator CompactSlotsKeepHopLandingExtraSlotAndRetryAligned(){
   yield return Load(1);var app=BeeHarvestApp.Interface;var hud=Level.Hud;var view=Level.GetComponent<HiveModelView>();
   var hop=Level.GetComponent<HiveHopController>();int hive=app.GetSystem<HarvestSystem>().Front(0);hop.ClickWaiting(hive);hop.Tick(.341f);
   Assert.AreEqual(hive,Model.Slots[0]);var target=hud.SlotTargets[0].position;
   Assert.Less(Vector2.Distance(Camera.main.WorldToScreenPoint(GameObject.Find("SmallHive_"+hive).transform.position),target),1,"Hop lands at the compact slot target");
   app.SendCommand(new AddHiveSlotCommand());hud.Sync();view.Sync();Assert.AreEqual(6,Model.Slots.Count);Assert.IsTrue(hud.SlotTargets[5].gameObject.activeSelf);
   Assert.AreEqual(hud.SlotTargets[0].position.x,hud.SlotTargets[5].position.x,.1f);Assert.Greater(hud.SlotTargets[5].position.y,hud.SlotTargets[0].position.y);
   for(int slot=0;slot<5;slot++){var tile=GameObject.Find("HiveSlotTile_"+slot);Assert.Less(Vector2.Distance(Camera.main.WorldToScreenPoint(tile.transform.position),hud.SlotTargets[slot].position),1);if(slot<4)Assert.Greater(hud.SlotTargets[slot+1].position.x-hud.SlotTargets[slot].position.x,hud.SlotTargets[slot].rect.width*hud.GetComponentInParent<Canvas>().scaleFactor,"Visible slot tiles retain a gap");}
   var anchors=hud.SlotLabels.Select(label=>label.rectTransform.anchorMin).ToArray();Level.RestartLevel();yield return null;
   Assert.AreEqual(5,Model.Slots.Count);for(int slot=0;slot<6;slot++)Assert.AreEqual(anchors[slot],hud.SlotLabels[slot].rectTransform.anchorMin,"Retry must not accumulate offsets");
  }
  [UnityTest] public IEnumerator AllSelectedLevelsUseExactSourcePoseNinetyMillisecondTransferAndOriginalLift(){
   foreach(int n in Enumerable.Range(1,6).Concat(Enumerable.Range(96,19))){
    yield return Load(n);Assert.AreEqual(.22f,Model.Timing.LiftSeconds);Assert.AreEqual(.09f,Model.Timing.HandoffSeconds);Assert.AreEqual(.26f,Model.Timing.DepartureSeconds);Assert.AreEqual(.18f,Model.Timing.SettleSeconds);Assert.AreEqual(.16f,Model.Timing.ReachSeconds);var hud=Level.Hud;Assert.AreEqual(.3495f,(hud.SlotLabels[0].rectTransform.anchorMin.y+hud.SlotLabels[0].rectTransform.anchorMax.y)*.5f,.0001f);
    for(int row=0;row<3;row++){var r=hud.RowControl(0,row);Assert.AreEqual(.274f-row*.07f,(r.anchorMin.y+r.anchorMax.y)*.5f,.0001f);if(row<2)Assert.GreaterOrEqual(r.anchorMin.y,hud.RowControl(0,row+1).anchorMax.y);}
    for(int c=0;c<3;c++)for(int row=0;row<3;row++){var control=hud.RowControl(c,row);Assert.AreEqual(.5f+(c-1)*.24f,(control.anchorMin.x+control.anchorMax.x)*.5f,.0001f,"Compact hive column "+n);Assert.AreEqual(.17f,control.anchorMax.x-control.anchorMin.x,.0001f);}
    for(int slot=0;slot<6;slot++){var control=hud.SlotLabels[slot].rectTransform;Assert.AreEqual(.5f+((slot<5?slot:0)-2)*.16f,(control.anchorMin.x+control.anchorMax.x)*.5f,.0001f,"Centered compact slots "+n);Assert.AreEqual(.17f,control.anchorMax.x-control.anchorMin.x,.0001f,"Preserve slot width");}
    var root=GameObject.Find(LevelOneAssets.ModelKey(n)+"(Clone)").transform;var rs=root.GetComponentsInChildren<Renderer>();var originals=Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.ModelKey(n)).GetComponentsInChildren<Renderer>().ToDictionary(r=>r.name);
    foreach(var r in rs){Assert.AreEqual("BeeHarvest/VoxelCavity",r.sharedMaterial.shader.name);Assert.Less(Vector4.Distance(new Color(.2f,.2f,.2f,1),r.sharedMaterial.GetColor("_EdgeColor")),.00001f);Assert.Less(Vector3.Distance(originals[r.name].transform.localScale*(.93f/.975f),r.transform.localScale),.00001f);Assert.AreEqual(originals[r.name].transform.localPosition,r.transform.localPosition);}
    var light=GameObject.Find("KeyLight").GetComponent<Light>();Assert.AreEqual(LightShadows.Soft,light.shadows);Assert.AreEqual(.3f,light.shadowStrength);Assert.Less(Vector4.Distance(new Color(.48f,.52f,.63f,1),RenderSettings.ambientLight),.00001f);
    float loX=1,hiX=0,loY=1,hiY=0;for(int turn=0;turn<16;turn++){root.rotation=Quaternion.Euler(0,turn*22.5f,0);foreach(var r in rs){var b=r.bounds;for(int corner=0;corner<8;corner++){var q=Camera.main.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1)));loX=Mathf.Min(loX,q.x);hiX=Mathf.Max(hiX,q.x);loY=Mathf.Min(loY,q.y);hiY=Mathf.Max(hiY,q.y);}}}
    Assert.Greater(loX,.05f,"left rotation boundary "+n);Assert.Less(hiX,.95f);Assert.Greater(loY,.43f,"bottom rotation boundary "+n);Assert.Less(hiY,.85f);TestContext.WriteLine("Level "+n+" rotation="+loX+","+hiX+","+loY+","+hiY);root.rotation=Quaternion.identity;
    var f=Select();var source=GameObject.Find(Model.Cubes[f.Cube].Data.name).GetComponent<Renderer>();var from=source.transform.position;var rotation=source.transform.rotation;var scale=source.transform.lossyScale;var material=source.sharedMaterial;BeeSceneSteps.FirstPickup();
    var cargo=GameObject.Find("BlenderBeeCarrier_"+f.Id).transform.Find("CarriedVoxel");var renderer=cargo.GetComponent<Renderer>();var grip=cargo.parent.Cast<Transform>().Single(t=>t!=cargo).Find("GripAnchor");float start=f.Age;
    Assert.Less(Vector3.Distance(from,cargo.position),.00001f,"Source takeover at level "+n);Assert.Less(Quaternion.Angle(rotation,cargo.rotation),.001f);Assert.Less(Vector3.Distance(scale,cargo.lossyScale),.00001f);Assert.IsFalse(source.enabled);Assert.AreSame(material,renderer.sharedMaterial);Assert.AreSame(source.GetComponent<MeshFilter>().sharedMesh,cargo.GetComponent<MeshFilter>().sharedMesh);
    var block=new MaterialPropertyBlock();
    foreach(float dt in new[]{.03f,.03f,.031f}){BeeSceneSteps.Advance(dt);float t=Mathf.Clamp01((f.Age-start)/.09f);float e=Mathf.SmoothStep(0,1,t);Assert.Less(Vector3.Distance(Vector3.Lerp(from,grip.position,e),cargo.position),.0002f,"90ms source-to-moving-grip interpolation at "+n);Assert.Less(Vector3.Distance(scale,cargo.lossyScale),.00001f);if(t<1&&material.HasProperty("_CavityStrength")){renderer.GetPropertyBlock(block);Assert.AreEqual(1-e,block.GetFloat("_CavityStrength"),.0001f);}}
    Assert.Less(Vector3.Distance(grip.position,cargo.position),.00001f);Assert.AreEqual(LevelOneAssets.HasVoxelEdges(n)?"BeeHarvest/VoxelEdges":"Universal Render Pipeline/Lit",renderer.sharedMaterial.shader.name);Assert.Less(Vector4.Distance(material.GetColor("_BaseColor"),renderer.sharedMaterial.GetColor("_BaseColor")),.00001f);Assert.IsFalse(renderer.HasPropertyBlock());Assert.AreEqual(0,f.TransportPhase,"90ms remains inside220ms lift");TestContext.WriteLine("Level "+n+": exact source,90ms blend,220ms lift; cavity="+material.HasProperty("_CavityStrength"));
   }
  }
 }
}
