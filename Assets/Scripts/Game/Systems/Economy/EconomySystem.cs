using QFramework;
using GameFoundation.Core;
namespace BeeHarvest {
 public sealed class EconomySystem : AbstractSystem {
  public const int MaximumStamina=5,RecoverySeconds=600;
  IFoundationClock clock;
  protected override void OnInit(){clock=this.GetUtility<IFoundationClock>();}
  public long Now=>clock.UtcNow.ToUnixTimeSeconds();
  public int SecondsUntilRecovery(EconomyModel model)=>model.Stamina>=MaximumStamina?0:(int)System.Math.Max(0,System.Math.Min(RecoverySeconds,model.NextRecoveryAt-Now));
 }
}
