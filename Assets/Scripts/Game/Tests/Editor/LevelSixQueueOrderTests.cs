using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class LevelSixQueueOrderTests {
  static LevelData Data()=>JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("Levels/Level06").text);
  [Test] public void OpeningFiveHivesMixColorsInsteadOfFiveWhiteHives(){
   var data=Data();var columns=Enumerable.Range(0,3).Select(c=>data.hives.Where(h=>h.column==c).ToArray()).ToArray();
   Assert.AreEqual(3,columns.Select(q=>q[0].color).Distinct().Count());var firstFive=new[]{columns[0][0],columns[1][0],columns[2][0],columns[0][1],columns[1][1]};
   Assert.LessOrEqual(firstFive.Count(h=>h.color=="Palette13"),2);
  }
  [Test] public void EveryOriginalHiveAndItsMysteryDepthArePreserved(){
   var data=Data();Assert.AreEqual(15,data.hives.Length);Assert.AreEqual(15,data.hives.Select(h=>h.sourceId).Distinct().Count());
   var originalIds=new[]{1,7,5,10,13,2,8,6,15,9,3,4,14,11,12};var quotas=new[]{50,40,50,50,70,80,78,50,68,65,80,50,59,40,40};
   var colors=new[]{13,13,13,10,10,13,13,13,2,7,13,13,10,10,10};
   for(int i=0;i<15;i++){var h=data.hives.Single(v=>v.sourceId==originalIds[i]);Assert.AreEqual(quotas[i],h.quota);Assert.AreEqual(i/5,h.column);Assert.AreEqual(colors[i],h.sourceColor);Assert.AreEqual("Palette"+colors[i].ToString("00"),h.color);Assert.AreEqual((1000+i).ToString(),h.sourceRid);Assert.IsEmpty(h.connected);Assert.AreEqual(i==2||i==8||i==14,h.isHidden);}
   for(int c=0;c<3;c++){var q=data.hives.Where(h=>h.column==c).ToArray();Assert.AreEqual(c+2,System.Array.FindIndex(q,h=>h.isHidden));}
   foreach(var group in data.cubes.GroupBy(c=>c.color))Assert.AreEqual(group.Count(),data.hives.Where(h=>h.color==group.Key).Sum(h=>h.quota));
  }
 }
}
