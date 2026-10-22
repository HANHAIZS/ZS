using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class DispersedCollectionTests:EconomyTestFixture {
  static LevelData Row(bool enabled){return JsonUtility.FromJson<LevelData>("{\"dispersedCollection\":"+(enabled?"true":"false")+",\"cubes\":[{\"id\":0,\"x\":0,\"color\":\"pink\"},{\"id\":1,\"x\":3,\"color\":\"pink\"},{\"id\":2,\"x\":12,\"color\":\"pink\"}],\"hives\":[{\"column\":0,\"color\":\"pink\",\"quota\":3}]}");}
  [TestCase(true,2)][TestCase(false,1)] public void DispatchSpreadsBeforeUsingTheFirstMerelyFreeRegion(bool enabled,int expected){
   var a=BeeHarvestApp.Interface;a.SendCommand(new StartLevelCommand(Row(enabled)));a.SendCommand(new ActivateHiveCommand(0));a.SendCommand(new AdvanceHarvestCommand(.18f));
   var m=a.GetModel<HarvestModel>();Assert.AreEqual(2,m.Flights.Count);Assert.AreEqual(expected,m.Flights[1].Cube,"Prefer the most distant visible same-color region only when enabled");
  }
  [Test] public void LoadedCarrierStillOccupiesItsSourceUntilItClears(){
   var a=BeeHarvestApp.Interface;a.SendCommand(new StartLevelCommand(Row(true)));a.SendCommand(new ActivateHiveCommand(0));var m=a.GetModel<HarvestModel>();var f=m.Flights[0];f.Stage=1;f.CollectionGranted=true;f.TransportPhase=0;
   Assert.AreEqual(2,a.GetSystem<HarvestSystem>().FindDispatchTarget("pink"));f.TransportPhase=2;f.ClearedSource=true;Assert.AreEqual(1,a.GetSystem<HarvestSystem>().FindDispatchTarget("pink"));
  }
  [Test] public void DenseOnlyTargetsStillDispatchAndFinishWithoutLosingBlocks(){
   var data=Row(true);for(int i=0;i<data.cubes.Length;i++)data.cubes[i].x=i;
   var a=BeeHarvestApp.Interface;a.SendCommand(new StartLevelCommand(data));a.SendCommand(new ActivateHiveCommand(0));var m=a.GetModel<HarvestModel>();
   for(int i=0;i<2000&&!m.Won;i++){a.SendCommand(new AdvanceHarvestCommand(.01f));Assert.AreEqual(3,m.Remaining+m.InTransit+m.Deposited);Assert.AreEqual(m.Flights.Count,m.Flights.Select(f=>f.Cube).Distinct().Count());}
   Assert.IsTrue(m.Won);Assert.AreEqual(3,m.Deposited);
  }
  [Test] public void OnlyTheFirstSixLevelsEnableTheNewPolicyAndRestartResetsIt(){
   var field=typeof(LevelData).GetField("dispersedCollection");Assert.IsNotNull(field,"The policy must be explicit per level");
   for(int n=1;n<=LevelOneAssets.LastLevel;n++){var data=JsonUtility.FromJson<LevelData>(Resources.Load<TextAsset>("Levels/"+LevelOneAssets.LevelKey(n)).text);Assert.AreEqual(n<=6,(bool)field.GetValue(data),"Level "+n);}
   var a=BeeHarvestApp.Interface;a.SendCommand(new StartLevelCommand(Row(true)));a.SendCommand(new StartLevelCommand(Row(false)));a.SendCommand(new ActivateHiveCommand(0));a.SendCommand(new AdvanceHarvestCommand(.18f));Assert.AreEqual(1,a.GetModel<HarvestModel>().Flights[1].Cube);
  }
 }
}
