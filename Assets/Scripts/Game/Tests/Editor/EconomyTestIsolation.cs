using NUnit.Framework;
using QFramework;
using GameFoundation.Core;
using GameFoundation.Save;
namespace BeeHarvest.Tests {
 // Unity's runner does not consistently execute assembly-level TestAction callbacks.
 // Inherited SetUp/TearDown explicitly isolates each fixture and keeps real saves untouched.
 public abstract class EconomyTestFixture {
  EconomyRepository previousRepository;EconomyModel previousModel;
  [SetUp] public void BeginEconomyIsolation(){var app=BeeHarvestApp.Interface;previousRepository=app.GetUtility<EconomyRepository>();previousModel=app.GetModel<EconomyModel>();app.RegisterUtility(new EconomyRepository(new MemorySaveStore(),new JsonUtilitySaveSerializer(),app.GetUtility<IFoundationClock>()));app.RegisterModel(new EconomyModel());}
  [TearDown] public void EndEconomyIsolation(){var app=BeeHarvestApp.Interface;app.RegisterUtility(previousRepository);app.RegisterModel(previousModel);}
 }
}
