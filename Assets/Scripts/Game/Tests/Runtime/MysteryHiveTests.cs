using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BeeHarvest.Tests
{
    public sealed class MysteryHiveTests : EconomyTestFixture
    {
        static HarvestModel Model => BeeHarvestApp.Interface.GetModel<HarvestModel>();
        static IEnumerator Load(int number = 12)
        {
            SceneManager.LoadScene(LevelOneAssets.SceneName(number));
            yield return null;
            yield return null;
            Object.FindObjectOfType<LevelOneController>().enabled = false;
            // Booster inventory intentionally survives scene reloads; every fixture prepares its own items.
            BeeHarvestApp.Interface.SendCommand(new RefillHarvestBoosterCommand(2));
            BeeHarvestApp.Interface.SendCommand(new RefillHarvestBoosterCommand(3));
        }
        static HiveModelView View => Object.FindObjectOfType<HiveModelView>();
        static LevelOneHud Hud => Object.FindObjectOfType<LevelOneController>().Hud;
        static Text Marker(int id)
        {
            var found = GameObject.Find("MysteryHive_" + id);
            Assert.IsNotNull(found, "Configured hidden hive must show its question marker");
            return found.GetComponent<Text>();
        }
        static Renderer Body(int id) => GameObject.Find("SmallHive_" + id).GetComponentsInChildren<Renderer>().First(r => r.sharedMaterials.Any(m => m.name.StartsWith("HivePalette")));
        static Color Tint(int id)
        {
            var renderer = Body(id);
            int slot = System.Array.FindIndex(renderer.sharedMaterials, m => m.name.StartsWith("HivePalette"));
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block, slot);
            return block.isEmpty ? renderer.sharedMaterials[slot].GetColor("_BaseColor") : block.GetColor("_BaseColor");
        }
        static void Sync() { Hud.Sync(); View.Sync(); }
        static void Tick(float seconds) { View.SendMessage("TickMysteries", seconds); }
        static void Near(Color a, Color b) { Assert.Less(Vector4.Distance(a, b), .0001f); }

        [UnityTest]
        public IEnumerator FiveProductionScenesConcealOnlyAuthoredVisibleHivesAndKeepModelQuotas()
        {
            foreach (int number in new[] { 6, 8, 12, 15, 18 })
            {
                yield return Load(number);int count=0;
                for(int column=0;column<3;column++)for(int row=0;row<Hud.VisibleRows;row++){
                    int id=Hud.DisplayedHive(column,row);if(id<0)continue;
                    var label=Hud.RowLabel(column,row);
                    if(Model.Hives[id].Data.isHidden){
                        count++;var marker=Marker(id);Assert.AreEqual("?",marker.text);Assert.IsFalse(marker.raycastTarget);Assert.AreSame(label.font,marker.font);
                        Assert.AreEqual(0,label.canvasRenderer.GetAlpha(),.001f,"Hidden quota must not render");
                        Assert.Greater(Model.Hives[id].Remaining,0);Near(new Color(.48f,.50f,.56f),Tint(id));
                    }else{Assert.AreEqual(1,label.canvasRenderer.GetAlpha(),.001f);Near(LevelOneAssets.ColorOf(Model.Hives[id].Data.color),Tint(id));}
                }
                Assert.AreEqual(count,Object.FindObjectsOfType<Text>().Count(t=>t.name.StartsWith("MysteryHive_")));
            }
        }

        [UnityTest]
        public IEnumerator FrontPromotionRevealsGraduallyPauseFreezesAndRetryRestoresHidden()
        {
            yield return Load();
            int id = Hud.DisplayedHive(0, 1);
            var marker = Marker(id);
            var shared = Body(id).sharedMaterials[0];
            var target = LevelOneAssets.ColorOf(Model.Hives[id].Data.color);
            BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));
            Sync();
            Tick(.15f);
            Assert.Greater(marker.color.a, .05f);
            Assert.Less(marker.color.a, .95f);
            Assert.Greater(Vector4.Distance(Tint(id), target), .01f);
            Assert.Greater(Vector4.Distance(Tint(id), new Color(.48f, .50f, .56f)), .01f);
            float alpha = marker.color.a;
            var tint = Tint(id);
            BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(true));
            Sync(); Tick(1);
            Assert.AreEqual(alpha, marker.color.a);
            Near(tint, Tint(id));
            BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(false));
            Sync(); Tick(.16f);
            Assert.IsFalse(marker.gameObject.activeSelf);
            Assert.AreEqual(1,Hud.QueueLabels[0].canvasRenderer.GetAlpha(),.001f);
            Near(target, Tint(id));
            Assert.AreSame(shared, Body(id).sharedMaterials[0]);
            Object.FindObjectOfType<LevelOneController>().RestartLevel();
            Sync();
            Assert.IsTrue(Marker(id).gameObject.activeInHierarchy);
            Near(new Color(.48f, .50f, .56f), Tint(id));
        }

        [UnityTest]
        public IEnumerator RearPickStaysHiddenUntilSuccessfulCommitAndCancelledHopDoesNotPeek()
        {
            yield return Load();
            int id = Hud.DisplayedHive(0, 1);
            var marker = Marker(id);
            var app = BeeHarvestApp.Interface;
            var hop = Object.FindObjectOfType<HiveHopController>();
            app.SendCommand(new UseFreeHiveSelectionCommand()); Sync();
            Assert.IsTrue(Model.FreeSelection, "Fixture must grant a real pending choice before testing cancellation");
            int inventory = Model.BoosterCount(3);
            hop.ClickWaiting(id); hop.Tick(.12f); Tick(.2f);
            Assert.IsTrue(marker.gameObject.activeSelf);
            Near(new Color(.48f, .50f, .56f), Tint(id));
            app.SendCommand(new PauseHarvestCommand(true)); hop.Tick(.01f);
            Assert.IsFalse(Model.Hives[id].Started);
            Assert.IsTrue(Model.FreeSelection);
            app.SendCommand(new PauseHarvestCommand(false)); Sync();
            hop.ClickWaiting(id); hop.Tick(.35f); Sync(); Tick(.31f);
            Assert.IsTrue(Model.Hives[id].Started);
            Assert.IsFalse(Model.FreeSelection);
            Assert.AreEqual(inventory, Model.BoosterCount(3));
            Assert.IsFalse(marker.gameObject.activeSelf);
            Near(LevelOneAssets.ColorOf(Model.Hives[id].Data.color), Tint(id));
        }

        [UnityTest]
        public IEnumerator RevealedIdentityStaysVisibleAfterShuffleAndSceneExitRemovesQuestions()
        {
            yield return Load();
            int id = Hud.DisplayedHive(0, 1);
            var marker = Marker(id);
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new ActivateHiveCommand(0)); Sync(); Tick(.31f);
            int shuffleInventory = Model.BoosterCount(2);
            app.SendCommand(new ShuffleWaitingHivesCommand(17)); Sync(); Tick(.31f);
            Assert.AreEqual(shuffleInventory - 1, Model.BoosterCount(2), "Exercise a successful shuffle, not an empty-inventory rejection");
            Assert.IsFalse(marker.gameObject.activeSelf, "Shuffle cannot re-hide an already revealed identity");
            // Native shared art is not recolored; all tint changes belong to renderer property blocks.
            foreach (var r in Object.FindObjectsOfType<Renderer>())
                foreach (var material in r.sharedMaterials.Where(m => m.name.StartsWith("HivePalette")))
                    Near(LevelOneAssets.ColorOf("Palette" + material.name.Substring("HivePalette".Length)), material.GetColor("_BaseColor"));
            SceneManager.LoadScene("Level03"); yield return null; yield return null;
            Assert.IsTrue(marker == null);
            Assert.IsFalse(Object.FindObjectsOfType<Text>().Any(t => t.name.StartsWith("MysteryHive_")));
        }
    }
}
