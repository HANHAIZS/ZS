using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class LevelSixSourceHandoffTests : EconomyTestFixture {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Object.FindObjectOfType<HarvestBoardView>();
  static LevelOneController Level=>Object.FindObjectOfType<LevelOneController>();
  static IEnumerator Load(int number){LevelSceneTestLoader.Load(LevelOneAssets.LevelKey(number));yield return null;yield return null;Level.enabled=false;Board.AutoRotate=false;Board.ResetRotation();Board.PublishCameraView();yield return null;}
  static FlightState Select(){var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();for(int c=0;c<3;c++){int h=rules.Front(c);if(h>=0&&rules.CanSelectHive(h)&&rules.FindTarget(Model.Hives[h].Data.color)>=0){app.SendCommand(new ActivateHiveCommand(c));Object.FindObjectOfType<HiveModelView>().Sync();Board.Sync();Object.FindObjectOfType<BeeFlightView>().RenderFlights();return Model.Flights[0];}}Assert.Fail("A legal visible hive is required");return null;}
  static Transform Cargo(FlightState f){return GameObject.Find("BlenderBeeCarrier_"+f.Id).transform.Find("CarriedVoxel");}
  static Transform Grip(Transform cargo){return cargo.parent.Cast<Transform>().Single(t=>t!=cargo).Find("GripAnchor");}
  [UnityTest] public IEnumerator FirstFrameUsesRealSourcePoseAndNinetyMillisecondsBlendToTheMovingGrip(){
   yield return Load(6);var f=Select();var source=GameObject.Find(Model.Cubes[f.Cube].Data.name).GetComponent<Renderer>();var from=source.transform.position;var rotation=source.transform.rotation;var scale=source.transform.lossyScale;var sourceMaterial=source.sharedMaterial;
   BeeSceneSteps.FirstPickup();var cargo=Cargo(f);var renderer=cargo.GetComponent<Renderer>();float start=f.Age;
   TestContext.WriteLine("First pickup world-position error="+Vector3.Distance(from,cargo.position));
   Assert.Less(Vector3.Distance(from,cargo.position),.00001f,"Cargo must begin inside the actual source cavity, not on the projected flight plane");Assert.Less(Quaternion.Angle(rotation,cargo.rotation),.001f);Assert.Less(Vector3.Distance(scale,cargo.lossyScale),.00001f);Assert.IsFalse(source.enabled);Assert.AreSame(source.GetComponent<MeshFilter>().sharedMesh,cargo.GetComponent<MeshFilter>().sharedMesh);Assert.AreSame(sourceMaterial,renderer.sharedMaterial,"First-frame cavity lighting must match the replaced block");
   var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);Assert.AreEqual(1,block.GetFloat("_CavityStrength"),.00001f);
   foreach(float dt in new[]{.03f,.03f,.031f}){BeeSceneSteps.Advance(dt);float t=Mathf.Clamp01((f.Age-start)/.09f);float e=Mathf.SmoothStep(0,1,t);Assert.Less(Vector3.Distance(Vector3.Lerp(from,Grip(cargo).position,e),cargo.position),.0001f,"Continuous source-to-current-grip handoff");Assert.Less(Vector3.Distance(scale,cargo.lossyScale),.00001f);if(t<1){Assert.AreSame(sourceMaterial,renderer.sharedMaterial);renderer.GetPropertyBlock(block);Assert.AreEqual(1-e,block.GetFloat("_CavityStrength"),.0001f);}}
   Assert.AreEqual("Universal Render Pipeline/Lit",renderer.sharedMaterial.shader.name);Assert.IsFalse(renderer.HasPropertyBlock(),"Temporary source-cavity overrides must be cleared");Assert.Less(Vector3.Distance(Grip(cargo).position,cargo.position),.00001f);Assert.AreEqual(0,f.TransportPhase,"The 90ms handoff stays inside the existing 220ms lift");BeeSceneSteps.Until(()=>Model.Deposited>0,40);Assert.AreEqual(870,Model.Remaining+Model.InTransit+Model.Deposited);
  }
  [UnityTest] public IEnumerator FrozenSourceSamplingPauseRotationRetryAndPoolReuseStayContinuous(){
   yield return Load(6);var f=Select();var source=GameObject.Find(Model.Cubes[f.Cube].Data.name).transform;var from=source.position;BeeSceneSteps.FirstPickup();var cargo=Cargo(f);float start=f.Age;var block=new MaterialPropertyBlock();var renderer=cargo.GetComponent<Renderer>();
   BeeSceneSteps.Advance(.03f);renderer.GetPropertyBlock(block);var logical=block.GetMatrix("_CavityWorldToGrid").MultiplyPoint3x4(cargo.position);var data=Model.Cubes[f.Cube].Data;Assert.Less(Vector3.Distance(logical,new Vector3(data.x,data.y,data.z)),.0002f,"The source face sample remains attached to the moving replacement");
   var old=cargo.position;var rot=cargo.rotation;var matrix=block.GetMatrix("_CavityWorldToGrid");float age=f.Age;GameObject.Find("Chick(Clone)").transform.Rotate(0,70,0);Board.PublishCameraView();BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(true));yield return new WaitForSeconds(.15f);BeeSceneSteps.Advance(2);
   Assert.AreEqual(age,f.Age);Assert.AreEqual(old,cargo.position);Assert.Less(Quaternion.Angle(rot,cargo.rotation),.001f);renderer.GetPropertyBlock(block);Assert.AreEqual(matrix,block.GetMatrix("_CavityWorldToGrid"));
   BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(false));BeeSceneSteps.Advance(.03f);float t=Mathf.Clamp01((f.Age-start)/.09f);Assert.Less(Vector3.Distance(Vector3.Lerp(from,Grip(cargo).position,Mathf.SmoothStep(0,1,t)),cargo.position),.0001f,"Manual model rotation must not drag the detached block back to the moving source");renderer.GetPropertyBlock(block);Assert.Less(Vector3.Distance(logical,block.GetMatrix("_CavityWorldToGrid").MultiplyPoint3x4(cargo.position)),.0002f);
   var poolIds=Object.FindObjectsOfType<Transform>().Where(v=>v.name.StartsWith("BlenderBeeCarrier_")).Select(v=>v.GetInstanceID()).ToArray();Level.RestartLevel();Board.Sync();Object.FindObjectOfType<BeeFlightView>().RenderFlights();Assert.IsNull(GameObject.Find("BlenderBeeCarrier_0"));Assert.AreEqual(870,Model.Remaining);Assert.IsTrue(GameObject.Find("Chick(Clone)").GetComponentsInChildren<Renderer>().All(r=>r.enabled));
   var retry=Select();var freshSource=GameObject.Find(Model.Cubes[retry.Cube].Data.name).transform;var freshPosition=freshSource.position;BeeSceneSteps.FirstPickup();var reused=Cargo(retry);Assert.Contains(reused.parent.GetInstanceID(),poolIds);Assert.Less(Vector3.Distance(freshPosition,reused.position),.00001f);var reusedBlock=new MaterialPropertyBlock();reused.GetComponent<Renderer>().GetPropertyBlock(reusedBlock);Assert.AreEqual(1,reusedBlock.GetFloat("_CavityStrength"),.00001f,"Reused actors start a new fade, never inherit the previous half-finished one");BeeSceneSteps.Advance(.1f);Assert.AreEqual("Universal Render Pipeline/Lit",reused.GetComponent<Renderer>().sharedMaterial.shader.name);Assert.IsFalse(reused.GetComponent<Renderer>().HasPropertyBlock());
  }
  [UnityTest] public IEnumerator RebuildingDuringCarryAndSkippingLiftDoNotReplayTheCavityHandoff(){
   yield return Load(6);var f=Select();BeeSceneSteps.FirstPickup();var app=BeeHarvestApp.Interface;app.SendCommand(new AdvanceHarvestCommand(.25f));Board.Sync();Object.FindObjectOfType<BeeFlightView>().RenderFlights();Assert.GreaterOrEqual(f.TransportPhase,2);Assert.AreEqual("Universal Render Pipeline/Lit",Cargo(f).GetComponent<Renderer>().sharedMaterial.shader.name);Assert.IsFalse(Cargo(f).GetComponent<Renderer>().HasPropertyBlock());
   Object.Destroy(Object.FindObjectOfType<BeeFlightView>());yield return null;var view=Board.gameObject.AddComponent<BeeFlightView>();view.Initialize(Model,Board,app.GetUtility<LevelOneAssets>(),true);view.RenderFlights();var cargo=Cargo(f);Assert.Less(Vector3.Distance(cargo.position,Grip(cargo).position),.00001f);Assert.AreEqual("Universal Render Pipeline/Lit",cargo.GetComponent<Renderer>().sharedMaterial.shader.name);Assert.Greater(Vector3.Distance(GameObject.Find(Model.Cubes[f.Cube].Data.name).transform.position,cargo.position),.1f);BeeSceneSteps.Until(()=>Model.Deposited>0,40);
  }
  [UnityTest] public IEnumerator ClawsFollowTheBlockOnScreenWithoutStretchingDuringTheNinetyMillisecondHandoff(){
   yield return Load(6);var f=Select();BeeSceneSteps.FirstPickup();var cargo=Cargo(f);var body=Grip(cargo).parent;var mesh=cargo.GetComponent<MeshFilter>().sharedMesh.bounds;var half=Vector3.Scale(mesh.extents,cargo.lossyScale);var forward=Camera.main.transform.forward;
   for(int i=0;i<10;i++){
    var projected=cargo.position+forward*Vector3.Dot(Grip(cargo).position-cargo.position,forward);float gap=0;
    foreach(var limb in body.GetComponentsInChildren<Transform>().Where(v=>v.name.Contains("_Limb_"))){var knee=limb.Find("GripKnee");var tip=limb.GetComponentsInChildren<Transform>().First(v=>v.name.Contains("_GripTip_"));Assert.AreEqual(body.lossyScale.x*.525f,Vector3.Distance(limb.position,knee.position),.001f);Assert.AreEqual(body.lossyScale.x*.525f,Vector3.Distance(knee.position,tip.position),.001f);var point=Quaternion.Inverse(cargo.rotation)*(tip.position-projected);float distance=Mathf.Min(Mathf.Abs(Mathf.Abs(point.x)-half.x),Mathf.Abs(Mathf.Abs(point.y)-half.y),Mathf.Abs(Mathf.Abs(point.z)-half.z));gap=Mathf.Max(gap,distance);}
    Assert.Less(gap,.008f,"Fixed-length feet track the visible block surface throughout handoff");if(i<9)BeeSceneSteps.Advance(.01f);
   }
  }
  [UnityTest] public IEnumerator UnselectedScenesKeepTheirExistingGripAttachment(){
   foreach(int n in new[]{7,8}){yield return Load(n);var f=Select();var source=GameObject.Find(Model.Cubes[f.Cube].Data.name).transform;BeeSceneSteps.FirstPickup();var cargo=Cargo(f);Assert.Less(Vector3.Distance(cargo.position,Grip(cargo).position),.00001f);Assert.Greater(Vector3.Distance(source.position,cargo.position),.1f,"Levels outside the requested range retain direct grip attachment");Assert.AreEqual(LevelOneAssets.HasVoxelEdges(n)?"BeeHarvest/VoxelEdges":"Universal Render Pipeline/Lit",cargo.GetComponent<Renderer>().sharedMaterial.shader.name);}
  }
  [UnityTest] public IEnumerator GpuFirstFrameKeepsTheSourceAppearanceInsteadOfOpeningAnEmptyHole(){
   yield return Load(6);var root=GameObject.Find("Chick(Clone)");foreach(var r in root.GetComponentsInChildren<Renderer>())r.gameObject.layer=30;var f=Select();
   // Isolate this handoff: dispersed targets may let other bees remove unrelated voxels before task zero.
   // Preserve total color quotas while dispatching a single bee from the selected hive.
   var data=JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("Levels/Level06").text);var selected=data.hives[f.Hive];int remainder=selected.quota-1;selected.quota=1;
   foreach(var hive in data.hives)hive.connected=new int[0];
   if(remainder>0)data.hives=data.hives.Concat(new[]{new HiveData{column=selected.column,color=selected.color,quota=remainder}}).ToArray();
   BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(data));Board.PublishCameraView();f=Select();
   var cameraObject=new GameObject("SourceHandoffGpuCamera");var camera=cameraObject.AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;camera.cullingMask=1<<30;camera.transform.SetPositionAndRotation(Camera.main.transform.position,Camera.main.transform.rotation);
   var target=new RenderTexture(384,684,24);var image=new Texture2D(384,684,TextureFormat.RGB24,false);var prior=RenderTexture.active;camera.targetTexture=target;
   try{
    camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,384,684),0,0);image.Apply();var before=image.GetPixels();BeeSceneSteps.FirstPickup();Assert.AreEqual(1,Model.Flights.Count,"Only the compared source changes in this GPU fixture");Cargo(f).gameObject.layer=30;camera.Render();image.ReadPixels(new Rect(0,0,384,684),0,0);image.Apply();var after=image.GetPixels();float difference=0;int changed=0;
    for(int i=0;i<before.Length;i++){float delta=Vector3.Distance(new Vector3(before[i].r,before[i].g,before[i].b),new Vector3(after[i].r,after[i].g,after[i].b));difference+=delta;if(delta>.08f)changed++;}
    TestContext.WriteLine("First-frame GPU mean difference="+(difference/before.Length)+", changed pixels="+changed);Assert.Less(difference/before.Length,.0003f);Assert.Less(changed,12,"Replacing a source block at the same depth must not reveal a blank cavity or change its first-frame shading");
   }finally{RenderTexture.active=prior;camera.targetTexture=null;target.Release();Object.Destroy(target);Object.Destroy(image);Object.Destroy(cameraObject);}
  }
 }
}
