using System.Collections;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
  int levelMask;
  bool ready,loading,applicationPaused;
  void Start(){
   navigation=this.GetModel<MenuNavigationModel>();var assets=this.GetUtility<MainMenuAssets>();
   var instance=Instantiate(assets.Page(),Canvas.transform,false);instance.name="MainMenuPage";
   Page=instance.GetComponent<MainMenuPage>();Page.Bind(StartGame);
   if(LevelCamera!=null)levelMask=LevelCamera.cullingMask;
   inputModule=EventSystem.current.GetComponent<BaseInputModule>();
   subscription=this.RegisterEvent<MenuNavigationChangedEvent>(e=>Render());ready=true;
   this.SendCommand(new InitializeMenuNavigationCommand(Level==null,Level!=null?Level.LevelNumber:1));
   var audio=GetComponent<GameAudioController>();if(audio==null)audio=gameObject.AddComponent<GameAudioController>();audio.Initialize(Level);
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
   if(navigation.IsHome){
    if(Level!=null){Level.BlockSettingsInput();Level.Hud.gameObject.SetActive(false);}
    if(LevelCamera!=null)LevelCamera.cullingMask=0;
    Page.Show(navigation.CurrentLevel);
   }else if(Level!=null&&Level.LevelNumber==navigation.CurrentLevel){
    Level.BlockSettingsInput();
    Page.Hide();if(LevelCamera!=null)LevelCamera.cullingMask=levelMask;
    Level.Hud.gameObject.SetActive(true);Level.ReleaseSettingsInputAfterPointers();
   }else if(!loading){loading=true;Page.StartGameButton.interactable=false;
    if(Level!=null){Level.BlockSettingsInput();Level.Hud.gameObject.SetActive(false);}
    StartCoroutine(LoadLevel(navigation.CurrentLevel));}
  }
  IEnumerator LoadLevel(int number){
   // Defers load beyond the Command/Event callback, preventing scene teardown during dispatch.
   yield return null;
   var operation=SceneManager.LoadSceneAsync(LevelOneAssets.SceneName(number));
   if(operation!=null)yield return operation;
  }
  void OnApplicationPause(bool value){applicationPaused=value;}
  void OnDestroy(){subscription?.UnRegister();}
 }
}
