using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest {
 /// <summary>Suppresses baked duplicates beneath the three overlaid Settings buttons only during feedback.</summary>
 [DisallowMultipleComponent]
 public sealed class ButtonPressBackdrop : BaseMeshEffect {
  sealed class Region {public ButtonPressMotion Owner;public Rect Rect;}
  readonly List<Region> regions=new List<Region>(3);
  readonly float[] xs=new float[8],ys=new float[8];
  bool valid;float clearX;
  public static ButtonPressBackdrop Register(Image overlay,ButtonPressMotion owner) {
   if(overlay.sprite==null)return null;
   Rect crop=overlay.sprite.rect;
   for(var node=owner.transform.parent;node!=null;node=node.parent) {
    var parent=node.GetComponent<Image>();
    if(parent==null||parent.sprite==null||parent.type!=Image.Type.Simple||parent.sprite.texture!=overlay.sprite.texture)continue;
    Rect whole=parent.sprite.rect;
    if(crop.xMin<whole.xMin||crop.yMin<whole.yMin||crop.xMax>whole.xMax||crop.yMax>whole.yMax||crop.width>=whole.width*.4f||crop.height>=whole.height*.8f)continue;
    var backdrop=parent.GetComponent<ButtonPressBackdrop>();if(backdrop==null)backdrop=parent.gameObject.AddComponent<ButtonPressBackdrop>();
    backdrop.Add(owner,new Rect((crop.x-whole.x)/whole.width,(crop.y-whole.y)/whole.height,crop.width/whole.width,crop.height/whole.height));return backdrop;
   }
   return null;
  }
  void Add(ButtonPressMotion owner,Rect rect) {
   foreach(var r in regions)if(r.Owner==owner)return;
   regions.Add(new Region{Owner=owner,Rect=rect});regions.Sort((a,b)=>a.Rect.x.CompareTo(b.Rect.x));
   // A contained atlas crop alone is insufficient. Require the three separated, aligned Settings faces.
   valid=regions.Count==3;float largest=0;
   for(int i=0;i<regions.Count&&valid;i++) {
    if(Mathf.Abs(regions[i].Rect.y-regions[0].Rect.y)>.005f||Mathf.Abs(regions[i].Rect.height-regions[0].Rect.height)>.005f){valid=false;break;}
    if(i==0)continue;float gap=regions[i].Rect.xMin-regions[i-1].Rect.xMax;
    if(gap<=.01f){valid=false;break;}
    if(gap>largest){largest=gap;clearX=(regions[i].Rect.xMin+regions[i-1].Rect.xMax)*.5f;}
   }
   Redraw();
  }
  public void Redraw(){if(graphic!=null)graphic.SetVerticesDirty();}
  bool Pressed(Region region)=>region.Owner!=null&&region.Owner.VisualScale!=1;
  public override void ModifyMesh(VertexHelper mesh) {
   if(!IsActive()||!valid||mesh.currentVertCount!=4)return;
   int nx=2,ny=2;xs[0]=ys[0]=0;xs[1]=ys[1]=1;
   foreach(var r in regions)if(Pressed(r)){xs[nx++]=r.Rect.xMin;xs[nx++]=r.Rect.xMax;ys[ny++]=r.Rect.yMin;ys[ny++]=r.Rect.yMax;}
   if(nx==2)return;
   var first=new UIVertex();mesh.PopulateUIVertex(ref first,0);float left=first.position.x,right=left,bottom=first.position.y,top=bottom;
   float u0=first.uv0.x,u1=u0,v0=first.uv0.y,v1=v0;
   for(int i=1;i<4;i++){var v=new UIVertex();mesh.PopulateUIVertex(ref v,i);left=Mathf.Min(left,v.position.x);right=Mathf.Max(right,v.position.x);bottom=Mathf.Min(bottom,v.position.y);top=Mathf.Max(top,v.position.y);u0=Mathf.Min(u0,v.uv0.x);u1=Mathf.Max(u1,v.uv0.x);v0=Mathf.Min(v0,v.uv0.y);v1=Mathf.Max(v1,v.uv0.y);}
   Array.Sort(xs,0,nx);Array.Sort(ys,0,ny);mesh.Clear();
   for(int x=0;x<nx-1;x++)for(int y=0;y<ny-1;y++) {
    float a=xs[x],b=xs[x+1],c=ys[y],d=ys[y+1];if(b-a<.00001f||d-c<.00001f)continue;
    bool fill=false;var midpoint=new Vector2((a+b)*.5f,(c+d)*.5f);
    foreach(var r in regions)if(Pressed(r)&&r.Rect.Contains(midpoint)){fill=true;break;}
    int start=mesh.currentVertCount;
    AddVertex(mesh,first,a,c,fill?clearX:a,left,right,bottom,top,u0,u1,v0,v1);
    AddVertex(mesh,first,a,d,fill?clearX:a,left,right,bottom,top,u0,u1,v0,v1);
    AddVertex(mesh,first,b,d,fill?clearX:b,left,right,bottom,top,u0,u1,v0,v1);
    AddVertex(mesh,first,b,c,fill?clearX:b,left,right,bottom,top,u0,u1,v0,v1);
    mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
   }
  }
  static void AddVertex(VertexHelper mesh,UIVertex vertex,float x,float y,float u,float left,float right,float bottom,float top,float u0,float u1,float v0,float v1) {
   vertex.position=new Vector3(Mathf.Lerp(left,right,x),Mathf.Lerp(bottom,top,y),vertex.position.z);
   vertex.uv0=new Vector4(Mathf.Lerp(u0,u1,u),Mathf.Lerp(v0,v1,y),vertex.uv0.z,vertex.uv0.w);mesh.AddVert(vertex);
  }
 }
}
