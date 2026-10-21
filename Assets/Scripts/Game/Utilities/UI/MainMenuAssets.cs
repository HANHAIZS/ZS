using QFramework;
using UnityEngine;
namespace BeeHarvest {
 public sealed class MainMenuAssets : IUtility {
  GameObject page;
  public GameObject Page(){
   if(page==null)page=Resources.Load<GameObject>("Prefabs/UI/MainMenuPage");
   if(page==null)throw new System.InvalidOperationException("MainMenuPage Resources prefab is missing.");
   return page;
  }
 }
}
