using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class LeaveLevelTests : EconomyTestFixture {
  IArchitecture app; HarvestModel harvest; MenuNavigationModel nav; EconomyModel economy;
  static LevelData Data()=>new LevelData {cubes=new[]{new CubeData{id=0,color="red"},new CubeData{id=1,x=1,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=2}}};
  [SetUp] public void Setup(){app=BeeHarvestApp.Interface;harvest=app.GetModel<HarvestModel>();nav=app.GetModel<MenuNavigationModel>();economy=app.GetModel<EconomyModel>();app.SendCommand(new StartLevelCommand(Data()));app.SendCommand(new InitializeMenuNavigationCommand(false,3));}
  object Leave(LevelData data,int level,int generation){var type=typeof(HarvestModel).Assembly.GetType("BeeHarvest.LeaveLevelCommand");Assert.IsNotNull(type,"Quit confirmation needs an atomic leave/reset command");var command=Activator.CreateInstance(type,data,level,generation);app.SendCommand((ICommand)command);return command;}
  bool Accepted(object command)=>(bool)command.GetType().GetProperty("Accepted").GetValue(command);
  [Test] public void ConfirmationResetsSameLevelAndSpendsExactlyOnce(){
   app.SendCommand(new ActivateHiveCommand(0));int generation=harvest.Generation;
   var command=Leave(Data(),3,generation);Assert.IsTrue(Accepted(command));Assert.AreEqual(4,economy.Stamina);Assert.IsTrue(nav.IsHome);Assert.AreEqual(3,nav.CurrentLevel);Assert.IsFalse(nav.ResumePaused);Assert.AreEqual(generation+1,harvest.Generation);Assert.AreEqual(2,harvest.Remaining);Assert.AreEqual(0,harvest.Deposited);Assert.IsTrue(harvest.Hives.All(h=>!h.Started));Assert.IsTrue(harvest.Slots.All(s=>s<0));Assert.AreEqual(0,harvest.Flights.Count);Assert.IsTrue(harvest.Paused);
   Assert.IsFalse(Accepted(Leave(Data(),3,generation)));Assert.AreEqual(4,economy.Stamina);
   app.SendCommand(new StartGameCommand());Assert.IsFalse(nav.IsHome);Assert.IsFalse(harvest.Paused);Assert.AreEqual(3,nav.CurrentLevel);Assert.AreEqual(generation+1,harvest.Generation);
  }
  [Test] public void StaleLevelAndGenerationDoNotResetOrSpend(){int generation=harvest.Generation;Assert.IsFalse(Accepted(Leave(Data(),2,generation)));Assert.IsFalse(Accepted(Leave(Data(),3,generation-1)));Assert.IsFalse(nav.IsHome);Assert.AreEqual(generation,harvest.Generation);Assert.AreEqual(5,economy.Stamina);}
  [Test] public void ZeroStaminaStillAllowsLeavingWithoutGoingNegative(){for(int i=0;i<5;i++)app.SendCommand(new SpendStaminaCommand());Assert.IsTrue(Accepted(Leave(Data(),3,harvest.Generation)));Assert.IsTrue(nav.IsHome);Assert.AreEqual(0,economy.Stamina);app.SendCommand(new StartGameCommand());Assert.IsTrue(nav.IsHome);}
  [Test] public void InvalidResetDataCannotSpendOrNavigate(){int generation=harvest.Generation;Assert.Throws<ArgumentException>(()=>Leave(new LevelData(),3,generation));Assert.AreEqual(5,economy.Stamina);Assert.IsFalse(nav.IsHome);Assert.AreEqual(generation,harvest.Generation);}
 }
}
