using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;

namespace BeeHarvest.Tests {
 public sealed class LevelThreeContentTests : EconomyTestFixture {
  [Test] public void ThirdLevelLoadsBananaWithDistinctVoxelIdentitiesAndMatchingQuotas(){
   var assets=new LevelOneAssets();LevelData data=null;
   Assert.DoesNotThrow(()=>data=assets.Level(3),"The third playable level must be registered");
   Assert.AreEqual("Banana",data.sourceName);Assert.AreEqual(812,data.cubes.Length);Assert.AreEqual(20,data.hives.Length);Assert.AreEqual(3,data.columns);
   Assert.AreEqual(812,data.cubes.Select(c=>c.name).Distinct().Count());Assert.IsTrue(data.cubes.All(c=>c.name.StartsWith("BananaV2_Voxel_")));
   foreach(var group in data.cubes.GroupBy(c=>c.color))Assert.AreEqual(group.Count(),data.hives.Where(h=>h.color==group.Key).Sum(h=>h.quota));
   var instance=assets.Spawn(LevelOneAssets.ModelKey(3),null);
   try{Assert.AreEqual(812,instance.GetComponentsInChildren<Renderer>().Length);foreach(var color in data.cubes.Select(c=>c.color).Distinct()){
    Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.BeeKey(color)),color);
    Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.HiveKey(color)),color);
   }}finally{Object.DestroyImmediate(instance);}
  }
  [Test] public void SecondVictoryAdvancesToThirdOnlyOnceWithoutSpendingStamina(){
   var app=BeeHarvestApp.Interface;app.SendCommand(new InitializeMenuNavigationCommand(false,2));
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));var model=app.GetModel<HarvestModel>();Assert.IsTrue(model.Won);
   int stamina=app.GetModel<EconomyModel>().Stamina;var command=new ContinueLevelCommand(2,model.Generation);app.SendCommand(command);app.SendCommand(command);
   var nav=app.GetModel<MenuNavigationModel>();Assert.AreEqual(3,nav.CurrentLevel);Assert.IsFalse(nav.IsHome);Assert.AreEqual(stamina,app.GetModel<EconomyModel>().Stamina);
  }
 }
}
