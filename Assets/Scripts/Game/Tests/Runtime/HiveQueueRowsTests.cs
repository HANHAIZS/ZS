using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class HiveQueueRowsTests : EconomyTestFixture {
  [UnityTest] public IEnumerator ThreeModelRowsPreviewPromoteAndRestoreOnRetry(){
   SceneManager.LoadScene("Level01");yield return null;yield return null;
   var app=BeeHarvestApp.Interface;var model=app.GetModel<HarvestModel>();
   var hud=Object.FindObjectOfType<LevelOneController>().Hud;
   var rear=GameObject.Find("SmallHive_1");
   Assert.IsNotNull(rear,"The second row must show real Blender hive models instead of a rear-quota caption");
   int identity=rear.GetInstanceID();var frontPosition=GameObject.Find("SmallHive_0").transform.position;
   Assert.AreEqual(30,model.Hives.Count);Assert.AreEqual(3,hud.VisibleRows);
   foreach(int column in new[]{0,1,2}){
    var ids=Enumerable.Range(0,model.Hives.Count).Where(i=>model.Hives[i].Data.column==column&&!model.Hives[i].Started).ToArray();
    Assert.AreEqual(new[]{11,8,11}[column],ids.Length,"Preserve source column lengths");
    var front=GameObject.Find("SmallHive_"+ids[0]);var back=GameObject.Find("SmallHive_"+ids[1]);
    Assert.IsNotNull(front);Assert.IsNotNull(back);
    Assert.AreEqual(model.Hives[ids[1]].Remaining.ToString(),hud.NextLabels[column].text);
    var a=Camera.main.WorldToScreenPoint(front.transform.position);var b=Camera.main.WorldToScreenPoint(back.transform.position);
    Assert.Greater(a.y-b.y,Camera.main.pixelHeight*.06f,"Two model rows must have clear vertical separation");
    var anchor=back.GetComponentsInChildren<Transform>().First(t=>t.name.EndsWith("_NumberAnchor"));
    Assert.Less(Vector2.Distance(Camera.main.WorldToScreenPoint(anchor.position),RectTransformUtility.WorldToScreenPoint(null,hud.NextLabels[column].transform.position)),1f);
   }
   var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,hud.NextLabels[0].transform.position)};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
   Assert.IsNotEmpty(hits);Assert.AreEqual(hud.NextLabels[0].gameObject,hits[0].gameObject,"Rear preview must be the first pointer hit, blocking the flower and front buttons");
   ExecuteEvents.ExecuteHierarchy(hud.NextLabels[0].gameObject,pointer,ExecuteEvents.pointerClickHandler);
   Assert.IsTrue(model.Hives.All(h=>!h.Started),"Rear hives are preview-only");
   yield return HiveInputFixture.Click(hud,0);
   app.SendCommand(new PauseHarvestCommand(true));yield return null;
   var promoted=GameObject.Find("SmallHive_1");
   Assert.AreEqual(identity,promoted.GetInstanceID(),"The actual rear model is reused at the front");
   Assert.Less(Vector3.Distance(frontPosition,promoted.transform.position),.005f);
   Assert.AreEqual("80",hud.QueueLabels[0].text);Assert.AreEqual("78",hud.NextLabels[0].text);Assert.AreEqual("40",hud.ThirdLabels[0].text);
   HiveInputFixture.RetryLevelForTest();yield return null;
   Assert.AreEqual(9,Object.FindObjectOfType<HiveModelView>().GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("SmallHive_")&&t.gameObject.activeInHierarchy));
   Assert.IsTrue(model.Hives.All(h=>!h.Started));Assert.AreEqual("80",hud.NextLabels[0].text);Assert.AreEqual("78",hud.ThirdLabels[0].text);
  }
 }
}
