using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class LevelTwoReplacementTests {
  [Test] public void TwoAndThirtyFiveUseTheSwappedDuckAndCat(){
   var data=new LevelOneAssets().Level(35);
   Assert.AreEqual("Cat_Luci",data.sourceName);Assert.AreEqual("Cat_Luci",LevelOneAssets.ModelKey(35));Assert.AreEqual("Level35",LevelOneAssets.SceneName(35));var duck=new LevelOneAssets().Level(2);Assert.AreEqual("Duck",duck.sourceName);Assert.AreEqual("Duck",LevelOneAssets.ModelKey(2));Assert.AreEqual(2190,duck.cubes.Length);Assert.AreEqual(30,duck.hives.Length);
   Assert.That(data.cubes.Length,Is.InRange(2000,2500));Assert.AreEqual(2471,data.cubes.Length);Assert.AreEqual(3,data.columns);
   foreach(var color in data.cubes.GroupBy(c=>c.color))Assert.AreEqual(color.Count(),data.hives.Where(h=>h.color==color.Key).Sum(h=>h.quota));
   Assert.AreEqual(data.cubes.Length,data.cubes.Select(c=>c.name).Distinct().Count());Assert.IsTrue(data.cubes.All(c=>c.name.StartsWith("Cat_LuciV2_Voxel_")));
   for(int n=1;n<=95;n++)if(n!=35)Assert.AreNotEqual("Cat_Luci",new LevelOneAssets().Level(n).sourceName,"The replacement must be a new model");
  }
  [Test] public void CatResourceContainsEveryVoxelWithValidSharedMeshAndMaterials(){
   var prefab=Resources.Load<GameObject>("Prefabs/Gameplay/Cat_Luci");Assert.IsNotNull(prefab,"The actual runtime Resources key must load");Assert.AreEqual("Cat_Luci",prefab.name);
   var data=new LevelOneAssets().Level(35);var parts=prefab.GetComponentsInChildren<MeshRenderer>(true);Assert.AreEqual(2471,parts.Length);CollectionAssert.AreEquivalent(data.cubes.Select(c=>c.name),parts.Select(p=>p.name));
   Assert.IsTrue(parts.All(p=>p.sharedMaterial!=null&&p.GetComponent<MeshFilter>()!=null&&p.GetComponent<MeshFilter>().sharedMesh!=null));Assert.AreEqual(1,parts.Select(p=>p.GetComponent<MeshFilter>().sharedMesh).Distinct().Count());
  }
 }
}
