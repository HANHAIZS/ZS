using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class HiddenTargetRecoverySceneTests:EconomyTestFixture {
  [UnityTest] public IEnumerator RotatingFlowerKeepsItsCommittedBlockAndDeliversWithoutWaitingForVisibility(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;level.RestartLevel();var board=level.GetComponent<HarvestBoardView>();board.AutoRotate=true;var bees=level.GetComponent<BeeFlightView>();var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();var rules=app.GetSystem<HarvestSystem>();app.SendCommand(new ActivateHiveCommand(0));level.GetComponent<HiveModelView>().Sync();board.Sync();bees.RenderFlights();var f=m.Flights[0];int target=f.Cube;bool occluded=false;float deliveredAt=-1;
   for(int frame=0;frame<480;frame++){
    board.TickAutoRotation(.025f);board.PublishCameraView();if(f.Stage==0&&!rules.IsCameraVisible(target))occluded=true;BeeSceneSteps.Advance(.025f);board.Sync();bees.RenderFlights();Assert.AreEqual(target,f.Cube,"Rotation cannot replace a committed cube");if(f.Stage==2&&deliveredAt<0)deliveredAt=(frame+1)*.025f;Assert.AreEqual(1784,m.Remaining+m.InTransit+m.Deposited);
   }
   Assert.IsTrue(occluded,"Exercise a real target that rotates out of sight during approach");Assert.AreEqual(2,f.Stage);Assert.Greater(deliveredAt,0);Assert.Less(deliveredAt,12f,"Moving targets must finish through the exterior route without a visibility wait");
   level.RestartLevel();app.SendCommand(new ActivateHiveCommand(0));level.GetComponent<HiveModelView>().Sync();board.Sync();bees.RenderFlights();var fresh=GameObject.Find("BlenderBeeCarrier_0").transform;Assert.Less(Vector3.Distance(board.FlightPoint(board.Source(m.Flights[0].Hive)),fresh.position),.01f);Assert.IsFalse(fresh.Find("CarriedVoxel").gameObject.activeSelf);
  }
  [UnityTest] public IEnumerator OriginalBrownWorkersFinishEachCommittedTargetAcrossAutomaticRotation(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;level.RestartLevel();var board=level.GetComponent<HarvestBoardView>();board.AutoRotate=true;var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();var rules=app.GetSystem<HarvestSystem>();// Palette04 is completely enclosed in the intact source at every initial angle. Advance real rules to expose it before recording its physical flights.
   var bees=level.GetComponent<BeeFlightView>();app.SendCommand(new DetachBeeFlightPresentationCommand(bees.GetInstanceID()));
   for(int setup=0;setup<2500;setup++){for(int c=0;c<m.ColumnCount;c++)app.SendCommand(new ActivateHiveCommand(c));board.TickAutoRotation(.5f);board.PublishCameraView();app.SendCommand(new AdvanceHarvestCommand(5));if(m.Flights.Any(f=>m.Hives[f.Hive].Data.color=="Palette04"&&f.Stage<2))break;}
   Assert.IsTrue(m.Flights.Any(f=>m.Hives[f.Hive].Data.color=="Palette04"&&f.Stage<2),"Expose an actual first brown target before beginning the physical regression");
   int completedBefore=m.Flights.Count(f=>m.Hives[f.Hive].Data.color=="Palette04"&&f.Stage==2);app.SendCommand(new AttachBeeFlightPresentationCommand(bees.GetInstanceID()));level.GetComponent<HiveModelView>().Sync();board.Sync();bees.RenderFlights();
   var born=new Dictionary<int,float>();var targets=new Dictionary<int,int>();int occluded=0,completed=0;
   for(int frame=0;frame<18000&&(completed-completedBefore<6||occluded==0);frame++){
    if(frame%100==0)yield return null;
    float now=frame*.05f;for(int c=0;c<m.ColumnCount;c++)app.SendCommand(new ActivateHiveCommand(c));board.TickAutoRotation(.05f);board.PublishCameraView();foreach(var f in m.Flights)if(f.Stage==0&&m.Hives[f.Hive].Data.color=="Palette04"&&!rules.IsCameraVisible(f.Cube))occluded++;BeeSceneSteps.Advance(.05f);
    foreach(var f in m.Flights){if(m.Hives[f.Hive].Data.color!="Palette04")continue;if(!born.ContainsKey(f.Id)){born[f.Id]=now;targets[f.Id]=f.Cube;}Assert.AreEqual(targets[f.Id],f.Cube,"Brown worker changed committed target: "+f.Id);if(now-born[f.Id]>30f)Assert.AreEqual(2,f.Stage,"Brown worker remained stuck beyond its normal flight: "+f.Id+" cube "+f.Cube);}
    Assert.AreEqual(1784,m.Remaining+m.InTransit+m.Deposited);completed=m.Flights.Count(f=>m.Hives[f.Hive].Data.color=="Palette04"&&f.Stage==2);
   }
   TestContext.WriteLine("brownBefore="+completedBefore+" after="+completed+" quota="+m.Hives[11].Remaining+" available="+m.Cubes.Count(c=>c.Stage==0&&c.Data.color=="Palette04")+" totalFlights="+m.Flights.Count+" active="+string.Join(";",m.Flights.Where(f=>f.Stage<2||!f.Returned).Select(f=>f.Id+":"+m.Hives[f.Hive].Data.color+" stage="+f.Stage+" phase="+f.TransportPhase+" t="+f.PhaseProgress+" return="+f.ReturnProgress+" flags="+f.ClearedEntry+","+f.ReturnCornerReached+","+f.Returned)));
   Assert.GreaterOrEqual(completed-completedBefore,6,"Exercise original brown hives, not just pink transport");Assert.Greater(occluded,0,"Some brown targets must rotate behind other blocks");level.Hud.Sync();level.GetComponent<HiveModelView>().Sync();board.Sync();level.GetComponent<BeeFlightView>().RenderFlights();
  }
 }
}
