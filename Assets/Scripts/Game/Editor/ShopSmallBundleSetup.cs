using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class ShopSmallBundleSetup {
  public const string ArtRoot="Assets/Sprites/UI/Shop/SmallBundle20261019/";
  [MenuItem("BeeHarvest/UI/Apply Small Bundle Art")]
  public static void Apply(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before updating Small Bundle");
   var path=MainMenuSetup.PrefabPath;var root=PrefabUtility.LoadPrefabContents(path);
   try{Style(ShopOffersSetup.FindOffers(root.GetComponent<MainMenuPage>()).Find("SmallBundleOffer"));PrefabUtility.SaveAsPrefabAsset(root,path);}
   finally{PrefabUtility.UnloadPrefabContents(root);}
  }
  /// <summary>Restyles the offer while preserving its existing price Button and click events.</summary>
  public static void Style(Transform offer){StyleBundle(offer,Sprite("SmallBundleBag"),"Small Bundle","×1","1h","US$9.99","5.000");}
  public static void StyleMedium(Transform offer){
   const string path="Assets/Sprites/UI/Shop/MediumBundle20261019/Chest.png";
   var chest=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().SingleOrDefault();
   if(chest==null)throw new InvalidOperationException("Medium Bundle art missing: "+path);
   StyleBundle(offer,chest,"Medium Bundle","×3","3h","US$18.99","10.000");
  }
  public static void StyleLarge(Transform offer){
   const string root="Assets/Sprites/UI/Shop/LargeBundle20261019/";
   var safe=AssetDatabase.LoadAllAssetsAtPath(root+"Safe.png").OfType<Sprite>().SingleOrDefault();
   var panel=AssetDatabase.LoadAllAssetsAtPath(root+"Panel.png").OfType<Sprite>().SingleOrDefault();
   if(safe==null||panel==null)throw new InvalidOperationException("Large Bundle art missing: "+root);
   StyleBundle(offer,safe,"Big Bundle","×6","12h","US$36.99","25.000",panel);
  }
  static void StyleBundle(Transform offer,Sprite bundle,string title,string quantity,string duration,string priceCaption,string coins,Sprite panelArt=null){
   if(offer==null)throw new InvalidOperationException("Bundle offer is missing");
   var card=offer.Find("Card").GetComponent<RectTransform>();card.GetComponent<AspectRatioFitter>().aspectRatio=3.6f;
   RemoveMask(card);var panel=card.Find("Artwork").GetComponent<Image>();panel.sprite=panelArt??Sprite("Panel");panel.type=Image.Type.Simple;panel.preserveAspect=false;panel.raycastTarget=false;
   var page=offer.GetComponentInParent<MainMenuPage>();var font=page.StartGameButton.GetComponentInChildren<TMP_Text>(true).font;
   string caption=title+quantity+duration+priceCaption+coins;
   if(!font.HasCharacters(caption)){
    var mode=font.atlasPopulationMode;
    try{font.atlasPopulationMode=AtlasPopulationMode.Dynamic;string missing;if(!font.TryAddCharacters(caption,out missing)&&missing.Length>0)throw new InvalidOperationException("Small Bundle font glyphs missing: "+missing);}
    finally{font.atlasPopulationMode=mode;}
    EditorUtility.SetDirty(font);AssetDatabase.SaveAssetIfDirty(font);
   }
   var labelMaterial=TextMaterial("LabelText",font,new Color(.25f,.065f,.02f),.16f);
   Picture(card,"BundleBag",bundle,.055f,.32f,.255f,.96f);
   string[] names={"Speed","FreeSelect","Shuffle","AddSlot","InfiniteStamina"};
   for(int i=0;i<names.Length;i++){
    float center=.45f+i*.115f;
    var sprite=i==4?Sprite(names[i]):OkaUiRefreshSetup.Sprite(names[i]);
    Picture(card,names[i]+"Icon",sprite,center-.052f,.51f,center+.052f,.94f);
    Label(card,names[i]+"Amount",i==4?duration:quantity,font,labelMaterial,center-.057f,.345f,center+.057f,.51f,22,TextAlignmentOptions.Center);
   }
   Label(card,"BundleTitle",title,font,labelMaterial,.035f,.035f,.70f,.30f,28,TextAlignmentOptions.MidlineLeft);
   StylePurchase(offer,priceCaption);
   var coinFont=page.Economy.CoinsLabel.font;
   var coinMaterial=TextMaterial("PriceText",coinFont,new Color(.025f,.075f,.28f),.20f);
   Label(card,"CoinAmount",coins,coinFont,coinMaterial,.175f,.30f,.325f,.46f,24,TextAlignmentOptions.Center);
   card.Find("PriceButton").SetAsLastSibling();
  }
  /// <summary>Uses the economy font and keeps both visible price buttons clear of their card borders.</summary>
  public static void StylePurchase(Transform offer,string caption){
   var card=offer.Find("Card");var page=offer.GetComponentInParent<MainMenuPage>();
   var font=page.Economy.CoinsLabel.font;
   var priceMaterial=TextMaterial("PriceText",font,new Color(.025f,.075f,.28f),.20f);
   var price=card.Find("PriceButton").GetComponent<RectTransform>();RemoveMask(price);
   price.anchorMin=price.anchorMax=new Vector2(1,0);price.pivot=new Vector2(1,0);price.anchoredPosition=new Vector2(-28,16);price.sizeDelta=new Vector2(164,50);
   var priceImage=price.Find("PriceArtwork").GetComponent<Image>();priceImage.sprite=Sprite("PurchaseButton");priceImage.type=Image.Type.Simple;priceImage.preserveAspect=false;
   Label(price,"PriceLabel",caption,font,priceMaterial,.04f,.08f,.96f,.94f,26,TextAlignmentOptions.Center);
   price.SetAsLastSibling();
  }
  static void RemoveMask(Transform owner){var mask=owner.GetComponent<Mask>();if(mask!=null)UnityEngine.Object.DestroyImmediate(mask);var graphic=owner.GetComponent<ShopRoundRectMaskGraphic>();if(graphic!=null)UnityEngine.Object.DestroyImmediate(graphic);}
  static Sprite Sprite(string name){var sprite=AssetDatabase.LoadAllAssetsAtPath(ArtRoot+name+".png").OfType<Sprite>().SingleOrDefault();if(sprite==null)throw new InvalidOperationException("Small Bundle art missing: "+name);return sprite;}
  static Material TextMaterial(string name,TMP_FontAsset font,Color outline,float width){
   string path=ArtRoot+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(material==null){material=new Material(font.material);material.name="SmallBundle"+name;AssetDatabase.CreateAsset(material,path);}
   material.SetColor("_FaceColor",Color.white);material.SetColor("_OutlineColor",outline);material.SetFloat("_OutlineWidth",width);material.SetFloat("_OutlineSoftness",0);material.SetFloat("_FaceDilate",.06f);
   EditorUtility.SetDirty(material);AssetDatabase.SaveAssetIfDirty(material);return material;
  }
  static RectTransform Rect(Transform parent,string name,float x0,float y0,float x1,float y1){
   var found=parent.Find(name);var rect=found==null?new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>():(RectTransform)found;
   if(found==null)rect.SetParent(parent,false);rect.anchorMin=new Vector2(x0,y0);rect.anchorMax=new Vector2(x1,y1);rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;
  }
  static void Picture(Transform parent,string name,Sprite sprite,float x0,float y0,float x1,float y1){
   var rect=Rect(parent,name,x0,y0,x1,y1);var image=rect.GetComponent<Image>();if(image==null)image=rect.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
  }
  static void Label(Transform parent,string name,string value,TMP_FontAsset font,Material material,float x0,float y0,float x1,float y1,float size,TextAlignmentOptions alignment){
   var rect=Rect(parent,name,x0,y0,x1,y1);var text=rect.GetComponent<TextMeshProUGUI>();if(text==null)text=rect.gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSharedMaterial=material;text.text=value;text.color=Color.white;text.alignment=alignment;text.enableWordWrapping=false;text.enableAutoSizing=true;text.fontSizeMin=size*.8f;text.fontSizeMax=size;text.fontSize=size;text.raycastTarget=false;text.extraPadding=true;text.UpdateMeshPadding();
  }
 }
}
