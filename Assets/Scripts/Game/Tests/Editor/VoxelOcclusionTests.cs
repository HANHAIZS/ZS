using System.Collections.Generic;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class VoxelOcclusionTests : EconomyTestFixture {
  // A closed voxel shell must hide its center even at a grazing camera angle.
  [TestCase(.01f,.17f)] [TestCase(.05f,.25f)] [TestCase(.1f,.17f)]
  [TestCase(89.95f,.17f)] [TestCase(90.05f,.25f)]
  public void ClosedShellDoesNotExposeCenterThroughBevelGaps(float yaw,float elevation){
   var cubes=new List<CubeData>{new CubeData{id=0,x=0,y=0,z=0,color="Blue"}};
   for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)for(int z=-1;z<=1;z++){
    if(x==0&&y==0&&z==0)continue;
    cubes.Add(new CubeData{id=cubes.Count,x=x,y=y,z=z,color="Red"});
   }
   var data=new LevelData{cubes=cubes.ToArray(),hives=new[]{new HiveData{column=0,color="Blue",quota=1},new HiveData{column=1,color="Red",quota=26}}};
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(data));
   float angle=yaw*Mathf.Deg2Rad;var direction=new Vector3(Mathf.Sin(angle),elevation,-Mathf.Cos(angle)).normalized;
   app.SendCommand(new UpdateHarvestViewCommand(direction,direction*100));
   Assert.AreEqual(-1,app.GetSystem<HarvestSystem>().FindTarget("Blue"),"A completely enclosed center cannot be collected through decorative gaps");
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(.1f));
   Assert.AreEqual(0,app.GetModel<HarvestModel>().Flights.Count,"Hidden color must wait without spending bee quota");
  }
 }
}
