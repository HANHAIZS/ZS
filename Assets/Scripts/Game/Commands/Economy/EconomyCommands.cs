using QFramework;
namespace BeeHarvest {
 internal static class EconomyMutation {
  internal static void Save(EconomyModel m,EconomyRepository repository){m.Revision++;repository.Save(m);}
 }
 public sealed class RefreshEconomyCommand : AbstractCommand {
  protected override void OnExecute(){
   var m=this.GetModel<EconomyModel>();var rules=this.GetSystem<EconomySystem>();var repository=this.GetUtility<EconomyRepository>();
   int old=m.Stamina;long deadline=m.NextRecoveryAt,now=rules.Now;
   if(m.Stamina>=EconomySystem.MaximumStamina)m.NextRecoveryAt=0;
   else if(m.NextRecoveryAt<=0||m.NextRecoveryAt>now+EconomySystem.RecoverySeconds)m.NextRecoveryAt=now+EconomySystem.RecoverySeconds;
   else if(now>=m.NextRecoveryAt){
    long recovered=1+(now-m.NextRecoveryAt)/EconomySystem.RecoverySeconds;
    m.Stamina+=(int)System.Math.Min(EconomySystem.MaximumStamina-m.Stamina,recovered);
    m.NextRecoveryAt=m.Stamina==EconomySystem.MaximumStamina?0:m.NextRecoveryAt+recovered*EconomySystem.RecoverySeconds;
   }
   if(old!=m.Stamina||deadline!=m.NextRecoveryAt)EconomyMutation.Save(m,repository);
  }
 }
 public sealed class AddCoinsCommand : AbstractCommand {
  protected override void OnExecute(){var m=this.GetModel<EconomyModel>();var repository=this.GetUtility<EconomyRepository>();if(m.Coins==int.MaxValue)return;m.Coins=(int)System.Math.Min(int.MaxValue,(long)m.Coins+500);EconomyMutation.Save(m,repository);}
 }
 public sealed class AddStaminaCommand : AbstractCommand {
  protected override void OnExecute(){this.SendCommand(new RefreshEconomyCommand());var m=this.GetModel<EconomyModel>();var repository=this.GetUtility<EconomyRepository>();if(m.Stamina>=EconomySystem.MaximumStamina)return;m.Stamina++;if(m.Stamina==EconomySystem.MaximumStamina)m.NextRecoveryAt=0;EconomyMutation.Save(m,repository);}
 }
 public sealed class SpendStaminaCommand : AbstractCommand {
  protected override void OnExecute(){this.SendCommand(new RefreshEconomyCommand());var m=this.GetModel<EconomyModel>();var rules=this.GetSystem<EconomySystem>();var repository=this.GetUtility<EconomyRepository>();if(m.Stamina<=0)return;m.Stamina--;if(m.NextRecoveryAt<=0)m.NextRecoveryAt=rules.Now+EconomySystem.RecoverySeconds;EconomyMutation.Save(m,repository);}
 }
 public sealed class RetryLevelCommand : AbstractCommand {
  readonly LevelData data;
  public bool Accepted {get;private set;}
  public RetryLevelCommand(LevelData value){data=value;}
  protected override void OnExecute(){
   var nav=this.GetModel<MenuNavigationModel>();var harvest=this.GetModel<HarvestModel>();
   if(nav.IsHome||harvest.Cubes.Count==0)return;
   this.SendCommand(new RefreshEconomyCommand());var economy=this.GetModel<EconomyModel>();
   if(economy.Stamina<=0)return;
   // Validate/reset first. Invalid level input must not charge the player.
   bool charge=!harvest.Won;this.SendCommand(new StartLevelCommand(data));
   if(charge)this.SendCommand(new SpendStaminaCommand());Accepted=true;
  }
 }
}
