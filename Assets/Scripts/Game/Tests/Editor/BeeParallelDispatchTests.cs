using System.Linq;
using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class BeeParallelDispatchTests:EconomyTestFixture {
  static LevelData Line(int count,int hives=1){
   return new LevelData{cubes=Enumerable.Range(0,count).Select(i=>new CubeData{id=i,x=i*3,color="pink"}).ToArray(),hives=Enumerable.Range(0,hives).Select(i=>new HiveData{column=i,color="pink",quota=count/hives}).ToArray()};
  }
  static HarvestModel Start(LevelData data){var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(data));return app.GetModel<HarvestModel>();}
  [Test] public void EverySlottedHiveDispatchesImmediatelyEvenOnTheSameSide(){
   var m=Start(Line(12,3));var app=BeeHarvestApp.Interface;for(int i=0;i<3;i++)app.SendCommand(new ActivateHiveCommand(i));
   Assert.AreEqual(3,m.Flights.Count,"Each independent hive can reserve and launch its first bee");Assert.AreEqual(3,m.Flights.Select(f=>f.Cube).Distinct().Count());
  }
  [Test] public void EachHiveDispatchesEveryPointOneEightSecondsWithoutABusyCap(){
   var m=Start(Line(12));var app=BeeHarvestApp.Interface;app.SendCommand(new ActivateHiveCommand(0));
   app.SendCommand(new AdvanceHarvestCommand(.17f));Assert.AreEqual(1,m.Flights.Count);app.SendCommand(new AdvanceHarvestCommand(.01f));Assert.AreEqual(2,m.Flights.Count);
   for(int i=0;i<4;i++)app.SendCommand(new AdvanceHarvestCommand(.18f));Assert.AreEqual(6,m.Flights.Count);Assert.Greater(m.Hives[0].Busy,3);Assert.AreEqual(6,m.Hives[0].Remaining);
  }
  [Test] public void PausedDispatchDoesNotAccumulateACatchupBurst(){
   var m=Start(Line(12));var app=BeeHarvestApp.Interface;app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(.17f));
   app.SendCommand(new PauseHarvestCommand(true));app.SendCommand(new AdvanceHarvestCommand(20));Assert.AreEqual(1,m.Flights.Count);
   app.SendCommand(new PauseHarvestCommand(false));app.SendCommand(new AdvanceHarvestCommand(.01f));Assert.AreEqual(2,m.Flights.Count);app.SendCommand(new AdvanceHarvestCommand(0));Assert.AreEqual(2,m.Flights.Count);
  }
  [Test] public void AdjacentTargetsCanBeReservedByDifferentHivesBeforeEitherArrives(){
   var data=Line(2,2);data.cubes[1].x=1;var m=Start(data);var app=BeeHarvestApp.Interface;app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new ActivateHiveCommand(1,1));
   Assert.AreEqual(2,m.Flights.Count);Assert.AreEqual(2,m.Cubes.Count(c=>c.Stage==1));Assert.AreEqual(0,m.Deposited);
  }
  [Test] public void DeliveryDoesNotBlockAReplacementHiveFromDeparting(){
   var m=Start(Line(2,2));var app=BeeHarvestApp.Interface;app.SendCommand(new ActivateHiveCommand(0));
   for(int i=0;i<1000&&m.Deposited==0;i++)app.SendCommand(new AdvanceHarvestCommand(.01f));Assert.AreEqual(1,m.Deposited);
   app.SendCommand(new ActivateHiveCommand(1));Assert.AreEqual(2,m.Flights.Count,"A completed delivery must not own any departure point");Assert.IsTrue(m.Flights[0].Returned,"The bee and cargo finish together; no empty return task");
  }
  [Test] public void ALongStepDoesNotReleaseManyBeesAtTheSameTime(){
   var m=Start(Line(12));var app=BeeHarvestApp.Interface;app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(30));
   Assert.AreEqual(2,m.Flights.Count);app.SendCommand(new AdvanceHarvestCommand(0));Assert.AreEqual(2,m.Flights.Count);
  }
  [Test] public void ReusedDeparturePortWaitsLocallyWithoutBlockingAnotherSlot(){
   var data=Line(3,3);var m=Start(data);var app=BeeHarvestApp.Interface;
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new ActivateHiveCommand(1));
   Assert.AreEqual(1,m.Flights.Count,"A replacement at the same physical mouth waits for the first bee");
   app.SendCommand(new ActivateHiveCommand(2,1));Assert.AreEqual(2,m.Flights.Count,"Another mouth remains independent");
   app.SendCommand(new AdvanceHarvestCommand(.17f));Assert.AreEqual(2,m.Flights.Count);
   app.SendCommand(new AdvanceHarvestCommand(.01f));Assert.AreEqual(3,m.Flights.Count);
  }
  [Test] public void ShortTransportStillDepositsEveryReservationExactlyOnce(){
   var m=Start(Line(12,3));var app=BeeHarvestApp.Interface;for(int i=0;i<3;i++)app.SendCommand(new ActivateHiveCommand(i));
   for(int i=0;i<1000&&!m.Won;i++)app.SendCommand(new AdvanceHarvestCommand(.01f));Assert.IsTrue(m.Won);Assert.AreEqual(12,m.Deposited);Assert.IsTrue(m.Flights.All(f=>f.Stage==2&&f.Returned));Assert.IsTrue(m.Hives.All(h=>h.Busy==0&&h.Reserved==0));
   app.SendCommand(new AdvanceHarvestCommand(20));Assert.AreEqual(12,m.Deposited);Assert.AreEqual(0,m.InTransit);Assert.AreEqual(12,m.Flights.Select(f=>f.Cube).Distinct().Count());
  }
  [Test] public void SixHiveEntryBurstKeepsTheLocalQueueShort(){
   var data=Line(60);data.hives=Enumerable.Range(0,6).Select(i=>new HiveData{column=i%5,color="pink",quota=10}).ToArray();var m=Start(data);var app=BeeHarvestApp.Interface;
   app.SendCommand(new RefillHarvestBoosterCommand(0));app.SendCommand(new AddHiveSlotCommand());for(int c=0;c<5;c++)app.SendCommand(new ActivateHiveCommand(c));app.SendCommand(new ActivateHiveCommand(0,5));
   Assert.AreEqual(6,m.Flights.Count);var began=new System.Collections.Generic.Dictionary<int,float>();float worst=0;
   for(int step=0;step<500&&m.Deposited<6;step++){
    app.SendCommand(new AdvanceHarvestCommand(.01f));float time=step*.01f;
    foreach(var f in m.Flights.Take(6)){
     if(f.Stage==1&&f.TransportPhase==3&&!began.ContainsKey(f.Id))began[f.Id]=time;
     if(f.Stage==1&&f.TransportPhase==4&&began.ContainsKey(f.Id)){worst=System.Math.Max(worst,time-began[f.Id]);began.Remove(f.Id);}
    }
   }
   Assert.GreaterOrEqual(m.Deposited,6);Assert.Less(worst,.16f,"Overlapping short entry queues must not turn into long loaded waits");
  }
 }
}
