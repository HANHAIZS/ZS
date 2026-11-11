using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class SoftDepthNewLevelsContentTests : EconomyTestFixture {
  static readonly string[] Names={"Microphone_2","Lamp_3","Mower_Ride","Car4x4","Alien"};
  static readonly int[] Counts={2434,2624,2678,2949,2963};
  [Test] public void FiveNewCompleteThemesHaveThreeColumnsAndMatch106DisplayOptions(){
   Assert.GreaterOrEqual(LevelOneAssets.LastLevel,111);var assets=new LevelOneAssets();
   for(int i=0;i<5;i++){int n=107+i;Assert.AreEqual(Names[i],LevelOneAssets.ModelKey(n));var data=assets.Level(n);Assert.AreEqual(Names[i],data.sourceName);Assert.AreEqual(Counts[i],data.cubes.Length);Assert.AreEqual(3,data.columns);Assert.AreEqual(Counts[i],data.cubes.Select(c=>c.name).Distinct().Count());
    foreach(var color in data.cubes.GroupBy(c=>c.color))Assert.AreEqual(color.Count(),data.hives.Where(h=>h.color==color.Key).Sum(h=>h.quota));
    Assert.IsFalse(LevelOneAssets.HasVoxelEdges(n));Assert.IsTrue(LevelOneAssets.HasVoxelSeparation(n));Assert.IsTrue(LevelOneAssets.HasVoxelCavity(n));Assert.AreEqual(LevelOneAssets.VoxelEdgeColor(106),LevelOneAssets.VoxelEdgeColor(n));Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+Names[i]));
   }
  }
  [TestCase(106,107,false)] [TestCase(107,108,false)] [TestCase(110,111,false)] [TestCase(111,112,false)]
  public void ContinueAdvancesNewRangeExactlyOnceAndRejectsOldGeneration(int from,int to,bool home){
   var app=BeeHarvestApp.Interface;app.SendCommand(new InitializeMenuNavigationCommand(false,from));app.SendCommand(new StartLevelCommand(new LevelData{columns=3,cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));var model=app.GetModel<HarvestModel>();var nav=app.GetModel<MenuNavigationModel>();app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(model.Won);
   app.SendCommand(new ContinueLevelCommand(from,model.Generation-1));Assert.AreEqual(from,nav.CurrentLevel);var command=new ContinueLevelCommand(from,model.Generation);app.SendCommand(command);app.SendCommand(command);Assert.AreEqual(to,nav.CurrentLevel);Assert.AreEqual(home,nav.IsHome);
  }
  [Test] public void AfterCatalogIsRejected(){Assert.Throws<ArgumentOutOfRangeException>(()=>new LevelOneAssets().Level(LevelOneAssets.LastLevel+1));}
 }
}
