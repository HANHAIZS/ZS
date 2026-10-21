using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class OutlinedNewLevelsContentTests : EconomyTestFixture {
  static readonly string[] Names={"Pigeon_2","Cola","iceberg","Wheelbarrow","Medieval_church","Ladybug","Maple","Willys","Scorpion","mouse_2"};
  static readonly int[] Counts={1291,1685,1732,1804,2402,2591,2648,2660,2876,2913};
  [Test] public void TenDistinctThemesHaveThreeColumnsAndExactSourceColorQuotas(){
   var assets=new LevelOneAssets();for(int i=0;i<10;i++){int n=i+96;var data=assets.Level(n);Assert.AreEqual(Names[i],data.sourceName);Assert.AreEqual(Names[i],LevelOneAssets.ModelKey(n));Assert.AreEqual(Counts[i],data.cubes.Length);Assert.AreEqual(3,data.columns);foreach(var color in data.cubes.GroupBy(c=>c.color))Assert.AreEqual(color.Count(),data.hives.Where(h=>h.color==color.Key).Sum(h=>h.quota));Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+Names[i]));}
  }
  [TestCase(95,96,false)] [TestCase(104,105,false)] [TestCase(105,1,true)]
  public void ContinueReachesNewLevelsOnceAndOnlyOneHundredFiveWraps(int from,int to,bool home){
   var app=BeeHarvestApp.Interface;app.SendCommand(new InitializeMenuNavigationCommand(false,from));app.SendCommand(new StartLevelCommand(new LevelData{columns=3,cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));var model=app.GetModel<HarvestModel>();var nav=app.GetModel<MenuNavigationModel>();app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(model.Won);app.SendCommand(new ContinueLevelCommand(from,model.Generation-1));Assert.AreEqual(from,nav.CurrentLevel);var command=new ContinueLevelCommand(from,model.Generation);app.SendCommand(command);app.SendCommand(command);Assert.AreEqual(to,nav.CurrentLevel);Assert.AreEqual(home,nav.IsHome);
  }
  [Test] public void AfterOneHundredFiveIsRejected(){Assert.Throws<ArgumentOutOfRangeException>(()=>new LevelOneAssets().Level(106));}
 }
}
