using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class SettingsPopupStyleSetup {
  public static void Build(SettingsPopupController popup,bool menu) {
   if(!menu){LevelPauseSetup.Build(popup);return;}
   // The menu template is derived from the level popup, but has no Quit flow.
   var leaving=popup.transform.Find("LeavingContent");if(leaving!=null)Object.DestroyImmediate(leaving.gameObject);
   popup.LeavingContent=null;popup.LeaveButton=null;popup.CancelLeaveButton=null;
   var content=(RectTransform)popup.PopupContent.transform;
   // Keep the modal root/dimmer and all external Level/OpenButton references.
   while(content.childCount>0)Object.DestroyImmediate(content.GetChild(0).gameObject);
   var oldImage=content.GetComponent<Image>();if(oldImage!=null)Object.DestroyImmediate(oldImage);
   var oldShape=content.GetComponent<ShopRoundRectMaskGraphic>();if(oldShape!=null)Object.DestroyImmediate(oldShape);
   content.anchorMin=content.anchorMax=Vector2.one*.5f;content.pivot=Vector2.one*.5f;content.sizeDelta=new Vector2(620,590);content.anchoredPosition=new Vector2(0,15);
   Body("OuterRim",content,Vector2.zero,new Color(.04f,.4f,.72f),.12f);
   Body("BlueBody",content,new Vector2(10,10),new Color(0,.28f,1),.12f);
   var title=OkaSettingsSetup.Rect("TitleMask",content);Place(title,new Vector2(390,126),new Vector2(-5,265));OkaSettingsSetup.Mask(title,.25f);
   OkaSettingsSetup.Image("Title",title,OkaSettingsSetup.Sprite("Title",true));
   var closeMask=OkaSettingsSetup.Rect("CloseMask",content);Place(closeMask,new Vector2(84,84),new Vector2(276,250));OkaSettingsSetup.Mask(closeMask,.5f);
   popup.CloseButton=OkaSettingsSetup.Button("CloseButton",closeMask,OkaSettingsSetup.Sprite("Close",true));
   var panel=OkaSettingsSetup.Image("PreferencePanel",content,OkaSettingsSetup.Sprite("Preferences",true));Place(panel.rectTransform,new Vector2(520,223),new Vector2(0,35));
   popup.PreferenceButtons=new Button[3];popup.PreferenceIcons=new Image[3];popup.OffMarks=new GameObject[3];
   string[] names={"Music","Sound","Haptic"};
   for(int i=0;i<3;i++) {
    var b=OkaSettingsSetup.RoundedButton(names[i]+"Toggle",panel.transform,OkaSettingsSetup.Sprite(names[i],true),.22f);
    Place((RectTransform)b.transform,new Vector2(101,102),new Vector2(new[]{-178.86f,-3.55f,174.12f}[i],-32.03f));
    popup.PreferenceButtons[i]=b;popup.PreferenceIcons[i]=b.image;popup.OffMarks[i]=OkaSettingsSetup.OffMark(b.transform);
   }
   popup.RetryButton=null;popup.HomeButton=null;
   if(!menu){popup.HomeButton=OkaSettingsSetup.Button("HomeButton",content,OkaSettingsSetup.Sprite("Quit",true));Place((RectTransform)popup.HomeButton.transform,new Vector2(242,108),new Vector2(-135,-190));}
   popup.ContinueButton=OkaSettingsSetup.Button("ContinueButton",content,OkaSettingsSetup.Sprite("Continue",true));Place((RectTransform)popup.ContinueButton.transform,new Vector2(242,108),new Vector2(menu?0:135,-190));
   popup.DimBackground.SetActive(false);content.gameObject.SetActive(false);
  }
  static void Place(RectTransform r,Vector2 size,Vector2 pos){r.anchorMin=r.anchorMax=Vector2.one*.5f;r.sizeDelta=size;r.anchoredPosition=pos;}
  static GameObject Body(string name,Transform parent,Vector2 inset,Color color,float radius) {
   var r=OkaSettingsSetup.Rect(name,parent);r.offsetMin=inset;r.offsetMax=-inset;var shape=r.gameObject.AddComponent<ShopRoundRectMaskGraphic>();shape.RadiusFraction=radius;shape.color=color;shape.raycastTarget=true;return r.gameObject;
  }
 }
}
