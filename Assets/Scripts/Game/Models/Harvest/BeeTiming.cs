using System;
namespace BeeHarvest {
 [Serializable] public sealed class BeeTimingData {
  public float departureSeconds,settleSeconds,reachSeconds,liftSeconds,handoffSeconds;
 }
 /// <summary>Validated immutable session timings shared by simulation and presentation.</summary>
 public sealed class BeeTiming {
  public static readonly BeeTiming Default=new BeeTiming(.18f,.13f,.12f,.22f,.09f);
  public readonly float DepartureSeconds,SettleSeconds,ReachSeconds,LiftSeconds,HandoffSeconds;
  public float OutboundSeconds=>AdvanceHarvestCommand.ApproachSeconds+SettleSeconds+ReachSeconds;
  BeeTiming(float departure,float settle,float reach,float lift,float handoff){DepartureSeconds=departure;SettleSeconds=settle;ReachSeconds=reach;LiftSeconds=lift;HandoffSeconds=handoff;}
  public static BeeTiming From(BeeTimingData data){
   // JsonUtility materializes omitted nested records as all-zero instances.
   if(data==null||(data.departureSeconds==0&&data.settleSeconds==0&&data.reachSeconds==0&&data.liftSeconds==0&&data.handoffSeconds==0))return Default;
   foreach(float t in new[]{data.departureSeconds,data.settleSeconds,data.reachSeconds,data.liftSeconds,data.handoffSeconds})if(float.IsNaN(t)||float.IsInfinity(t)||t<=0||t>2)throw new ArgumentException("Invalid bee timing");
   if(data.handoffSeconds>data.liftSeconds)throw new ArgumentException("Handoff exceeds extraction time");
   return new BeeTiming(data.departureSeconds,data.settleSeconds,data.reachSeconds,data.liftSeconds,data.handoffSeconds);
  }
 }
}
