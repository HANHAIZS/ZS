using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest.Editor {
 public static class MainMenuNavigationBarSetup {
  const string Art="Assets/Sprites/UI/NavigationBar/";
  const string Prefab="Assets/Resources/Prefabs/UI/MainMenuPage.prefab";
  [MenuItem("BeeHarvest/UI/Apply Purple Navigation Bar")]
  public static void Apply() {
   if(Application.isPlaying||EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
   ImportArt();var root=PrefabUtility.LoadPrefabContents(Prefab);
   try{Build(root.GetComponent<MainMenuPage>());PrefabUtility.SaveAsPrefabAsset(root,Prefab);}
   finally{PrefabUtility.UnloadPrefabContents(root);}
   AssetDatabase.SaveAssets();
   var profile=AssetDatabase.LoadAssetAtPath<GameFoundation.UI.UiPrefabConventionProfile>("Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset");
   var report=GameFoundation.UI.Editor.UiPrefabValidator.Validate(Prefab,profile,GameFoundation.UI.UiPrefabKind.Page);
   if(!report.IsValid)throw new InvalidOperationException(report.ToMultilineString());
  }
  public static void Build(MainMenuPage page) {
   var safe=page.transform.Find("Content/SafeArea");var tabs=page.Tabs;
   var view=page.GetComponent<MainMenuNavigationBarView>();if(view==null)view=page.gameObject.AddComponent<MainMenuNavigationBarView>();tabs.NavigationBar=view;
   var back=safe.Find("BottomNavigation").GetComponent<Image>();
   var b=back.rectTransform;b.anchorMin=Vector2.zero;b.anchorMax=new Vector2(1,0);b.pivot=new Vector2(.5f,0);b.sizeDelta=new Vector2(0,147);b.anchoredPosition=Vector2.zero;
   back.sprite=Sprite("Bar","Idle");back.color=Color.white;back.raycastTarget=false;back.type=Image.Type.Simple;
   while(b.childCount>0)UnityEngine.Object.DestroyImmediate(b.GetChild(0).gameObject);
   var flat=OkaSettingsSetup.Image("GoldRim",b,Sprite("Rim","Flat"));
   var fr=flat.rectTransform;fr.anchorMin=new Vector2(0,1);fr.anchorMax=Vector2.one;fr.pivot=new Vector2(.5f,1);fr.sizeDelta=new Vector2(0,19.3f);fr.anchoredPosition=new Vector2(0,2);
   for(int i=1;i<3;i++) {
    var divider=OkaSettingsSetup.Image("Divider"+i,b,null);divider.color=new Color(.20f,.10f,.55f,.8f);
    var d=divider.rectTransform;d.anchorMin=new Vector2(i/3f,0);d.anchorMax=new Vector2(i/3f,1);d.sizeDelta=new Vector2(2,0);d.anchoredPosition=Vector2.zero;
   }
   var selection=safe.Find("HomeSelection").GetComponent<Image>();selection.sprite=Sprite("Bar","Selected");selection.type=Image.Type.Simple;selection.color=Color.white;selection.raycastTarget=false;
   var inset=selection.transform.Find("CreamInset").GetComponent<Image>();inset.enabled=false;
   foreach(string name in new[]{"SelectedGoldRim","RimLeft","RimCenter","RimRight"}){var old=selection.transform.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);}
   var left=OkaSettingsSetup.Image("RimLeft",selection.transform,Sprite("Rim","Curve"));
   var l=left.rectTransform;l.anchorMin=l.anchorMax=new Vector2(0,1);l.pivot=new Vector2(0,1);l.sizeDelta=new Vector2(52.6f,55.3f);l.anchoredPosition=new Vector2(39.5f,0);l.localScale=new Vector3(-1,1,1);
   var right=OkaSettingsSetup.Image("RimRight",selection.transform,Sprite("Rim","Curve"));
   var r=right.rectTransform;r.anchorMin=r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(1,1);r.sizeDelta=new Vector2(52.6f,55.3f);r.anchoredPosition=new Vector2(13.1f,0);
   var center=OkaSettingsSetup.Image("RimCenter",selection.transform,Sprite("Rim","Top"));
   var c=center.rectTransform;c.anchorMin=new Vector2(0,1);c.anchorMax=Vector2.one;c.pivot=new Vector2(.5f,1);c.offsetMin=new Vector2(39.5f,-12.3f);c.offsetMax=new Vector2(-39.5f,0);
   view.Selection=selection.rectTransform;view.Selection.pivot=new Vector2(.5f,0);
   view.Icons=new RectTransform[3];view.Labels=new TMPro.TMP_Text[3];
   var buttons=new[]{page.ShopButton,page.HomeButton,page.GalleryButton};var names=new[]{"Shop","Home","Setting"};var labels=new[]{"ShopLabel","HomeLabel","GalleryLabel"};
   for(int i=0;i<3;i++) {
    var button=buttons[i];var br=(RectTransform)button.transform;
    br.anchorMin=new Vector2(i/3f,0);br.anchorMax=new Vector2((i+1)/3f,0);br.pivot=new Vector2(.5f,0);br.offsetMin=Vector2.zero;br.offsetMax=new Vector2(0,200);
    var hit=button.GetComponent<Image>();hit.sprite=null;hit.color=Color.clear;hit.raycastTarget=true;hit.type=Image.Type.Simple;
    var oldIcon=br.Find("Icon");if(oldIcon!=null)UnityEngine.Object.DestroyImmediate(oldIcon.gameObject);
    var icon=OkaSettingsSetup.Image("Icon",br,Sprite(names[i],names[i]));icon.preserveAspect=true;
    var ir=icon.rectTransform;ir.anchorMin=ir.anchorMax=new Vector2(.5f,0);ir.pivot=Vector2.one*.5f;
    view.Icons[i]=ir;button.targetGraphic=icon;
    var colors=button.colors;colors.normalColor=colors.highlightedColor=colors.selectedColor=Color.white;colors.pressedColor=new Color(.82f,.82f,.82f);colors.disabledColor=Color.white;button.colors=colors;
    var text=safe.Find(labels[i]).GetComponent<TMPro.TMP_Text>();text.text=names[i];text.fontSize=43;text.raycastTarget=false;
    var tr=text.rectTransform;tr.anchorMin=tr.anchorMax=new Vector2((i+.5f)/3f,0);tr.sizeDelta=new Vector2(235,56);tr.anchoredPosition=new Vector2(0,30);
    StyleLabel(text,text.font);view.Labels[i]=text;
   }
   tabs.NavigationSelection=view.Selection;tabs.NavigationBase=back;tabs.NavigationInset=inset;
   view.ValidateBindings();view.Render(MainMenuTab.Home,true);
  }
  public static void StyleLabel(TMPro.TMP_Text text,TMPro.TMP_FontAsset font) {
   text.font=font;text.color=new Color(1,.985f,.96f);text.fontSharedMaterial=LabelMaterial(font);text.UpdateMeshPadding();
  }
  static Material LabelMaterial(TMPro.TMP_FontAsset font) {
   const string path=Art+"NavigationLabel.mat";
   var material=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(material==null){material=new Material(font.material);material.name="NavigationLabel";AssetDatabase.CreateAsset(material,path);}
   material.SetColor("_FaceColor",Color.white);material.SetColor("_OutlineColor",new Color(.36f,.20f,.72f));material.SetFloat("_OutlineWidth",.2f);material.EnableKeyword("OUTLINE_ON");EditorUtility.SetDirty(material);return material;
  }
  static Sprite Sprite(string file,string name){return AssetDatabase.LoadAllAssetsAtPath(Art+file+".png").OfType<Sprite>().Single(s=>s.name==name);}
  static SpriteMetaData Slice(string name,float x,float y,float w,float h){return new SpriteMetaData{name=name,rect=new Rect(x,y,w,h),alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f};}
  static void ImportArt() {
   Import("Bar",new[]{Slice("Idle",565,0,274,167),Slice("Selected",0,0,285,209)});
   Import("Rim",new[]{Slice("Flat",300,0,540,22),Slice("Selected",0,0,300,63),Slice("Curve",240,0,60,63),Slice("Top",60,49,180,14)});
   foreach(string name in new[]{"Shop","Home","Setting"}) {
    var bytes=System.IO.File.ReadAllBytes(Art+name+".png");var texture=new Texture2D(2,2);texture.LoadImage(bytes);
    var slice=Slice(name,0,0,texture.width,texture.height);UnityEngine.Object.DestroyImmediate(texture);Import(name,new[]{slice});
   }
  }
  static void Import(string file,SpriteMetaData[] slices) {
   string path=Art+file+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);var t=(TextureImporter)AssetImporter.GetAtPath(path);
   t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Multiple;t.mipmapEnabled=false;t.isReadable=false;t.maxTextureSize=2048;t.wrapMode=TextureWrapMode.Clamp;t.alphaIsTransparency=true;t.textureCompression=TextureImporterCompression.Uncompressed;
#pragma warning disable 0618
   t.spritesheet=slices;
#pragma warning restore 0618
   // Unity 2022 can leave newly added legacy slices with fileID zero. Assign stable native importer IDs before saving.
   var so=new SerializedObject(t);var entries=so.FindProperty("m_SpriteSheet.m_Sprites");var table=so.FindProperty("m_SpriteSheet.m_NameFileIdTable");table.arraySize=slices.Length;
   for(int i=0;i<slices.Length;i++) {
    byte[] hash;using(var md5=System.Security.Cryptography.MD5.Create())hash=md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(path+"/"+slices[i].name));
    long id=BitConverter.ToInt64(hash,0)&long.MaxValue;if(id==0)id=1;
    var entry=entries.GetArrayElementAtIndex(i);entry.FindPropertyRelative("m_InternalID").longValue=id;entry.FindPropertyRelative("m_SpriteID").stringValue=new Guid(hash).ToString("N");
    var pair=table.GetArrayElementAtIndex(i);pair.FindPropertyRelative("first").stringValue=slices[i].name;pair.FindPropertyRelative("second").longValue=id;
   }
   so.ApplyModifiedPropertiesWithoutUndo();t.SaveAndReimport();
  }
 }
}
