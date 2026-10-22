using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeeHarvest {
 /// <summary>Presentation-only feedback. Drawing changes while Button layout, raycasts and masks stay fixed.</summary>
 [DisallowMultipleComponent,RequireComponent(typeof(Button))]
 public sealed class ButtonPressMotion : BeeHarvestController,IPointerDownHandler,IPointerUpHandler,IPointerEnterHandler,IPointerExitHandler {
  const float PressSeconds=.06f,ReboundHalfSeconds=.09f,PressedScale=.94f,PeakScale=1.025f;
  Button button;RectTransform rect;ButtonPressGraphic[] visuals;ButtonPressBackdrop[] backdrops;
  internal float VisualScale=>factor;
  internal Vector3 PivotWorld=>rect.TransformPoint(rect.rect.center);
  int phase; // 0 idle, 1 pressing, 2 rebounding, 3 held
  int? pointer;float elapsed,factor=1,startFactor=1;bool releasePending;

  public static void Install(GameObject root) {
   foreach(var target in root.GetComponentsInChildren<Button>(true))
    if(target.GetComponent<ButtonPressMotion>()==null)target.gameObject.AddComponent<ButtonPressMotion>();
  }
  void Cache() {
   if(button!=null)return;
   button=GetComponent<Button>();rect=(RectTransform)transform;
   var owned=new List<ButtonPressGraphic>();var backgrounds=new List<ButtonPressBackdrop>();
   foreach(var graphic in GetComponentsInChildren<Graphic>(true)) {
    if(graphic is TMP_SubMeshUI||graphic.GetComponentInParent<Button>(true)!=button)continue;
    var visual=graphic.GetComponent<ButtonPressGraphic>();if(visual==null)visual=graphic.gameObject.AddComponent<ButtonPressGraphic>();
    visual.Initialize(this);owned.Add(visual);
    var image=graphic as Image;var backdrop=image!=null?ButtonPressBackdrop.Register(image,this):null;
    if(backdrop!=null&&!backgrounds.Contains(backdrop))backgrounds.Add(backdrop);
   }
   visuals=owned.ToArray();backdrops=backgrounds.ToArray();
  }
  void Awake(){Cache();}
  void OnEnable(){Cache();factor=1;phase=0;pointer=null;releasePending=false;}
  public void OnPointerDown(PointerEventData data) {
   if(!isActiveAndEnabled||data.button!=PointerEventData.InputButton.Left||pointer.HasValue||!button.IsInteractable())return;
   pointer=data.pointerId;BeginPress();
  }
  public void OnPointerUp(PointerEventData data) {
   if(data.button!=PointerEventData.InputButton.Left||pointer!=data.pointerId)return;
   pointer=null;
   // A tap released before the first rendered frame must still show its press.
   if(phase==1)releasePending=true;else BeginRelease();
  }
  public void OnPointerExit(PointerEventData data){if(pointer==data.pointerId)BeginRelease();}
  public void OnPointerEnter(PointerEventData data){if(pointer==data.pointerId&&button.IsInteractable())BeginPress();}
  void BeginPress() {
   startFactor=factor;elapsed=0;phase=1;releasePending=false;
  }
  void BeginRelease() {
   releasePending=false;
   if(phase==0)return;
   startFactor=factor;elapsed=0;phase=2;
  }
  void Update(){if(phase!=0)Tick(Time.unscaledDeltaTime);}
  public void Tick(float delta) {
   if(phase==0)return;
   if(pointer.HasValue&&!button.IsInteractable()){Restore();return;}
   if(delta<=0||float.IsNaN(delta)||float.IsInfinity(delta)||phase==3)return;
   elapsed+=delta;
   if(phase==1) {
    float t=Mathf.Clamp01(elapsed/PressSeconds);SetScale(Mathf.Lerp(startFactor,PressedScale,1-(1-t)*(1-t)*(1-t)));
    if(t>=1){phase=3;if(releasePending)BeginRelease();}
    return;
   }
   if(elapsed>=2*ReboundHalfSeconds){SetScale(1);phase=0;return;}
   float progress=elapsed<ReboundHalfSeconds?elapsed/ReboundHalfSeconds:(elapsed-ReboundHalfSeconds)/ReboundHalfSeconds;
   float eased=progress*progress*(3-2*progress);
   SetScale(elapsed<ReboundHalfSeconds?Mathf.Lerp(startFactor,PeakScale,eased):Mathf.Lerp(PeakScale,1,eased));
  }
  void SetScale(float value) {
   factor=value;
   if(visuals!=null)foreach(var visual in visuals)if(visual!=null)visual.Redraw();
   if(backdrops!=null)foreach(var backdrop in backdrops)if(backdrop!=null)backdrop.Redraw();
  }
  void Restore(){SetScale(1);phase=0;pointer=null;releasePending=false;elapsed=0;}
  void OnDisable(){Restore();}
  void OnApplicationPause(bool paused){if(paused)Restore();}
  void OnApplicationFocus(bool focused){if(!focused)Restore();}
 }
}
