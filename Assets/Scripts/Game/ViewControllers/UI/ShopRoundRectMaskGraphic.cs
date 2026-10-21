using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest {
 // A vector stencil for composed shop artwork; source textures remain unchanged.
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class ShopRoundRectMaskGraphic : MaskableGraphic {
  [Range(0f,.5f)] public float RadiusFraction=.5f;
  protected override void OnPopulateMesh(VertexHelper mesh){
   mesh.Clear();var rect=GetPixelAdjustedRect();
   if(rect.width<=0f||rect.height<=0f)return;
   float radius=Mathf.Min(rect.width*.5f,rect.height*Mathf.Clamp(RadiusFraction,0f,.5f));
   mesh.AddVert(rect.center,color,Vector2.zero);
   const int segments=16;
   for(int corner=0;corner<4;corner++){
    var center=new Vector2(corner==0||corner==3?rect.xMax-radius:rect.xMin+radius,corner<2?rect.yMax-radius:rect.yMin+radius);
    for(int step=0;step<=segments;step++){
     float angle=(corner*90f+step*90f/segments)*Mathf.Deg2Rad;
     mesh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,color,Vector2.zero);
    }
   }
   const int count=4*(segments+1);
   for(int i=0;i<count;i++)mesh.AddTriangle(0,(i+1)%count+1,i+1);
  }
 }
}
