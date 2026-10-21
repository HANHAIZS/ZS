using System;
using System.Linq;
using GameFoundation.UI;
using GameFoundation.UI.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class HarvestInventorySetup {
  const string Prefab="Assets/Sprites/Prefabs/UI/HarvestBoosterBar.prefab",Art="Assets/Sprites/UI/Boosters/";
  [MenuItem("BeeHarvest/Level One/Apply Inventory Badges")]
  public static void Apply(){
   if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode before badge setup");
   var hud=UnityEngine.Object.FindObjectOfType<LevelOneHud>();if(hud==null)throw new InvalidOperationException("Load Level01 first");
   var quantity=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"QuantityBadge.png");var refill=OkaUiRefreshSetup.Sprite("RefillBadge");
   var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Sprites/UI/Fonts/FredokaBoldSDF.asset");
   if(quantity==null||refill==null||font==null)throw new InvalidOperationException("Badge art or Fredoka font missing");
   if(!font.HasCharacters("0123456789:")){var mode=font.atlasPopulationMode;try{font.isMultiAtlasTexturesEnabled=true;font.atlasPopulationMode=AtlasPopulationMode.Dynamic;string missing;if(!font.TryAddCharacters("0123456789:",out missing)&&missing.Length>0)throw new InvalidOperationException("Missing countdown glyphs: "+missing);}finally{font.atlasPopulationMode=mode;}EditorUtility.SetDirty(font);}
   var root=PrefabUtility.LoadPrefabContents(Prefab);
   try{
    var bar=root.GetComponent<HarvestBoosterBar>();bar.QuantityBadges=new GameObject[4];bar.QuantityLabels=new TMP_Text[4];bar.RefillButtons=new Button[4];
    for(int i=0;i<4;i++){
     var count=Badge(bar.Buttons[i].transform,"QuantityBadge",quantity);count.raycastTarget=false;
     var text=count.transform.Find("Count");if(text==null){text=new GameObject("Count",typeof(RectTransform),typeof(TextMeshProUGUI)).transform;text.SetParent(count.transform,false);}
     var rect=(RectTransform)text;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
     var label=text.GetComponent<TextMeshProUGUI>();label.font=font;label.fontSharedMaterial=font.material;label.fontSize=28;label.alignment=TextAlignmentOptions.Center;label.color=new Color(1,.965f,.898f);label.text="1";label.raycastTarget=false;label.enableWordWrapping=false;
     bar.QuantityBadges[i]=count.gameObject;bar.QuantityLabels[i]=label;
     var plus=Badge(bar.Buttons[i].transform,"RefillBadge",refill);plus.raycastTarget=true;var button=plus.GetComponent<Button>();if(button==null)button=plus.gameObject.AddComponent<Button>();button.targetGraphic=plus;button.navigation=new Navigation{mode=Navigation.Mode.None};
     bar.RefillButtons[i]=button;plus.gameObject.SetActive(false);count.gameObject.SetActive(true);
    }
    var timer=bar.StateLabels[1];timer.fontSize=18;timer.rectTransform.anchorMin=timer.rectTransform.anchorMax=new Vector2(.34f,.08f);timer.rectTransform.sizeDelta=new Vector2(108,31);
    OkaUiRefreshSetup.StyleBoosterBar(bar);
    PrefabUtility.SaveAsPrefabAsset(root,Prefab);
   }finally{PrefabUtility.UnloadPrefabContents(root);}
   var parent=hud.Boosters.transform.parent;int sibling=hud.Boosters.transform.GetSiblingIndex();UnityEngine.Object.DestroyImmediate(hud.Boosters.gameObject);
   var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab),parent);instance.transform.SetSiblingIndex(sibling);hud.Boosters=instance.GetComponent<HarvestBoosterBar>();
   EditorUtility.SetDirty(hud);EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);AssetDatabase.SaveAssets();
   var profile=AssetDatabase.LoadAssetAtPath<UiPrefabConventionProfile>("Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset");var report=UiPrefabValidator.Validate(Prefab,profile,UiPrefabKind.Component);if(!report.IsValid)throw new InvalidOperationException(report.ToMultilineString());
   Debug.Log("INVENTORY_BADGES_READY: component validator passed");
  }
  static Image Badge(Transform parent,string name,Sprite sprite){var child=parent.Find(name);if(child==null){child=new GameObject(name,typeof(RectTransform),typeof(Image)).transform;child.SetParent(parent,false);}var r=(RectTransform)child;r.anchorMin=r.anchorMax=new Vector2(.87f,.08f);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(52,52);var image=child.GetComponent<Image>();image.sprite=sprite;image.preserveAspect=true;return image;}
 }
}
