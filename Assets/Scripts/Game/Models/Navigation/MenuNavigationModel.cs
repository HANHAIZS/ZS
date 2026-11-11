using QFramework;
namespace BeeHarvest {
 public enum MainMenuTab { Home, Shop, Gallery }
 public sealed class MenuNavigationModel : AbstractModel {
  public bool Initialized {get;internal set;}
  public int CurrentLevel {get;internal set;}=1;
  public bool IsHome {get;internal set;}
  public bool ResumePaused {get;internal set;}
  public MainMenuTab SelectedTab {get;internal set;}
  public int GalleryPage {get;internal set;}
  public bool IsPlaytest {get;internal set;}
  public bool LevelSelectOpen {get;internal set;}
  public int LastPlaytestLevel {get;internal set;}
  public float LevelSelectScroll {get;internal set;}=1f;
  public int LevelLoadVersion {get;internal set;}
  internal int NormalLevelBeforePlaytest;
  internal int[] NormalBoostersBeforePlaytest;
  protected override void OnInit() {}
 }
}
