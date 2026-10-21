using System.Linq;
using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class BeeTrafficRulesTests:EconomyTestFixture {
  static LevelData Pair(int x){return new LevelData{cubes=new[]{new CubeData{id=0,x=0,color="pink"},new CubeData{id=1,x=x,color="pink"}},hives=new[]{new HiveData{column=0,color="pink",quota=1},new HiveData{column=1,color="pink",quota=1}}};}
  [Test] public void AdjacentRegionStaysExclusiveUntilTheCarrierHasCleared(){
   var a=BeeHarvestApp.Interface;a.SendCommand(new StartLevelCommand(Pair(1)));a.SendCommand(new ActivateHiveCommand(0));a.SendCommand(new ActivateHiveCommand(1,1));var m=a.GetModel<HarvestModel>();
   Assert.AreEqual(2,m.Flights.Count);a.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.ApproachSeconds*.65f+.01f));
   Assert.IsTrue(m.Flights[0].CollectionGranted);Assert.IsFalse(m.Flights[1].CollectionGranted);Assert.AreEqual(2,m.Flights.Select(f=>f.Cube).Distinct().Count());
   for(int i=0;i<500&&!m.Flights[1].CollectionGranted;i++)a.SendCommand(new AdvanceHarvestCommand(.01f));Assert.IsTrue(m.Flights[1].CollectionGranted);Assert.IsTrue(m.Flights[0].ClearedSource);
  }
  [Test] public void CollectorSeparatesDeliveriesWithOnlyAShortLocalInterval(){
   var a=BeeHarvestApp.Interface;a.SendCommand(new StartLevelCommand(Pair(12)));a.SendCommand(new ActivateHiveCommand(0));a.SendCommand(new ActivateHiveCommand(1,1));var m=a.GetModel<HarvestModel>();float first=-1,second=-1;
   for(int i=0;i<1000&&second<0;i++){a.SendCommand(new AdvanceHarvestCommand(.01f));if(m.Deposited>=1&&first<0)first=i*.01f;if(m.Deposited>=2)second=i*.01f;}
   Assert.Greater(first,0);Assert.GreaterOrEqual(second-first,.005f);Assert.Less(second-first,.08f,"Entry must not become a long global lock");a.SendCommand(new AdvanceHarvestCommand(20));Assert.AreEqual(2,m.Deposited);
  }
  [Test] public void NewPresentationOwnerRepairsHeadlessFlightsAndOldDetachCannotReleaseIt(){
   var a=BeeHarvestApp.Interface;a.SendCommand(new StartLevelCommand(Pair(12)));a.SendCommand(new ActivateHiveCommand(0));a.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+AdvanceHarvestCommand.LiftSeconds+.7f));var m=a.GetModel<HarvestModel>();
   try{a.SendCommand(new AttachBeeFlightPresentationCommand(11));Assert.IsTrue(m.Flights[0].ClearedSource,"An already passed outside corner must not become an unreachable wait");a.SendCommand(new AttachBeeFlightPresentationCommand(22));a.SendCommand(new DetachBeeFlightPresentationCommand(11));Assert.IsTrue(m.RequireFlightArrival);}finally{a.SendCommand(new DetachBeeFlightPresentationCommand(22));}
   Assert.IsFalse(m.RequireFlightArrival);for(int i=0;i<100;i++)a.SendCommand(new AdvanceHarvestCommand(.1f));a.SendCommand(new AttachBeeFlightPresentationCommand(33));try{Assert.IsTrue(m.Flights[0].Returned,"A rebuilt view has no actor for an old completed flight");}finally{a.SendCommand(new DetachBeeFlightPresentationCommand(33));}
  }

 }
}
