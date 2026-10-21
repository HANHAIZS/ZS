using System;
using NUnit.Framework;
using UnityEngine;

namespace BeeHarvest.Tests
{
    public sealed class SettingsPreferencesTests : EconomyTestFixture
    {
        string prefix;
        Type storeType, optionType;
        object store;

        [SetUp]
        public void Setup()
        {
            prefix = "BeeHarvest.tests.settings." + Guid.NewGuid().ToString("N");
            storeType = typeof(HarvestModel).Assembly.GetType("BeeHarvest.SettingsPreferences");
            optionType = typeof(HarvestModel).Assembly.GetType("BeeHarvest.GamePreference");
            Assert.IsNotNull(storeType, "A local preference store must exist");
            store = Activator.CreateInstance(storeType, prefix);
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (var name in new[] { "Music", "Sound", "Haptic" }) PlayerPrefs.DeleteKey(prefix + "." + name);
        }

        object Option(int value) { return Enum.ToObject(optionType, value); }
        bool Read(int value) { return (bool)storeType.GetMethod("Read").Invoke(store, new[] { Option(value) }); }

        [Test]
        public void MissingAndInvalidPreferencesDefaultToEnabled()
        {
            for (int i = 0; i < 3; i++) Assert.IsTrue(Read(i));
            PlayerPrefs.SetInt(prefix + ".Music", 42);
            Assert.IsTrue(Read(0), "An invalid preference cannot silently mute a new install");
        }

        [Test]
        public void AllPreferencesRoundTripIndependently()
        {
            for (int i = 0; i < 3; i++) storeType.GetMethod("Write").Invoke(store, new[] { Option(i), (object)false });
            store = Activator.CreateInstance(storeType, prefix);
            for (int i = 0; i < 3; i++) Assert.IsFalse(Read(i));
            storeType.GetMethod("Write").Invoke(store, new[] { Option(1), (object)true });
            Assert.IsFalse(Read(0));
            Assert.IsTrue(Read(1));
            Assert.IsFalse(Read(2));
        }
    }
}
