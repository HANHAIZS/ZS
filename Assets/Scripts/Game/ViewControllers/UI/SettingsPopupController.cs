using GameFoundation.UI;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeeHarvest
{
    /// <summary>Owns a scene-cached modal and dispatches project Commands from UI callbacks.</summary>
    public sealed class SettingsPopupController : BeeHarvestController, IUiPopupView
    {
        public LevelOneController Level;
        public MainMenuPage MenuPage;
        public Button OpenButton, CloseButton, ContinueButton, RetryButton, HomeButton;
        public Button[] PreferenceButtons;
        public GameObject[] OffMarks;
        public Image[] PreferenceIcons;
        public GameObject DimBackground, PopupContent;
        public GameObject LeavingContent;
        public Button LeaveButton, CancelLeaveButton;
        HarvestModel harvest;
        SettingsModel settings;
        EconomyModel economy;
        float nextEconomyRefresh;
        HiveHopController hop;
        MainMenuNavigationController navigation;
        UiPopupRouter router;
        BaseInputModule inputModule;
        bool initialized, wasPaused, applicationPaused, modalPointerReleasePending;
        int generation;
        PopupMotion motion;
        enum CloseAction { Resume, Retry, Leave }
        CloseAction closeAction;
        public string PopupId => Level != null ? "LevelOneSettings" : "MainMenuSettings";
        public bool IsOpen { get; private set; }
        public bool IsLeaving { get; private set; }

        void Awake()
        {
            DimBackground.SetActive(false);
            PopupContent.SetActive(false);
            if (LeavingContent != null) LeavingContent.SetActive(false);
        }

        void Start()
        {
            harvest = this.GetModel<HarvestModel>();
            settings = this.GetModel<SettingsModel>();
            if (RetryButton != null) economy = this.GetModel<EconomyModel>();
            router = this.GetUtility<UiPopupRouter>();
            if (Level != null)
            {
                hop = Level.GetComponent<HiveHopController>();
                navigation = Level.GetComponent<MainMenuNavigationController>();
            }
            // EventSystem activates its module in Update, after this component's Start.
            inputModule = EventSystem.current.GetComponent<BaseInputModule>();
            OpenButton.onClick.AddListener(Show);
            CloseButton.onClick.AddListener(CloseFromButton);
            if (ContinueButton != null) ContinueButton.onClick.AddListener(CloseFromButton);
            if (RetryButton != null) RetryButton.onClick.AddListener(Retry);
            if (HomeButton != null)
            {
                HomeButton.interactable = navigation != null;
                HomeButton.onClick.AddListener(Home);
            }
            if (LeaveButton != null) LeaveButton.onClick.AddListener(ConfirmLeave);
            if (CancelLeaveButton != null) CancelLeaveButton.onClick.AddListener(CancelLeave);
            PreferenceButtons[0].onClick.AddListener(ToggleMusic);
            PreferenceButtons[1].onClick.AddListener(ToggleSound);
            PreferenceButtons[2].onClick.AddListener(ToggleHaptic);
            motion = new PopupMotion(DimBackground, PopupContent, null, LeavingContent);
            router.Register(this);
            initialized = true;
            RenderPreferences();
        }

        void Show()
        {
            if (!initialized || !isActiveAndEnabled || applicationPaused || (Level != null && Level.IsApplicationPaused) || IsOpen) return;
            router.Show(PopupId, UiPopupOpenMode.Replace);
        }

        public void Open(object arguments)
        {
            if (!initialized || !isActiveAndEnabled || applicationPaused || (Level != null && Level.IsApplicationPaused))
            {
                if (router != null) router.NotifyClosed(this);
                return;
            }
            if (IsOpen) return;
            if (Level != null)
            {
                hop.Cancel();
                Level.BlockSettingsInput();
            }
            else MenuPage.BlockSettingsInput();
            // Cancels pre-modal presses so their eventual release cannot click the queue after Continue/Close.
            if (inputModule != null) inputModule.DeactivateModule();
            if (Level != null)
            {
                wasPaused = harvest.Paused;
                generation = harvest.Generation;
                this.SendCommand(new PauseHarvestCommand(true));
            }
            IsOpen = true;
            RefreshRetry();
            OpenButton.interactable = false;
            motion.Show();
            RenderPreferences();
        }

        bool CanInteract => initialized && !motion.IsClosing && IsOpen && isActiveAndEnabled && !applicationPaused && !modalPointerReleasePending && (Level == null || !Level.IsApplicationPaused);
        void CloseFromButton() { if (CanInteract && !IsLeaving) BeginClose(CloseAction.Resume); }
        public void Close() { Hide(true); }

        void Hide(bool restorePause)
        {
            if (!IsOpen) return;
            motion.HideImmediately();
            IsOpen = false;
            IsLeaving = false;
            if (LeavingContent != null) LeavingContent.SetActive(false);
            modalPointerReleasePending = false;
            PopupContent.SetActive(false);
            DimBackground.SetActive(false);
            if (OpenButton != null && Level != null) OpenButton.interactable = true;
            if (restorePause && Level != null) Level.RestorePauseAfterSettings(wasPaused, generation);
            if (Level != null) Level.ReleaseSettingsInputAfterPointers();
            else if (MenuPage != null)
            {
                if (inputModule != null) inputModule.DeactivateModule();
                MenuPage.ReleaseSettingsInputAfterPointers();
            }
            router.NotifyClosed(this);
        }

        void Retry()
        {
            if (!CanInteract || IsLeaving || Level == null) return;
            if (economy.Stamina > 0) BeginClose(CloseAction.Retry);
            else RefreshRetry();
        }

        void RefreshRetry()
        {
            if (RetryButton == null || economy == null) return;
            this.SendCommand(new RefreshEconomyCommand());
            RetryButton.interactable = economy.Stamina > 0;
            nextEconomyRefresh = Time.unscaledTime + 1;
        }
        void Update()
        {
            if (!initialized || !IsOpen) return;
            if (!applicationPaused && (Level == null || !Level.IsApplicationPaused) && motion.Tick(Time.unscaledDeltaTime)) CompleteClose();
            if (IsOpen && !motion.IsClosing && RetryButton != null && !applicationPaused && Time.unscaledTime >= nextEconomyRefresh) RefreshRetry();
        }

        void Home()
        {
            if (!CanInteract || IsLeaving || navigation == null || LeavingContent == null) return;
            SwitchLeaving(true);
        }

        void SwitchLeaving(bool value)
        {
            // Switching views keeps the same modal/pause ownership. Discard presses from the old view.
            if (inputModule != null)
            {
                modalPointerReleasePending = inputModule.input.touchCount > 0 || inputModule.input.GetMouseButton(0);
                inputModule.DeactivateModule();
            }
            IsLeaving = value;
            motion.Show(value ? 1 : 0);
        }

        void CancelLeave()
        {
            if (!CanInteract || !IsLeaving) return;
            SwitchLeaving(false);
        }

        void ConfirmLeave()
        {
            if (!CanInteract || !IsLeaving || Level == null) return;
            BeginClose(CloseAction.Leave);
        }

        void BeginClose(CloseAction action)
        {
            closeAction = action;
            motion.Close();
        }

        void CompleteClose()
        {
            if (closeAction == CloseAction.Resume) { Hide(true); return; }
            if (!CanInteract || Level == null || harvest.Generation != generation) { Hide(true); return; }
            bool accepted = closeAction == CloseAction.Retry ? Level.TryRetryLevel() : Level.TryLeaveLevel(generation);
            if (accepted) Hide(false);
            else { motion.Show(IsLeaving ? 1 : 0); RefreshRetry(); }
        }

        void ToggleMusic() { Toggle(GamePreference.Music); }
        void ToggleSound() { Toggle(GamePreference.Sound); }
        void ToggleHaptic() { Toggle(GamePreference.Haptic); }
        void Toggle(GamePreference option)
        {
            if (!CanInteract || IsLeaving) return;
            this.SendCommand(new SetPreferenceCommand(option, !settings.IsEnabled(option)));
            RenderPreferences();
        }

        void RenderPreferences()
        {
            for (int i = 0; i < 3; i++)
            {
                bool enabled = settings.IsEnabled((GamePreference)i);
                OffMarks[i].SetActive(!enabled);
                PreferenceIcons[i].color = enabled ? Color.white : new Color(.64f, .64f, .64f, 1);
            }
        }

        void OnApplicationPause(bool value)
        {
            applicationPaused = value;
            if (!initialized || !IsOpen) return;
            if (value)
            {
                modalPointerReleasePending = true;
                // Never execute a deferred spend/navigation action after a background interruption.
                if (motion.IsClosing)
                {
                    if (closeAction == CloseAction.Resume) Hide(true);
                    else { motion.Show(IsLeaving ? 1 : 0); closeAction = CloseAction.Resume; }
                }
            }
            if (inputModule != null) inputModule.DeactivateModule();
        }
        void LateUpdate()
        {
            if (!initialized || !IsOpen || applicationPaused || !modalPointerReleasePending) return;
            var input = inputModule.input;
            if (input.touchCount > 0 || input.GetMouseButton(0)) return;
            // uGUI can recreate a cleared Stationary touch as a press. Discard it after
            // every old pointer has ended, then allow only a subsequent new press.
            inputModule.DeactivateModule();
            modalPointerReleasePending = false;
        }

        void OnDisable()
        {
            // A disabled scene-cached modal releases only its own foreground pause.
            if (!initialized) return;
            Hide(true);
            router.Unregister(this);
            if (OpenButton != null) OpenButton.interactable = false;
        }

        void OnEnable()
        {
            if (!initialized) return;
            router.Register(this);
            if (OpenButton != null) OpenButton.interactable = true;
        }

        void OnDestroy()
        {
            if (!initialized) return;
            router.Unregister(this);
            if (OpenButton != null) OpenButton.onClick.RemoveListener(Show);
            if (CloseButton != null) CloseButton.onClick.RemoveListener(CloseFromButton);
            if (ContinueButton != null) ContinueButton.onClick.RemoveListener(CloseFromButton);
            if (RetryButton != null) RetryButton.onClick.RemoveListener(Retry);
            if (HomeButton != null) HomeButton.onClick.RemoveListener(Home);
            if (LeaveButton != null) LeaveButton.onClick.RemoveListener(ConfirmLeave);
            if (CancelLeaveButton != null) CancelLeaveButton.onClick.RemoveListener(CancelLeave);
            if (PreferenceButtons[0] != null) PreferenceButtons[0].onClick.RemoveListener(ToggleMusic);
            if (PreferenceButtons[1] != null) PreferenceButtons[1].onClick.RemoveListener(ToggleSound);
            if (PreferenceButtons[2] != null) PreferenceButtons[2].onClick.RemoveListener(ToggleHaptic);
        }
    }
}
