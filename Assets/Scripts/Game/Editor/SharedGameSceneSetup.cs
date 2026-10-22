using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace BeeHarvest.Editor {
 /// <summary>Creates the one gameplay scene and previews data-selected levels without duplicating scenes.</summary>
 public static class SharedGameSceneSetup {
  [MenuItem("BeeHarvest/Levels/Migrate To Shared Game Scene")]
  public static void Migrate(){
   RequireStopped();
   if(!AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelOneAssets.GameScenePath))throw new InvalidOperationException("Restore the authored Game scene from version control before configuring it");
   LevelOneAssets.ReloadPresentationCatalog();
   var scene=EditorSceneManager.OpenScene(LevelOneAssets.GameScenePath);var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();
   level.gameObject.name="LevelOne";level.PreviewLevel=1;
   var view=level.GetComponent<LevelPresentationView>();if(view==null)view=level.gameObject.AddComponent<LevelPresentationView>();
   view.KeyLight=GameObject.Find("KeyLight").GetComponent<Light>();view.Title=level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>();level.Presentation=view;
   view.Apply(LevelOneAssets.Presentation(1),1);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);ConfigureBuild();AssetDatabase.SaveAssets();
  }
  public static void ConfigureBuild(){EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",true),new EditorBuildSettingsScene(LevelOneAssets.GameScenePath,true)};}
  public static void Preview(int number){
   RequireStopped();LevelOneAssets.ReloadPresentationCatalog();var data=LevelOneAssets.Presentation(number);
   var scene=EditorSceneManager.OpenScene(LevelOneAssets.GameScenePath);var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();level.PreviewLevel=number;level.Presentation.Apply(data,number);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);ConfigureBuild();AssetDatabase.SaveAssets();
  }
  [MenuItem("BeeHarvest/Levels/Preview Game Inspector Level")]
  public static void PreviewSelected(){RequireStopped();var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();if(level==null)throw new InvalidOperationException("Open Game first");Preview(level.PreviewLevel);}
  public static int RemoveLegacyScenes(){
   RequireStopped();if(!AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelOneAssets.GameScenePath))throw new InvalidOperationException("Create and verify Game first");
   EditorSceneManager.OpenScene(LevelOneAssets.GameScenePath);ConfigureBuild();int removed=0;
   for(int n=1;n<=LevelOneAssets.LastLevel;n++){string path="Assets/Scenes/"+LevelOneAssets.LevelKey(n)+".unity";if(AssetDatabase.LoadAssetAtPath<SceneAsset>(path)){if(!AssetDatabase.DeleteAsset(path))throw new InvalidOperationException("Could not remove legacy scene "+path);removed++;}}
   AssetDatabase.SaveAssets();return removed;
  }
  static void RequireStopped(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before editing Game");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save the current scene before editing Game");}
 }
}
