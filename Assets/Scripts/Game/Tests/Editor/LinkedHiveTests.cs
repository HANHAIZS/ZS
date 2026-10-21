using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class LinkedHiveTests {
  [Serializable] sealed class SourceHive {public int column,quota;public string color;public int[] connected;}
  [Serializable] sealed class SourceLevel {public CubeData[] cubes;public SourceHive[] hives;}
  static LevelData Data(int[] columns,int[][] links){
   var h=columns.Select((c,i)=>new SourceHive{column=c,color="Red",quota=3,connected=links[i]}).ToArray();
   var cubes=Enumerable.Range(0,h.Length*3).Select(i=>new CubeData{id=i,x=i,color="Red"}).ToArray();
   return JsonUtility.FromJson<LevelData>(JsonUtility.ToJson(new SourceLevel{cubes=cubes,hives=h}));
  }
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static void Start(LevelData d){BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(d));}
  [Test] public void EitherLinkedFrontCommitsWholeGroupToDistinctSlots(){
   Start(Data(new[]{0,1},new[]{new[]{1},new[]{0}}));
   BeeHarvestApp.Interface.SendCommand(new ActivateWaitingHiveCommand(1,3));
   Assert.AreEqual(2,Model.Hives.Count(h=>h.Started));Assert.AreEqual(1,Model.Slots[3]);Assert.AreEqual(0,Model.Slots[0]);
   Assert.AreEqual(0,Model.WaitingColumn(0).Count);Assert.AreEqual(0,Model.WaitingColumn(1).Count);
   int revision=Model.Revision;BeeHarvestApp.Interface.SendCommand(new ActivateWaitingHiveCommand(0,1));Assert.AreEqual(revision,Model.Revision);
  }
  [Test] public void RearPartnerBlocksWholeGroupEvenWithFreeSelection(){
   Start(Data(new[]{0,1,1},new[]{new[]{2},new int[0],new[]{0}}));var app=BeeHarvestApp.Interface;
   app.SendCommand(new RefillHarvestBoosterCommand(3));app.SendCommand(new UseFreeHiveSelectionCommand());int revision=Model.Revision;
   app.SendCommand(new ActivateWaitingHiveCommand(0,0));Assert.AreEqual(0,Model.Hives.Count(h=>h.Started));Assert.AreEqual(revision,Model.Revision);Assert.IsTrue(Model.FreeSelection);
   app.SendCommand(new ActivateWaitingHiveCommand(1,0));app.SendCommand(new ActivateWaitingHiveCommand(0,1));
   Assert.AreEqual(3,Model.Hives.Count(h=>h.Started));Assert.IsFalse(Model.FreeSelection);
  }
  [Test] public void InsufficientCapacityOrPauseNeverPartiallyMovesTheGroup(){
   Start(Data(new[]{0,1,2,3,4,2},new[]{new[]{1},new[]{0},new int[0],new int[0],new int[0],new int[0]}));var app=BeeHarvestApp.Interface;
   foreach(int id in new[]{2,3,4,5})app.SendCommand(new ActivateWaitingHiveCommand(id,Array.IndexOf(new[]{2,3,4,5},id)));
   int revision=Model.Revision;app.SendCommand(new ActivateWaitingHiveCommand(0,4));Assert.AreEqual(revision,Model.Revision);Assert.IsFalse(Model.Hives[0].Started);Assert.IsFalse(Model.Hives[1].Started);
   Start(Data(new[]{0,1},new[]{new[]{1},new[]{0}}));app.SendCommand(new PauseHarvestCommand(true));revision=Model.Revision;
   app.SendCommand(new ActivateWaitingHiveCommand(0,0));Assert.AreEqual(revision,Model.Revision);Assert.IsTrue(Model.Hives.All(h=>!h.Started));
  }
  [Test] public void InvalidLinkIsRejectedBeforeReplacingCurrentLevel(){
   Start(Data(new[]{0,1},new[]{new int[0],new int[0]}));int generation=Model.Generation;
   Assert.Throws<ArgumentException>(()=>Start(Data(new[]{0,1},new[]{new[]{99},new int[0]})));Assert.AreEqual(generation,Model.Generation);
   Assert.Throws<ArgumentException>(()=>Start(Data(new[]{0,0},new[]{new[]{1},new[]{0}})));Assert.AreEqual(generation,Model.Generation);
  }
  [Test] public void ShuffleKeepsLinkedPartnersInDifferentColumnsAndFrontOrderCompatible(){
   var app=BeeHarvestApp.Interface;
   for(int seed=0;seed<20;seed++){
    Start(Data(new[]{0,1,0,1,2,2},new[]{new[]{1},new[]{0},new[]{3},new[]{2},new int[0],new int[0]}));
    app.SendCommand(new RefillHarvestBoosterCommand(2));app.SendCommand(new ShuffleWaitingHivesCommand(seed));var rules=app.GetSystem<HarvestSystem>();
    Assert.AreNotEqual(rules.WaitingColumnOf(0),rules.WaitingColumnOf(1));Assert.AreNotEqual(rules.WaitingColumnOf(2),rules.WaitingColumnOf(3));
    bool first=rules.Front(0)==0&&rules.Front(1)==1;bool second=rules.Front(0)==2&&rules.Front(1)==3;
    Assert.IsTrue(first||second,"Group order must remain aligned across the two linked columns");Assert.AreEqual(6,Enumerable.Range(0,5).Sum(c=>Model.WaitingColumn(c).Count));
   }
  }
  [Test] public void LandingKeepsEveryPlannedSlotWhenAnEarlierSlotBecomesFree(){
   var data=Data(new[]{0,1,2},new[]{new int[0],new[]{2},new[]{1}});data.hives[0].quota=2;data.hives[1].quota=4;
   Start(data);var app=BeeHarvestApp.Interface;app.SendCommand(new ActivateWaitingHiveCommand(0,0));
   for(int i=0;i<200&&Model.Slots[0]>=0;i++)app.SendCommand(new AdvanceHarvestCommand(.1f));Assert.AreEqual(-1,Model.Slots[0]);
   var constructor=typeof(ActivateWaitingHiveCommand).GetConstructor(new[]{typeof(int),typeof(int),typeof(int[])});
   Assert.IsNotNull(constructor,"Landing must accept all previously planned destinations");
   var command=(ActivateWaitingHiveCommand)constructor.Invoke(new object[]{1,1,new[]{1,2}});app.SendCommand(command);
   Assert.AreEqual(1,Model.Slots[1]);Assert.AreEqual(2,Model.Slots[2]);Assert.AreEqual(-1,Model.Slots[0]);
  }
  [Test] public void ProductionLevelsMatchFlowerAndTheSecondLevelDuck(){
   var assets=new LevelOneAssets();var flower=assets.Level(1);var duck=assets.Level(2);
   Assert.AreEqual(1784,flower.cubes.Length);Assert.AreEqual(30,flower.hives.Length);Assert.AreEqual(2190,duck.cubes.Length);Assert.AreEqual(30,duck.hives.Length);
   Assert.AreEqual(40,flower.hives[0].quota);Assert.AreEqual("Palette17",flower.hives[0].color);
   Assert.AreEqual(80,duck.hives[0].quota);Assert.AreEqual("Palette12",duck.hives[0].color);
  }
 }
}
