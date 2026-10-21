using System;
using System.Collections.Generic;
using QFramework;
namespace BeeHarvest {
 /// <summary>Only a zero inventory can receive one replacement; repeated stale taps cannot stack grants.</summary>
 public sealed class RefillHarvestBoosterCommand:AbstractCommand {
  readonly int index;
  public RefillHarvestBoosterCommand(int booster){index=booster;}
  protected override void OnExecute(){var m=this.GetModel<HarvestModel>();var rules=this.GetSystem<HarvestSystem>();if(index<0||index>=4||!rules.CanUseBoosters||m.boosterCounts[index]!=0)return;m.boosterCounts[index]=1;m.Revision++;}
 }
 public sealed class AddHiveSlotCommand:AbstractCommand {
  protected override void OnExecute(){var m=this.GetModel<HarvestModel>();var rules=this.GetSystem<HarvestSystem>();if(!rules.CanUseBoosters||m.slots.Count>=6||m.BoosterCount(0)==0)return;m.slots.Add(-1);m.boosterCounts[0]--;m.Revision++;}
 }
 public sealed class UseHarvestSpeedCommand:AbstractCommand {
  public const float DurationSeconds=180;
  protected override void OnExecute(){var m=this.GetModel<HarvestModel>();var rules=this.GetSystem<HarvestSystem>();if(!rules.CanUseBoosters||m.BoosterCount(1)==0)return;m.boosterCounts[1]--;m.SpeedSeconds=DurationSeconds;m.Revision++;}
 }
 /// <summary>Spends one item for the next successful waiting-hive selection; never toggles an existing choice.</summary>
 public sealed class UseFreeHiveSelectionCommand:AbstractCommand {
  protected override void OnExecute(){var m=this.GetModel<HarvestModel>();var rules=this.GetSystem<HarvestSystem>();if(!rules.CanUseBoosters||m.FreeSelection||m.BoosterCount(3)==0)return;m.boosterCounts[3]--;m.FreeSelection=true;m.Revision++;}
 }
 public sealed class ShuffleWaitingHivesCommand:AbstractCommand {
  readonly int seed;
  public ShuffleWaitingHivesCommand():this(Environment.TickCount){}
  public ShuffleWaitingHivesCommand(int value){seed=value;}
  protected override void OnExecute(){
   var m=this.GetModel<HarvestModel>();var rules=this.GetSystem<HarvestSystem>();if(!rules.CanUseBoosters||m.BoosterCount(2)==0)return;
   var ids=new List<int>();foreach(var q in m.waiting)ids.AddRange(q);if(ids.Count<2)return;
   if(m.HasLinkedGroups){
    var order=new List<int[]>();var seen=new HashSet<int>();foreach(int id in ids)if(seen.Add(id)){var group=m.hives[id].group;order.Add(group);foreach(int member in group)seen.Add(member);}
    var randomGroups=new Random(seed);for(int i=order.Count-1;i>0;i--){int j=randomGroups.Next(i+1);var group=order[i];order[i]=order[j];order[j]=group;}
    foreach(var q in m.waiting)q.Clear();foreach(var group in order)foreach(int id in group)m.waiting[m.hives[id].Data.column].Add(id);
    m.boosterCounts[2]--;m.Revision++;return;
   }
   var before=ids.ToArray();var random=new Random(seed);
   for(int i=ids.Count-1;i>0;i--){int j=random.Next(i+1);int v=ids[i];ids[i]=ids[j];ids[j]=v;}
   bool same=true;for(int i=0;i<ids.Count;i++)same&=ids[i]==before[i];
   if(same){int v=ids[0];ids.RemoveAt(0);ids.Add(v);}
   int at=0;foreach(var q in m.waiting)for(int i=0;i<q.Count;i++)q[i]=ids[at++];m.boosterCounts[2]--;m.Revision++;
  }
 }
 public sealed class ActivateWaitingHiveCommand:AbstractCommand {
  readonly int hive,slot;readonly int[] plannedSlots;
  public ActivateWaitingHiveCommand(int hiveId,int destination){hive=hiveId;slot=destination;}
  public ActivateWaitingHiveCommand(int hiveId,int destination,int[] destinations):this(hiveId,destination){plannedSlots=destinations==null?null:(int[])destinations.Clone();}
  protected override void OnExecute(){
   var m=this.GetModel<HarvestModel>();var rules=this.GetSystem<HarvestSystem>();
   int[] members,slots;if(slot<0||!rules.TryPlanActivation(hive,slot,out members,out slots))return;
   if(plannedSlots!=null){
    if(plannedSlots.Length!=members.Length||plannedSlots[0]!=slot)return;
    var used=new HashSet<int>();foreach(int target in plannedSlots)if(target<0||target>=m.Slots.Count||m.Slots[target]>=0||!used.Add(target))return;
    slots=plannedSlots;
   }
   for(int i=0;i<members.Length;i++){int id=members[i],column=rules.WaitingColumnOf(id);m.waiting[column].Remove(id);m.slots[slots[i]]=id;m.hives[id].Slot=slots[i];m.hives[id].Started=true;}
   // Commit consumes the single choice; invalid destinations and cancelled hops never reach this point.
   m.FreeSelection=false;m.Revision++;this.SendEvent(new HiveEnteredSlotEvent{Generation=m.Generation,Hive=hive});this.SendCommand(new AdvanceHarvestCommand(0));
  }
 }
}
