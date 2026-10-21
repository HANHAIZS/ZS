using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class EconomyHeaderSetup {
  public static void Build(MainMenuPage page,Transform safe,TMP_FontAsset font){
   var previous=safe.Find("EconomyHeader");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
   string missing=new string("0123456789:full".Distinct().Where(c=>!font.characterLookupTable.ContainsKey(c)).ToArray());
   try{if(missing.Length>0){font.atlasPopulationMode=AtlasPopulationMode.Dynamic;if(!font.TryAddCharacters(missing))throw new InvalidOperationException("Missing economy font glyphs");}}
   finally{font.atlasPopulationMode=AtlasPopulationMode.Static;EditorUtility.SetDirty(font);}
   var header=Rect("EconomyHeader",safe);var view=header.gameObject.AddComponent<EconomyHeaderView>();page.Economy=view;
   var energy=Counter(header,"StaminaCounter",OkaUiRefreshSetup.Sprite("StaminaCounter"),.29f);
   var coins=Counter(header,"CoinCounter",OkaUiRefreshSetup.Sprite("CoinCounter"),.62f);
   var brown=new Color(.52f,.29f,.23f);
   view.CoinsLabel=Label(coins,"CoinsLabel","1600",font,new Vector2(.43f,.2f),new Vector2(.96f,.84f),34,brown);
   view.StaminaLabel=Label(energy,"StaminaLabel","5",font,new Vector2(.02f,.16f),new Vector2(.45f,.91f),34,new Color(1,.97f,.90f));
   view.RecoveryLabel=Label(energy,"RecoveryLabel","full",font,new Vector2(.51f,.22f),new Vector2(.95f,.82f),30,brown);
   var badge=OkaUiRefreshSetup.Sprite("RefillBadge");
   view.CoinsPlus=Plus(coins,"CoinsPlus",.30f,badge);view.StaminaPlus=Plus(energy,"StaminaPlus",.35f,badge);
   OkaUiRefreshSetup.StyleHeader(view);
  }
  static Sprite Sprite(string path){return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single();}
  static RectTransform Rect(string name,Transform parent){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
  static RectTransform Counter(Transform parent,string name,Sprite sprite,float x){var r=Rect(name,parent);r.anchorMin=r.anchorMax=new Vector2(x,1);r.anchoredPosition=new Vector2(0,-64);r.sizeDelta=new Vector2(234,94);var image=r.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return r;}
  static TMP_Text Label(Transform parent,string name,string words,TMP_FontAsset font,Vector2 min,Vector2 max,float size,Color color){var r=Rect(name,parent);r.anchorMin=min;r.anchorMax=max;var text=r.gameObject.AddComponent<TextMeshProUGUI>();text.font=font;text.fontSharedMaterial=font.material;text.fontSize=size;text.text=words;text.color=color;text.alignment=TextAlignmentOptions.Center;text.enableWordWrapping=false;text.raycastTarget=false;text.UpdateMeshPadding();return text;}
  static Button Plus(Transform parent,string name,float x,Sprite sprite){var r=Rect(name,parent);r.anchorMin=r.anchorMax=new Vector2(x,.20f);r.sizeDelta=new Vector2(44,44);var image=r.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;var button=r.gameObject.AddComponent<Button>();button.targetGraphic=image;button.navigation=new Navigation{mode=Navigation.Mode.None};var colors=button.colors;colors.pressedColor=new Color(.84f,.84f,.84f);colors.disabledColor=Color.white;button.colors=colors;return button;}
 }
}
