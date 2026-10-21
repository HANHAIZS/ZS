using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class LevelsFourFiveContentTests : EconomyTestFixture {
  [TestCase(4,"Bee",1105,15)]
  [TestCase(5,"Watermelon",2840,51)]
  public void NewLevelLoadsItsOwnSourceAndAllPaletteAssets(int number,string name,int count,int hives){
   var assets=new LevelOneAssets();LevelData data=null;Assert.DoesNotThrow(()=>data=assets.Level(number));
   Assert.AreEqual(name,data.sourceName);Assert.AreEqual(count,data.cubes.Length);Assert.AreEqual(hives,data.hives.Length);Assert.AreEqual(3,data.columns);
   Assert.AreEqual(count,data.cubes.Select(c=>c.name).Distinct().Count());Assert.IsTrue(data.cubes.All(c=>c.name.StartsWith(name+"V2_Voxel_")));
   foreach(var group in data.cubes.GroupBy(c=>c.color)){Assert.AreEqual(group.Count(),data.hives.Where(h=>h.color==group.Key).Sum(h=>h.quota));
    Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.BeeKey(group.Key)));Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.HiveKey(group.Key)));}
   var instance=assets.Spawn(LevelOneAssets.ModelKey(number),null);try{var parts=instance.GetComponentsInChildren<Renderer>();Assert.AreEqual(count,parts.Length);Assert.IsTrue(parts.All(r=>r.sharedMaterial!=null));}finally{Object.DestroyImmediate(instance);}
   var groups=LinkedHiveGroups.Build(data.hives).Where(g=>g.Length>1).Select(g=>string.Join(",",g)).Distinct().OrderBy(x=>x).ToArray();
   if(number==5)CollectionAssert.AreEquivalent(new[]{"1,20","6,24,39","12,30,45","21,37"},groups);else Assert.IsEmpty(groups);
  }
  [Test] public void BeeVoxelModelHasItsOwnKeyAndPreservesTheFlightBeePrefab(){
   Assert.AreNotEqual("Bee",LevelOneAssets.ModelKey(4),"The source model must not replace the legacy flight-bee resource");
   var flight=Resources.Load<GameObject>("Prefabs/Gameplay/Bee");Assert.IsNotNull(flight);
   Assert.AreEqual(4,flight.GetComponentsInChildren<Transform>(true).Count(t=>t.name.Contains("_Limb_")));
   Assert.IsFalse(flight.GetComponentsInChildren<Renderer>(true).Any(r=>r.name.StartsWith("BeeV2_Voxel_")));
  }
  [TestCase(3,4)] [TestCase(4,5)]
  public void CompletingPrecedingLevelAdvancesOnlyOnceWithoutCharge(int from,int to){
   var app=BeeHarvestApp.Interface;Assert.DoesNotThrow(()=>app.SendCommand(new InitializeMenuNavigationCommand(false,from)));
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));var m=app.GetModel<HarvestModel>();Assert.IsTrue(m.Won);int energy=app.GetModel<EconomyModel>().Stamina;
   var command=new ContinueLevelCommand(from,m.Generation);app.SendCommand(command);app.SendCommand(command);var nav=app.GetModel<MenuNavigationModel>();Assert.AreEqual(to,nav.CurrentLevel);Assert.IsFalse(nav.IsHome);Assert.AreEqual(energy,app.GetModel<EconomyModel>().Stamina);
  }
 }
}
