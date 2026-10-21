using System.Linq;
using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class TransportReservationTests:EconomyTestFixture {
  [TestCase(false)][TestCase(true)] public void AdjacentTargetsStaggerAcrossSameAndDifferentColors(bool mixed){
   var app=BeeHarvestApp.Interface;var color=mixed?"blue":"pink";
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="pink"},new CubeData{id=1,x=1,color=color}},hives=new[]{new HiveData{column=0,color="pink",quota=1},new HiveData{column=1,color=color,quota=1}}}));
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new ActivateHiveCommand(1,1));var m=app.GetModel<HarvestModel>();
   Assert.AreEqual(2,m.Flights.Count,"Both mouths may reserve and dispatch before local collection permission");Assert.AreEqual(2,m.Flights.Select(f=>f.Cube).Distinct().Count());
   app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.ApproachSeconds*.65f+.01f));
   Assert.IsTrue(m.Flights[0].CollectionGranted);Assert.IsFalse(m.Flights[1].CollectionGranted,"Only the neighboring final approach waits");Assert.AreEqual(1,m.Cubes[0].Stage);Assert.AreEqual(1,m.Cubes[1].Stage);
   for(int i=0;i<1000&&!m.Won;i++)app.SendCommand(new AdvanceHarvestCommand(.01f));Assert.AreEqual(2,m.Deposited);Assert.IsTrue(m.Won);app.SendCommand(new AdvanceHarvestCommand(5));Assert.AreEqual(2,m.Deposited);
  }
 }
}
