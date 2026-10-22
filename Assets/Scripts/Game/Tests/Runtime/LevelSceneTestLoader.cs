using System;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace BeeHarvest.Tests {
 /// <summary>Loads real shared scenes while keeping existing tests addressed by logical level identity.</summary>
 static class LevelSceneTestLoader {
  public static void Load(string key){
   int number;if(TryNumber(key,out number)){BeeHarvestApp.Interface.SendCommand(new InitializeMenuNavigationCommand(false,number));SceneManager.LoadScene(LevelOneAssets.GameSceneName);}
   else{if(key=="MainMenu")BeeHarvestApp.Interface.SendCommand(new InitializeMenuNavigationCommand(true));SceneManager.LoadScene(key);}
  }
  public static string CurrentKey {get{var scene=SceneManager.GetActiveScene();if(scene.name!=LevelOneAssets.GameSceneName)return scene.name;var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();return level==null||level.LevelNumber<1?scene.name:LevelOneAssets.LevelKey(level.LevelNumber);}}
  public static bool CanLoad(string key){int number;if(!TryNumber(key,out number))return Application.CanStreamedLevelBeLoaded(key);return Application.CanStreamedLevelBeLoaded(LevelOneAssets.GameSceneName)&&Resources.Load<TextAsset>("Levels/"+LevelOneAssets.LevelKey(number))!=null;}
  static bool TryNumber(string key,out int number){number=0;return key!=null&&key.StartsWith("Level",StringComparison.Ordinal)&&int.TryParse(key.Substring(5),out number)&&number>=1&&number<=LevelOneAssets.LastLevel;}
 }
}
