using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest {
 public sealed class EconomyHeaderView : BeeHarvestController {
  public TMP_Text CoinsLabel,StaminaLabel,RecoveryLabel;
  public Button CoinsPlus,StaminaPlus;
  MainMenuPage page;
  EconomyModel model;
  EconomySystem rules;
  bool ready,applicationPaused;
  float nextRefresh;
  int lastCoins=-1,lastStamina=-1,lastSeconds=-1;
  public bool HasStamina=>model!=null&&model.Stamina>0;
  public void Bind(MainMenuPage owner){
   if(ready){CoinsPlus.onClick.RemoveListener(AddCoins);StaminaPlus.onClick.RemoveListener(AddStamina);}
   page=owner;model=this.GetModel<EconomyModel>();rules=this.GetSystem<EconomySystem>();
   CoinsPlus.onClick.AddListener(AddCoins);StaminaPlus.onClick.AddListener(AddStamina);ready=true;Refresh();
  }
  bool CanGrant=>ready&&isActiveAndEnabled&&page.IsInputReady&&!page.SettingsPopup.IsOpen&&!page.Tabs.IsApplicationPaused&&!applicationPaused;
  void AddCoins(){if(!CanGrant)return;this.SendCommand(new AddCoinsCommand());Render();}
  void AddStamina(){if(!CanGrant)return;this.SendCommand(new AddStaminaCommand());Render();}
  public void Refresh(){if(!ready)return;this.SendCommand(new RefreshEconomyCommand());nextRefresh=Time.unscaledTime+1;Render();}
  public void SetInputEnabled(bool value){if(!ready)return;CoinsPlus.interactable=value&&!applicationPaused;StaminaPlus.interactable=value&&!applicationPaused&&model.Stamina<5;}
  void Render(){
   if(lastCoins!=model.Coins){CoinsLabel.text=model.Coins.ToString();lastCoins=model.Coins;}
   if(lastStamina!=model.Stamina){StaminaLabel.text=model.Stamina.ToString();lastStamina=model.Stamina;}
   int seconds=rules.SecondsUntilRecovery(model);
   if(lastSeconds!=seconds){RecoveryLabel.text=model.Stamina>=5?"full":(seconds/60).ToString("00")+":"+(seconds%60).ToString("00");lastSeconds=seconds;}
   SetInputEnabled(CanGrant);
   if(page.IsInputReady)page.StartGameButton.interactable=HasStamina;
  }
  void Update(){if(ready&&Time.unscaledTime>=nextRefresh&&!applicationPaused)Refresh();}
  void OnEnable(){if(ready)Refresh();}
  void OnApplicationPause(bool value){applicationPaused=value;if(value)SetInputEnabled(false);else if(ready)Refresh();}
  void OnDestroy(){if(!ready)return;CoinsPlus.onClick.RemoveListener(AddCoins);StaminaPlus.onClick.RemoveListener(AddStamina);}
 }
}
