using System;
using System.Collections.Generic;
using UnityEngine;
namespace BeeHarvest {
 /// <summary>Owns one level's shared outlined display palettes; original meshes and materials remain intact.</summary>
 public sealed class VoxelEdgeVisuals : IDisposable {
  readonly Material template;
  readonly Bounds bounds;
  readonly Dictionary<Material,Material> palettes=new Dictionary<Material,Material>();
  public VoxelEdgeVisuals(Bounds meshBounds){
   template=Resources.Load<Material>("Materials/ChickVoxelOutline");
   if(template==null||template.shader==null||!template.shader.isSupported)throw new InvalidOperationException("Missing supported Chick voxel-outline material");
   bounds=meshBounds;
  }
  public void Apply(Renderer renderer){
   var source=renderer.sharedMaterial;Material palette;
   if(!palettes.TryGetValue(source,out palette)){
    palette=new Material(template){name=source.name+"_VoxelEdges",enableInstancing=true};
    palette.SetColor("_BaseColor",source.GetColor("_BaseColor"));
    palette.SetFloat("_Metallic",source.GetFloat("_Metallic"));palette.SetFloat("_Smoothness",source.GetFloat("_Smoothness"));
    palette.SetVector("_VoxelCenter",bounds.center);palette.SetVector("_VoxelExtents",bounds.extents);
    palettes.Add(source,palette);
   }
   renderer.sharedMaterial=palette;
  }
  public void Dispose(){foreach(var palette in palettes.Values)if(palette!=null)UnityEngine.Object.Destroy(palette);palettes.Clear();}
 }
}
