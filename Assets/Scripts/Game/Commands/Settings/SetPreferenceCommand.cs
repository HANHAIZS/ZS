using QFramework;

namespace BeeHarvest
{
    public sealed class SetPreferenceCommand : AbstractCommand
    {
        readonly GamePreference option;
        readonly bool enabled;
        public SetPreferenceCommand(GamePreference option, bool enabled) { this.option = option; this.enabled = enabled; }

        protected override void OnExecute()
        {
            var model = this.GetModel<SettingsModel>();
            if (model.IsEnabled(option) == enabled) return;
            this.GetUtility<SettingsPreferences>().Write(option, enabled);
            model.Set(option, enabled);
        }
    }
}
