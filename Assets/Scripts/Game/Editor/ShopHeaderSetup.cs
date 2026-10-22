using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest.Editor {
 public static class ShopHeaderSetup {
  public const string ArtRoot="Assets/Sprites/UI/Shop/Header20261022/";
  const float TopInset=0, PlaqueWidth=370, ContentGap=16;

  [MenuItem("BeeHarvest/UI/Apply Shop Header")]
  public static void Apply() {
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before updating shop header.");
   var root=PrefabUtility.LoadPrefabContents(MainMenuSetup.PrefabPath);
   try {Build(root.GetComponent<MainMenuPage>());PrefabUtility.SaveAsPrefabAsset(root,MainMenuSetup.PrefabPath);}
   finally {PrefabUtility.UnloadPrefabContents(root);}
  }

  public static void Build(MainMenuPage page) {
   var background=Sprite("Background");var plaque=Sprite("Plaque");
   var shop=page.Tabs.ShopContent.transform;
   var scroll=shop.Find("ShopScroll") as RectTransform;
   var settingsHeader=page.Tabs.GalleryContent.transform.Find("SettingsHeader") as RectTransform;
   if(scroll==null||settingsHeader==null)throw new InvalidOperationException("Shop scroll and Settings header must be built first.");
   var old=shop.Find("ShopTitle");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
   var header=Rect(shop,"ShopHeader");header.anchorMin=new Vector2(0,1);header.anchorMax=Vector2.one;header.pivot=new Vector2(.5f,1);
   float plaqueHeight=PlaqueWidth*plaque.rect.height/plaque.rect.width;
   float height=Mathf.Max(settingsHeader.rect.height,plaqueHeight);
   // The canvas-free prefab still stores the Settings header's authored height in sizeDelta.
   float backgroundHeight=settingsHeader.sizeDelta.y;
   header.sizeDelta=new Vector2(0,Mathf.Max(backgroundHeight,height));header.anchoredPosition=new Vector2(0,-TopInset);
   var back=Picture(header,"Background",background);var b=back.rectTransform;
   b.anchorMin=new Vector2(0,1);b.anchorMax=Vector2.one;b.pivot=new Vector2(.5f,1);b.sizeDelta=new Vector2(0,backgroundHeight);b.anchoredPosition=Vector2.zero;
   var title=Picture(header,"Plaque",plaque);var t=title.rectTransform;
   t.anchorMin=t.anchorMax=new Vector2(.5f,1);t.pivot=new Vector2(.5f,1);t.sizeDelta=new Vector2(PlaqueWidth,plaqueHeight);t.anchoredPosition=Vector2.zero;
   back.transform.SetAsFirstSibling();title.transform.SetAsLastSibling();header.SetAsLastSibling();
   scroll.anchorMax=new Vector2(scroll.anchorMax.x,1);
   scroll.offsetMax=new Vector2(scroll.offsetMax.x,-TopInset-header.sizeDelta.y-ContentGap);
  }

  static RectTransform Rect(Transform parent,string name) {
   var found=parent.Find(name) as RectTransform;if(found!=null)return found;
   var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);return r;
  }
  static Image Picture(Transform parent,string name,Sprite sprite) {
   var r=Rect(parent,name);var image=r.GetComponent<Image>();if(image==null)image=r.gameObject.AddComponent<Image>();
   image.sprite=sprite;image.color=Color.white;image.type=Image.Type.Simple;image.raycastTarget=false;return image;
  }
  static Sprite Sprite(string name) {
   var sprite=AssetDatabase.LoadAllAssetsAtPath(ArtRoot+name+".png").OfType<Sprite>().SingleOrDefault();
   if(sprite==null)throw new InvalidOperationException("Shop header Sprite missing: "+name);return sprite;
  }

  public static void ImportArt() {
   foreach(string name in new[]{"Background","Plaque"}) {
    string path=ArtRoot+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
    var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
    importer.spriteImportMode=name=="Background"?SpriteImportMode.Single:SpriteImportMode.Multiple;
    importer.mipmapEnabled=false;importer.isReadable=false;importer.alphaIsTransparency=true;
    importer.maxTextureSize=2048;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;
    if(name=="Plaque") {
#pragma warning disable 0618
     importer.spritesheet=new[]{new SpriteMetaData{name="Plaque",rect=new Rect(62,107,1328,842),alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f}};
#pragma warning restore 0618
     byte[] hash;using(var md5=System.Security.Cryptography.MD5.Create())hash=md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(path+"/Plaque"));
     long id=BitConverter.ToInt64(hash,0)&long.MaxValue;if(id==0)id=1;
     var serialized=new SerializedObject(importer);var entry=serialized.FindProperty("m_SpriteSheet.m_Sprites").GetArrayElementAtIndex(0);
     entry.FindPropertyRelative("m_InternalID").longValue=id;entry.FindPropertyRelative("m_SpriteID").stringValue=new Guid(hash).ToString("N");
     var table=serialized.FindProperty("m_SpriteSheet.m_NameFileIdTable");table.arraySize=1;var pair=table.GetArrayElementAtIndex(0);
     pair.FindPropertyRelative("first").stringValue="Plaque";pair.FindPropertyRelative("second").longValue=id;serialized.ApplyModifiedPropertiesWithoutUndo();
    }
    importer.SaveAndReimport();
   }
  }
 }
}
