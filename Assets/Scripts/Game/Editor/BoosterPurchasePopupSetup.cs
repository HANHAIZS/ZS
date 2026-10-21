using System;
using System.Linq;
using GameFoundation.UI;
using GameFoundation.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class BoosterPurchasePopupSetup {
  public const string PrefabPath="Assets/Sprites/Prefabs/UI/BoosterPurchasePopup.prefab";
  public const string ArtRoot="Assets/Sprites/UI/BoosterPurchase/";
  const string BarPath="Assets/Sprites/Prefabs/UI/HarvestBoosterBar.prefab";
  const float Scale=740f/758f;
  [MenuItem("BeeHarvest/UI/Apply Booster Purchase Popup")]
  public static void Apply(){
   if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
   Import("PurchaseButton",new Rect(0,0,2057,765),1024);Import("Coin",new Rect(110,106,1023,1054),256);
   var profile=AssetDatabase.LoadAssetAtPath<UiPrefabConventionProfile>("Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset");
   var font=Font();var priceMaterial=PriceMaterial(font);
   var root=OkaSettingsSetup.Rect("BoosterPurchasePopup",null);var popup=root.gameObject.AddComponent<BoosterPurchasePopupController>();
   try{
    var dim=OkaSettingsSetup.Image("DimBackground",root,null);dim.color=new Color(0,0,0,profile.DimBackgroundAlpha);dim.raycastTarget=true;
    var content=OkaSettingsSetup.Rect("PopupContent",root);Place(content,new Vector2(758,990),Vector2.zero);content.gameObject.AddComponent<PausedDialogFit>();
    var background=OkaSettingsSetup.Image("PanelBackground",content,PauseArt("Background"));background.raycastTarget=true;Place(background.rectTransform,new Vector2(723,959),new Vector2(-7.5f,-24.5f));
    var header=OkaSettingsSetup.Image("TitleBanner",content,PauseArt("Header"));Place(header.rectTransform,new Vector2(482,170),new Vector2(-6,410));
    popup.TitleLabel=Label(header.transform,"Title","Extra Slot",font,new Vector2(430,125),new Vector2(0,8),86);popup.TitleLabel.enableAutoSizing=true;popup.TitleLabel.fontSizeMin=65;popup.TitleLabel.fontSizeMax=86*Scale;
    popup.CloseButton=OkaSettingsSetup.Button("CloseButton",content,PauseArt("Close"));Place((RectTransform)popup.CloseButton.transform,new Vector2(119,142),new Vector2(310.5f,376));
    var panel=OkaSettingsSetup.Image("IconPanel",content,AssetDatabase.LoadAllAssetsAtPath(LeavingDialogSetup.ArtRoot+"Inset.png").OfType<Sprite>().Single());Place(panel.rectTransform,new Vector2(604,438),new Vector2(-7,144));
    popup.Icons=new[]{"AddSlot","Speed","Shuffle","FreeSelect"}.Select(OkaUiRefreshSetup.Sprite).ToArray();
    popup.BoosterIcon=OkaSettingsSetup.Image("BoosterIcon",panel.transform,popup.Icons[0]);popup.BoosterIcon.preserveAspect=true;Place(popup.BoosterIcon.rectTransform,new Vector2(310,300),new Vector2(-15,4));
    popup.QuantityLabel=Label(panel.transform,"Quantity","×3",font,new Vector2(210,125),new Vector2(111,-96),100);
    popup.DescriptionLabel=Label(content,"Description","Add 1 extra slot",font,new Vector2(620,145),new Vector2(-7,-160),48);popup.DescriptionLabel.enableWordWrapping=true;popup.DescriptionLabel.enableAutoSizing=true;popup.DescriptionLabel.fontSizeMax=48*Scale;popup.DescriptionLabel.fontSizeMin=38*Scale;
    popup.PurchaseButton=OkaSettingsSetup.Button("PurchaseButton",content,Art("PurchaseButton"));Place((RectTransform)popup.PurchaseButton.transform,new Vector2(484,180),new Vector2(-7,-329));
    var colors=popup.PurchaseButton.colors;colors.disabledColor=new Color(.55f,.55f,.55f);popup.PurchaseButton.colors=colors;
    var coin=OkaSettingsSetup.Image("Coin",popup.PurchaseButton.transform,Art("Coin"));coin.preserveAspect=true;Place(coin.rectTransform,new Vector2(103,108),new Vector2(-132,8));
    popup.PriceLabel=Label(popup.PurchaseButton.transform,"Price","1600",font,new Vector2(280,130),new Vector2(61,8),90);popup.PriceLabel.fontSharedMaterial=priceMaterial;popup.PriceLabel.UpdateMeshPadding();
    popup.DimBackground=dim.gameObject;popup.PopupContent=content.gameObject;dim.gameObject.SetActive(false);content.gameObject.SetActive(false);
    PrefabUtility.SaveAsPrefabAsset(root.gameObject,PrefabPath);
   }finally{UnityEngine.Object.DestroyImmediate(root.gameObject);}
   var bar=PrefabUtility.LoadPrefabContents(BarPath);try{Bind(bar.GetComponent<HarvestBoosterBar>());PrefabUtility.SaveAsPrefabAsset(bar,BarPath);}finally{PrefabUtility.UnloadPrefabContents(bar);}
   AssetDatabase.SaveAssets();var report=UiPrefabValidator.Validate(PrefabPath,profile,UiPrefabKind.Popup);if(!report.IsValid)throw new InvalidOperationException(report.ToMultilineString());
  }
  public static void Bind(HarvestBoosterBar bar){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);if(prefab==null)throw new InvalidOperationException("Build booster purchase popup first.");bar.PurchasePopupPrefab=prefab.GetComponent<BoosterPurchasePopupController>();EditorUtility.SetDirty(bar);}
  static Sprite PauseArt(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(LevelPauseSetup.ArtRoot+name+".png");
  static Sprite Art(string name)=>AssetDatabase.LoadAllAssetsAtPath(ArtRoot+name+".png").OfType<Sprite>().Single();
  static void Place(RectTransform r,Vector2 size,Vector2 position){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=size*Scale;r.anchoredPosition=position*Scale;}
  static TMP_Text Label(Transform parent,string name,string text,TMP_FontAsset font,Vector2 size,Vector2 position,float pointSize){
   var label=OkaSettingsSetup.Rect(name,parent).gameObject.AddComponent<TextMeshProUGUI>();Place(label.rectTransform,size,position);label.font=font;label.fontSharedMaterial=font.material;label.text=text;label.fontSize=pointSize*Scale;label.color=new Color(1,.985f,.96f);label.alignment=TextAlignmentOptions.Center;label.enableWordWrapping=false;label.raycastTarget=false;label.UpdateMeshPadding();return label;
  }
  static void Import(string name,Rect rect,int maximum){
   string path=ArtRoot+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
   importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.isReadable=false;importer.maxTextureSize=maximum;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.CompressedHQ;
#pragma warning disable 0618
   importer.spritesheet=new[]{new SpriteMetaData{name=name,rect=rect,alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f}};
#pragma warning restore 0618
   EditorUtility.SetDirty(importer);AssetDatabase.WriteImportSettingsIfDirty(path);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
  }
  static TMP_FontAsset Font(){
   string path=ArtRoot+"BoosterPurchaseFont.asset";var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
   if(font==null){
    var source=AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/Sprites/UI/Fonts/Fredoka-Bold.ttf");font=TMP_FontAsset.CreateFontAsset(source,90,12,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,false);
    if(font==null||!font.TryAddCharacters("Extra SlotSpeed UpShufflePick AnyAdd 1 extra slotSpeed up all bee carry speedsShuffle all single hive positionsPick any Hive from queue×31600 _"))throw new InvalidOperationException("Booster glyph bake failed");
    font.name="BoosterPurchaseFont";font.atlasPopulationMode=AtlasPopulationMode.Static;AssetDatabase.CreateAsset(font,path);font.material.name="BoosterPurchaseFont Material";AssetDatabase.AddObjectToAsset(font.material,font);foreach(var atlas in font.atlasTextures){atlas.name="BoosterPurchaseFont Atlas";AssetDatabase.AddObjectToAsset(atlas,font);}
   }
   font.material.SetColor("_FaceColor",Color.white);font.material.SetColor("_OutlineColor",new Color(.25f,.005f,.015f));font.material.SetFloat("_OutlineWidth",.16f);font.material.EnableKeyword("OUTLINE_ON");EditorUtility.SetDirty(font);EditorUtility.SetDirty(font.material);return font;
  }
  static Material PriceMaterial(TMP_FontAsset font){
   string path=ArtRoot+"Price.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material==null){material=new Material(font.material);material.name="BoosterPurchasePrice";AssetDatabase.CreateAsset(material,path);}
   material.SetColor("_OutlineColor",new Color(.015f,.22f,.005f));EditorUtility.SetDirty(material);return material;
  }
 }
}
