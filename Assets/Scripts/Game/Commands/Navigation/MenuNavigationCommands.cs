using QFramework;
namespace BeeHarvest {
 public sealed class InitializeMenuNavigationCommand : AbstractCommand {
  readonly bool home;readonly int levelNumber;
  public InitializeMenuNavigationCommand(bool value,int number=1){home=value;levelNumber=number;}
  protected override void OnExecute(){
   LevelOneAssets.SceneName(levelNumber);
   var nav=this.GetModel<MenuNavigationModel>();nav.CurrentLevel=levelNumber;nav.IsHome=home;nav.ResumePaused=false;nav.SelectedTab=MainMenuTab.Home;nav.GalleryPage=0;
   if(home)this.SendCommand(new PauseHarvestCommand(true));
   this.SendEvent(new MenuNavigationChangedEvent());
  }
 }
 public sealed class ReturnHomeCommand : AbstractCommand {
  readonly bool previousPause;
  public ReturnHomeCommand(bool value){previousPause=value;}
  protected override void OnExecute(){
   var nav=this.GetModel<MenuNavigationModel>();if(nav.IsHome)return;
   var harvest=this.GetModel<HarvestModel>();
   if(harvest.Cubes.Count>0&&!harvest.Won)this.SendCommand(new SpendStaminaCommand());
   nav.ResumePaused=previousPause;nav.IsHome=true;nav.SelectedTab=MainMenuTab.Home;nav.GalleryPage=0;
   this.SendCommand(new PauseHarvestCommand(true));this.SendEvent(new MenuNavigationChangedEvent());
  }
 }
 public sealed class StartGameCommand : AbstractCommand {
  protected override void OnExecute(){
   var nav=this.GetModel<MenuNavigationModel>();if(!nav.IsHome)return;
   this.SendCommand(new RefreshEconomyCommand());if(this.GetModel<EconomyModel>().Stamina<=0)return;
   nav.IsHome=false;this.SendCommand(new PauseHarvestCommand(nav.ResumePaused));
   this.SendEvent(new MenuNavigationChangedEvent());
  }
 }
 public sealed class ContinueLevelCommand : AbstractCommand {
  readonly int levelNumber,generation;
  public ContinueLevelCommand(int number,int expectedGeneration){levelNumber=number;generation=expectedGeneration;}
  protected override void OnExecute(){
   var nav=this.GetModel<MenuNavigationModel>();var model=this.GetModel<HarvestModel>();
   if(nav.IsHome||!model.Won||model.Generation!=generation||nav.CurrentLevel!=levelNumber)return;
   bool final=levelNumber>=LevelOneAssets.LastLevel;
   nav.CurrentLevel=final?1:levelNumber+1;nav.IsHome=final;nav.ResumePaused=false;
   nav.SelectedTab=MainMenuTab.Home;nav.GalleryPage=0;
   this.SendCommand(new PauseHarvestCommand(true));
   this.SendEvent(new MenuNavigationChangedEvent());
  }
 }
}