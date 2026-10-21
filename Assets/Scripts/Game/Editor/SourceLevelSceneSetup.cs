using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace BeeHarvest.Editor {
 public static class SourceLevelSceneSetup {
  /// <summary>Builds one selected source model and scene while preserving existing bindings.</summary>
  public static string Apply(int number){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before building a source scene");
   string name=LevelOneAssets.ModelKey(number),scenePath="Assets/Scenes/"+LevelOneAssets.SceneName(number)+".unity";
   var previous=EditorSceneManager.GetSceneManagerSetup();
   try{
    CubeLandLevelSetup.ApplyLevel(number);
    if(!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)&&!AssetDatabase.CopyAsset("Assets/Scenes/Level02.unity",scenePath))throw new InvalidOperationException("Could not create source scene: "+scenePath);
    var scene=EditorSceneManager.OpenScene(scenePath);var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();level.LevelNumber=number;
    var title=level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>();var font=title.font;string caption="Level "+number;
    if(!font.HasCharacters(caption)){
     var mode=font.atlasPopulationMode;
     try{font.atlasPopulationMode=AtlasPopulationMode.Dynamic;string missing;if(!font.TryAddCharacters(caption,out missing)&&missing.Length>0)throw new InvalidOperationException("Missing title glyphs: "+missing);}
     finally{font.atlasPopulationMode=mode;}
     EditorUtility.SetDirty(font);
    }
    title.text=caption;OkaUiRefreshSetup.StyleBoosterBar(level.Hud.Boosters);
    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    var entries=EditorBuildSettings.scenes.ToList();var entry=entries.FirstOrDefault(s=>s.path==scenePath);
    if(entry==null)entries.Add(new EditorBuildSettingsScene(scenePath,true));else entry.enabled=true;
    EditorBuildSettings.scenes=entries.ToArray();AssetDatabase.SaveAssets();return name+" "+LevelOneAssets.SceneName(number)+" ready";
   }finally{EditorSceneManager.RestoreSceneManagerSetup(previous);}
  }
  [MenuItem("BeeHarvest/Levels/Build Lightweight Levels 06-15")]
  public static void ApplyAdditional(){for(int number=6;number<=15;number++)Apply(number);}
  [MenuItem("BeeHarvest/Levels/Build Bee And Watermelon")]
  public static void ApplyFourthAndFifth(){Apply(4);Apply(5);}
 }
}
