using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class BeeGraspAnimationTests : EconomyTestFixture {
  [UnityTest] public IEnumerator BellyGripFacesVoxelAndFourFeetReachBeforeExtraction(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;
   Object.FindObjectOfType<LevelOneController>().enabled=false;
   var app=BeeHarvestApp.Interface;app.SendCommand(new ActivateHiveCommand(0));
   var hive=Object.FindObjectOfType<HiveModelView>();var board=Object.FindObjectOfType<HarvestBoardView>();var bees=Object.FindObjectOfType<BeeFlightView>();
   hive.Sync();board.Sync();bees.RenderFlights();
   BeeSceneSteps.Until(()=>app.GetModel<HarvestModel>().Flights[0].Reached&&app.GetModel<HarvestModel>().Flights[0].Progress>=AdvanceHarvestCommand.ApproachSeconds);BeeSceneSteps.Advance(AdvanceHarvestCommand.SettleSeconds+.02f);hive.Sync();board.Sync();bees.RenderFlights();
   var model=app.GetModel<HarvestModel>();var actor=GameObject.Find("BlenderBeeCarrier_0").transform;
   var body=actor.Cast<Transform>().First(t=>t.name!="CarriedVoxel");
   var target=board.PickupPose(model.Flights[0].Cube).position;
   Assert.Greater(Vector3.Dot(body.TransformDirection(body.Find("GripAnchor").localPosition.normalized),(target-body.position).normalized),.98f,"Belly grip faces the source without stretching the legs");
   var start=body.localPosition;
   BeeSceneSteps.Advance(.025f);hive.Sync();board.Sync();bees.RenderFlights();
   Assert.Greater(Vector3.Distance(start,body.localPosition),.003f,"The body settles into its grasp pose");
   BeeSceneSteps.Advance(AdvanceHarvestCommand.ReachSeconds-.045f-.005f);hive.Sync();board.Sync();bees.RenderFlights();
   Assert.AreEqual(0,model.Flights[0].Stage,"Four limbs acquire the source before removing it");
   var tips=body.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("_GripTip_")).ToArray();Assert.AreEqual(4,tips.Length);
   foreach(var tip in tips)Assert.Less(Vector3.Distance(tip.position,target),.22f,"Every limb reaches a corner of the source voxel");
   foreach(var tip in tips)Assert.Greater(Vector3.Dot(tip.position-target,body.TransformDirection(Vector3.right))*(tip.name.Contains("Left")?1:-1),.03f,"Left and right grips must stay on their own sides, without crossed legs");
   BeeSceneSteps.Advance(.006f);hive.Sync();board.Sync();bees.RenderFlights();
   var cargo=actor.Find("CarriedVoxel").GetComponent<Renderer>();Assert.IsTrue(cargo.gameObject.activeSelf);
   var before=Camera.main.WorldToScreenPoint(cargo.bounds.center);
   BeeSceneSteps.Advance(.02f);hive.Sync();board.Sync();bees.RenderFlights();
   Assert.Less(Vector2.Distance(before,Camera.main.WorldToScreenPoint(cargo.bounds.center)),12f,"Grasp-to-lift keeps the same visible cube");
   BeeSceneSteps.Advance(AdvanceHarvestCommand.LiftSeconds);hive.Sync();board.Sync();bees.RenderFlights();
   var b=cargo.bounds;b.Expand(.055f);foreach(var tip in tips)Assert.IsTrue(b.Contains(tip.position),"Four feet stay attached while carrying");
  }
  [UnityTest] public IEnumerator ApproachTurnsSmoothlyBeforeLock(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;
   Object.FindObjectOfType<LevelOneController>().enabled=false;var app=BeeHarvestApp.Interface;
   app.SendCommand(new ActivateHiveCommand(0));var hive=Object.FindObjectOfType<HiveModelView>();var board=Object.FindObjectOfType<HarvestBoardView>();var bees=Object.FindObjectOfType<BeeFlightView>();
   hive.Sync();board.Sync();bees.RenderFlights();var actor=GameObject.Find("BlenderBeeCarrier_0").transform;var body=actor.Cast<Transform>().First(t=>t.name!="CarriedVoxel");var previous=body.rotation;
   for(int i=0;i<34;i++){BeeSceneSteps.Advance(.02f);hive.Sync();board.Sync();bees.RenderFlights();Assert.Less(Quaternion.Angle(previous,body.rotation),30f,"Turning should blend at every approach/lock frame, frame "+i);previous=body.rotation;}
  }
 }
}
