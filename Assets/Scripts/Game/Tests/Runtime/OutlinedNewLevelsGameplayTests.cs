using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class OutlinedNewLevelsGameplayTests : EconomyTestFixture {
  static readonly string[] Names={"Pigeon_2","Cola","iceberg","Wheelbarrow","Medieval_church","Ladybug","Maple","Willys","Scorpion","mouse_2"};
  static readonly int[] Counts={1291,1685,1732,1804,2402,2591,2648,2660,2876,2913};
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  [UnityTest] public IEnumerator EveryNewSceneLoadsOutlinedOriginalMeshWithCurrentUiAndTransportsThenRestarts(){
   for(int i=0;i<10;i++){
    int n=i+96;string scene="Level"+n;Assert.IsTrue(Application.CanStreamedLevelBeLoaded(scene),scene);SceneManager.LoadScene(scene);yield return null;yield return null;
    var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var board=level.GetComponent<HarvestBoardView>();board.AutoRotate=false;board.ResetRotation();var root=GameObject.Find(Names[i]+"(Clone)");Assert.IsNotNull(root);Assert.AreEqual("Level "+n,level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);Assert.AreEqual(Counts[i],Model.Cubes.Count);Assert.AreEqual(3,Model.ColumnCount);Assert.AreEqual(5,Model.Slots.Count);
    var renderers=root.GetComponentsInChildren<Renderer>();Assert.AreEqual(Counts[i],renderers.Length);Assert.IsTrue(renderers.All(r=>r.sharedMaterial.shader.name=="BeeHarvest/VoxelEdges"));Assert.AreEqual(Model.Cubes.Select(c=>c.Data.color).Distinct().Count(),renderers.Select(r=>r.sharedMaterial).Distinct().Count());
    Assert.IsNotNull(level.Hud.Boosters.PurchasePopup);Assert.IsNotNull(Object.FindObjectOfType<GameAudioController>());Assert.IsNotNull(Object.FindObjectOfType<VictoryPopupController>(true).Celebration);
    var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();int selected=-1;for(int turn=0;turn<8&&selected<0;turn++){board.PublishCameraView();for(int c=0;c<3;c++){int h=rules.Front(c);if(h>=0&&rules.CanSelectHive(h)&&rules.FindTarget(Model.Hives[h].Data.color)>=0){selected=c;break;}}if(selected<0)root.transform.Rotate(0,45,0);}
    Assert.GreaterOrEqual(selected,0);app.SendCommand(new ActivateHiveCommand(selected));level.GetComponent<HiveModelView>().Sync();board.Sync();level.GetComponent<BeeFlightView>().RenderFlights();BeeSceneSteps.FirstPickup();var f=Model.Flights[0];var cargo=GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel").GetComponent<Renderer>();Assert.AreEqual("BeeHarvest/VoxelEdges",cargo.sharedMaterial.shader.name);Assert.IsFalse(root.GetComponentsInChildren<Renderer>().Single(r=>r.name==Model.Cubes[f.Cube].Data.name).enabled);BeeSceneSteps.Until(()=>Model.Deposited>0,40);Assert.AreEqual(Counts[i],Model.Remaining+Model.InTransit+Model.Deposited);
    level.RestartLevel();level.Hud.Sync();board.Sync();Assert.AreEqual(Counts[i],Model.Remaining);Assert.AreEqual(0,Model.Deposited);Assert.AreEqual(Counts[i],renderers.Count(r=>r.enabled));Assert.AreEqual(n,level.LevelNumber);
   }
  }
 }
}
