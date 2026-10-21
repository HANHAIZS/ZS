using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest.Editor {
 public static class OkaSettingsSetup {
  public const string ArtRoot="Assets/Sprites/UI/SettingsOka/";
  [MenuItem("BeeHarvest/UI/Apply Oka Settings")]
  public static void Apply() {
   if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
   for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save the current scene before applying settings art.");
   ImportArt();
   var original=EditorSceneManager.GetSceneManagerSetup();
   ApplyPrefab(LevelOneSettingsSetup.PrefabPath,false);
   ApplyPrefab(MainMenuSetup.SettingsPrefabPath,true);
   MainMenuTabsSetup.Apply();
   try {
    foreach(string path in Enumerable.Range(1,LevelOneAssets.LastLevel).Select(n=>"Assets/Scenes/"+LevelOneAssets.SceneName(n)+".unity")) {
     var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
     var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();
     var popup=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SettingsPopupController>(true)).Single(p=>p.Level==level);
     var parent=popup.transform.parent;int sibling=popup.transform.GetSiblingIndex();var open=popup.OpenButton;
     UnityEngine.Object.DestroyImmediate(popup.gameObject);
     var replacement=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(LevelOneSettingsSetup.PrefabPath),parent);
     replacement.transform.SetSiblingIndex(sibling);var view=replacement.GetComponent<SettingsPopupController>();view.Level=level;view.OpenButton=open;
     PrefabUtility.RecordPrefabInstancePropertyModifications(view);
     foreach(var victory in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<VictoryPopupController>(true)).Where(v=>v.Level==level)) {
      victory.SettingsPopup=view;EditorUtility.SetDirty(victory);PrefabUtility.RecordPrefabInstancePropertyModifications(victory);
     }
     EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
   } finally {EditorSceneManager.RestoreSceneManagerSetup(original);}
   AssetDatabase.SaveAssets();Debug.Log("OKA_SETTINGS_APPLIED");
  }
  static void ApplyPrefab(string path,bool menu) {
   var root=PrefabUtility.LoadPrefabContents(path);
   try {SettingsPopupStyleSetup.Build(root.GetComponent<SettingsPopupController>(),menu);PrefabUtility.SaveAsPrefabAsset(root,path);}
   finally{PrefabUtility.UnloadPrefabContents(root);}
  }
  public static Sprite Sprite(string name,bool popup=false) {
   string path=ArtRoot+(popup?"SettingsPopupReference.png":"SettingsScreen.png");
   return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single(s=>s.name==name);
  }
  public static void ImportArt() {
   Import("SettingsScreen.png",2048,new[]{
    Slice("Backdrop",0,100,849,1000),Slice("Header",0,1598,849,255),Slice("Panel",85,1212,671,260),
    Slice("Music",158,1239,145,144),Slice("Sound",350,1239,145,144),Slice("Haptic",540,1239,145,144)});
   Import("SettingsPopupReference.png",1024,new[]{
    Slice("Title",99,448,355,115),Slice("Close",470,454,76,76),Slice("Preferences",57,215,439,188),
    Slice("Music",83,239,85,86),Slice("Sound",231,239,85,86),Slice("Haptic",381,239,85,86),Slice("Quit",51,74,213,95),Slice("Continue",286,74,212,95)});
  }
  static SpriteMetaData Slice(string name,float x,float y,float w,float h){return new SpriteMetaData{name=name,rect=new Rect(x,y,w,h),alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f};}
  static void Import(string file,int maximum,SpriteMetaData[] slices) {
   string path=ArtRoot+file;AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
   var t=(TextureImporter)AssetImporter.GetAtPath(path);
   t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Multiple;t.mipmapEnabled=false;t.isReadable=false;
   t.maxTextureSize=maximum;t.wrapMode=TextureWrapMode.Clamp;t.alphaIsTransparency=true;t.textureCompression=TextureImporterCompression.CompressedHQ;
#pragma warning disable 0618
   t.spritesheet=slices;
#pragma warning restore 0618
   t.SaveAndReimport();
  }
  public static RectTransform Rect(string name,Transform parent) {
   var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);Stretch(r);return r;
  }
  public static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
  public static Image Image(string name,Transform parent,Sprite sprite) {
   var r=Rect(name,parent);var i=r.gameObject.AddComponent<Image>();i.sprite=sprite;i.raycastTarget=false;return i;
  }
  public static Button Button(string name,Transform parent,Sprite sprite) {
   var i=Image(name,parent,sprite);i.raycastTarget=true;var b=i.gameObject.AddComponent<Button>();b.targetGraphic=i;b.navigation=new Navigation{mode=Navigation.Mode.None};
   var colors=b.colors;colors.pressedColor=new Color(.8f,.8f,.8f);colors.disabledColor=Color.white;b.colors=colors;return b;
  }
  public static Button RoundedButton(string name,Transform parent,Sprite sprite,float radius) {
   var r=Rect(name,parent);Mask(r,radius);r.GetComponent<ShopRoundRectMaskGraphic>().raycastTarget=true;
   var icon=Image("Icon",r,sprite);var b=r.gameObject.AddComponent<Button>();b.targetGraphic=icon;
   b.navigation=new Navigation{mode=Navigation.Mode.None};var colors=b.colors;colors.pressedColor=new Color(.8f,.8f,.8f);colors.disabledColor=Color.white;b.colors=colors;return b;
  }
  public static GameObject OffMark(Transform button) {
   var r=Rect("OffMark",button);r.anchorMin=r.anchorMax=Vector2.one*.5f;r.sizeDelta=new Vector2(85,10);r.localRotation=Quaternion.Euler(0,0,-45);
   var i=r.gameObject.AddComponent<Image>();i.color=new Color(.96f,.1f,.16f);i.raycastTarget=false;r.gameObject.SetActive(false);return r.gameObject;
  }
  public static void Mask(RectTransform r,float radius) {
   var shape=r.gameObject.AddComponent<ShopRoundRectMaskGraphic>();shape.RadiusFraction=radius;shape.raycastTarget=false;
   r.gameObject.AddComponent<Mask>().showMaskGraphic=false;
  }
 }
}
