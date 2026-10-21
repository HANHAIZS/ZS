using QFramework;
namespace BeeHarvest {
 // Scene presentation opts into physical arrival; non-presented simulations remain deterministic.
 public sealed class AttachBeeFlightPresentationCommand:AbstractCommand {
  readonly int owner;
  public AttachBeeFlightPresentationCommand(int value){owner=value;}
  protected override void OnExecute(){var m=this.GetModel<HarvestModel>();m.FlightPresentationOwner=owner;m.RequireFlightArrival=true;foreach(var f in m.Flights){if(f.Stage==2){f.Returned=true;continue;}f.Reached=false;if(f.Stage==0&&f.Progress>AdvanceHarvestCommand.ApproachSeconds*.65f)f.ClearedApproach=true;if(f.Stage==1&&f.TransportPhase==2&&f.PhaseProgress>AdvanceHarvestCommand.EscapeSeconds)f.ClearedSource=true;}}
 }
 public sealed class DetachBeeFlightPresentationCommand:AbstractCommand {
  readonly int owner;
  public DetachBeeFlightPresentationCommand(int value){owner=value;}
  protected override void OnExecute(){var m=this.GetModel<HarvestModel>();if(m.FlightPresentationOwner!=owner)return;m.FlightPresentationOwner=0;m.RequireFlightArrival=false;foreach(var f in m.Flights)if(f.Stage<2)f.Reached=true;}
 }
 public sealed class ConfirmBeeFlightArrivalCommand:AbstractCommand {
  readonly int generation,task,stage,phase;readonly bool arrived,cleared,approached;
  public ConfirmBeeFlightArrivalCommand(int g,int id,int s,int p,bool ready,bool outside=false,bool approach=false){generation=g;task=id;stage=s;phase=p;arrived=ready;cleared=outside;approached=approach;}
  protected override void OnExecute(){var m=this.GetModel<HarvestModel>();if(m.Generation!=generation||task<0||task>=m.Flights.Count)return;var f=m.flights[task];if(f.Stage!=stage||f.TransportPhase!=phase)return;f.Reached=arrived;f.ClearedSource=cleared;f.ClearedApproach=approached;f.ArrivalView=m.ViewDirection;}
 }
 public sealed class ConfirmBeeReturnedCommand:AbstractCommand {
  readonly int generation,task;readonly bool entry,corner,returned;
  public ConfirmBeeReturnedCommand(int g,int id,bool done=true,bool exited=true,bool turned=true){generation=g;task=id;returned=done;entry=exited;corner=turned;}
  protected override void OnExecute(){var m=this.GetModel<HarvestModel>();if(m.Generation==generation&&task>=0&&task<m.Flights.Count&&m.flights[task].Stage==2){var f=m.flights[task];f.Returned|=returned;f.ClearedEntry|=entry;f.ReturnCornerReached|=corner;}}
 }

}
