using System.Collections;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace BeeHarvest {
 [DefaultExecutionOrder(100)]
 public sealed class MainMenuNavigationController : BeeHarvestController {
  public LevelOneController Level;
  public Canvas Canvas;
  public Camera LevelCamera;
  public MainMenuPage Page {get;private set;}
  MenuNavigationModel navigation;
  IUnRegister subscription;
  BaseInputModule inputModule;
  LevelSelectPanel levelSelect;
  int levelMask;
  bool ready,loading,applicationPaused;
  void Start(){
   navigation=this.GetModel<MenuNavigationModel>();var assets=this.GetUtility<MainMenuAssets>();
   var instance=Instantiate(assets.Page(),Canvas.transform,false);instance.name="MainMenuPage";
   Page=instance.GetComponent<MainMenuPage>();Page.Bind(StartGame);
   if(LevelCamera!=null)levelMask=LevelCamera.cullingMask;
   inputModule=EventSystem.current.GetComponent<BaseInputModule>();
   if(PlaytestBuild.Available)BindLevelSelect();
   subscription=this.RegisterEvent<MenuNavigationChangedEvent>(e=>Render());ready=true;
   if(!navigation.Initialized)this.SendCommand(new InitializeMenuNavigationCommand(Level==null,Level!=null?Level.LevelNumber:1));
   else Render();
   var audio=GetComponent<GameAudioController>();if(audio==null)audio=gameObject.AddComponent<GameAudioController>();audio.Initialize(Level);
  }
  void BindLevelSelect(){
   var start=(RectTransform)Page.StartGameButton.transform;
   var font=Page.Tabs.StartLevelLabel.font;
   var button=PlaytestUi.Button("LevelSelectButton",start.parent,"Level Select",font,34,.5f,.27f,.5f,.27f);
   var rect=(RectTransform)button.transform;
   rect.sizeDelta=new Vector2(310,78);
   rect.anchoredPosition=start.anchoredPosition+new Vector2(0,start.sizeDelta.y*.5f+51);
   button.onClick.AddListener(()=>{
    if(ready&&!loading&&!applicationPaused&&Page.IsInputReady&&Page.Tabs.IsHomeTab&&!Page.SettingsPopup.IsOpen)
     this.SendCommand(new OpenPlaytestSelectorCommand());
   });
   Page.PlaytestEntry=button;
   ButtonPressMotion.Install(button.gameObject);
   var root=PlaytestUi.Rect("LevelSelectPanel",start.parent,0,0,1,1);
   root.gameObject.SetActive(false);
   levelSelect=root.gameObject.AddComponent<LevelSelectPanel>();
   levelSelect.Build(Page,navigation,inputModule);
  }
  public void ReturnHome(bool resumePaused){
   if(!ready||!isActiveAndEnabled||loading||Level==null||Level.IsApplicationPaused)return;
   this.SendCommand(new ReturnHomeCommand(resumePaused));
  }
  void StartGame(){
   if(!ready||!isActiveAndEnabled||loading||applicationPaused||(Level!=null&&Level.IsApplicationPaused))return;
   this.SendCommand(new StartGameCommand());
  }
  void Render(){
   if(!ready)return;
   if(inputModule!=null)inputModule.DeactivateModule();
   if(levelSelect!=null&&!navigation.LevelSelectOpen)levelSelect.Present(false);
   Page.SetLevelSelectOpen(navigation.LevelSelectOpen);
   if(navigation.IsHome){
    if(Level!=null){Level.BlockSettingsInput();Level.Hud.gameObject.SetActive(false);}
    if(LevelCamera!=null)LevelCamera.cullingMask=0;
    Page.Show(navigation.CurrentLevel);
    if(levelSelect!=null)levelSelect.Present(navigation.LevelSelectOpen);
   }else if(Level!=null&&Level.MatchesNavigation){
    Level.BlockSettingsInput();
    Page.Hide();if(LevelCamera!=null)LevelCamera.cullingMask=levelMask;
    Level.Hud.gameObject.SetActive(true);Level.ReleaseSettingsInputAfterPointers();
   }else if(!loading){loading=true;Page.BlockSettingsInput();
    if(Level!=null){Level.BlockSettingsInput();Level.Hud.gameObject.SetActive(false);}
    StartCoroutine(LoadLevel());}
  }
  IEnumerator LoadLevel(){
   // Defers load beyond the Command/Event callback, preventing scene teardown during dispatch.
   yield return null;
   var operation=SceneManager.LoadSceneAsync(LevelOneAssets.GameSceneName);
   if(operation!=null)yield return operation;
  }
  void OnApplicationPause(bool value){applicationPaused=value;}
  void OnDestroy(){subscription?.UnRegister();}
 }
}
