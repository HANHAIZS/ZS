using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class FixedGripTransportTests:EconomyTestFixture {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Object.FindObjectOfType<HarvestBoardView>();
  static void Draw(){Object.FindObjectOfType<HiveModelView>().Sync();Board.Sync();Object.FindObjectOfType<BeeFlightView>().RenderFlights();}
  static void Tick(float dt){Board.PublishCameraView();BeeSceneSteps.Advance(dt);Draw();}
  static IEnumerator Load(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;
   var data=JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("Levels/Level01").text);
   data.hives[12].quota+=data.hives[0].quota-1;data.hives[0].quota=1;
   BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(data));Board.ResetRotation();Board.PublishCameraView();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Draw();
  }
  // Catches root-relative cargo, pickup pop, and entry shrink at actual rendered poses.
  [UnityTest] public IEnumerator CargoKeepsSourceSizeAndBellyAttachmentThroughTurnsAndEntry(){
   yield return Load();var f=Model.Flights[0];var source=GameObject.Find(Model.Cubes[f.Cube].Data.name).GetComponent<Renderer>();var scale=source.transform.lossyScale;
   BeeSceneSteps.FirstPickup();var actor=GameObject.Find("BlenderBeeCarrier_0").transform;var cargo=actor.Find("CarriedVoxel");var body=actor.Cast<Transform>().First(t=>t!=cargo);
   var grip=body.Find("GripAnchor");Assert.IsNotNull(grip,"Cargo needs a body-owned belly anchor");var local=grip.localPosition;int checks=0;
   while(f.Stage==1){
    Assert.Less(Vector3.Distance(cargo.lossyScale,scale),.001f,"Cargo size is constant from handoff through entry");
    Assert.Less(Vector3.Distance(cargo.position,grip.position),.001f,"Turning must carry the block with the belly");
    Assert.AreEqual(local,grip.localPosition);Assert.AreSame(source.sharedMaterial,cargo.GetComponent<Renderer>().sharedMaterial);
    foreach(var limb in body.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("_Limb_"))){var tip=limb.GetComponentsInChildren<Transform>().First(t=>t.name.Contains("_GripTip_"));Assert.Less(Vector3.Distance(limb.position,tip.position),.19f,"No rubber-band legs: "+limb.name);var knee=limb.Find("GripKnee");Assert.AreEqual(body.lossyScale.x*.525f,Vector3.Distance(limb.position,knee.position),.001f);Assert.AreEqual(body.lossyScale.x*.525f,Vector3.Distance(knee.position,tip.position),.001f);}
    Tick(.02f);checks++;
   }
   Assert.Greater(checks,40);Assert.AreEqual(1,Model.Deposited);Tick(2);Assert.AreEqual(1,Model.Deposited);
  }
  // A distant source must never stretch the rest rig to satisfy an unreachable contact.
  [UnityTest] public IEnumerator UnreachableContactDoesNotLengthenTheLegs(){
   var root=Object.Instantiate(Resources.Load<GameObject>("Prefabs/Gameplay/BeePalette17")).transform;root.localScale=Vector3.one*.16f;var grasp=new BeeGraspPose(root);
   try{grasp.Grasp(new Pose(new Vector3(5,5,5),Quaternion.identity),Vector3.one*.11f,1);
    foreach(var limb in root.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("_Limb_"))){var tip=limb.GetComponentsInChildren<Transform>().First(t=>t.name.Contains("_GripTip_"));Assert.Less(Vector3.Distance(limb.position,tip.position),.19f);}
   }finally{grasp.Dispose();Object.Destroy(root.gameObject);}yield return null;
  }
  // Fixed debug view still accepts explicit manual rotation and keeps uncollected cells in place.
  [UnityTest] public IEnumerator ExplicitFixedViewStaysStillAndManualPoseDoesNotRecenterAfterPickup(){
   yield return Load();Board.AutoRotate=false;var flower=GameObject.Find("Flower(Clone)").transform;var before=flower.rotation;Board.TickAutoRotation(1);Assert.Less(Quaternion.Angle(before,flower.rotation),.001f);
   flower.rotation=Quaternion.Euler(0,35,0);Board.PublishCameraView();var untouched=Model.Cubes.First(c=>c.Stage==0);var t=GameObject.Find(untouched.Data.name).transform;var position=t.localPosition;
   BeeSceneSteps.FirstPickup();Assert.AreEqual(position,t.localPosition);Assert.AreEqual(35,flower.eulerAngles.y,.01f);Assert.IsFalse(GameObject.Find(Model.Cubes[Model.Flights[0].Cube].Data.name).GetComponent<Renderer>().enabled);
  }
  [UnityTest] public IEnumerator SourceExtractionFollowsTheExposedFaceAfterManualRotation(){
   yield return Load();var f=Model.Flights[0];GameObject.Find("Flower(Clone)").transform.rotation=Quaternion.Euler(0,35,0);Board.PublishCameraView();
   BeeSceneSteps.FirstPickup();var cargo=GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel");var start=cargo.position;var outward=Board.PickupOutward(f);
   Tick(.11f);var delta=cargo.position-start;Assert.Greater(delta.magnitude,.10f);Assert.Greater(Vector3.Dot(delta.normalized,outward),.995f,"Pull along the selected voxel face, rather than through adjacent cells");
   Object.FindObjectOfType<LevelOneController>().RestartLevel();Draw();Assert.IsEmpty(Model.Flights);Assert.IsTrue(Model.Cubes.All(c=>c.Stage==0&&c.Task==-1));
  }
  [UnityTest] public IEnumerator OneDepositPlaysOneCollectorPulseAndPauseFreezesIt(){
   yield return Load();var collector=GameObject.Find("Collector(Clone)").transform;var rest=collector.localScale;
   BeeSceneSteps.Until(()=>Model.Deposited==1);
   Assert.AreEqual(1,Model.Deposited);Assert.Greater(collector.localScale.x,rest.x,"The delivered block needs visible entrance feedback");
   BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(true));var held=collector.localScale;yield return new WaitForSeconds(.1f);Assert.AreEqual(held,collector.localScale);
   BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(false));yield return new WaitForSeconds(.3f);Assert.Less(Vector3.Distance(rest,collector.localScale),.001f);
   Tick(5);Assert.AreEqual(1,Model.Deposited);Assert.Less(Vector3.Distance(rest,collector.localScale),.001f,"Completed task cannot replay feedback");
  }
  [UnityTest] public IEnumerator RetryClearsPendingCollectorFeedback(){
   yield return Load();var collector=GameObject.Find("Collector(Clone)").transform;var rest=collector.localScale;
   BeeSceneSteps.Until(()=>Model.Deposited==1);Assert.Greater(collector.localScale.x,rest.x);
   Object.FindObjectOfType<LevelOneController>().RestartLevel();Draw();Assert.AreEqual(rest,collector.localScale,"Retry starts a clean presentation generation");
  }
  [UnityTest] public IEnumerator CargoEntersCollectorContinuouslyWithoutTheOldNinetyTwoPercentSnap(){
   yield return Load();BeeSceneSteps.Until(()=>Model.Flights[0].Stage==1&&Model.Flights[0].TransportPhase==4&&Model.Flights[0].PhaseProgress>=AdvanceHarvestCommand.EntrySeconds*.8f);var cargo=GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel");var p=cargo.position;
   Tick(.0002f);Assert.Less(Vector3.Distance(p,cargo.position),.015f,"The last approach must not snap the last tenth of its outside lane");
   BeeSceneSteps.Until(()=>Model.Deposited==1);Assert.AreEqual(1,Model.Deposited);
  }
  [UnityTest] public IEnumerator BentFeetContactTheCargoSurfaceInsteadOfStoppingInsideTheBlock(){
   yield return Load();BeeSceneSteps.FirstPickup();Tick(AdvanceHarvestCommand.LiftSeconds+.2f);var actor=GameObject.Find("BlenderBeeCarrier_0").transform;var cargo=actor.Find("CarriedVoxel");var mesh=cargo.GetComponent<MeshFilter>().sharedMesh.bounds;
   foreach(var tip in actor.GetComponentsInChildren<Transform>().Where(t=>t.name.Contains("_GripTip_"))){var p=cargo.InverseTransformPoint(tip.position)-mesh.center;var half=mesh.extents;var scale=cargo.lossyScale;float gap=Mathf.Min(Mathf.Abs(Mathf.Abs(p.x)-half.x)*scale.x,Mathf.Abs(Mathf.Abs(p.y)-half.y)*scale.y,Mathf.Abs(Mathf.Abs(p.z)-half.z)*scale.z);Assert.Less(gap,.006f,"Feet meet the block surface, not an arbitrary capped point");var bounds=mesh;bounds.Expand(.05f);Assert.IsTrue(bounds.Contains(cargo.InverseTransformPoint(tip.position)));}
  }
 }
}
