using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class BeeTransportTimingTests:EconomyTestFixture {
  [Test] public void ArrivalPauseAndGraspKeepTheOriginalBlockUntilPickup(){
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="Red"}},hives=new[]{new HiveData{column=0,color="Red",quota=1}}}));
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds-.06f));var m=app.GetModel<HarvestModel>();
   Assert.AreEqual(1,m.Remaining,"Arrival pause and grasp must finish before the source cell is removed");Assert.AreEqual(0,m.InTransit);Assert.AreEqual(1,m.Cubes[0].Stage);
   app.SendCommand(new AdvanceHarvestCommand(.07f));Assert.AreEqual(0,m.Remaining);Assert.AreEqual(1,m.InTransit);Assert.AreEqual(0,m.Deposited);
   app.SendCommand(new AdvanceHarvestCommand(3));Assert.AreEqual(1,m.Deposited);Assert.IsTrue(m.Won);
  }
 }
}
