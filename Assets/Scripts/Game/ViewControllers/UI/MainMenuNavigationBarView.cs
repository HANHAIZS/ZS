using System;
using TMPro;
using UnityEngine;

namespace BeeHarvest {
 public sealed class MainMenuNavigationBarView : BeeHarvestController {
  const float TransitionDuration=.25f;
  public RectTransform Selection;
  public RectTransform[] Icons;
  public TMP_Text[] Labels;
  readonly float[] weights=new float[3],startWeights=new float[3];
  float center,startCenter,elapsed;
  int target;
  bool hasPose,animating;

  public void ValidateBindings() {
   if(Selection==null||Icons==null||Labels==null||Icons.Length!=3||Labels.Length!=3)
    throw new InvalidOperationException("Bottom navigation requires its selection, three icons and labels.");
   for(int i=0;i<3;i++)if(Icons[i]==null||Labels[i]==null)
    throw new InvalidOperationException("Bottom navigation has an unbound icon or label.");
  }

  public void Render(MainMenuTab tab,bool immediate=false) {
   int selected=tab==MainMenuTab.Shop?0:tab==MainMenuTab.Home?1:2;
   if(!hasPose||immediate||!isActiveAndEnabled){target=selected;Settle();return;}
   if(selected==target)return;
   target=selected;startCenter=center;elapsed=0;animating=true;
   for(int i=0;i<3;i++)startWeights[i]=weights[i];
   ApplyPose();
  }

  void Update(){Tick(Time.unscaledDeltaTime);}

  public void Tick(float delta) {
   if(!animating||delta<=0||float.IsNaN(delta)||float.IsInfinity(delta))return;
   elapsed+=delta;
   if(elapsed>=TransitionDuration){Settle();return;}
   float remaining=1-elapsed/TransitionDuration;
   float eased=1-remaining*remaining*remaining;
   center=Mathf.Lerp(startCenter,(target+.5f)/3,eased);
   for(int i=0;i<3;i++)weights[i]=Mathf.Lerp(startWeights[i],i==target?1:0,eased);
   ApplyPose();
  }

  void Settle() {
   hasPose=true;animating=false;center=(target+.5f)/3;
   for(int i=0;i<3;i++)weights[i]=i==target?1:0;
   ApplyPose();
  }

  void ApplyPose() {
   Selection.anchorMin=new Vector2(center-1f/6,0);Selection.anchorMax=new Vector2(center+1f/6,0);
   Selection.offsetMin=Vector2.zero;Selection.offsetMax=new Vector2(0,185);
   for(int i=0;i<3;i++) {
    Icons[i].sizeDelta=Vector2.one*Mathf.Lerp(110,142,weights[i]);
    Icons[i].anchoredPosition=new Vector2(0,Mathf.Lerp(75,126,weights[i]));
    Labels[i].alpha=weights[i];
    Labels[i].gameObject.SetActive(i==target||weights[i]>0);
   }
  }

  void OnApplicationPause(bool paused){if(paused&&hasPose)Settle();}
  void OnDisable(){if(hasPose)Settle();hasPose=false;animating=false;}
 }
}
