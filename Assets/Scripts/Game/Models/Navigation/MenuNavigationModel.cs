using QFramework;
namespace BeeHarvest {
 public enum MainMenuTab { Home, Shop, Gallery }
 public sealed class MenuNavigationModel : AbstractModel {
  public int CurrentLevel {get;internal set;}=1;
  public bool IsHome {get;internal set;}
  public bool ResumePaused {get;internal set;}
  public MainMenuTab SelectedTab {get;internal set;}
  public int GalleryPage {get;internal set;}
  protected override void OnInit() {}
 }
}
