using System;
using QFramework;

namespace BeeHarvest
{
    public enum GamePreference { Music, Sound, Haptic }

    public sealed class SettingsModel : AbstractModel
    {
        readonly bool[] enabled = new bool[3];
        public int Revision { get; internal set; }

        protected override void OnInit()
        {
            var preferences = this.GetUtility<SettingsPreferences>();
            for (int i = 0; i < enabled.Length; i++) enabled[i] = preferences.Read((GamePreference)i);
        }

        public bool IsEnabled(GamePreference option)
        {
            if (option < GamePreference.Music || option > GamePreference.Haptic)
                throw new ArgumentOutOfRangeException(nameof(option));
            return enabled[(int)option];
        }

        internal void Set(GamePreference option, bool value) { enabled[(int)option] = value; Revision++; }
    }
}
