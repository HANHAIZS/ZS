using System.Collections.Generic;
using UnityEngine;
using QFramework;
namespace BeeHarvest {
 public sealed class HarvestSystem : AbstractSystem {
  HarvestModel model;int boardRevision=-1,viewRevision=-1;
  readonly HashSet<Vector3Int> occupied=new HashSet<Vector3Int>();
  bool[] visible=new bool[0];Vector3[] pickupNormals=new Vector3[0]; Vector3Int min,max;
  static readonly Vector3[] faces={Vector3.left,Vector3.right,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
  // Center/interior probes handle broad exposure first; edge probes catch oblique visible face patches.
  static readonly Vector2[] probes={Vector2.zero,new Vector2(-.32f,-.32f),new Vector2(.32f,-.32f),new Vector2(-.32f,.32f),new Vector2(.32f,.32f),
   new Vector2(-.44f,-.44f),new Vector2(.44f,-.44f),new Vector2(-.44f,.44f),new Vector2(.44f,.44f),
   new Vector2(-.44f,0),new Vector2(.44f,0),new Vector2(0,-.44f),new Vector2(0,.44f)};
  protected override void OnInit(){model=this.GetModel<HarvestModel>();}
  public int Front(int column)=>WaitingAt(column,0);
  public int Next(int column)=>WaitingAt(column,1);
  public int WaitingAt(int column,int depth){if(column<0||column>=5||depth<0)return -1;var q=model.WaitingColumn(column);return depth<q.Count?q[depth]:-1;}
  public int WaitingColumnOf(int hive){for(int c=0;c<5;c++)for(int i=0;i<model.waiting[c].Count;i++)if(model.waiting[c][i]==hive)return c;return -1;}
  public bool CanSelectHive(int hive){
   int column=WaitingColumnOf(hive);if(column<0)return false;var members=model.Hives[hive].Group;
   if(members.Count==1)return model.FreeSelection||Front(column)==hive;
   int free=0;foreach(int slot in model.Slots)if(slot<0)free++;if(free<members.Count)return false;
   foreach(int member in members){int c=WaitingColumnOf(member);if(c<0||Front(c)!=member)return false;}
   return true;
  }
  public bool TryPlanActivation(int hive,int destination,out int[] members,out int[] slots){
   members=null;slots=null;if(!CanUseBoosters||!CanSelectHive(hive))return false;
   if(destination<0){for(int i=0;i<model.Slots.Count;i++)if(model.Slots[i]<0){destination=i;break;}}
   if(destination<0||destination>=model.Slots.Count||model.Slots[destination]>=0)return false;
   var group=model.Hives[hive].Group;int free=0;foreach(int s in model.Slots)if(s<0)free++;if(free<group.Count)return false;
   members=new int[group.Count];slots=new int[group.Count];members[0]=hive;slots[0]=destination;int at=1;
   foreach(int member in group)if(member!=hive)members[at++]=member;
   at=1;for(int s=0;s<model.Slots.Count&&at<slots.Length;s++)if(s!=destination&&model.Slots[s]<0)slots[at++]=s;
   return true;
  }
  public bool CanUseBoosters=>!model.Paused&&!model.Won&&!model.Deadlocked&&model.Cubes.Count>0;
  /// <summary>Matches color only against faces visible from the current camera, including reserved occluders.</summary>
  public int FindTarget(string color){RefreshVisibility();for(int i=0;i<model.Cubes.Count;i++)if(model.Cubes[i].Stage==0&&model.Cubes[i].Data.color==color&&visible[i])return i;return -1;}
  // Prefer an unoccupied neighborhood, but reserve every available color even if it must wait locally.
  public int FindDispatchTarget(string color){
   RefreshVisibility();int fallback=-1;
   for(int i=0;i<model.Cubes.Count;i++){
    if(model.Cubes[i].Stage!=0||model.Cubes[i].Data.color!=color||!visible[i])continue;
    if(fallback<0)fallback=i;bool near=false;
    foreach(var f in model.Flights)if(f.Stage==0&&Neighbors(i,f.Cube)){near=true;break;}
    if(!near)return i;
   }return fallback;
  }
  public Vector3 PickupNormal(int id){RefreshVisibility();return id>=0&&id<pickupNormals.Length?pickupNormals[id]:Vector3.back;}
  public int ApproachLane(int id){RefreshVisibility();var d=model.Cubes[id].Data;var right=Vector3.Cross(model.ViewDirection,Vector3.up).normalized;if(right.sqrMagnitude<.1f)right=Vector3.right;return Vector3.Dot((Vector3)new Vector3Int(d.x,d.y,d.z)-(Vector3)(min+max)*.5f,right)<0?0:1;}
  bool Neighbors(int a,int b){var x=model.Cubes[a].Data;var y=model.Cubes[b].Data;var delta=new Vector3(x.x-y.x,x.y-y.y,x.z-y.z);delta-=model.ViewDirection*Vector3.Dot(delta,model.ViewDirection);return delta.sqrMagnitude<2.25f;}
  // A permit covers only the final approach, grasp and short source clearance, never a half-screen lane.
  public bool CollectionFree(FlightState task){
   foreach(var f in model.Flights){
    if(f==task||!f.CollectionGranted||f.Stage==2)continue;
    bool atSource=f.Stage==0||(f.Stage==1&&(f.TransportPhase<2||(f.TransportPhase==2&&!f.ClearedSource)));
    if(atSource&&Neighbors(task.Cube,f.Cube))return false;
   }return true;
  }
  public bool DepartureFree(int hive){int slot=model.Hives[hive].Slot;return slot<0||model.DeparturePorts[slot]<=.00001f;}
  public bool LoadedLaneFree(FlightState task)=>true;
  public bool EntryFree(FlightState task)=>model.EntryClock<=.00001f;
  public bool IsCameraVisible(int id){RefreshVisibility();return id>=0&&id<visible.Length&&visible[id];}
  public bool HasRemainingColor(string color){foreach(var c in model.Cubes)if(c.Stage<2&&c.Data.color==color)return true;return false;}
  void RefreshVisibility(){
   if(boardRevision==model.BoardRevision&&viewRevision==model.ViewRevision)return;
   boardRevision=model.BoardRevision;viewRevision=model.ViewRevision;occupied.Clear();
   if(visible.Length!=model.Cubes.Count){visible=new bool[model.Cubes.Count];pickupNormals=new Vector3[model.Cubes.Count];}
   min=new Vector3Int(int.MaxValue,int.MaxValue,int.MaxValue);max=new Vector3Int(int.MinValue,int.MinValue,int.MinValue);
   foreach(var c in model.Cubes){var p=Cell(c);min=Vector3Int.Min(min,p);max=Vector3Int.Max(max,p);if(c.Stage<2)occupied.Add(p);}
   min-=Vector3Int.one;max+=Vector3Int.one;
   for(int i=0;i<visible.Length;i++){pickupNormals[i]=Vector3.zero;visible[i]=model.Cubes[i].Stage<2&&Visible(Cell(model.Cubes[i]),out pickupNormals[i]);}
  }
  static Vector3Int Cell(CubeState c){return new Vector3Int(c.Data.x,c.Data.y,c.Data.z);}
  bool Visible(Vector3Int cell,out Vector3 exposed){
   exposed=Vector3.zero;
   Vector3 toward=model.OrthographicView?model.ViewDirection:(model.ViewCameraPosition-(Vector3)cell).normalized;
   for(int f=0;f<faces.Length;f++){
    var normal=faces[f];if(Vector3.Dot(normal,toward)<=.0001f)continue;
    // An occupied adjacent cell covers this entire voxel face; bevel seams are decoration.
    if(occupied.Contains(cell+Vector3Int.RoundToInt(normal)))continue;
    var u=f<2?Vector3.up:Vector3.right;var v=f<4?Vector3.forward:Vector3.up;
    for(int p=0;p<probes.Length;p++){
     var point=(Vector3)cell+normal*.501f+u*probes[p].x+v*probes[p].y;
     var direction=model.OrthographicView?toward:(model.ViewCameraPosition-point).normalized;
     if(Unblocked(point,direction,cell)){exposed=normal;return true;}
    }
   }return false;
  }
  /// <summary>Traverses only crossed grid cells; small bevel gaps never count as an exposed face.</summary>
  bool Unblocked(Vector3 origin,Vector3 direction,Vector3Int source){
   var cell=new Vector3Int(Mathf.FloorToInt(origin.x+.5f),Mathf.FloorToInt(origin.y+.5f),Mathf.FloorToInt(origin.z+.5f));
   var step=new Vector3Int(direction.x>=0?1:-1,direction.y>=0?1:-1,direction.z>=0?1:-1);
   var delta=new Vector3(Mathf.Abs(direction.x)<.000001f?float.PositiveInfinity:Mathf.Abs(1/direction.x),Mathf.Abs(direction.y)<.000001f?float.PositiveInfinity:Mathf.Abs(1/direction.y),Mathf.Abs(direction.z)<.000001f?float.PositiveInfinity:Mathf.Abs(1/direction.z));
   var next=new Vector3(delta.x==float.PositiveInfinity?delta.x:(cell.x+step.x*.5f-origin.x)/direction.x,delta.y==float.PositiveInfinity?delta.y:(cell.y+step.y*.5f-origin.y)/direction.y,delta.z==float.PositiveInfinity?delta.z:(cell.z+step.z*.5f-origin.z)/direction.z);
   var ray=new Ray(origin,direction);
   int limit=(max.x-min.x)+(max.y-min.y)+(max.z-min.z)+6;
   for(int n=0;n<limit;n++){
    if(cell.x<min.x||cell.x>max.x||cell.y<min.y||cell.y>max.y||cell.z<min.z||cell.z>max.z)return true;
    if(cell!=source&&occupied.Contains(cell)){
     float distance;if(new Bounds(cell,Vector3.one*.975f).IntersectRay(ray,out distance)&&distance>=0)return false;
    }
    if(next.x<=next.y&&next.x<=next.z){cell.x+=step.x;next.x+=delta.x;}
    else if(next.y<=next.z){cell.y+=step.y;next.y+=delta.y;}
    else{cell.z+=step.z;next.z+=delta.z;}
   }return true;
  }
 }
}
