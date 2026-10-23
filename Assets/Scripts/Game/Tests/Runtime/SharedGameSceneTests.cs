using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class SharedGameSceneTests : EconomyTestFixture {
  MenuNavigationModel previous;
  [SetUp] public void IsolateNavigation(){var app=BeeHarvestApp.Interface;previous=app.GetModel<MenuNavigationModel>();app.RegisterModel(new MenuNavigationModel());}
  [TearDown] public void RestoreNavigation(){BeeHarvestApp.Interface.RegisterModel(previous);}
  static LevelOneController Level=>Object.FindObjectOfType<LevelOneController>();
  static IEnumerator Load(int n){Assert.IsTrue(Application.CanStreamedLevelBeLoaded("Game"));BeeHarvestApp.Interface.SendCommand(new InitializeMenuNavigationCommand(false,n));SceneManager.LoadScene("Game");yield return null;yield return null;Level.enabled=false;}
  [UnityTest] public IEnumerator AllLevelsUseTheActualSameSceneAndKeepTheirOwnPresentation(){
   GameObject prior=null;
   for(int n=1;n<=LevelOneAssets.LastLevel;n++){
    yield return Load(n);Assert.AreEqual("Game",SceneManager.GetActiveScene().name);Assert.AreEqual(n,Level.LevelNumber);if(n>1)Assert.IsTrue(prior==null,"Old model must be destroyed on same-name scene reload");var model=BeeHarvestApp.Interface.GetModel<HarvestModel>();var data=new LevelOneAssets().Level(n);Assert.AreEqual(data.cubes.Length,model.Cubes.Count);Assert.AreEqual(3,model.ColumnCount);Assert.AreEqual("Level "+n,Level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);
    prior=GameObject.Find(LevelOneAssets.ModelKey(n)+"(Clone)");Assert.IsNotNull(prior);var rs=prior.GetComponentsInChildren<Renderer>();Assert.AreEqual(data.cubes.Length,rs.Length);bool depth=n<=6||n>=96;bool cavity=n<=7||n>=96;bool edges=n<=6||n>=96;string shader=cavity?"BeeHarvest/VoxelCavity":edges?"BeeHarvest/VoxelEdges":"Universal Render Pipeline/Lit";Assert.IsTrue(rs.All(r=>r.sharedMaterial.shader.name==shader),"Level "+n);Assert.AreEqual(depth?LightShadows.Soft:LightShadows.None,GameObject.Find("KeyLight").GetComponent<Light>().shadows);Assert.Less(Vector4.Distance(depth?new Color(.48f,.52f,.63f):new Color(.65f,.7f,.85f),RenderSettings.ambientLight),.00001f);Assert.Less(Vector3.Distance(Vector3.one*(depth?.86f*1.04f:.86f)*LevelOneAssets.Presentation(n).modelScaleMultiplier,prior.transform.localScale),.00001f);
    if(edges){var expected=n<=6||(n>=96&&n<=114)?new Color(.2f,.2f,.2f,1):Color.black;Assert.Less(Vector4.Distance(expected,rs[0].sharedMaterial.GetColor("_EdgeColor")),.00001f);}
   }
  }
  [UnityTest] public IEnumerator HomeSelectionLoadsGameWithoutInspectorOverridingItsLevel(){
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;var app=BeeHarvestApp.Interface;app.SendCommand(new InitializeMenuNavigationCommand(true,35));yield return null;yield return null;var page=Object.FindObjectOfType<MainMenuPage>();page.StartGameButton.onClick.Invoke();float until=Time.realtimeSinceStartup+4;while(SceneManager.GetActiveScene().name!="Game"&&Time.realtimeSinceStartup<until)yield return null;yield return null;yield return null;Assert.AreEqual("Game",SceneManager.GetActiveScene().name);Assert.AreEqual(35,Level.LevelNumber);Assert.IsNotNull(GameObject.Find("Cat_Luci(Clone)"));
  }
  [UnityTest] public IEnumerator ColdGameUsesInspectorPreviewButInitializedNavigationWins(){
   Assert.IsTrue(Application.CanStreamedLevelBeLoaded("Game"));
   UnityEngine.Events.UnityAction<Scene,LoadSceneMode> configure=(scene,mode)=>{if(scene.name=="Game")Level.PreviewLevel=106;};
   SceneManager.sceneLoaded+=configure;
   try{
    SceneManager.LoadScene("Game");yield return null;yield return null;Level.enabled=false;Assert.AreEqual(106,Level.LevelNumber);Assert.AreEqual(870,BeeHarvestApp.Interface.GetModel<HarvestModel>().Cubes.Count);
    var app=BeeHarvestApp.Interface;app.SendCommand(new InitializeMenuNavigationCommand(false,6));SceneManager.LoadScene("Game");yield return null;yield return null;Level.enabled=false;Assert.AreEqual(6,Level.LevelNumber);
   }finally{SceneManager.sceneLoaded-=configure;}
  }
  [UnityTest] public IEnumerator VictoryReloadsTheSameGameSceneForTheNextLevelAndFinalHomeRestartsOne(){
   foreach(int n in new[]{1,6,111,113,114,115,118,119}){
    yield return Load(n);var app=BeeHarvestApp.Interface;int old=Level.GetInstanceID();app.SendCommand(new DetachBeeFlightPresentationCommand(Level.GetComponent<BeeFlightView>().GetInstanceID()));app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(app.GetModel<HarvestModel>().Won);yield return null;var popup=Object.FindObjectOfType<VictoryPopupController>();float until=Time.realtimeSinceStartup+4;while(!popup.ContinueButton.interactable&&Time.realtimeSinceStartup<until)yield return null;Assert.IsTrue(popup.ContinueButton.interactable);popup.ContinueButton.onClick.Invoke();popup.ContinueButton.onClick.Invoke();
    if(n==LevelOneAssets.LastLevel){yield return PopupMotionTestWait.Close();yield return null;Assert.IsTrue(app.GetModel<MenuNavigationModel>().IsHome);var page=Object.FindObjectOfType<MainMenuPage>();Assert.AreEqual("Level 1",page.StartGameButton.GetComponentInChildren<TMP_Text>(true).text);page.StartGameButton.onClick.Invoke();}
    int next=n==LevelOneAssets.LastLevel?1:n+1;until=Time.realtimeSinceStartup+5;while((Level==null||Level.GetInstanceID()==old||Level.LevelNumber!=next)&&Time.realtimeSinceStartup<until)yield return null;yield return null;Assert.AreEqual("Game",SceneManager.GetActiveScene().name);Assert.IsNotNull(Level);Assert.AreNotEqual(old,Level.GetInstanceID());Assert.AreEqual(next,Level.LevelNumber);Assert.AreEqual(new LevelOneAssets().Level(next).cubes.Length,app.GetModel<HarvestModel>().Cubes.Count);
   }
  }
 }
}
