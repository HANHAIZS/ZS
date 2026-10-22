using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class SettingsScreenSetup {
  public const string PreferencesArt="Assets/Sprites/UI/SettingsOka/Preferences20261022.png";
  static readonly string[] PreferenceNames={"Panel","Music","Sound","Haptic"};
  static readonly Rect[] PreferenceRects={new Rect(0,0,659,264),new Rect(75,32,141,141),new Rect(262,32,141,141),new Rect(449,32,141,141)};

  [MenuItem("BeeHarvest/UI/Apply Settings Page Artwork")]
  public static void ApplyArtwork() {
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before updating Settings artwork.");
   var root=PrefabUtility.LoadPrefabContents(MainMenuSetup.PrefabPath);
   try {StylePreferences(root.GetComponent<MainMenuPage>().Tabs.SettingsPage);PrefabUtility.SaveAsPrefabAsset(root,MainMenuSetup.PrefabPath);}
   finally {PrefabUtility.UnloadPrefabContents(root);}
  }
  static Sprite PreferenceSprite(string name) {
   var sprite=AssetDatabase.LoadAllAssetsAtPath(PreferencesArt).OfType<Sprite>().SingleOrDefault(s=>s.name==name);
   if(sprite==null)throw new InvalidOperationException("Settings page Sprite missing: "+name);return sprite;
  }
  static void StylePreferences(SettingsPageView view) {
   var panel=(RectTransform)view.transform.Find("PreferencePanel");
   var art=panel.Find("Artwork").GetComponent<Image>();var sprite=PreferenceSprite("Panel");art.sprite=sprite;
   panel.sizeDelta=new Vector2(panel.sizeDelta.x,panel.sizeDelta.x*sprite.rect.height/sprite.rect.width);
   for(int i=0;i<3;i++) {
    view.PreferenceIcons[i].sprite=PreferenceSprite(PreferenceNames[i+1]);
    var rect=PreferenceRects[i+1];var hit=(RectTransform)view.PreferenceButtons[i].transform;
    hit.anchorMin=new Vector2(rect.xMin/sprite.rect.width,rect.yMin/sprite.rect.height);
    hit.anchorMax=new Vector2(rect.xMax/sprite.rect.width,rect.yMax/sprite.rect.height);
    hit.offsetMin=hit.offsetMax=Vector2.zero;
   }
  }
  public static void ImportPreferenceArt() {
   AssetDatabase.ImportAsset(PreferencesArt,ImportAssetOptions.ForceSynchronousImport);
   var importer=(TextureImporter)AssetImporter.GetAtPath(PreferencesArt);importer.textureType=TextureImporterType.Sprite;
   importer.spriteImportMode=SpriteImportMode.Multiple;importer.mipmapEnabled=false;importer.isReadable=false;
   importer.maxTextureSize=1024;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;
#pragma warning disable 0618
   importer.spritesheet=PreferenceNames.Select((name,i)=>new SpriteMetaData{name=name,rect=PreferenceRects[i],alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f}).ToArray();
#pragma warning restore 0618
   var serialized=new SerializedObject(importer);var entries=serialized.FindProperty("m_SpriteSheet.m_Sprites");var table=serialized.FindProperty("m_SpriteSheet.m_NameFileIdTable");table.arraySize=PreferenceNames.Length;
   for(int i=0;i<PreferenceNames.Length;i++) {
    byte[] hash;using(var md5=System.Security.Cryptography.MD5.Create())hash=md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(PreferencesArt+"/"+PreferenceNames[i]));
    long id=BitConverter.ToInt64(hash,0)&long.MaxValue;if(id==0)id=1;
    var entry=entries.GetArrayElementAtIndex(i);entry.FindPropertyRelative("m_InternalID").longValue=id;entry.FindPropertyRelative("m_SpriteID").stringValue=new Guid(hash).ToString("N");
    var pair=table.GetArrayElementAtIndex(i);pair.FindPropertyRelative("first").stringValue=PreferenceNames[i];pair.FindPropertyRelative("second").longValue=id;
   }
   serialized.ApplyModifiedPropertiesWithoutUndo();importer.SaveAndReimport();
  }
  public static void Build(MainMenuPage page,Transform safe) {
   var old=safe.Find("GalleryContent");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
   var root=OkaSettingsSetup.Rect("GalleryContent",safe);root.SetAsFirstSibling();
   var header=OkaSettingsSetup.Image("SettingsHeader",root,OkaSettingsSetup.Sprite("Header"));
   var h=header.rectTransform;h.anchorMin=new Vector2(0,1);h.anchorMax=Vector2.one;h.pivot=new Vector2(.5f,1);h.sizeDelta=new Vector2(0,225);h.anchoredPosition=Vector2.zero;
   var panel=OkaSettingsSetup.Rect("PreferencePanel",root);panel.anchorMin=panel.anchorMax=new Vector2(.5f,1);panel.pivot=new Vector2(.5f,1);panel.sizeDelta=new Vector2(592,230);panel.anchoredPosition=new Vector2(0,-337);
   var art=OkaSettingsSetup.Image("Artwork",panel,PreferenceSprite("Panel"));
   var view=root.gameObject.AddComponent<SettingsPageView>();view.PreferenceButtons=new Button[3];view.PreferenceIcons=new Image[3];view.OffMarks=new GameObject[3];
   string[] names={"Music","Sound","Haptic"};float[] centers={.21684f,.503f,.78614f};
   for(int i=0;i<3;i++) {
    var b=OkaSettingsSetup.RoundedButton(names[i]+"Toggle",art.transform,PreferenceSprite(names[i]),.24f);
    var r=(RectTransform)b.transform;r.anchorMin=new Vector2(centers[i]-.108f,.104f);r.anchorMax=new Vector2(centers[i]+.108f,.658f);r.offsetMin=r.offsetMax=Vector2.zero;
    view.PreferenceButtons[i]=b;
    if(i<3){view.PreferenceIcons[i]=b.image;view.OffMarks[i]=OkaSettingsSetup.OffMark(b.transform);}
   }
   StylePreferences(view);
   var tabs=page.Tabs;tabs.GalleryContent=root.gameObject;tabs.SettingsPage=view;tabs.GalleryBackground=OkaSettingsSetup.Sprite("Backdrop");
   tabs.NavigationBase=safe.Find("BottomNavigation").GetComponent<Image>();tabs.NavigationInset=safe.Find("HomeSelection/CreamInset").GetComponent<Image>();
   page.GalleryButton.image.sprite=page.SettingsButton.image.sprite;
   safe.Find("GalleryLabel").GetComponent<TMPro.TMP_Text>().text="Settings";
   root.gameObject.SetActive(false);
  }
 }
}
