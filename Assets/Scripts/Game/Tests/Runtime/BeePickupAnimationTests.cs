using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
    public sealed class BeePickupAnimationTests : EconomyTestFixture
    {
        static IEnumerator Load()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            Object.FindObjectOfType<LevelOneController>().enabled = false;
        }

        static void Draw()
        {
            Object.FindObjectOfType<HiveModelView>().Sync();
            Object.FindObjectOfType<HarvestBoardView>().Sync();
            Object.FindObjectOfType<BeeFlightView>().RenderFlights();
        }

        static void Tick(float seconds)
        {
            BeeSceneSteps.Advance(seconds);
            Draw();
        }

        static Transform Body(Transform actor)
        { return actor.Cast<Transform>().First(t => t.name != "CarriedVoxel"); }

        static void AssertCargoClear(Transform actor)
        {
            var cargo = actor.Find("CarriedVoxel").GetComponent<Renderer>();
            foreach (var part in actor.GetComponentsInChildren<Renderer>())
                if (part != cargo && !part.name.Contains("_Grip_")) Assert.IsFalse(BeeGeometryAssertions.Intersects(cargo,part),
                    "The pickup lift must not enter " + part.name);
        }

        // Removing the hold, body reach, or cargo lift must fail independently of the animation's implementation.
        [UnityTest]
        public IEnumerator PickupHoldsPositionAndLiftsCargoBeforeReturning()
        {
            yield return Load();
            BeeHarvestApp.Interface.SendCommand(new ActivateHiveCommand(0));
            Draw();
            BeeSceneSteps.Until(()=>BeeHarvestApp.Interface.GetModel<HarvestModel>().Flights[0].Reached&&BeeHarvestApp.Interface.GetModel<HarvestModel>().Flights[0].Progress>=AdvanceHarvestCommand.ApproachSeconds);
            var actor = GameObject.Find("BlenderBeeCarrier_0").transform;
            var body = Body(actor);
            var cargo = actor.Find("CarriedVoxel");
            var arrival = actor.position;
            float restingY = body.localPosition.y;
            Assert.IsFalse(cargo.gameObject.activeSelf, "Approach ends before the block is collected");
            Tick(AdvanceHarvestCommand.SettleSeconds+AdvanceHarvestCommand.ReachSeconds * .5f);
            Assert.Less(Vector3.Distance(arrival, actor.position), .005f, "The bee should hover at its collection point");
            Assert.Less(body.localPosition.y, restingY - .02f, "Reach down visibly while hovering");
            Tick(AdvanceHarvestCommand.ReachSeconds * .5f + .01f);
            Assert.IsTrue(cargo.gameObject.activeSelf);
            AssertCargoClear(actor);
            var loweredCargo = cargo.localPosition;
            var smallCargo = cargo.localScale;
            var liftOrigin = actor.position;
            Tick(.10f);
            Assert.Greater(Vector3.Distance(liftOrigin, actor.position), .03f, "Bee and held block extract together");
            Assert.Less(Vector3.Distance(cargo.position, body.Find("GripAnchor").position),.001f,"Cargo stays attached during extraction");
            Assert.Less(Vector3.Distance(cargo.localScale,smallCargo),.001f,"No pop resizes the block");
            AssertCargoClear(actor);
            Tick(.06f);
            AssertCargoClear(actor);
            Tick(AdvanceHarvestCommand.LiftSeconds - .17f + .1f);
            Assert.Greater(Vector3.Distance(liftOrigin, actor.position), .05f, "Return begins after the pickup action");
            AssertCargoClear(actor);
        }

        // A wall-clock animation or a stale pooled pose would move during pause or leak into the next departure.
        [UnityTest]
        public IEnumerator PausedPickupAndRetryRestoreAnimation()
        {
            yield return Load();
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new ActivateHiveCommand(0));
            Draw();
            BeeSceneSteps.FirstPickup();Tick(.06f);
            var actor = GameObject.Find("BlenderBeeCarrier_0").transform;
            var body = Body(actor);
            var cargo = actor.Find("CarriedVoxel");
            Assert.IsTrue(cargo.gameObject.activeSelf);
            Assert.Less(body.localPosition.y, -.01f, "Exercise the body pose during the pickup lift");
            app.SendCommand(new PauseHarvestCommand(true));
            Draw();
            var position = actor.position;
            var bodyPosition = body.localPosition;
            var cargoPosition = cargo.localPosition;
            var cargoScale = cargo.localScale;
            var wings = body.GetComponentsInChildren<Transform>().Where(t => t.name.Contains("Wing")).ToArray();
            var rotations = wings.Select(t => t.localRotation).ToArray();
            yield return null;
            Tick(5);
            Assert.Less(Vector3.Distance(position, actor.position), .001f);
            Assert.AreEqual(bodyPosition, body.localPosition);
            Assert.AreEqual(cargoPosition, cargo.localPosition);
            Assert.AreEqual(cargoScale, cargo.localScale);
            for (int i = 0; i < wings.Length; i++) Assert.Less(Quaternion.Angle(rotations[i], wings[i].localRotation), .01f);
            Object.FindObjectOfType<LevelOneController>().RestartLevel();
            Draw();
            Assert.IsNull(GameObject.Find("BlenderBeeCarrier_0"));
            Assert.AreEqual(1784, app.GetModel<HarvestModel>().Remaining);
            app.SendCommand(new ActivateHiveCommand(0));
            Draw();
            actor = GameObject.Find("BlenderBeeCarrier_0").transform;
            Assert.Less(Mathf.Abs(Body(actor).localPosition.y), .001f, "Pooled bee must depart with a neutral pose");
            Assert.IsFalse(actor.Find("CarriedVoxel").gameObject.activeSelf);
        }
    }
}
