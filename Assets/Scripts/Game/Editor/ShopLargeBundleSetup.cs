using System;
using UnityEditor;
using UnityEngine;
namespace BeeHarvest.Editor {
 public static class ShopLargeBundleSetup {
  [MenuItem("BeeHarvest/UI/Apply Large Bundle And Coin Formatting")]
  public static void Apply(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before updating shop art");
   var path=MainMenuSetup.PrefabPath;var root=PrefabUtility.LoadPrefabContents(path);
   try{
    var page=root.GetComponent<MainMenuPage>();var offers=ShopOffersSetup.FindOffers(page);
    ShopSmallBundleSetup.Style(offers.Find("SmallBundleOffer"));
    ShopSmallBundleSetup.StyleMedium(offers.Find("BigBundleOffer"));
    AddAndLayout(page);PrefabUtility.SaveAsPrefabAsset(root,path);
   }finally{PrefabUtility.UnloadPrefabContents(root);}
  }
  public static void AddAndLayout(MainMenuPage page){
   var offers=ShopOffersSetup.FindOffers(page);
   var large=offers.Find("LargeBundleOffer");
   if(large==null){
    var template=offers.Find("BigBundleOffer");
    if(template==null)throw new InvalidOperationException("Medium Bundle layout is missing");
    large=UnityEngine.Object.Instantiate(template.gameObject,offers,false).transform;large.name="LargeBundleOffer";
   }
   ShopSmallBundleSetup.StyleLarge(large);
   string[] names={"RemoveAdsOffer","SmallBundleOffer","BigBundleOffer","LargeBundleOffer"};
   float[] centers={.70f,.49f,.28f,.07f};
   for(int i=0;i<names.Length;i++){
    var rect=(RectTransform)offers.Find(names[i]);
    rect.anchorMin=new Vector2(.035f,centers[i]-.11f);rect.anchorMax=new Vector2(.965f,centers[i]+.11f);
    rect.offsetMin=rect.offsetMax=Vector2.zero;rect.SetSiblingIndex(i);
   }
   ShopCoinOffersSetup.Build(page);
  }
 }
}
