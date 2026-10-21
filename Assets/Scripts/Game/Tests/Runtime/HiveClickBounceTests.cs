using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
    public sealed class HiveClickBounceTests : EconomyTestFixture
    {
        static HarvestModel Model => BeeHarvestApp.Interface.GetModel<HarvestModel>();
        static HiveHopController Hop => Object.FindObjectOfType<HiveHopController>();
        static GameObject Hive => GameObject.Find("SmallHive_0");
        static float Proportion => Hive.transform.localScale.y / Hive.transform.localScale.x;
        static Vector3 Position => Camera.main.WorldToScreenPoint(Hive.transform.position);
        static IEnumerator Load()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            Object.FindObjectOfType<LevelOneController>().enabled = false;
        }
        [TearDown] public void CancelUnfinishedAnimation() { if (Hop != null) Hop.Cancel(); }

        [UnityTest]
        public IEnumerator AcceptedClickImmediatelySquashesWithoutSpendingQuotaOrLeavingItsQueue()
        {
            yield return Load();
            var before = Position;
            int quota = Model.Hives[0].Remaining;
            Hop.ClickWaiting(0);
            Assert.Less(Proportion, .98f, "Click must give visible feedback in its first frame");
            Assert.Greater(Hive.transform.localScale.x, Hive.transform.localScale.y);
            Assert.Less(Vector2.Distance(before, Position), 5, "The press remains at the source");
            Assert.AreEqual(quota, Model.Hives[0].Remaining);
            Assert.IsFalse(Model.Hives[0].Started);
            Assert.AreEqual(0, Model.Flights.Count);
        }

        [UnityTest]
        public IEnumerator PressThenSpringThenLandingPreservesTheOriginalSingleCommitTime()
        {
            yield return Load();
            var before = Position;
            Hop.ClickWaiting(0);
            Hop.Tick(.02f);
            Assert.Less(Proportion, .92f, "Early click compresses, rather than stretches");
            Assert.Less(Vector2.Distance(before, Position), 5);
            Hop.Tick(.10f);
            Assert.Greater(Proportion, 1.08f, "Takeoff stretches upward");
            Assert.Greater(Position.y - before.y, 20);
            Hop.Tick(.17f);
            Assert.Less(Proportion, .95f, "Landing has one small squash");
            Hop.Tick(.049f);
            Assert.IsFalse(Model.Hives[0].Started);
            Assert.IsTrue(Model.Slots.All(id => id < 0));
            Hop.Tick(.002f);
            Assert.IsTrue(Model.Hives[0].Started);
            Assert.AreEqual(1, Model.Hives.Count(h => h.Started));
            Assert.AreEqual(0, Model.Slots[0]);
            Assert.AreEqual(1, Proportion, .001f);
            Hop.Tick(1);
            Assert.AreEqual(1, Model.Hives.Count(h => h.Started));
        }

        [UnityTest]
        public IEnumerator CancellingThePressRestoresTheModelAndTheNextClickCanBounceAgain()
        {
            yield return Load();
            var before = Position;
            var scale = Hive.transform.localScale;
            Hop.ClickWaiting(0); Hop.Tick(.02f);
            Assert.Less(Proportion, .92f);
            Hop.ClickWaiting(0); Hop.ClickWaiting(1);
            Hop.SendMessage("OnApplicationPause", true);
            Hop.SendMessage("OnApplicationPause", false);
            Assert.Less(Vector2.Distance(before, Position), 1);
            Assert.Less(Vector3.Distance(scale, Hive.transform.localScale), .001f);
            Assert.IsFalse(Model.Hives.Any(h => h.Started));
            Assert.AreEqual(0, Model.Flights.Count);
            // SendMessage also invokes the level's background pointer gate. This fixture manually
            // drives its disabled controller, just as the next fresh-input frame would.
            Object.FindObjectOfType<LevelOneController>().SendMessage("LateUpdate");
            Hop.ClickWaiting(0);
            Assert.Less(Proportion, .98f);
        }
    }
}
