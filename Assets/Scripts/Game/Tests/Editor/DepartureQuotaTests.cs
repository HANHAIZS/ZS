using NUnit.Framework;
using QFramework;

namespace BeeHarvest.Tests
{
    public sealed class DepartureQuotaTests : EconomyTestFixture
    {
        static LevelData Line(int count, params HiveData[] hives)
        {
            var cubes = new CubeData[count];
            for (int i = 0; i < count; i++) cubes[i] = new CubeData { id = i, x = i, color = "pink" };
            return new LevelData { cubes = cubes, hives = hives };
        }

        [Test]
        public void DepartureSpendsQuotaBeforePickupWithoutDoubleCharge()
        {
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new StartLevelCommand(Line(1, new HiveData { column = 0, color = "pink", quota = 1 })));
            app.SendCommand(new ActivateHiveCommand(0));
            var m = app.GetModel<HarvestModel>();
            Assert.AreEqual(0, m.Hives[0].Remaining, "The outbound bee spends its quota immediately");
            Assert.AreEqual(1, m.Hives[0].Reserved);
            Assert.AreEqual(-1, m.Slots[0], "Last departure releases the slot");
            Assert.AreEqual(1, m.Remaining, "The voxel is still present during outbound flight");
            Assert.AreEqual(0, m.InTransit);
            Assert.AreEqual(0, m.Deposited);
            app.SendCommand(new AdvanceHarvestCommand(.3f));
            Assert.AreEqual(0, m.Hives[0].Remaining);
            app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds - .3f + .01f));
            Assert.AreEqual(0, m.Hives[0].Remaining, "Pickup must not spend quota twice");
            Assert.AreEqual(0, m.Hives[0].Reserved);
            Assert.AreEqual(1, m.InTransit);
            Assert.IsFalse(m.Won);
            app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.LoadedSeconds));
            Assert.AreEqual(1, m.Deposited);
            Assert.IsTrue(m.Won);
        }

        [Test]
        public void LastDepartureReleasesSlotWithoutClearingNewOwnerAtPickup()
        {
            var app = BeeHarvestApp.Interface;
            app.SendCommand(new StartLevelCommand(Line(5,
                new HiveData { column = 0, color = "pink", quota = 1 },
                new HiveData { column = 1, color = "pink", quota = 4 })));
            app.SendCommand(new ActivateHiveCommand(0));
            app.SendCommand(new ActivateHiveCommand(1));
            var m = app.GetModel<HarvestModel>();
            Assert.AreEqual(1, m.Slots[0], "The second hive reuses the slot while the first bee is outbound");
            Assert.AreEqual(4,m.Hives[1].Remaining,"Slot reuse cannot bypass the approach queue");
            Assert.AreEqual(1,m.Flights.Count);
            app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds));
            Assert.AreEqual(1,m.Slots[0],"The prior pickup cannot clear the new slot owner");
            Assert.AreEqual(4,m.Remaining);Assert.AreEqual(1,m.InTransit);
            for(int i=0;i<600&&!m.Won;i++)app.SendCommand(new AdvanceHarvestCommand(.1f));
            Assert.AreEqual(0, m.Hives[1].Remaining);
            Assert.AreEqual(-1, m.Slots[0]);
            Assert.AreEqual(5, m.Deposited);
            Assert.IsTrue(m.Won);
        }
    }
}
