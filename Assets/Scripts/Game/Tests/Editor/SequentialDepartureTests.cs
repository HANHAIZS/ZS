using System.Linq;
using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class SequentialDepartureTests:EconomyTestFixture {
  static HarvestModel Start(){var a=BeeHarvestApp.Interface;a.SendCommand(new StartLevelCommand(new LevelData{cubes=Enumerable.Range(0,8).Select(i=>new CubeData{id=i,x=i*3,color="pink"}).ToArray(),hives=new[]{new HiveData{column=0,color="pink",quota=8}}}));a.SendCommand(new ActivateHiveCommand(0));return a.GetModel<HarvestModel>();}
  [Test] public void HiveDepartsOneAtATimeAndPauseDoesNotQueueABurst(){
   var m=Start();var app=BeeHarvestApp.Interface;Assert.AreEqual(1,m.Flights.Count);Assert.AreEqual(7,m.Hives[0].Remaining);
   for(int i=0;i<4;i++)app.SendCommand(new AdvanceHarvestCommand(0));Assert.AreEqual(1,m.Flights.Count);
   app.SendCommand(new AdvanceHarvestCommand(.17f));Assert.AreEqual(1,m.Flights.Count);
   app.SendCommand(new PauseHarvestCommand(true));app.SendCommand(new AdvanceHarvestCommand(10));Assert.AreEqual(1,m.Flights.Count);
   app.SendCommand(new PauseHarvestCommand(false));app.SendCommand(new AdvanceHarvestCommand(.01f));Assert.AreEqual(2,m.Flights.Count);
   Assert.Greater(m.Flights[0].Progress,m.Flights[1].Progress);
   for(int i=0;i<3;i++)app.SendCommand(new AdvanceHarvestCommand(.18f));Assert.AreEqual(5,m.Flights.Count);Assert.Greater(m.Hives[0].Busy,3);
   Assert.AreEqual(5,m.Flights.Select(f=>f.Cube).Distinct().Count());
  }
  [Test] public void LongFrameReleasesOnlyOneReplacementBee(){
   var m=Start();var app=BeeHarvestApp.Interface;app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+AdvanceHarvestCommand.LoadedSeconds+.1f));
   Assert.AreEqual(2,m.Flights.Count,"One replacement per hive per update, with no catch-up burst");Assert.AreEqual(1,m.Deposited);Assert.AreEqual(1,m.Hives[0].Busy);
   app.SendCommand(new AdvanceHarvestCommand(0));Assert.AreEqual(2,m.Flights.Count);app.SendCommand(new AdvanceHarvestCommand(.18f));Assert.AreEqual(3,m.Flights.Count);Assert.AreEqual(5,m.Hives[0].Remaining);
  }
 }
}
