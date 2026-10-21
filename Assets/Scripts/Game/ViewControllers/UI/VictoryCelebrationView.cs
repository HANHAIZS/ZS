using UnityEngine;
namespace BeeHarvest {
 /// <summary>Owns the lifetime of the imported, UI-adapted celebration without changing gameplay.</summary>
 public sealed class VictoryCelebrationView : BeeHarvestController {
  ParticleSystem[] particles;
  bool playing,paused;
  void Awake(){particles=GetComponentsInChildren<ParticleSystem>(true);}
  public void Play(){gameObject.SetActive(true);playing=true;paused=false;foreach(var p in particles){p.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);p.Play(false);}}
  public void SetPaused(bool value){if(!playing||paused==value)return;paused=value;foreach(var p in particles){if(value)p.Pause(false);else if(p.isPaused)p.Play(false);}}
  public void Stop(){playing=false;paused=false;if(particles!=null)foreach(var p in particles)p.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);gameObject.SetActive(false);}
  void OnDisable(){playing=false;paused=false;if(particles!=null)foreach(var p in particles)p.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);}
 }
}
