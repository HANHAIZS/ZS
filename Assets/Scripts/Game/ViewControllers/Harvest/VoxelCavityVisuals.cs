using System;
using System.Collections.Generic;
using UnityEngine;
namespace BeeHarvest {
 /// <summary>Presentation-only face occlusion cache. A removed cell dirties at most its 26 neighbors.</summary>
 public sealed class VoxelCavityVisuals : IDisposable {
  const int Faces=6,TextureWidth=384;
  const float FadeDuration=.12f;
  static readonly Vector3Int[] Normals={Vector3Int.right,Vector3Int.left,Vector3Int.up,Vector3Int.down,new Vector3Int(0,0,1),new Vector3Int(0,0,-1)};
  static readonly Vector3Int[] U={new Vector3Int(0,0,1),new Vector3Int(0,0,1),Vector3Int.right,Vector3Int.right,Vector3Int.right,Vector3Int.right};
  static readonly Vector3Int[] V={Vector3Int.up,Vector3Int.up,new Vector3Int(0,0,1),new Vector3Int(0,0,1),Vector3Int.up,Vector3Int.up};
  readonly IReadOnlyList<CubeState> cells;
  readonly Renderer[] renderers;
  readonly Material[] originals;
  readonly List<Material> palettes=new List<Material>();
  readonly Vector3Int min,size;
  readonly Vector3Int[] coordinates;
  readonly int[] indices,ids;
  readonly bool[] occupied,animating;
  readonly float[] elapsed;
  readonly Color32[] current,from,target;
  readonly Color32[] row=new Color32[Faces];
  readonly List<int> active=new List<int>();
  readonly Texture2D texture;
  Matrix4x4 previousMatrix;
  bool hasMatrix;
  public int LastRecomputedCount {get;private set;}
  public VoxelCavityVisuals(IReadOnlyList<CubeState> source,Renderer[] sources){
   cells=source;renderers=sources;
   if(cells.Count==0||sources.Length!=cells.Count)throw new ArgumentException("Cavity visuals require matching nonempty voxel renderers");
   var low=new Vector3Int(int.MaxValue,int.MaxValue,int.MaxValue);var high=new Vector3Int(int.MinValue,int.MinValue,int.MinValue);
   coordinates=new Vector3Int[cells.Count];indices=new int[cells.Count];originals=new Material[cells.Count];
   for(int i=0;i<cells.Count;i++){var d=cells[i].Data;var p=new Vector3Int(d.x,d.y,d.z);coordinates[i]=p;low=Vector3Int.Min(low,p);high=Vector3Int.Max(high,p);}
   min=low;size=high-low+Vector3Int.one;int volume=size.x*size.y*size.z;
   occupied=new bool[volume];ids=new int[volume];for(int i=0;i<volume;i++)ids[i]=-1;
   for(int i=0;i<cells.Count;i++){indices[i]=Index(coordinates[i]);ids[indices[i]]=i;}
   int height=Mathf.CeilToInt(volume*Faces/(float)TextureWidth);
   current=new Color32[TextureWidth*height];from=new Color32[current.Length];target=new Color32[current.Length];
   elapsed=new float[volume];animating=new bool[volume];
   texture=new Texture2D(TextureWidth,height,TextureFormat.RGBA32,false,true){name="Local voxel occlusion",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
   var template=Resources.Load<Material>("Materials/Level07VoxelCavity");
   if(template==null)throw new InvalidOperationException("Missing Level07VoxelCavity material");
   var materials=new Dictionary<Material,Material>();
   for(int i=0;i<sources.Length;i++){
    var original=sources[i].sharedMaterial;originals[i]=original;Material material;
    if(!materials.TryGetValue(original,out material)){
     material=new Material(template){name=original.name+" (voxel cavity)",enableInstancing=true};
     material.SetColor("_BaseColor",original.GetColor("_BaseColor"));
     material.SetFloat("_Metallic",original.GetFloat("_Metallic"));material.SetFloat("_Smoothness",original.GetFloat("_Smoothness"));
     // Preserve existing box-edge style while adding local occlusion; unoutlined palettes keep width zero.
     if(original.HasProperty("_EdgeWidth")){
      material.SetFloat("_EdgeWidth",original.GetFloat("_EdgeWidth"));material.SetColor("_EdgeColor",original.GetColor("_EdgeColor"));
      material.SetVector("_VoxelCenter",original.GetVector("_VoxelCenter"));material.SetVector("_VoxelExtents",original.GetVector("_VoxelExtents"));
     }
     material.SetTexture("_CavityData",texture);material.SetVector("_CavityGridMin",new Vector4(min.x,min.y,min.z,0));
     material.SetVector("_CavityGridSize",new Vector4(size.x,size.y,size.z,0));material.SetVector("_CavityTextureSize",new Vector4(TextureWidth,height,0,0));
     materials.Add(original,material);palettes.Add(material);
    }
    sources[i].sharedMaterial=material;
   }
   Reset();
  }
  int Index(Vector3Int p){var v=p-min;return v.x+size.x*(v.y+size.y*v.z);}
  bool Contains(Vector3Int p){var v=p-min;return v.x>=0&&v.y>=0&&v.z>=0&&v.x<size.x&&v.y<size.y&&v.z<size.z;}
  bool Solid(Vector3Int p){return Contains(p)&&occupied[Index(p)];}
  byte Corner(Vector3Int outside,Vector3Int u,Vector3Int v){bool a=Solid(outside+u),b=Solid(outside+v);if(a&&b)return 255;int count=(a?1:0)+(b?1:0)+(Solid(outside+u+v)?1:0);return (byte)(count*85);}
  void Recompute(int index){
   int id=ids[index],offset=index*Faces;
   if(id<0||!occupied[index]){for(int f=0;f<Faces;f++)target[offset+f]=new Color32();return;}
   var p=coordinates[id];
   for(int f=0;f<Faces;f++){
    var outside=p+Normals[f];
    target[offset+f]=Solid(outside)?new Color32():new Color32(Corner(outside,-U[f],-V[f]),Corner(outside,U[f],-V[f]),Corner(outside,-U[f],V[f]),Corner(outside,U[f],V[f]));
   }
  }
  /// <summary>Restores initial shadows and reuses the same texture and four palette instances on retry.</summary>
  public void Reset(){
   Array.Clear(occupied,0,occupied.Length);Array.Clear(animating,0,animating.Length);active.Clear();
   Array.Clear(target,0,target.Length);for(int i=0;i<cells.Count;i++)occupied[indices[i]]=cells[i].Stage<2;
   for(int i=0;i<cells.Count;i++)Recompute(indices[i]);
   Array.Copy(target,current,target.Length);Array.Copy(target,from,target.Length);texture.SetPixels32(current);texture.Apply(false,false);LastRecomputedCount=0;
  }
  public Material OriginalMaterial(int id){return originals[id];}
  /// <summary>Queues an eased local visual change without changing the harvest model.</summary>
  public void Remove(int id){
   int removed=indices[id];if(!occupied[removed])return;occupied[removed]=false;LastRecomputedCount=0;
   var p=coordinates[id];
   for(int z=-1;z<=1;z++)for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++){
    var q=p+new Vector3Int(x,y,z);if(!Contains(q))continue;int index=Index(q);if(ids[index]<0)continue;
    Recompute(index);LastRecomputedCount++;int offset=index*Faces;bool changed=false;
    for(int f=0;f<Faces;f++){from[offset+f]=current[offset+f];if(!current[offset+f].Equals(target[offset+f]))changed=true;}
    if(!changed)continue;elapsed[index]=0;if(!animating[index]){animating[index]=true;active.Add(index);}
   }
  }
  public void Tick(float dt,Matrix4x4 gridToWorld){
   if(!hasMatrix||previousMatrix!=gridToWorld){previousMatrix=gridToWorld;hasMatrix=true;var inverse=gridToWorld.inverse;foreach(var material in palettes)material.SetMatrix("_CavityWorldToGrid",inverse);}
   if(dt<=0||float.IsNaN(dt)||float.IsInfinity(dt)||active.Count==0)return;
   bool changed=false;
   for(int a=active.Count-1;a>=0;a--){
    int index=active[a],offset=index*Faces;elapsed[index]=Mathf.Min(FadeDuration,elapsed[index]+dt);float t=elapsed[index]/FadeDuration;t=t*t*(3-2*t);
    for(int f=0;f<Faces;f++){current[offset+f]=Color32.Lerp(from[offset+f],target[offset+f],t);row[f]=current[offset+f];}
    texture.SetPixels32(offset%TextureWidth,offset/TextureWidth,Faces,1,row);changed=true;
    if(elapsed[index]>=FadeDuration){animating[index]=false;active[a]=active[active.Count-1];active.RemoveAt(active.Count-1);}
   }
   // Unity uploads this small shared texture once per changed frame, never once per renderer.
   if(changed)texture.Apply(false,false);
  }
  public void Dispose(){
   for(int i=0;i<renderers.Length;i++)if(renderers[i]!=null)renderers[i].sharedMaterial=originals[i];
   foreach(var material in palettes)UnityEngine.Object.Destroy(material);UnityEngine.Object.Destroy(texture);palettes.Clear();active.Clear();
  }
 }
}
