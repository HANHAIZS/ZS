using UnityEngine;
namespace BeeHarvest {
 // Responsive fitting and popup motion compose; resizing never replaces the animation multiplier.
 public sealed class PausedDialogFit : BeeHarvestController {
  RectTransform frame;
  bool fitting;
  float motionScale=1,fitScale=1;
  void Awake(){frame=(RectTransform)transform;}
  void OnEnable(){Fit();}
  void OnRectTransformDimensionsChange(){Fit();}
  public void SetMotionScale(float value){motionScale=value;Fit();}
  void Fit(){
   if(fitting)return;
   if(frame==null)frame=(RectTransform)transform;
   var parent=frame.parent as RectTransform;
   if(parent==null||parent.rect.width<=0||parent.rect.height<=0||frame.rect.width<=0||frame.rect.height<=0)return;
   fitting=true;
   fitScale=Mathf.Clamp(Mathf.Min(parent.rect.width*.90f/frame.rect.width,parent.rect.height*.55f/frame.rect.height),.01f,1f);
   frame.localScale=Vector3.one*(fitScale*motionScale);
   fitting=false;
  }
 }
}
