using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class HarvestInventoryTests : EconomyTestFixture {
  static void Command(string name,params object[] args){var type=typeof(HarvestModel).Assembly.GetType("BeeHarvest."+name);Assert.IsNotNull(type,"Missing inventory command: "+name);BeeHarvestApp.Interface.SendCommand((ICommand)Activator.CreateInstance(type,args));}
  static int Count(HarvestModel m,int index){var method=typeof(HarvestModel).GetMethod("BoosterCount");Assert.IsNotNull(method,"Missing per-booster inventory");return (int)method.Invoke(m,new object[]{index});}
  static float Seconds(HarvestModel m){var p=typeof(HarvestModel).GetProperty("SpeedSeconds");Assert.IsNotNull(p,"Missing timed speed state");return (float)p.GetValue(m);}
  static LevelData Data(){return new LevelData{cubes=Enumerable.Range(0,40).Select(i=>new CubeData{id=i,x=i,color="pink"}).ToArray(),hives=Enumerable.Range(0,10).Select(i=>new HiveData{column=i/2,quota=4,color="pink"}).ToArray()};}
  static HarvestModel Start(){BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(Data()));var m=BeeHarvestApp.Interface.GetModel<HarvestModel>();for(int i=0;i<4;i++)if(Count(m,i)==0)Command("RefillHarvestBoosterCommand",i);return m;}
  [Test] public void EachBoosterInitiallyHasOne(){var m=new HarvestModel();for(int i=0;i<4;i++)Assert.AreEqual(1,Count(m,i));}
  [Test] public void SuccessfulActionsConsumeAndZeroStockRejectsAnotherUse(){
   var m=Start();Command("AddHiveSlotCommand");Command("UseHarvestSpeedCommand");Command("ShuffleWaitingHivesCommand",17);Command("UseFreeHiveSelectionCommand");
   for(int i=0;i<4;i++)Assert.AreEqual(0,Count(m,i));Assert.AreEqual(6,m.Slots.Count);Assert.IsTrue(m.FreeSelection);Assert.AreEqual(180,Seconds(m));
   var order=Enumerable.Range(0,5).SelectMany(c=>m.WaitingColumn(c)).ToArray();int rev=m.Revision;
   Command("AddHiveSlotCommand");Command("UseHarvestSpeedCommand");Command("ShuffleWaitingHivesCommand",3);Command("UseFreeHiveSelectionCommand");
   Assert.AreEqual(rev,m.Revision);CollectionAssert.AreEqual(order,Enumerable.Range(0,5).SelectMany(c=>m.WaitingColumn(c)));
  }
  [Test] public void RefillAtZeroGrantsExactlyOneWithoutUsingIt(){
   var m=Start();Command("RefillHarvestBoosterCommand",1);Assert.AreEqual(1,Count(m,1));
   Command("UseHarvestSpeedCommand");BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(10));float remaining=Seconds(m);
   Command("RefillHarvestBoosterCommand",1);Command("RefillHarvestBoosterCommand",1);Assert.AreEqual(1,Count(m,1));Assert.AreEqual(remaining,Seconds(m));
   Command("UseHarvestSpeedCommand");Assert.AreEqual(0,Count(m,1));Assert.AreEqual(180,Seconds(m),"Another speed item refreshes duration without stacking multiplier");Assert.AreEqual(2,m.SpeedMultiplier);
   Command("RefillHarvestBoosterCommand",-1);Command("RefillHarvestBoosterCommand",4);
  }
  [Test] public void RetryKeepsConsumedInventoryAndClearsEffects(){
   var m=Start();Command("AddHiveSlotCommand");Command("UseHarvestSpeedCommand");Command("ShuffleWaitingHivesCommand",17);Command("UseFreeHiveSelectionCommand");Command("RefillHarvestBoosterCommand",2);
   var counts=Enumerable.Range(0,4).Select(i=>Count(m,i)).ToArray();BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(Data()));
   CollectionAssert.AreEqual(counts,Enumerable.Range(0,4).Select(i=>Count(m,i)));Assert.AreEqual(5,m.Slots.Count);Assert.IsFalse(m.FreeSelection);Assert.AreEqual(0,Seconds(m));Assert.AreEqual(1,m.SpeedMultiplier);
  }
  [Test] public void SpeedLasts180GameplaySecondsAndPauseFreezesIt(){
   var m=Start();Command("UseHarvestSpeedCommand");BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(179.75f));Assert.AreEqual(.25f,Seconds(m),.001f);Assert.AreEqual(2,m.SpeedMultiplier);
   BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(true));BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(200));Assert.AreEqual(.25f,Seconds(m),.001f);
   BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(false));BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(.25f));Assert.AreEqual(0,Seconds(m));Assert.AreEqual(1,m.SpeedMultiplier);Assert.IsFalse(m.SpeedBoost);
  }
  [Test] public void ExpiryStepOnlyBoostsTimeBeforeTheDeadline(){
   var m=Start();Command("UseHarvestSpeedCommand");BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(179.75f));BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));
   foreach(float invalid in new[]{-1f,float.NaN,float.PositiveInfinity})BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(invalid));Assert.AreEqual(.25f,Seconds(m),.001f);Assert.AreEqual(0,m.Flights[0].Progress);
   BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(.5f));Assert.AreEqual(.75f,m.Flights[0].Progress,.001f);Assert.AreEqual(0,Seconds(m));Assert.AreEqual(1,m.SpeedMultiplier);
  }
  [Test] public void LongFramesDoNotExtend180SecondsByTheSimulationClamp(){
   var m=Start();Command("UseHarvestSpeedCommand");var ctor=typeof(AdvanceHarvestCommand).GetConstructor(new[]{typeof(float),typeof(float)});Assert.IsNotNull(ctor,"Separate real duration from capped animation steps");
   for(int frame=0;frame<359;frame++)BeeHarvestApp.Interface.SendCommand((ICommand)ctor.Invoke(new object[]{.1f,.5f}));Assert.AreEqual(.5f,Seconds(m),.001f);
   BeeHarvestApp.Interface.SendCommand((ICommand)ctor.Invoke(new object[]{.1f,.5f}));Assert.AreEqual(0,Seconds(m));Assert.AreEqual(1,m.SpeedMultiplier);
  }
  [Test] public void UnavailableActionsAndPausedRefillsDoNotSpendOrGrant(){
   var m=Start();Command("AddHiveSlotCommand");Command("RefillHarvestBoosterCommand",0);Command("AddHiveSlotCommand");Assert.AreEqual(1,Count(m,0));
   Command("UseFreeHiveSelectionCommand");Command("RefillHarvestBoosterCommand",3);Command("UseFreeHiveSelectionCommand");Assert.AreEqual(1,Count(m,3));
   Command("UseHarvestSpeedCommand");BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(true));int rev=m.Revision;
   Command("RefillHarvestBoosterCommand",1);Command("ShuffleWaitingHivesCommand",3);Assert.AreEqual(rev,m.Revision);Assert.AreEqual(0,Count(m,1));Assert.AreEqual(1,Count(m,2));
   BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="pink"}},hives=new[]{new HiveData{column=0,color="pink",quota=1}}}));Command("ShuffleWaitingHivesCommand",7);Assert.AreEqual(1,Count(m,2),"No possible reorder must not consume an item");
  }
 }
}
