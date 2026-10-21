using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine.EventSystems;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class HarvestBoostersUiTests : EconomyTestFixture {
  static void Restock(){var m=BeeHarvestApp.Interface.GetModel<HarvestModel>();var stock=(int[])typeof(HarvestModel).GetField("boosterCounts",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(m);for(int i=0;i<4;i++)stock[i]=1;Object.FindObjectOfType<HarvestBoosterBar>().SetInputEnabled(true);}
  static Button Button(string name){var go=GameObject.Find(name);Assert.IsNotNull(go,"Missing booster UI: "+name);return go.GetComponent<Button>();}
  [UnityTest] public IEnumerator ToolbarAllowsRearHopAndSixthSlotAboveFirst(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;Restock();
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();var hud=Object.FindObjectOfType<LevelOneHud>();
   Button("Booster_FreeSelect").onClick.Invoke();yield return null;
   var rear=hud.NextLabels[1].GetComponent<Button>();Assert.IsNotNull(rear);Assert.IsTrue(rear.interactable);
   rear.onClick.Invoke();float deadline=Time.realtimeSinceStartup+2;
   while(!m.Hives[12].Started&&Time.realtimeSinceStartup<deadline)yield return null;
   Assert.IsTrue(m.Hives[12].Started);Assert.IsFalse(m.Hives[11].Started);
   Button("Booster_AddSlot").onClick.Invoke();yield return null;
   Assert.AreEqual(6,m.Slots.Count);Assert.IsFalse(Button("Booster_AddSlot").interactable);
   var first=Camera.main.WorldToScreenPoint(GameObject.Find("HiveSlotTile_0").transform.position);
   var sixth=Camera.main.WorldToScreenPoint(GameObject.Find("HiveSlotTile_5").transform.position);
   Assert.AreEqual(first.x,sixth.x,1);Assert.Greater(sixth.y-first.y,Camera.main.pixelHeight*.06f);
   app.SendCommand(new PauseHarvestCommand(true));yield return null;
   int rev=m.Revision;Button("Booster_Shuffle").onClick.Invoke();Assert.AreEqual(rev,m.Revision);
  }
  [UnityTest] public IEnumerator SettingsBlocksHelpersLeaveAndRetryResetTheirEffects(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;Restock();
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();
   Button("Booster_AddSlot").onClick.Invoke();Button("Booster_Speed").onClick.Invoke();Button("Booster_FreeSelect").onClick.Invoke();
   var settings=Object.FindObjectOfType<SettingsPopupController>();settings.OpenButton.onClick.Invoke();yield return null;
   Assert.IsFalse(Button("Booster_Shuffle").interactable);int rev=m.Revision;
   Button("Booster_Shuffle").onClick.Invoke();Assert.AreEqual(rev,m.Revision);
   settings.HomeButton.onClick.Invoke();settings.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();
   Object.FindObjectOfType<MainMenuNavigationController>().Page.StartGameButton.onClick.Invoke();yield return null;yield return null;
   Assert.AreEqual(5,m.Slots.Count);Assert.IsFalse((bool)typeof(HarvestModel).GetProperty("SpeedBoost").GetValue(m));
   Assert.IsFalse((bool)typeof(HarvestModel).GetProperty("FreeSelection").GetValue(m));
   HiveInputFixture.RetryLevelForTest();yield return null;yield return null;
   Assert.AreEqual(5,m.Slots.Count);Assert.IsFalse((bool)typeof(HarvestModel).GetProperty("SpeedBoost").GetValue(m));
   Assert.IsFalse((bool)typeof(HarvestModel).GetProperty("FreeSelection").GetValue(m));
   Assert.IsFalse(GameObject.Find("LevelOne").transform.Find("HiveSlotTile_5").gameObject.activeSelf);
  }
  [UnityTest] public IEnumerator AnySelectionCanPageToAFourthWaitingHive(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;Restock();
   var app=BeeHarvestApp.Interface;var data=app.GetUtility<LevelOneAssets>().Level();
   var hives=data.hives.Select(h=>new HiveData{column=h.column,color=h.color,quota=h.quota}).ToList();
   int quota=hives[0].quota;hives[0].quota=20;hives.Add(new HiveData{column=0,color=hives[0].color,quota=quota-20});int appendedHive=hives.Count-1;
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=data.cubes,hives=hives.ToArray()}));yield return null;
   Button("Booster_FreeSelect").onClick.Invoke();yield return null;
   Assert.AreEqual("",Object.FindObjectOfType<HarvestBoosterBar>().StateLabels[3].text,"Single-use item has no ON display");
   for(int page=0;page<3;page++){Button("WaitingNextPage").onClick.Invoke();yield return null;}
   var hud=Object.FindObjectOfType<LevelOneHud>();Assert.AreEqual((quota-20).ToString(),hud.ThirdLabels[0].text);
   hud.ThirdButtons[0].onClick.Invoke();float deadline=Time.realtimeSinceStartup+2;
   var m=app.GetModel<HarvestModel>();while(!m.Hives[appendedHive].Started&&Time.realtimeSinceStartup<deadline)yield return null;
   Assert.IsTrue(m.Hives[appendedHive].Started);Assert.IsFalse(m.Hives[0].Started);Assert.IsFalse(m.Hives[1].Started);
   Assert.IsFalse(m.FreeSelection);Assert.AreEqual(0,m.BoosterCount(3));Assert.AreEqual(0,hud.QueuePage);
   Assert.IsFalse(hud.NextPage.gameObject.activeSelf);Assert.IsTrue(hud.QueueButtons[0].interactable);Assert.IsFalse(hud.RearButtons[0].interactable);
   var bar=Object.FindObjectOfType<HarvestBoosterBar>();Assert.AreEqual("",bar.StateLabels[3].text,"No ON/OFF state label");
   hud.RearButtons[0].onClick.Invoke();yield return new WaitForSeconds(.4f);Assert.IsFalse(m.Hives[1].Started,"A second rear hive needs another item");
   bar.RefillButtons[3].onClick.Invoke();bar.PurchasePopup.PurchaseButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;Assert.AreEqual(3,m.BoosterCount(3));Assert.IsFalse(m.FreeSelection);
   bar.Buttons[3].onClick.Invoke();Assert.AreEqual(2,m.BoosterCount(3));hud.RearButtons[0].onClick.Invoke();
   deadline=Time.realtimeSinceStartup+2;while(!m.Hives[1].Started&&Time.realtimeSinceStartup<deadline)yield return null;
   Assert.IsTrue(m.Hives[1].Started);Assert.IsFalse(m.FreeSelection);
  }
  [UnityTest] public IEnumerator PressesDuringHopCannotFireAfterLanding(){
   foreach(string name in new[]{"Booster_Speed","Next_0","WaitingNextPage"}){
    SceneManager.LoadScene("Level01");yield return null;yield return null;Restock();
    var app=BeeHarvestApp.Interface;var original=app.GetUtility<LevelOneAssets>().Level();
    var data=original.hives.Select(h=>new HiveData{column=h.column,color=h.color,quota=h.quota}).ToList();
    int quota=data[0].quota;data[0].quota=20;data.Add(new HiveData{column=0,color=data[0].color,quota=quota-20});
    app.SendCommand(new StartLevelCommand(new LevelData{cubes=original.cubes,hives=data.ToArray()}));yield return null;
    Button("Booster_FreeSelect").onClick.Invoke();yield return null;
    var m=app.GetModel<HarvestModel>();var hud=Object.FindObjectOfType<LevelOneHud>();
    var eventSystem=EventSystem.current;var module=(StandaloneInputModule)eventSystem.currentInputModule;module.enabled=false;eventSystem.enabled=false;
    var fake=module.gameObject.AddComponent<SettingsTouchInput>();module.inputOverride=fake;
    var flags=BindingFlags.Instance|BindingFlags.NonPublic;
    var read=typeof(PointerInputModule).GetMethod("GetTouchPointerEventData",flags);
    var process=typeof(StandaloneInputModule).GetMethod("ProcessTouchPress",flags);
    var target=name=="Next_0"?hud.NextLabels[0].gameObject:Button(name).gameObject;
    int hopping=hud.DisplayedHive(2,false);hud.QueueButtons[2].onClick.Invoke();yield return new WaitForSeconds(.08f);
    Assert.IsFalse(Button("Booster_Speed").interactable);
    var point=RectTransformUtility.WorldToScreenPoint(null,target.transform.position);fake.Count=1;
    var args=new object[]{new Touch{fingerId=21,position=point,phase=TouchPhase.Began},false,false};
    var pointer=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{pointer,args[1],args[2]});
    yield return new WaitForSeconds(.4f);Assert.IsTrue(m.Hives[hopping].Started,"Initial hop lands before held touch is released");
    args=new object[]{new Touch{fingerId=21,position=point,phase=TouchPhase.Ended},false,false};
    pointer=(PointerEventData)read.Invoke(module,args);process.Invoke(module,new object[]{pointer,args[1],args[2]});fake.Count=0;
    yield return new WaitForSeconds(.4f);
    Assert.IsFalse(m.SpeedBoost,name+" captured a blocked press");Assert.IsFalse(m.Hives[1].Started,name+" started a second hop from a blocked press");Assert.AreEqual(0,hud.QueuePage,name+" changed pages from a blocked press");
    module.inputOverride=null;module.enabled=true;eventSystem.enabled=true;
    Button("Booster_Speed").onClick.Invoke();Assert.IsTrue(m.SpeedBoost,"Fresh input still works");
   }
  }
  static Bounds Geometry(Transform root){var rs=root.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
  [UnityTest] public IEnumerator SixAcceleratedHivesKeepContinuousCargoAndCollisionClearance(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;Restock();
   var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;HiveInputFixture.UseFiveColumnGeometryFixture();
   var app=BeeHarvestApp.Interface;var m=app.GetModel<HarvestModel>();
   Button("Booster_AddSlot").onClick.Invoke();Button("Booster_Speed").onClick.Invoke();
   for(int c=0;c<5;c++)app.SendCommand(new ActivateHiveCommand(c));app.SendCommand(new ActivateHiveCommand(0));
   var hive=Object.FindObjectOfType<HiveModelView>();var board=Object.FindObjectOfType<HarvestBoardView>();var bees=Object.FindObjectOfType<BeeFlightView>();
   hive.Sync();board.Sync();bees.RenderFlights();Assert.AreEqual(6,m.Slots.Count);Assert.AreEqual(6,m.Flights.Count,"Six separate mouths dispatch concurrently");
   var source=GameObject.Find(m.Cubes[m.Flights[0].Cube].Data.name).GetComponent<Renderer>();
   var screen=Camera.main.WorldToScreenPoint(source.bounds.center);Vector3 prior=Vector3.zero;bool carried=false,sawReach=false,sawLift=false;
   for(int step=0;step<1000;step++){
    board.PublishCameraView();BeeSceneSteps.Advance(.01f);hive.Sync();board.Sync();bees.RenderFlights();
    var actors=Object.FindObjectsOfType<Transform>().Where(t=>t.name.StartsWith("BlenderBeeCarrier_")).ToArray();
    foreach(var loaded in actors.Where(t=>t.Find("CarriedVoxel").gameObject.activeSelf))Assert.IsFalse(Geometry(loaded).Intersects(Geometry(GameObject.Find("Flower(Clone)").transform)),"Loaded bee or cargo intersects the model at frame "+step);
    if(m.Flights[0].Stage==0&&m.Flights[0].Progress>=AdvanceHarvestCommand.ApproachSeconds)sawReach=true;
    var actor=actors.FirstOrDefault(t=>t.name=="BlenderBeeCarrier_0");if(actor==null)continue;
    // The entrance shrinks actors below a pixel before recycling. Zero-sized float bounds cannot prove penetration.
    if(actor.lossyScale.x<.001f)continue;
    var cargo=actor.Find("CarriedVoxel").GetComponent<Renderer>();if(!cargo.gameObject.activeSelf)continue;
    foreach(var part in actor.GetComponentsInChildren<Renderer>())if(part!=cargo&&!part.name.Contains("_Grip_"))Assert.IsFalse(BeeGeometryAssertions.Intersects(cargo,part),"Cargo overlaps "+part.name+" at step "+step+", stage "+m.Flights[0].Stage+", progress "+m.Flights[0].Progress);
    var now=Camera.main.WorldToScreenPoint(cargo.bounds.center);
    if(!carried){Assert.Less(Vector2.Distance(screen,now),1f,"Boosted pickup starts at the source");carried=true;}
    else if(m.Flights[0].Progress<AdvanceHarvestCommand.LiftSeconds){Assert.Less(Vector2.Distance(prior,now),12f,"Boosted lift remains continuous");sawLift=true;}
    prior=now;
   }
   Assert.IsTrue(sawReach&&sawLift&&carried);Assert.Greater(m.Deposited,0);Assert.AreEqual(1784,m.Remaining+m.InTransit+m.Deposited);
  }
 }
}
