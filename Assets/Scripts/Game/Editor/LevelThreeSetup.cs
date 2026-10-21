using UnityEditor;
namespace BeeHarvest.Editor {
 public static class LevelThreeSetup {
  [MenuItem("BeeHarvest/Levels/Build Banana Level Three")]
  public static string Apply(){return SourceLevelSceneSetup.Apply(3);}
 }
}
