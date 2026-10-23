using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class SelectedPresentationProfileTests : EconomyTestFixture {
  [Test] public void EverySelectedLevelCombinesCompactLayoutSoftEdgesLightingAndSlowerGraspWithNinetyMillisecondHandoff(){
   var repo=new LevelOneAssets();var app=BeeHarvestApp.Interface;
   foreach(int n in Enumerable.Range(1,6).Concat(Enumerable.Range(96,19))){
    var p=LevelOneAssets.Presentation(n);var data=repo.Level(n);app.SendCommand(new StartLevelCommand(data));var m=app.GetModel<HarvestModel>();
    Assert.AreEqual(.26f,m.Timing.DepartureSeconds,"Departure at "+n);Assert.AreEqual(.18f,m.Timing.SettleSeconds);Assert.AreEqual(.16f,m.Timing.ReachSeconds);Assert.AreEqual(.22f,m.Timing.LiftSeconds);Assert.AreEqual(.09f,m.Timing.HandoffSeconds);
    Assert.IsTrue(p.edges&&p.separation&&p.cavity&&p.softShadows&&p.sourceHandoff);Assert.IsTrue(p.overrideEdgeColor);Assert.AreEqual(new Color(.2f,.2f,.2f,1),p.edgeColor);Assert.AreEqual(new Color(.48f,.52f,.63f,1),p.ambientLight);Assert.AreEqual(.3f,p.shadowStrength);Assert.AreEqual(.04f,p.shadowBias);Assert.AreEqual(.2f,p.shadowNormalBias);
    Assert.AreEqual(.24f,p.queueColumnPitch);Assert.AreEqual(.16f,p.slotColumnPitch);Assert.AreEqual(-.048f,p.slotYOffset);Assert.AreEqual(.274f,p.queueFrontY);Assert.AreEqual(.07f,p.queueRowPitch);Assert.AreEqual(.64f,p.modelCenterY);Assert.Greater(p.modelScaleMultiplier,1);Assert.LessOrEqual(p.modelScaleMultiplier,1.28f);Assert.AreEqual(n<=6,m.DispersedCollection,"Retain the existing target assignment policy");
   }
  }
 }
}
