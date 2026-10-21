namespace BeeHarvest.Editor {
 public static class MainMenuTabsSetup {
  public static void Apply(){
if(UnityEngine.Application.isPlaying)throw new System.InvalidOperationException("Stop Play Mode before tab setup.");
var path="Assets/Resources/Prefabs/UI/MainMenuPage.prefab";
var root=UnityEditor.PrefabUtility.LoadPrefabContents(path);
try {
 var page=root.GetComponent<BeeHarvest.MainMenuPage>();
 var safe=root.transform.Find("Content/SafeArea");
 var font=page.StartGameButton.GetComponentInChildren<TMPro.TMP_Text>().font;
 var missing=new System.Collections.Generic.List<char>();
 foreach(var letter in "ShopGallerylevel 123456789")if(!font.characterLookupTable.ContainsKey(letter)&&!missing.Contains(letter))missing.Add(letter);
 if(missing.Count>0)throw new System.InvalidOperationException("Missing font glyphs: "+new string(missing.ToArray()));
 System.Func<string,UnityEngine.Transform,UnityEngine.RectTransform> rect=(name,parent)=>{
  var obj=new UnityEngine.GameObject(name,typeof(UnityEngine.RectTransform));obj.transform.SetParent(parent,false);
  var rt=obj.GetComponent<UnityEngine.RectTransform>();rt.anchorMin=UnityEngine.Vector2.zero;rt.anchorMax=UnityEngine.Vector2.one;rt.offsetMin=rt.offsetMax=UnityEngine.Vector2.zero;return rt;
 };
 System.Func<UnityEngine.RectTransform,string,string,float,TMPro.TextMeshProUGUI> label=(parent,name,words,size)=>{
  var rt=rect(name,parent);var text=rt.gameObject.AddComponent<TMPro.TextMeshProUGUI>();text.font=font;text.fontSharedMaterial=font.material;
  text.text=words;text.fontSize=size;text.color=new UnityEngine.Color(1,.965f,.898f);text.alignment=TMPro.TextAlignmentOptions.Center;
  text.enableWordWrapping=false;text.raycastTarget=false;text.UpdateMeshPadding();return text;
 };
 var oldShop=safe.Find("ShopContent");if(oldShop!=null)UnityEngine.Object.DestroyImmediate(oldShop.gameObject);
 var tabs=root.GetComponent<BeeHarvest.MainMenuTabsController>();if(tabs==null)tabs=root.AddComponent<BeeHarvest.MainMenuTabsController>();page.Tabs=tabs;tabs.Page=page;
 tabs.Background=root.transform.Find("Content/Background").GetComponent<UnityEngine.UI.Image>();tabs.HomeBackground=tabs.Background.sprite;
 tabs.ShopBackground=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>("Assets/Sprites/UI/MainMenu/ShopBackground.png");
 
 tabs.NavigationSelection=safe.Find("HomeSelection").GetComponent<UnityEngine.RectTransform>();
 var shop=rect("ShopContent",safe);shop.anchorMin=new UnityEngine.Vector2(0,.17f);shop.anchorMax=UnityEngine.Vector2.one;shop.SetAsFirstSibling();
 var shopTitle=label(shop,"ShopTitle","Shop",56);shopTitle.rectTransform.anchorMin=new UnityEngine.Vector2(.1f,.81f);shopTitle.rectTransform.anchorMax=new UnityEngine.Vector2(.9f,.90f);
 tabs.ShopContent=shop.gameObject;
 SettingsScreenSetup.Build(page,safe);
 shop.gameObject.SetActive(false);
 page.ShopButton.interactable=page.HomeButton.interactable=page.GalleryButton.interactable=true;
 HomeArtworkSetup.Build(page,safe,font);
 ShopOffersSetup.Build(page);
 MainMenuNavigationBarSetup.Build(page);
 page.SettingsPopup.transform.SetAsLastSibling();
 UnityEditor.PrefabUtility.SaveAsPrefabAsset(root,path);
} finally { UnityEditor.PrefabUtility.UnloadPrefabContents(root); }
UnityEditor.AssetDatabase.SaveAssets();
var profile=UnityEditor.AssetDatabase.LoadAssetAtPath<GameFoundation.UI.UiPrefabConventionProfile>("Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset");
var report=GameFoundation.UI.Editor.UiPrefabValidator.Validate(path,profile,GameFoundation.UI.UiPrefabKind.Page);
if(!report.IsValid)throw new System.InvalidOperationException(report.ToMultilineString());

  }
 }
}
