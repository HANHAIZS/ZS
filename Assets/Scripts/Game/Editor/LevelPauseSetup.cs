using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class LevelPauseSetup {
  public const string ArtRoot="Assets/Sprites/UI/PauseDialog/";
  const float Scale=740f/758f;
  const string FontPath=ArtRoot+"PausedTitleFont.asset";
  [MenuItem("BeeHarvest/UI/Apply Level Paused Dialog")]
  public static void Apply() {
   if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
   for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save the current scene first.");
   ImportArt();TitleFont();
   var root=PrefabUtility.LoadPrefabContents(LevelOneSettingsSetup.PrefabPath);
   try{Build(root.GetComponent<SettingsPopupController>());PrefabUtility.SaveAsPrefabAsset(root,LevelOneSettingsSetup.PrefabPath);}
   finally{PrefabUtility.UnloadPrefabContents(root);}
   var original=EditorSceneManager.GetSceneManagerSetup();
   try {
    foreach(string path in Enumerable.Range(1,LevelOneAssets.LastLevel).Select(n=>"Assets/Scenes/"+LevelOneAssets.SceneName(n)+".unity")) {
     var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
     var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();
     var popup=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SettingsPopupController>(true)).Single(p=>p.Level==level);
     if(popup.RetryButton==null||popup.ContinueButton!=null||popup.OpenButton==null||popup.CloseButton==null||popup.HomeButton==null)throw new InvalidOperationException("Paused dialog bindings invalid: "+path);
     foreach(var victory in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<VictoryPopupController>(true)).Where(v=>v.Level==level)) {
      if(victory.SettingsPopup!=popup)throw new InvalidOperationException("Victory settings binding changed: "+path);
     }
     EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
   }finally{EditorSceneManager.RestoreSceneManagerSetup(original);}
   AssetDatabase.SaveAssets();
   var profile=AssetDatabase.LoadAssetAtPath<GameFoundation.UI.UiPrefabConventionProfile>("Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset");
   var report=GameFoundation.UI.Editor.UiPrefabValidator.Validate(LevelOneSettingsSetup.PrefabPath,profile,GameFoundation.UI.UiPrefabKind.Popup);
   if(!report.IsValid)throw new InvalidOperationException(report.ToMultilineString());
   Debug.Log("LEVEL_PAUSED_DIALOG_READY");
  }
  public static void Build(SettingsPopupController popup) {
   if(AssetDatabase.LoadAllAssetsAtPath(ArtRoot+"Preferences.png").OfType<Sprite>().Count()!=4)ImportArt();
   var content=(RectTransform)popup.PopupContent.transform;
   while(content.childCount>0)UnityEngine.Object.DestroyImmediate(content.GetChild(0).gameObject);
   var oldImage=content.GetComponent<Image>();if(oldImage!=null)UnityEngine.Object.DestroyImmediate(oldImage);
   var oldShape=content.GetComponent<ShopRoundRectMaskGraphic>();if(oldShape!=null)UnityEngine.Object.DestroyImmediate(oldShape);
   content.anchorMin=content.anchorMax=Vector2.one*.5f;content.pivot=Vector2.one*.5f;content.sizeDelta=new Vector2(740,990*Scale);content.anchoredPosition=Vector2.zero;content.localScale=Vector3.one;
   if(content.GetComponent<PausedDialogFit>()==null)content.gameObject.AddComponent<PausedDialogFit>();
   var background=OkaSettingsSetup.Image("PanelBackground",content,Art("Background"));background.raycastTarget=true;
   Place(background.rectTransform,new Vector2(723,959),new Vector2(-7.5f,-24.5f));
   var header=OkaSettingsSetup.Image("TitleBanner",content,Art("Header"));Place(header.rectTransform,new Vector2(482,170),new Vector2(-6,410));
   var label=OkaSettingsSetup.Rect("Title",header.transform).gameObject.AddComponent<TextMeshProUGUI>();
   label.font=TitleFont();label.fontSharedMaterial=label.font.material;label.text="Paused";label.fontSize=90*Scale;label.color=new Color(1,.985f,.96f);label.alignment=TextAlignmentOptions.Center;label.enableWordWrapping=false;label.raycastTarget=false;
   label.rectTransform.offsetMin=new Vector2(25,34)*Scale;label.rectTransform.offsetMax=new Vector2(-25,-12)*Scale;label.UpdateMeshPadding();
   popup.CloseButton=OkaSettingsSetup.Button("CloseButton",content,Art("Close"));Place((RectTransform)popup.CloseButton.transform,new Vector2(119,142),new Vector2(310.5f,376));
   var panel=OkaSettingsSetup.Image("PreferencePanel",content,Art("PreferencesPanel","Preferences"));Place(panel.rectTransform,new Vector2(602,262),new Vector2(-7,170));
   popup.PreferenceButtons=new Button[3];popup.PreferenceIcons=new Image[3];popup.OffMarks=new GameObject[3];
   string[] names={"Music","Sound","Haptic"};float[] x={-185,3,190};
   for(int i=0;i<3;i++) {
    var b=OkaSettingsSetup.RoundedButton(names[i]+"Toggle",panel.transform,Art(names[i],"Preferences"),.24f);
    Place((RectTransform)b.transform,new Vector2(140,140),new Vector2(x[i],-31));
    popup.PreferenceButtons[i]=b;popup.PreferenceIcons[i]=b.image;popup.OffMarks[i]=OkaSettingsSetup.OffMark(b.transform);
   }
   popup.HomeButton=OkaSettingsSetup.Button("HomeButton",content,Art("Quit"));Place((RectTransform)popup.HomeButton.transform,new Vector2(484,180),new Vector2(-7,-130));
   popup.RetryButton=OkaSettingsSetup.Button("RetryButton",content,Art("Restart"));Place((RectTransform)popup.RetryButton.transform,new Vector2(485,180),new Vector2(-5.5f,-329));
   var colors=popup.RetryButton.colors;colors.disabledColor=new Color(.55f,.55f,.55f);popup.RetryButton.colors=colors;
   popup.ContinueButton=null;popup.DimBackground.SetActive(false);content.gameObject.SetActive(false);
   LeavingDialogSetup.Build(popup);
  }
  static void Place(RectTransform r,Vector2 size,Vector2 pos){r.anchorMin=r.anchorMax=Vector2.one*.5f;r.sizeDelta=size*Scale;r.anchoredPosition=pos*Scale;}
  static Sprite Art(string name,string file=null){return AssetDatabase.LoadAllAssetsAtPath(ArtRoot+(file??name)+".png").OfType<Sprite>().Single(s=>s.name==name);}
  public static void ImportArt() {
   foreach(var name in new[]{"Background","Header","Close","Quit","Restart"}) {
    var t=Importer(name);t.spriteImportMode=SpriteImportMode.Single;t.maxTextureSize=name=="Background"?1024:name=="Close"?256:512;t.SaveAndReimport();
   }
   var panel=Importer("Preferences");panel.spriteImportMode=SpriteImportMode.Multiple;panel.maxTextureSize=1024;
#pragma warning disable 0618
   panel.spritesheet=new[]{Slice("PreferencesPanel",0,0,602,262),Slice("Music",46,30,140,140),Slice("Sound",234,30,140,140),Slice("Haptic",421,30,140,140)};
#pragma warning restore 0618
   panel.SaveAndReimport();
  }
  static TextureImporter Importer(string name) {
   string path=ArtRoot+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
   var t=AssetImporter.GetAtPath(path) as TextureImporter;if(t==null)throw new InvalidOperationException("Pause art missing: "+path);
   t.textureType=TextureImporterType.Sprite;t.mipmapEnabled=false;t.isReadable=false;t.alphaIsTransparency=true;t.wrapMode=TextureWrapMode.Clamp;t.textureCompression=TextureImporterCompression.CompressedHQ;return t;
  }
  static SpriteMetaData Slice(string name,float x,float y,float w,float h){return new SpriteMetaData{name=name,rect=new Rect(x,y,w,h),alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f};}
  static TMP_FontAsset TitleFont() {
   var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
   if(font==null) {
    var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/Sprites/UI/Fonts/Fredoka-Bold.ttf");
    font=TMP_FontAsset.CreateFontAsset(source,90,12,GlyphRenderMode.SDFAA,512,512,AtlasPopulationMode.Dynamic,false);
    if(font==null||!font.TryAddCharacters("Paused _"))throw new InvalidOperationException("Paused title glyph bake failed.");
    font.name="PausedTitleFont";font.atlasPopulationMode=AtlasPopulationMode.Static;AssetDatabase.CreateAsset(font,FontPath);
    font.material.name="PausedTitleFont Material";AssetDatabase.AddObjectToAsset(font.material,font);
    foreach(var atlas in font.atlasTextures){atlas.name="PausedTitleFont Atlas";AssetDatabase.AddObjectToAsset(atlas,font);}
   }
   font.material.SetColor("_FaceColor",Color.white);font.material.SetColor("_OutlineColor",new Color(.25f,.005f,.015f));font.material.SetFloat("_OutlineWidth",.16f);font.material.EnableKeyword("OUTLINE_ON");
   EditorUtility.SetDirty(font);EditorUtility.SetDirty(font.material);return font;
  }
 }
}
