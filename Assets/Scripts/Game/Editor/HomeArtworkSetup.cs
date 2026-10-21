using System;
using TMPro;
using UnityEditor;
using UnityEngine;
namespace BeeHarvest.Editor {
 public static class HomeArtworkSetup {
  public const string SpritePath="Assets/Sprites/UI/MainMenu/HomeBackgroundOka.png";
  const string PrefabPath="Assets/Resources/Prefabs/UI/MainMenuPage.prefab";
  [MenuItem("BeeHarvest/UI/Apply Home Level Button")]
  public static void Apply(){
   if(Application.isPlaying||EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode before applying Home UI.");
   var root=PrefabUtility.LoadPrefabContents(PrefabPath);
   try{var page=root.GetComponent<MainMenuPage>();Build(page,root.transform.Find("Content/SafeArea"),page.StartGameButton.GetComponentInChildren<TMP_Text>(true).font);PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);}
   finally{PrefabUtility.UnloadPrefabContents(root);}
   AssetDatabase.SaveAssets();
   var profile=AssetDatabase.LoadAssetAtPath<GameFoundation.UI.UiPrefabConventionProfile>("Assets/Settings/GameFoundation/BeeHarvestUiPrefabConvention.asset");
   var report=GameFoundation.UI.Editor.UiPrefabValidator.Validate(PrefabPath,profile,GameFoundation.UI.UiPrefabKind.Page);
   if(!report.IsValid)throw new InvalidOperationException(report.ToMultilineString());
  }
  public static void StyleLevelButton(TMP_Text label,TMP_FontAsset font){
   const string path="Assets/Sprites/UI/MainMenu/LevelButtonLabel.mat";
   var material=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(material==null){material=new Material(font.material){name="LevelButtonLabel"};AssetDatabase.CreateAsset(material,path);}
   material.SetColor("_FaceColor",Color.white);material.SetColor("_OutlineColor",new Color(0,106f/255,11f/255));
   material.SetFloat("_OutlineWidth",.2f);material.SetFloat("_OutlineSoftness",0);material.EnableKeyword("OUTLINE_ON");EditorUtility.SetDirty(material);
   label.font=font;label.color=Color.white;label.fontSharedMaterial=material;label.UpdateMeshPadding();
  }
  public static void Build(MainMenuPage page,Transform safe,TMP_FontAsset font){
   var importer=AssetImporter.GetAtPath(SpritePath) as TextureImporter;
   if(importer==null)throw new InvalidOperationException("Home background is missing: "+SpritePath);
   importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.isReadable=false;importer.maxTextureSize=2048;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
   var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);if(sprite==null)throw new InvalidOperationException("Home background Sprite is missing");
   foreach(char c in "level 12")if(!font.characterLookupTable.ContainsKey(c))throw new InvalidOperationException("level button font glyph is missing: "+c);
   var old=safe.Find("HomeArtwork");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
   var tabs=page.Tabs;tabs.HomeBackground=sprite;tabs.Background.sprite=sprite;
   var fitter=tabs.Background.GetComponent<UnityEngine.UI.AspectRatioFitter>();if(fitter!=null)fitter.aspectRatio=sprite.rect.width/sprite.rect.height;
   var label=page.StartGameButton.GetComponentInChildren<TMP_Text>(true);if(label==null)throw new InvalidOperationException("Start button level label is missing");
   label.text="level 1";label.raycastTarget=false;StyleLevelButton(label,font);tabs.StartLevelLabel=label;
   page.SettingsButton.gameObject.SetActive(false);
  }
 }
}
