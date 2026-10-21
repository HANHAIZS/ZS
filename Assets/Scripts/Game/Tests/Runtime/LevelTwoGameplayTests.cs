using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests
{
 public sealed class LevelTwoGameplayTests : EconomyTestFixture
 {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static IEnumerator LoadDuck(){SceneManager.LoadScene("Level02");yield return null;yield return null;}
  [UnityTest] public IEnumerator DuckSceneMatchesVoxelsQuotasAndShowsThreePlanningRows(){
   yield return LoadDuck();var level=Object.FindObjectOfType<LevelOneController>();var hud=level.Hud;
   Assert.AreEqual(2,level.LevelNumber);Assert.AreEqual("Level 2",hud.transform.Find("LevelLabel").GetComponent<TMP_Text>().text);
   Assert.AreEqual(2190,Model.Cubes.Count);Assert.AreEqual(30,Model.Hives.Count);
   var duck=level.GetComponent<HarvestBoardView>().GetComponentsInChildren<Renderer>().Where(r=>r.name.StartsWith("DuckV2_Voxel_")).ToArray();
   Assert.AreEqual(2190,duck.Length);Assert.AreEqual(2190,duck.Select(r=>r.name).Distinct().Count());
   Assert.IsFalse(level.GetComponentsInChildren<Transform>().Any(t=>t.name=="Flower(Clone)"));
   foreach(var group in Model.Cubes.GroupBy(c=>c.Data.color))Assert.AreEqual(group.Count(),Model.Hives.Where(h=>h.Data.color==group.Key).Sum(h=>h.Data.quota),group.Key+" quotas must match actual cubes");
   Assert.AreEqual(3,hud.VisibleRows);
   int[] third=Enumerable.Range(0,3).Select(c=>Model.WaitingColumn(c)[2]).ToArray();
   for(int c=0;c<3;c++){
    Assert.AreEqual(third[c],hud.DisplayedHive(c,2));Assert.IsNotNull(GameObject.Find("SmallHive_"+third[c]));
    Assert.AreEqual(Model.Hives[third[c]].Remaining.ToString(),hud.ThirdLabels[c].text);
    Assert.IsFalse(hud.ThirdButtons[c].interactable,"Only column fronts can normally be chosen");
    var front=Camera.main.WorldToScreenPoint(GameObject.Find("SmallHive_"+hud.DisplayedHive(c,0)).transform.position);
    var second=Camera.main.WorldToScreenPoint(GameObject.Find("SmallHive_"+hud.DisplayedHive(c,1)).transform.position);
    var back=Camera.main.WorldToScreenPoint(GameObject.Find("SmallHive_"+third[c]).transform.position);
    Assert.Greater(front.y-second.y,Camera.main.pixelHeight*.06f);Assert.Greater(second.y-back.y,Camera.main.pixelHeight*.06f);
   }
  }
  [UnityTest] public IEnumerator ThirdRowSitsBehindToolbarAndPromotesWithFullArt(){
   yield return LoadDuck();Canvas.ForceUpdateCanvases();var level=Object.FindObjectOfType<LevelOneController>();int thirdId=level.Hud.DisplayedHive(0,2);var third=GameObject.Find("SmallHive_"+thirdId);
   var renderers=third.GetComponentsInChildren<Renderer>();var originalShader=GameObject.Find("SmallHive_"+level.Hud.DisplayedHive(0,0)).GetComponentInChildren<Renderer>().sharedMaterial.shader;
   Assert.IsTrue(renderers.All(r=>r.sharedMaterials.All(m=>m.shader==originalShader)),"Use the whole hive behind the toolbar instead of cutting its mesh with a shader");
   var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
   var center=Camera.main.WorldToScreenPoint(bounds.center);var toolbar=(RectTransform)level.Hud.Boosters.transform.Find("ToolbarBase");var corners=new Vector3[4];((RectTransform)level.Hud.Boosters.Buttons[0].transform).GetWorldCorners(corners);var baseCorners=new Vector3[4];toolbar.GetWorldCorners(baseCorners);Assert.Less(baseCorners[1].y,center.y-Camera.main.pixelHeight*.025f,"Blue background stays below row three");
   Assert.Less(Mathf.Abs(center.y-corners[1].y),Camera.main.pixelHeight*.02f,"The booster upper edge must overlap roughly the lower half of row three");
   var quota=level.Hud.ThirdLabels[0];var canvas=quota.GetComponentInParent<Canvas>();
   Assert.Greater(quota.rectTransform.position.y-quota.fontSize*canvas.scaleFactor*.5f,corners[1].y,"The full quota must remain readable above the booster");
   Assert.Less(level.Hud.ThirdLabels[0].transform.GetSiblingIndex(),level.Hud.Boosters.transform.GetSiblingIndex(),"The toolbar also draws over third-row quota labels");
   var pointer=new PointerEventData(EventSystem.current){position=new Vector2(center.x,corners[1].y-5)};var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
   Assert.IsNotEmpty(hits);Assert.IsTrue(hits[0].gameObject==toolbar.gameObject||hits[0].gameObject.transform.IsChildOf(level.Hud.Boosters.transform),"Covered hive area must be owned by the toolbar");
   int identity=third.GetInstanceID();yield return HiveInputFixture.Click(level.Hud,0);yield return HiveInputFixture.Click(level.Hud,0);
   Assert.AreEqual(thirdId,level.Hud.DisplayedHive(0,0));Assert.AreEqual(identity,GameObject.Find("SmallHive_"+thirdId).GetInstanceID());Assert.IsTrue(renderers.All(r=>r.sharedMaterials.All(m=>m.shader==originalShader)),"Promoted model keeps full original materials");
  }
  [UnityTest] public IEnumerator ThirdRowConsumesOneChoiceThenRetryKeepsDuckAndInventory(){
   yield return LoadDuck();var app=BeeHarvestApp.Interface;var level=Object.FindObjectOfType<LevelOneController>();var hud=level.Hud;int choiceColumn=Enumerable.Range(0,3).First(c=>Model.Hives[hud.DisplayedHive(c,2)].Group.Count==1);
   app.SendCommand(new RefillHarvestBoosterCommand(3));hud.Boosters.Buttons[3].onClick.Invoke();yield return null;
   foreach(int c in Enumerable.Range(0,3).Where(c=>Model.Hives[hud.DisplayedHive(c,2)].Group.Count>1))Assert.IsFalse(hud.ThirdButtons[c].interactable,"Pick Any keeps the original linked-group queue-front rule");
   Assert.IsTrue(Model.FreeSelection);Assert.IsTrue(hud.ThirdButtons[choiceColumn].interactable);int count=Model.BoosterCount(3);int chosen=hud.DisplayedHive(choiceColumn,2);string quota=Model.Hives[chosen].Remaining.ToString();
   var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
   ExecuteEvents.Execute(hud.ThirdButtons[choiceColumn].gameObject,pointer,ExecuteEvents.pointerClickHandler);
   float limit=Time.realtimeSinceStartup+2;while(!Model.Hives[chosen].Started&&Time.realtimeSinceStartup<limit)yield return null;
   Assert.IsTrue(Model.Hives[chosen].Started);Assert.IsFalse(Model.FreeSelection);Assert.AreEqual(count,Model.BoosterCount(3));
   Assert.IsTrue(Model.Hives.Where((h,i)=>i!=chosen).All(h=>!h.Started));
   HiveInputFixture.RetryLevelForTest();yield return null;Assert.AreEqual(2190,Model.Remaining);Assert.AreEqual(2,level.LevelNumber);Assert.AreEqual(count,Model.BoosterCount(3));
   Assert.IsFalse(Model.FreeSelection);Assert.AreEqual(quota,hud.ThirdLabels[choiceColumn].text);
  }
  [UnityTest] public IEnumerator DuckLeaveAndStartBeginTheSameLevel(){
   yield return LoadDuck();var level=Object.FindObjectOfType<LevelOneController>();yield return HiveInputFixture.Click(level.Hud,0);
   var settings=Object.FindObjectOfType<SettingsPopupController>();settings.OpenButton.onClick.Invoke();settings.HomeButton.onClick.Invoke();settings.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
   int generation=Model.Generation,remaining=Model.Remaining;yield return new WaitForSeconds(.2f);
   GameObject.Find("StartGameButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;
   Assert.AreEqual("Level02",SceneManager.GetActiveScene().name);Assert.AreEqual(generation,Model.Generation);Assert.AreEqual(2190,Model.Remaining);Assert.IsFalse(Model.Hives[0].Started);
  }
  [UnityTest] public IEnumerator DuckReallyDeliversVisibleCargoAndRetryRestoresItsCompleteModel(){
   yield return LoadDuck();var level=Object.FindObjectOfType<LevelOneController>();var board=level.GetComponent<HarvestBoardView>();board.AutoRotate=false;board.PublishCameraView();var rules=BeeHarvestApp.Interface.GetSystem<HarvestSystem>();int column=-1;
   for(int c=0;c<3;c++){int hive=rules.Front(c);if(hive>=0&&rules.CanSelectHive(hive)&&rules.FindTarget(Model.Hives[hive].Data.color)>=0){column=c;break;}}
   Assert.GreaterOrEqual(column,0,"A starting duck hive must have a visible target");yield return HiveInputFixture.Click(level.Hud,column);BeeSceneSteps.Until(()=>Model.Deposited>0,40);Assert.Greater(Model.Deposited,0);Assert.AreEqual(2190,Model.Remaining+Model.InTransit+Model.Deposited);
   HiveInputFixture.RetryLevelForTest();yield return null;Assert.AreEqual(2190,Model.Remaining);Assert.AreEqual(0,Model.Deposited);Assert.IsNotNull(GameObject.Find("Duck(Clone)"));Assert.IsNull(GameObject.Find("Apple(Clone)"));
  }
  [UnityTest] public IEnumerator FinalLevelContinueReturnsHomeAndStartBeginsFirstLevel(){
   SceneManager.LoadScene(LevelOneAssets.SceneName(LevelOneAssets.LastLevel));yield return null;yield return null;var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var app=BeeHarvestApp.Interface;
   BeeHarvestApp.Interface.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
            app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+AdvanceHarvestCommand.LoadedSeconds+.01f));Assert.IsTrue(Model.Won);
   yield return null;var popup=Object.FindObjectOfType<VictoryPopupController>();
   float entryDeadline=Time.realtimeSinceStartup+3;
   while(!popup.ContinueButton.interactable&&Time.realtimeSinceStartup<entryDeadline)yield return null;
   Assert.IsTrue(popup.ContinueButton.interactable,"Wait for the real entry animation and fresh pointer gate before Continue");
   popup.ContinueButton.onClick.Invoke();popup.ContinueButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
   var nav=app.GetModel<MenuNavigationModel>();Assert.IsTrue(nav.IsHome);Assert.AreEqual(1,nav.CurrentLevel);Assert.IsFalse(popup.IsOpen);
   GameObject.Find("StartGameButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
   popup.SendMessage("Update");Assert.IsFalse(popup.IsOpen,"Final settlement remains consumed while loading a new cycle");
   float limit=Time.realtimeSinceStartup+5;while(SceneManager.GetActiveScene().name!="Level01"&&Time.realtimeSinceStartup<limit)yield return null;
   yield return null;yield return null;Assert.AreEqual("Level01",SceneManager.GetActiveScene().name);Assert.AreEqual(1784,Model.Remaining);Assert.IsFalse(Model.Won);
  }
 }
}
