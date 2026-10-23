using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace BeeHarvest.Editor {
 /// <summary>Build-time surface variants; the runtime continues using ordinary shared prefab materials.</summary>
 public static class CubeLandVoxelTextureSetup {
  const string Art="Assets/Sprites/Gameplay/CubeLand", Textures=Art+"/SurfaceTextures", Materials=Art+"/SurfaceMaterials";
  static readonly Dictionary<Material,Material> surfaces=new Dictionary<Material,Material>();
  static Texture2D baseMap,normalMap;
  static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;int slash=path.LastIndexOf('/');Folder(path.Substring(0,slash));AssetDatabase.CreateFolder(path.Substring(0,slash),path.Substring(slash+1));}
  static Texture2D Texture(string name,bool normal){
   string path=Textures+"/"+name+".png";var importer=AssetImporter.GetAtPath(path) as TextureImporter;
   if(importer==null)throw new InvalidOperationException("Missing Cube Land voxel texture: "+path);
   importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal;importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Bilinear;importer.mipmapEnabled=true;importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.isReadable=false;importer.SaveAndReimport();
   return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  }
  /// <summary>Each authoring batch refreshes changed source palettes while retaining persistent variant GUIDs.</summary>
  public static void BeginBuild(){surfaces.Clear();baseMap=normalMap=null;}
  public static Material ForLevel(int number,Material source){
   if(number<9||number>12)return source;
   if(source==null)throw new InvalidOperationException("Missing voxel palette for textured level "+number);
   if(source.name.EndsWith("_Surface",StringComparison.Ordinal))source=AssetDatabase.LoadAssetAtPath<Material>(Art+"/"+source.name.Replace("_Surface","")+".mat");
   if(source==null)throw new InvalidOperationException("Missing original voxel palette");
   Material result;if(surfaces.TryGetValue(source,out result)&&result!=null)return result;
   if(baseMap==null)baseMap=AssetDatabase.LoadAssetAtPath<Texture2D>(Textures+"/Tex_Base_Voxel.png");if(normalMap==null)normalMap=AssetDatabase.LoadAssetAtPath<Texture2D>(Textures+"/Tex_Normal_Voxel.png");
   if(baseMap==null||normalMap==null)throw new InvalidOperationException("Prepare Cube Land voxel surface textures before rebuilding levels9–12");
   Folder(Materials);string path=Materials+"/"+source.name+"_Surface.mat";result=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(result==null){result=new Material(source);AssetDatabase.CreateAsset(result,path);}else{result.shader=source.shader;result.CopyPropertiesFromMaterial(source);}
   result.name=source.name+"_Surface";result.SetTexture("_BaseMap",baseMap);result.SetTexture("_BumpMap",normalMap);result.SetTextureScale("_BaseMap",new Vector2(4,4));result.SetTextureOffset("_BaseMap",new Vector2(.5f,0));
   if(result.HasProperty("_MainTex")){result.SetTexture("_MainTex",baseMap);result.SetTextureScale("_MainTex",new Vector2(4,4));result.SetTextureOffset("_MainTex",new Vector2(.5f,0));}
   result.SetFloat("_BumpScale",.45f);result.EnableKeyword("_NORMALMAP");result.enableInstancing=true;EditorUtility.SetDirty(result);AssetDatabase.SaveAssetIfDirty(result);surfaces[source]=result;return result;
  }
  public static string Apply(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before preparing voxel surfaces");
   BeginBuild();baseMap=Texture("Tex_Base_Voxel",false);normalMap=Texture("Tex_Normal_Voxel",true);
   for(int number=9;number<=12;number++){
    string path="Assets/Resources/Prefabs/Gameplay/"+LevelOneAssets.ModelKey(number)+".prefab";var root=PrefabUtility.LoadPrefabContents(path);
    try{foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))renderer.sharedMaterial=ForLevel(number,renderer.sharedMaterial);PrefabUtility.SaveAsPrefabAsset(root,path);}
    finally{PrefabUtility.UnloadPrefabContents(root);}
   }
   return "Textured four level prefabs using "+surfaces.Count+" shared palette variants and two original voxel textures";
  }
 }
}
