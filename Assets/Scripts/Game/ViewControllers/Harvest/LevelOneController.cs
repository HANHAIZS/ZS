using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeeHarvest
{
    public sealed class LevelOneController : BeeHarvestController
    {
        HarvestModel model;
        MenuNavigationModel navigation;
        HarvestSystem rules;
        LevelOneAssets assets;
        HarvestBoardView board;
        HiveModelView hives;
        HiveHopController hop;
        BeeFlightView bees;
        public LevelOneHud Hud;
        [Range(1, LevelOneAssets.LastLevel)] public int LevelNumber = 1;
        bool backgroundPause, initialized, applicationPaused;
        bool uiInputReleasePending, settingsInputBlocked;
        BaseInputModule inputModule;
        public bool IsApplicationPaused => applicationPaused;

        /// <summary>Starts directly in the first level with the single project architecture.</summary>
        void Start()
        {
            model = this.GetModel<HarvestModel>();
            navigation = this.GetModel<MenuNavigationModel>();
            rules = this.GetSystem<HarvestSystem>();
            assets = this.GetUtility<LevelOneAssets>();
            this.SendCommand(new StartLevelCommand(assets.Level(LevelNumber)));
            hives = GetComponent<HiveModelView>();
            hives.Initialize(model, rules, assets, Hud);
            board = GetComponent<HarvestBoardView>();
            board.Initialize(model, assets, hives, LevelOneAssets.ModelKey(LevelNumber), LevelOneAssets.HasVoxelEdges(LevelNumber));
            bees = GetComponent<BeeFlightView>();
            bees.Initialize(model, board, assets);
            hop = GetComponent<HiveHopController>();
            hop.Initialize(model, rules, hives, Hud);
            Hud.Initialize(model, rules, hop.ClickWaiting, RetryFromHud, this);
            inputModule = EventSystem.current.GetComponent<BaseInputModule>();
            Application.runInBackground = Application.isEditor;
            initialized = true;
        }

        void Update()
        {
            if (!initialized || navigation.IsHome) return;
            float delta = Mathf.Min(Time.deltaTime, .1f);
            board.ObserveInput();
            board.TickAutoRotation(delta);
            board.PublishCameraView();
            this.SendCommand(new AdvanceHarvestCommand(delta,Time.unscaledDeltaTime));
            hop.Tick(delta);
            Hud.Sync();
            hives.Sync();
            hives.TickMysteries(delta);
            board.Sync();
            bees.RenderFlights();
        }

        public void RestartLevel()
        {
            hop.Cancel();
            this.SendCommand(new StartLevelCommand(assets.Level(LevelNumber)));
            board.ResetRotation();
            board.PublishCameraView();
        }

        void RetryFromHud() { TryRetryLevel(); }

        public bool TryRetryLevel()
        {
            if (!initialized || applicationPaused || navigation.IsHome) return false;
            var command = new RetryLevelCommand(assets.Level(LevelNumber));
            this.SendCommand(command);
            if (!command.Accepted) return false;
            hop.Cancel();
            board.ResetRotation();
            board.PublishCameraView();
            return true;
        }

        public bool TryLeaveLevel(int expectedGeneration)
        {
            if (!initialized || applicationPaused || navigation.IsHome) return false;
            var command = new LeaveLevelCommand(assets.Level(LevelNumber), LevelNumber, expectedGeneration);
            this.SendCommand(command);
            if (!command.Accepted) return false;
            hop.Cancel();
            board.ResetRotation();
            board.PublishCameraView();
            return true;
        }

        /// <summary>Blocks the underlying level for the whole modal input session.</summary>
        public void BlockSettingsInput()
        {
            settingsInputBlocked = true;
            BlockLevelInput();
        }

        void BlockLevelInput()
        {
            uiInputReleasePending = false;
            hop.SetModalInputBlocked(true);
            board.SetUiInputBlocked(true);
            Hud.SetHiveInputEnabled(false);
            if (Hud.Restart != null) Hud.Restart.interactable = false;
        }

        public void ReleaseSettingsInputAfterPointers() { settingsInputBlocked = false; uiInputReleasePending = true; }

        void LateUpdate()
        {
            if (!initialized || applicationPaused || navigation.IsHome || !uiInputReleasePending) return;
            var input = inputModule.input;
            // uGUI recreates stationary touches after clearing its cache. Keep the level
            // blocked through their Ended frame; only a subsequent fresh press is accepted.
            if (input.touchCount > 0 || input.GetMouseButton(0)) return;
            uiInputReleasePending = false;
            hop.SetModalInputBlocked(false);
            board.SetUiInputBlocked(false);
            Hud.SetHiveInputEnabled(true);
            if (Hud.Restart != null) Hud.Restart.interactable = true;
        }

        /// <summary>Restores only the modal-owned pause and defers resume while the app is backgrounded.</summary>
        public void RestorePauseAfterSettings(bool wasPaused, int generation)
        {
            if (!initialized || wasPaused || model.Generation != generation) return;
            if (applicationPaused) backgroundPause = true;
            else this.SendCommand(new PauseHarvestCommand(false));
        }

        /// <summary>Clears unfinished visual motion before pausing the mobile simulation.</summary>
        void OnApplicationPause(bool paused)
        {
            applicationPaused = paused;
            if (!initialized) return;
            if (paused)
            {
                BlockLevelInput();
                if(inputModule!=null)inputModule.DeactivateModule();
                backgroundPause = !model.Paused;
                this.SendCommand(new PauseHarvestCommand(true));
            }
            else
            {
                if(backgroundPause){this.SendCommand(new PauseHarvestCommand(false));backgroundPause=false;}
                // A cleared stationary touch is recreated as a press by uGUI; wait for every old pointer to end.
                if(!settingsInputBlocked)uiInputReleasePending=true;
            }
        }
    }
}
