using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class V2AssetIntegrationTests : EconomyTestFixture {
  // Old production prefabs lack the rebuilt palette, per-limb pivots and new voxel identities.
  [Test] public void NewBeePaletteHasFourGraspLimbsAndTwoWingHinges(){
   foreach(var color in new[]{"Red","Orange","Yellow","Green","Cyan","Blue","Purple","White","Black"}){
    var bee=Resources.Load<GameObject>("Prefabs/Gameplay/Bee"+color);Assert.IsNotNull(bee,color);
    Assert.AreEqual(4,bee.GetComponentsInChildren<Transform>(true).Count(t=>t.name.Contains("_Limb_")));
    Assert.AreEqual(2,bee.GetComponentsInChildren<Transform>(true).Count(t=>t.name.EndsWith("Wing_Left")||t.name.EndsWith("Wing_Right")));
    Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/Hive"+color));
   }
  }
  [Test] public void LevelLoadsEveryNewFlowerCubeWithMatchingQuotas(){
   var data=new LevelOneAssets().Level();Assert.AreEqual(1784,data.cubes.Length);
   Assert.IsTrue(data.cubes.All(c=>c.name.StartsWith("FlowerV2_Voxel_")));
   foreach(var group in data.cubes.GroupBy(c=>c.color))Assert.AreEqual(group.Count(),data.hives.Where(h=>h.color==group.Key).Sum(h=>h.quota));
  }
 }
}