using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class BeeRouteContinuityTests:EconomyTestFixture {
  static HarvestModel M=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Object.FindObjectOfType<HarvestBoardView>();
  static void Draw(){Object.FindObjectOfType<HiveModelView>().Sync();Board.Sync();Object.FindObjectOfType<BeeFlightView>().RenderFlights();}
  static void Tick(float dt){Board.PublishCameraView();BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(dt));Draw();}
  public static LevelData StemData(){var data=JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("Levels/Level01").text);foreach(var c in data.cubes)if(c.color=="Palette17"&&c.id!=406)c.color="Palette01";var list=new System.Collections.Generic.List<HiveData>{new HiveData{column=0,color="Palette17",quota=1}};foreach(var h in data.hives)if(h.color!="Palette17"){h.connected=new int[0];list.Add(h);}list.First(h=>h.color=="Palette01").quota+=313;data.hives=list.ToArray();return data;}
  static IEnumerator Load(bool single){SceneManager.LoadScene("Level01");yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;Board.AutoRotate=false;if(single)BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(StemData()));Board.ResetRotation();Board.PublishCameraView();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Draw();}
  [UnityTest] public IEnumerator DestroyingTheFlightViewReleasesPhysicalArrivalRequirement(){
   yield return Load(true);Object.Destroy(Object.FindObjectOfType<BeeFlightView>());yield return null;
   Assert.IsFalse(M.RequireFlightArrival,"A destroyed scene must not leave pure rule simulation waiting for an absent renderer");
   BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="pink"}},hives=new[]{new HiveData{column=0,color="pink",quota=1}}}));
   BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));
   for(int i=0;i<100&&!M.Won;i++)BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(.1f));
   Assert.IsTrue(M.Won);
  }
  [UnityTest] public IEnumerator LoadedBeeActuallyReachesTheOutsideWaypointBeforeClimbing(){
   yield return Load(true);BeeSceneSteps.FirstPickup();var f=M.Flights[0];
   BeeSceneSteps.Until(()=>f.TransportPhase==2&&f.ClearedSource);
   var cargo=GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel");var exit=Board.CarryLanePoint(Board.PickupPose(f.Cube).position+Board.PickupOutward(f)*.24f,f.Lane,f.Id);
   Assert.Less(Vector3.Distance(cargo.position,exit),.03f,"A capped speed must not shortcut the exterior waypoint when its timer expires");
  }
  [UnityTest] public IEnumerator NarrowingTheViewportDoesNotStrandALoadedOrReturningBee(){
   yield return Load(true);BeeSceneSteps.FirstPickup();var f=M.Flights[0];
   BeeSceneSteps.Until(()=>f.TransportPhase==2);var camera=Camera.main;float old=camera.aspect;
   try{camera.aspect=.28f;BeeSceneSteps.Until(()=>f.Stage==2,20);BeeSceneSteps.Until(()=>GameObject.Find("BlenderBeeCarrier_0")==null,20);Assert.AreEqual(1,M.Deposited);}finally{camera.aspect=old;}
  }
  // Fails on the measured 39px/20ms stem approach, and on any reintroduced hard clearance relocation.
  [UnityTest] public IEnumerator SingleRedBeeNearTheStemKeepsContinuousReadableSpeed(){
   yield return Load(true);Assert.AreEqual(406,M.Flights[0].Cube);var actor=GameObject.Find("BlenderBeeCarrier_0").transform;var previous=Camera.main.WorldToScreenPoint(actor.position);int checks=0;
   for(int i=0;i<450&&M.Flights[0].Stage!=2;i++){Tick(.02f);var p=Camera.main.WorldToScreenPoint(actor.position);Assert.Less(Vector2.Distance(previous,p),Camera.main.pixelHeight*.034f,"Root jumped or crossed the model too fast at frame "+i+" stage "+M.Flights[0].Stage);previous=p;checks++;}
   Assert.AreEqual(2,M.Flights[0].Stage);Assert.Greater(checks,80);Assert.AreEqual(1,M.Deposited);
  }
  // Catches the original three-bee Step0.62 clearance jump around 5.88s without assuming a Stage transition.
  [UnityTest] public IEnumerator SameColorCarriersNeverJumpWhenAnotherBeeOccupiesTheNextPose(){
   yield return Load(false);var prior=new System.Collections.Generic.Dictionary<int,Vector3>();
   for(int i=0;i<600;i++){Tick(.02f);foreach(var f in M.Flights){if(f.Stage==2)continue;var actor=GameObject.Find("BlenderBeeCarrier_"+f.Id).transform;Vector3 last;if(prior.TryGetValue(f.Id,out last))Assert.Less(Vector3.Distance(last,actor.position),.481f,"Clearance relocation: task "+f.Id+" at "+i*.02f);prior[f.Id]=actor.position;}}
   Assert.Greater(M.Deposited,1);
  }
 }
}
