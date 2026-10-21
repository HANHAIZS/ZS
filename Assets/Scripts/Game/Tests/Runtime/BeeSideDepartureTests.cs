using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class BeeSideDepartureTests:EconomyTestFixture {
  static HarvestModel M=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HarvestBoardView Board=>Object.FindObjectOfType<HarvestBoardView>();
  static Transform Shape=>GameObject.Find(LevelOneAssets.ModelKey(Object.FindObjectOfType<LevelOneController>().LevelNumber)+"(Clone)").transform;
  static IEnumerator Load(string scene){SceneManager.LoadScene(scene);yield return null;yield return null;Object.FindObjectOfType<LevelOneController>().enabled=false;Board.AutoRotate=false;Board.ResetRotation();Board.PublishCameraView();}
  static Bounds Silhouette(){
   var result=new Bounds();bool first=true;
   foreach(var r in Shape.GetComponentsInChildren<Renderer>())for(int i=0;i<8;i++){
    var p=Camera.main.WorldToViewportPoint(r.bounds.center+Vector3.Scale(r.bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));
    if(first){result=new Bounds(p,Vector3.zero);first=false;}else result.Encapsulate(p);
   }return result;
  }
  static void AssertSides(){
   var shape=Silhouette();var target=Board.PickupPose(M.Cubes.Count/2).position;
   for(int task=0;task<6;task++){
    var left=Camera.main.WorldToViewportPoint(Board.ApproachPoint(target,0,task));var right=Camera.main.WorldToViewportPoint(Board.ApproachPoint(target,1,task));
    Assert.Less(left.x,shape.min.x-.025f,"Left outbound lane must clear the complete model silhouette");
    Assert.Greater(right.x,shape.max.x+.025f,"Right outbound lane must clear the complete model silhouette");
    Assert.Greater(left.x,.025f);Assert.Less(right.x,.975f);
   }
  }
  // A nearby offset at the target still crosses the model center; assert against independent renderer bounds.
  [UnityTest] public IEnumerator FlowerOutboundStagingClearsBothSidesOfTheModel(){yield return Load("Level01");AssertSides();}
  [UnityTest] public IEnumerator SecondLevelOutboundStagingTracksBothSidesAfterRotation(){yield return Load("Level02");AssertSides();Shape.rotation=Quaternion.Euler(0,75,0);Board.PublishCameraView();AssertSides();}
  static void CheckUpwardEmergence(){
   BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));
   Object.FindObjectOfType<HiveModelView>().Sync();Board.Sync();Object.FindObjectOfType<BeeFlightView>().RenderFlights();
   var f=M.Flights[0];var actor=GameObject.Find("BlenderBeeCarrier_0").transform;var origin=Camera.main.WorldToViewportPoint(actor.position);
   // Check the actual first movement from the mouth, before any horizontal branch is allowed.
   for(int i=0;i<10;i++){
    BeeSceneSteps.Advance(.01f);var p=Camera.main.WorldToViewportPoint(actor.position);
    Assert.AreEqual(origin.x,p.x,.001f,"The bee must rise from its hive mouth before moving sideways");
    Assert.GreaterOrEqual(p.y,origin.y-.001f);
   }
   Assert.Greater(Camera.main.WorldToViewportPoint(actor.position).y-origin.y,.025f,"Upward emergence must be visibly separate from the side route");
   BeeSceneSteps.Until(()=>Mathf.Abs(Camera.main.WorldToViewportPoint(actor.position).x-origin.x)>.01f);
   Assert.Greater(Camera.main.WorldToViewportPoint(actor.position).y-origin.y,.035f,"No diagonal shortcut before clearing the hive");
   BeeSceneSteps.Until(()=>f.Stage==2);Assert.Greater(M.Deposited,0);
  }
  [UnityTest] public IEnumerator FlowerBeeRisesOutOfTheMouthBeforeBranchingToItsSide(){yield return Load("Level01");CheckUpwardEmergence();}
  [UnityTest] public IEnumerator SecondLevelBeeRisesOutOfTheMouthBeforeBranchingToItsSide(){yield return Load("Level02");CheckUpwardEmergence();}
  static void CheckRisingFan(){
   BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));
   Object.FindObjectOfType<HiveModelView>().Sync();Board.Sync();Object.FindObjectOfType<BeeFlightView>().RenderFlights();
   var f=M.Flights[0];var actor=GameObject.Find("BlenderBeeCarrier_0").transform;var mouth=Camera.main.WorldToViewportPoint(actor.position);
   var silhouette=Silhouette();var side=Camera.main.WorldToViewportPoint(Board.ApproachPoint(Board.PickupPose(f.Cube).position,f.Lane,f.Id));
   Assert.Greater(Mathf.Abs(side.x-mouth.x),.05f,"Fixture must exercise a substantial sideways fan");
   BeeSceneSteps.Advance(.17f);float rise=Camera.main.WorldToViewportPoint(actor.position).y;
   BeeSceneSteps.Until(()=>Mathf.Abs(Camera.main.WorldToViewportPoint(actor.position).x-mouth.x)>=Mathf.Abs(side.x-mouth.x)*.5f);
   var halfFan=Camera.main.WorldToViewportPoint(actor.position);
   Assert.Greater(halfFan.y,rise+(silhouette.min.y-rise)*.2f,"The fan must continue gaining height instead of crossing a horizontal shelf");
   BeeSceneSteps.Until(()=>f.Stage==2);Assert.Greater(M.Deposited,0);
  }
  [UnityTest] public IEnumerator FlowerDepartureFansUpwardInsteadOfCrossingAHorizontalShelf(){yield return Load("Level01");CheckRisingFan();}
  [UnityTest] public IEnumerator SecondLevelDepartureFansUpwardInsteadOfCrossingAHorizontalShelf(){yield return Load("Level02");CheckRisingFan();}
  // A time-only turn can skip its lower corner when capped speed lags. Check the actual rendered roots, not curve samples.
  [UnityTest] public IEnumerator DepartingBeesStayBesideTheModelUntilTheirFinalApproach(){
   yield return Load("Level01");var app=BeeHarvestApp.Interface;app.SendCommand(new ActivateHiveCommand(0));BeeSceneSteps.Advance(.01f);
   var bounds=Silhouette();int checks=0;var previous=new System.Collections.Generic.Dictionary<int,Vector3>();
   for(int frame=0;frame<650;frame++){
    BeeSceneSteps.Advance(.01f);
    foreach(var f in M.Flights){
     if(f.Stage!=0)continue;var actor=GameObject.Find("BlenderBeeCarrier_"+f.Id).transform;Vector3 last;
     if(previous.TryGetValue(f.Id,out last))Assert.LessOrEqual(Vector3.Distance(last,actor.position),.131f,"Outbound root must remain continuous");previous[f.Id]=actor.position;
     if(f.ClearedApproach)continue;var p=Camera.main.WorldToViewportPoint(actor.position);
     if(p.y<bounds.min.y+.015f)continue;
     if(f.Lane==0)Assert.Less(p.x,bounds.min.x-.015f,"Left bee climbed through the model before reaching its side");
     else Assert.Greater(p.x,bounds.max.x+.015f,"Right bee climbed through the model before reaching its side");checks++;
    }
   }
   Assert.Greater(checks,25,"Exercise real outbound side travel");Assert.Greater(M.Deposited,3,"Side routes must still reach pickup and delivery");
  }
 }
}
