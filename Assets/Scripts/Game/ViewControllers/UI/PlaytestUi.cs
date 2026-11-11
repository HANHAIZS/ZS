using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest {
 /// <summary>Shared styling for the scene-owned playtest controls.</summary>
 internal static class PlaytestUi {
  static readonly Color32 Gold=new Color32(248,181,56,255);
  internal static readonly Color32 Purple=new Color32(68,44,136,255);
  internal static readonly Color32 DeepPurple=new Color32(42,26,91,255);
  static readonly Color32 Selected=new Color32(150,95,22,255);

  internal static RectTransform Rect(string name,Transform parent,float left,float bottom,float right,float top){
   var rect=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;
   rect.SetParent(parent,false);rect.anchorMin=new Vector2(left,bottom);rect.anchorMax=new Vector2(right,top);
   rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;
  }
  internal static Image Image(RectTransform rect,Color color,bool raycast=false){
   var image=rect.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=raycast;return image;
  }
  internal static TMP_Text Label(string name,Transform parent,string value,TMP_FontAsset font,float size){
   var rect=Rect(name,parent,0,0,1,1);rect.offsetMin=new Vector2(4,2);rect.offsetMax=new Vector2(-4,-2);
   var label=rect.gameObject.AddComponent<TextMeshProUGUI>();
   label.font=font!=null&&font.HasCharacters(value)?font:TMP_Settings.defaultFontAsset;
   label.text=value;label.fontSize=size;label.fontSizeMax=size;label.fontSizeMin=size*.56f;label.enableAutoSizing=true;
   label.enableWordWrapping=false;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;
   return label;
  }
  internal static Button Button(string name,Transform parent,string value,TMP_FontAsset font,float size,float left,float bottom,float right,float top){
   var rect=Rect(name,parent,left,bottom,right,top);
   var image=Image(rect,Gold,true);var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;
   Decorate(button,value,font,size);
   return button;
  }
  internal static void Decorate(Button button,string value,TMP_FontAsset font,float size){
   var image=button.image;
   image.sprite=null;image.overrideSprite=null;image.type=UnityEngine.UI.Image.Type.Simple;image.color=Gold;
   var fill=Rect("PlaytestFill",button.transform,0,0,1,1);
   fill.offsetMin=new Vector2(3,3);fill.offsetMax=new Vector2(-3,-3);Image(fill,Purple);
   Label("PlaytestLabel",button.transform,value,font,size);
  }
  internal static void Highlight(Button button,bool value){
   button.transform.Find("PlaytestFill").GetComponent<Image>().color=value?Selected:Purple;
  }
 }
}
