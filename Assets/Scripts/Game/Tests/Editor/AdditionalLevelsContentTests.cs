using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class AdditionalLevelsContentTests : EconomyTestFixture {
  [TestCase(6,"Chick",870,15)] [TestCase(7,"Sheep",1030,17)] [TestCase(8,"Shark",1054,19)]
  [TestCase(9,"Broccoli",1084,20)] [TestCase(10,"Cactustoy",1205,19)] [TestCase(11,"Carrot",1244,20)]
  [TestCase(12,"CatHead",1300,23)] [TestCase(13,"Snake",1595,26)] [TestCase(14,"Onion",1617,25)] [TestCase(15,"DodoBird",1622,28)]
  public void EachLightweightLevelLoadsItsOwnCompleteModelAndMatchingHivePalette(int number,string name,int count,int hives){
   var assets=new LevelOneAssets();LevelData data=null;
   Assert.DoesNotThrow(()=>data=assets.Level(number),"New level data must be reachable through the production loader");
   Assert.AreEqual(name,data.sourceName);Assert.AreEqual(count,data.cubes.Length);Assert.LessOrEqual(data.cubes.Length,2000);
   Assert.AreEqual(hives,data.hives.Length);Assert.AreEqual(3,data.columns);
   Assert.AreEqual(count,data.cubes.Select(c=>c.name).Distinct().Count());
   Assert.IsTrue(data.cubes.All(c=>c.name.StartsWith(name+"V2_Voxel_")));
   foreach(var group in data.cubes.GroupBy(c=>c.color)){
    Assert.AreEqual(group.Count(),data.hives.Where(h=>h.color==group.Key).Sum(h=>h.quota));
    foreach(string key in new[]{LevelOneAssets.BeeKey(group.Key),LevelOneAssets.HiveKey(group.Key)})
     Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+key),key);
   }
   var instance=assets.Spawn(LevelOneAssets.ModelKey(number),null);
   try {var parts=instance.GetComponentsInChildren<Renderer>();Assert.AreEqual(count,parts.Length);
    Assert.IsTrue(parts.All(r=>r.sharedMaterial!=null&&r.GetComponent<MeshFilter>().sharedMesh!=null));
   }finally{UnityEngine.Object.DestroyImmediate(instance);}
   var links=LinkedHiveGroups.Build(data.hives).Where(g=>g.Length>1).Select(g=>string.Join(",",g)).Distinct().ToArray();
   if(number==11)CollectionAssert.AreEquivalent(new[]{"11,16","13,18"},links);
   else if(number==13)CollectionAssert.AreEquivalent(new[]{"3,11","12,18","16,25"},links);
   else Assert.IsEmpty(links);
  }
  [TestCase(5,6)] [TestCase(6,7)] [TestCase(7,8)] [TestCase(8,9)] [TestCase(9,10)]
  [TestCase(10,11)] [TestCase(11,12)] [TestCase(12,13)] [TestCase(13,14)] [TestCase(14,15)] [TestCase(15,16)]
  public void VictoryAdvancesOnceThroughAllFifteenLevelsWithoutSpendingLife(int from,int to){
   var app=BeeHarvestApp.Interface;Assert.DoesNotThrow(()=>app.SendCommand(new InitializeMenuNavigationCommand(false,from)));
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));
   var model=app.GetModel<HarvestModel>();var nav=app.GetModel<MenuNavigationModel>();int life=app.GetModel<EconomyModel>().Stamina;
   app.SendCommand(new ContinueLevelCommand(from,model.Generation));Assert.AreEqual(from,nav.CurrentLevel,"Unwon level cannot advance");
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(model.Won);
   app.SendCommand(new ContinueLevelCommand(from,model.Generation-1));Assert.AreEqual(from,nav.CurrentLevel,"Stale callbacks cannot advance");
   var command=new ContinueLevelCommand(from,model.Generation);app.SendCommand(command);app.SendCommand(command);
   Assert.AreEqual(to,nav.CurrentLevel);Assert.AreEqual(false,nav.IsHome);Assert.AreEqual(life,app.GetModel<EconomyModel>().Stamina);
  }
  [Test] public void InvalidLevelBeyondCatalogIsRejectedWithoutLoading(){
   Assert.Throws<ArgumentOutOfRangeException>(()=>new LevelOneAssets().Level(LevelOneAssets.LastLevel+1));
  }
 }
}
