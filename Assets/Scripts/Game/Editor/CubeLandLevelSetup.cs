using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace BeeHarvest.Editor {
 public static class CubeLandLevelSetup {
  const string Art="Assets/Sprites/Gameplay/CubeLand", Prefabs="Assets/Resources/Prefabs/Gameplay/";
  static void Folder(string p){if(AssetDatabase.IsValidFolder(p))return;int n=p.LastIndexOf('/');Folder(p.Substring(0,n));AssetDatabase.CreateFolder(p.Substring(0,n),p.Substring(n+1));}
  public static string Apply(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before rebuilding source levels");
   Folder(Art);Folder("Assets/Resources/Materials");
   var original=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+"Flower.prefab");
   string meshPath=Art+"/Voxel.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
   if(!mesh){mesh=UnityEngine.Object.Instantiate(original.GetComponentInChildren<MeshFilter>().sharedMesh);mesh.name="CubeLandVoxel";var b=mesh.bounds;float size=Mathf.Max(b.size.x,b.size.y,b.size.z);if(size<=0)throw new InvalidOperationException("Empty voxel prototype");var verts=mesh.vertices;for(int i=0;i<verts.Length;i++)verts[i]=(verts[i]-b.center)/size*.975f;mesh.vertices=verts;mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,meshPath);}
   var prototype=original.GetComponentInChildren<MeshRenderer>().sharedMaterial;
   foreach(int id in CubeLandPalette.Ids){string key="Palette"+id.ToString("00");MakeMaterial(Art+"/Voxel"+key+".mat",prototype,CubeLandPalette.ColorOf(id));TintPrefab("Hive",key,id);TintPrefab("Bee",key,id);}
   var shader=Shader.Find("Universal Render Pipeline/Unlit");if(!shader)throw new InvalidOperationException("Missing existing URP Unlit shader");
   string linkPath="Assets/Resources/Materials/HarvestLink.mat";var link=AssetDatabase.LoadAssetAtPath<Material>(linkPath);if(!link){link=new Material(shader);AssetDatabase.CreateAsset(link,linkPath);}link.SetColor("_BaseColor",new Color(1f,.72f,.06f));EditorUtility.SetDirty(link);
   for(int number=1;number<=LevelOneAssets.LastLevel;number++)BuildModel(number,mesh);
   AssetDatabase.SaveAssets();return "Imported "+LevelOneAssets.LastLevel+" source levels and "+CubeLandPalette.Ids.Length+" independent palette hive/bee pairs.";
  }
  /// <summary>Builds one selected source level without rewriting the other production models.</summary>
  public static void ApplyLevel(int number){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before rebuilding a source level");
   LevelOneAssets.ModelKey(number);Folder(Art);
   var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Art+"/Voxel.asset");if(!mesh)throw new InvalidOperationException("Shared voxel mesh is missing");
   var text=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Levels/"+LevelOneAssets.SceneName(number)+".json");if(!text)throw new InvalidOperationException("Source level data is missing");
   var data=JsonUtility.FromJson<LevelData>(text.text);var original=AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+"Flower.prefab");var prototype=original.GetComponentInChildren<MeshRenderer>().sharedMaterial;
   foreach(int id in data.cubes.Select(c=>c.sourceColor).Distinct()){
    string key="Palette"+id.ToString("00");var color=CubeLandPalette.ColorOf(id);
    if(!AssetDatabase.LoadAssetAtPath<Material>(Art+"/Voxel"+key+".mat"))MakeMaterial(Art+"/Voxel"+key+".mat",prototype,color);
    foreach(var kind in new[]{"Bee","Hive"})if(!AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+kind+key+".prefab"))TintPrefab(kind,key,id);
   }
   BuildModel(number,mesh);AssetDatabase.SaveAssets();
  }
  static void BuildModel(int number,Mesh mesh){
    var text=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Levels/Level"+number.ToString("00")+".json");var data=JsonUtility.FromJson<LevelData>(text.text);
    var min=new Vector3(data.cubes.Min(c=>c.x),data.cubes.Min(c=>c.y),data.cubes.Min(c=>c.z));var max=new Vector3(data.cubes.Max(c=>c.x),data.cubes.Max(c=>c.y),data.cubes.Max(c=>c.z));var size=max-min+Vector3.one;float cell=Mathf.Min(4.8f/size.y,4.6f/size.x,4.6f/size.z);var center=(min+max)*.5f;
    string name=LevelOneAssets.ModelKey(number);var root=new GameObject(name);Transform parent=root.transform;
    // Authored orientation shows the silhouette/face at the unchanged runtime zero yaw.
    if(number==2||number==3||number==6||number==7||number==10||number==12||number==15||new[]{17,18,19,21,24,26,30,35,55,58,62,64,71,78,79,80,82,84,85,86,87,88,89,95,99,101,103,104,105}.Contains(number)){var orientation=new GameObject(LevelOneAssets.ModelKey(number)+"Orientation");orientation.transform.SetParent(root.transform,false);orientation.transform.localRotation=Quaternion.Euler(0,number==3||number==10?90:180,0);parent=orientation.transform;}
    try{foreach(var cube in data.cubes){var child=new GameObject(cube.name);child.transform.SetParent(parent,false);child.transform.localPosition=(new Vector3(cube.x,cube.y,cube.z)-center)*cell;child.transform.localScale=Vector3.one*cell;child.AddComponent<MeshFilter>().sharedMesh=mesh;child.AddComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Voxel"+cube.color+".mat");}PrefabUtility.SaveAsPrefabAsset(root,Prefabs+name+".prefab");}finally{UnityEngine.Object.DestroyImmediate(root);}
  }
  static Material MakeMaterial(string path,Material source,Color color){var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(source);AssetDatabase.CreateAsset(material,path);}if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",color);if(material.HasProperty("_Color"))material.SetColor("_Color",color);EditorUtility.SetDirty(material);return material;}
  static void TintPrefab(string kind,string key,int id){
   var root=PrefabUtility.LoadPrefabContents(Prefabs+kind+"Red.prefab");
   try{root.name=kind+key;foreach(var renderer in root.GetComponentsInChildren<Renderer>(true)){var materials=renderer.sharedMaterials;for(int i=0;i<materials.Length;i++)if(materials[i]&&materials[i].name.IndexOf("_Tint_",StringComparison.Ordinal)>=0)materials[i]=MakeMaterial(Art+"/"+kind+key+".mat",materials[i],CubeLandPalette.ColorOf(id));renderer.sharedMaterials=materials;}PrefabUtility.SaveAsPrefabAsset(root,Prefabs+kind+key+".prefab");}finally{PrefabUtility.UnloadPrefabContents(root);}
  }
 }
}
