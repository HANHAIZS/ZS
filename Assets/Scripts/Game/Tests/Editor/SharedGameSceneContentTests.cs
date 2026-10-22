using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class SharedGameSceneContentTests {
  [Test] public void BuildContainsOnlyMenuAndSharedGame(){CollectionAssert.AreEqual(new[]{"Assets/Scenes/MainMenu.unity","Assets/Scenes/Game.unity"},EditorBuildSettings.scenes.Select(s=>s.path).ToArray());Assert.IsTrue(EditorBuildSettings.scenes.All(s=>s.enabled));Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Game.unity"));}
  [Test] public void SceneFolderContainsOnlyMenuAndSharedGame(){CollectionAssert.AreEquivalent(new[]{"Assets/Scenes/MainMenu.unity","Assets/Scenes/Game.unity"},AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).ToArray());}
  [Test] public void PresentationCatalogExistsSeparatelyFromUnchangedLevelData(){var text=Resources.Load<TextAsset>("Levels/PresentationCatalog");Assert.IsNotNull(text,"All level-specific display choices must move out of duplicated scenes");var catalog=JsonUtility.FromJson<Snapshot>(text.text);Assert.AreEqual(114,catalog.entries.Length);CollectionAssert.AreEqual(Enumerable.Range(1,114).ToArray(),catalog.entries.Select(e=>e.number).ToArray());Assert.AreEqual("Chick",catalog.entries[5].modelKey);Assert.IsTrue(catalog.entries[5].sourceHandoff);Assert.IsFalse(catalog.entries[105].sourceHandoff);Assert.IsTrue(catalog.entries.Skip(111).All(e=>e.sourceHandoff));}
  [System.Serializable] sealed class Snapshot {public Entry[] entries;}
  [System.Serializable] sealed class Entry {public int number;public string modelKey;public bool sourceHandoff;}
 }
}
