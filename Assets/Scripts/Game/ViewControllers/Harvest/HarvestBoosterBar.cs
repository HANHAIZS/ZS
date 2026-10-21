using QFramework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace BeeHarvest {
 public sealed class HarvestBoosterBar:BeeHarvestController {
  public Button[] Buttons,RefillButtons;
  public TMP_Text[] StateLabels,QuantityLabels;
  public GameObject[] QuantityBadges;
  public BoosterPurchasePopupController PurchasePopupPrefab;
  public BoosterPurchasePopupController PurchasePopup {get;private set;}
  HarvestModel model;HarvestSystem rules;LevelOneHud hud;
  int revision=-1,lastSecond=-1;bool inputEnabled=true;
  public void Initialize(HarvestModel state,HarvestSystem system,LevelOneHud controls,LevelOneController level){
   model=state;rules=system;hud=controls;
   PurchasePopup=Instantiate(PurchasePopupPrefab,controls.GetComponentInParent<Canvas>().transform,false);
   PurchasePopup.name="BoosterPurchasePopup";PurchasePopup.Initialize(level);
   for(int i=0;i<Buttons.Length;i++){int action=i;Buttons[i].onClick.RemoveAllListeners();Buttons[i].onClick.AddListener(()=>Click(action));RefillButtons[i].onClick.RemoveAllListeners();RefillButtons[i].onClick.AddListener(()=>Refill(action));}Sync();
  }
  void Click(int action){
   if(!isActiveAndEnabled||!inputEnabled||!rules.CanUseBoosters||model.BoosterCount(action)==0)return;
   if(action==0)this.SendCommand(new AddHiveSlotCommand());
   else if(action==1)this.SendCommand(new UseHarvestSpeedCommand());
   else if(action==2){this.SendCommand(new ShuffleWaitingHivesCommand());hud.ResetQueuePage();}
   else if(action==3){if(model.FreeSelection)return;this.SendCommand(new UseFreeHiveSelectionCommand());hud.ResetQueuePage();}
   Sync();hud.Sync();
  }
  void Refill(int action){if(!isActiveAndEnabled||!inputEnabled||!rules.CanUseBoosters||model.BoosterCount(action)!=0)return;PurchasePopup.Show(action);}
  public void SetInputEnabled(bool value){inputEnabled=value;revision=-1;Sync();}
  public void Sync(){
   if(model==null)return;
   int seconds=Mathf.CeilToInt(model.SpeedSeconds);
   if(seconds!=lastSecond){lastSecond=seconds;StateLabels[1].text=seconds>0?"2x "+seconds/60+":"+(seconds%60).ToString("00"):"";}
   if(revision==model.Revision)return;revision=model.Revision;
   bool available=inputEnabled&&rules.CanUseBoosters;
   for(int i=0;i<Buttons.Length;i++){
    int count=model.BoosterCount(i);
    bool canUse=available&&count>0&&(i!=0||model.Slots.Count<6)&&(i!=2||WaitingCount()>1)&&(i!=3||!model.FreeSelection);
    Buttons[i].interactable=canUse;Buttons[i].targetGraphic.raycastTarget=canUse;
    QuantityBadges[i].SetActive(count>0);QuantityLabels[i].text=count.ToString();
    RefillButtons[i].gameObject.SetActive(count==0);RefillButtons[i].interactable=available&&count==0;
    // Disabled Selectables still cache clicks in uGUI: reject pointer capture during hop/modal/background locks.
    RefillButtons[i].targetGraphic.raycastTarget=available&&count==0;
   }
   bool extra=model.Slots.Count==6;
   StateLabels[0].text=extra?"+1":"";StateLabels[2].text="";StateLabels[3].text="";
   for(int i=0;i<Buttons.Length;i++){bool active=i==0?extra:i==1&&model.SpeedBoost;Buttons[i].targetGraphic.color=active?new Color(1,.83f,.43f):Color.white;}
  }
  // Harvest revisions can stay unchanged while idle; the timer has its own once-per-second display cache.
  void Update(){Sync();}
  int WaitingCount(){int n=0;for(int c=0;c<5;c++)n+=model.WaitingColumn(c).Count;return n;}
  void OnDisable(){if(PurchasePopup!=null)PurchasePopup.Close();}
  void OnDestroy(){if(PurchasePopup!=null)Destroy(PurchasePopup.gameObject);if(Buttons!=null)foreach(var b in Buttons)if(b!=null)b.onClick.RemoveAllListeners();if(RefillButtons!=null)foreach(var b in RefillButtons)if(b!=null)b.onClick.RemoveAllListeners();}
 }
}
