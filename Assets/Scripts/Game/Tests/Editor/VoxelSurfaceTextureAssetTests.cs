using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class VoxelSurfaceTextureAssetTests {
  const string Art="Assets/Sprites/Gameplay/CubeLand/";
  [Test] public void FourModelPrefabsUseSharedOriginalSurfaceMapsAndRetainTheirPaletteAndMesh(){
   foreach(int n in Enumerable.Range(9,4)){
    var prefab=Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.ModelKey(n));var rs=prefab.GetComponentsInChildren<Renderer>();
    foreach(var r in rs){var m=r.sharedMaterial;Assert.IsNotNull(m.GetTexture("_BaseMap"),"Surface map missing at "+n);Assert.IsNotNull(m.GetTexture("_BumpMap"));Assert.AreEqual("Tex_Base_Voxel",m.GetTexture("_BaseMap").name);Assert.AreEqual("Tex_Normal_Voxel",m.GetTexture("_BumpMap").name);Assert.IsTrue(m.IsKeywordEnabled("_NORMALMAP"));Assert.AreEqual(.45f,m.GetFloat("_BumpScale"));Assert.AreEqual(new Vector2(4,4),m.GetTextureScale("_BaseMap"));Assert.AreEqual(new Vector2(.5f,0),m.GetTextureOffset("_BaseMap"));
     var original=AssetDatabase.LoadAssetAtPath<Material>(Art+m.name.Replace("_Surface","")+".mat");Assert.IsNotNull(original);Assert.Less(Vector4.Distance(original.GetColor("_BaseColor"),m.GetColor("_BaseColor")),.00001f);Assert.AreEqual(original.GetFloat("_Smoothness"),m.GetFloat("_Smoothness"));Assert.AreEqual(original.GetFloat("_Metallic"),m.GetFloat("_Metallic"));Assert.AreSame(original.shader,m.shader);Assert.IsNull(original.GetTexture("_BaseMap"));Assert.IsNull(original.GetTexture("_BumpMap"));
     var mesh=r.GetComponent<MeshFilter>().sharedMesh;Assert.AreEqual(Art+"Voxel.asset",AssetDatabase.GetAssetPath(mesh));Assert.AreEqual(mesh.vertexCount,mesh.tangents.Length);}
    Assert.AreEqual(rs.Select(r=>r.sharedMaterial.GetColor("_BaseColor")).Distinct().Count(),rs.Select(r=>r.sharedMaterial).Distinct().Count(),"One shared material per palette, never per cube");
   }
  }
  [Test] public void RebuildBatchRefreshesChangedPaletteWithoutCreatingAnotherMaterial(){
   var type=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("BeeHarvest.Editor.CubeLandVoxelTextureSetup")).First(t=>t!=null);var method=type.GetMethod("ForLevel");var begin=type.GetMethod("BeginBuild");
   var source=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="VoxelSurfaceCacheProbe_"+System.Guid.NewGuid().ToString("N")};string path=null;
   try{begin?.Invoke(null,null);source.SetColor("_BaseColor",new Color(.3f,.7f,.2f));var first=(Material)method.Invoke(null,new object[]{9,source});path=AssetDatabase.GetAssetPath(first);string guid=AssetDatabase.AssetPathToGUID(path);source.SetColor("_BaseColor",new Color(.7f,.2f,.4f));source.SetFloat("_Smoothness",.25f);begin?.Invoke(null,null);var rebuilt=(Material)method.Invoke(null,new object[]{9,source});Assert.AreEqual(path,AssetDatabase.GetAssetPath(rebuilt));Assert.AreEqual(guid,AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(rebuilt)),"Keep the same persistent asset GUID and serialized references");Assert.Less(Vector4.Distance(source.GetColor("_BaseColor"),rebuilt.GetColor("_BaseColor")),.00001f,"A new authoring batch must copy updated source color");Assert.AreEqual(.25f,rebuilt.GetFloat("_Smoothness"));Assert.AreSame(source,method.Invoke(null,new object[]{8,source}),"Outside scope remains the original material");}
   finally{if(!string.IsNullOrEmpty(path))AssetDatabase.DeleteAsset(path);Object.DestroyImmediate(source);begin?.Invoke(null,null);}
  }
  [Test] public void TextureImportsKeepColorAndNormalInTheirCorrectColorSpaces(){
   foreach(bool normal in new[]{false,true}){string name=normal?"Tex_Normal_Voxel":"Tex_Base_Voxel";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"SurfaceTextures/"+name+".png");Assert.IsNotNull(texture);Assert.AreEqual(512,texture.width);Assert.AreEqual(512,texture.height);var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));Assert.AreEqual(normal?TextureImporterType.NormalMap:TextureImporterType.Default,importer.textureType);Assert.AreEqual(!normal,importer.sRGBTexture);Assert.AreEqual(TextureWrapMode.Repeat,importer.wrapMode);Assert.IsTrue(importer.mipmapEnabled);}
  }
 }
}
