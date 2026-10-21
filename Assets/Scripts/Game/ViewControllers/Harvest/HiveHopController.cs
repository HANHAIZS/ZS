using QFramework;
using UnityEngine;
namespace BeeHarvest {
 public sealed class HiveHopController : BeeHarvestController {
  public const float TravelSeconds=.24f,LandingSeconds=.10f;
  HarvestModel model;HarvestSystem rules;HiveModelView view;LevelOneHud hud;
  int hive=-1,generation;int[] members,columns,slots;float elapsed;bool modalInputBlocked;
  public void SetModalInputBlocked(bool value){modalInputBlocked=value;if(value)Cancel();}
  public void Initialize(HarvestModel state,HarvestSystem system,HiveModelView models,LevelOneHud controls){model=state;rules=system;view=models;hud=controls;}
  bool Available(){return isActiveAndEnabled&&!modalInputBlocked&&model!=null&&!model.Paused&&!model.Won&&!model.Deadlocked;}
  public void Click(int value){if(rules!=null)ClickWaiting(rules.Front(value));}
  public void ClickWaiting(int value){
   if(hive>=0||!Available()||value<0||value>=model.Hives.Count)return;
   int[] plannedMembers,plannedSlots;if(!rules.TryPlanActivation(value,-1,out plannedMembers,out plannedSlots)){view.BeginWaitingBounce(value);return;}
   view.CancelWaitingBounces();
   hive=value;members=plannedMembers;slots=plannedSlots;columns=new int[members.Length];for(int i=0;i<members.Length;i++)columns[i]=rules.WaitingColumnOf(members[i]);
   generation=model.Generation;elapsed=0;view.BeginHops(members,columns,slots);hud.SetHiveInputEnabled(false);
  }
  public void Tick(float delta){
   if(!Available()){Cancel();return;}
   view.TickWaitingBounces(delta);
   if(hive<0)return;
   bool valid=Available()&&generation==model.Generation&&rules.CanSelectHive(hive);
   for(int i=0;i<members.Length&&valid;i++)valid=rules.WaitingColumnOf(members[i])==columns[i]&&model.Slots[slots[i]]<0;
   if(!valid){Cancel();return;}
   elapsed+=delta;if(elapsed<TravelSeconds+LandingSeconds){view.ShowHop(elapsed);return;}
   int source=hive,destination=slots[0];var destinations=slots;Cancel();this.SendCommand(new ActivateWaitingHiveCommand(source,destination,destinations));view.Sync();hud.Sync();
  }
  public void Cancel(){if(view!=null)view.CancelWaitingBounces();if(hive<0)return;hive=-1;members=columns=slots=null;view.EndHop();hud.SetHiveInputEnabled(true);}
  void OnDisable(){Cancel();}
  void OnApplicationPause(bool value){if(value)Cancel();}
 }
}
