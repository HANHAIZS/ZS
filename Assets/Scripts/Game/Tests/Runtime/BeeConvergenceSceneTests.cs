using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class BeeConvergenceSceneTests:EconomyTestFixture {
  static HarvestModel M=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Object.FindObjectOfType<HarvestBoardView>();
  static Transform Actor(int id)=>GameObject.Find("BlenderBeeCarrier_"+id)?.transform;
  static Transform Body(Transform actor)=>actor.Cast<Transform>().First(t=>t.name!="CarriedVoxel");
  static void Draw(){Object.FindObjectOfType<HiveModelView>().Sync();Board.Sync();Object.FindObjectOfType<BeeFlightView>().RenderFlights();}
  static IEnumerator Load(){SceneManager.LoadScene("Level01");yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;Board.AutoRotate=false;Board.ResetRotation();Board.PublishCameraView();}
  [UnityTest] public IEnumerator ModelIsSmallerAndEmptyDepartureBeeIsLargerThanPickupBee(){
   yield return Load();Assert.AreEqual(.86f,GameObject.Find("Flower(Clone)").transform.localScale.x,.001f,"Give the transport routes more space");
   BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Draw();float near=Body(Actor(0)).lossyScale.x;
   BeeSceneSteps.Until(()=>M.Flights[0].Reached&&M.Flights[0].Progress>=AdvanceHarvestCommand.ApproachSeconds);float far=Body(Actor(0)).lossyScale.x;
   Assert.Greater(near/far,1.4f,"Near departure bees need a clear depth hierarchy");
  }
  [UnityTest] public IEnumerator BeeAndCargoDisappearTogetherAfterOneDelivery(){
   yield return Load();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Draw();BeeSceneSteps.FirstPickup();var f=M.Flights[0];
   BeeSceneSteps.Until(()=>f.Stage==2);Assert.IsNull(Actor(0),"No visible empty return to the small hive");int deposited=M.Deposited;Assert.Greater(deposited,0);BeeSceneSteps.Advance(.01f);Assert.IsNull(Actor(0));
  }
  [UnityTest] public IEnumerator LoadedDepartureUsesALocalEscapeInsteadOfAFixedScreenEdge(){
   yield return Load();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Draw();BeeSceneSteps.FirstPickup();var f=M.Flights[0];
   var pickup=Board.PickupPose(f.Cube).position;var escape=Board.CarryLanePoint(pickup+Board.PickupOutward(f)*.24f,f.Lane);
   Assert.Less(Vector3.Distance(pickup,escape),1.5f,"Local clearance must not send every cargo to a remote screen border");
  }
  [UnityTest] public IEnumerator RenderedLoadedTransitTakesAboutHalfASecondAfterExtraction(){
   yield return Load();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Draw();BeeSceneSteps.FirstPickup();var f=M.Flights[0];
   BeeSceneSteps.Until(()=>f.TransportPhase==2);float age=f.Age;BeeSceneSteps.Until(()=>f.Stage==2);
   Assert.Less(f.Age-age,.95f,"A short loaded route replaces the multi-second border detour");Assert.Greater(f.Age-age,.35f);
  }
  [UnityTest] public IEnumerator RebuildingTheViewDuringEntryRestoresANearbyCarrierAndCleansOldActors(){
   yield return Load();var app=BeeHarvestApp.Interface;app.SendCommand(new ActivateHiveCommand(0));Draw();
   BeeSceneSteps.Until(()=>M.Flights[0].Stage==1&&M.Flights[0].TransportPhase==4&&M.Flights[0].PhaseProgress>=.02f);
   var f=M.Flights[0];var root=Board.gameObject;Object.Destroy(Object.FindObjectOfType<BeeFlightView>());yield return null;
   var view=root.AddComponent<BeeFlightView>();view.Initialize(M,Board,app.GetUtility<LevelOneAssets>());Draw();
   var actors=root.transform.Cast<Transform>().Where(t=>t.name=="BlenderBeeCarrier_0"&&t.gameObject.activeSelf).ToArray();var cargo=actors.Last().Find("CarriedVoxel");
   Assert.Less(Vector3.Distance(cargo.position,Board.EntryWaitingPoint(f.Lane,f.Id)),.6f,"Rebuilt entry must not start at an uninitialized Cargo world origin");
   Assert.AreEqual(1,actors.Length,"Destroyed presentation leaves no visible ghost actors");
   BeeSceneSteps.Until(()=>f.Stage==2);Assert.IsTrue(f.Returned);Assert.IsNull(Actor(0));
  }
 }
}
