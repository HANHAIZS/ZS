using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class MainMenuSetupTests : EconomyTestFixture {
  [Test] public void RegeneratingMenuReusesBakedFontAndKeepsOneLaunchScene(){
   string scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
   try{
    var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("BeeHarvest.Editor.MainMenuSetup")).First(t=>t!=null);
    Assert.DoesNotThrow(()=>type.GetMethod("Apply",BindingFlags.Static|BindingFlags.Public).Invoke(null,null),"Rebuilding menu must reuse already baked glyphs");
    Assert.AreEqual("Assets/Scenes/MainMenu.unity",EditorBuildSettings.scenes[0].path);
    Assert.AreEqual(1,EditorBuildSettings.scenes.Count(s=>s.path=="Assets/Scenes/MainMenu.unity"));
    Assert.AreEqual(1,EditorBuildSettings.scenes.Count(s=>s.path=="Assets/Scenes/Level01.unity"));
    Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/UI/MainMenuPage.prefab"));
    var page=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/UI/MainMenuPage.prefab").GetComponent<MainMenuPage>();
    Assert.IsNotNull(page.Economy,"Regenerating menu must retain the economy header");
    var header=new SerializedObject(page.Economy);
    foreach(string reference in new[]{"CoinsLabel","StaminaLabel","RecoveryLabel","CoinsPlus","StaminaPlus"})Assert.IsNotNull(header.FindProperty(reference).objectReferenceValue,reference);
   }finally{if(!string.IsNullOrEmpty(scene))EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);}
  }
 }
}
