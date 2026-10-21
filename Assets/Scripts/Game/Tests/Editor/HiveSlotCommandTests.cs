using System;
using NUnit.Framework;
using QFramework;

namespace BeeHarvest.Tests
{
    public sealed class HiveSlotCommandTests : EconomyTestFixture
    {
        static void Start()
        {
            var cubes = new CubeData[12];
            for (int i = 0; i < cubes.Length; i++) cubes[i] = new CubeData { id = i, x = i, color = "pink" };
            BeeHarvestApp.Interface.SendCommand(new StartLevelCommand(new LevelData
            {
                cubes = cubes,
                hives = new[]
                {
                    new HiveData { column = 0, color = "pink", quota = 6 },
                    new HiveData { column = 1, color = "pink", quota = 6 }
                }
            }));
        }

        static void Place(int column, int slot)
        {
            var ctor = typeof(ActivateHiveCommand).GetConstructor(new[] { typeof(int), typeof(int) });
            Assert.IsNotNull(ctor, "Placement must accept an explicit destination slot");
            BeeHarvestApp.Interface.SendCommand((ActivateHiveCommand)ctor.Invoke(new object[] { column, slot }));
        }

        [Test]
        public void RequestedSlotDoesNotChooseFirstFreeSlot()
        {
            Start();
            Place(0, 4);
            var m = BeeHarvestApp.Interface.GetModel<HarvestModel>();
            Assert.AreEqual(0, m.Slots[4]);
            Assert.AreEqual(4, m.Hives[0].Slot);
            Assert.AreEqual(5, m.Hives[0].Remaining);
            for (int i = 0; i < 4; i++) Assert.AreEqual(-1, m.Slots[i]);
            foreach (var flight in m.Flights) Assert.AreEqual(4, flight.Slot);
        }

        [Test]
        public void OccupiedAndInvalidDestinationsDoNotConsumeQueueOrQuota()
        {
            Start();
            Place(0, 4);
            var m = BeeHarvestApp.Interface.GetModel<HarvestModel>();
            int revision = m.Revision;
            foreach (int slot in new[] { 4, -1, 5 }) Place(1, slot);
            Assert.AreEqual(revision, m.Revision);
            Assert.IsFalse(m.Hives[1].Started);
            Assert.AreEqual(6, m.Hives[1].Remaining);
            Assert.AreEqual(0, m.Slots[4]);
            for (int i = 0; i < 4; i++) Assert.AreEqual(-1, m.Slots[i]);
        }
    }
}
