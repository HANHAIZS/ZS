using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class FirstLevelCompletionTests:EconomyTestFixture {
  [UnityTest] public IEnumerator WholeFirstLevelWinsWithoutBoostersAndCanRestart(){
   LevelSceneTestLoader.Load("Level01");yield return null;yield return null;
   var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var board=Object.FindObjectOfType<HarvestBoardView>();board.AutoRotate=true;board.ResetRotation();
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();var rules=app.GetSystem<HarvestSystem>();Assert.AreEqual(1784,m.Cubes.Count);Assert.IsFalse(m.HasLinkedGroups);int last=0,stagnant=0;
   for(int step=0;step<5000&&!m.Won;step++){
    for(int c=0;c<3;c++)if(m.Slots.Any(slot=>slot<0)&&rules.CanSelectHive(rules.Front(c)))app.SendCommand(new ActivateHiveCommand(c));
    board.TickAutoRotation(.04f);BeeSceneSteps.Advance(.04f);Assert.AreEqual(1784,m.Remaining+m.InTransit+m.Deposited);
    if(m.Deposited==last)stagnant++;else{last=m.Deposited;stagnant=0;}if(stagnant>1200)break;
   }
   TestContext.WriteLine("Full first-level no-item completion: deposited="+m.Deposited+", flights="+m.Flights.Count+", remaining="+m.Remaining+", inTransit="+m.InTransit);
   Assert.IsTrue(m.Won,"Actual scene must complete 1784 blocks with no items; deposited="+m.Deposited);Assert.AreEqual(1784,m.Deposited);Assert.AreEqual(5,m.Slots.Count);for(int i=0;i<4;i++)Assert.AreEqual(1,m.BoosterCount(i));Assert.AreEqual(0,m.SpeedSeconds);
   level.RestartLevel();board.Sync();Object.FindObjectOfType<BeeFlightView>().RenderFlights();Assert.AreEqual(1784,m.Remaining);Assert.AreEqual(0,m.Deposited);Assert.IsFalse(m.Won);Assert.IsTrue(m.Slots.All(s=>s<0));Assert.IsFalse(m.HasLinkedGroups);
  }
 }
}
