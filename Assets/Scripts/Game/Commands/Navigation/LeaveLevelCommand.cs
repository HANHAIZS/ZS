using QFramework;
namespace BeeHarvest {
 /// <summary>Commits a confirmed departure once, resetting only the current level.</summary>
 public sealed class LeaveLevelCommand : AbstractCommand {
  readonly LevelData data;
  readonly int levelNumber,generation;
  public bool Accepted {get;private set;}
  public LeaveLevelCommand(LevelData value,int number,int expectedGeneration){data=value;levelNumber=number;generation=expectedGeneration;}
  protected override void OnExecute(){
   var nav=this.GetModel<MenuNavigationModel>();var harvest=this.GetModel<HarvestModel>();
   if(nav.IsHome||nav.CurrentLevel!=levelNumber||harvest.Generation!=generation||harvest.Cubes.Count==0)return;
   // StartLevel validates all input before mutation, so malformed data cannot charge or navigate.
   this.SendCommand(new StartLevelCommand(data));
   this.SendCommand(new SpendStaminaCommand());
   nav.IsHome=true;nav.ResumePaused=false;nav.SelectedTab=MainMenuTab.Home;nav.GalleryPage=0;
   this.SendCommand(new PauseHarvestCommand(true));Accepted=true;
   this.SendEvent(new MenuNavigationChangedEvent());
  }
 }
}
