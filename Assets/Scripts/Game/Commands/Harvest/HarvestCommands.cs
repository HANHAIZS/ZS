using System;
using System.Collections.Generic;
using QFramework;
namespace BeeHarvest {
 public sealed class StartLevelCommand:AbstractCommand {
  public readonly LevelData Data;
  /// <summary>Supplies immutable level input.</summary>
  public StartLevelCommand(LevelData data){Data=data;}
  /// <summary>Validates and resets the first-level session atomically.</summary>
  protected override void OnExecute(){
   if(Data==null||Data.cubes==null||Data.hives==null||Data.cubes.Length==0)throw new ArgumentException("Missing level data");
   var cells=new HashSet<string>();var totals=new Dictionary<string,int>();var quotas=new Dictionary<string,int>();
   for(int i=0;i<Data.cubes.Length;i++){var c=Data.cubes[i];if(c.id!=i||string.IsNullOrEmpty(c.color)||!cells.Add(c.x+","+c.y+","+c.z))throw new ArgumentException("Invalid cube identity/grid");if(!totals.ContainsKey(c.color))totals[c.color]=0;totals[c.color]++;}
   foreach(var h in Data.hives){if(h.quota<=0||h.column<0||h.column>4||string.IsNullOrEmpty(h.color))throw new ArgumentException("Invalid hive");if(!quotas.ContainsKey(h.color))quotas[h.color]=0;quotas[h.color]+=h.quota;}
   if(totals.Count!=quotas.Count)throw new ArgumentException("Color quota mismatch");foreach(var t in totals)if(!quotas.ContainsKey(t.Key)||quotas[t.Key]!=t.Value)throw new ArgumentException("Color quota mismatch");
   int columns=Data.columns==0?5:Data.columns;
   if(columns<1||columns>5)throw new ArgumentException("Invalid queue column count");
   foreach(var h in Data.hives)if(h.column>=columns)throw new ArgumentException("Hive is outside queue columns");
   var groups=LinkedHiveGroups.Build(Data.hives);
   var m=this.GetModel<HarvestModel>();m.ColumnCount=columns;m.HasLinkedGroups=false;foreach(var group in groups)m.HasLinkedGroups|=group.Length>1;m.cubes.Clear();m.hives.Clear();m.flights.Clear();
   foreach(var c in Data.cubes)m.cubes.Add(new CubeState{Data=c});for(int i=0;i<Data.hives.Length;i++){var h=Data.hives[i];m.hives.Add(new HiveState{Data=h,Remaining=h.quota,group=groups[i]});}
   m.slots.Clear();for(int i=0;i<5;i++)m.slots.Add(-1);m.SpeedSeconds=0;m.FreeSelection=false;m.EntryClock=0;Array.Clear(m.DeparturePorts,0,m.DeparturePorts.Length);
   foreach(var q in m.waiting)q.Clear();for(int i=0;i<m.hives.Count;i++)m.waiting[m.hives[i].Data.column].Add(i);m.Deposited=0;m.Remaining=Data.cubes.Length;m.InTransit=0;m.Won=false;m.Deadlocked=false;m.Paused=false;m.ViewDirection=UnityEngine.Vector3.back;m.ViewCameraPosition=new UnityEngine.Vector3(0,0,-1000);m.OrthographicView=true;m.ViewRevision++;m.BoardRevision++;m.Revision++;m.Generation++;
  }
 }
 public sealed class ActivateHiveCommand:AbstractCommand {
  readonly int column,targetSlot;readonly bool automatic;
  /// <summary>Requests the visible hive in one column.</summary>
  public ActivateHiveCommand(int value){column=value;targetSlot=-1;automatic=true;}
  /// <summary>Requests the visible hive in one exact empty destination slot.</summary>
  public ActivateHiveCommand(int value,int slot){column=value;targetSlot=slot;}
  /// <summary>Moves the queue front into the first free slot.</summary>
  protected override void OnExecute(){
   var m=this.GetModel<HarvestModel>();var rules=this.GetSystem<HarvestSystem>();if(column<0||column>4)return;
   int slot=targetSlot;if(automatic){for(int i=0;i<m.Slots.Count;i++)if(m.slots[i]<0){slot=i;break;}}
   this.SendCommand(new ActivateWaitingHiveCommand(rules.Front(column),slot));
  }
 }
 public sealed class PauseHarvestCommand:AbstractCommand {
  readonly bool paused;
  /// <summary>Sets explicit simulation pause state.</summary>
  public PauseHarvestCommand(bool value){paused=value;}
  /// <summary>Pauses without changing task progress or counts.</summary>
  protected override void OnExecute(){var m=this.GetModel<HarvestModel>();m.Paused=paused;m.Revision++;}
 }
 public sealed class AdvanceHarvestCommand:AbstractCommand {
  public const float DepartureInterval=.18f,ApproachSeconds=1.05f,SettleSeconds=.13f,ReachSeconds=.12f,LiftSeconds=.22f,ReturnSeconds=.66f;
  public const float EntrySeconds=.12f,EntryInterval=.02f,EmptyReturnSeconds=0f,EscapeSeconds=.14f;
  public const float OutboundSeconds=ApproachSeconds+SettleSeconds+ReachSeconds,LoadedSeconds=LiftSeconds+ReturnSeconds;
  readonly float dt,clockDelta;
  public AdvanceHarvestCommand(float value):this(value,value){}
  public AdvanceHarvestCommand(float value,float gameplaySeconds){dt=value;clockDelta=gameplaySeconds;}

  static void Consume(ref float progress,float end,ref float budget){float used=Math.Min(budget,Math.Max(0,end-progress));progress+=used;budget-=used;}

  protected override void OnExecute(){
   var m=this.GetModel<HarvestModel>();var rules=this.GetSystem<HarvestSystem>();
   if(m.Paused||m.Deadlocked||float.IsNaN(dt)||float.IsInfinity(dt)||dt<0||float.IsNaN(clockDelta)||float.IsInfinity(clockDelta)||clockDelta<0)return;
   float fraction=clockDelta>0?Math.Min(clockDelta,m.SpeedSeconds)/clockDelta:(m.SpeedBoost?1f:0f);float step=dt*(1+fraction);
   if(m.SpeedSeconds>0){m.SpeedSeconds=Math.Max(0,m.SpeedSeconds-clockDelta);if(m.SpeedSeconds==0)m.Revision++;}
   if(m.Won)return;
   m.EntryClock=Math.Max(0,m.EntryClock-step);
   for(int port=0;port<m.DeparturePorts.Length;port++)m.DeparturePorts[port]=Math.Max(0,m.DeparturePorts[port]-step);
   int existing=m.flights.Count;
   for(int i=0;i<existing;i++){
    var f=m.flights[i];if(f.Stage==2)continue;f.Age+=step;float budget=step;
    if(f.Stage==0){
     float gate=ApproachSeconds*.65f;
     Consume(ref f.Progress,gate,ref budget);
     if(f.Progress<gate-.00001f||(m.RequireFlightArrival&&!f.ClearedApproach))continue;
     if(!f.CollectionGranted){if(!rules.CollectionFree(f))continue;f.CollectionGranted=true;}
     Consume(ref f.Progress,ApproachSeconds,ref budget);
     if(f.Progress<ApproachSeconds-.00001f||(m.RequireFlightArrival&&!f.Reached))continue;
     Consume(ref f.Progress,OutboundSeconds,ref budget);
     if(f.Progress<OutboundSeconds-.00001f)continue;
     var c=m.cubes[f.Cube];var h=m.hives[f.Hive];
     c.Stage=2;h.Reserved--;m.Remaining--;m.InTransit++;m.BoardRevision++;
     f.Stage=1;f.Progress=0;f.TransportPhase=0;f.PhaseProgress=0;f.Reached=!m.RequireFlightArrival;
     m.Revision++;this.SendEvent(new HarvestChangedEvent{Task=f.Id,Picked=true});
    }
    f.Progress+=budget;
    for(int transition=0;transition<8&&f.Stage==1;transition++){
     // Local extraction never waits for unrelated traffic once the voxel is in the bee's grip.
     if(f.TransportPhase==1){f.TransportPhase=2;f.PhaseProgress=0;f.Reached=!m.RequireFlightArrival;}
     if(f.TransportPhase==3){
      if(!rules.EntryFree(f))break;
      f.TransportPhase=4;f.PhaseProgress=0;f.Reached=!m.RequireFlightArrival;m.EntryClock=EntryInterval;
     }
     float duration=f.TransportPhase==0?LiftSeconds:f.TransportPhase==2?ReturnSeconds-EntrySeconds:EntrySeconds;
     if(f.TransportPhase==2&&m.RequireFlightArrival&&!f.ClearedSource)duration=EscapeSeconds;
     Consume(ref f.PhaseProgress,duration,ref budget);
     if(!m.RequireFlightArrival&&f.TransportPhase==2&&f.PhaseProgress>=EscapeSeconds)f.ClearedSource=true;
     if(f.PhaseProgress<duration-.00001f)break;
     if(f.TransportPhase==2&&m.RequireFlightArrival&&!f.ClearedSource)break;
     if(f.TransportPhase>0&&m.RequireFlightArrival&&!f.Reached)break;
     if(f.TransportPhase==0){f.TransportPhase=1;f.PhaseProgress=0;continue;}
     if(f.TransportPhase==2){f.ClearedSource=true;f.TransportPhase=3;f.PhaseProgress=0;continue;}
     // The carrier and cargo end at the collector together; there is no visible empty return.
     f.Stage=2;f.Returned=true;f.ClearedEntry=true;f.ReturnCornerReached=true;
     m.InTransit--;m.Deposited++;m.hives[f.Hive].Busy--;m.Revision++;
     this.SendEvent(new HarvestChangedEvent{Task=f.Id,Picked=false});break;
    }
   }
   for(int s=0;s<m.Slots.Count;s++){
    int hi=m.slots[s];if(hi<0)continue;var h=m.hives[hi];
    h.DepartureCooldown=Math.Max(0,h.DepartureCooldown-step);
    if(h.Remaining<=0||h.DepartureCooldown>.00001f||!rules.DepartureFree(hi))continue;
    int ci=rules.FindDispatchTarget(h.Data.color);if(ci<0)continue;
    var f=new FlightState{Id=m.flights.Count,Cube=ci,Hive=hi,Slot=s,PickupNormal=rules.PickupNormal(ci),Lane=rules.ApproachLane(ci),Reached=!m.RequireFlightArrival};
    m.flights.Add(f);m.cubes[ci].Stage=1;m.cubes[ci].Task=f.Id;
    h.Remaining--;h.Reserved++;h.Busy++;h.DepartureCooldown=DepartureInterval;m.DeparturePorts[s]=DepartureInterval;
    if(h.Remaining==0){m.slots[s]=-1;h.Slot=-1;}m.Revision++;
   }
   if(m.Deposited==m.Cubes.Count){m.Won=true;m.Revision++;return;}
   bool busy=false,free=false,front=false;foreach(var f in m.flights)if(f.Stage<2){busy=true;break;}
   for(int i=0;i<m.Slots.Count;i++)free|=m.slots[i]<0;for(int c=0;c<5;c++)front|=rules.Front(c)>=0;
   bool canRotateToWork=false;foreach(var h in m.hives)if(h.Started&&h.Remaining>0&&rules.HasRemainingColor(h.Data.color)){canRotateToWork=true;break;}
   if(!busy&&!canRotateToWork&&!(free&&front)){m.Deadlocked=true;m.Revision++;}
  }
 }
}
