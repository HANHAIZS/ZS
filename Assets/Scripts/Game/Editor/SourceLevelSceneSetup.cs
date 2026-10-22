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
    bool useSixTemplate=number>=112&&number<=114;
    bool useSoftDepthTemplate=number>=106&&number<=111||useSixTemplate;
    string template=number==106||useSixTemplate?"Assets/Scenes/Level06.unity":useSoftDepthTemplate?"Assets/Scenes/Level106.unity":"Assets/Scenes/Level02.unity";
    if(!AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath)&&!AssetDatabase.CopyAsset(template,scenePath))throw new InvalidOperationException("Could not create source scene: "+scenePath);
    var scene=EditorSceneManager.OpenScene(scenePath);var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();level.LevelNumber=number;
    var title=level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>();var font=title.font;string caption="Level "+number;
    if(!font.HasCharacters(caption)){
     var mode=font.atlasPopulationMode;
     try{font.atlasPopulationMode=AtlasPopulationMode.Dynamic;string missing;if(!font.TryAddCharacters(caption,out missing)&&missing.Length>0)throw new InvalidOperationException("Missing title glyphs: "+missing);}
     finally{font.atlasPopulationMode=mode;}
     EditorUtility.SetDirty(font);
    }
    title.text=caption;if(!useSoftDepthTemplate)OkaUiRefreshSetup.StyleBoosterBar(level.Hud.Boosters);
    ApplyVoxelLighting(number);
    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    var entries=EditorBuildSettings.scenes.ToList();var entry=entries.FirstOrDefault(s=>s.path==scenePath);
    if(entry==null)entries.Add(new EditorBuildSettingsScene(scenePath,true));else entry.enabled=true;
    EditorBuildSettings.scenes=entries.ToArray();AssetDatabase.SaveAssets();return name+" "+LevelOneAssets.SceneName(number)+" ready";
   }finally{EditorSceneManager.RestoreSceneManagerSetup(previous);}
  }
  /// <summary>Sets per-scene presentation lighting; ordinary cloned levels retain the original lighting.</summary>
  public static void ApplyVoxelLighting(int number){
   bool depth=LevelOneAssets.HasVoxelSeparation(number);
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
   RenderSettings.ambientLight=depth?new Color(.48f,.52f,.63f):new Color(.65f,.7f,.85f);
   var light=GameObject.Find("KeyLight").GetComponent<Light>();light.shadows=depth?LightShadows.Soft:LightShadows.None;
   light.shadowStrength=depth?.3f:1;light.shadowBias=depth?.04f:.05f;light.shadowNormalBias=depth?.2f:.4f;
   if(depth){
    var pipeline=AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>("Assets/Settings/GameFoundation/LevelOnePipeline.asset");
    var serialized=new SerializedObject(pipeline);serialized.FindProperty("m_SoftShadowsSupported").boolValue=true;serialized.ApplyModifiedPropertiesWithoutUndo();
   }
   EditorUtility.SetDirty(light);
  }
  [MenuItem("BeeHarvest/Levels/Build Lightweight Levels 06-15")]
  public static void ApplyAdditional(){for(int number=6;number<=15;number++)Apply(number);}
  [MenuItem("BeeHarvest/Levels/Build Bee And Watermelon")]
  public static void ApplyFourthAndFifth(){Apply(4);Apply(5);}
 }
}
