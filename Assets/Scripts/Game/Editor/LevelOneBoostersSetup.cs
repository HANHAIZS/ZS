using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
namespace BeeHarvest.Editor {
 public static class LevelOneBoostersSetup {
  const string Art=OkaUiRefreshSetup.ArtRoot,Prefab="Assets/Sprites/Prefabs/UI/HarvestBoosterBar.prefab";
  [MenuItem("BeeHarvest/Level One/Apply Four Boosters")]
  public static void Apply(){
   if(Application.isPlaying)throw new InvalidOperationException("Boosters setup requires Edit Mode");
   var hud=UnityEngine.Object.FindObjectOfType<LevelOneHud>();if(hud==null)throw new InvalidOperationException("Load Level01 first");
   var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Sprites/UI/Fonts/FredokaBoldSDF.asset");
   string chars="+1 2x ON <>";if(!font.HasCharacters(chars)){var mode=font.atlasPopulationMode;try{font.atlasPopulationMode=AtlasPopulationMode.Dynamic;string missing;font.TryAddCharacters(chars,out missing);}finally{font.atlasPopulationMode=mode;}EditorUtility.SetDirty(font);}
   var root=new GameObject("HarvestBoosterBar",typeof(RectTransform));var rect=(RectTransform)root.transform;Set(rect,0,0,1,.115f);
   var bar=root.AddComponent<HarvestBoosterBar>();bar.Buttons=new Button[4];bar.StateLabels=new TMP_Text[4];
   var background=Image(root.transform,"ToolbarBase",Sprite("ToolbarBase"));Set(background.rectTransform,0,0,1,.86f);
   var names=new[]{"AddSlot","Speed","Shuffle","FreeSelect"};
   for(int i=0;i<4;i++){
    var image=Image(root.transform,"Booster_"+names[i],Sprite("ButtonDisc"));
    var r=image.rectTransform;r.anchorMin=r.anchorMax=new Vector2(.14f+i*.24f,.85f);r.sizeDelta=new Vector2(142,142);r.anchoredPosition=Vector2.zero;image.preserveAspect=true;image.raycastTarget=true;
    var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;bar.Buttons[i]=b;
    var icon=Image(r,"Icon",Sprite(names[i]));icon.rectTransform.anchorMin=icon.rectTransform.anchorMax=new Vector2(.5f,.53f);icon.rectTransform.sizeDelta=new Vector2(112,112);icon.preserveAspect=true;
    var state=Label(r,"State","",font,24);state.rectTransform.anchorMin=state.rectTransform.anchorMax=new Vector2(.5f,.1f);state.rectTransform.sizeDelta=new Vector2(85,31);bar.StateLabels[i]=state;
   }
   OkaUiRefreshSetup.StyleBoosterBar(bar);
   BoosterPurchasePopupSetup.Bind(bar);
   PrefabUtility.SaveAsPrefabAsset(root,Prefab);UnityEngine.Object.DestroyImmediate(root);
   if(hud.Boosters!=null)UnityEngine.Object.DestroyImmediate(hud.Boosters.gameObject);
   var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab),hud.transform);hud.Boosters=instance.GetComponent<HarvestBoosterBar>();
   hud.RearButtons=new Button[5];
   for(int i=0;i<5;i++){
    float x=.025f+i*.195f;Set((RectTransform)hud.QueueButtons[i].transform,x,.225f,x+.17f,.3f);Set(hud.NextLabels[i].rectTransform,x,.14f,x+.17f,.215f);
    Set(hud.SlotLabels[i].rectTransform,x,.325f,x+.17f,.355f);hud.SlotTargets[i].anchorMin=hud.SlotTargets[i].anchorMax=new Vector2(x+.085f,.34f);
    var rear=hud.NextLabels[i].GetComponent<Button>();if(rear==null)rear=hud.NextLabels[i].gameObject.AddComponent<Button>();rear.targetGraphic=hud.NextLabels[i];rear.transition=Selectable.Transition.None;hud.RearButtons[i]=rear;
   }
   if(hud.SlotLabels.Length==5){var sixth=UnityEngine.Object.Instantiate(hud.SlotLabels[0],hud.transform);sixth.name="Slot_5";hud.SlotLabels=hud.SlotLabels.Concat(new[]{sixth}).ToArray();}
   if(hud.SlotTargets.Length==5){var sixth=UnityEngine.Object.Instantiate(hud.SlotTargets[0],hud.transform);sixth.name="HiveSlotTarget_5";hud.SlotTargets=hud.SlotTargets.Concat(new[]{sixth}).ToArray();}
   Set(hud.SlotLabels[5].rectTransform,.025f,.410f,.195f,.440f);hud.SlotTargets[5].anchorMin=hud.SlotTargets[5].anchorMax=new Vector2(.11f,.425f);
   hud.SlotLabels[5].gameObject.SetActive(false);hud.SlotTargets[5].gameObject.SetActive(false);
   hud.PreviousPage=PageButton(hud.transform,"WaitingPreviousPage","<",.045f,font);hud.NextPage=PageButton(hud.transform,"WaitingNextPage",">",.955f,font);
   EditorUtility.SetDirty(hud);EditorSceneManager.MarkSceneDirty(hud.gameObject.scene);EditorSceneManager.SaveScene(hud.gameObject.scene);AssetDatabase.SaveAssets();
   HarvestInventorySetup.Apply();
  }
  static Button PageButton(Transform parent,string name,string text,float x,TMP_FontAsset font){
   var previous=parent.Find(name);if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
   var image=Image(parent,name,Sprite("ButtonDisc"));image.rectTransform.anchorMin=image.rectTransform.anchorMax=new Vector2(x,.312f);image.rectTransform.sizeDelta=new Vector2(35,35);image.raycastTarget=true;
   var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;var label=Label(image.transform,"Arrow",text,font,27);Set(label.rectTransform,0,0,1,1);return b;
  }
  static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".png");
  static Image Image(Transform parent,string name,Sprite sprite){var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var image=go.GetComponent<Image>();image.sprite=sprite;image.raycastTarget=false;return image;}
  static TMP_Text Label(Transform parent,string name,string text,TMP_FontAsset font,int size){var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);var t=go.GetComponent<TextMeshProUGUI>();t.font=font;t.fontSize=size;t.text=text;t.alignment=TextAlignmentOptions.Center;t.color=new Color(1,.965f,.898f);t.raycastTarget=false;return t;}
  static void Set(RectTransform r,float x0,float y0,float x1,float y1){r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;}
 }
}
