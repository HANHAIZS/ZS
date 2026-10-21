using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class ShopCoinOffersSetup {
  public const string ArtRoot="Assets/Sprites/UI/Shop/CoinOffers20261019/";
  [MenuItem("BeeHarvest/UI/Apply Coin Offers And Shop Scrolling")]
  public static void Apply(){
   if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before updating shop");
   var path=MainMenuSetup.PrefabPath;var root=PrefabUtility.LoadPrefabContents(path);
   try{Build(root.GetComponent<MainMenuPage>());PrefabUtility.SaveAsPrefabAsset(root,path);}
   finally{PrefabUtility.UnloadPrefabContents(root);}
  }
  public static void Build(MainMenuPage page){
   var shop=page.Tabs.ShopContent.transform;var offers=(RectTransform)ShopOffersSetup.FindOffers(page);
   if(offers==null)throw new InvalidOperationException("Shop offers are missing");
   var scrollRoot=Rect(shop,"ShopScroll",0,0,1,.79f);
   var viewport=Rect(scrollRoot,"Viewport",0,0,1,1);
   var surface=Get<Image>(viewport);surface.color=Color.clear;surface.raycastTarget=true;
   Get<RectMask2D>(viewport);
   offers.SetParent(viewport,false);offers.anchorMin=new Vector2(0,1);offers.anchorMax=Vector2.one;offers.pivot=new Vector2(.5f,1);offers.anchoredPosition=Vector2.zero;
   // MainMenuPage is a canvas-free prefab; MainMenuSetup authors a 750-wide scene canvas.
   float width=750f*.93f;
   float cursor=6;
   foreach(string name in new[]{"RemoveAdsOffer","SmallBundleOffer","BigBundleOffer","LargeBundleOffer"}){
    var cell=(RectTransform)offers.Find(name);
    if(cell==null)throw new InvalidOperationException("Shop offer missing: "+name);
    float height=width/cell.Find("Card").GetComponent<AspectRatioFitter>().aspectRatio;
    Top(cell,.035f,.965f,cursor,height);cursor+=height+16;
   }
   var font=page.Economy.CoinsLabel.font;
   string missing;
   if(!font.HasCharacters("Coins0123456789.$")){
    var mode=font.atlasPopulationMode;
    try{font.atlasPopulationMode=AtlasPopulationMode.Dynamic;if(!font.TryAddCharacters("Coins0123456789.$",out missing)&&missing.Length>0)throw new InvalidOperationException("Coin font glyphs missing: "+missing);}
    finally{font.atlasPopulationMode=mode;}
    EditorUtility.SetDirty(font);AssetDatabase.SaveAssetIfDirty(font);
   }
   var priceMaterial=AssetDatabase.LoadAssetAtPath<Material>(ShopSmallBundleSetup.ArtRoot+"PriceText.mat");
   if(priceMaterial==null)throw new InvalidOperationException("Shop price material missing");
   var amountMaterial=AmountMaterial(font);
   var title=Rect(offers,"CoinSectionTitle",0,1,1,1);Top(title,.035f,.965f,cursor,48);
   Label(title,"Text","Coins",font,priceMaterial,0,0,1,1,34);cursor+=54;
   var grid=Rect(offers,"CoinOffers",0,1,1,1);Top(grid,.035f,.965f,cursor,620);
   string[] amounts={"1.000","5.000","10.000","25.000","50.000","100.000"};
   string[] prices={"$1.99","$8.99","$16.99","$34.99","$64.99","$99.99"};
   string[] icons={"Coins1000","Coins5000","Coins10000","Coins25000","Coins50000","Coins100000"};
   for(int i=0;i<icons.Length;i++){
    int col=i%3,row=i/3;
    var cell=Rect(grid,icons[i]+"Offer",0,1,1,1);Top(cell,col/3f,(col+1)/3f,row*320,300);
    cell.offsetMin=new Vector2(col==0?0:6,cell.offsetMin.y);cell.offsetMax=new Vector2(col==2?0:-6,cell.offsetMax.y);
    Picture(cell,"Card",Sprite("Card"),0,0,1,1,false);
    Picture(cell,"Icon",Sprite(icons[i]),.10f,.34f,.90f,.94f,true);
    Label(cell,"CoinAmount",amounts[i],font,amountMaterial,.025f,.23f,.975f,.45f,40);
    var price=Rect(cell,"PriceButton",.13f,.075f,.87f,.245f);
    var face=Get<Image>(price);face.sprite=AssetDatabase.LoadAllAssetsAtPath(ShopSmallBundleSetup.ArtRoot+"PurchaseButton.png").OfType<Sprite>().Single();face.type=Image.Type.Simple;face.raycastTarget=true;
    var button=Get<Button>(price);button.targetGraphic=face;button.navigation=new Navigation{mode=Navigation.Mode.None};
    Label(price,"PriceLabel",prices[i],font,priceMaterial,.04f,.06f,.96f,.96f,26);
   }
   offers.sizeDelta=new Vector2(0,cursor+620+16);
   var scroll=Get<ScrollRect>(scrollRoot);scroll.viewport=viewport;scroll.content=offers;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=true;scroll.decelerationRate=.135f;scroll.scrollSensitivity=48;scroll.StopMovement();scroll.verticalNormalizedPosition=1;
   scrollRoot.SetAsFirstSibling();
  }
  static T Get<T>(RectTransform r) where T:Component {var c=r.GetComponent<T>();return c==null?r.gameObject.AddComponent<T>():c;}
  static RectTransform Rect(Transform parent,string name,float x0,float y0,float x1,float y1){
   var found=parent.Find(name);var r=found==null?new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>():(RectTransform)found;
   if(found==null)r.SetParent(parent,false);r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;return r;
  }
  static void Top(RectTransform r,float left,float right,float y,float height){r.anchorMin=new Vector2(left,1);r.anchorMax=new Vector2(right,1);r.pivot=new Vector2(.5f,1);r.sizeDelta=new Vector2(0,height);r.anchoredPosition=new Vector2(0,-y);}
  static Sprite Sprite(string name){var s=AssetDatabase.LoadAllAssetsAtPath(ArtRoot+name+".png").OfType<Sprite>().SingleOrDefault();if(s==null)throw new InvalidOperationException("Coin art missing: "+name);return s;}
  static void Picture(Transform parent,string name,Sprite sprite,float x0,float y0,float x1,float y1,bool aspect){var r=Rect(parent,name,x0,y0,x1,y1);var image=Get<Image>(r);image.sprite=sprite;image.preserveAspect=aspect;image.raycastTarget=false;}
  static void Label(Transform parent,string name,string value,TMP_FontAsset font,Material material,float x0,float y0,float x1,float y1,float size){var r=Rect(parent,name,x0,y0,x1,y1);var t=Get<TextMeshProUGUI>(r);t.font=font;t.fontSharedMaterial=material;t.text=value;t.color=Color.white;t.alignment=TextAlignmentOptions.Center;t.enableWordWrapping=false;t.enableAutoSizing=true;t.fontSizeMin=size*.8f;t.fontSizeMax=size;t.fontSize=size;t.raycastTarget=false;t.extraPadding=true;t.UpdateMeshPadding();}
  static Material AmountMaterial(TMP_FontAsset font){
   var path=ArtRoot+"CoinAmount.mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(m==null){m=new Material(font.material);m.name="ShopCoinAmount";AssetDatabase.CreateAsset(m,path);}
   m.SetColor("_FaceColor",new Color(1,.69f,.08f));m.SetColor("_OutlineColor",new Color(.32f,.11f,.015f));m.SetFloat("_OutlineWidth",.14f);m.SetFloat("_FaceDilate",.04f);m.SetFloat("_OutlineSoftness",0);
   m.EnableKeyword("UNDERLAY_ON");m.SetColor("_UnderlayColor",Color.white);m.SetFloat("_UnderlayOffsetX",0);m.SetFloat("_UnderlayOffsetY",0);m.SetFloat("_UnderlayDilate",.3f);m.SetFloat("_UnderlaySoftness",0);
   EditorUtility.SetDirty(m);AssetDatabase.SaveAssetIfDirty(m);return m;
  }
 }
}
