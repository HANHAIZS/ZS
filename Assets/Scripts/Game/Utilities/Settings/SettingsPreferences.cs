using System;
using QFramework;
using UnityEngine;

namespace BeeHarvest
{
    /// <summary>Stores only the three offline presentation preferences.</summary>
    public sealed class SettingsPreferences : IUtility
    {
        readonly string prefix;
        public SettingsPreferences(string prefix = "BeeHarvest.settings.v1") { this.prefix = prefix; }

        string Key(GamePreference option)
        {
            if (option < GamePreference.Music || option > GamePreference.Haptic)
                throw new ArgumentOutOfRangeException(nameof(option));
            return prefix + "." + option;
        }

        public bool Read(GamePreference option)
        {
            // Missing and malformed preferences use the enabled default.
            return PlayerPrefs.GetInt(Key(option), 1) != 0;
        }

        public void Write(GamePreference option, bool enabled)
        {
            PlayerPrefs.SetInt(Key(option), enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
