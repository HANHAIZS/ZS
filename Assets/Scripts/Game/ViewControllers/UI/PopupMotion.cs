using UnityEngine;
namespace BeeHarvest {
 /// <summary>Cached presentation only; the owning controller keeps modal and command ownership.</summary>
 public sealed class PopupMotion {
  sealed class Panel {
   public readonly GameObject Root;
   public readonly CanvasGroup Group;
   readonly Transform visual;
   readonly Vector3 restScale;
   readonly PausedDialogFit fit;
   readonly bool interactable;
   public Panel(GameObject root,RectTransform presentation){
    Root=root;Group=root.GetComponent<CanvasGroup>();if(Group==null)Group=root.AddComponent<CanvasGroup>();
    visual=presentation!=null?presentation:root.transform;restScale=visual.localScale;
    fit=visual.GetComponent<PausedDialogFit>();interactable=Group.interactable;
   }
   public void Scale(float value){if(fit!=null)fit.SetMotionScale(value);else if(visual!=null)visual.localScale=restScale*value;}
   public void Restore(){Scale(1);if(Group!=null){Group.alpha=1;Group.interactable=interactable;}}
  }
  readonly GameObject dim;
  readonly CanvasGroup dimGroup;
  readonly Panel[] panels;
  int selected,phase; // 0 hidden, 1 opening, 2 shown, 3 closing
  float elapsed,scale,startScale,startAlpha,startDim;
  public bool IsClosing=>phase==3;
  public bool IsOpening=>phase==1;
  public bool IsVisible=>phase!=0;
  public PopupMotion(GameObject background,GameObject content,RectTransform presentation=null,GameObject alternate=null){
   dim=background;dimGroup=dim.GetComponent<CanvasGroup>();if(dimGroup==null)dimGroup=dim.AddComponent<CanvasGroup>();
   panels=alternate==null?new[]{new Panel(content,presentation)}:new[]{new Panel(content,presentation),new Panel(alternate,null)};
  }
  public void Show(int index=0){
   bool visible=IsVisible,same=visible&&selected==index;
   if(same&&!IsClosing)return;
   startDim=visible?dimGroup.alpha:0;
   startAlpha=same?panels[selected].Group.alpha:0;startScale=same?scale:.85f;
   if(visible&&!same){panels[selected].Root.SetActive(false);panels[selected].Restore();}
   selected=index;phase=1;elapsed=0;scale=startScale;
   dim.SetActive(true);panels[selected].Root.SetActive(true);panels[selected].Restore();
   panels[selected].Group.alpha=startAlpha;panels[selected].Scale(scale);dimGroup.alpha=startDim;
  }
  public void Close(){
   if(!IsVisible||IsClosing)return;
   phase=3;elapsed=0;startScale=scale;startAlpha=panels[selected].Group.alpha;startDim=dimGroup.alpha;
   panels[selected].Group.interactable=false;
  }
  /// <returns>True exactly on the frame when a requested close completes.</returns>
  public bool Tick(float delta){
   if((phase!=1&&phase!=3)||delta<=0||float.IsNaN(delta)||float.IsInfinity(delta))return false;
   elapsed+=delta;
   if(phase==3){
    float t=Mathf.Clamp01(elapsed/.15f),e=t*t*(3-2*t);
    scale=Mathf.Lerp(startScale,.85f,e);panels[selected].Scale(scale);
    panels[selected].Group.alpha=Mathf.Lerp(startAlpha,0,e);dimGroup.alpha=Mathf.Lerp(startDim,0,e);
    if(t<1)return false;HideImmediately();return true;
   }
   float progress=Mathf.Clamp01(elapsed/.25f),fade=progress*progress*(3-2*progress);
   scale=elapsed<.175f?Mathf.Lerp(startScale,1.03f,1-Mathf.Pow(1-elapsed/.175f,3)):Mathf.Lerp(1.03f,1,Mathf.SmoothStep(0,1,(elapsed-.175f)/.075f));
   panels[selected].Scale(scale);panels[selected].Group.alpha=Mathf.Lerp(startAlpha,1,fade);dimGroup.alpha=Mathf.Lerp(startDim,1,fade);
   if(progress>=1){phase=2;scale=1;panels[selected].Scale(1);panels[selected].Group.alpha=1;dimGroup.alpha=1;}
   return false;
  }
  public void HideImmediately(){
   phase=0;
   foreach(var panel in panels){if(panel.Root!=null)panel.Root.SetActive(false);panel.Restore();}
   if(dim!=null)dim.SetActive(false);if(dimGroup!=null)dimGroup.alpha=1;
  }
 }
}
