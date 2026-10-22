using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class BeeCollectionWaitingTests:EconomyTestFixture {
  static HarvestModel M=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Object.FindObjectOfType<HarvestBoardView>();
  static BeeFlightView Bees=>Object.FindObjectOfType<BeeFlightView>();
  static void Draw(){Object.FindObjectOfType<HiveModelView>().Sync();Board.Sync();Bees.RenderFlights();}
  static IEnumerator LoadPair(bool enabled){
   LevelSceneTestLoader.Load("Level01");yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;Board.AutoRotate=false;Board.ResetRotation();Board.PublishCameraView();
   var rules=BeeHarvestApp.Interface.GetSystem<HarvestSystem>();int first=-1,second=-1;
   for(int i=0;i<M.Cubes.Count&&first<0;i++)if(rules.IsCameraVisible(i))for(int j=i+1;j<M.Cubes.Count;j++)if(rules.IsCameraVisible(j)){var x=M.Cubes[i].Data;var y=M.Cubes[j].Data;if(new Vector3(x.x-y.x,x.y-y.y,x.z-y.z).sqrMagnitude<=1.01f){first=i;second=j;break;}}
   Assert.GreaterOrEqual(first,0);var data=JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("Levels/Level01").text);
   foreach(var c in data.cubes)c.color=c.id==first||c.id==second?"Palette17":"Palette01";
   data.hives=new[]{new HiveData{column=0,color="Palette17",quota=2},new HiveData{column=1,color="Palette01",quota=data.cubes.Length-2}};
   string json=JsonUtility.ToJson(data);json=System.Text.RegularExpressions.Regex.Replace(json,"\"dispersedCollection\":(true|false),?","");json="{\"dispersedCollection\":"+(enabled?"true":"false")+","+json.Substring(1);
   BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(JsonUtility.FromJson<LevelData>(json)));Board.PublishCameraView();BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));Draw();
   BeeSceneSteps.Until(()=>M.Flights.Any(f=>f.Stage==0&&f.ClearedApproach&&!f.CollectionGranted));
  }
  static FlightState Waiter=>M.Flights.First(f=>f.Stage==0&&f.ClearedApproach&&!f.CollectionGranted);
  static Transform Actor(FlightState f)=>GameObject.Find("BlenderBeeCarrier_"+f.Id).transform;
  static float ObserveWaiting(FlightState f){
   var origin=Actor(f).position;float range=0;var previous=origin;
   // Hold the occupied source state and advance only the waiting actor's visual clock.
   for(int i=0;i<30;i++){f.Age+=.04f;Draw();var p=Actor(f).position;range=Mathf.Max(range,Vector3.Distance(origin,p));Assert.Less(Vector3.Distance(previous,p),.13f,"Waiting flight must be slow and continuous");previous=p;Assert.IsFalse(Actor(f).Find("CarriedVoxel").gameObject.activeSelf);}
   return range;
  }
  [UnityTest] public IEnumerator WaitingBeeCruisesOutsideThenRejoinsPickupWithoutJumping(){
   yield return LoadPair(true);var f=Waiter;Assert.Greater(ObserveWaiting(f),.25f,"Waiting needs a visible slow route rather than the old tiny hover");
   var app=BeeHarvestApp.Interface;app.SendCommand(new PauseHarvestCommand(true));var frozen=Actor(f).position;BeeSceneSteps.Advance(1);Draw();Assert.AreEqual(frozen,Actor(f).position);app.SendCommand(new PauseHarvestCommand(false));
   int checkedFrames=0;var last=Actor(f).position;
   for(int i=0;i<3000&&f.Stage!=2;i++){BeeSceneSteps.Advance(.01f);if(f.Stage==2)break;var p=Actor(f).position;Assert.Less(Vector3.Distance(last,p),.241f,"Permit must not teleport the waiter to its source");last=p;checkedFrames++;}
   Assert.AreEqual(2,f.Stage);Assert.Greater(checkedFrames,10);Assert.AreEqual(M.Cubes.Count,M.Remaining+M.InTransit+M.Deposited);
   Object.FindObjectOfType<LevelOneController>().RestartLevel();Draw();Assert.IsNull(GameObject.Find("BlenderBeeCarrier_"+f.Id));
  }
  [UnityTest] public IEnumerator DisabledPolicyKeepsItsOriginalLocalHover(){
   yield return LoadPair(false);var f=Waiter;Assert.Less(ObserveWaiting(f),.20f,"Other levels retain their original hover");
  }
 }
}
