using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class LightweightMysteryContentTests : EconomyTestFixture {
  [TestCase(112,"Rabbit",1860,31,15)]
  [TestCase(113,"Astronaut",1995,35,16)]
  [TestCase(114,"Hourglass",1432,26,12)]
  public void CompleteOriginalModelsAndHiddenQueuesAreAvailable(int number,string name,int cubes,int hives,int hidden){
   var data=new LevelOneAssets().Level(number);Assert.AreEqual(name,LevelOneAssets.ModelKey(number));Assert.AreEqual(name,data.sourceName);Assert.AreEqual(cubes,data.cubes.Length);Assert.Less(data.cubes.Length,2000);Assert.AreEqual(hives,data.hives.Length);Assert.AreEqual(hidden,data.hives.Count(h=>h.isHidden));Assert.AreEqual(3,data.columns);
   Assert.AreEqual(cubes,data.cubes.Select(c=>c.name).Distinct().Count());Assert.AreEqual(cubes,data.cubes.Select(c=>new Vector3Int(c.x,c.y,c.z)).Distinct().Count());
   foreach(var group in data.cubes.GroupBy(c=>c.color))Assert.AreEqual(group.Count(),data.hives.Where(h=>h.color==group.Key).Sum(h=>h.quota));
   Assert.IsTrue(LevelOneAssets.HasVoxelSeparation(number));Assert.IsTrue(LevelOneAssets.HasVoxelCavity(number));Assert.IsFalse(LevelOneAssets.HasVoxelEdges(number));Assert.AreEqual(LevelOneAssets.VoxelEdgeColor(6),LevelOneAssets.VoxelEdgeColor(number));Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+name));
  }
  [TestCase(111,112,false)] [TestCase(112,113,false)] [TestCase(113,114,false)] [TestCase(114,115,false)] [TestCase(115,116,false)] [TestCase(116,117,false)] [TestCase(117,118,false)] [TestCase(118,119,false)] [TestCase(119,1,true)]
  public void VictoryContinuesAcrossNewRangeOnceAndRejectsStaleGeneration(int from,int to,bool home){
   var app=BeeHarvestApp.Interface;app.SendCommand(new InitializeMenuNavigationCommand(false,from));app.SendCommand(new StartLevelCommand(new LevelData{columns=3,cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));var model=app.GetModel<HarvestModel>();var nav=app.GetModel<MenuNavigationModel>();app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(model.Won);
   app.SendCommand(new ContinueLevelCommand(from,model.Generation-1));Assert.AreEqual(from,nav.CurrentLevel);var command=new ContinueLevelCommand(from,model.Generation);app.SendCommand(command);app.SendCommand(command);Assert.AreEqual(to,nav.CurrentLevel);Assert.AreEqual(home,nav.IsHome);
  }
  [Test] public void AfterLastIsRejected(){Assert.Throws<ArgumentOutOfRangeException>(()=>new LevelOneAssets().Level(LevelOneAssets.LastLevel+1));}
 }
}
