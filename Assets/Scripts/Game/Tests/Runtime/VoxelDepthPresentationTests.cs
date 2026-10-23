using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class VoxelDepthPresentationTests : EconomyTestFixture {
  [UnityTest] public IEnumerator RequestedScenesKeepCentersPaletteAndCargoWithSevenPercentGapsAndSoftLighting(){
   foreach(int n in new[]{1,2,3,4,5,6,106,7,8}){
    LevelSceneTestLoader.Load(LevelOneAssets.LevelKey(n));yield return null;yield return null;
    var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var board=level.GetComponent<HarvestBoardView>();board.AutoRotate=false;board.ResetRotation();
    var root=GameObject.Find(LevelOneAssets.ModelKey(n)+"(Clone)");var rs=root.GetComponentsInChildren<Renderer>();var originals=Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.ModelKey(n)).GetComponentsInChildren<Renderer>().ToDictionary(r=>r.name);
    var model=BeeHarvestApp.Interface.GetModel<HarvestModel>();bool target=n<=6||n==106;Assert.AreEqual(model.Cubes.Count,rs.Length);
    Assert.Less(Vector3.Distance(root.transform.localScale,Vector3.one*(target?.86f*1.04f:.86f)*LevelOneAssets.Presentation(n).modelScaleMultiplier),.00001f,"Deliberately modest whole-model enlargement");
    var coords=model.Cubes.ToDictionary(c=>new Vector3Int(c.Data.x,c.Data.y,c.Data.z));var renders=rs.ToDictionary(r=>r.name);
    foreach(var c in model.Cubes){var r=renders[c.Data.name];var original=originals[r.name];Assert.AreEqual(original.transform.localPosition,r.transform.localPosition,"Centers within the model must not move");Assert.AreEqual(original.transform.localRotation,r.transform.localRotation);Assert.AreSame(original.GetComponent<MeshFilter>().sharedMesh,r.GetComponent<MeshFilter>().sharedMesh);Assert.Less(Vector4.Distance(original.sharedMaterial.GetColor("_BaseColor"),r.sharedMaterial.GetColor("_BaseColor")),.00001f);
     if(target){Assert.AreEqual("BeeHarvest/VoxelCavity",r.sharedMaterial.shader.name);Assert.IsFalse(r.HasPropertyBlock());var p=new Vector3Int(c.Data.x,c.Data.y,c.Data.z);foreach(var axis in new[]{Vector3Int.right,Vector3Int.up,new Vector3Int(0,0,1)}){CubeState next;if(!coords.TryGetValue(p+axis,out next))continue;var delta=renders[next.Data.name].transform.localPosition-r.transform.localPosition;float width=Vector3.Dot(r.bounds.size,new Vector3(Mathf.Abs(delta.normalized.x),Mathf.Abs(delta.normalized.y),Mathf.Abs(delta.normalized.z)))/root.transform.lossyScale.x;float gap=1-width/delta.magnitude;Assert.That(gap,Is.InRange(.069f,.071f),"Physical mesh bounds vs unchanged center spacing, level "+n);}
      float edge=LevelOneAssets.HasVoxelEdges(n)?.11f:0;Assert.AreEqual(edge,r.sharedMaterial.GetFloat("_EdgeWidth"));if(edge>0)Assert.Less(Vector4.Distance(LevelOneAssets.VoxelEdgeColor(n)??Color.black,r.sharedMaterial.GetColor("_EdgeColor")),.00001f);
     }else Assert.AreEqual(original.transform.localScale,r.transform.localScale);
    }
    if(target){var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);var center=Camera.main.WorldToViewportPoint(bounds.center);Assert.AreEqual(.5f,center.x,.002f);Assert.AreEqual(LevelOneAssets.Presentation(n).modelCenterY+board.LayoutViewportYOffset,center.y,.002f);var lo=Vector2.one;var hi=Vector2.zero;for(int corner=0;corner<8;corner++){var point=bounds.center+Vector3.Scale(bounds.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1));Vector2 screen=Camera.main.WorldToViewportPoint(point);lo=Vector2.Min(lo,screen);hi=Vector2.Max(hi,screen);}TestContext.WriteLine("Level "+n+" viewport bounds "+lo+" -> "+hi);Assert.Greater(lo.x,.035f);Assert.Less(hi.x,.965f);Assert.Greater(lo.y,.4f);Assert.Less(hi.y,.9f);}
    var light=GameObject.Find("KeyLight").GetComponent<Light>();Assert.AreEqual(target?LightShadows.Soft:LightShadows.None,light.shadows);Assert.Less(Vector4.Distance(target?new Color(.48f,.52f,.63f):new Color(.65f,.7f,.85f),RenderSettings.ambientLight),.00001f);if(target)Assert.AreEqual(.3f,light.shadowStrength);
    if(target){var probe=new GameObject("DepthCargoProbe");var filter=probe.AddComponent<MeshFilter>();var renderer=probe.AddComponent<MeshRenderer>();board.CopyPickupVisual(0,filter,renderer,new MaterialPropertyBlock());Assert.AreEqual(LevelOneAssets.HasVoxelEdges(n)?"BeeHarvest/VoxelEdges":"Universal Render Pipeline/Lit",renderer.sharedMaterial.shader.name);Assert.AreEqual(renders[model.Cubes[0].Data.name].transform.lossyScale,board.PickupScale(0));Object.Destroy(probe);}
   }
  }
  [UnityTest] public IEnumerator ActualGpuSoftShadowChangesReceiverGently(){
   LevelSceneTestLoader.Load("Level06");yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;
   var light=GameObject.Find("KeyLight").GetComponent<Light>();var priorShadows=light.shadows;
   var root=new GameObject("SoftShadowFixture");root.transform.position=new Vector3(1000,1000,1000);
   var receiver=GameObject.CreatePrimitive(PrimitiveType.Plane);receiver.transform.SetParent(root.transform,false);receiver.transform.localScale=Vector3.one*.6f;receiver.layer=30;
   var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.transform.SetParent(root.transform,false);blocker.transform.localPosition=Vector3.up;blocker.layer=30;
   var material=new Material(GameObject.Find("Chick(Clone)").GetComponentInChildren<Renderer>().sharedMaterial);material.SetColor("_BaseColor",Color.white);material.SetFloat("_Smoothness",0);material.SetFloat("_CavityStrength",0);material.SetFloat("_EdgeWidth",0);receiver.GetComponent<Renderer>().sharedMaterial=material;blocker.GetComponent<Renderer>().sharedMaterial=material;
   var cameraObject=new GameObject("SoftShadowTestCamera");var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=3.5f;camera.nearClipPlane=.1f;camera.farClipPlane=20;camera.cullingMask=1<<30;camera.transform.position=root.transform.position+new Vector3(3,6,-6);camera.transform.LookAt(root.transform.position);
   var target=new RenderTexture(192,192,24);var image=new Texture2D(192,192,TextureFormat.RGB24,false);var previous=RenderTexture.active;camera.targetTexture=target;
   try{
    light.shadows=LightShadows.None;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,192,192),0,0);image.Apply();var baseline=image.GetPixels();
    light.shadows=LightShadows.Soft;camera.Render();image.ReadPixels(new Rect(0,0,192,192),0,0);image.Apply();var shaded=image.GetPixels();int changed=0;float largest=0;
    for(int i=0;i<baseline.Length;i++){float difference=baseline[i].grayscale-shaded[i].grayscale;if(difference>.01f)changed++;largest=Mathf.Max(largest,difference);}
    TestContext.WriteLine("Soft shadow pixels="+changed+", max brightness decrease="+largest);Assert.Greater(changed,20,"Soft shadows must actually render, not only have a Light flag");Assert.That(largest,Is.InRange(.015f,.35f),"Keep the low-strength projected shadow gentle");
   }finally{light.shadows=priorShadows;RenderTexture.active=previous;camera.targetTexture=null;target.Release();Object.Destroy(target);Object.Destroy(image);Object.Destroy(cameraObject);Object.Destroy(root);Object.Destroy(material);}
  }
 }
}
