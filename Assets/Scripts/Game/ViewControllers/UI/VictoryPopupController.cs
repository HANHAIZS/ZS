using System;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeeHarvest
{
    /// <summary>Shows a scene-cached settlement above the completed level; progression writes use the project Continue command.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class VictoryPopupController : BeeHarvestController, ICanSendEvent
    {
        public LevelOneController Level;
        public SettingsPopupController SettingsPopup;
        public Button SettingsButton, ContinueButton;
        public Image DimBackground;
        public CanvasGroup PopupContent;
        public RectTransform Presentation;
        public VictoryCelebrationView Celebration;
        HarvestModel model;
        MenuNavigationModel navigation;
        HiveHopController hop;
        BaseInputModule inputModule;
        PopupMotion motion;
        bool continuePending;
        bool initialized, applicationPaused, settingsWasInteractable, waitingForFreshPointer;
        int generation, continuedGeneration = -1;
        
        public bool IsOpen { get; private set; }

        void Awake()
        {
            if (DimBackground != null) DimBackground.gameObject.SetActive(false);
            if (PopupContent != null) PopupContent.gameObject.SetActive(false);
        }

        void Start()
        {
            if (Level == null || SettingsPopup == null || SettingsButton == null || ContinueButton == null || DimBackground == null || PopupContent == null || Presentation == null)
                throw new InvalidOperationException("VictoryPopup requires its level, button, dim and presentation bindings.");
            model = this.GetModel<HarvestModel>();
            navigation = this.GetModel<MenuNavigationModel>();
            hop = Level.GetComponent<HiveHopController>();
            inputModule = EventSystem.current.GetComponent<BaseInputModule>();
            motion = new PopupMotion(DimBackground.gameObject, PopupContent.gameObject, Presentation);
            DimBackground.color = new Color(0, 0, 0, .65f);
            ContinueButton.onClick.AddListener(Continue);
            ContinueButton.interactable = false;
            initialized = true;
        }

        void Update()
        {
            if (!initialized) return;
            if (IsOpen && (!model.Won || model.Generation != generation || navigation.IsHome)) Hide();
            if (applicationPaused || Level.IsApplicationPaused || navigation.IsHome) return;
            if (model.Won && !IsOpen && model.Generation != continuedGeneration) Show();
            if (!IsOpen) return;
            if (motion.Tick(Time.unscaledDeltaTime))
            {
                bool advance = continuePending && model.Won && model.Generation == generation;
                if (advance) continuedGeneration = generation;
                Hide();
                if (advance) this.SendCommand(new ContinueLevelCommand(Level.LevelNumber, generation));
                return;
            }
            RefreshInput();
        }

        void Show()
        {
            // A final-frame settings tap can arrive after delivery but before this view's Update.
            // Release that modal before taking its input gate and remembering the gear state.
            if (SettingsPopup.IsOpen) SettingsPopup.Close();
            hop.Cancel();
            Level.BlockSettingsInput();
            if (inputModule != null) inputModule.DeactivateModule();
            settingsWasInteractable = SettingsButton.interactable;
            SettingsButton.interactable = false;
            generation = model.Generation;
            continuePending = false;
            waitingForFreshPointer = true;
            IsOpen = true;
            transform.SetAsLastSibling();
            motion.Show();
            if (Celebration != null) Celebration.Play();
            this.SendEvent(new VictoryPresentationEvent { Generation = generation, Visible = true });
            ContinueButton.interactable = false;
            RefreshInput();
        }

        void RefreshInput()
        {
            if (!motion.IsOpening && !motion.IsClosing && waitingForFreshPointer)
            {
                var input = inputModule.input;
                if (input.touchCount == 0 && !input.GetMouseButton(0))
                {
                    inputModule.DeactivateModule();
                    waitingForFreshPointer = false;
                }
            }
            ContinueButton.interactable = !motion.IsOpening && !motion.IsClosing && !waitingForFreshPointer;
        }

        void Continue()
        {
            if (!initialized || !isActiveAndEnabled || !IsOpen || !ContinueButton.interactable || applicationPaused || Level.IsApplicationPaused || !model.Won || model.Generation != generation) return;
            continuePending = true;
            motion.Close();
            ContinueButton.interactable = false;
        }

        void Hide()
        {
            if (!IsOpen) return;
            IsOpen = false;
            continuePending = false;
            motion.HideImmediately();
            if (Celebration != null) Celebration.Stop();
            this.SendEvent(new VictoryPresentationEvent { Generation = generation, Visible = false });
            ContinueButton.interactable = false;
            PopupContent.gameObject.SetActive(false);
            DimBackground.gameObject.SetActive(false);
            
            if (SettingsButton != null) SettingsButton.interactable = settingsWasInteractable;
            if (Level != null && !navigation.IsHome)
            {
                if (inputModule != null) inputModule.DeactivateModule();
                Level.ReleaseSettingsInputAfterPointers();
            }
        }

        void OnApplicationPause(bool value)
        {
            applicationPaused = value;
            if (Celebration != null) Celebration.SetPaused(value);
            if (value)
            {
                if (continuePending) { continuePending = false; motion.Show(); }
                waitingForFreshPointer = true;
                if (ContinueButton != null) ContinueButton.interactable = false;
            }
        }

        void OnDisable() { if (initialized) Hide(); }
        void OnDestroy() { if (ContinueButton != null) ContinueButton.onClick.RemoveListener(Continue); }
    }
}
