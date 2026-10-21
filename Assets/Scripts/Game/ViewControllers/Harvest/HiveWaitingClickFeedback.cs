using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BeeHarvest {
 // Disabled queue buttons retain selection semantics while accepting presentation-only taps.
 public sealed class HiveWaitingClickFeedback : BeeHarvestController, IPointerClickHandler {
  LevelOneHud hud;Button button;int column,row;Action<int> click;
  public void Initialize(LevelOneHud controls,Button target,int c,int r,Action<int> callback){hud=controls;button=target;column=c;row=r;click=callback;}
  public void OnPointerClick(PointerEventData data){
   if(data.button==PointerEventData.InputButton.Left&&isActiveAndEnabled&&hud!=null&&button!=null&&!button.IsInteractable()&&hud.CanPlayWaitingFeedback(column,row))click(hud.DisplayedHive(column,row));
  }
 }
}
