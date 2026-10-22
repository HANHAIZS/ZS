using GameFoundation.Core;
using GameFoundation.Save;
using GameFoundation.HotUpdate;
using GameFoundation.Sdk;
using GameFoundation.UI;
using QFramework;

namespace BeeHarvest
{
    public sealed class BeeHarvestApp : Architecture<BeeHarvestApp>
    {
        // This callback runs for every new Play session even when Unity skips domain and scene reloads.
        // It does not run for ordinary Game-to-Game loads, which must retain the selected next level.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetNavigationSession()
        {
            Interface.SendCommand(new ResetMenuNavigationSessionCommand());
        }

        /// <summary>
        /// Registers shared infrastructure before project-owned models and systems.
        /// </summary>
        protected override void Init()
        {
            FoundationCoreModule.Register(this);
            FoundationSaveModule.Register(this);
            FoundationHotUpdateModule.Register(this);
            FoundationSdkModule.Register(this);
            FoundationUiModule.Register(this);

            RegisterUtility(new EconomyRepository(this.GetUtility<ISaveStore>(),this.GetUtility<ISaveSerializer>(),this.GetUtility<IFoundationClock>()));
            RegisterModel(new EconomyModel());
            RegisterSystem(new EconomySystem());
            RegisterUtility(new MainMenuAssets());
            RegisterModel(new MenuNavigationModel());
            RegisterUtility(new LevelOneAssets());
            RegisterUtility(new SettingsPreferences());
            RegisterModel(new SettingsModel());
            RegisterModel(new HarvestModel());
            RegisterSystem(new HarvestSystem());
        }
    }
}
