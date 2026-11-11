using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class VoxelSurfaceTextureGameplayTests:EconomyTestFixture {
  static LevelOneController Level=>Object.FindObjectOfType<LevelOneController>();
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Level.GetComponent<HarvestBoardView>();
  static IEnumerator Load(int n){LevelSceneTestLoader.Load(LevelOneAssets.LevelKey(n));yield return null;yield return null;Level.enabled=false;Board.AutoRotate=false;Board.ResetRotation();Board.PublishCameraView();}
  static FlightState Select(Transform root){var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();for(int turn=0;turn<8;turn++){Board.PublishCameraView();for(int c=0;c<3;c++){int h=rules.Front(c);if(h>=0&&rules.CanSelectHive(h)&&rules.FindTarget(Model.Hives[h].Data.color)>=0){app.SendCommand(new ActivateHiveCommand(c));Level.GetComponent<HiveModelView>().Sync();Board.Sync();Level.GetComponent<BeeFlightView>().RenderFlights();return Model.Flights[0];}}root.Rotate(0,45,0);}Assert.Fail("A visible source block is required");return null;}
  [UnityTest] public IEnumerator TexturedBlocksKeepTheirPaletteMapsDuringPickupDeliveryAndRetry(){
   foreach(int n in Enumerable.Range(9,4)){
    yield return Load(n);var root=GameObject.Find(LevelOneAssets.ModelKey(n)+"(Clone)").transform;var rs=root.GetComponentsInChildren<Renderer>();var palettes=rs.Select(r=>r.sharedMaterial).Distinct().ToArray();Assert.AreEqual(Model.Cubes.Select(c=>c.Data.color).Distinct().Count(),palettes.Length);
    foreach(var r in rs){Assert.AreEqual("Tex_Base_Voxel",r.sharedMaterial.GetTexture("_BaseMap").name);Assert.AreEqual("Tex_Normal_Voxel",r.sharedMaterial.GetTexture("_BumpMap").name);Assert.IsTrue(r.sharedMaterial.IsKeywordEnabled("_NORMALMAP"));}
    var flight=Select(root);var source=GameObject.Find(Model.Cubes[flight.Cube].Data.name).GetComponent<Renderer>();var material=source.sharedMaterial;BeeSceneSteps.FirstPickup();var cargo=GameObject.Find("BlenderBeeCarrier_"+flight.Id).transform.Find("CarriedVoxel").GetComponent<Renderer>();Assert.IsFalse(source.enabled);Assert.AreSame(material.GetTexture("_BaseMap"),cargo.sharedMaterial.GetTexture("_BaseMap"));Assert.AreSame(material.GetTexture("_BumpMap"),cargo.sharedMaterial.GetTexture("_BumpMap"));Assert.AreEqual("Universal Render Pipeline/Lit",cargo.sharedMaterial.shader.name);Assert.AreSame(source.GetComponent<MeshFilter>().sharedMesh,cargo.GetComponent<MeshFilter>().sharedMesh);Assert.Less(Vector4.Distance(LevelOneAssets.ColorOf(Model.Cubes[flight.Cube].Data.color),cargo.sharedMaterial.GetColor("_BaseColor")),.00001f);
    BeeSceneSteps.Until(()=>Model.Deposited>0);Assert.Greater(Model.Deposited,0,"Textured cargo is delivered normally");
    Level.RestartLevel();Level.Hud.Sync();Level.GetComponent<HiveModelView>().Sync();Board.Sync();Level.GetComponent<BeeFlightView>().RenderFlights();yield return null;CollectionAssert.AreEquivalent(palettes,rs.Select(r=>r.sharedMaterial).Distinct());Assert.IsTrue(rs.All(r=>r.enabled));Assert.AreEqual(0,Model.Deposited);
   }
   foreach(int n in new[]{8,13}){yield return Load(n);Assert.IsTrue(GameObject.Find(LevelOneAssets.ModelKey(n)+"(Clone)").GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.GetTexture("_BaseMap")==null&&r.sharedMaterial.GetTexture("_BumpMap")==null));}
  }
  [UnityTest] public IEnumerator RealGpuRenderShowsBothSurfaceAndNormalDetail(){
   yield return Load(9);var prefab=Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.ModelKey(9));var original=prefab.GetComponentsInChildren<Renderer>().OrderByDescending(r=>r.sharedMaterial.GetColor("_BaseColor").grayscale).First();var material=new Material(original.sharedMaterial);var cube=new GameObject("SurfaceRenderProbe");var cameraObject=new GameObject("SurfaceRenderCamera");var target=new RenderTexture(128,128,24);var pixels=new Texture2D(128,128,TextureFormat.RGB24,false);var previous=RenderTexture.active;
   try{
    cube.layer=30;cube.transform.position=new Vector3(1000,1000,1000);cube.AddComponent<MeshFilter>().sharedMesh=original.GetComponent<MeshFilter>().sharedMesh;cube.AddComponent<MeshRenderer>().sharedMaterial=material;
    var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=.64f;camera.nearClipPlane=.1f;camera.farClipPlane=10;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.blue;camera.transform.position=cube.transform.position+Vector3.back*3;camera.targetTexture=target;
    camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,128,128),0,0);pixels.Apply();var textured=pixels.GetPixels();material.SetFloat("_BumpScale",0);camera.Render();pixels.ReadPixels(new Rect(0,0,128,128),0,0);pixels.Apply();var noNormal=pixels.GetPixels();material.SetTexture("_BaseMap",null);camera.Render();pixels.ReadPixels(new Rect(0,0,128,128),0,0);pixels.Apply();var plain=pixels.GetPixels();
    int surfaceChanged=0,normalChanged=0;float surfaceMax=0,normalMax=0;for(int i=0;i<plain.Length;i++){float s=Vector4.Distance(noNormal[i],plain[i]);float b=Vector4.Distance(textured[i],noNormal[i]);surfaceMax=Mathf.Max(surfaceMax,s);normalMax=Mathf.Max(normalMax,b);if(s>.01f)surfaceChanged++;if(b>.001f)normalChanged++;}
    TestContext.WriteLine("GPU surface changed="+surfaceChanged+" max="+surfaceMax+"; normal changed="+normalChanged+" max="+normalMax);Assert.Greater(surfaceChanged,100);Assert.Greater(surfaceMax,.02f);Assert.Greater(normalChanged,10);Assert.Greater(normalMax,.005f);
   }finally{RenderTexture.active=previous;Object.Destroy(cube);Object.Destroy(cameraObject);Object.Destroy(material);Object.Destroy(pixels);target.Release();Object.Destroy(target);}
  }
 }
}
