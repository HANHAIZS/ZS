using QFramework;
using UnityEngine;
namespace BeeHarvest {
 public sealed class UpdateHarvestViewCommand:AbstractCommand {
  readonly Vector3 direction,position;readonly bool orthographic;
  public UpdateHarvestViewCommand(Vector3 towardCamera,Vector3 cameraPosition,bool isOrthographic=true){direction=towardCamera.normalized;position=cameraPosition;orthographic=isOrthographic;}
  protected override void OnExecute(){
   var m=this.GetModel<HarvestModel>();if(direction.sqrMagnitude<.5f)return;
   if(m.OrthographicView==orthographic&&(m.ViewDirection-direction).sqrMagnitude<.00000001f&&(orthographic||(m.ViewCameraPosition-position).sqrMagnitude<.000001f))return;
   m.ViewDirection=direction;m.ViewCameraPosition=position;m.OrthographicView=orthographic;m.ViewRevision++;m.Deadlocked=false;
   if(m.RequireFlightArrival)foreach(var f in m.Flights)if(f.Stage==0&&f.Reached&&Vector3.Angle(f.ArrivalView,direction)>.8f)f.Reached=false;
  }
 }
}
