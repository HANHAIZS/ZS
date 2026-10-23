using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class CompactLightweightContentTests {
  [TestCase(115,"Lute",1369,24,11)] [TestCase(116,"BustStatue",1476,19,8)] [TestCase(117,"Sundae",1508,29,8)] [TestCase(118,"GoldenBox",1331,31,0)] [TestCase(119,"HiddenBox",1331,24,21)]
  public void NewContentUsesCompleteLightweightModelsAndOriginalHiveIdentities(int n,string name,int count,int hives,int hidden){var d=new LevelOneAssets().Level(n);Assert.AreEqual(name,d.sourceName);Assert.AreEqual(count,d.cubes.Length);Assert.AreEqual(hives,d.hives.Length);Assert.AreEqual(hidden,d.hives.Count(h=>h.isHidden));Assert.AreEqual(3,d.columns);Assert.IsTrue(d.dispersedCollection);foreach(var color in d.cubes.GroupBy(c=>c.color))Assert.AreEqual(color.Count(),d.hives.Where(h=>h.color==color.Key).Sum(h=>h.quota));Assert.AreEqual(count,Resources.Load<GameObject>("Prefabs/Gameplay/"+name).GetComponentsInChildren<Renderer>().Length);Assert.IsTrue(LevelOneAssets.Presentation(n).sourceHandoff);Assert.IsTrue(LevelOneAssets.Presentation(n).cavity);}
  [TestCase("modelScaleMultiplier",0)] [TestCase("modelScaleMultiplier",2)] [TestCase("modelCenterY",1)] [TestCase("slotYOffset",.1)] [TestCase("queueRowPitch",.01)]
  public void UnsafeDisplayProfileFailsBeforeEnteringRuntimeCache(string key,double value){var root=JObject.Parse(Resources.Load<TextAsset>("Levels/PresentationCatalog").text);root["entries"][114][key]=value;Assert.Throws<System.InvalidOperationException>(()=>LevelOneAssets.ParsePresentationCatalog(root.ToString()));}
 }
}
