using System;
using QFramework;

namespace BeeHarvest {
 internal static class PlaytestRoute {
  internal const int LevelsPerGroup=30;
  internal static void Begin(IArchitecture app,MenuNavigationModel nav){
   if(nav.IsPlaytest)return;
   var harvest=app.GetModel<HarvestModel>();
   nav.NormalLevelBeforePlaytest=nav.CurrentLevel;
   nav.NormalBoostersBeforePlaytest=(int[])harvest.boosterCounts.Clone();
   app.GetModel<EconomyModel>().BeginPlaytest();
   nav.IsPlaytest=true;
  }
  internal static void End(IArchitecture app,MenuNavigationModel nav){
   if(!nav.IsPlaytest)return;
   var harvest=app.GetModel<HarvestModel>();
   Array.Copy(nav.NormalBoostersBeforePlaytest,harvest.boosterCounts,harvest.boosterCounts.Length);
   nav.NormalBoostersBeforePlaytest=null;
   harvest.Revision++;
   app.GetModel<EconomyModel>().EndPlaytest();
   nav.CurrentLevel=nav.NormalLevelBeforePlaytest;
   nav.IsPlaytest=false;
   nav.ResumePaused=false;
   nav.LevelLoadVersion++;
  }
  internal static void Enter(IArchitecture app,MenuNavigationModel nav,int level,float scroll){
   nav.CurrentLevel=level;
   nav.LastPlaytestLevel=level;
   nav.LevelSelectScroll=float.IsNaN(scroll)?1f:Math.Max(0f,Math.Min(1f,scroll));
   nav.LevelSelectOpen=false;
   nav.IsHome=false;
   nav.ResumePaused=false;
   nav.SelectedTab=MainMenuTab.Home;
   nav.LevelLoadVersion++;
   var harvest=app.GetModel<HarvestModel>();
   for(int i=0;i<harvest.boosterCounts.Length;i++)harvest.boosterCounts[i]=1;
   app.SendCommand(new PauseHarvestCommand(true));
   app.SendEvent(new MenuNavigationChangedEvent());
  }
 }

 public sealed class OpenPlaytestSelectorCommand : AbstractCommand {
  protected override void OnExecute(){
   var nav=this.GetModel<MenuNavigationModel>();
   if(!PlaytestBuild.Available||!nav.Initialized||nav.LevelSelectOpen||
      (nav.IsHome?nav.SelectedTab!=MainMenuTab.Home:!nav.IsPlaytest))return;
   nav.IsHome=true;
   nav.LevelSelectOpen=true;
   nav.ResumePaused=false;
   nav.SelectedTab=MainMenuTab.Home;
   this.SendCommand(new PauseHarvestCommand(true));
   this.SendEvent(new MenuNavigationChangedEvent());
  }
 }

 public sealed class SelectPlaytestLevelCommand : AbstractCommand {
  readonly int level;
  readonly float scroll;
  public SelectPlaytestLevelCommand(int number,float position){level=number;scroll=position;}
  protected override void OnExecute(){
   var nav=this.GetModel<MenuNavigationModel>();
   if(!PlaytestBuild.Available||!nav.IsHome||!nav.LevelSelectOpen||level<1||level>LevelOneAssets.LastLevel)return;
   var app=((IBelongToArchitecture)this).GetArchitecture();
   PlaytestRoute.Begin(app,nav);
   PlaytestRoute.Enter(app,nav,level,scroll);
  }
 }

 public sealed class ClosePlaytestSelectorCommand : AbstractCommand {
  protected override void OnExecute(){
   var nav=this.GetModel<MenuNavigationModel>();
   if(!nav.IsHome||!nav.LevelSelectOpen)return;
   nav.LevelSelectOpen=false;
   PlaytestRoute.End(((IBelongToArchitecture)this).GetArchitecture(),nav);
   this.SendEvent(new MenuNavigationChangedEvent());
  }
 }
}
