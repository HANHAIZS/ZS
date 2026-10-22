using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class LightweightMysteryGameplayTests : EconomyTestFixture {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static LevelOneController Level=>Object.FindObjectOfType<LevelOneController>();
  static HarvestBoardView Board=>Level.GetComponent<HarvestBoardView>();
  static BeeFlightView Bees=>Level.GetComponent<BeeFlightView>();
  static HiveModelView Hives=>Level.GetComponent<HiveModelView>();
  static IEnumerator Load(int n){Assert.IsTrue(LevelSceneTestLoader.CanLoad("Level"+n));LevelSceneTestLoader.Load("Level"+n);yield return null;yield return null;Level.enabled=false;Board.AutoRotate=false;Board.ResetRotation();Board.PublishCameraView();}
  static FlightState Select(){var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();var root=GameObject.Find(LevelOneAssets.ModelKey(Level.LevelNumber)+"(Clone)");for(int turn=0;turn<8;turn++){Board.PublishCameraView();for(int c=0;c<3;c++){int h=rules.Front(c);if(h>=0&&rules.CanSelectHive(h)&&rules.FindTarget(Model.Hives[h].Data.color)>=0){app.SendCommand(new ActivateHiveCommand(c));Hives.Sync();Board.Sync();Bees.RenderFlights();return Model.Flights[0];}}root.transform.Rotate(0,45,0);}Assert.Fail("A legal visible hive is required");return null;}
  static Transform Cargo(FlightState f)=>GameObject.Find("BlenderBeeCarrier_"+f.Id).transform.Find("CarriedVoxel");
  static Transform Grip(Transform cargo)=>cargo.parent.Cast<Transform>().Single(t=>t!=cargo).Find("GripAnchor");
  [UnityTest] public IEnumerator ThreeScenesUseSixStyleAndRealSourceHandoffThenRestartAndRelease(){
   Material[] previous=null;Texture cache=null;
   foreach(int n in new[]{112,113,114}){
    yield return Load(n);if(previous!=null){Assert.IsTrue(previous.All(m=>m==null));Assert.IsTrue(cache==null);}
    var root=GameObject.Find(LevelOneAssets.ModelKey(n)+"(Clone)");var renderers=root.GetComponentsInChildren<Renderer>();var original=Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.ModelKey(n)).GetComponentsInChildren<Renderer>().ToDictionary(r=>r.name);var outline=Resources.Load<Material>("Materials/ChickVoxelOutline");
    Assert.AreEqual(Model.Cubes.Count,renderers.Length);Assert.AreEqual(3,Model.ColumnCount);Assert.AreEqual(5,Model.Slots.Count);Assert.AreEqual("Level "+n,Level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);Assert.Less(Vector3.Distance(Vector3.one*(.86f*1.04f),root.transform.localScale),.00001f);
    foreach(var r in renderers){var src=original[r.name];Assert.AreEqual(src.transform.localPosition,r.transform.localPosition);Assert.AreSame(src.GetComponent<MeshFilter>().sharedMesh,r.GetComponent<MeshFilter>().sharedMesh);Assert.Less(Vector3.Distance(src.transform.localScale*(.93f/.975f),r.transform.localScale),.00001f);Assert.AreEqual("BeeHarvest/VoxelCavity",r.sharedMaterial.shader.name);Assert.AreEqual(outline.GetColor("_EdgeColor"),r.sharedMaterial.GetColor("_EdgeColor"));Assert.AreEqual(outline.GetFloat("_EdgeWidth"),r.sharedMaterial.GetFloat("_EdgeWidth"));Assert.Less(Vector4.Distance(src.sharedMaterial.GetColor("_BaseColor"),r.sharedMaterial.GetColor("_BaseColor")),.00001f);}
    previous=renderers.Select(r=>r.sharedMaterial).Distinct().ToArray();cache=previous[0].GetTexture("_CavityData");Assert.IsNotNull(cache);Assert.AreEqual(Model.Cubes.Select(c=>c.Data.color).Distinct().Count(),previous.Length);
    var light=GameObject.Find("KeyLight").GetComponent<Light>();Assert.AreEqual(LightShadows.Soft,light.shadows);Assert.AreEqual(.3f,light.shadowStrength);Assert.AreEqual(.04f,light.shadowBias);Assert.AreEqual(.2f,light.shadowNormalBias);Assert.Less(Vector4.Distance(new Color(.48f,.52f,.63f),RenderSettings.ambientLight),.00001f);
    Assert.IsNotNull(Level.Hud.Boosters.PurchasePopup);Assert.IsNotNull(Object.FindObjectOfType<GameAudioController>());Assert.IsNotNull(Object.FindObjectOfType<VictoryPopupController>(true).Celebration);
    var flight=Select();var source=renderers.Single(r=>r.name==Model.Cubes[flight.Cube].Data.name);var from=source.transform.position;var rotation=source.transform.rotation;var size=source.transform.lossyScale;BeeSceneSteps.FirstPickup();var cargo=Cargo(flight);var renderer=cargo.GetComponent<Renderer>();
    Assert.IsFalse(source.enabled);Assert.Less(Vector3.Distance(from,cargo.position),.00001f,"New scenes must inherit level six's true source-depth handoff");Assert.Less(Quaternion.Angle(rotation,cargo.rotation),.001f);Assert.Less(Vector3.Distance(size,cargo.lossyScale),.00001f);Assert.AreSame(source.sharedMaterial,renderer.sharedMaterial);
    float started=flight.Age;BeeSceneSteps.Advance(.03f);float e=Mathf.SmoothStep(0,1,Mathf.Clamp01((flight.Age-started)/.09f));Assert.Less(Vector3.Distance(Vector3.Lerp(from,Grip(cargo).position,e),cargo.position),.0001f);
    var app=BeeHarvestApp.Interface;app.SendCommand(new PauseHarvestCommand(true));var paused=cargo.position;float age=flight.Age;yield return new WaitForSeconds(.12f);BeeSceneSteps.Advance(1);Assert.AreEqual(age,flight.Age);Assert.AreEqual(paused,cargo.position);app.SendCommand(new PauseHarvestCommand(false));BeeSceneSteps.Advance(.061f);Assert.AreEqual("BeeHarvest/VoxelEdges",renderer.sharedMaterial.shader.name);Assert.AreEqual(outline.GetColor("_EdgeColor"),renderer.sharedMaterial.GetColor("_EdgeColor"));Assert.IsFalse(renderer.HasPropertyBlock());Assert.Less(Vector3.Distance(Grip(cargo).position,cargo.position),.00001f);BeeSceneSteps.Until(()=>Model.Deposited>0,40);
    Level.RestartLevel();Board.Sync();Bees.RenderFlights();Assert.AreEqual(renderers.Length,Model.Remaining);Assert.AreEqual(0,Model.InTransit);Assert.AreEqual(0,Model.Deposited);Assert.IsTrue(renderers.All(r=>r.enabled));Assert.AreSame(cache,previous[0].GetTexture("_CavityData"));
   }
   LevelSceneTestLoader.Load("Level08");yield return null;yield return null;Assert.IsTrue(previous.All(m=>m==null));Assert.IsTrue(cache==null);
  }
  [UnityTest] public IEnumerator OriginalMysteriesHideQuotasAndRevealAtTheFrontInAllThreeScenes(){
   foreach(int n in new[]{112,113,114}){
    yield return Load(n);var app=BeeHarvestApp.Interface;int id=Level.Hud.DisplayedHive(0,1);Assert.IsTrue(Model.Hives[id].Data.isHidden);var marker=GameObject.Find("MysteryHive_"+id).GetComponent<Text>();Assert.AreEqual("?",marker.text);Assert.AreEqual(0,Level.Hud.NextLabels[0].canvasRenderer.GetAlpha(),.001f);var rs=GameObject.Find("SmallHive_"+id).GetComponentsInChildren<Renderer>();var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);Assert.Less(Vector2.Distance(Camera.main.WorldToScreenPoint(bounds.center),marker.rectTransform.position),1);
    app.SendCommand(new ActivateHiveCommand(0));Level.Hud.Sync();Hives.Sync();Assert.AreEqual(id,app.GetSystem<HarvestSystem>().Front(0));Hives.TickMysteries(.15f);float alpha=Level.Hud.QueueLabels[0].canvasRenderer.GetAlpha();Assert.Greater(alpha,0);Assert.Less(alpha,1);app.SendCommand(new PauseHarvestCommand(true));Hives.TickMysteries(1);Assert.AreEqual(alpha,Level.Hud.QueueLabels[0].canvasRenderer.GetAlpha());app.SendCommand(new PauseHarvestCommand(false));Hives.TickMysteries(.16f);Assert.AreEqual(1,Level.Hud.QueueLabels[0].canvasRenderer.GetAlpha(),.001f);Assert.IsFalse(marker.gameObject.activeSelf);Assert.Greater(Model.Hives[id].Remaining,0);Assert.IsFalse(Model.Hives[id].Started);
    Level.RestartLevel();Level.Hud.Sync();Hives.Sync();Assert.AreEqual(0,Level.Hud.NextLabels[0].canvasRenderer.GetAlpha(),.001f);Assert.IsTrue(GameObject.Find("MysteryHive_"+id).activeSelf);
   }
  }
 }
}
