using GameFoundation.UI;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BeeHarvest {
 /// <summary>One cached three-use purchase view, owned by the scene's booster bar.</summary>
 public sealed class BoosterPurchasePopupController : BeeHarvestController,IUiPopupView {
  public GameObject DimBackground,PopupContent;
  public Button CloseButton,PurchaseButton;
  public TMP_Text TitleLabel,DescriptionLabel,QuantityLabel,PriceLabel;
  public Image BoosterIcon;
  public Sprite[] Icons;
  public LevelOneController Level {get;private set;}
  public bool IsOpen {get;private set;}
  public int SelectedBooster {get;private set;}=-1;
  public string PopupId=>"BoosterPurchase";
  static readonly string[] Titles={"Extra Slot","Speed Up","Shuffle","Pick Any"};
  static readonly string[] Descriptions={"Add 1 extra slot","Speed up all bee carry speeds","Shuffle all single hive positions","Pick any Hive from queue"};
  HarvestModel model;HarvestSystem rules;EconomyModel economy;MenuNavigationModel navigation;
  UiPopupRouter router;BaseInputModule inputModule;HiveHopController hop;
  bool initialized,wasPaused,applicationPaused,pointerReleasePending;
  int generation,economyRevision=-1;
  PopupMotion motion;
  void Awake(){DimBackground.SetActive(false);PopupContent.SetActive(false);}
  public void Initialize(LevelOneController level){
   if(initialized)return;
   Level=level;model=this.GetModel<HarvestModel>();rules=this.GetSystem<HarvestSystem>();economy=this.GetModel<EconomyModel>();navigation=this.GetModel<MenuNavigationModel>();router=this.GetUtility<UiPopupRouter>();
   inputModule=EventSystem.current.GetComponent<BaseInputModule>();hop=level.GetComponent<HiveHopController>();
   CloseButton.onClick.AddListener(Cancel);PurchaseButton.onClick.AddListener(Purchase);
   motion=new PopupMotion(DimBackground,PopupContent);initialized=true;router.Register(this);
  }
  bool CanOpen(int index)=>initialized&&isActiveAndEnabled&&!applicationPaused&&Level!=null&&!Level.IsApplicationPaused&&!navigation.IsHome&&!IsOpen&&index>=0&&index<4&&model.BoosterCount(index)==0&&rules.CanUseBoosters;
  public void Show(int index){if(!CanOpen(index)||router.ActiveView!=null)return;router.Show(PopupId,UiPopupOpenMode.Replace,index);}
  public void Open(object arguments){
   int index=arguments is int?(int)arguments:-1;
   if(!CanOpen(index)){router.NotifyClosed(this);return;}
   SelectedBooster=index;generation=model.Generation;wasPaused=model.Paused;
   hop.Cancel();Level.BlockSettingsInput();this.SendCommand(new PauseHarvestCommand(true));
   IsOpen=true;transform.SetAsLastSibling();
   TitleLabel.text=Titles[index];DescriptionLabel.text=Descriptions[index];BoosterIcon.sprite=Icons[index];QuantityLabel.text="×3";PriceLabel.text="1600";
   pointerReleasePending=inputModule.input.touchCount>0||inputModule.input.GetMouseButton(0);inputModule.DeactivateModule();
   motion.Show();RefreshPurchase();
  }
  bool CanInteract=>initialized&&!motion.IsClosing&&IsOpen&&isActiveAndEnabled&&!applicationPaused&&!pointerReleasePending&&Level!=null&&!Level.IsApplicationPaused&&!navigation.IsHome&&model.Generation==generation&&!model.Won;
  void Cancel(){if(CanInteract){motion.Close();RefreshPurchase();}}
  void Purchase(){
   if(!CanInteract)return;
   var command=new PurchaseHarvestBoosterCommand(SelectedBooster,generation);this.SendCommand(command);
   if(command.Accepted){motion.Close();RefreshPurchase();}else RefreshPurchase();
  }
  public void Close(){
   if(!IsOpen)return;
   motion.HideImmediately();IsOpen=false;SelectedBooster=-1;pointerReleasePending=false;PopupContent.SetActive(false);DimBackground.SetActive(false);PurchaseButton.interactable=false;
   if(Level!=null){if(!navigation.IsHome&&!model.Won)Level.RestorePauseAfterSettings(wasPaused,generation);if(inputModule!=null)inputModule.DeactivateModule();Level.ReleaseSettingsInputAfterPointers();}
   router.NotifyClosed(this);
  }
  void RefreshPurchase(){economyRevision=economy.Revision;PurchaseButton.interactable=CanInteract&&model.BoosterCount(SelectedBooster)==0&&economy.Coins>=PurchaseHarvestBoosterCommand.Price;}
  void Update(){
   if(!initialized||!IsOpen)return;
   if(Level==null||navigation.IsHome||model.Won||model.Generation!=generation){Close();return;}
   if(!applicationPaused&&!Level.IsApplicationPaused&&motion.Tick(Time.unscaledDeltaTime)){Close();return;}
   if(economyRevision!=economy.Revision)RefreshPurchase();
  }
  void LateUpdate(){
   if(!initialized||!IsOpen||applicationPaused||Level.IsApplicationPaused||!pointerReleasePending)return;
   var input=inputModule.input;if(input.touchCount>0||input.GetMouseButton(0))return;
   inputModule.DeactivateModule();pointerReleasePending=false;RefreshPurchase();
  }
  void OnApplicationPause(bool value){applicationPaused=value;if(!initialized||!IsOpen)return;if(value){pointerReleasePending=true;if(motion.IsClosing)Close();}inputModule.DeactivateModule();RefreshPurchase();}
  void OnDisable(){if(!initialized)return;Close();router.Unregister(this);}
  void OnEnable(){if(initialized)router.Register(this);}
  void OnDestroy(){if(!initialized)return;router.Unregister(this);CloseButton.onClick.RemoveListener(Cancel);PurchaseButton.onClick.RemoveListener(Purchase);}
 }
}
