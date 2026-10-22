using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;

namespace BeeHarvest.Tests {
 public sealed class ButtonPressMotionTests {
  GameObject root,canvas;Button button;RectTransform rect;Type type;Component motion;
  readonly Vector3 original=new Vector3(.8f,1.2f,1);
  [SetUp] public void Setup() {
   root=new GameObject("PressFixture",typeof(RectTransform),typeof(Image),typeof(Button));
   canvas=new GameObject("PressFixtureCanvas",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
   root.transform.SetParent(canvas.transform,false);
   rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(200,80);rect.localScale=original;
   button=root.GetComponent<Button>();button.navigation=new Navigation{mode=Navigation.Mode.None};
   type=typeof(MainMenuPage).Assembly.GetType("BeeHarvest.ButtonPressMotion");
   Assert.IsNotNull(type,"All UI buttons need shared press/release feedback");
   type.GetMethod("Install").Invoke(null,new object[]{root});motion=root.GetComponent(type);
  }
  [TearDown] public void Cleanup(){if(canvas!=null)UnityEngine.Object.DestroyImmediate(canvas);else if(root!=null)UnityEngine.Object.DestroyImmediate(root);}
  PointerEventData Pointer(int id=7,PointerEventData.InputButton key=PointerEventData.InputButton.Left)=>new PointerEventData(EventSystem.current){pointerId=id,button=key};
  void Down(PointerEventData p){ExecuteEvents.Execute(root,p,ExecuteEvents.pointerDownHandler);}
  void Up(PointerEventData p){ExecuteEvents.Execute(root,p,ExecuteEvents.pointerUpHandler);}
  void Tick(float dt){type.GetMethod("Tick").Invoke(motion,new object[]{dt});}
  float VisualWidth(){Canvas.ForceUpdateCanvases();var mesh=button.image.canvasRenderer.GetMesh();return mesh==null?0:mesh.bounds.size.x;}
  void Restored(){Assert.That(Vector3.Distance(original,rect.localScale),Is.LessThan(.0001f),"Button layout transform must preserve its original nonuniform scale");if(root.activeInHierarchy)Assert.AreEqual(200,VisualWidth(),.01f,"Release/cancel must restore the drawn button size");}

  [Test] public void PressAndReleaseBounceBackWithoutAccumulatingScaleOrChangingClick() {
   int clicks=0;button.onClick.AddListener(()=>clicks++);var p=Pointer();
   Down(p);Tick(.03f);Assert.That(VisualWidth(),Is.LessThan(200));Tick(.06f);
   Assert.That(VisualWidth(),Is.LessThan(200));Assert.AreEqual(0,clicks);
   Up(p);Tick(.09f);Assert.That(VisualWidth(),Is.GreaterThan(200));Tick(.1f);Restored();
   ExecuteEvents.Execute(root,p,ExecuteEvents.pointerClickHandler);Assert.AreEqual(1,clicks);
   for(int i=0;i<5;i++){Down(p);Tick(.02f);Up(p);Down(p);Tick(.07f);Up(p);Tick(.09f);Tick(.1f);Restored();}
  }
  [Test] public void PointerOwnershipAndDisabledControlsRejectUnrelatedPresses() {
   var first=Pointer();Down(Pointer(8,PointerEventData.InputButton.Right));Tick(.08f);Restored();
   button.interactable=false;Down(first);Tick(.08f);Restored();button.interactable=true;
   Down(first);Tick(.08f);float held=VisualWidth();Down(Pointer(8));Up(Pointer(8));Tick(.2f);
   Assert.AreEqual(held,VisualWidth(),.0001f,"Another finger must not release the first finger's button");
   Up(first);Tick(.09f);Tick(.1f);Restored();
  }
  [Test] public void DragOutReentryDisableAndBackgroundCannotLeaveAButtonShrunk() {
   var p=Pointer();Down(p);Tick(.08f);ExecuteEvents.Execute(root,p,ExecuteEvents.pointerExitHandler);Tick(.09f);Tick(.1f);Restored();
   ExecuteEvents.Execute(root,p,ExecuteEvents.pointerEnterHandler);Tick(.08f);Assert.That(VisualWidth(),Is.LessThan(200));
   root.SetActive(false);Restored();root.SetActive(true);Tick(.3f);Restored();
   Down(p);Tick(.08f);button.interactable=false;Tick(.01f);Restored();button.interactable=true;
   Down(p);Tick(.08f);root.SendMessage("OnApplicationPause",true);Restored();Tick(.3f);Restored();
  }
  [Test] public void ShrinkingPreservesTheOriginalRaycastEdgeAndPadding() {
   var graphic=button.image;var padding=new Vector4(3,4,5,6);graphic.raycastPadding=padding;
   var edge=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(94,0,0)));
   Assert.IsTrue(RectTransformUtility.RectangleContainsScreenPoint(rect,edge,null,padding));
   Down(Pointer());Tick(.08f);
   Assert.IsTrue(RectTransformUtility.RectangleContainsScreenPoint(rect,edge,null,graphic.raycastPadding),"A press near the original edge must remain clickable while the art shrinks");
   Up(Pointer());Tick(.09f);Tick(.1f);Assert.AreEqual(padding,graphic.raycastPadding);Restored();
  }
  [UnityTest] public IEnumerator MaskedButtonsAndChildGraphicsKeepTheOriginalNativeRaycastEdge() {
   foreach(bool childGraphic in new[]{false,true}) {
    var oldMask=root.GetComponent<Mask>();if(oldMask!=null)UnityEngine.Object.DestroyImmediate(oldMask);
    if(childGraphic) {
     button.image.raycastTarget=false;var child=new GameObject("PriceArtwork",typeof(RectTransform),typeof(Image));child.transform.SetParent(root.transform,false);
     var r=(RectTransform)child.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
    } else root.AddComponent<Mask>();
    UnityEngine.Object.DestroyImmediate(motion);type.GetMethod("Install").Invoke(null,new object[]{root});motion=root.GetComponent(type);
    yield return null;Canvas.ForceUpdateCanvases();
    var p=Pointer();p.position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(98,0,0)));
    var hits=new List<RaycastResult>();canvas.GetComponent<GraphicRaycaster>().Raycast(p,hits);Assert.IsTrue(hits.Any(h=>h.gameObject.transform.IsChildOf(root.transform)),"Fixture edge must be clickable before pressing");
    Down(p);Tick(.08f);Canvas.ForceUpdateCanvases();hits.Clear();canvas.GetComponent<GraphicRaycaster>().Raycast(p,hits);
    Assert.IsTrue(hits.Any(h=>h.gameObject.transform.IsChildOf(root.transform)),"Masks and nested artwork must preserve the original edge while the art shrinks");
    Up(p);Tick(.09f);Tick(.1f);Restored();
   }
  }
  static Color SamplePanel(Image image,Texture2D texture,Vector2 point) {
   Canvas.ForceUpdateCanvases();var mesh=image.canvasRenderer.GetMesh();var vertices=mesh.vertices;var triangles=mesh.triangles;var uvs=mesh.uv;
   for(int i=0;i<triangles.Length;i+=3) {
    int ia=triangles[i],ib=triangles[i+1],ic=triangles[i+2];Vector2 a=vertices[ia],b=vertices[ib],c=vertices[ic];
    var ab=b-a;var ac=c-a;var ap=point-a;float determinant=ab.x*ac.y-ab.y*ac.x;if(Mathf.Abs(determinant)<.00001f)continue;
    float u=(ap.x*ac.y-ap.y*ac.x)/determinant,v=(ab.x*ap.y-ab.y*ap.x)/determinant;
    if(u>=-.0001f&&v>=-.0001f&&u+v<=1.0001f){var uv=uvs[ia]+(uvs[ib]-uvs[ia])*u+(uvs[ic]-uvs[ia])*v;return texture.GetPixelBilinear(uv.x,uv.y);}
   }
   Assert.Fail("Panel must still draw its background under a pressed button");return Color.clear;
  }
  [UnityTest] public IEnumerator BakedSettingsFacesDoNotStayBehindShrinkingButtonsAndRestoreAfterCancel() {
   root.SetActive(false);var panel=new GameObject("CompositePreferences",typeof(RectTransform),typeof(Image));panel.transform.SetParent(canvas.transform,false);
   ((RectTransform)panel.transform).sizeDelta=new Vector2(384,96);var texture=new Texture2D(384,96);var pixels=Enumerable.Repeat(new Color(.2f,.1f,.4f,1),384*96).ToArray();
   for(int n=0;n<3;n++)for(int y=16;y<80;y++)for(int x=40+120*n;x<104+120*n;x++)pixels[y*384+x]=Color.green;
   texture.SetPixels(pixels);texture.Apply();var sprites=new List<Sprite>();var full=Sprite.Create(texture,new Rect(0,0,384,96),Vector2.one*.5f);sprites.Add(full);panel.GetComponent<Image>().sprite=full;
   var controls=new Button[3];
   try {
    for(int n=0;n<3;n++) {
     var child=new GameObject("Preference"+n,typeof(RectTransform),typeof(Image),typeof(Button));child.transform.SetParent(panel.transform,false);
     var r=(RectTransform)child.transform;r.anchorMin=r.anchorMax=Vector2.one*.5f;r.sizeDelta=Vector2.one*64;r.anchoredPosition=new Vector2(72+120*n-192,0);
     var sprite=Sprite.Create(texture,new Rect(40+120*n,16,64,64),Vector2.one*.5f);sprites.Add(sprite);child.GetComponent<Image>().sprite=sprite;controls[n]=child.GetComponent<Button>();
    }
    type.GetMethod("Install").Invoke(null,new object[]{panel});yield return null;var image=panel.GetComponent<Image>();var ring=new Vector2(-151,0);
    Assert.That(SamplePanel(image,texture,ring).g,Is.GreaterThan(.9f));
    for(int n=0;n<2;n++){var p=Pointer(7+n);ExecuteEvents.Execute(controls[n].gameObject,p,ExecuteEvents.pointerDownHandler);var m=controls[n].GetComponent(type);type.GetMethod("Tick").Invoke(m,new object[]{.08f});}
    Assert.That(SamplePanel(image,texture,ring).g,Is.LessThan(.2f),"The exposed ring must contain the purple panel, not the baked green button");
    Assert.That(SamplePanel(image,texture,new Vector2(-31,0)).g,Is.LessThan(.2f),"Two pressed buttons must share one correctly composed backing mesh");
    controls[0].SendMessage("OnApplicationPause",true);Assert.That(SamplePanel(image,texture,ring).g,Is.GreaterThan(.9f));
    Assert.That(SamplePanel(image,texture,new Vector2(-31,0)).g,Is.LessThan(.2f));
    controls[1].SendMessage("OnApplicationPause",true);Assert.That(SamplePanel(image,texture,new Vector2(-31,0)).g,Is.GreaterThan(.9f));
    Assert.AreEqual(4,image.canvasRenderer.GetMesh().vertexCount,"Idle must restore the exact original panel quad");
   } finally {UnityEngine.Object.DestroyImmediate(panel);foreach(var s in sprites)UnityEngine.Object.DestroyImmediate(s);UnityEngine.Object.DestroyImmediate(texture);}
  }
  [Test] public void InstallationIncludesInactiveButtonsAndDoesNotDuplicateComponents() {
   var child=new GameObject("InactiveButton",typeof(RectTransform),typeof(Image),typeof(Button));child.transform.SetParent(root.transform,false);child.SetActive(false);
   type.GetMethod("Install").Invoke(null,new object[]{root});type.GetMethod("Install").Invoke(null,new object[]{root});
   Assert.AreEqual(1,root.GetComponents(type).Length);Assert.AreEqual(1,child.GetComponents(type).Length);
  }
  [UnityTest] public IEnumerator FeedbackUsesUnscaledTimeAndQuickTapStillShrinks() {
   float previous=Time.timeScale;
   try {Time.timeScale=0;var p=Pointer();Down(p);Up(p);yield return null;
    Assert.That(VisualWidth(),Is.LessThan(200),"Even a tap released in one frame must show the press");
    yield return new WaitForSecondsRealtime(.35f);Restored();
   } finally {Time.timeScale=previous;}
  }
 }
 public sealed class ButtonPressMotionUiTests : EconomyTestFixture {
  static Component Feedback(Button button)=>button.GetComponent(typeof(MainMenuPage).Assembly.GetType("BeeHarvest.ButtonPressMotion"));
  static void Advance(Component motion,float delta)=>motion.GetType().GetMethod("Tick").Invoke(motion,new object[]{delta});
  static float FirstGlyphWidth(TMP_Text text){var glyph=text.textInfo.characterInfo[0];var vertices=text.textInfo.meshInfo[glyph.materialReferenceIndex].vertices;return Vector3.Distance(vertices[glyph.vertexIndex],vertices[glyph.vertexIndex+3]);}
  [UnityTest] public IEnumerator TmpTextRebuildsDuringPressWithoutAccumulatingGeometryScale() {
   LevelSceneTestLoader.Load("MainMenu");yield return null;yield return null;
   var page=UnityEngine.Object.FindObjectOfType<MainMenuPage>();var b=page.StartGameButton;var text=b.GetComponentInChildren<TMP_Text>();
   text.ForceMeshUpdate();float originalWidth=FirstGlyphWidth(text);var originalScale=b.transform.localScale;
   var p=new PointerEventData(EventSystem.current){pointerId=7,button=PointerEventData.InputButton.Left};
   ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerDownHandler);Advance(Feedback(b),.08f);
   Assert.That(FirstGlyphWidth(text),Is.LessThan(originalWidth));Assert.AreEqual(originalScale,b.transform.localScale,"Press feedback must leave layout/input transforms fixed");
   text.text="Level 100";text.ForceMeshUpdate();
   float unscaledWidth=Vector3.Distance(text.textInfo.characterInfo[0].vertex_BL.position,text.textInfo.characterInfo[0].vertex_BR.position);
   Assert.That(FirstGlyphWidth(text)/unscaledWidth,Is.InRange(.9f,.99f),"A new text mesh must receive one scale, without reusing already scaled glyphs");
   ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerUpHandler);Advance(Feedback(b),.09f);Assert.That(FirstGlyphWidth(text),Is.GreaterThan(unscaledWidth));
   Advance(Feedback(b),.1f);
   // TMP may advance auto-sizing on rebuild; compare with the current native glyph metrics.
   float finalUnscaled=Vector3.Distance(text.textInfo.characterInfo[0].vertex_BL.position,text.textInfo.characterInfo[0].vertex_BR.position);
   Assert.AreEqual(finalUnscaled,FirstGlyphWidth(text),.001f);Assert.AreEqual(originalScale,b.transform.localScale);
  }
  [UnityTest] public IEnumerator RealNavigationPressComposesWithPlateMotionAndPopupClose() {
   LevelSceneTestLoader.Load("MainMenu");yield return null;yield return null;
   var page=UnityEngine.Object.FindObjectOfType<MainMenuPage>();var b=page.ShopButton;var original=b.transform.localScale;
   var p=new PointerEventData(EventSystem.current){pointerId=7,button=PointerEventData.InputButton.Left};
   ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerDownHandler);Advance(Feedback(b),.07f);
   ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(b.gameObject,p,ExecuteEvents.pointerClickHandler);
   yield return new WaitForSecondsRealtime(.35f);
   Assert.IsTrue(page.Tabs.ShopContent.activeSelf);Assert.AreEqual(original,b.transform.localScale);
   Assert.AreEqual(1f/6,(page.Tabs.NavigationBar.Selection.anchorMin.x+page.Tabs.NavigationBar.Selection.anchorMax.x)*.5f,.0001f);
   Assert.AreEqual(142,page.Tabs.NavigationBar.Icons[0].sizeDelta.x,.001f);
   page.SettingsPopup.Open(null);yield return PopupMotionTestWait.Open(PopupMotionTestWait.Motion(page.SettingsPopup));
   var close=page.SettingsPopup.CloseButton;var closeScale=close.transform.localScale;
   ExecuteEvents.Execute(close.gameObject,p,ExecuteEvents.pointerDownHandler);Advance(Feedback(close),.07f);
   ExecuteEvents.Execute(close.gameObject,p,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(close.gameObject,p,ExecuteEvents.pointerClickHandler);
   yield return PopupMotionTestWait.Close();Assert.IsFalse(page.SettingsPopup.IsOpen);Assert.AreEqual(closeScale,close.transform.localScale);
  }
  [UnityTest] public IEnumerator MainMenuAndGameIncludingDynamicPurchasePopupHaveFeedbackOnEveryButton() {
   var type=typeof(MainMenuPage).Assembly.GetType("BeeHarvest.ButtonPressMotion");Assert.IsNotNull(type,"Button feedback must be installed at both UI creation boundaries");
   foreach(string scene in new[]{"MainMenu","Level01"}) {
    LevelSceneTestLoader.Load(scene);yield return null;yield return null;
    var buttons=UnityEngine.Object.FindObjectsOfType<Button>(true);Assert.That(buttons.Length,Is.GreaterThan(15));
    foreach(var b in buttons)Assert.AreEqual(1,b.GetComponents(type).Length,scene+" missing/duplicate press feedback on "+b.name);
    if(scene=="Level01")Assert.IsNotNull(UnityEngine.Object.FindObjectOfType<HarvestBoosterBar>().PurchasePopup);
   }
  }
 }
}
