using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class ShopOffersSetup {
  public static void Build(MainMenuPage page){
   var shop=page.Tabs.ShopContent.transform;
   var previous=FindOffers(page);
   if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
   var offers=Rect("ShopOffers",shop);offers.SetAsFirstSibling();
   string[] names={"RemoveAds","SmallBundle","BigBundle"};
   float[] centers={.645f,.390f,.135f};
   for(int i=0;i<names.Length;i++){
    var art=Sprite(names[i]);var price=Sprite(names[i]+"Price");
    var cell=Rect(names[i]+"Offer",offers);cell.anchorMin=new Vector2(.035f,centers[i]-.12f);cell.anchorMax=new Vector2(.965f,centers[i]+.12f);
    var card=Rect("Card",cell);var fit=card.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=art.rect.width/art.rect.height;
    var mask=card.gameObject.AddComponent<ShopRoundRectMaskGraphic>();mask.RadiusFraction=i==0?.27f:.33f;mask.raycastTarget=false;
    card.gameObject.AddComponent<Mask>().showMaskGraphic=false;
    var artwork=Rect("Artwork",card);var image=artwork.gameObject.AddComponent<Image>();image.sprite=art;image.raycastTarget=false;
    var buttonRect=Rect("PriceButton",card);buttonRect.anchorMin=buttonRect.anchorMax=new Vector2(1,0);buttonRect.pivot=new Vector2(1,0);buttonRect.anchoredPosition=new Vector2(-16,4);buttonRect.sizeDelta=new Vector2(164,164*price.rect.height/price.rect.width);
    var clip=buttonRect.gameObject.AddComponent<ShopRoundRectMaskGraphic>();clip.RadiusFraction=.5f;clip.raycastTarget=false;
    buttonRect.gameObject.AddComponent<Mask>().showMaskGraphic=false;
    var visible=Rect("PriceArtwork",buttonRect);var priceImage=visible.gameObject.AddComponent<Image>();priceImage.sprite=price;
    var button=buttonRect.gameObject.AddComponent<Button>();button.targetGraphic=priceImage;button.navigation=new Navigation{mode=Navigation.Mode.None};
    if(i==0)ShopRemoveAdsSetup.Style(cell);
    if(i==1)ShopSmallBundleSetup.Style(cell);
    if(i==2)ShopSmallBundleSetup.StyleMedium(cell);
   }
   ShopLargeBundleSetup.AddAndLayout(page);
  }
  public static Transform FindOffers(MainMenuPage page){var shop=page.Tabs.ShopContent.transform;return shop.Find("ShopOffers")??shop.Find("ShopScroll/Viewport/ShopOffers");}
  static Sprite Sprite(string name){var path="Assets/Sprites/UI/Shop/"+name+".png";var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().SingleOrDefault();if(sprite==null)throw new InvalidOperationException("Shop Sprite is missing: "+path);return sprite;}
  static RectTransform Rect(string name,Transform parent){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
 }
}
