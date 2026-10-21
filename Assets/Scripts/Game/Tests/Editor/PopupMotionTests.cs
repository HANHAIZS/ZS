using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class PopupMotionTests {
  GameObject root,dim,panel,alternate;object motion;Type type;
  CanvasGroup Group(GameObject go)=>go.GetComponent<CanvasGroup>();
  object Call(string method,params object[] args)=>type.GetMethod(method).Invoke(motion,args);
  [SetUp] public void Setup(){root=new GameObject("PopupFixture",typeof(RectTransform));dim=new GameObject("Dim",typeof(RectTransform));panel=new GameObject("Panel",typeof(RectTransform));alternate=new GameObject("Alternate",typeof(RectTransform));foreach(var go in new[]{dim,panel,alternate})go.transform.SetParent(root.transform,false);panel.transform.localScale=Vector3.one*.6f;alternate.transform.localScale=Vector3.one*.7f;type=typeof(SettingsPopupController).Assembly.GetType("BeeHarvest.PopupMotion");Assert.IsNotNull(type,"Popups need a shared time-driven presentation");motion=Activator.CreateInstance(type,new object[]{dim,panel,null,alternate});}
  [TearDown] public void Cleanup(){UnityEngine.Object.DestroyImmediate(root);}
  [Test] public void OpensThroughExactElasticPeakWithSynchronizedFade(){Call("Show",0);Assert.AreEqual(.6f*.85f,panel.transform.localScale.x,.0001f);Assert.AreEqual(0,Group(panel).alpha);Assert.AreEqual(0,Group(dim).alpha);Call("Tick",.08f);Assert.That(Group(panel).alpha,Is.InRange(.01f,.99f));Assert.That(Group(dim).alpha,Is.InRange(.01f,.99f));Call("Tick",.095f);Assert.AreEqual(.6f*1.03f,panel.transform.localScale.x,.0001f);Call("Tick",.075f);Assert.AreEqual(.6f,panel.transform.localScale.x,.0001f);Assert.AreEqual(1,Group(panel).alpha);Assert.AreEqual(1,Group(dim).alpha);}
  [Test] public void CloseFromIntermediatePoseBlocksClicksUntilFadeCompletes(){Call("Show",0);Call("Tick",.08f);float scale=panel.transform.localScale.x,alpha=Group(panel).alpha;Call("Close");Assert.AreEqual(scale,panel.transform.localScale.x);Assert.AreEqual(alpha,Group(panel).alpha);Assert.IsFalse(Group(panel).interactable);Assert.IsTrue(Group(dim).blocksRaycasts);Call("Tick",.075f);Assert.IsTrue(panel.activeSelf);Assert.That(Group(panel).alpha,Is.InRange(.0001f,alpha));Call("Tick",.075f);Assert.IsFalse(panel.activeSelf);Assert.IsFalse(dim.activeSelf);Assert.AreEqual(.6f,panel.transform.localScale.x,.0001f);}
  [Test] public void SwitchingPanelsKeepsDimAndRepeatedUseDoesNotAccumulateScale(){for(int i=0;i<4;i++){Call("Show",0);Call("Tick",.25f);Call("Show",1);Assert.IsFalse(panel.activeSelf);Assert.AreEqual(1,Group(dim).alpha);Assert.AreEqual(.7f*.85f,alternate.transform.localScale.x,.0001f);Call("Tick",.25f);Assert.AreEqual(.7f,alternate.transform.localScale.x,.0001f);Call("HideImmediately");Assert.IsFalse(dim.activeSelf);}}
  [Test] public void InvalidTimeDoesNotAdvanceAndForcedHideCancelsOldMotion(){Call("Show",0);float scale=panel.transform.localScale.x;foreach(float t in new[]{0,-1,float.NaN,float.PositiveInfinity})Call("Tick",t);Assert.AreEqual(scale,panel.transform.localScale.x);Call("Close");Call("HideImmediately");Call("Tick",10f);Assert.IsFalse(dim.activeSelf);Call("Show",0);Assert.AreEqual(.6f*.85f,panel.transform.localScale.x,.0001f);}
 }
}
