using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class HarvestRulesTests : EconomyTestFixture {
  /// <summary>Protects Blender import units when the runtime sets a presentation scale.</summary>
  [Test] public void FlowerPrefabKeepsWorldSizeWhenScaled(){var prefab=UnityEngine.Resources.Load<UnityEngine.GameObject>("Prefabs/Gameplay/Flower");var go=UnityEngine.Object.Instantiate(prefab);try{go.transform.localScale=UnityEngine.Vector3.one*1.28f;var renderers=go.GetComponentsInChildren<UnityEngine.Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);Assert.Greater(bounds.size.y,5f,"Runtime scale must not overwrite the FBX centimetre conversion");Assert.Less(bounds.size.y,7f);}finally{UnityEngine.Object.DestroyImmediate(go);}}
  /// <summary>Builds independent, manually checked fixtures.</summary>
  static LevelData One(){return new LevelData{cubes=new[]{new CubeData{id=0,name="Cube_000",color="pink",x=0,y=0,z=0}},hives=new[]{new HiveData{column=0,color="pink",quota=1}}};}
  /// <summary>Catches premature deposit or slot retention at pickup.</summary>
  [Test] public void PickupCountsTransportWithoutSpendingQuotaAgain(){
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(One()));app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds + .05f));
   var m=app.GetModel<HarvestModel>();Assert.AreEqual(0,m.Remaining);Assert.AreEqual(1,m.InTransit);Assert.AreEqual(0,m.Deposited);Assert.IsFalse(m.Won);Assert.AreEqual(-1,m.Slots[0]);
   app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.LoadedSeconds));Assert.AreEqual(1,m.Deposited);Assert.IsTrue(m.Won);
  }
  /// <summary>Retains the original no-tool full-win simulation independently of production puzzle data.</summary>
  [Test] public void LegacyFlowerFixtureCanStillDeliverAll522(){var app=BeeHarvestApp.Interface;var data=UnityEngine.JsonUtility.FromJson<LevelData>(System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath,"Scripts/Game/Tests/Fixtures/LegacyFlower.json")));app.SendCommand(new StartLevelCommand(data));for(int c=0;c<5;c++)app.SendCommand(new ActivateHiveCommand(c));for(int i=0;i<12000&&!app.GetModel<HarvestModel>().Won;i++){for(int c=0;c<5;c++)app.SendCommand(new ActivateHiveCommand(c));app.SendCommand(new AdvanceHarvestCommand(.2f));}var m=app.GetModel<HarvestModel>();Assert.AreEqual(522,m.Deposited);Assert.AreEqual(0,m.Remaining);Assert.AreEqual(0,m.InTransit);Assert.IsTrue(m.Won);}
  /// <summary>Catches duplicate callbacks and excess reservations.</summary>
  [Test] public void ManyTicksCannotDuplicateOrOverdraw(){
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(One()));app.SendCommand(new ActivateHiveCommand(0));
   for(int i=0;i<30;i++)app.SendCommand(new AdvanceHarvestCommand(.2f));
   var m=app.GetModel<HarvestModel>();Assert.AreEqual(1,m.Deposited);Assert.AreEqual(0,m.Hives[0].Remaining);Assert.AreEqual(1,m.Flights.Count);Assert.AreEqual(1,m.Remaining+m.InTransit+m.Deposited);
  }
  /// <summary>Catches sampling of fully enclosed cells.</summary>
  [Test] public void EnclosedCenterCannotBeReserved(){
   var cells=new System.Collections.Generic.List<CubeData>();int id=0;
   for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)for(int z=-1;z<=1;z++)cells.Add(new CubeData{id=id++,x=x,y=y,z=z,color=x==0&&y==0&&z==0?"yellow":"pink"});
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(new LevelData{cubes=cells.ToArray(),hives=new[]{new HiveData{column=0,color="yellow",quota=1},new HiveData{column=1,color="pink",quota=26}}}));
   app.SendCommand(new ActivateHiveCommand(0));Assert.AreEqual(0,app.GetModel<HarvestModel>().Flights.Count);Assert.AreEqual(1,app.GetModel<HarvestModel>().Hives[0].Remaining);
   app.SendCommand(new ActivateHiveCommand(1));for(int i=0;i<1500&&!app.GetModel<HarvestModel>().Won;i++)app.SendCommand(new AdvanceHarvestCommand(.2f));Assert.IsTrue(app.GetModel<HarvestModel>().Won);
  }
  /// <summary>A camera-facing cap occludes the center even after reservation.</summary>
  [Test] public void SealedCavityStaysClosedUntilCapPickup(){var cells=new System.Collections.Generic.List<CubeData>();int id=0;for(int x=-2;x<=2;x++)for(int y=-2;y<=2;y++)for(int z=-2;z<=2;z++)if(System.Math.Abs(x)==2||System.Math.Abs(y)==2||System.Math.Abs(z)==2)cells.Add(new CubeData{id=id++,x=x,y=y,z=z,color=x==0&&y==0&&z==-2?"green":"pink"});int target=id;cells.Add(new CubeData{id=id,x=0,y=0,z=0,color="yellow"});var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(new LevelData{cubes=cells.ToArray(),hives=new[]{new HiveData{column=0,color="yellow",quota=1},new HiveData{column=1,color="green",quota=1},new HiveData{column=2,color="pink",quota=97}}}));app.SendCommand(new ActivateHiveCommand(0));Assert.AreEqual(0,app.GetModel<HarvestModel>().Flights.Count);app.SendCommand(new ActivateHiveCommand(1));var m=app.GetModel<HarvestModel>();Assert.AreEqual(1,m.Flights.Count);app.SendCommand(new AdvanceHarvestCommand(.5f));Assert.AreEqual(0,m.Cubes[target].Stage,"Reserved cap must still seal cavity");app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds - .5f + .01f));Assert.IsTrue(app.GetSystem<HarvestSystem>().IsCameraVisible(target),"Removing the cap reveals its interior");Assert.AreEqual(1,m.Cubes[target].Stage,"The waiting yellow hive immediately reserves its newly exposed block");app.SendCommand(new AdvanceHarvestCommand(.8f));Assert.AreEqual(1,m.Cubes[target].Stage);}
  /// <summary>Concurrent units cannot reserve one voxel twice or exceed three tasks.</summary>
  [Test] public void ConcurrentHiveReservationsAreUnique(){var cells=new CubeData[10];for(int i=0;i<10;i++)cells[i]=new CubeData{id=i,x=i,color="pink"};var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(new LevelData{cubes=cells,hives=new[]{new HiveData{column=0,color="pink",quota=5},new HiveData{column=1,color="pink",quota=5}}}));app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new ActivateHiveCommand(1));var m=app.GetModel<HarvestModel>();var ids=new System.Collections.Generic.HashSet<int>();foreach(var f in m.Flights)Assert.IsTrue(ids.Add(f.Cube));foreach(var h in m.Hives){Assert.GreaterOrEqual(h.Busy,h.Reserved);Assert.LessOrEqual(h.Reserved,h.Busy);Assert.GreaterOrEqual(h.Remaining,0);}for(int i=0;i<600&&!m.Won;i++){app.SendCommand(new AdvanceHarvestCommand(.1f));Assert.AreEqual(10,m.Remaining+m.InTransit+m.Deposited);}Assert.IsTrue(m.Won);}
  /// <summary>Catches background or explicit pause moving a task.</summary>
  [Test] public void PausedTaskRetainsCounts(){var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(One()));app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new PauseHarvestCommand(true));app.SendCommand(new AdvanceHarvestCommand(5));var m=app.GetModel<HarvestModel>();Assert.AreEqual(1,m.Remaining);Assert.AreEqual(0,m.Deposited);app.SendCommand(new PauseHarvestCommand(false));app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+AdvanceHarvestCommand.LoadedSeconds+.01f));Assert.IsTrue(m.Won);}
  /// <summary>Catches queue skipping, full-slot false failure and paused movement.</summary>
  [Test] public void FullSlotsContinueAndCannotSkipFront(){
   var cells=new CubeData[24];var hives=new HiveData[6];
   for(int i=0;i<24;i++)cells[i]=new CubeData{id=i,color="pink",x=i};
   for(int i=0;i<6;i++){hives[i]=new HiveData{column=i%5,color="pink",quota=4};}
   var app=BeeHarvestApp.Interface;app.SendCommand(new StartLevelCommand(new LevelData{cubes=cells,hives=hives}));
   for(int c=0;c<5;c++)app.SendCommand(new ActivateHiveCommand(c));app.SendCommand(new ActivateHiveCommand(0));
   var m=app.GetModel<HarvestModel>();Assert.IsFalse(m.Hives[5].Started);Assert.IsFalse(m.Deadlocked);Assert.AreEqual(5,System.Linq.Enumerable.Count(m.Slots,s=>s>=0));Assert.AreEqual(5,m.Flights.Count,"Every occupied slot may dispatch independently");
   for(int i=0;i<3000&&!m.Won;i++){app.SendCommand(new AdvanceHarvestCommand(.1f));app.SendCommand(new ActivateHiveCommand(0));}Assert.IsTrue(m.Won);
  }
 }
}
