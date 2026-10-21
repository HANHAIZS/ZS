using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class CameraRotationTests : EconomyTestFixture {
  static LevelData Stack(){return new LevelData{cubes=new[]{
   new CubeData{id=0,x=0,y=0,z=0,color="Red"},new CubeData{id=1,x=0,y=0,z=1,color="Yellow"}},
   hives=new[]{new HiveData{column=0,color="Red",quota=1},new HiveData{column=1,color="Yellow",quota=1}}};}
  [Test] public void CameraRotationExchangesFrontAndBackAndSideFacesBecomeVisible(){
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(Stack()));var rules=app.GetSystem<HarvestSystem>();
   Assert.AreEqual(0,rules.FindTarget("Red"));Assert.AreEqual(-1,rules.FindTarget("Yellow"));
   app.SendCommand(new UpdateHarvestViewCommand(Vector3.forward,new Vector3(0,0,100)));
   Assert.AreEqual(-1,rules.FindTarget("Red"));Assert.AreEqual(1,rules.FindTarget("Yellow"));
   app.SendCommand(new UpdateHarvestViewCommand(Vector3.right,new Vector3(100,0,0)));
   Assert.AreEqual(0,rules.FindTarget("Red"));Assert.AreEqual(1,rules.FindTarget("Yellow"));
  }
  [Test] public void RotationDuringApproachDoesNotInterruptTheCommittedPickup(){
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(Stack()));app.SendCommand(new ActivateHiveCommand(0));
   var model=app.GetModel<HarvestModel>();app.SendCommand(new UpdateHarvestViewCommand(Vector3.forward,new Vector3(0,0,100)));
   app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+.01f));
   Assert.AreEqual(1,model.Flights[0].Stage);Assert.AreEqual(1,model.Remaining);Assert.AreEqual(1,model.InTransit);
   Assert.AreEqual(0,model.Flights[0].Cube,"The originally visible red cube stays committed after it rotates behind yellow");
   app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.LoadedSeconds));
   Assert.AreEqual(2,model.Flights[0].Stage);Assert.AreEqual(1,model.Deposited);Assert.AreEqual(0,model.Hives[0].Remaining,"Occlusion never spends a second quota");
  }
  [Test] public void OcclusionDuringGraspKeepsAdvancingTheSameReservedCube(){
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(Stack()));app.SendCommand(new ActivateHiveCommand(0));
   var model=app.GetModel<HarvestModel>();app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.ApproachSeconds+AdvanceHarvestCommand.SettleSeconds+AdvanceHarvestCommand.ReachSeconds*.5f));
   float progress=model.Flights[0].Progress;
   app.SendCommand(new UpdateHarvestViewCommand(Vector3.forward,new Vector3(0,0,100)));
   app.SendCommand(new AdvanceHarvestCommand(.02f));
   Assert.AreEqual(progress+.02f,model.Flights[0].Progress,.00001f,"An already locked grasp continues despite occlusion");Assert.AreEqual(0,model.Flights[0].Cube);
   Assert.AreEqual(0,model.Flights[0].Stage);
  }
  [Test] public void ObliqueVisibleFaceEdgeIsHarvestable(){
   var data=new LevelData{cubes=new[]{
    new CubeData{id=0,x=0,y=0,z=0,color="Green"},new CubeData{id=1,x=-1,y=0,z=0,color="Red"},
    new CubeData{id=2,x=-1,y=1,z=1,color="Red"},new CubeData{id=3,x=0,y=0,z=1,color="Red"}},
    hives=new[]{new HiveData{column=0,color="Green",quota=1},new HiveData{column=1,color="Red",quota=3}}};
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(data));
   var direction=new Vector3(-.60874552f,.16731342f,.77552248f).normalized;
   app.SendCommand(new UpdateHarvestViewCommand(direction,direction*100));
   Assert.AreEqual(0,app.GetSystem<HarvestSystem>().FindTarget("Green"),"A visible face edge must not be rejected just because its center is covered");
  }
  [Test] public void HiddenMatchingColorWaitsWithoutFalseFailureOrDeparture(){
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(Stack()));app.SendCommand(new ActivateHiveCommand(1));
   var model=app.GetModel<HarvestModel>();for(int i=0;i<30;i++)app.SendCommand(new AdvanceHarvestCommand(.1f));
   Assert.IsFalse(model.Deadlocked);Assert.AreEqual(0,model.Flights.Count);Assert.AreEqual(1,model.Hives[1].Remaining);
   app.SendCommand(new UpdateHarvestViewCommand(Vector3.forward,new Vector3(0,0,100)));
   app.SendCommand(new AdvanceHarvestCommand(.01f));Assert.AreEqual(1,model.Flights.Count);Assert.AreEqual(1,model.Flights[0].Cube);
  }
 }
}