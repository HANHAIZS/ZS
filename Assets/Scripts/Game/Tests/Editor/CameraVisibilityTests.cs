using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class CameraVisibilityTests : EconomyTestFixture {
  // The old exterior-air rule must fail: a matching rear cube is hidden by another color.
  [Test] public void HiddenRearLayerWaitsUntilFrontPickup(){
   var app=BeeHarvestApp.Interface;
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,x=0,y=0,z=0,color="Red"},new CubeData{id=1,x=0,y=0,z=1,color="Yellow"}},hives=new[]{new HiveData{column=0,color="Red",quota=1},new HiveData{column=1,color="Yellow",quota=1}}}));
   var rules=app.GetSystem<HarvestSystem>();
   Assert.AreEqual(-1,rules.FindTarget("Yellow"),"A rear layer must remain unavailable while the front cube blocks the camera");
   app.SendCommand(new ActivateHiveCommand(1));
   Assert.AreEqual(0,app.GetModel<HarvestModel>().Flights.Count);
   app.SendCommand(new ActivateHiveCommand(0));
   Assert.AreEqual(-1,rules.FindTarget("Yellow"),"A reserved front cube still blocks the rear cube");
   app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+.001f));
   Assert.IsTrue(rules.IsCameraVisible(1),"Removing the cap reveals the real rear cell");Assert.AreEqual(1,app.GetModel<HarvestModel>().Cubes[1].Stage,"The independent yellow hive reserves the newly exposed target immediately");
   app.SendCommand(new AdvanceHarvestCommand(.8f));
   Assert.AreEqual(1,app.GetModel<HarvestModel>().Cubes[1].Stage,"Rear cube unlocks only after the front cube is extracted");
  }
 }
}