using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest {
 /// <summary>Scales UI vertices, including TMP glyphs, without moving their input or stencil rectangles.</summary>
 [DisallowMultipleComponent]
 public sealed class ButtonPressGraphic : BaseMeshEffect {
  ButtonPressMotion owner;RectTransform rect;TMP_Text text;
  Vector3[][] originals;bool subscribed;
  public void Initialize(ButtonPressMotion motion) {
   owner=motion;rect=(RectTransform)transform;text=GetComponent<TMP_Text>();
   if(isActiveAndEnabled)Subscribe();
   if(graphic!=null)graphic.SetVerticesDirty();
  }
  protected override void OnEnable(){base.OnEnable();Subscribe();}
  protected override void OnDisable(){Unsubscribe();base.OnDisable();}
  void Subscribe(){if(text!=null&&!subscribed){text.OnPreRenderText+=TextRebuilt;subscribed=true;}}
  void Unsubscribe(){if(text!=null&&subscribed)text.OnPreRenderText-=TextRebuilt;subscribed=false;}
  public override void ModifyMesh(VertexHelper mesh) {
   if(!IsActive()||owner==null||owner.VisualScale==1||text!=null)return;
   Vector3 pivot=rect.InverseTransformPoint(owner.PivotWorld);var vertex=new UIVertex();
   for(int i=0;i<mesh.currentVertCount;i++){mesh.PopulateUIVertex(ref vertex,i);vertex.position=pivot+(vertex.position-pivot)*owner.VisualScale;mesh.SetUIVertex(vertex,i);}
  }
  void TextRebuilt(TMP_TextInfo info) {
   // TMP rebuild callbacks contain fresh unscaled glyphs. Reuse buffers, including after text/width changes.
   if(originals==null)originals=new Vector3[info.materialCount][];
   else if(originals.Length<info.materialCount)Array.Resize(ref originals,info.materialCount);
   for(int i=0;i<info.materialCount;i++) {
    var mesh=info.meshInfo[i];
    if(originals[i]==null||originals[i].Length<mesh.vertexCount)originals[i]=new Vector3[mesh.vertices.Length];
    Array.Copy(mesh.vertices,originals[i],mesh.vertexCount);
   }
   ApplyText(info);
  }
  void ApplyText(TMP_TextInfo info) {
   if(owner==null||originals==null)return;
   Vector3 pivot=rect.InverseTransformPoint(owner.PivotWorld);float factor=owner.VisualScale;
   for(int i=0;i<info.materialCount;i++)for(int j=0;j<info.meshInfo[i].vertexCount;j++)
    info.meshInfo[i].vertices[j]=pivot+(originals[i][j]-pivot)*factor;
  }
  public void Redraw() {
   if(graphic==null)return;
   if(text==null){graphic.SetVerticesDirty();return;}
   if(!text.isActiveAndEnabled)return;
   if(originals==null||text.havePropertiesChanged||owner.VisualScale==1)text.ForceMeshUpdate();
   else {ApplyText(text.textInfo);text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);}
  }
 }
}
