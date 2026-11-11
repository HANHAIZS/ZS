using QFramework;
namespace BeeHarvest {
 public sealed class EconomyModel : AbstractModel {
  public int Coins {get;internal set;}=1600;
  public int Stamina {get;internal set;}=5;
  public long NextRecoveryAt {get;internal set;}
  public int Revision {get;internal set;}
  public bool IsPlaytest {get;private set;}
  int normalCoins,normalStamina;
  long normalRecovery;
  internal void BeginPlaytest(){
   if(IsPlaytest)return;
   normalCoins=Coins;normalStamina=Stamina;normalRecovery=NextRecoveryAt;
   IsPlaytest=true;Coins=1600;Stamina=EconomySystem.MaximumStamina;NextRecoveryAt=0;Revision++;
  }
  internal void EndPlaytest(){
   if(!IsPlaytest)return;
   Coins=normalCoins;Stamina=normalStamina;NextRecoveryAt=normalRecovery;IsPlaytest=false;Revision++;
  }
  protected override void OnInit(){
   var saved=this.GetUtility<EconomyRepository>().Load();
   Coins=System.Math.Max(0,saved.coins);Stamina=System.Math.Max(0,System.Math.Min(5,saved.stamina));
   NextRecoveryAt=Stamina==5?0:System.Math.Max(0,saved.nextRecoveryAt);
  }
 }
}
