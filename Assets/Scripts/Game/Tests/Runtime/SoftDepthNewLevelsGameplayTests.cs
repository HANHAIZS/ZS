using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class SoftDepthNewLevelsGameplayTests : EconomyTestFixture {
  [UnityTest] public IEnumerator FiveScenesPreserveArtAndAll106EffectsThenTransportRestartAndRelease(){
   Material[] previous=null;Texture previousCache=null;
   for(int n=107;n<=111;n++){
    Assert.IsTrue(LevelSceneTestLoader.CanLoad("Level"+n),"New scene must be in build settings");LevelSceneTestLoader.Load("Level"+n);yield return null;yield return null;
    if(previous!=null){Assert.IsTrue(previous.All(m=>m==null));Assert.IsTrue(previousCache==null,"Old scene occlusion cache is released");}
    var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var board=level.GetComponent<HarvestBoardView>();board.AutoRotate=false;board.ResetRotation();board.PublishCameraView();var root=GameObject.Find(LevelOneAssets.ModelKey(n)+"(Clone)");var rs=root.GetComponentsInChildren<Renderer>();var source=Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.ModelKey(n));var originals=source.GetComponentsInChildren<Renderer>().ToDictionary(r=>r.name);
    var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();Assert.AreEqual(model.Cubes.Count,rs.Length);Assert.AreEqual(3,model.ColumnCount);Assert.AreEqual(5,model.Slots.Count);Assert.AreEqual("Level "+n,level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);Assert.Less(Vector3.Distance(Vector3.one*(.86f*1.04f)*LevelOneAssets.Presentation(n).modelScaleMultiplier,root.transform.localScale),.00001f);
    foreach(var r in rs){var original=originals[r.name];Assert.AreEqual(original.transform.localPosition,r.transform.localPosition);Assert.AreEqual(original.transform.localRotation,r.transform.localRotation);Assert.Less(Vector3.Distance(original.transform.localScale*(.93f/.975f),r.transform.localScale),.00001f);Assert.AreSame(original.GetComponent<MeshFilter>().sharedMesh,r.GetComponent<MeshFilter>().sharedMesh);Assert.AreEqual("BeeHarvest/VoxelCavity",r.sharedMaterial.shader.name);Assert.AreEqual(.11f,r.sharedMaterial.GetFloat("_EdgeWidth"));Assert.Less(Vector4.Distance(new Color(.2f,.2f,.2f,1),r.sharedMaterial.GetColor("_EdgeColor")),.00001f);Assert.Less(Vector4.Distance(original.sharedMaterial.GetColor("_BaseColor"),r.sharedMaterial.GetColor("_BaseColor")),.00001f);Assert.IsFalse(r.HasPropertyBlock());}
    previous=rs.Select(r=>r.sharedMaterial).Distinct().ToArray();Assert.AreEqual(model.Cubes.Select(c=>c.Data.color).Distinct().Count(),previous.Length);previousCache=previous[0].GetTexture("_CavityData");Assert.IsNotNull(previousCache);Assert.IsTrue(previous.All(m=>m.GetTexture("_CavityData")==previousCache));
    var light=GameObject.Find("KeyLight").GetComponent<Light>();Assert.AreEqual(LightShadows.Soft,light.shadows);Assert.AreEqual(.3f,light.shadowStrength);Assert.AreEqual(.04f,light.shadowBias);Assert.AreEqual(.2f,light.shadowNormalBias);Assert.Less(Vector4.Distance(new Color(.48f,.52f,.63f),RenderSettings.ambientLight),.00001f);
    Assert.IsNotNull(level.Hud.Boosters.PurchasePopup);Assert.IsNotNull(Object.FindObjectOfType<GameAudioController>());Assert.IsNotNull(Object.FindObjectOfType<VictoryPopupController>(true).Celebration);
    var rules=app.GetSystem<HarvestSystem>();int selected=-1;for(int turn=0;turn<8&&selected<0;turn++){board.PublishCameraView();for(int c=0;c<3;c++){int h=rules.Front(c);if(h>=0&&rules.CanSelectHive(h)&&rules.FindTarget(model.Hives[h].Data.color)>=0){selected=c;break;}}if(selected<0)root.transform.Rotate(0,45,0);}
    Assert.GreaterOrEqual(selected,0);app.SendCommand(new ActivateHiveCommand(selected));level.GetComponent<HiveModelView>().Sync();board.Sync();level.GetComponent<BeeFlightView>().RenderFlights();BeeSceneSteps.FirstPickup();BeeSceneSteps.Advance(.091f);var f=model.Flights[0];var cargo=GameObject.Find("BlenderBeeCarrier_0").transform.Find("CarriedVoxel").GetComponent<Renderer>();Assert.AreEqual("BeeHarvest/VoxelEdges",cargo.sharedMaterial.shader.name);Assert.Less(Vector4.Distance(new Color(.2f,.2f,.2f,1),cargo.sharedMaterial.GetColor("_EdgeColor")),.00001f);var removed=rs.Single(r=>r.name==model.Cubes[f.Cube].Data.name);Assert.IsFalse(removed.enabled);Assert.Less(Vector3.Distance(removed.transform.lossyScale,cargo.transform.lossyScale),.001f);
    app.SendCommand(new PauseHarvestCommand(true));var cache=(Texture2D)previousCache;var frozen=cache.GetRawTextureData().ToArray();yield return new WaitForSeconds(.15f);CollectionAssert.AreEqual(frozen,cache.GetRawTextureData().ToArray());app.SendCommand(new PauseHarvestCommand(false));BeeSceneSteps.Until(()=>model.Deposited>0,40);Assert.AreEqual(rs.Length,model.Remaining+model.InTransit+model.Deposited);
    level.RestartLevel();board.Sync();Assert.AreEqual(rs.Length,model.Remaining);Assert.AreEqual(0,model.InTransit);Assert.AreEqual(0,model.Deposited);Assert.IsTrue(rs.All(r=>r.enabled));CollectionAssert.AreEquivalent(previous,rs.Select(r=>r.sharedMaterial).Distinct());Assert.AreSame(previousCache,previous[0].GetTexture("_CavityData"));
   }
   LevelSceneTestLoader.Load("Level08");yield return null;yield return null;Assert.IsTrue(previous.All(m=>m==null));Assert.IsTrue(previousCache==null);Assert.IsTrue(GameObject.Find("Shark(Clone)").GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterial.shader.name=="Universal Render Pipeline/Lit"));
  }
 }
}
