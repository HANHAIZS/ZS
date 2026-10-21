using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class HarvestBoosterTests : EconomyTestFixture {
  static void Command(string name, params object[] args){
   var type=typeof(HarvestModel).Assembly.GetType("BeeHarvest."+name);
   Assert.IsNotNull(type,"Missing gameplay command: "+name);
   BeeHarvestApp.Interface.SendCommand((ICommand)Activator.CreateInstance(type,args));
  }
  static HarvestModel Start(){
   var cubes=Enumerable.Range(0,40).Select(i=>new CubeData{id=i,x=i,color="pink"}).ToArray();
   BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(new LevelData{cubes=cubes,hives=Enumerable.Range(0,10).Select(i=>new HiveData{column=i/2,quota=4,color="pink"}).ToArray()}));
   var model=BeeHarvestApp.Interface.GetModel<HarvestModel>();for(int i=0;i<4;i++)if(model.BoosterCount(i)==0)Command("RefillHarvestBoosterCommand",i);return model;
  }
  [Test] public void ExtraSlotCanWorkBeyondFiveFullSlotsAndRetryRemovesIt(){
   var m=Start();for(int c=0;c<5;c++)BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(c));
   Assert.IsTrue(m.Slots.All(s=>s>=0));Command("AddHiveSlotCommand");
   Assert.AreEqual(6,m.Slots.Count);Command("AddHiveSlotCommand");Assert.AreEqual(6,m.Slots.Count);
   BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Assert.AreEqual(1,m.Slots[5]);
   Assert.AreEqual(6,m.Flights.Count,"All six slotted hives dispatch independently");for(int i=0;i<1500&&m.Hives[1].Busy==0;i++)BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(.1f));Assert.Greater(m.Hives[1].Busy,0);Assert.Less(m.Hives[1].Remaining,4);
   Start();Assert.AreEqual(5,m.Slots.Count);Assert.IsTrue(m.Slots.All(s=>s<0));
  }
  [Test] public void SpeedHalvesDepartureAndPickupTimeWithoutBurstingOrSkippingReach(){
   var m=Start();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Command("UseHarvestSpeedCommand");
   BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(.15f));Assert.AreEqual(2,m.Flights.Count,"Speed scales the independent per-hive cadence without bursting");
   Assert.AreEqual(.3f,m.Flights[0].Progress,.0001f);Assert.AreEqual(0,m.Flights[0].Stage);
   BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(.2f));Assert.AreEqual(.7f,m.Flights[0].Progress,.0001f);
   Assert.AreEqual(0,m.Flights[0].Stage,"Reach pause remains before pickup");
   BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand((AdvanceHarvestCommand.OutboundSeconds-.7f)*.5f+.001f));Assert.AreEqual(1,m.Flights[0].Stage);
   Assert.AreEqual(1,m.InTransit);Assert.AreEqual(0,m.Deposited);
   BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(180));
   BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(1));var ordinary=m.Flights.Last();float before=ordinary.Progress;
   BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(.1f));Assert.AreEqual(before+.1f,ordinary.Progress,.0001f);
   Command("UseHarvestSpeedCommand");Start();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));
   BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(.15f));Assert.AreEqual(1,m.Flights.Count);
  }
  [Test] public void ShufflePreservesEveryIdentityQuotaAndWorkingFlight(){
   var m=Start();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));var rules=BeeHarvestApp.Interface.GetSystem<HarvestSystem>();
   var fronts=Enumerable.Range(0,5).Select(rules.Front).ToArray();var data=m.Hives.Select(h=>h.Data).ToArray();
   var flight=m.Flights[0];var counts=m.Hives.Select(h=>h.Remaining).ToArray();
   Command("ShuffleWaitingHivesCommand",137);
   CollectionAssert.AreEquivalent(Enumerable.Range(1,9),Enumerable.Range(0,5).SelectMany(c=>m.WaitingColumn(c)));
   CollectionAssert.AreEqual(data,m.Hives.Select(h=>h.Data));CollectionAssert.AreEqual(counts,m.Hives.Select(h=>h.Remaining));
   Assert.AreSame(flight,m.Flights[0]);Assert.AreEqual(0,m.Slots[0]);Assert.IsTrue(m.Hives[0].Started);
   CollectionAssert.AreNotEqual(fronts,Enumerable.Range(0,5).Select(rules.Front));
   for(int c=0;c<5;c++){int h=rules.Front(c);if(h>=0)Assert.IsFalse(m.Hives[h].Started);}
  }
  [Test] public void AnyWaitingSelectionAllowsRearOnlyWhenEnabledAndStillHonorsCapacity(){
   var m=Start();Command("ActivateWaitingHiveCommand",1,0);Assert.IsFalse(m.Hives[1].Started);
   Command("UseFreeHiveSelectionCommand");Command("ActivateWaitingHiveCommand",1,0);
   Assert.IsTrue(m.Hives[1].Started);Assert.IsFalse(m.Hives[0].Started);Assert.AreEqual(1,m.Slots[0]);
   Command("ActivateWaitingHiveCommand",3,0);Assert.IsFalse(m.Hives[3].Started);
   Start();Command("ActivateWaitingHiveCommand",5,1);Assert.IsFalse(m.Hives[5].Started);
   Command("UseFreeHiveSelectionCommand");Start();Command("ActivateWaitingHiveCommand",1,0);Assert.IsFalse(m.Hives[1].Started);
  }
  [Test] public void OneFreeSelectionChoosesOneRearHiveThenRestoresFrontRules(){
   var m=Start();var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();
   app.SendCommand(new UseFreeHiveSelectionCommand());Assert.AreEqual(0,m.BoosterCount(3));Assert.IsTrue(rules.CanSelectHive(1));
   app.SendCommand(new ActivateWaitingHiveCommand(1,0));
   Assert.IsTrue(m.Hives[1].Started);Assert.IsFalse(m.FreeSelection,"One successful selection consumes the entitlement");
   Assert.IsTrue(rules.CanSelectHive(0));Assert.IsFalse(rules.CanSelectHive(3));
   app.SendCommand(new ActivateWaitingHiveCommand(3,1));Assert.IsFalse(m.Hives[3].Started);Assert.AreEqual(-1,m.Slots[1]);
   app.SendCommand(new RefillHarvestBoosterCommand(3));Assert.IsFalse(m.FreeSelection,"Refill only grants inventory");
   app.SendCommand(new UseFreeHiveSelectionCommand());app.SendCommand(new ActivateWaitingHiveCommand(3,1));
   Assert.IsTrue(m.Hives[3].Started);Assert.IsFalse(m.FreeSelection);Assert.AreEqual(0,m.BoosterCount(3));
  }
  [Test] public void InvalidAndRepeatedRequestsKeepOnePendingChoiceAndFrontSelectionUsesIt(){
   var m=Start();var app=BeeHarvestApp.Interface;
   for(int c=0;c<5;c++)app.SendCommand(new ActivateHiveCommand(c));
   app.SendCommand(new UseFreeHiveSelectionCommand());app.SendCommand(new RefillHarvestBoosterCommand(3));
   app.SendCommand(new UseFreeHiveSelectionCommand());Assert.IsTrue(m.FreeSelection);Assert.AreEqual(1,m.BoosterCount(3),"A repeated use cannot toggle or spend twice");
   app.SendCommand(new ActivateWaitingHiveCommand(1,0));app.SendCommand(new ActivateWaitingHiveCommand(-1,0));app.SendCommand(new ActivateWaitingHiveCommand(1,-1));app.SendCommand(new ActivateWaitingHiveCommand(1,6));
   Assert.IsTrue(m.FreeSelection,"Full or invalid destinations do not waste the entitlement");
   app.SendCommand(new AddHiveSlotCommand());app.SendCommand(new PauseHarvestCommand(true));app.SendCommand(new ActivateWaitingHiveCommand(1,5));
   Assert.IsTrue(m.FreeSelection);Assert.IsFalse(m.Hives[1].Started);
   app.SendCommand(new PauseHarvestCommand(false));app.SendCommand(new ActivateHiveCommand(0,5));
   Assert.IsTrue(m.Hives[1].Started);Assert.IsFalse(m.FreeSelection,"Choosing a front hive also counts as the single choice");Assert.AreEqual(1,m.BoosterCount(3));
  }
  [Test] public void PausedCommandsDoNotReorderOrEnableBoosters(){
   var m=Start();BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(true));int rev=m.Revision;
   Command("AddHiveSlotCommand");Command("UseHarvestSpeedCommand");Command("UseFreeHiveSelectionCommand");Command("ShuffleWaitingHivesCommand",3);
   Assert.AreEqual(rev,m.Revision);Assert.AreEqual(5,m.Slots.Count);
  }
 }
}
