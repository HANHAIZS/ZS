using System;
using System.Reflection;
using NUnit.Framework;
using QFramework;
using GameFoundation.Core;
using GameFoundation.Save;
namespace BeeHarvest.Tests {
 public sealed class EconomyTestApp : Architecture<EconomyTestApp> { protected override void Init(){} }
 public sealed class EconomyClock : IFoundationClock { public long Seconds=1000000; public DateTimeOffset UtcNow=>DateTimeOffset.FromUnixTimeSeconds(Seconds); public float RealtimeSinceStartup=>0; }
 public sealed class EconomyTests : EconomyTestFixture {
  IArchitecture app; EconomyClock clock; MemorySaveStore store; object model,repo; Type modelType;
  static Type Type(string name){var t=typeof(HarvestModel).Assembly.GetType("BeeHarvest."+name);Assert.IsNotNull(t,"Missing economy type: "+name);return t;}
  void Register(string method,Type t,object value){app.GetType().GetMethod(method).MakeGenericMethod(t).Invoke(app,new[]{value});}
  void RegisterEconomy(){repo=Activator.CreateInstance(Type("EconomyRepository"),store,new JsonUtilitySaveSerializer(),clock);Register("RegisterUtility",repo.GetType(),repo);modelType=Type("EconomyModel");model=Activator.CreateInstance(modelType);Register("RegisterModel",modelType,model);Register("RegisterSystem",Type("EconomySystem"),Activator.CreateInstance(Type("EconomySystem")));}
  void Command(string name,params object[] args){app.SendCommand((ICommand)Activator.CreateInstance(Type(name),args));}
  int Coins=>(int)modelType.GetProperty("Coins").GetValue(model);
  int Stamina=>(int)modelType.GetProperty("Stamina").GetValue(model);
  long Deadline=>(long)modelType.GetProperty("NextRecoveryAt").GetValue(model);
  void Set(string property,object value){modelType.GetProperty(property).SetValue(model,value);}
  void Navigation(bool home){typeof(MenuNavigationModel).GetProperty("IsHome").SetValue(app.GetModel<MenuNavigationModel>(),home);}
  static LevelData Data()=>new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}};
  void Begin(){app.SendCommand(new StartLevelCommand(Data()));Navigation(false);}
  [SetUp] public void Setup(){app=EconomyTestApp.Interface;clock=new EconomyClock();store=new MemorySaveStore();app.RegisterUtility<IFoundationClock>(clock);app.RegisterModel(new MenuNavigationModel());app.RegisterModel(new HarvestModel());RegisterEconomy();}
  [Test] public void DefaultsAndPlusGrantsMatchCounters(){Assert.AreEqual(1600,Coins);Assert.AreEqual(5,Stamina);Assert.AreEqual(0,Deadline);Command("AddCoinsCommand");Assert.AreEqual(2100,Coins);Command("AddStaminaCommand");Assert.AreEqual(5,Stamina);}
  [Test] public void HomeChargesOnceAndResumePreservesLevel(){Begin();int g=app.GetModel<HarvestModel>().Generation;app.SendCommand(new ReturnHomeCommand(false));Assert.AreEqual(4,Stamina);Assert.AreEqual(1000600,Deadline);app.SendCommand(new ReturnHomeCommand(false));Assert.AreEqual(4,Stamina);app.SendCommand(new StartGameCommand());Assert.IsFalse(app.GetModel<MenuNavigationModel>().IsHome);Assert.AreEqual(g,app.GetModel<HarvestModel>().Generation);Assert.AreEqual(4,Stamina);}
  [Test] public void SubsequentSpendsAndPlusKeepPartialRecovery(){Begin();app.SendCommand(new ReturnHomeCommand(false));clock.Seconds+=120;app.SendCommand(new StartGameCommand());app.SendCommand(new ReturnHomeCommand(false));Assert.AreEqual(3,Stamina);Assert.AreEqual(1000600,Deadline);Command("AddStaminaCommand");Assert.AreEqual(4,Stamina);Assert.AreEqual(1000600,Deadline);Command("AddStaminaCommand");Assert.AreEqual(5,Stamina);Assert.AreEqual(0,Deadline);}
  [Test] public void RecoveryUsesExactDeadlineAndCatchesUpOffline(){Set("Stamina",1);Set("NextRecoveryAt",1000600L);clock.Seconds=1000599;Command("RefreshEconomyCommand");Assert.AreEqual(1,Stamina);clock.Seconds=1000600;Command("RefreshEconomyCommand");Assert.AreEqual(2,Stamina);Assert.AreEqual(1001200,Deadline);clock.Seconds=1002100;Command("RefreshEconomyCommand");Assert.AreEqual(4,Stamina);Assert.AreEqual(1002400,Deadline);clock.Seconds=1002400;Command("RefreshEconomyCommand");Assert.AreEqual(5,Stamina);Assert.AreEqual(0,Deadline);}
  [Test] public void LastPointAllowsRetryAndZeroRejectsStartAndRetry(){Begin();Set("Stamina",1);int g=app.GetModel<HarvestModel>().Generation;Command("RetryLevelCommand",Data());Assert.AreEqual(0,Stamina);Assert.AreEqual(g+1,app.GetModel<HarvestModel>().Generation);Command("RetryLevelCommand",Data());Assert.AreEqual(g+1,app.GetModel<HarvestModel>().Generation);app.SendCommand(new ReturnHomeCommand(false));Assert.AreEqual(0,Stamina);app.SendCommand(new StartGameCommand());Assert.IsTrue(app.GetModel<MenuNavigationModel>().IsHome);Command("AddStaminaCommand");app.SendCommand(new StartGameCommand());Assert.IsFalse(app.GetModel<MenuNavigationModel>().IsHome);}
  [Test] public void VictoryContinueDoesNotChargeStamina(){Begin();var h=app.GetModel<HarvestModel>();typeof(HarvestModel).GetProperty("Won").SetValue(h,true);app.SendCommand(new ContinueLevelCommand(1,h.Generation));Assert.AreEqual(5,Stamina);Assert.AreEqual(2,app.GetModel<MenuNavigationModel>().CurrentLevel);Navigation(false);app.SendCommand(new ReturnHomeCommand(false));Assert.AreEqual(5,Stamina);}
  [Test] public void BalancesAndDeadlineRoundTripAfterClosedTime(){Begin();Command("AddCoinsCommand");app.SendCommand(new ReturnHomeCommand(false));clock.Seconds+=610;RegisterEconomy();Command("RefreshEconomyCommand");Assert.AreEqual(2100,Coins);Assert.AreEqual(5,Stamina);Assert.AreEqual(0,Deadline);}
  [Test] public void CorruptPrimaryRestoresProjectBackup(){Command("AddCoinsCommand");Command("AddCoinsCommand");store.Write("BeeHarvest.economy","broken json");RegisterEconomy();Assert.AreEqual(2600,Coins);Assert.AreEqual(5,Stamina);}
  [Test] public void FutureSaveVersionIsNotOverwritten(){var future=new VersionedSaveService<object>("BeeHarvest.economy",2,store,new JsonUtilitySaveSerializer(),clock,null);future.Save(new object());string before;store.TryRead("BeeHarvest.economy",out before);RegisterEconomy();Command("AddCoinsCommand");string after;store.TryRead("BeeHarvest.economy",out after);Assert.AreEqual(before,after);}
  [TestCase(false)] [TestCase(true)] public void FuturePrimaryWithBackupIsNeverRestoredOrOverwritten(bool corruptBackup){
   Command("AddCoinsCommand");
   if(corruptBackup)store.Write("BeeHarvest.economy","invalid backup");
   var future=new VersionedSaveService<EconomySaveData>("BeeHarvest.economy",2,store,new JsonUtilitySaveSerializer(),clock,null);
   future.Save(new EconomySaveData{coins=9999,stamina=2,nextRecoveryAt=1000600});
   string before;store.TryRead("BeeHarvest.economy",out before);RegisterEconomy();Command("AddCoinsCommand");string after;store.TryRead("BeeHarvest.economy",out after);Assert.AreEqual(before,after);
  }
  [Test] public void ClockRollbackDoesNotGiveFreeStaminaOrHugeCountdown(){Set("Stamina",3);Set("NextRecoveryAt",1000600L);clock.Seconds=999000;Command("RefreshEconomyCommand");Assert.AreEqual(3,Stamina);Assert.AreEqual(999600,Deadline);clock.Seconds=999600;Command("RefreshEconomyCommand");Assert.AreEqual(4,Stamina);}
 }
}
