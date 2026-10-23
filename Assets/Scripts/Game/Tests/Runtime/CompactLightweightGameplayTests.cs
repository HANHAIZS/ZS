using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class CompactLightweightGameplayTests : EconomyTestFixture {
  static LevelOneController Level=>Object.FindObjectOfType<LevelOneController>();
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Level.GetComponent<HarvestBoardView>();
  static BeeFlightView Bees=>Level.GetComponent<BeeFlightView>();
  static HiveModelView Hives=>Level.GetComponent<HiveModelView>();
  static float Center(RectTransform r)=>(r.anchorMin.y+r.anchorMax.y)*.5f;
  static IEnumerator Load(int n){LevelOneAssets.ReloadPresentationCatalog();LevelSceneTestLoader.Load("Level"+n);yield return null;yield return null;Level.enabled=false;Board.AutoRotate=false;Board.ResetRotation();Board.PublishCameraView();}
  static FlightState Select(Transform root){var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();for(int turn=0;turn<8;turn++){Board.PublishCameraView();for(int c=0;c<3;c++){int h=rules.Front(c);if(h>=0&&rules.CanSelectHive(h)&&rules.FindTarget(Model.Hives[h].Data.color)>=0){app.SendCommand(new ActivateHiveCommand(c));Hives.Sync();Board.Sync();Bees.RenderFlights();return Model.Flights[0];}}root.Rotate(0,45,0);}Assert.Fail("Visible hive required");return null;}
  [UnityTest] public IEnumerator AllFiveCompactLayoutsKeepTheLargerArtClearAndUseSlowerRealSourcePickup(){
   foreach(int n in new[]{115,116,117,118,119}){
    yield return Load(n);var hud=Level.Hud;var root=GameObject.Find(LevelOneAssets.ModelKey(n)+"(Clone)").transform;var rs=root.GetComponentsInChildren<Renderer>();var original=Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.ModelKey(n));var byName=original.GetComponentsInChildren<Renderer>().ToDictionary(r=>r.name);var outline=Resources.Load<Material>("Materials/ChickVoxelOutline");
    Assert.AreEqual(.3495f,Center(hud.SlotLabels[0].rectTransform),.0001f);Assert.AreEqual(.274f,Center(hud.RowControl(0,0)),.0001f);Assert.AreEqual(.204f,Center(hud.RowControl(0,1)),.0001f);Assert.AreEqual(.134f,Center(hud.RowControl(0,2)),.0001f);
    for(int row=0;row<2;row++)Assert.GreaterOrEqual(hud.RowControl(0,row).anchorMin.y,hud.RowControl(0,row+1).anchorMax.y,"Queue click targets must not overlap vertically");
    Assert.Less(Vector3.Distance(Vector3.one*(.86f*1.04f*(n<=117?1.28f:1.18f)),root.localScale),.00001f);Assert.AreEqual(LightShadows.Soft,GameObject.Find("KeyLight").GetComponent<Light>().shadows);Assert.AreEqual(.3f,GameObject.Find("KeyLight").GetComponent<Light>().shadowStrength);Assert.Less(Vector4.Distance(new Color(.48f,.52f,.63f),RenderSettings.ambientLight),.0001f);
    foreach(var r in rs){Assert.AreEqual("BeeHarvest/VoxelCavity",r.sharedMaterial.shader.name);Assert.AreEqual(outline.GetColor("_EdgeColor"),r.sharedMaterial.GetColor("_EdgeColor"));Assert.Less(Vector3.Distance(byName[r.name].transform.localScale*(.93f/.975f),r.transform.localScale),.00001f);}
    float minX=1,maxX=0,minY=1,maxY=0;
    for(int turn=0;turn<16;turn++){
     root.rotation=Quaternion.Euler(0,turn*22.5f,0);foreach(var r in rs){var b=r.bounds;for(int i=0;i<8;i++){var point=Camera.main.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));minX=Mathf.Min(minX,point.x);maxX=Mathf.Max(maxX,point.x);minY=Mathf.Min(minY,point.y);maxY=Mathf.Max(maxY,point.y);}}
    }
    TestContext.WriteLine("Level "+n+" rotation envelope x="+minX+".."+maxX+", y="+minY+".."+maxY);Assert.Greater(minX,.05f);Assert.Less(maxX,.95f);Assert.Greater(minY,.43f);Assert.Less(maxY,.85f);
    root.rotation=Quaternion.identity;var f=Select(root);var source=rs.Single(r=>r.name==Model.Cubes[f.Cube].Data.name);var from=source.transform.position;var rotation=source.transform.rotation;BeeSceneSteps.FirstPickup();var cargo=GameObject.Find("BlenderBeeCarrier_"+f.Id).transform.Find("CarriedVoxel");var grip=cargo.parent.Cast<Transform>().Single(t=>t!=cargo).Find("GripAnchor");float started=f.Age;Assert.IsFalse(source.enabled);Assert.Less(Vector3.Distance(from,cargo.position),.00001f);Assert.Less(Quaternion.Angle(rotation,cargo.rotation),.001f);
    BeeSceneSteps.Advance(.06f);Assert.Less(Vector3.Distance(Vector3.Lerp(from,grip.position,.5f),cargo.position),.0003f,"Halfway through the new120ms handoff");var app=BeeHarvestApp.Interface;app.SendCommand(new PauseHarvestCommand(true));var paused=cargo.position;BeeSceneSteps.Advance(1);Assert.AreEqual(paused,cargo.position);app.SendCommand(new PauseHarvestCommand(false));BeeSceneSteps.Advance(.061f);Assert.Less(Vector3.Distance(grip.position,cargo.position),.00001f);Assert.AreEqual(0,f.TransportPhase);Assert.AreEqual("BeeHarvest/VoxelEdges",cargo.GetComponent<Renderer>().sharedMaterial.shader.name);
    Level.RestartLevel();hud.Sync();Hives.Sync();Board.Sync();Bees.RenderFlights();Assert.AreEqual(rs.Length,Model.Remaining);Assert.AreEqual(0,Model.Deposited);Assert.AreEqual(.3495f,Center(hud.SlotLabels[0].rectTransform),.0001f,"Retry must not apply the downward offset a second time");Assert.IsTrue(rs.All(r=>r.enabled));
   }
   yield return Load(13);Assert.AreEqual(.3975f,Center(Level.Hud.SlotLabels[0].rectTransform),.0001f);Assert.AreEqual(.315f,Center(Level.Hud.RowControl(0,0)),.0001f);Assert.AreEqual(.18f,Model.Timing.DepartureSeconds);Assert.AreEqual(.09f,Model.Timing.HandoffSeconds);
  }
 }
}
