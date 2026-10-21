using System;
using QFramework;
using UnityEngine;
namespace BeeHarvest {
 /// <summary>Scene-owned audio presentation; Commands and Models retain gameplay and preference ownership.</summary>
 [DisallowMultipleComponent]
 public sealed class GameAudioController : BeeHarvestController {
  const int EffectVoices=6;
  AudioSource music;
  AudioSource[] effects;
  AudioClip homeMusic,levelMusic,hiveSlot,beeCarry,victory;
  SettingsModel settings;
  MenuNavigationModel navigation;
  HarvestModel harvest;
  LevelOneController level;
  IUnRegister navigationSubscription,slotSubscription,pickupSubscription,victorySubscription;
  bool initialized,applicationPaused,musicStarted,musicPaused,suppressed;
  int settingsRevision,generation,victoryGeneration=-1;

  public void Initialize(LevelOneController owner){
   if(initialized)return;
   level=owner;settings=this.GetModel<SettingsModel>();navigation=this.GetModel<MenuNavigationModel>();harvest=this.GetModel<HarvestModel>();
   homeMusic=Clip("Audio/Music/HomeMusic");levelMusic=Clip("Audio/Music/LevelMusic");hiveSlot=Clip("Audio/Sfx/HiveSlot");beeCarry=Clip("Audio/Sfx/BeeCarry");victory=Clip("Audio/Sfx/Victory");
   music=Source(true);music.volume=.4f;effects=new AudioSource[EffectVoices];for(int i=0;i<effects.Length;i++)effects[i]=Source(false);
   settingsRevision=settings.Revision;generation=harvest.Generation;suppressed=!CanPlayEffects;
   initialized=true;if(isActiveAndEnabled)Subscribe();SyncMusic();
  }
  static AudioClip Clip(string key){var clip=Resources.Load<AudioClip>(key);if(clip==null)throw new InvalidOperationException("Missing game audio: "+key);return clip;}
  AudioSource Source(bool looping){var source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=looping;source.spatialBlend=0;source.dopplerLevel=0;source.priority=looping?64:128;return source;}
  void Subscribe(){
   if(navigationSubscription!=null)return;
   navigationSubscription=this.RegisterEvent<MenuNavigationChangedEvent>(e=>{StopEffects();SyncMusic();});
   slotSubscription=this.RegisterEvent<HiveEnteredSlotEvent>(OnHiveEnteredSlot);
   pickupSubscription=this.RegisterEvent<HarvestChangedEvent>(OnHarvestChanged);
   victorySubscription=this.RegisterEvent<VictoryPresentationEvent>(OnVictoryPresentation);
  }
  void Unsubscribe(){navigationSubscription?.UnRegister();slotSubscription?.UnRegister();pickupSubscription?.UnRegister();victorySubscription?.UnRegister();navigationSubscription=slotSubscription=pickupSubscription=victorySubscription=null;}
  bool CanPlayEffects=>isActiveAndEnabled&&!applicationPaused&&level!=null&&!level.IsApplicationPaused&&!navigation.IsHome&&!harvest.Paused&&!harvest.Won&&settings.IsEnabled(GamePreference.Sound);
  void SyncMusic(){
   if(!initialized)return;
   var desired=navigation.IsHome?homeMusic:levelMusic;
   if(music.clip!=desired){music.Stop();music.clip=desired;musicStarted=musicPaused=false;}
   bool audible=isActiveAndEnabled&&!applicationPaused&&settings.IsEnabled(GamePreference.Music);
   if(audible){if(musicPaused){music.UnPause();musicPaused=false;}else if(!musicStarted){music.Play();musicStarted=true;}}
   else if(musicStarted&&!musicPaused){music.Pause();musicPaused=true;}
  }
  void OnHiveEnteredSlot(HiveEnteredSlotEvent e){if(e.Generation==harvest.Generation)PlayEffect(hiveSlot,.8f);}
  void OnHarvestChanged(HarvestChangedEvent e){if(e.Picked&&e.Task>=0&&e.Task<harvest.Flights.Count&&harvest.Flights[e.Task].Stage==1)PlayEffect(beeCarry,.55f);}
  void OnVictoryPresentation(VictoryPresentationEvent e){
   if(!e.Visible){if(e.Generation!=victoryGeneration)return;foreach(var source in effects)if(source.clip==victory)source.Stop();return;}
   if(e.Generation!=harvest.Generation||!harvest.Won||navigation.IsHome||victoryGeneration==e.Generation)return;
   victoryGeneration=e.Generation;StopEffects();generation=harvest.Generation;suppressed=!CanPlayEffects;
   if(!isActiveAndEnabled||applicationPaused||level==null||level.IsApplicationPaused||!settings.IsEnabled(GamePreference.Sound))return;
   effects[0].clip=victory;effects[0].volume=.9f;effects[0].Play();
  }
  void PlayEffect(AudioClip clip,float volume){
   if(generation!=harvest.Generation){StopEffects();generation=harvest.Generation;}
   if(!CanPlayEffects)return;
   for(int i=0;i<effects.Length;i++)if(!effects[i].isPlaying){effects[i].clip=clip;effects[i].volume=volume;effects[i].Play();return;}
   // A saturated burst is dropped rather than queued into a later scene or growing the voice count.
  }
  void StopEffects(){if(effects==null)return;foreach(var source in effects)if(source!=null)source.Stop();}
  void Update(){
   if(!initialized)return;
   if(settingsRevision!=settings.Revision){settingsRevision=settings.Revision;SyncMusic();if(!settings.IsEnabled(GamePreference.Sound))StopEffects();}
   bool blocked=!CanPlayEffects;
   if(generation!=harvest.Generation||(!suppressed&&blocked))StopEffects();
   generation=harvest.Generation;suppressed=blocked;
  }
  void OnApplicationPause(bool paused){applicationPaused=paused;if(!initialized)return;if(paused)StopEffects();SyncMusic();}
  void OnDisable(){if(!initialized)return;Unsubscribe();music.Stop();musicStarted=musicPaused=false;StopEffects();}
  void OnEnable(){if(initialized){Subscribe();SyncMusic();}}
  void OnDestroy(){Unsubscribe();}
 }
}
