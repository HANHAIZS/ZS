using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
namespace BeeHarvest.Editor {
 public static class LeavingDialogSetup {
  public const string ArtRoot="Assets/Sprites/UI/LeavingDialog/";
  const float Scale=740f/758f;
  [MenuItem("BeeHarvest/UI/Apply Leaving Dialog")]
  public static void Apply(){
   if(Application.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
   ImportArt();
   var root=PrefabUtility.LoadPrefabContents(LevelOneSettingsSetup.PrefabPath);
   try {Build(root.GetComponent<SettingsPopupController>());PrefabUtility.SaveAsPrefabAsset(root,LevelOneSettingsSetup.PrefabPath);}
   finally {PrefabUtility.UnloadPrefabContents(root);}
   var profile=AssetDatabase.LoadAssetAtPath<GameFoundation.UI.UiPrefabConventionProfile>("Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset");
   var rule=profile.FindRule(GameFoundation.UI.UiPrefabKind.Popup);
   if(!rule.OptionalRootChildren.Contains("LeavingContent")){rule.OptionalRootChildren=rule.OptionalRootChildren.Concat(new[]{"LeavingContent"}).ToArray();EditorUtility.SetDirty(profile);}
   AssetDatabase.SaveAssets();
   var report=GameFoundation.UI.Editor.UiPrefabValidator.Validate(LevelOneSettingsSetup.PrefabPath,profile,GameFoundation.UI.UiPrefabKind.Popup);
   if(!report.IsValid)throw new InvalidOperationException(report.ToMultilineString());
  }
  public static void Build(SettingsPopupController popup){
   var old=popup.transform.Find("LeavingContent");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
   var content=OkaSettingsSetup.Rect("LeavingContent",popup.transform);
   Place(content,new Vector2(758,990),Vector2.zero);
   content.gameObject.AddComponent<PausedDialogFit>();
   var background=OkaSettingsSetup.Image("PanelBackground",content,PauseArt("Background"));background.raycastTarget=true;
   Place(background.rectTransform,new Vector2(723,959),new Vector2(-7.5f,-24.5f));
   var header=OkaSettingsSetup.Image("TitleBanner",content,PauseArt("Header"));Place(header.rectTransform,new Vector2(482,170),new Vector2(-6,410));
   var font=Font();
   Label(header.transform,"Title","Leaving",font,new Vector2(432,125),new Vector2(0,8),90);
   var panel=OkaSettingsSetup.Image("LifePanel",content,Art("Inset"));Place(panel.rectTransform,new Vector2(604,438),new Vector2(-7,144));
   var heart=OkaSettingsSetup.Image("BrokenHeart",panel.transform,Art("BrokenHeart"));Place(heart.rectTransform,new Vector2(290,262),new Vector2(0,0));
   Label(panel.transform,"LifeCost","-1",font,new Vector2(145,120),new Vector2(100,-90),100);
   Label(content,"Message","You will lose 1 life",font,new Vector2(630,100),new Vector2(-7,-153),51);
   var leave=OkaSettingsSetup.Button("LeaveButton",content,Art("LeaveButton"));Place((RectTransform)leave.transform,new Vector2(484,180),new Vector2(-7,-329));
   Label(leave.transform,"Label","Leave",font,new Vector2(390,130),new Vector2(0,8),90);
   var close=OkaSettingsSetup.Button("CancelLeaveButton",content,PauseArt("Close"));Place((RectTransform)close.transform,new Vector2(119,142),new Vector2(310.5f,376));
   popup.LeavingContent=content.gameObject;popup.LeaveButton=leave;popup.CancelLeaveButton=close;
   content.gameObject.SetActive(false);EditorUtility.SetDirty(popup);
  }
  static void Place(RectTransform r,Vector2 size,Vector2 position){r.anchorMin=r.anchorMax=r.pivot=Vector2.one*.5f;r.sizeDelta=size*Scale;r.anchoredPosition=position*Scale;}
  static void Label(Transform parent,string name,string text,TMP_FontAsset font,Vector2 size,Vector2 position,float pointSize){
   var label=OkaSettingsSetup.Rect(name,parent).gameObject.AddComponent<TextMeshProUGUI>();Place(label.rectTransform,size,position);
   label.font=font;label.fontSharedMaterial=font.material;label.text=text;label.fontSize=pointSize*Scale;label.color=new Color(1,.985f,.96f);label.alignment=TextAlignmentOptions.Center;label.enableWordWrapping=false;label.raycastTarget=false;label.UpdateMeshPadding();
  }
  static Sprite PauseArt(string name){return AssetDatabase.LoadAssetAtPath<Sprite>(LevelPauseSetup.ArtRoot+name+".png");}
  static Sprite Art(string name){return AssetDatabase.LoadAllAssetsAtPath(ArtRoot+name+".png").OfType<Sprite>().Single();}
  public static void ImportArt(){
   Import("Inset",new Rect(0,0,564,409),1024);
   Import("BrokenHeart",new Rect(120,180,1012,914),512);
   Import("LeaveButton",new Rect(0,0,2056,765),1024);
  }
  static void Import(string name,Rect rect,int maximum){
   string path=ArtRoot+name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
   var importer=(TextureImporter)AssetImporter.GetAtPath(path);if(importer==null)throw new InvalidOperationException("Leaving art missing: "+name);
   importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.isReadable=false;importer.maxTextureSize=maximum;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.CompressedHQ;
#pragma warning disable 0618
   importer.spritesheet=new[]{new SpriteMetaData{name=name,rect=rect,alignment=(int)SpriteAlignment.Center,pivot=Vector2.one*.5f}};
#pragma warning restore 0618
   EditorUtility.SetDirty(importer);AssetDatabase.WriteImportSettingsIfDirty(path);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
  }
  static TMP_FontAsset Font(){
   string path=ArtRoot+"LeavingFont.asset";var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
   if(font==null){
    var source=AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/Sprites/UI/Fonts/Fredoka-Bold.ttf");
    font=TMP_FontAsset.CreateFontAsset(source,90,12,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,false);
    if(font==null||!font.TryAddCharacters("LeavingLeaveYou will lose 1 life-1 _"))throw new InvalidOperationException("Leaving glyph bake failed");
    font.name="LeavingFont";font.atlasPopulationMode=AtlasPopulationMode.Static;AssetDatabase.CreateAsset(font,path);
    font.material.name="LeavingFont Material";AssetDatabase.AddObjectToAsset(font.material,font);
    foreach(var atlas in font.atlasTextures){atlas.name="LeavingFont Atlas";AssetDatabase.AddObjectToAsset(atlas,font);}
   }
   font.material.SetColor("_FaceColor",Color.white);font.material.SetColor("_OutlineColor",new Color(.25f,.005f,.015f));font.material.SetFloat("_OutlineWidth",.16f);font.material.EnableKeyword("OUTLINE_ON");EditorUtility.SetDirty(font);EditorUtility.SetDirty(font.material);return font;
  }
 }
}
