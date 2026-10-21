using System;
using System.Reflection;
using NUnit.Framework;
using QFramework;
namespace BeeHarvest.Tests {
 public sealed class PurchaseHarvestBoosterTests : EconomyTestFixture {
  IArchitecture app; HarvestModel model; EconomyModel economy; int[] stock,previousStock;
  [TearDown] public void RestoreInventory(){previousStock.CopyTo(stock,0);}
  [SetUp] public void Setup(){app=BeeHarvestApp.Interface;model=app.GetModel<HarvestModel>();economy=app.GetModel<EconomyModel>();app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));app.SendCommand(new InitializeMenuNavigationCommand(false,2));app.SendCommand(new PauseHarvestCommand(true));stock=(int[])typeof(HarvestModel).GetField("boosterCounts",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(model);previousStock=(int[])stock.Clone();Array.Clear(stock,0,stock.Length);}
  object Buy(int index,int generation){var t=typeof(HarvestModel).Assembly.GetType("BeeHarvest.PurchaseHarvestBoosterCommand");Assert.IsNotNull(t,"Purchase must atomically charge coins and grant a three-use pack");var command=Activator.CreateInstance(t,index,generation);app.SendCommand((ICommand)command);return command;}
  static bool Accepted(object command)=>(bool)command.GetType().GetProperty("Accepted").GetValue(command);
  void Coins(int value){typeof(EconomyModel).GetProperty("Coins").SetValue(economy,value);}
  [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] public void ExactPriceGrantsThreeWithoutUsingOrChangingProgress(int index){int generation=model.Generation;Assert.IsTrue(Accepted(Buy(index,generation)));Assert.AreEqual(0,economy.Coins);Assert.AreEqual(3,model.BoosterCount(index));Assert.AreEqual(0,app.GetUtility<EconomyRepository>().Load().coins);for(int i=0;i<4;i++)if(i!=index)Assert.AreEqual(0,model.BoosterCount(i));Assert.AreEqual(5,model.Slots.Count);Assert.IsFalse(model.SpeedBoost);Assert.IsFalse(model.FreeSelection);Assert.AreEqual(1,model.Remaining);Assert.AreEqual(generation,model.Generation);Assert.IsTrue(model.Paused);}
  [Test] public void DuplicatePackCannotChargeEvenWithEnoughCoins(){Coins(5000);Assert.IsTrue(Accepted(Buy(1,model.Generation)));Assert.IsFalse(Accepted(Buy(1,model.Generation)));Assert.AreEqual(3400,economy.Coins);Assert.AreEqual(3,model.BoosterCount(1));}
  [Test] public void LowBalanceAndStaleRequestsChangeNothing(){Coins(1599);Assert.IsFalse(Accepted(Buy(0,model.Generation)));Assert.AreEqual(1599,economy.Coins);Coins(5000);Assert.IsFalse(Accepted(Buy(0,model.Generation-1)));Assert.IsFalse(Accepted(Buy(-1,model.Generation)));Assert.IsFalse(Accepted(Buy(4,model.Generation)));Assert.AreEqual(5000,economy.Coins);Assert.AreEqual(0,model.BoosterCount(0));}
  [Test] public void UnpausedOrHomeRequestsAreRejected(){app.SendCommand(new PauseHarvestCommand(false));Assert.IsFalse(Accepted(Buy(2,model.Generation)));app.SendCommand(new InitializeMenuNavigationCommand(true,2));Assert.IsFalse(Accepted(Buy(2,model.Generation)));Assert.AreEqual(1600,economy.Coins);Assert.AreEqual(0,model.BoosterCount(2));}
 }
}
