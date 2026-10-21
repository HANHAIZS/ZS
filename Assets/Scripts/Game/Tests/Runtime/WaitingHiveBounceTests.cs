using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests
{
 public sealed class WaitingHiveBounceTests : EconomyTestFixture
 {
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static LevelOneController Level=>Object.FindObjectOfType<LevelOneController>();
  static HiveHopController Hop=>Level.GetComponent<HiveHopController>();
  static HiveModelView View=>Level.GetComponent<HiveModelView>();
  static GameObject Root(int id)=>GameObject.Find("SmallHive_"+id);
  static Vector3 Screen(int id)=>Camera.main.WorldToScreenPoint(Root(id).transform.position);
  static float Ratio(int id)=>Root(id).transform.localScale.y/Root(id).transform.localScale.x;
  static IEnumerator Load(int number=1){SceneManager.LoadScene(LevelOneAssets.SceneName(number));yield return null;yield return null;Level.enabled=false;}
  [TearDown] public void CancelFeedback(){if(Level!=null)Hop.Cancel();}
  static void Sync(){Level.Hud.Sync();View.Sync();}
  static void Tap(int column,int row){
   var control=row==0?Level.Hud.QueueButtons[column].gameObject:row==1?Level.Hud.RearButtons[column].gameObject:Level.Hud.ThirdButtons[column].gameObject;
   var point=RectTransformUtility.WorldToScreenPoint(null,Level.Hud.RowLabel(column,row).transform.position);
   var data=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=point};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Assert.IsNotEmpty(hits);
   var target=ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
   Assert.AreEqual(control,target,"Real raycast must reach the intended existing hive control");
   ExecuteEvents.Execute(target,data,ExecuteEvents.pointerClickHandler);
  }
  [UnityTest] public IEnumerator RearTapBouncesInPlaceWithoutSelectingOrConsumingAnything(){
   yield return Load();int id=Level.Hud.DisplayedHive(0,1);var before=Screen(id);int quota=Model.Hives[id].Remaining;var inventory=Enumerable.Range(0,4).Select(Model.BoosterCount).ToArray();
   Assert.IsFalse(Level.Hud.RearButtons[0].interactable);Tap(0,1);Assert.Less(Ratio(id),.98f);
   Hop.Tick(.12f);Assert.Greater(Screen(id).y-before.y,10);Assert.AreEqual(before.x,Screen(id).x,1);
   Assert.IsTrue(Model.Slots.All(i=>i<0));Assert.IsFalse(Model.Hives.Any(h=>h.Started));Assert.AreEqual(0,Model.Flights.Count);
   Assert.AreEqual(quota,Model.Hives[id].Remaining);CollectionAssert.AreEqual(inventory,Enumerable.Range(0,4).Select(Model.BoosterCount).ToArray());
   Hop.Tick(.23f);Assert.Less(Vector2.Distance(before,Screen(id)),1);Assert.AreEqual(1,Ratio(id),.001f);
  }
  [UnityTest] public IEnumerator ExposedThirdRowBouncesWhileTheToolbarStillOwnsItsCoveredArea(){
   yield return Load();int id=Level.Hud.DisplayedHive(0,2);var before=Screen(id);Assert.IsFalse(Level.Hud.ThirdButtons[0].interactable);Tap(0,2);Hop.Tick(.12f);Assert.Greater(Screen(id).y-before.y,10);
   var button=Level.Hud.Boosters.Buttons[0];var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Assert.IsNotEmpty(hits);Assert.IsTrue(hits[0].gameObject.transform.IsChildOf(Level.Hud.Boosters.transform));
   Assert.IsFalse(Model.Hives[id].Started);Hop.Tick(.23f);Assert.Less(Vector2.Distance(before,Screen(id)),1);
  }
  [UnityTest] public IEnumerator PauseRestartAndEntryInputLockCancelWaitingFeedbackWithoutReplay(){
   yield return Load();int id=Level.Hud.DisplayedHive(0,1);var before=Screen(id);Tap(0,1);Hop.Tick(.12f);Assert.Greater(Screen(id).y-before.y,10);
   BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(true));Hop.Tick(.01f);Assert.Less(Vector2.Distance(before,Screen(id)),1);
   Hop.ClickWaiting(id);Hop.Tick(.2f);Assert.Less(Vector2.Distance(before,Screen(id)),1);
   BeeHarvestApp.Interface.SendCommand(new PauseHarvestCommand(false));Sync();Tap(0,1);Hop.Tick(.12f);Level.RestartLevel();Sync();Hop.Tick(.2f);Assert.Less(Vector2.Distance(before,Screen(id)),1);
   Tap(0,1);Hop.Tick(.12f);Hop.ClickWaiting(Level.Hud.DisplayedHive(0,0));Assert.Less(Vector2.Distance(before,Screen(id)),1,"Starting real entry cancels waiting-only feedback");
   var data=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(Level.Hud.RearButtons[0].gameObject,data,ExecuteEvents.pointerClickHandler);Hop.Tick(.12f);Assert.Less(Vector2.Distance(before,Screen(id)),1);Hop.Tick(.23f);Assert.IsFalse(Model.Hives[id].Started);
  }
  [UnityTest] public IEnumerator PickAnyStillActivatesAnEligibleRearHiveInsteadOfPlayingOnlyFeedback(){
   yield return Load();int column=Enumerable.Range(0,3).First(c=>Model.Hives[Level.Hud.DisplayedHive(c,1)].Group.Count==1);int id=Level.Hud.DisplayedHive(column,1);var app=BeeHarvestApp.Interface;
   app.SendCommand(new RefillHarvestBoosterCommand(3));app.SendCommand(new UseFreeHiveSelectionCommand());Sync();Assert.IsTrue(Level.Hud.RearButtons[column].interactable);Tap(column,1);Hop.Tick(.35f);
   Assert.IsTrue(Model.Hives[id].Started);Assert.IsFalse(Model.FreeSelection);Assert.AreEqual(1,Model.Hives.Count(h=>h.Started));
  }
  [UnityTest] public IEnumerator HiddenRearHiveKeepsItsQuestionAndGreyTintThroughoutWaitingBounce(){
   yield return Load(12);int id=Level.Hud.DisplayedHive(0,1);var before=Screen(id);var marker=GameObject.Find("MysteryHive_"+id).GetComponent<Text>();Tap(0,1);Hop.Tick(.12f);View.TickMysteries(.3f);Assert.Greater(Screen(id).y-before.y,10);
   Assert.IsTrue(marker.gameObject.activeInHierarchy);Assert.AreEqual(1,marker.color.a);var renderer=Root(id).GetComponentInChildren<Renderer>();var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block,0);Assert.Less(Vector4.Distance(new Color(.48f,.50f,.56f),block.GetColor("_BaseColor")),.0001f);Assert.IsFalse(Model.Hives[id].Started);
   Hop.Tick(.23f);Assert.Less(Vector2.Distance(before,Screen(id)),1);Assert.IsTrue(marker.gameObject.activeInHierarchy);
  }
  [UnityTest] public IEnumerator SceneExitDuringWaitingBounceLeavesNoCallbackOrDestroyedHudAccess(){
   yield return Load();Tap(0,1);Hop.Tick(.1f);SceneManager.LoadScene("MainMenu");yield return null;yield return null;Assert.IsNull(Object.FindObjectOfType<HiveHopController>());
  }
 }
}
