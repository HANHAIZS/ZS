using NUnit.Framework;
using QFramework;

namespace BeeHarvest.Tests
{
    public sealed class BeePickupTimingTests : EconomyTestFixture
    {
        // Removing the collection hold would extract/deposit the voxel before these boundaries.
        [Test]
        public void ArrivalWaitsForPickupAndLiftBeforeDeposit()
        {
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new StartLevelCommand(new LevelData
            {
                cubes = new[] { new CubeData { id = 0, x = 0, color = "pink" } },
                hives = new[] { new HiveData { column = 0, color = "pink", quota = 1 } }
            }));
            app.SendCommand(new ActivateHiveCommand(0));
            var m = app.GetModel<HarvestModel>();
            app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.ApproachSeconds));
            Assert.AreEqual(1, m.Remaining, "Arrival starts the pickup hold instead of removing the voxel");
            Assert.AreEqual(1, m.Hives[0].Reserved);
            Assert.AreEqual(0, m.Hives[0].Remaining, "Departure has already spent its quota");
            app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.SettleSeconds+AdvanceHarvestCommand.ReachSeconds-.01f));
            Assert.AreEqual(0, m.InTransit, "The voxel stays on the flower during the downward reach");
            app.SendCommand(new AdvanceHarvestCommand(.02f));
            Assert.AreEqual(0, m.Remaining);
            Assert.AreEqual(1, m.InTransit);
            Assert.AreEqual(0, m.Hives[0].Reserved);
            app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.LoadedSeconds-.02f));
            Assert.AreEqual(0, m.Deposited, "The lift adds time before the normal return flight");
            app.SendCommand(new AdvanceHarvestCommand(.03f));
            Assert.AreEqual(1, m.Deposited);
            Assert.AreEqual(1, m.Remaining + m.InTransit + m.Deposited);
            Assert.IsTrue(m.Won);
            app.SendCommand(new AdvanceHarvestCommand(10));
            Assert.AreEqual(1, m.Deposited, "A pickup must be accounted for only once");
        }
    }
}
