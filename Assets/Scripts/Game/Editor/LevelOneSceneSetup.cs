using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using GameFoundation.UI;
namespace BeeHarvest.Editor {
 public static class LevelOneSceneSetup {
  static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
  /// <summary>Builds only level-one production assets with Unity Editor APIs.</summary>
  public static void Build(){
   Ensure("Assets/Resources/Prefabs/Gameplay");Ensure("Assets/Sprites/Gameplay/Materials");Ensure("Assets/Scenes");
   foreach(var name in new[]{"Flower","Bee","HivePink","HiveYellow","HiveGreen","Collector"}){
    var path="Assets/Sprites/Gameplay/"+name+".fbx";var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.isReadable=false;importer.importAnimation=false;importer.globalScale=1;importer.SaveAndReimport();
    var original=AssetDatabase.LoadAssetAtPath<GameObject>(path);var go=new GameObject(name);var art=UnityEngine.Object.Instantiate(original,go.transform,false);art.name=name+"_BlenderArt";var renderers=art.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);art.transform.localPosition-=bounds.center;
    foreach(var r in go.GetComponentsInChildren<Renderer>()){
     var ms=r.sharedMaterials;for(int i=0;i<ms.Length;i++){string key=ms[i]!=null?ms[i].name:"Default";Material mat;
      if(!materials.TryGetValue(key,out mat)){string asset="Assets/Sprites/Gameplay/Materials/"+key.Replace('/','_')+".mat";mat=AssetDatabase.LoadAssetAtPath<Material>(asset);if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.name=key;AssetDatabase.CreateAsset(mat,asset);}mat.SetColor("_BaseColor",Palette(key));mat.SetFloat("_Smoothness",.32f);EditorUtility.SetDirty(mat);materials[key]=mat;}ms[i]=mat;
     }r.sharedMaterials=ms;
    }
    PrefabUtility.SaveAsPrefabAsset(go,"Assets/Resources/Prefabs/Gameplay/"+name+".prefab");UnityEngine.Object.DestroyImmediate(go);
   }
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var camera=new GameObject("LevelOneCamera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.position=new Vector3(3,8.6f,-21);camera.transform.LookAt(new Vector3(0,5,0));camera.orthographic=true;camera.orthographicSize=7.2f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.045f,.085f,.18f);camera.gameObject.AddComponent<AudioListener>();
   var light=new GameObject("KeyLight").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=Quaternion.Euler(35,-25,0);light.color=new Color(1,.91f,.78f);
   RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.7f,.85f);
   var root=new GameObject("LevelOne");var controller=root.AddComponent<LevelOneController>();root.AddComponent<HarvestBoardView>();root.AddComponent<BeeFlightView>();root.AddComponent<HiveModelView>();
   var canvas=new GameObject("CoreControls",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(750,1334);scaler.matchWidthOrHeight=.5f;
   var safe=new GameObject("SafeArea",typeof(RectTransform));safe.transform.SetParent(canvas.transform,false);Set(safe.GetComponent<RectTransform>(),0,0,1,1);safe.AddComponent<UiSafeAreaFitter>();
   var hud=safe.AddComponent<LevelOneHud>();controller.Hud=hud;
   Label(safe.transform,"LevelLabel","Level 1",.08f,.9f,.92f,.965f,36);
   hud.SlotLabels=new Text[5];hud.QueueLabels=new Text[5];hud.NextLabels=new Text[5];hud.QueueImages=new Image[5];hud.QueueButtons=new Button[5];
   for(int i=0;i<5;i++){
    float x=.025f+i*.195f;
    hud.SlotLabels[i]=Label(safe.transform,"Slot_"+i,"空槽",x,.205f,x+.17f,.235f,24);
    var go=new GameObject("HiveColumn_"+i,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(safe.transform,false);Set(go.GetComponent<RectTransform>(),x,.085f,x+.17f,.19f);hud.QueueImages[i]=go.GetComponent<Image>();hud.QueueButtons[i]=go.GetComponent<Button>();hud.QueueButtons[i].targetGraphic=hud.QueueImages[i];hud.QueueLabels[i]=Label(go.transform,"Quota","0",0,0,1,1,38);hud.QueueLabels[i].color=new Color(.09f,.12f,.2f);
    hud.NextLabels[i]=Label(safe.transform,"Next_"+i,"",x,.05f,x+.17f,.08f,18);
   }
   new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
   PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;PlayerSettings.allowedAutorotateToLandscapeLeft=false;PlayerSettings.allowedAutorotateToLandscapeRight=false;PlayerSettings.productName="BeeHarvest";
   var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/GameFoundation/LevelOnePipeline.asset");if(pipeline==null){var renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,"Assets/Settings/GameFoundation/LevelOneRenderer.asset");pipeline=UniversalRenderPipelineAsset.Create(renderer);AssetDatabase.CreateAsset(pipeline,"Assets/Settings/GameFoundation/LevelOnePipeline.asset");}GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
   if(System.IO.File.Exists(LevelOneBackgroundSetup.TexturePath))LevelOneBackgroundSetup.Apply();
   LevelOneTitleSetup.Apply();
   LevelOneSmallHiveSetup.Apply();
   if(System.IO.File.Exists(LevelOneHiveClickSetup.TexturePath))LevelOneHiveClickSetup.Apply();
   if(System.IO.File.Exists(LevelOneSettingsSetup.ArtRoot+"SettingsGear.png"))LevelOneSettingsSetup.Apply();
   EditorSceneManager.SaveScene(scene,"Assets/Scenes/Level01.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Level01.unity",true)};AssetDatabase.SaveAssets();Debug.Log("LEVEL_ONE_SCENE_READY");
  }
  /// <summary>Creates required production folders without touching legacy paths.</summary>
  static void Ensure(string path){if(AssetDatabase.IsValidFolder(path))return;int slash=path.LastIndexOf('/');Ensure(path.Substring(0,slash));AssetDatabase.CreateFolder(path.Substring(0,slash),path.Substring(slash+1));}
  /// <summary>Maps Blender material identity to the compact first-level palette.</summary>
  static Color Palette(string name){string n=name.ToLowerInvariant();if(n.Contains("levelone_pink"))return LevelOneAssets.ColorOf("pink");if(n.Contains("levelone_green")||n.Contains("mint")||n.Contains("green")||n.Contains("teal"))return LevelOneAssets.ColorOf("green");if(n.Contains("charcoal")||n.Contains("dark")||n.Contains("shadow"))return new Color(.075f,.055f,.1f);if(n.Contains("white"))return Color.white;if(n.Contains("wing"))return new Color(.6f,.88f,1);if(n.Contains("pink")||n.Contains("coral")||n.Contains("rose"))return LevelOneAssets.ColorOf("pink");return LevelOneAssets.ColorOf("yellow");}
  /// <summary>Builds anchored uGUI text for essential gameplay feedback.</summary>
  static Text Label(Transform parent,string name,string value,float x0,float y0,float x1,float y1,int size){var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);Set(go.GetComponent<RectTransform>(),x0,y0,x1,y1);var t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.raycastTarget=false;return t;}
  /// <summary>Sets exact anchors and clears fixed offsets.</summary>
  static void Set(RectTransform t,float x0,float y0,float x1,float y1){t.anchorMin=new Vector2(x0,y0);t.anchorMax=new Vector2(x1,y1);t.offsetMin=Vector2.zero;t.offsetMax=Vector2.zero;}
 }
}
