using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class LevelSixQueueOrderPlayTests:EconomyTestFixture {
  [UnityTest] public IEnumerator ContinuousReplacementFinishesWithLessWaitingThanTheAllWhiteOpening(){
   var areas=new List<float>();var reports=new List<string>();
   foreach(bool legacy in new[]{true,false}){
    LevelSceneTestLoader.Load("Level06");yield return null;yield return null;
    Object.FindObjectOfType<LevelOneController>().enabled=false;var board=Object.FindObjectOfType<HarvestBoardView>();board.AutoRotate=true;board.ResetRotation();
    var app=BeeHarvestApp.Interface;var data=JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("Levels/Level06").text);
    if(legacy){var order=new[]{1,7,5,10,13,2,8,6,15,9,3,4,14,11,12};data.hives=order.Select(id=>data.hives.Single(h=>h.sourceId==id)).ToArray();}
    app.SendCommand(new StartLevelCommand(data));board.PublishCameraView();var m=app.GetModel<HarvestModel>();var rules=app.GetSystem<HarvestSystem>();
    float area=0;int peak=0;float finish=-1;
    for(int step=0;step<6000;step++){
     for(int c=0;c<3;c++)if(m.Slots.Any(slot=>slot<0)&&rules.CanSelectHive(rules.Front(c)))app.SendCommand(new ActivateHiveCommand(c));
     board.TickAutoRotation(.02f);BeeSceneSteps.Advance(.02f);int waiting=m.Flights.Count(f=>f.Stage==0&&f.ClearedApproach&&!f.CollectionGranted);area+=waiting*.02f;peak=Mathf.Max(peak,waiting);
     Assert.AreEqual(870,m.Remaining+m.InTransit+m.Deposited);Assert.IsFalse(m.Deadlocked,"Queue must remain playable; legacy="+legacy);
     if(step%500==499)reports.Add("legacy="+legacy+" time="+((step+1)*.02f)+" waiting="+waiting+" dispatched="+m.Flights.Count+" deposited="+m.Deposited);
     if(m.Won){finish=(step+1)*.02f;break;}
    }
    reports.Add("legacy="+legacy+" queueSeconds="+area+" peak="+peak+" finish="+finish);areas.Add(area);Assert.IsTrue(m.Won,"Continuous replacement must finish; legacy="+legacy+"\n"+string.Join("\n",reports));
   }
   string report=string.Join("\n",reports);TestContext.WriteLine(report);
   Assert.Less(areas[1],areas[0],"Reordering should reduce total waiting under identical continuous replacement");
  }
 }
}
