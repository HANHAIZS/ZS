using System;
using TMPro;
using UnityEngine;
namespace BeeHarvest {
 /// <summary>Applies a level's presentation before its model is instantiated in the shared scene.</summary>
 public sealed class LevelPresentationView : BeeHarvestController {
  public Light KeyLight;
  public TMP_Text Title;
  public void Apply(LevelPresentationData data,int number){
   if(Title==null)throw new InvalidOperationException("Shared Game requires its level title binding");
   ApplyLighting(data,KeyLight);Title.text="Level "+number;
  }
  public static void ApplyLighting(LevelPresentationData data,Light light){
   if(light==null)throw new InvalidOperationException("Shared Game requires its key light binding");
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=data.ambientLight;
   light.shadows=data.softShadows?LightShadows.Soft:LightShadows.None;light.shadowStrength=data.shadowStrength;light.shadowBias=data.shadowBias;light.shadowNormalBias=data.shadowNormalBias;
  }
 }
}
