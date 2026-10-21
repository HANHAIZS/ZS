using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest.Editor {
 public static class OkaUiRefreshSetup {
  public const string ArtRoot="Assets/Sprites/UI/OkaRefresh20261018/";
  public const string SlotTexturePath=ArtRoot+"HiveSlotTile.png";
  const string BarPrefab="Assets/Sprites/Prefabs/UI/HarvestBoosterBar.prefab";
  const string SlotMaterialPath="Assets/Sprites/UI/Gameplay/HiveSlotTile.mat";
  static readonly string[] Names={"CoinCounter","StaminaCounter","RefillBadge","ToolbarBase","ButtonDisc","SettingsGear","Speed","FreeSelect","Shuffle","AddSlot","HiveSlotTile"};
  [Serializable] sealed class AlphaRect {public Rect rect;public int width,height;}

  /// <summary>Imports art and updates existing serialized views without replacing their controls or listeners.</summary>
  [MenuItem("BeeHarvest/UI/Apply Oka Economy And Gameplay Art")]
  public static void Apply(){
   if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode before applying UI art");
   string previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
   foreach(var name in Names)Import(name);
   EditPrefab(MainMenuSetup.PrefabPath,root=>StyleHeader(root.GetComponent<MainMenuPage>().Economy));
   EditPrefab(BarPrefab,root=>StyleBoosterBar(root.GetComponent<HarvestBoosterBar>()));
   ApplySlotMaterial(AssetDatabase.LoadAssetAtPath<Material>(SlotMaterialPath));
   foreach(var path in Enumerable.Range(1,LevelOneAssets.LastLevel).Select(n=>"Assets/Scenes/"+LevelOneAssets.SceneName(n)+".unity")){
    var scene=EditorSceneManager.OpenScene(path);var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();
    StyleBoosterBar(level.Hud.Boosters);
    var popup=UnityEngine.Object.FindObjectsOfType<SettingsPopupController>(true).Single(p=>p.gameObject.scene==scene);
    popup.OpenButton.image.sprite=Sprite("SettingsGear");popup.OpenButton.image.preserveAspect=true;
    EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
   }
   AssetDatabase.SaveAssets();if(!string.IsNullOrEmpty(previous))EditorSceneManager.OpenScene(previous);
  }

  static void EditPrefab(string path,Action<GameObject> style){var root=PrefabUtility.LoadPrefabContents(path);try{style(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}}
  public static Sprite Sprite(string name){var value=AssetDatabase.LoadAllAssetsAtPath(ArtRoot+name+".png").OfType<Sprite>().SingleOrDefault();if(value==null)throw new InvalidOperationException("Missing Oka Sprite: "+name);return value;}

  static void Import(string name){
   var path=ArtRoot+name+".png";var t=AssetImporter.GetAtPath(path) as TextureImporter;
   if(t==null)throw new InvalidOperationException("Missing Oka source: "+path);
   bool slot=name=="HiveSlotTile";
   t.textureType=slot?TextureImporterType.Default:TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;
   t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;
   t.wrapMode=TextureWrapMode.Clamp;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=8192;t.isReadable=true;t.SaveAndReimport();
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var pixels=texture.GetPixels32();int x0=texture.width,y0=texture.height,x1=-1,y1=-1;
   for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>128){x0=Mathf.Min(x0,x);y0=Mathf.Min(y0,y);x1=Mathf.Max(x1,x);y1=Mathf.Max(y1,y);}
   if(x1<0)throw new InvalidOperationException("Empty Oka cutout: "+name);
   int pad=3;x0=Mathf.Max(0,x0-pad);y0=Mathf.Max(0,y0-pad);x1=Mathf.Min(texture.width,x1+pad+1);y1=Mathf.Min(texture.height,y1+pad+1);
   var rect=new Rect(x0,y0,x1-x0,y1-y0);t.userData=JsonUtility.ToJson(new AlphaRect{rect=rect,width=texture.width,height=texture.height});
   if(!slot){
    float corner=name=="ToolbarBase"?rect.height*.42f:0;
    t.spriteImportMode=SpriteImportMode.Multiple;
#pragma warning disable 0618
    t.spritesheet=new[]{new SpriteMetaData{name=name,rect=rect,pivot=Vector2.one*.5f,alignment=(int)SpriteAlignment.Center,border=Vector4.one*corner}};
#pragma warning restore 0618
   }
   t.isReadable=false;t.maxTextureSize=name=="ToolbarBase"?2048:name.EndsWith("Counter")?1024:512;t.SaveAndReimport();
  }

  public static void StyleHeader(EconomyHeaderView header){
   if(header==null)throw new InvalidOperationException("Economy header missing");
   var coins=header.CoinsLabel.transform.parent.GetComponent<Image>();var energy=header.StaminaLabel.transform.parent.GetComponent<Image>();
   coins.sprite=Sprite("CoinCounter");energy.sprite=Sprite("StaminaCounter");coins.preserveAspect=energy.preserveAspect=true;
   header.CoinsPlus.image.sprite=header.StaminaPlus.image.sprite=Sprite("RefillBadge");
   header.CoinsPlus.image.preserveAspect=header.StaminaPlus.image.preserveAspect=true;
   var red=new Color(.49f,.018f,.006f);header.CoinsLabel.color=header.RecoveryLabel.color=red;
   FitLabel(header.CoinsLabel,new Vector2(.39f,.16f),new Vector2(.97f,.86f),34,18);
   FitLabel(header.StaminaLabel,new Vector2(.02f,.13f),new Vector2(.40f,.91f),34,28);
   FitLabel(header.RecoveryLabel,new Vector2(.49f,.18f),new Vector2(.96f,.85f),30,22);
   EditorUtility.SetDirty(header);
  }

  static void FitLabel(TMP_Text text,Vector2 min,Vector2 max,float size,float minimum){
   var r=text.rectTransform;r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;
   text.enableAutoSizing=true;text.fontSizeMax=size;text.fontSizeMin=minimum;text.fontSize=size;text.enableWordWrapping=false;
  }

  public static void StyleBoosterBar(HarvestBoosterBar bar){
   if(bar==null)throw new InvalidOperationException("Booster bar missing");
   var back=bar.transform.Find("ToolbarBase").GetComponent<Image>();back.sprite=Sprite("ToolbarBase");back.type=Image.Type.Sliced;back.pixelsPerUnitMultiplier=3;
   var r=back.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=new Vector2(1,.86f);r.offsetMin=r.offsetMax=Vector2.zero;
   var icons=new[]{"AddSlot","Speed","Shuffle","FreeSelect"};
   for(int i=0;i<4;i++){
    var face=bar.Buttons[i].image;face.sprite=Sprite("ButtonDisc");face.preserveAspect=true;
    r=(RectTransform)face.transform;r.anchorMin=r.anchorMax=new Vector2(.14f+i*.24f,.85f);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(142,142);
    var icon=face.transform.Find("Icon").GetComponent<Image>();icon.sprite=Sprite(icons[i]);icon.preserveAspect=true;
    r=icon.rectTransform;r.anchorMin=r.anchorMax=new Vector2(.5f,.53f);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(112,112);
    if(bar.RefillButtons!=null&&bar.RefillButtons.Length>i&&bar.RefillButtons[i]!=null){bar.RefillButtons[i].image.sprite=Sprite("RefillBadge");bar.RefillButtons[i].image.preserveAspect=true;}
    var label=bar.StateLabels[i];r=label.rectTransform;r.anchorMin=r.anchorMax=new Vector2(.5f,.08f);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(114,24);
    label.fontSize=i==1?18:20;label.enableAutoSizing=true;label.fontSizeMax=label.fontSize;label.fontSizeMin=14;label.enableWordWrapping=false;
   }
   EditorUtility.SetDirty(bar);
  }

  /// <summary>Crop only transparent texture padding in UVs; keep the existing square tile and click geometry.</summary>
  public static void ApplySlotMaterial(Material material){
   if(material==null)throw new InvalidOperationException("Slot material missing");
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(SlotTexturePath);var importer=AssetImporter.GetAtPath(SlotTexturePath);
   var data=JsonUtility.FromJson<AlphaRect>(importer.userData);float side=Mathf.Max(data.rect.width,data.rect.height);var center=data.rect.center;
   material.SetTexture("_BaseMap",texture);
   material.SetTextureScale("_BaseMap",new Vector2(side/data.width,side/data.height));
   material.SetTextureOffset("_BaseMap",new Vector2((center.x-side*.5f)/data.width,(center.y-side*.5f)/data.height));EditorUtility.SetDirty(material);
  }
 }
}
