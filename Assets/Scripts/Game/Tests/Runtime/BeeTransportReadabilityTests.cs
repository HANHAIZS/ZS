using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class BeeTransportReadabilityTests:EconomyTestFixture {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static BeeFlightView Bees=>Object.FindObjectOfType<BeeFlightView>();
  static HarvestBoardView Board=>Object.FindObjectOfType<HarvestBoardView>();
  static Transform Actor=>GameObject.Find("BlenderBeeCarrier_0").transform;
  static Transform Body=>Actor.Cast<Transform>().First(t=>t.name!="CarriedVoxel");
  static Transform Cargo=>Actor.Find("CarriedVoxel");
  static void Draw(){Object.FindObjectOfType<HiveModelView>().Sync();Board.Sync();Bees.RenderFlights();}
  static void Tick(float dt){Board.PublishCameraView();BeeSceneSteps.Advance(dt);Draw();}
  static IEnumerator Load(){SceneManager.LoadScene("Level01");yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;Board.ResetRotation();Board.PublishCameraView();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Draw();}
  static Vector2 HorizontalBounds(){
   var root=GameObject.Find("Flower(Clone)");var b=root.GetComponentsInChildren<Renderer>()[0].bounds;foreach(var r in root.GetComponentsInChildren<Renderer>())b.Encapsulate(r.bounds);
   float lo=1,hi=0;for(int i=0;i<8;i++){var p=Camera.main.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));lo=Mathf.Min(lo,p.x);hi=Mathf.Max(hi,p.x);}return new Vector2(lo,hi);
  }
  [UnityTest] public IEnumerator ArrivalHasAStablePauseBeforeTheHandsReach(){
   yield return Load();BeeSceneSteps.Until(()=>Model.Flights[0].Reached&&Model.Flights[0].Progress>=AdvanceHarvestCommand.ApproachSeconds);var root=Actor.position;var body=Body.localPosition;var source=GameObject.Find(Model.Cubes[Model.Flights[0].Cube].Data.name).GetComponent<Renderer>();
   Tick(.1f);Assert.Less(Vector3.Distance(body,Body.localPosition),.008f,"Arrival needs a distinct still moment before reach");
   Assert.Less(Vector3.Distance(root,Actor.position),.005f);Assert.IsTrue(source.enabled);Assert.IsFalse(Cargo.gameObject.activeSelf);
  }
  [UnityTest] public IEnumerator CargoPullFinishesBeforeTheCarrierLeavesItsSource(){
   yield return Load();var source=GameObject.Find(Model.Cubes[Model.Flights[0].Cube].Data.name).GetComponent<Renderer>();var original=Camera.main.WorldToScreenPoint(source.bounds.center);
   BeeSceneSteps.FirstPickup();Assert.IsFalse(source.enabled);Assert.IsTrue(Cargo.gameObject.activeSelf);
   Assert.Less(Vector2.Distance(original,Camera.main.WorldToScreenPoint(Cargo.GetComponent<Renderer>().bounds.center)),.5f);
   var root=Camera.main.WorldToScreenPoint(Actor.position);Tick(.16f);
   Assert.Greater(Vector2.Distance(root,Camera.main.WorldToScreenPoint(Actor.position)),1f,"Bee and cube pull out together before loaded travel");
   Assert.Greater(Vector2.Distance(original,Camera.main.WorldToScreenPoint(Cargo.GetComponent<Renderer>().bounds.center)),10f,"The block visibly moves away from its old cell");
   Assert.AreEqual(0,Model.Deposited);Assert.AreEqual(Model.Cubes.Count,Model.Remaining+Model.InTransit+Model.Deposited);
  }
  [UnityTest] public IEnumerator LoadedCarrierReachesAnOutsideLaneBeforeClimbingToTheCollector(){
   yield return Load();var extent=HorizontalBounds();BeeSceneSteps.FirstPickup();Tick(AdvanceHarvestCommand.LiftSeconds);
   BeeSceneSteps.Until(()=>Model.Flights[0].TransportPhase==2&&Model.Flights[0].ClearedSource);
   var cargo=Cargo.GetComponent<Renderer>();var shape=GameObject.Find("Flower(Clone)").GetComponentsInChildren<Renderer>()[0].bounds;foreach(var r in GameObject.Find("Flower(Clone)").GetComponentsInChildren<Renderer>())shape.Encapsulate(r.bounds);
   Assert.IsFalse(shape.Intersects(cargo.bounds),"Short loaded travel stays in front of the model without a remote border detour");
   BeeSceneSteps.Until(()=>Model.Flights[0].Stage==2);
   Assert.AreEqual(2,Model.Flights[0].Stage);Assert.Greater(Model.Deposited,0);
  }
  [UnityTest] public IEnumerator BoostedLoadedCarriersKeepTheirWholeGeometryInsideTheScreen(){
   yield return Load();Object.FindObjectOfType<LevelOneController>().RestartLevel();HiveInputFixture.UseFiveColumnGeometryFixture();var app=BeeHarvestApp.Interface;
   app.SendCommand(new RefillHarvestBoosterCommand(0));app.SendCommand(new AddHiveSlotCommand());app.SendCommand(new RefillHarvestBoosterCommand(1));app.SendCommand(new UseHarvestSpeedCommand());
   for(int c=0;c<5;c++)app.SendCommand(new ActivateHiveCommand(c));app.SendCommand(new ActivateHiveCommand(0));Draw();int checkedFrames=0;
   for(int frame=0;frame<1200;frame++){
    Tick(.01f);
    foreach(var f in Model.Flights){
     if(f.Stage!=1||f.Progress<AdvanceHarvestCommand.LiftSeconds+.25f||f.Progress>AdvanceHarvestCommand.LoadedSeconds-.08f)continue;
     var actor=GameObject.Find("BlenderBeeCarrier_"+f.Id);Assert.IsNotNull(actor);var renderers=actor.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
     for(int corner=0;corner<8;corner++){var v=Camera.main.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1)));Assert.Greater(v.x,-.01f,"Loaded carrier clipped left: "+f.Id+" frame "+frame);Assert.Less(v.x,1.01f,"Loaded carrier clipped right: "+f.Id+" frame "+frame);Assert.Greater(v.y,-.01f);Assert.Less(v.y,1.01f);}
     checkedFrames++;
    }
   }
   Assert.Greater(checkedFrames,100,"Exercise many concurrent loaded poses rather than one destination point");
  }
  [UnityTest] public IEnumerator SecondLevelSideLanesKeepTheCompleteCarrierVisibleWhenFiveHivesWorkTogether(){
   SceneManager.LoadScene("Level02");yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;var app=BeeHarvestApp.Interface;Board.ResetRotation();Board.PublishCameraView();
   for(int c=0;c<5;c++)app.SendCommand(new ActivateHiveCommand(c));Draw();int checks=0;
   for(int frame=0;frame<600;frame++){
    Tick(.02f);
    foreach(var f in Model.Flights){
     if(f.Stage!=1||f.Progress<AdvanceHarvestCommand.LiftSeconds+.25f||f.Progress>AdvanceHarvestCommand.LoadedSeconds-.08f)continue;
     var actor=GameObject.Find("BlenderBeeCarrier_"+f.Id);foreach(var r in actor.GetComponentsInChildren<Renderer>()){
      var b=r.bounds;for(int c=0;c<8;c++){var v=Camera.main.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1)));Assert.Greater(v.x,-.005f,"Second-level loaded carrier clipped left: task "+f.Id+" frame "+frame);Assert.Less(v.x,1.005f,"Second-level loaded carrier clipped right: task "+f.Id+" frame "+frame);Assert.Greater(v.y,-.005f);Assert.Less(v.y,1.005f);}
     }checks++;
    }
   }
   Assert.Greater(checks,100);
  }
  [UnityTest] public IEnumerator CachedLoadedLaneStaysVisibleAfterTheViewportNarrows(){
   yield return Load();BeeSceneSteps.FirstPickup();Tick(AdvanceHarvestCommand.LiftSeconds+.38f);
   var camera=Camera.main;float originalAspect=camera.aspect;
   try{
    camera.aspect=.28f;
    for(int frame=0;frame<15&&Model.Flights[0].Stage==1;frame++){
     Tick(.02f);if(Model.Flights[0].Stage==2)break;
     foreach(var r in Actor.GetComponentsInChildren<Renderer>()){
      var b=r.bounds;for(int c=0;c<8;c++){var v=camera.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1)));Assert.Greater(v.x,-.005f,"Cached lane leaves narrowed viewport");Assert.Less(v.x,1.005f);Assert.Greater(v.y,-.005f);Assert.Less(v.y,1.005f);}
     }
    }
   }finally{camera.aspect=originalAspect;}
  }
  [UnityTest] public IEnumerator LoadedFlightBanksSlightlyAndPauseFreezesItsWholePose(){
   yield return Load();BeeSceneSteps.FirstPickup();Tick(AdvanceHarvestCommand.LiftSeconds+.18f);
   Assert.Greater(Mathf.Abs(Vector3.Dot(Body.right,-Camera.main.transform.forward)),.025f,"Loaded flight has a visible gentle bank");
   var app=BeeHarvestApp.Interface;app.SendCommand(new PauseHarvestCommand(true));Draw();var position=Actor.position;var bodyRotation=Body.rotation;var cargoPosition=Cargo.position;var cargoRotation=Cargo.rotation;
   yield return null;Tick(5);Assert.AreEqual(position,Actor.position);Assert.Less(Quaternion.Angle(bodyRotation,Body.rotation),.01f);Assert.AreEqual(cargoPosition,Cargo.position);Assert.Less(Quaternion.Angle(cargoRotation,Cargo.rotation),.01f);
   Object.FindObjectOfType<LevelOneController>().RestartLevel();Draw();Assert.IsNull(GameObject.Find("BlenderBeeCarrier_0"));
   app.SendCommand(new ActivateHiveCommand(0));Draw();Assert.IsFalse(Cargo.gameObject.activeSelf);Assert.Less(Body.localPosition.magnitude,.001f);
  }
 }
}
