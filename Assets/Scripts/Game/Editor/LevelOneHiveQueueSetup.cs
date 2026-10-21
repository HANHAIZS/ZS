using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class LevelOneHiveQueueSetup {
  /// <summary>Reuses the existing five queue buttons and rear labels for two equal-height model rows.</summary>
  [MenuItem("BeeHarvest/Level One/Apply Two Hive Rows")]
  public static void Apply(){
   if(Application.isPlaying)throw new InvalidOperationException("Queue layout must be saved in edit mode.");
   var hud=UnityEngine.Object.FindObjectOfType<LevelOneHud>();
   if(hud==null)throw new InvalidOperationException("Level01 HUD is missing.");
   Undo.RecordObject(hud,"Two hive model rows");
   for(int i=0;i<5;i++){
    float x=.025f+i*.195f;
    Set((RectTransform)hud.QueueButtons[i].transform,x,.105f,x+.17f,.18f);
    var label=hud.NextLabels[i];Set(label.rectTransform,x,.02f,x+.17f,.095f);
    Undo.RecordObject(label,"Rear hive quota");
    label.fontSize=hud.QueueLabels[i].fontSize;label.fontStyle=FontStyle.Bold;
    label.color=Color.white;label.raycastTarget=true;label.alignment=TextAnchor.MiddleCenter;
    var outline=label.GetComponent<Outline>();if(outline==null)outline=Undo.AddComponent<Outline>(label.gameObject);
    Undo.RecordObject(outline,"Rear hive outline");
    var front=hud.QueueLabels[i].GetComponent<Outline>();
    outline.effectColor=front!=null?front.effectColor:Color.black;
    outline.effectDistance=front!=null?front.effectDistance:new Vector2(2,-2);
    outline.useGraphicAlpha=true;EditorUtility.SetDirty(label);EditorUtility.SetDirty(outline);
   }
   EditorUtility.SetDirty(hud);EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);
  }
  static void Set(RectTransform r,float x0,float y0,float x1,float y1){
   Undo.RecordObject(r,"Hive queue row anchors");
   r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;
   EditorUtility.SetDirty(r);
  }
 }
}