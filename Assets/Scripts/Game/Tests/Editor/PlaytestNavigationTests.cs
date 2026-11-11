using System;
using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class PlaytestNavigationTests : EconomyTestFixture {
  IArchitecture App=>BeeHarvestApp.Interface;
  void Send(string name,params object[] args){
   var type=typeof(MenuNavigationModel).Assembly.GetType("BeeHarvest."+name);
   Assert.IsNotNull(type,name+" must be available");
   App.SendCommand((ICommand)Activator.CreateInstance(type,args));
  }
  [Test] public void SelectorRejectsInvalidAndDuplicateRequestsAndReselectsFresh(){
   App.SendCommand(new ResetMenuNavigationSessionCommand());
   App.SendCommand(new InitializeMenuNavigationCommand(true,6));
   Send("OpenPlaytestSelectorCommand");
   Send("SelectPlaytestLevelCommand",0,1f);Assert.IsTrue(App.GetModel<MenuNavigationModel>().IsHome);
   Send("SelectPlaytestLevelCommand",LevelOneAssets.LastLevel+1,1f);Assert.IsTrue(App.GetModel<MenuNavigationModel>().IsHome);
   Send("SelectPlaytestLevelCommand",106,.3f);Assert.AreEqual(106,App.GetModel<MenuNavigationModel>().CurrentLevel);Assert.IsFalse(App.GetModel<MenuNavigationModel>().IsHome);
   Send("SelectPlaytestLevelCommand",1,1f);Assert.AreEqual(106,App.GetModel<MenuNavigationModel>().CurrentLevel);
   Send("OpenPlaytestSelectorCommand");Assert.IsTrue(App.GetModel<MenuNavigationModel>().IsHome);
   Send("SelectPlaytestLevelCommand",106,.3f);Assert.IsFalse(App.GetModel<MenuNavigationModel>().IsHome);
   Send("OpenPlaytestSelectorCommand");Send("ClosePlaytestSelectorCommand");
   Assert.AreEqual(6,App.GetModel<MenuNavigationModel>().CurrentLevel);
  }
  [Test] public void PlaytestCanStartAtZeroStaminaAndCannotWriteItsEconomy(){
   App.SendCommand(new ResetMenuNavigationSessionCommand());
   App.SendCommand(new InitializeMenuNavigationCommand(true,8));
   for(int i=0;i<5;i++)App.SendCommand(new SpendStaminaCommand());
   var economy=App.GetModel<EconomyModel>();var repo=App.GetUtility<EconomyRepository>();int coins=economy.Coins;
   Assert.AreEqual(0,economy.Stamina);
   Send("OpenPlaytestSelectorCommand");Send("SelectPlaytestLevelCommand",119,1f);
   Assert.AreEqual(119,App.GetModel<MenuNavigationModel>().CurrentLevel);Assert.Greater(economy.Stamina,0);
   int energy=economy.Stamina;App.SendCommand(new SpendStaminaCommand());Assert.AreEqual(energy,economy.Stamina);
   App.SendCommand(new AddCoinsCommand());Assert.AreEqual(coins,repo.Load().coins);Assert.AreEqual(0,repo.Load().stamina);
   Send("OpenPlaytestSelectorCommand");Send("ClosePlaytestSelectorCommand");
   Assert.AreEqual(8,App.GetModel<MenuNavigationModel>().CurrentLevel);Assert.AreEqual(coins,economy.Coins);Assert.AreEqual(0,economy.Stamina);
  }
  [TearDown] public void ResetSelection(){App.SendCommand(new ResetMenuNavigationSessionCommand());}
 }
}
