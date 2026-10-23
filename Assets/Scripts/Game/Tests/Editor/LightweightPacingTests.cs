using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class LightweightPacingTests : EconomyTestFixture {
  static LevelData Data(bool tuned=true){
   var d=new LevelData{columns=3,cubes=Enumerable.Range(0,10).Select(i=>new CubeData{id=i,x=i,color="red"}).ToArray(),hives=new[]{new HiveData{column=0,color="red",quota=10}}};
   if(tuned)d.beeTiming=new BeeTimingData{departureSeconds=.26f,settleSeconds=.18f,reachSeconds=.16f,liftSeconds=.26f,handoffSeconds=.12f};
   return JsonUtility.FromJson<LevelData>(JsonUtility.ToJson(d));
  }
  [Test] public void TunedDepartureWaitsLongerAndLegacyRestartRestoresOldRate(){
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();
   app.SendCommand(new StartLevelCommand(Data()));app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(.001f));app.SendCommand(new AdvanceHarvestCommand(.20f));Assert.AreEqual(1,m.Flights.Count,"New levels must not dispatch a second bee at the old .18s interval");app.SendCommand(new AdvanceHarvestCommand(.06f));Assert.AreEqual(2,m.Flights.Count);
   app.SendCommand(new StartLevelCommand(Data(false)));app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(.001f));app.SendCommand(new AdvanceHarvestCommand(.20f));Assert.AreEqual(2,m.Flights.Count,"Old levels must retain their departure rate after leaving a new level");
  }
  [Test] public void TunedPickupKeepsTheSourceLongerAndGivesExtractionMoreTime(){
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();app.SendCommand(new StartLevelCommand(Data()));app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(.001f));var f=m.Flights[0];app.SendCommand(new AdvanceHarvestCommand(1.31f));Assert.AreEqual(0,f.Stage,"The old 1.30s pickup boundary must no longer hide the source");app.SendCommand(new AdvanceHarvestCommand(.08f));Assert.AreEqual(1,f.Stage);app.SendCommand(new AdvanceHarvestCommand(.24f));Assert.AreEqual(0,f.TransportPhase,"Extraction must last longer than the legacy .22s lift");app.SendCommand(new AdvanceHarvestCommand(.021f));Assert.GreaterOrEqual(f.TransportPhase,2);
  }
  [Test] public void InvalidTimingRejectsBeforeResettingTheCurrentLevel(){
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();app.SendCommand(new StartLevelCommand(Data(false)));int generation=m.Generation;var invalid=Data();invalid.beeTiming.handoffSeconds=.5f;Assert.Throws<ArgumentException>(()=>app.SendCommand(new StartLevelCommand(invalid)));Assert.AreEqual(generation,m.Generation);Assert.AreEqual(10,m.Remaining);
  }
 }
}
