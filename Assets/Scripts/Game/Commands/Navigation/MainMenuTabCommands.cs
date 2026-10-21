using QFramework;
namespace BeeHarvest {
 public sealed class SelectMainMenuTabCommand : AbstractCommand {
  readonly MainMenuTab tab;
  public SelectMainMenuTabCommand(MainMenuTab value){tab=value;}
  protected override void OnExecute(){
   var navigation=this.GetModel<MenuNavigationModel>();
   if(!navigation.IsHome||tab<MainMenuTab.Home||tab>MainMenuTab.Gallery||navigation.SelectedTab==tab)return;
   navigation.SelectedTab=tab;navigation.GalleryPage=0;
   this.SendEvent(new MainMenuTabChangedEvent());
  }
 }
 public sealed class SetGalleryPageCommand : AbstractCommand {
  readonly int page;
  public SetGalleryPageCommand(int value){page=value;}
  protected override void OnExecute(){
   var navigation=this.GetModel<MenuNavigationModel>();
   if(!navigation.IsHome||navigation.SelectedTab!=MainMenuTab.Gallery||page<0||page>1||navigation.GalleryPage==page)return;
   navigation.GalleryPage=page;this.SendEvent(new MainMenuTabChangedEvent());
  }
 }
}
