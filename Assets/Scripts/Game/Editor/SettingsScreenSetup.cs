using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class SettingsScreenSetup {
  public static void Build(MainMenuPage page,Transform safe) {
   var old=safe.Find("GalleryContent");if(old!=null)Object.DestroyImmediate(old.gameObject);
   var root=OkaSettingsSetup.Rect("GalleryContent",safe);root.SetAsFirstSibling();
   var header=OkaSettingsSetup.Image("SettingsHeader",root,OkaSettingsSetup.Sprite("Header"));
   var h=header.rectTransform;h.anchorMin=new Vector2(0,1);h.anchorMax=Vector2.one;h.pivot=new Vector2(.5f,1);h.sizeDelta=new Vector2(0,225);h.anchoredPosition=Vector2.zero;
   var panel=OkaSettingsSetup.Rect("PreferencePanel",root);panel.anchorMin=panel.anchorMax=new Vector2(.5f,1);panel.pivot=new Vector2(.5f,1);panel.sizeDelta=new Vector2(592,230);panel.anchoredPosition=new Vector2(0,-337);
   var art=OkaSettingsSetup.Image("Artwork",panel,OkaSettingsSetup.Sprite("Panel"));
   var view=root.gameObject.AddComponent<SettingsPageView>();view.PreferenceButtons=new Button[3];view.PreferenceIcons=new Image[3];view.OffMarks=new GameObject[3];
   string[] names={"Music","Sound","Haptic"};float[] centers={.21684f,.503f,.78614f};
   for(int i=0;i<3;i++) {
    var b=OkaSettingsSetup.RoundedButton(names[i]+"Toggle",art.transform,OkaSettingsSetup.Sprite(names[i]),.24f);
    var r=(RectTransform)b.transform;r.anchorMin=new Vector2(centers[i]-.108f,.104f);r.anchorMax=new Vector2(centers[i]+.108f,.658f);r.offsetMin=r.offsetMax=Vector2.zero;
    view.PreferenceButtons[i]=b;
    if(i<3){view.PreferenceIcons[i]=b.image;view.OffMarks[i]=OkaSettingsSetup.OffMark(b.transform);}
   }
   var tabs=page.Tabs;tabs.GalleryContent=root.gameObject;tabs.SettingsPage=view;tabs.GalleryBackground=OkaSettingsSetup.Sprite("Backdrop");
   tabs.NavigationBase=safe.Find("BottomNavigation").GetComponent<Image>();tabs.NavigationInset=safe.Find("HomeSelection/CreamInset").GetComponent<Image>();
   page.GalleryButton.image.sprite=page.SettingsButton.image.sprite;
   safe.Find("GalleryLabel").GetComponent<TMPro.TMP_Text>().text="Settings";
   root.gameObject.SetActive(false);
  }
 }
}
