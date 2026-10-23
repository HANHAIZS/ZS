using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class FirstLevelProgressionTests:EconomyTestFixture {
  [TestCase(0)][TestCase(2)] public void FiveSlotsAndNoItemsCompleteTheWholeFirstLevel(int firstColumn){
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();var rules=app.GetSystem<HarvestSystem>();
   int owner=(int)typeof(HarvestModel).GetField("FlightPresentationOwner",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(m);app.SendCommand(new DetachBeeFlightPresentationCommand(owner));
   var data=JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("Levels/Level01").text);app.SendCommand(new StartLevelCommand(data));int last=-1,stalled=0;
   for(int step=0;step<5000&&!m.Won;step++){
    var view=Quaternion.Euler(0,-step*3.6f,0)*Vector3.back;app.SendCommand(new UpdateHarvestViewCommand(view,view*1000));
    for(int i=0;i<3;i++){int column=firstColumn==0?i:2-i;if(rules.CanSelectHive(rules.Front(column)))app.SendCommand(new ActivateHiveCommand(column));}
    app.SendCommand(new AdvanceHarvestCommand(.36f));Assert.AreEqual(1784,m.Remaining+m.InTransit+m.Deposited);
    if(m.Deposited==last)stalled++;else{last=m.Deposited;stalled=0;}if(stalled>400)break;
   }
   Assert.IsTrue(m.Won,"First level stalled with only "+m.Deposited+"/1784 deposited; started="+m.Hives.Count(h=>h.Started));Assert.AreEqual(1784,m.Deposited);Assert.AreEqual(5,m.Slots.Count);for(int i=0;i<4;i++)Assert.AreEqual(1,m.BoosterCount(i),"Never spend an item to pass level one");Assert.AreEqual(0,m.SpeedSeconds);Assert.IsFalse(m.FreeSelection);
  }
  [Test] public void FirstLevelRetainsTheModelAndQuotasWithoutIntroductoryChains(){
   var data=JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("Levels/Level01").text);Assert.AreEqual(1784,data.cubes.Length);Assert.AreEqual(30,data.hives.Length);Assert.AreEqual(3,data.columns);Assert.AreEqual(8,data.cubes.Select(c=>c.color).Distinct().Count());Assert.IsTrue(data.hives.All(h=>h.connected==null||h.connected.Length==0));Assert.AreEqual(30,data.hives.Select(h=>h.sourceId).Distinct().Count());
   foreach(var color in data.cubes.GroupBy(c=>c.color))Assert.AreEqual(color.Count(),data.hives.Where(h=>h.color==color.Key).Sum(h=>h.quota));
  }
 }
}
