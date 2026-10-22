using System;
using UnityEditor;
using UnityEngine;
namespace BeeHarvest.Editor {
 public static class SourceLevelSceneSetup {
  /// <summary>Builds only the chosen source model, then previews its data in the shared Game scene.</summary>
  public static string Apply(int number){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before building a source model");
   LevelOneAssets.ReloadPresentationCatalog();
   string name=LevelOneAssets.ModelKey(number);CubeLandLevelSetup.ApplyLevel(number);SharedGameSceneSetup.Preview(number);return name+" ready in Game ("+LevelOneAssets.LevelKey(number)+")";
  }
  public static void ApplyVoxelLighting(int number){
   LevelPresentationView.ApplyLighting(LevelOneAssets.Presentation(number),GameObject.Find("KeyLight").GetComponent<Light>());
  }
  [MenuItem("BeeHarvest/Levels/Build Lightweight Models 06-15")]
  public static void ApplyAdditional(){for(int n=6;n<=15;n++)Apply(n);}
  [MenuItem("BeeHarvest/Levels/Build Bee And Watermelon")]
  public static void ApplyFourthAndFifth(){Apply(4);Apply(5);}
 }
}
