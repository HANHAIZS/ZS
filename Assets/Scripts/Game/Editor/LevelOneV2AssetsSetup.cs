using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace BeeHarvest.Editor {
 public static class LevelOneV2AssetsSetup {
  const string Root="Assets/Sprites/Gameplay/V2";
  static readonly string[] Colors={"Red","Orange","Yellow","Green","Cyan","Blue","Purple","White","Black"};
  [Serializable] sealed class Manifest{public Voxel[] voxels;}
  [Serializable] sealed class Voxel{public int id;public string name,color;public int[] grid;}
  /// <summary>Updates existing Resources prefab identities through Editor APIs; scene/UI references are preserved.</summary>
  public static void Apply(string manifestPath){
   Ensure(Root+"/Materials");Ensure("Assets/Resources/Prefabs/Gameplay");
   foreach(var color in Colors){Make("Bee_"+color,"Bee"+color,0,false);Make("Small_Hive_"+color,"Hive"+color,1.02f,false);}
   Make("Flower_Voxel_V2","Flower",4.63f,true);Make("Large_Hive_Collector","Collector",2.12f,false);
   Make("Bee_Red","Bee",0,false);Make("Small_Hive_Red","HivePink",1.02f,false);
   var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));var data=new LevelData{cubes=new CubeData[manifest.voxels.Length],hives=new[]{
    new HiveData{column=0,color="Red",quota=79},new HiveData{column=0,color="Red",quota=79},
    new HiveData{column=1,color="Yellow",quota=39},new HiveData{column=2,color="Green",quota=42},
    new HiveData{column=3,color="Red",quota=79},new HiveData{column=3,color="Red",quota=79},new HiveData{column=4,color="Orange",quota=22},
    new HiveData{column=1,color="Yellow",quota=39},new HiveData{column=2,color="Green",quota=42},new HiveData{column=4,color="Orange",quota=22}}};
   foreach(var v in manifest.voxels)data.cubes[v.id]=new CubeData{id=v.id,name=v.name,color=v.color,x=v.grid[0],y=v.grid[2],z=-v.grid[1]};
   var path="Assets/Resources/Levels/Level01.json";File.WriteAllText(path,JsonUtility.ToJson(data,true));AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);AssetDatabase.SaveAssets();
   Debug.Log("V2_ASSETS_READY: 9 bee prefabs, 9 hive prefabs, collector, 522 independent flower cubes");
  }
  static void Make(string source,string key,float height,bool turn){
   var path=Root+"/Models/"+source+".fbx";var imp=(ModelImporter)AssetImporter.GetAtPath(path);if(imp==null)throw new InvalidOperationException(path);
   imp.isReadable=source.StartsWith("Bee_");imp.importAnimation=false;imp.globalScale=1;imp.SaveAndReimport();
   var original=AssetDatabase.LoadAssetAtPath<GameObject>(path);var go=new GameObject(key);
   try{
    var art=UnityEngine.Object.Instantiate(original,go.transform,false);art.name=source+"_BlenderArt";
    // The Blender flower front is +Z after FBX conversion; turn its raised yellow center toward the level camera.
    if(turn)art.transform.localRotation=Quaternion.Euler(0,180,0)*art.transform.localRotation;
    var renderers=art.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
    art.transform.localPosition-=b.center;float scale=height>0?height/b.size.y:1;art.transform.localScale*=scale;art.transform.localPosition*=scale;
    foreach(var r in renderers){var ms=r.sharedMaterials;for(int i=0;i<ms.Length;i++)ms[i]=MaterialFor(ms[i].name);r.sharedMaterials=ms;}
    var animator=art.GetComponent<Animator>();if(animator!=null)UnityEngine.Object.DestroyImmediate(animator);
    PrefabUtility.SaveAsPrefabAsset(go,"Assets/Resources/Prefabs/Gameplay/"+key+".prefab");
   }finally{UnityEngine.Object.DestroyImmediate(go);}
  }
  static Material MaterialFor(string key){
   var path=Root+"/Materials/"+key+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.name=key;AssetDatabase.CreateAsset(mat,path);}
   Color c;if(key.Contains("Dark"))ColorUtility.TryParseHtmlString("#111A17",out c);
   else if(key.Contains("White_Wing"))c=Color.white;
   else{string color=null;foreach(var value in Colors)if(key.EndsWith("_"+value)||key.Contains("_"+value+"_")){color=value;break;}if(color==null)throw new InvalidOperationException("Unknown Blender material "+key);c=LevelOneAssets.ColorOf(color);}
   mat.SetColor("_BaseColor",c);mat.SetFloat("_Smoothness",key.Contains("Wing")?.25f:.4f);EditorUtility.SetDirty(mat);return mat;
  }
  static void Ensure(string path){if(AssetDatabase.IsValidFolder(path))return;int slash=path.LastIndexOf('/');Ensure(path.Substring(0,slash));AssetDatabase.CreateFolder(path.Substring(0,slash),path.Substring(slash+1));}
 }
}
