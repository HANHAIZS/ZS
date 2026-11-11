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
  [UnityTest] public IEnumerator EveryNewSceneLoadsOriginalMeshWithoutOutlineWithCurrentUiAndTransportsThenRestarts(){
   for(int i=0;i<10;i++){
    int n=i+96;string scene="Level"+n;Assert.IsTrue(LevelSceneTestLoader.CanLoad(scene),scene);LevelSceneTestLoader.Load(scene);yield return null;yield return null;
    var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var board=level.GetComponent<HarvestBoardView>();board.AutoRotate=false;board.ResetRotation();var root=GameObject.Find(Names[i]+"(Clone)");Assert.IsNotNull(root);Assert.AreEqual("Level "+n,level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);Assert.AreEqual(Counts[i],Model.Cubes.Count);Assert.AreEqual(3,Model.ColumnCount);Assert.AreEqual(5,Model.Slots.Count);
    var renderers=root.GetComponentsInChildren<Renderer>();Assert.AreEqual(Counts[i],renderers.Length);Assert.IsTrue(renderers.All(r=>r.sharedMaterial.shader.name=="BeeHarvest/VoxelCavity"));Assert.AreEqual(Model.Cubes.Select(c=>c.Data.color).Distinct().Count(),renderers.Select(r=>r.sharedMaterial).Distinct().Count());
    Assert.IsNotNull(level.Hud.Boosters.PurchasePopup);Assert.IsNotNull(Object.FindObjectOfType<GameAudioController>());Assert.IsNotNull(Object.FindObjectOfType<VictoryPopupController>(true).Celebration);
    var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();int selected=-1;for(int turn=0;turn<8&&selected<0;turn++){board.PublishCameraView();for(int c=0;c<3;c++){int h=rules.Front(c);if(h>=0&&rules.CanSelectHive(h)&&rules.FindTarget(Model.Hives[h].Data.color)>=0){selected=c;break;}}if(selected<0)root.transform.Rotate(0,45,0);}
    Assert.GreaterOrEqual(selected,0);app.SendCommand(new ActivateHiveCommand(selected));level.GetComponent<HiveModelView>().Sync();board.Sync();level.GetComponent<BeeFlightView>().RenderFlights();BeeSceneSteps.FirstPickup();BeeSceneSteps.Advance(.091f);var f=Model.Flights[0];var cargo=GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel").GetComponent<Renderer>();Assert.AreEqual("Universal Render Pipeline/Lit",cargo.sharedMaterial.shader.name);Assert.IsFalse(root.GetComponentsInChildren<Renderer>().Single(r=>r.name==Model.Cubes[f.Cube].Data.name).enabled);BeeSceneSteps.Until(()=>Model.Deposited>0,40);Assert.AreEqual(Counts[i],Model.Remaining+Model.InTransit+Model.Deposited);
    level.RestartLevel();level.Hud.Sync();board.Sync();Assert.AreEqual(Counts[i],Model.Remaining);Assert.AreEqual(0,Model.Deposited);Assert.AreEqual(Counts[i],renderers.Count(r=>r.enabled));Assert.AreEqual(n,level.LevelNumber);
   }
  }
  [UnityTest] public IEnumerator ReplicatedChick106KeepsContentAndGrayCargoThenRestartsAndReleasesMaterials(){
   Assert.IsTrue(LevelSceneTestLoader.CanLoad("Level106"));LevelSceneTestLoader.Load("Level106");yield return null;yield return null;
   var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var board=level.GetComponent<HarvestBoardView>();board.AutoRotate=false;board.ResetRotation();var root=GameObject.Find("Chick(Clone)");var renderers=root.GetComponentsInChildren<Renderer>();
   Assert.AreEqual(106,level.LevelNumber);Assert.AreEqual("Level 106",level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);Assert.AreEqual(870,Model.Cubes.Count);Assert.AreEqual(3,Model.ColumnCount);Assert.AreEqual(5,Model.Slots.Count);
   var assets=new LevelOneAssets();CollectionAssert.AreEqual(assets.Level(6).cubes.Select(c=>JsonUtility.ToJson(c)),assets.Level(106).cubes.Select(c=>JsonUtility.ToJson(c)));CollectionAssert.AreEquivalent(assets.Level(6).hives.Select(h=>h.color+":"+h.quota),assets.Level(106).hives.Select(h=>h.color+":"+h.quota));Assert.AreEqual(870,renderers.Length);var materials=renderers.Select(r=>r.sharedMaterial).Distinct().ToArray();Assert.AreEqual(4,materials.Length);
   var originals=Resources.Load<GameObject>("Prefabs/Gameplay/Chick").GetComponentsInChildren<MeshFilter>().ToDictionary(f=>f.name);
   foreach(var r in renderers){Assert.AreEqual("BeeHarvest/VoxelCavity",r.sharedMaterial.shader.name);Assert.AreEqual(0f,r.sharedMaterial.GetFloat("_EdgeWidth"));Assert.AreSame(originals[r.name].sharedMesh,r.GetComponent<MeshFilter>().sharedMesh);Assert.Less(Vector4.Distance(originals[r.name].GetComponent<Renderer>().sharedMaterial.GetColor("_BaseColor"),r.sharedMaterial.GetColor("_BaseColor")),.00001f);}
   var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();int selected=-1;for(int turn=0;turn<8&&selected<0;turn++){board.PublishCameraView();for(int c=0;c<3;c++){int h=rules.Front(c);if(h>=0&&rules.CanSelectHive(h)&&rules.FindTarget(Model.Hives[h].Data.color)>=0){selected=c;break;}}if(selected<0)root.transform.Rotate(0,45,0);}
   Assert.GreaterOrEqual(selected,0);app.SendCommand(new ActivateHiveCommand(selected));level.GetComponent<HiveModelView>().Sync();board.Sync();level.GetComponent<BeeFlightView>().RenderFlights();BeeSceneSteps.FirstPickup();var cargo=GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel").GetComponent<Renderer>();Assert.AreEqual(0f,cargo.sharedMaterial.GetFloat("_EdgeWidth"));BeeSceneSteps.Until(()=>Model.Deposited>0,40);
   level.RestartLevel();level.Hud.Sync();board.Sync();Assert.AreEqual(870,Model.Remaining);Assert.AreEqual(0,Model.Deposited);Assert.AreEqual(870,renderers.Count(r=>r.enabled));CollectionAssert.AreEquivalent(materials,renderers.Select(r=>r.sharedMaterial).Distinct());
   LevelSceneTestLoader.Load("Level06");yield return null;yield return null;Assert.IsTrue(materials.All(m=>m==null));Assert.IsTrue(GameObject.Find("Chick(Clone)").GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.GetFloat("_EdgeWidth")==0f));
  }
 }
}
