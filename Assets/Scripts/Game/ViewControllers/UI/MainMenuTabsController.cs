using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeeHarvest {
 public sealed class MainMenuTabsController : BeeHarvestController {
  public MainMenuPage Page;
  public Image Background;
  public Sprite HomeBackground, ShopBackground, GalleryBackground;
  public GameObject ShopContent, GalleryContent;
  public SettingsPageView SettingsPage;
  public TMPro.TMP_Text StartLevelLabel;
  public RectTransform NavigationSelection;
  public MainMenuNavigationBarView NavigationBar;
  public Image NavigationBase,NavigationInset;
  public bool IsApplicationPaused {get;private set;}
  public bool IsHomeTab => navigation!=null&&navigation.SelectedTab==MainMenuTab.Home;
  MenuNavigationModel navigation;
  IUnRegister subscription;
  BaseInputModule inputModule;
  AspectRatioFitter backgroundFitter;
  bool inputAllowed;
  public void Bind(MainMenuPage page) {
   RemoveListeners();subscription?.UnRegister();Page=page;
   if(StartLevelLabel==null||Background==null||HomeBackground==null||ShopBackground==null||GalleryBackground==null||ShopContent==null||GalleryContent==null||SettingsPage==null||NavigationSelection==null||NavigationBase==null||NavigationInset==null||NavigationBar==null)
    throw new System.InvalidOperationException("MainMenuTabsController requires its serialized UI references.");
   navigation=this.GetModel<MenuNavigationModel>();
   inputModule=EventSystem.current.GetComponent<BaseInputModule>();
   backgroundFitter=Background.GetComponent<AspectRatioFitter>();
   NavigationBar.ValidateBindings();
   Page.ShopButton.onClick.AddListener(OpenShop);Page.HomeButton.onClick.AddListener(OpenHome);Page.GalleryButton.onClick.AddListener(OpenSettings);
   SettingsPage.Bind(page);
   subscription=this.RegisterEvent<MainMenuTabChangedEvent>(e=>RenderTabs());
  }
  bool CanNavigate=>inputAllowed&&navigation!=null&&navigation.IsHome&&Page.isActiveAndEnabled&&!Page.SettingsPopup.IsOpen&&!IsApplicationPaused;
  void OpenShop(){SelectTab(MainMenuTab.Shop);}
  void OpenHome(){SelectTab(MainMenuTab.Home);}
  void OpenSettings(){SelectTab(MainMenuTab.Gallery);}
  void SelectTab(MainMenuTab tab) {
   if(!CanNavigate||navigation.SelectedTab==tab)return;
   this.SendCommand(new SelectMainMenuTabCommand(tab));
   Page.BlockSettingsInput();inputModule.DeactivateModule();Page.ReleaseSettingsInputAfterPointers();
  }
  public void SetInputEnabled(bool value) {
   inputAllowed=value&&!IsApplicationPaused;
   if(Page==null)return;
   Page.ShopButton.interactable=inputAllowed;Page.HomeButton.interactable=inputAllowed;Page.GalleryButton.interactable=inputAllowed;
   if(SettingsPage!=null)SettingsPage.SetInputEnabled(inputAllowed&&navigation!=null&&navigation.SelectedTab==MainMenuTab.Gallery);
  }
  public void RenderTabs(bool immediate=false) {
   if(navigation==null)return;
   var tab=navigation.SelectedTab;bool settingsTab=tab==MainMenuTab.Gallery;
   Background.sprite=tab==MainMenuTab.Shop?ShopBackground:settingsTab?GalleryBackground:HomeBackground;
   if(backgroundFitter!=null)backgroundFitter.aspectRatio=Background.sprite.rect.width/Background.sprite.rect.height;
   Page.StartGameButton.gameObject.SetActive(tab==MainMenuTab.Home);
   StartLevelLabel.text="level "+Page.DisplayedLevel;
   ShopContent.SetActive(tab==MainMenuTab.Shop);GalleryContent.SetActive(settingsTab);
   Page.Economy.gameObject.SetActive(!settingsTab);Page.SettingsButton.gameObject.SetActive(false);
   NavigationBar.Render(tab,immediate);
   SettingsPage.SetInputEnabled(inputAllowed&&settingsTab);SettingsPage.Render();
  }
  void OnApplicationPause(bool value) {
   IsApplicationPaused=value;
   if(Page==null||navigation==null)return;
   if(value){Page.BlockSettingsInput();inputModule.DeactivateModule();}
   else if(Page.isActiveAndEnabled)Page.ReleaseSettingsInputAfterPointers();
  }
  void RemoveListeners() {
   if(Page!=null){Page.ShopButton.onClick.RemoveListener(OpenShop);Page.HomeButton.onClick.RemoveListener(OpenHome);Page.GalleryButton.onClick.RemoveListener(OpenSettings);}
  }
  void OnDestroy(){RemoveListeners();subscription?.UnRegister();}
 }
}
