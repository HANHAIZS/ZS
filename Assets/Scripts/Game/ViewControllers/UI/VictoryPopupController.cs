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
        enum Action { None, Next, Retry, Select }
        Action pendingAction;
        GameObject trialActions;
        Button trialRetry,trialSelect,trialNext;
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
            if(PlaytestBuild.Available&&navigation.IsPlaytest)BuildTrialActions();
            ContinueButton.interactable = false;
            initialized = true;
        }

        void BuildTrialActions()
        {
            var original=(RectTransform)ContinueButton.transform;
            var row=PlaytestUi.Rect("PlaytestVictoryActions",original.parent,0,0,1,1);
            row.anchorMin=original.anchorMin;row.anchorMax=original.anchorMax;row.pivot=original.pivot;
            row.sizeDelta=original.sizeDelta;row.anchoredPosition=original.anchoredPosition;
            trialActions=row.gameObject;trialActions.SetActive(false);
            var label=ContinueButton.GetComponentInChildren<TMPro.TMP_Text>();
            var font=label==null?TMPro.TMP_Settings.defaultFontAsset:label.font;
            trialRetry=PlaytestUi.Button("PlaytestReplay",row,"Replay",font,32,0,0,.31f,1);
            trialSelect=PlaytestUi.Button("PlaytestSelectLevel",row,"Level Select",font,32,.345f,0,.655f,1);
            trialNext=PlaytestUi.Button("PlaytestNextLevel",row,"Next",font,32,.69f,0,1,1);
            trialRetry.onClick.AddListener(()=>Begin(Action.Retry,trialRetry));
            trialSelect.onClick.AddListener(()=>Begin(Action.Select,trialSelect));
            trialNext.onClick.AddListener(()=>Begin(Action.Next,trialNext));
            ButtonPressMotion.Install(trialActions);
        }

        void Update()
        {
            if (!initialized) return;
            if (IsOpen && (!model.Won || model.Generation != generation || navigation.IsHome || !Level.MatchesNavigation)) Hide();
            if (applicationPaused || Level.IsApplicationPaused || navigation.IsHome || !Level.MatchesNavigation) return;
            if (model.Won && !IsOpen && model.Generation != continuedGeneration) Show();
            if (!IsOpen) return;
            if (motion.Tick(Time.unscaledDeltaTime))
            {
                var action=model.Won&&model.Generation==generation?pendingAction:Action.None;
                if(action==Action.Next||action==Action.Select)continuedGeneration=generation;
                Hide();
                if(action==Action.Next)this.SendCommand(new ContinueLevelCommand(Level.LevelNumber,generation));
                else if(action==Action.Select)this.SendCommand(new OpenPlaytestSelectorCommand());
                else if(action==Action.Retry)Level.TryRetryLevel();
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
            pendingAction=Action.None;
            bool trial=trialActions!=null&&navigation.IsPlaytest;
            ContinueButton.gameObject.SetActive(!trial);
            if(trial)
            {
                bool next=Level.LevelNumber<LevelOneAssets.LastLevel;
                trialActions.SetActive(true);trialNext.gameObject.SetActive(next);
                var retry=(RectTransform)trialRetry.transform;var select=(RectTransform)trialSelect.transform;
                retry.anchorMax=new Vector2(next?.31f:.48f,1);
                select.anchorMin=new Vector2(next?.345f:.52f,0);
                select.anchorMax=new Vector2(next?.655f:1,1);
            }
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
            bool ready=!motion.IsOpening&&!motion.IsClosing&&!waitingForFreshPointer;
            ContinueButton.interactable=ready&&ContinueButton.gameObject.activeSelf;
            if(trialActions!=null)
            {
                trialRetry.interactable=ready&&trialActions.activeSelf;
                trialSelect.interactable=ready&&trialActions.activeSelf;
                trialNext.interactable=ready&&trialActions.activeSelf&&trialNext.gameObject.activeSelf;
            }
        }

        void Continue(){Begin(Action.Next,ContinueButton);}
        void Begin(Action action,Button button)
        {
            if(!initialized||!isActiveAndEnabled||!IsOpen||pendingAction!=Action.None||!button.isActiveAndEnabled||
               !button.interactable||applicationPaused||Level.IsApplicationPaused||!model.Won||model.Generation!=generation)return;
            pendingAction=action;
            motion.Close();
            RefreshInput();
        }

        void Hide()
        {
            if (!IsOpen) return;
            IsOpen = false;
            pendingAction=Action.None;
            if(trialActions!=null)trialActions.SetActive(false);
            ContinueButton.gameObject.SetActive(true);
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
                if(pendingAction!=Action.None){pendingAction=Action.None;motion.Show();}
                waitingForFreshPointer = true;
                if(ContinueButton!=null)ContinueButton.interactable=false;
                if(trialActions!=null){trialRetry.interactable=false;trialSelect.interactable=false;trialNext.interactable=false;}
            }
        }

        void OnDisable() { if (initialized) Hide(); }
        void OnDestroy() { if (ContinueButton != null) ContinueButton.onClick.RemoveListener(Continue); }
    }
}
