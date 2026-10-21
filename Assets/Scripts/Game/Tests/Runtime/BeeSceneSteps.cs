using System;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 // Actual scene stepping: render every logical frame so arrival acknowledgements are exercised.
 public static class BeeSceneSteps {
  public static void Advance(float seconds){
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();
   if(m.Paused||seconds<=0){app.SendCommand(new AdvanceHarvestCommand(seconds));return;}
   var board=UnityEngine.Object.FindObjectOfType<HarvestBoardView>();var bees=UnityEngine.Object.FindObjectOfType<BeeFlightView>();var hives=UnityEngine.Object.FindObjectOfType<HiveModelView>();
   while(seconds>.000001f){float dt=Mathf.Min(.01f,seconds);board?.PublishCameraView();app.SendCommand(new AdvanceHarvestCommand(dt));hives?.Sync();board?.Sync();bees?.RenderFlights();seconds-=dt;}
  }
  public static void Until(Func<bool> predicate,float limit=30){for(int i=0;i<limit*100&&!predicate();i++)Advance(.01f);Assert.IsTrue(predicate(),"Scene traffic did not reach its expected stage within "+limit+"s");}
  public static void FirstPickup(){Until(()=>BeeHarvestApp.Interface.GetModel<HarvestModel>().Flights[0].Stage==1);}
 }
}
