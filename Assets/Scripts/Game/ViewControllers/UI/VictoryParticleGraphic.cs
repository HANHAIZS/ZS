using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest {
 /// <summary>Draws a bounded native particle mesh in the overlay Canvas, retaining the source atlas animation.</summary>
 [RequireComponent(typeof(ParticleSystem),typeof(CanvasRenderer))]
 public sealed class VictoryParticleGraphic : MaskableGraphic {
  public Texture ParticleTexture;
  ParticleSystemRenderer particleRenderer;
  Camera bakeCamera;
  Mesh mesh;
  readonly List<Vector3> positions=new List<Vector3>(256);
  readonly List<Color32> colors=new List<Color32>(256);
  readonly List<Vector2> uvs=new List<Vector2>(256);
  readonly List<int> triangles=new List<int>(384);
  public override Texture mainTexture=>ParticleTexture!=null?ParticleTexture:Texture2D.whiteTexture;
  protected override void Awake(){base.Awake();raycastTarget=false;particleRenderer=GetComponent<ParticleSystemRenderer>();particleRenderer.enabled=false;bakeCamera=Camera.main;mesh=new Mesh{name="Victory UI particle mesh"};mesh.MarkDynamic();}
  void LateUpdate(){if(isActiveAndEnabled)SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();if(mesh==null||particleRenderer==null||bakeCamera==null)return;
   particleRenderer.BakeMesh(mesh,bakeCamera,(ParticleSystemBakeMeshOptions)0);
   mesh.GetVertices(positions);mesh.GetColors(colors);mesh.GetUVs(0,uvs);mesh.GetTriangles(triangles,0);
   for(int i=0;i<positions.Count;i++){var p=positions[i]*100f;p.z=0;vh.AddVert(p,colors.Count>i?colors[i]:(Color32)Color.white,uvs.Count>i?uvs[i]:Vector2.zero);}
   for(int i=0;i+2<triangles.Count;i+=3)vh.AddTriangle(triangles[i],triangles[i+1],triangles[i+2]);
  }
  protected override void OnDestroy(){if(mesh!=null)Destroy(mesh);base.OnDestroy();}
 }
}
