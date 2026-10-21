using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class ChickVoxelEdgeTests : EconomyTestFixture {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Object.FindObjectOfType<HarvestBoardView>();
  static IEnumerator Load(int n){SceneManager.LoadScene(LevelOneAssets.SceneName(n));yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;Board.AutoRotate=false;Board.ResetRotation();}
  [UnityTest] public IEnumerator ChickUsesOneBorderedPalettePerOriginalColorAndKeepsExactMeshes(){
   yield return Load(6);var root=GameObject.Find("Chick(Clone)");var renders=root.GetComponentsInChildren<Renderer>();Assert.AreEqual(870,renders.Length);Assert.AreEqual(4,renders.Select(v=>v.sharedMaterial).Distinct().Count());
   var prefab=Resources.Load<GameObject>("Prefabs/Gameplay/Chick");var originals=prefab.GetComponentsInChildren<MeshFilter>().ToDictionary(f=>f.name);
   foreach(var renderer in renders){Assert.AreEqual("BeeHarvest/VoxelEdges",renderer.sharedMaterial.shader.name);Assert.AreSame(originals[renderer.name].sharedMesh,renderer.GetComponent<MeshFilter>().sharedMesh);var original=originals[renderer.name].GetComponent<Renderer>().sharedMaterial;Assert.Less(Vector4.Distance(original.GetColor("_BaseColor"),renderer.sharedMaterial.GetColor("_BaseColor")),.00001f);Assert.AreEqual("Universal Render Pipeline/Lit",original.shader.name);}
   var materials=renders.Select(v=>v.sharedMaterial).Distinct().ToArray();Object.FindObjectOfType<LevelOneController>().RestartLevel();Board.Sync();Assert.AreEqual(870,root.GetComponentsInChildren<Renderer>().Count(v=>v.enabled));CollectionAssert.AreEquivalent(materials,root.GetComponentsInChildren<Renderer>().Select(v=>v.sharedMaterial).Distinct());
   SceneManager.LoadScene("Level08");yield return null;yield return null;Assert.IsTrue(materials.All(m=>m==null));Assert.IsTrue(GameObject.Find("Shark(Clone)").GetComponentsInChildren<Renderer>().All(v=>v.sharedMaterial.shader.name=="Universal Render Pipeline/Lit"));
  }
  [UnityTest] public IEnumerator ActualGpuRenderShowsBlackEdgesAndBrightFaceWithoutTriangleDiagonal(){
   yield return Load(6);int id=Enumerable.Range(0,Model.Cubes.Count).First(i=>Model.Cubes[i].Data.color=="Palette13");
   var cube=new GameObject("EdgeRenderProbe");var camObject=new GameObject("EdgeRenderCamera");var texture=new Texture2D(128,128,TextureFormat.RGB24,false);var target=new RenderTexture(128,128,24);var previous=RenderTexture.active;
   try{
    cube.layer=30;cube.transform.position=new Vector3(1000,1000,1000);var filter=cube.AddComponent<MeshFilter>();var renderer=cube.AddComponent<MeshRenderer>();Board.CopyPickupVisual(id,filter,renderer,new MaterialPropertyBlock());
    var camera=camObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=.64f;camera.nearClipPlane=.1f;camera.farClipPlane=10;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.blue;camera.transform.position=cube.transform.position+Vector3.back*3;camera.targetTexture=target;
    camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,128,128),0,0);texture.Apply();
    Assert.Greater(texture.GetPixel(64,64).grayscale,.25f,"Face center must preserve its white color, without a triangle wire");Assert.Greater(texture.GetPixel(40,40).grayscale,.25f,"Face diagonal must remain colored");
    int black=0;for(int x=15;x<22;x++)if(texture.GetPixel(x,64).maxColorComponent<.12f)black++;
    Assert.GreaterOrEqual(black,1,"A real visible cube boundary must contain a black outline");
   }finally{RenderTexture.active=previous;Object.Destroy(cube);Object.Destroy(camObject);Object.Destroy(texture);target.Release();Object.Destroy(target);}
  }
 }
}
