using System;
using System.Collections.Generic;
using UnityEngine;
namespace BeeHarvest {
 /// <summary>Animates Blender wing hinges and four real grip meshes without touching gameplay state.</summary>
 public sealed class BeeGraspPose {
  sealed class Limb {
   public Transform Pivot,Tip,Geometry,Knee;public Quaternion RestRotation,GeometryRotation;
   public Mesh Mesh;public Vector3[] RestVertices,Vertices,Radial;public float[] Along;
   public Vector3 Hip,RestTip,Axis;public bool Left,Front;
  }
  float lastAmount=-1;Vector3 lastCubePosition,lastHalfSize;Quaternion lastCubeRotation;
  readonly Transform body;readonly Limb[] limbs;readonly Transform[] wings;readonly Quaternion[] wingRest;
  public BeeGraspPose(Transform root){
   body=root;var ls=new List<Limb>();var ws=new List<Transform>();
   foreach(var t in body.GetComponentsInChildren<Transform>()){
    if(t.name.EndsWith("Wing_Left")||t.name.EndsWith("Wing_Right"))ws.Add(t);
    if(!t.name.Contains("_Limb_"))continue;
    var filter=t.GetComponentInChildren<MeshFilter>();Transform tip=null;foreach(var child in t.GetComponentsInChildren<Transform>())if(child.name.Contains("_GripTip_"))tip=child;
    if(filter==null||tip==null)throw new InvalidOperationException("Bee grip rig is incomplete: "+t.name);
    var l=new Limb{Pivot=t,Tip=tip,Geometry=filter.transform,RestRotation=t.localRotation,GeometryRotation=filter.transform.localRotation,Mesh=filter.mesh,Left=t.name.Contains("Left"),Front=t.name.Contains("Front")};
    l.Knee=new GameObject("GripKnee").transform;l.Knee.SetParent(t,false);
    l.Hip=body.InverseTransformPoint(t.position);l.RestTip=body.InverseTransformPoint(tip.position);l.Axis=(l.RestTip-l.Hip).normalized;
    l.RestVertices=l.Mesh.vertices;l.Vertices=new Vector3[l.RestVertices.Length];l.Radial=new Vector3[l.RestVertices.Length];l.Along=new float[l.RestVertices.Length];
    for(int i=0;i<l.RestVertices.Length;i++){
     var p=body.InverseTransformPoint(filter.transform.TransformPoint(l.RestVertices[i]));
     l.Along[i]=Mathf.Clamp01(Vector3.Dot(p-l.Hip,l.Axis)/Vector3.Distance(l.Hip,l.RestTip));
     l.Radial[i]=p-Vector3.Lerp(l.Hip,l.RestTip,l.Along[i]);
    }ls.Add(l);
   }
   limbs=ls.ToArray();wings=ws.ToArray();wingRest=new Quaternion[wings.Length];for(int i=0;i<wings.Length;i++)wingRest[i]=wings[i].localRotation;
   if(limbs.Length!=4||wings.Length!=2)throw new InvalidOperationException("Bee requires four limbs and two wing hinges");
  }
  public void Wings(float clock,int id){for(int i=0;i<wings.Length;i++)wings[i].localRotation=wingRest[i]*Quaternion.AngleAxis(Mathf.Sin(clock*48+id)*32*(wings[i].name.EndsWith("Wing_Left")?1:-1),Vector3.up);}
  /// <summary>Keeps foot shape while bending the four limbs around the body toward the block corners.</summary>
  public void Grasp(Pose cube,Vector3 halfSize,float amount){
   amount=Mathf.SmoothStep(0,1,Mathf.Clamp01(amount));
   var relativePosition=body.InverseTransformPoint(cube.position);var relativeRotation=Quaternion.Inverse(body.rotation)*cube.rotation;
   if(Mathf.Abs(lastAmount-amount)<.00001f&&(amount==0||(Vector3.SqrMagnitude(halfSize-lastHalfSize)<.00000001f&&Vector3.SqrMagnitude(relativePosition-lastCubePosition)<.00000001f&&Quaternion.Angle(relativeRotation,lastCubeRotation)<.01f)))return;
   lastAmount=amount;lastHalfSize=halfSize;lastCubePosition=relativePosition;lastCubeRotation=relativeRotation;
   foreach(var l in limbs){
    // Nearest point on the real box: the feet contact its surface before the fixed reach clamp.
    var hipInCube=Quaternion.Inverse(cube.rotation)*(body.TransformPoint(l.Hip)-cube.position);
    var contact=new Vector3(Mathf.Clamp(hipInCube.x,-halfSize.x,halfSize.x),Mathf.Clamp(hipInCube.y,-halfSize.y,halfSize.y),Mathf.Clamp(hipInCube.z,-halfSize.z,halfSize.z));
    var target=body.InverseTransformPoint(cube.position+cube.rotation*contact);
    // Fixed two-segment rig: 1.05 body units total, independent of target distance.
    const float length=1.05f;var offset=target-l.Hip;
    target=l.Hip+Vector3.ClampMagnitude(offset,length*.98f);
    var end=Vector3.Lerp(l.RestTip,target,amount);
    var axis=(end-l.Hip).normalized;var bendDirection=Vector3.ProjectOnPlane(new Vector3(Mathf.Sign(l.Hip.x),-.25f,0),axis).normalized;
    if(bendDirection.sqrMagnitude<.01f)bendDirection=Vector3.Cross(axis,Vector3.forward).normalized;
    float distance=Vector3.Distance(l.Hip,end);
    var knee=(l.Hip+end)*.5f+bendDirection*Mathf.Sqrt(Mathf.Max(0,length*length*.25f-distance*distance*.25f));
    var desired=Quaternion.FromToRotation(l.Axis,(end-l.Hip).normalized)*l.RestRotation;
    l.Pivot.localRotation=Quaternion.Slerp(l.RestRotation,desired,amount);l.Knee.position=body.TransformPoint(knee);
    // A body-relative mesh frame keeps an unchanged held pose cached through world-space turns.
    l.Geometry.localRotation=l.GeometryRotation;
    for(int i=0;i<l.Vertices.Length;i++){
     float t=l.Along[i];var p=t<.5f?Vector3.Lerp(l.Hip,knee,t*2):Vector3.Lerp(knee,end,(t-.5f)*2);
     var tangent=(t<.5f?knee-l.Hip:end-knee).normalized;
     p+=Quaternion.FromToRotation(l.Axis,tangent)*l.Radial[i];
     l.Vertices[i]=l.Geometry.InverseTransformPoint(body.TransformPoint(p));
    }
    l.Mesh.vertices=l.Vertices;l.Mesh.RecalculateBounds();l.Mesh.RecalculateNormals();
    l.Tip.position=body.TransformPoint(end);
   }
  }
  public void Dispose(){foreach(var l in limbs)UnityEngine.Object.Destroy(l.Mesh);}
 }
}
