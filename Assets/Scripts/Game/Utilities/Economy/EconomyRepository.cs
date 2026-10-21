using System;
using QFramework;
using GameFoundation.Core;
using GameFoundation.Save;
namespace BeeHarvest {
 [Serializable] public sealed class EconomySaveData {
  public int coins=1600,stamina=5;
  public long nextRecoveryAt;
 }
 public sealed class EconomyRepository : IUtility {
  public const string Slot="BeeHarvest.economy";
  readonly VersionedSaveService<EconomySaveData> saves;
  readonly ISaveStore store;
  readonly ISaveSerializer serializer;
  bool canWrite=true;
  public EconomyRepository(ISaveStore store,ISaveSerializer serializer,IFoundationClock clock){this.store=store;this.serializer=serializer;saves=new VersionedSaveService<EconomySaveData>(Slot,1,store,serializer,clock,null);}
  public EconomySaveData Load(){
   try{
    // Check before Foundation Load: its backup recovery may replace a failed primary.
    string raw;
    if(store.TryRead(Slot,out raw)){
     SaveEnvelope envelope=null;
     try{envelope=serializer.Deserialize<SaveEnvelope>(raw);}catch(ArgumentException){}catch(FormatException){}
     if(envelope!=null&&envelope.Version>1){canWrite=false;return new EconomySaveData();}
    }
    var result=saves.Load();canWrite=result.Status!=SaveLoadStatus.MigrationMissing;
    return result.IsSuccess?result.Value:new EconomySaveData();
   }catch(Exception e){UnityEngine.Debug.LogWarning("Economy save read failed: "+e.Message);return new EconomySaveData();}
  }
  public void Save(EconomyModel model){
   if(!canWrite)return;
   try{saves.Save(new EconomySaveData{coins=model.Coins,stamina=model.Stamina,nextRecoveryAt=model.NextRecoveryAt});}
   catch(Exception e){UnityEngine.Debug.LogWarning("Economy save write failed: "+e.Message);}
  }
 }
}
