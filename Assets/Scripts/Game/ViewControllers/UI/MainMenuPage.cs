using System;
using GameFoundation.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BeeHarvest {
 public sealed class MainMenuPage : BeeHarvestController, IUiScreenView {
  public Button StartGameButton,ShopButton,HomeButton,GalleryButton,SettingsButton;
  public SettingsPopupController SettingsPopup;
  public MainMenuTabsController Tabs;
  public EconomyHeaderView Economy;
  public bool IsInputReady {get;private set;}
  public int DisplayedLevel {get;private set;}=1;
  Action start;
  BaseInputModule inputModule;
  bool uiInputReleasePending;
  public string ScreenId=>"MainMenu";
  public bool IsVisible=>gameObject.activeSelf;
  public void Bind(Action action){
   StartGameButton.onClick.RemoveListener(StartGame);start=action;
   StartGameButton.onClick.AddListener(StartGame);
   Tabs.Bind(this);
   SettingsPopup.MenuPage=this;SettingsPopup.OpenButton=SettingsButton;
   inputModule=EventSystem.current.GetComponent<BaseInputModule>();
   Economy.Bind(this);
  }
  void StartGame(){if(isActiveAndEnabled&&StartGameButton.isActiveAndEnabled&&StartGameButton.interactable&&Tabs.IsHomeTab&&!Tabs.IsApplicationPaused&&!SettingsPopup.IsOpen)start?.Invoke();}
  public void BlockSettingsInput(){IsInputReady=false;Economy?.SetInputEnabled(false);uiInputReleasePending=false;StartGameButton.interactable=false;SettingsButton.interactable=false;Tabs.SetInputEnabled(false);}
  public void ReleaseSettingsInputAfterPointers(){uiInputReleasePending=true;}
  void LateUpdate(){
   if(!uiInputReleasePending||SettingsPopup.IsOpen||Tabs.IsApplicationPaused)return;
   var input=inputModule.input;
   if(input.touchCount>0||input.GetMouseButton(0))return;
   uiInputReleasePending=false;IsInputReady=true;StartGameButton.interactable=Economy.HasStamina;Economy.SetInputEnabled(true);SettingsButton.interactable=true;Tabs.SetInputEnabled(true);
  }
  public void Show(object arguments){if(arguments is int)DisplayedLevel=(int)arguments;gameObject.SetActive(true);Tabs.RenderTabs(true);BlockSettingsInput();ReleaseSettingsInputAfterPointers();}
  public void Hide(){gameObject.SetActive(false);IsInputReady=false;uiInputReleasePending=false;}
  void OnDestroy(){if(StartGameButton!=null)StartGameButton.onClick.RemoveListener(StartGame);}
 }
}
