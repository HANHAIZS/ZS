using System;
using System.Linq;
using GameFoundation.UI;
using GameFoundation.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class MainMenuSetup {
  public const string PrefabPath="Assets/Resources/Prefabs/UI/MainMenuPage.prefab";
  public const string SettingsPrefabPath="Assets/Sprites/Prefabs/UI/MainMenuSettingsPopup.prefab";
  public const string ArtRoot="Assets/Sprites/UI/MainMenu/";
  const string LevelPath="Assets/Scenes/Level01.unity",MenuPath="Assets/Scenes/MainMenu.unity";
  static Color cream=new Color(1,.965f,.898f),gold=new Color(1,.71f,.10f);
  public static void Apply(){
   if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode before menu setup.");
   Ensure("Assets/Resources/Prefabs/UI");Ensure("Assets/Sprites/UI/MainMenu");
   var background=Background();var icons=new[]{Icon("ShopIcon"),Icon("HomeIcon"),Icon("GalleryIcon")};
   var retry=AssetDatabase.LoadAllAssetsAtPath(LevelOneSettingsSetup.ArtRoot+"RetryButton.png").OfType<Sprite>().Single();
   var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LevelOneSettingsSetup.FontPath);
   string missing=new string("start gameShopHomeGallery".Distinct().Where(c=>!font.characterLookupTable.ContainsKey(c)).ToArray());
   try{
    if(missing.Length>0){font.atlasPopulationMode=AtlasPopulationMode.Dynamic;
     if(!font.TryAddCharacters(missing))throw new InvalidOperationException("Menu font atlas has missing glyphs.");}
   }finally{font.atlasPopulationMode=AtlasPopulationMode.Static;EditorUtility.SetDirty(font);}
   var root=Rect("MainMenuPage",null);Stretch(root);var page=root.gameObject.AddComponent<MainMenuPage>();
   var content=Rect("Content",root);Stretch(content);
   var bg=Rect("Background",content);Stretch(bg);var bgImage=bg.gameObject.AddComponent<Image>();bgImage.sprite=background;
   var cover=bg.gameObject.AddComponent<AspectRatioFitter>();cover.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;cover.aspectRatio=background.rect.width/background.rect.height;
   var safe=Rect("SafeArea",content);Stretch(safe);safe.gameObject.AddComponent<UiSafeAreaFitter>();
   var nav=Rect("BottomNavigation",safe);nav.anchorMin=Vector2.zero;nav.anchorMax=new Vector2(1,0);nav.pivot=new Vector2(.5f,0);nav.sizeDelta=new Vector2(0,145);
   nav.gameObject.AddComponent<Image>().color=new Color(1,.91f,.67f);
   var edge=Rect("GoldRim",nav);edge.anchorMin=new Vector2(0,1);edge.anchorMax=Vector2.one;edge.sizeDelta=new Vector2(0,7);edge.gameObject.AddComponent<Image>().color=gold;
   var highlight=Rect("HomeSelection",safe);highlight.anchorMin=highlight.anchorMax=new Vector2(.5f,0);highlight.pivot=new Vector2(.5f,0);highlight.sizeDelta=new Vector2(234,185);
   var selection=highlight.gameObject.AddComponent<Image>();selection.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");selection.type=Image.Type.Sliced;selection.color=gold;selection.pixelsPerUnitMultiplier=.3f;selection.raycastTarget=false;
   var inset=Rect("CreamInset",highlight);Stretch(inset);inset.offsetMin=new Vector2(5,5);inset.offsetMax=new Vector2(-5,-5);var insetImage=inset.gameObject.AddComponent<Image>();insetImage.sprite=selection.sprite;insetImage.type=Image.Type.Sliced;insetImage.pixelsPerUnitMultiplier=.3f;insetImage.color=cream;insetImage.raycastTarget=false;
   var controls=new Button[3];string[] names={"Shop","Home","Gallery"};
   for(int i=0;i<3;i++){
    var b=Button(safe,names[i]+"Tab",icons[i],new Vector2(136,132));var r=(RectTransform)b.transform;
    r.anchorMin=r.anchorMax=new Vector2((i+.5f)/3,0);r.anchoredPosition=new Vector2(0,i==1?112:96);
    var colors=b.colors;colors.disabledColor=Color.white;b.colors=colors;b.interactable=false;
    Label(safe,names[i]+"Label",names[i],font,new Vector2(180,48),new Vector2((i+.5f)/3,0),new Vector2(0,27),31);
    controls[i]=b;
   }
   page.ShopButton=controls[0];page.HomeButton=controls[1];page.GalleryButton=controls[2];
   page.StartGameButton=Button(safe,"StartGameButton",retry,new Vector2(384,138));
   var startRect=(RectTransform)page.StartGameButton.transform;startRect.anchorMin=startRect.anchorMax=new Vector2(.5f,.27f);
   Label(startRect,"Label","start game",font,new Vector2(344,102),Vector2.one*.5f,Vector2.zero,48);
   var gear=AssetDatabase.LoadAllAssetsAtPath(LevelOneSettingsSetup.ArtRoot+"SettingsGear.png").OfType<Sprite>().Single();
   page.SettingsButton=Button(safe,"SettingsButton",gear,new Vector2(80,80));
   var gearRect=(RectTransform)page.SettingsButton.transform;gearRect.anchorMin=gearRect.anchorMax=Vector2.one;gearRect.anchoredPosition=new Vector2(-60,-60);
   EconomyHeaderSetup.Build(page,safe,font);
   var settingsObject=(GameObject)PrefabUtility.InstantiatePrefab(MenuSettingsPrefab(),safe);
   Stretch((RectTransform)settingsObject.transform);settingsObject.transform.SetAsLastSibling();
   page.SettingsPopup=settingsObject.GetComponent<SettingsPopupController>();
   page.SettingsPopup.MenuPage=page;page.SettingsPopup.OpenButton=page.SettingsButton;
   PrefabUtility.RecordPrefabInstancePropertyModifications(page.SettingsPopup);
   PrefabUtility.SaveAsPrefabAsset(root.gameObject,PrefabPath);UnityEngine.Object.DestroyImmediate(root.gameObject);
   MainMenuTabsSetup.Apply();
   var profile=AssetDatabase.LoadAssetAtPath<UiPrefabConventionProfile>("Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset");
   var report=UiPrefabValidator.Validate(PrefabPath,profile,UiPrefabKind.Page);
   if(!report.IsValid)throw new InvalidOperationException(report.ToMultilineString());
   var scene=EditorSceneManager.OpenScene(LevelPath,OpenSceneMode.Single);
   var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();
   var navigation=level.GetComponent<MainMenuNavigationController>();if(navigation==null)navigation=level.gameObject.AddComponent<MainMenuNavigationController>();
   navigation.Level=level;navigation.Canvas=level.Hud.GetComponentInParent<Canvas>();navigation.LevelCamera=Camera.main;
   EditorUtility.SetDirty(navigation);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   var popup=PrefabUtility.LoadPrefabContents(LevelOneSettingsSetup.PrefabPath);
   try{popup.GetComponent<SettingsPopupController>().HomeButton.interactable=true;PrefabUtility.SaveAsPrefabAsset(popup,LevelOneSettingsSetup.PrefabPath);}finally{PrefabUtility.UnloadPrefabContents(popup);}
   var menu=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);camera.GetComponent<Camera>().orthographic=true;
   var light=new GameObject("Main Light",typeof(Light));light.GetComponent<Light>().type=LightType.Directional;light.transform.rotation=Quaternion.Euler(45,-30,0);
   var canvasRoot=new GameObject("MainMenuCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=canvasRoot.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
   var scaler=canvasRoot.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(750,1334);scaler.matchWidthOrHeight=0;
   new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
   var navObject=new GameObject("MainMenuNavigation",typeof(MainMenuNavigationController));navObject.GetComponent<MainMenuNavigationController>().Canvas=canvas;
   EditorSceneManager.SaveScene(menu,MenuPath);
   var old=EditorBuildSettings.scenes.Where(s=>s.path!=MenuPath&&s.path!=LevelPath).ToList();old.Insert(0,new EditorBuildSettingsScene(LevelPath,true));old.Insert(0,new EditorBuildSettingsScene(MenuPath,true));EditorBuildSettings.scenes=old.ToArray();
   AssetDatabase.SaveAssets();Debug.Log("MAIN_MENU_READY: Page validation passed; MainMenu is launch scene.");
  }
  static GameObject MenuSettingsPrefab(){
   var root=PrefabUtility.LoadPrefabContents(LevelOneSettingsSetup.PrefabPath);
   try{
    root.name="MainMenuSettingsPopup";var popup=root.GetComponent<SettingsPopupController>();
    popup.Level=null;popup.MenuPage=null;popup.OpenButton=null;
    SettingsPopupStyleSetup.Build(popup,true);
    PrefabUtility.SaveAsPrefabAsset(root,SettingsPrefabPath);
   }finally{PrefabUtility.UnloadPrefabContents(root);}
   var profile=AssetDatabase.LoadAssetAtPath<UiPrefabConventionProfile>("Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset");
   var report=UiPrefabValidator.Validate(SettingsPrefabPath,profile,UiPrefabKind.Popup);
   if(!report.IsValid)throw new InvalidOperationException(report.ToMultilineString());
   return AssetDatabase.LoadAssetAtPath<GameObject>(SettingsPrefabPath);
  }
  static Sprite Background(){
   var p=ArtRoot+"Background.png";var t=(TextureImporter)AssetImporter.GetAtPath(p);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.mipmapEnabled=false;t.maxTextureSize=2048;t.wrapMode=TextureWrapMode.Clamp;t.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(p);
  }
  static Sprite Icon(string name){
   var path=ArtRoot+name+".png";var t=(TextureImporter)AssetImporter.GetAtPath(path);
   t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.mipmapEnabled=false;t.wrapMode=TextureWrapMode.Clamp;t.alphaIsTransparency=true;t.isReadable=true;t.maxTextureSize=8192;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=texture.GetPixels32();int x0=texture.width,y0=texture.height,x1=0,y1=0;
   for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>64){x0=Mathf.Min(x0,x);y0=Mathf.Min(y0,y);x1=Mathf.Max(x1,x);y1=Mathf.Max(y1,y);}
   int pad=24;x0=Mathf.Max(0,x0-pad);y0=Mathf.Max(0,y0-pad);x1=Mathf.Min(texture.width,x1+pad+1);y1=Mathf.Min(texture.height,y1+pad+1);
   t.spriteImportMode=SpriteImportMode.Multiple;
#pragma warning disable 0618
   t.spritesheet=new[]{new SpriteMetaData{name=name,rect=new Rect(x0,y0,x1-x0,y1-y0),alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f}};
#pragma warning restore 0618
   t.isReadable=false;t.maxTextureSize=512;t.SaveAndReimport();return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single();
  }
  static RectTransform Rect(string name,Transform parent){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=Vector2.one*.5f;r.sizeDelta=Vector2.zero;return r;}
  static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
  static Button Button(Transform parent,string name,Sprite sprite,Vector2 size){var r=Rect(name,parent);r.sizeDelta=size;var image=r.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;var b=r.gameObject.AddComponent<Button>();b.targetGraphic=image;b.navigation=new Navigation{mode=Navigation.Mode.None};var c=b.colors;c.pressedColor=new Color(.82f,.82f,.82f);b.colors=c;return b;}
  static void Label(Transform parent,string name,string text,TMP_FontAsset font,Vector2 size,Vector2 anchor,Vector2 position,float point){var r=Rect(name,parent);r.sizeDelta=size;r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=position;var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSharedMaterial=font.material;t.text=text;t.fontSize=point;t.color=cream;t.alignment=TextAlignmentOptions.Center;t.enableWordWrapping=false;t.raycastTarget=false;t.UpdateMeshPadding();}
  static void Ensure(string p){if(AssetDatabase.IsValidFolder(p))return;int i=p.LastIndexOf('/');Ensure(p.Substring(0,i));AssetDatabase.CreateFolder(p.Substring(0,i),p.Substring(i+1));}
 }
}
