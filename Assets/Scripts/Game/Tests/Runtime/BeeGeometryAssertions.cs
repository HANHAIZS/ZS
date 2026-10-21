using UnityEngine;
namespace BeeHarvest.Tests {
 // Oriented mesh bounds distinguish real overlap from empty corners of a rotated world AABB.
 public static class BeeGeometryAssertions {
  public static bool Intersects(Transform a,Transform b){foreach(var x in a.GetComponentsInChildren<Renderer>())foreach(var y in b.GetComponentsInChildren<Renderer>())if(x.enabled&&y.enabled&&x.gameObject.activeInHierarchy&&y.gameObject.activeInHierarchy&&Intersects(x,y))return true;return false;}
  public static bool Intersects(Renderer a,Renderer b){
   if(!a.bounds.Intersects(b.bounds))return false;
   var am=a.GetComponent<MeshFilter>();var bm=b.GetComponent<MeshFilter>();if(am==null||bm==null)return true;
   var x=am.sharedMesh.bounds;var y=bm.sharedMesh.bounds;var at=a.transform;var bt=b.transform;
   var ax=new[]{at.TransformVector(Vector3.right),at.TransformVector(Vector3.up),at.TransformVector(Vector3.forward)};var bx=new[]{bt.TransformVector(Vector3.right),bt.TransformVector(Vector3.up),bt.TransformVector(Vector3.forward)};
   var ae=x.extents;var be=y.extents;var delta=bt.TransformPoint(y.center)-at.TransformPoint(x.center);
   foreach(var axis in ax)if(Separated(axis,delta,ax,bx,ae,be))return false;
   foreach(var axis in bx)if(Separated(axis,delta,ax,bx,ae,be))return false;
   foreach(var u in ax)foreach(var v in bx)if(Separated(Vector3.Cross(u,v),delta,ax,bx,ae,be))return false;return true;
  }
  static bool Separated(Vector3 axis,Vector3 delta,Vector3[] a,Vector3[] b,Vector3 x,Vector3 y){if(axis.sqrMagnitude<1e-12f)return false;axis.Normalize();float radius=0;for(int i=0;i<3;i++)radius+=Mathf.Abs(Vector3.Dot(axis,a[i]))*x[i]+Mathf.Abs(Vector3.Dot(axis,b[i]))*y[i];return Mathf.Abs(Vector3.Dot(delta,axis))>=radius-1e-5f;}
 }
}
