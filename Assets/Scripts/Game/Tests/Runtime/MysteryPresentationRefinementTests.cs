using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class MysteryPresentationRefinementTests : EconomyTestFixture {
  static LevelOneController Level=>Object.FindObjectOfType<LevelOneController>();
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static HiveModelView View=>Level.GetComponent<HiveModelView>();
  static HiveHopController Hop=>Level.GetComponent<HiveHopController>();
  static Text Marker(int id)=>GameObject.Find("MysteryHive_"+id).GetComponent<Text>();
  static IEnumerator Load(int n){SceneManager.LoadScene(LevelOneAssets.SceneName(n));yield return null;yield return null;Level.enabled=false;}
  static void Sync(){Level.Hud.Sync();View.Sync();}
  static void Centered(int id){var root=GameObject.Find("SmallHive_"+id);var renderers=root.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);Assert.Less(Vector2.Distance(Camera.main.WorldToScreenPoint(bounds.center),Marker(id).rectTransform.position),1,"Question must remain at the visual center");}
  [TearDown] public void Cancel(){if(Level!=null)Hop.Cancel();}
  [UnityTest] public IEnumerator HiddenQuotaStaysInvisibleAcrossInputOnlyRefreshAndBounce(){
   yield return Load(12);int id=Level.Hud.DisplayedHive(0,1);var label=Level.Hud.NextLabels[0];
   Assert.AreEqual(0,label.canvasRenderer.GetAlpha(),.001f);Centered(id);
   Level.Hud.SetHiveInputEnabled(false);Assert.AreEqual(0,label.canvasRenderer.GetAlpha(),.001f);
   Level.Hud.SetHiveInputEnabled(true);Hop.ClickWaiting(id);Hop.Tick(.12f);Centered(id);Assert.AreEqual(0,label.canvasRenderer.GetAlpha(),.001f);
   Hop.Tick(.23f);Centered(id);Assert.AreEqual(0,label.canvasRenderer.GetAlpha(),.001f);Assert.IsFalse(Model.Hives[id].Started);
  }
  [UnityTest] public IEnumerator DeeperMysteryHidesItsQuotaUntilActualFrontThenRestoresIt(){
   yield return Load(6);int column=1;int id=Model.WaitingColumn(column).Where(i=>Model.Hives[i].Data.isHidden).Single();
   var rules=BeeHarvestApp.Interface.GetSystem<HarvestSystem>();int depth=Model.WaitingColumn(column).ToList().IndexOf(id);Assert.AreEqual(3,depth);
   for(int step=0;step<depth;step++){
    if(step>0){var label=Level.Hud.WaitingLabel(id);Assert.AreEqual(0,label.canvasRenderer.GetAlpha(),.001f);Centered(id);}
    BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(column));Sync();
    if(step<depth-1){View.TickMysteries(.31f);Assert.AreEqual(0,Level.Hud.WaitingLabel(id).canvasRenderer.GetAlpha(),.001f);Assert.AreNotEqual(id,rules.Front(column));}
   }
   Assert.AreEqual(id,rules.Front(column));View.TickMysteries(.15f);float alpha=Level.Hud.QueueLabels[column].canvasRenderer.GetAlpha();Assert.Greater(alpha,0);Assert.Less(alpha,1);
   View.TickMysteries(.16f);Assert.AreEqual(1,Level.Hud.QueueLabels[column].canvasRenderer.GetAlpha(),.001f);Assert.IsFalse(Object.FindObjectsOfType<Text>().Any(t=>t.name=="MysteryHive_"+id));
   Assert.IsFalse(Model.Hives[id].Started);
  }
  [UnityTest] public IEnumerator ShuffleRebindingKeepsEveryUnrevealedQuotaHiddenAndCanRevealLater(){
   yield return Load(12);var app=BeeHarvestApp.Interface;int id=Level.Hud.DisplayedHive(0,1);
   app.SendCommand(new RefillHarvestBoosterCommand(2));app.SendCommand(new ShuffleWaitingHivesCommand(27));Sync();
   Assert.AreEqual(id,Level.Hud.DisplayedHive(1,1),"Seed must exercise two existing mysteries exchanging labels");
   Assert.AreEqual(0,Level.Hud.NextLabels[1].canvasRenderer.GetAlpha(),.001f,"Another helper must not restore this hidden label");
   Centered(id);app.SendCommand(new ActivateHiveCommand(1));Sync();View.TickMysteries(.31f);
   Assert.AreEqual(1,Level.Hud.QueueLabels[1].canvasRenderer.GetAlpha(),.001f,"Reveal must not capture another helper's zero alpha");
  }
  [UnityTest] public IEnumerator PagingRestoresOrdinaryLabelsWhenConcealedLabelsAreReused(){
   yield return Load(6);var app=BeeHarvestApp.Interface;app.SendCommand(new RefillHarvestBoosterCommand(3));app.SendCommand(new UseFreeHiveSelectionCommand());Sync();
   for(int page=0;page<3;page++){
    for(int c=0;c<3;c++)for(int row=0;row<Level.Hud.VisibleRows;row++){int id=Level.Hud.DisplayedHive(c,row);if(id<0)continue;Assert.AreEqual(Model.Hives[id].Data.isHidden?0:1,Level.Hud.RowLabel(c,row).canvasRenderer.GetAlpha(),.001f);if(Model.Hives[id].Data.isHidden)Centered(id);}
    if(page==0)Level.Hud.NextPage.onClick.Invoke();else Level.Hud.PreviousPage.onClick.Invoke();View.Sync();
   }
  }
 }
}
