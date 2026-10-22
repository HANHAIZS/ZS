using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace BeeHarvest.Tests {
 public sealed class LevelSevenCavityVisualTests : EconomyTestFixture {
  static IEnumerator Load(int number){LevelSceneTestLoader.Load(LevelOneAssets.LevelKey(number));yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;}
  sealed class Fixture : IDisposable {
   public GameObject Root;public List<CubeState> Cells=new List<CubeState>();public List<Renderer> Renderers=new List<Renderer>();
   public object Visual;public Type Type;public Material Original;
   public Fixture(int radius){
    Type=typeof(HarvestModel).Assembly.GetType("BeeHarvest.VoxelCavityVisuals");Assert.IsNotNull(Type,"Local cavity presentation is missing");
    Root=new GameObject("CavityGridFixture");Root.transform.position=new Vector3(1000,1000,1000);
    var mesh=Resources.Load<GameObject>("Prefabs/Gameplay/Sheep").GetComponentInChildren<MeshFilter>().sharedMesh;
    Original=new Material(Shader.Find("Universal Render Pipeline/Lit"));Original.SetColor("_BaseColor",new Color(.85f,.85f,.85f));Original.SetFloat("_Smoothness",0);
    for(int x=-radius;x<=radius;x++)for(int y=-radius;y<=radius;y++)for(int z=-radius;z<=radius;z++){
     int id=Cells.Count;Cells.Add(new CubeState{Data=new CubeData{id=id,x=x,y=y,z=z,color="white"}});
     var go=new GameObject("FixtureVoxel_"+id);go.transform.SetParent(Root.transform,false);go.transform.localPosition=new Vector3(x,y,z);go.layer=30;
     go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=Original;Renderers.Add(r);
    }
    Visual=Activator.CreateInstance(Type,new object[]{Cells,Renderers.ToArray()});Tick(0);
   }
   public int Id(int x,int y,int z){return Cells.FindIndex(c=>c.Data.x==x&&c.Data.y==y&&c.Data.z==z);}
   public void Remove(int id){Cells[id].Stage=2;Renderers[id].enabled=false;Type.GetMethod("Remove").Invoke(Visual,new object[]{id});}
   public void Tick(float dt){Type.GetMethod("Tick").Invoke(Visual,new object[]{dt,Root.transform.localToWorldMatrix});}
   public Texture2D Texture{get{return (Texture2D)Renderers[0].sharedMaterial.GetTexture("_CavityData");}}
   public void Dispose(){if(Visual!=null)((IDisposable)Visual).Dispose();Object.Destroy(Root);Object.Destroy(Original);}
  }
  static byte[] Pixels(Texture2D t){return t.GetRawTextureData().ToArray();}
  [UnityTest] public IEnumerator SeventhLevelPreservesGeometryAndPaletteAndOtherLevelsKeepTheirShaders(){
   yield return Load(7);var root=GameObject.Find("Sheep(Clone)");var rs=root.GetComponentsInChildren<Renderer>();Assert.AreEqual(1030,rs.Length);
   var original=Resources.Load<GameObject>("Prefabs/Gameplay/Sheep").GetComponentsInChildren<Renderer>().ToDictionary(r=>r.name);
   foreach(var r in rs){Assert.AreEqual("BeeHarvest/VoxelCavity",r.sharedMaterial.shader.name);Assert.IsFalse(r.HasPropertyBlock());Assert.AreSame(original[r.name].GetComponent<MeshFilter>().sharedMesh,r.GetComponent<MeshFilter>().sharedMesh);Assert.Less(Vector4.Distance(original[r.name].sharedMaterial.GetColor("_BaseColor"),r.sharedMaterial.GetColor("_BaseColor")),.00001f);}
   var palettes=rs.Select(r=>r.sharedMaterial).Distinct().ToArray();Assert.AreEqual(4,palettes.Length);
   var cache=palettes[0].GetTexture("_CavityData");Assert.IsNotNull(cache);Assert.IsTrue(palettes.All(p=>p.GetTexture("_CavityData")==cache));
   yield return Load(8);Assert.IsTrue(palettes.All(p=>p==null));Assert.IsTrue(cache==null);Assert.IsTrue(GameObject.Find("Shark(Clone)").GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.shader.name=="Universal Render Pipeline/Lit"));
   yield return Load(6);Assert.IsTrue(GameObject.Find("Chick(Clone)").GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.shader.name=="BeeHarvest/VoxelCavity"));
  }
  [UnityTest] public IEnumerator GpuShowsRecessedCenterAndSoftContactEdgesWithoutDarkeningOuterFaces(){
   yield return Load(7);
   using(var fixture=new Fixture(1)){
    fixture.Remove(fixture.Id(0,0,-1));fixture.Tick(.2f);
    var camObject=new GameObject("CavityTestCamera");var camera=camObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=1.8f;camera.nearClipPlane=.1f;camera.farClipPlane=20;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.magenta;
    camera.transform.position=fixture.Root.transform.position+Vector3.back*6;
    var target=new RenderTexture(192,192,24);var image=new Texture2D(192,192,TextureFormat.RGB24,false);var previous=RenderTexture.active;camera.targetTexture=target;
    try{
     var material=fixture.Renderers[0].sharedMaterial;material.SetFloat("_CavityStrength",0);camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,192,192),0,0);image.Apply();float center=image.GetPixel(96,96).grayscale,outer=image.GetPixel(43,96).grayscale;
     material.SetFloat("_CavityStrength",1);camera.Render();image.ReadPixels(new Rect(0,0,192,192),0,0);image.Apply();float shaded=image.GetPixel(96,96).grayscale,rim=image.GetPixel(118,96).grayscale;
     Assert.Greater(center-shaded,.035f,"A real same-color recess must become visibly darker");Assert.Less(rim,shaded-.015f,"Contact shadow should be stronger near the rim than at face center");Assert.Less(Mathf.Abs(outer-image.GetPixel(43,96).grayscale),.02f,"Unchanged outer faces must not receive whole-object darkening");
     fixture.Root.transform.rotation=Quaternion.Euler(0,35,0);fixture.Tick(0);camera.transform.position=fixture.Root.transform.TransformPoint(Vector3.back*6);camera.transform.rotation=fixture.Root.transform.rotation;
     material.SetFloat("_CavityStrength",0);camera.Render();image.ReadPixels(new Rect(0,0,192,192),0,0);image.Apply();float rotatedBase=image.GetPixel(96,96).grayscale;
     material.SetFloat("_CavityStrength",1);camera.Render();image.ReadPixels(new Rect(0,0,192,192),0,0);image.Apply();float rotatedShaded=image.GetPixel(96,96).grayscale;
     TestContext.WriteLine("Same-pose cavity ratios: original="+(shaded/center)+", rotated="+(rotatedShaded/rotatedBase)+"; lighting baseline="+center+" -> "+rotatedBase);
     Assert.Less(Mathf.Abs(shaded/center-rotatedShaded/rotatedBase),.025f,"Cavity attenuation must remain attached to rotated geometry, independently of directional lighting");
     for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++){int id=fixture.Id(x,y,-1);if(fixture.Cells[id].Stage<2)fixture.Remove(id);}fixture.Tick(.2f);camera.Render();image.ReadPixels(new Rect(0,0,192,192),0,0);image.Apply();Assert.Greater(image.GetPixel(96,96).grayscale,rotatedShaded+.025f,"Opening the surrounding shell must release its occlusion");
    }finally{RenderTexture.active=previous;camera.targetTexture=null;target.Release();Object.Destroy(target);Object.Destroy(image);Object.Destroy(camObject);}
   }
  }
  [UnityTest] public IEnumerator UpdatesStayLocalAndZeroTimeFreezesTheTransition(){
   yield return Load(7);
   using(var fixture=new Fixture(3)){
    var before=Pixels(fixture.Texture);fixture.Remove(fixture.Id(0,0,-3));fixture.Tick(0);var frozen=Pixels(fixture.Texture);CollectionAssert.AreEqual(before,frozen,"New shadows ease in rather than changing instantly");
    fixture.Tick(.06f);var mid=Pixels(fixture.Texture);Assert.IsFalse(mid.SequenceEqual(before));fixture.Tick(0);CollectionAssert.AreEqual(mid,Pixels(fixture.Texture));fixture.Tick(.2f);Assert.IsFalse(mid.SequenceEqual(Pixels(fixture.Texture)));
    int updated=(int)fixture.Type.GetProperty("LastRecomputedCount").GetValue(fixture.Visual,null);Assert.Greater(updated,0);Assert.LessOrEqual(updated,27,"A single removal only recalculates the local voxel ring");
    Assert.AreEqual(1,fixture.Renderers.Select(r=>r.sharedMaterial).Distinct().Count());Assert.IsTrue(fixture.Renderers.All(r=>!r.HasPropertyBlock()));
   }
  }
  [UnityTest] public IEnumerator RealPickupPauseRetryAndDetachedCargoKeepTheirExistingContracts(){
   yield return Load(7);var level=Object.FindObjectOfType<LevelOneController>();var board=level.GetComponent<HarvestBoardView>();var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();var root=GameObject.Find("Sheep(Clone)");var r=root.GetComponentInChildren<Renderer>();Assert.AreEqual("BeeHarvest/VoxelCavity",r.sharedMaterial.shader.name);
   var texture=(Texture2D)r.sharedMaterial.GetTexture("_CavityData");var initial=Pixels(texture);var palettes=root.GetComponentsInChildren<Renderer>().Select(v=>v.sharedMaterial).Distinct().ToArray();
   app.SendCommand(new ActivateHiveCommand(0));BeeSceneSteps.FirstPickup();var flight=model.Flights[0];Assert.IsFalse(GameObject.Find(model.Cubes[flight.Cube].Data.name).GetComponent<Renderer>().enabled);Assert.AreEqual(1030,model.Remaining+model.InTransit+model.Deposited);
   var cargo=new GameObject("CavityDetachedCargoProbe");var mesh=cargo.AddComponent<MeshFilter>();var material=cargo.AddComponent<MeshRenderer>();board.CopyPickupVisual(flight.Cube,mesh,material,new MaterialPropertyBlock());Assert.AreEqual("Universal Render Pipeline/Lit",material.sharedMaterial.shader.name,"A detached block must not sample the model's cavity map");Object.Destroy(cargo);
   app.SendCommand(new PauseHarvestCommand(true));var paused=Pixels(texture);yield return new WaitForSeconds(.15f);CollectionAssert.AreEqual(paused,Pixels(texture));app.SendCommand(new PauseHarvestCommand(false));yield return new WaitForSeconds(.18f);Assert.IsFalse(initial.SequenceEqual(Pixels(texture)));
   level.RestartLevel();board.Sync();CollectionAssert.AreEqual(initial,Pixels(texture));Assert.AreEqual(1030,root.GetComponentsInChildren<Renderer>().Count(v=>v.enabled));CollectionAssert.AreEquivalent(palettes,root.GetComponentsInChildren<Renderer>().Select(v=>v.sharedMaterial).Distinct());Assert.AreEqual(0,model.Flights.Count);
  }
 }
}
