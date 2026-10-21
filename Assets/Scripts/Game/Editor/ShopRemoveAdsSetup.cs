using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class ShopRemoveAdsSetup {
  public const string ArtPath="Assets/Sprites/UI/Shop/RemoveAds20261019/Panel.png";
  [MenuItem("BeeHarvest/UI/Apply Remove Ads Art And Price Insets")]
  public static void Apply(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before updating shop art");
   var path=MainMenuSetup.PrefabPath;var root=PrefabUtility.LoadPrefabContents(path);
   try{
    var offers=ShopOffersSetup.FindOffers(root.GetComponent<MainMenuPage>());
    Style(offers.Find("RemoveAdsOffer"));
    ShopSmallBundleSetup.StylePurchase(offers.Find("SmallBundleOffer"),"US$9.99");
    PrefabUtility.SaveAsPrefabAsset(root,path);
   }finally{PrefabUtility.UnloadPrefabContents(root);}
  }
  public static void Style(Transform offer){
   if(offer==null)throw new InvalidOperationException("Remove Ads offer is missing");
   var sprite=AssetDatabase.LoadAllAssetsAtPath(ArtPath).OfType<Sprite>().SingleOrDefault();
   if(sprite==null)throw new InvalidOperationException("Remove Ads panel is missing: "+ArtPath);
   var card=offer.Find("Card");card.GetComponent<AspectRatioFitter>().aspectRatio=sprite.rect.width/sprite.rect.height;
   var mask=card.GetComponent<Mask>();if(mask!=null)UnityEngine.Object.DestroyImmediate(mask);
   var clip=card.GetComponent<ShopRoundRectMaskGraphic>();if(clip!=null)UnityEngine.Object.DestroyImmediate(clip);
   var image=card.Find("Artwork").GetComponent<Image>();image.sprite=sprite;image.type=Image.Type.Simple;image.preserveAspect=false;image.raycastTarget=false;
   ShopSmallBundleSetup.StylePurchase(offer,"US$6.99");
  }
 }
}
