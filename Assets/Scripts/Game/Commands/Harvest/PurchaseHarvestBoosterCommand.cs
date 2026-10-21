using QFramework;
namespace BeeHarvest {
 /// <summary>Commits one zero-stock pack; all validation precedes the coin/inventory mutation.</summary>
 public sealed class PurchaseHarvestBoosterCommand : AbstractCommand {
  public const int Price=1600,Quantity=3;
  readonly int index,generation;
  public bool Accepted {get;private set;}
  public PurchaseHarvestBoosterCommand(int booster,int expectedGeneration){index=booster;generation=expectedGeneration;}
  protected override void OnExecute(){
   var model=this.GetModel<HarvestModel>();var nav=this.GetModel<MenuNavigationModel>();var economy=this.GetModel<EconomyModel>();var repository=this.GetUtility<EconomyRepository>();
   if(Accepted||index<0||index>=4||nav.IsHome||model.Generation!=generation||!model.Paused||model.Won||model.Deadlocked||model.Cubes.Count==0||model.BoosterCount(index)!=0||economy.Coins<Price)return;
   economy.Coins-=Price;model.boosterCounts[index]=Quantity;model.Revision++;Accepted=true;
   EconomyMutation.Save(economy,repository);
  }
 }
}
