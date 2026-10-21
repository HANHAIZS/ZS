using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest {
 public sealed class SettingsPageView : BeeHarvestController {
  public Button[] PreferenceButtons;
  public Image[] PreferenceIcons;
  public GameObject[] OffMarks;
  MainMenuPage page;
  SettingsModel settings;
  bool inputAllowed;
  public void Bind(MainMenuPage owner) {
   RemoveListeners(); page=owner; settings=this.GetModel<SettingsModel>();
   PreferenceButtons[0].onClick.AddListener(ToggleMusic);
   PreferenceButtons[1].onClick.AddListener(ToggleSound);
   PreferenceButtons[2].onClick.AddListener(ToggleHaptic);
   Render();
  }
  public void SetInputEnabled(bool value) {
   inputAllowed=value;
   foreach(var button in PreferenceButtons)button.interactable=value;
   if(settings!=null)Render();
  }
  void ToggleMusic(){Toggle(GamePreference.Music);}
  void ToggleSound(){Toggle(GamePreference.Sound);}
  void ToggleHaptic(){Toggle(GamePreference.Haptic);}
  void Toggle(GamePreference option) {
   if(!inputAllowed||!isActiveAndEnabled||page==null||!page.IsInputReady||page.SettingsPopup.IsOpen||page.Tabs.IsApplicationPaused)return;
   this.SendCommand(new SetPreferenceCommand(option,!settings.IsEnabled(option)));Render();
  }
  public void Render() {
   if(settings==null)return;
   for(int i=0;i<3;i++) {
    bool enabled=settings.IsEnabled((GamePreference)i);
    OffMarks[i].SetActive(!enabled);
    PreferenceIcons[i].color=enabled?Color.white:new Color(.64f,.64f,.64f,1);
   }
  }
  void RemoveListeners() {
   if(PreferenceButtons==null||PreferenceButtons.Length<3)return;
   if(PreferenceButtons[0]!=null)PreferenceButtons[0].onClick.RemoveListener(ToggleMusic);
   if(PreferenceButtons[1]!=null)PreferenceButtons[1].onClick.RemoveListener(ToggleSound);
   if(PreferenceButtons[2]!=null)PreferenceButtons[2].onClick.RemoveListener(ToggleHaptic);
  }
  void OnDestroy(){RemoveListeners();}
 }
}
