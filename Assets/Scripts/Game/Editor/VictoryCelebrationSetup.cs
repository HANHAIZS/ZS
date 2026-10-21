using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace BeeHarvest.Editor {
 public static class VictoryCelebrationSetup {
  const string Imported="Assets/Sprites/DesignImports/EpicToonFX/";
  const string FxPath="Assets/Sprites/Prefabs/UI/VictoryCelebration.prefab";
  const string PopupPath="Assets/Sprites/Prefabs/UI/VictoryPopup.prefab";
  static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max){var go=new GameObject(name,typeof(RectTransform));var r=(RectTransform)go.transform;r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
  static void Particle(Transform parent,string source,string name,Vector2 anchor,bool confetti,bool right){
   var original=AssetDatabase.LoadAssetAtPath<GameObject>(Imported+source).GetComponent<ParticleSystem>();
   var rect=Rect(name,parent,anchor,anchor);var ps=rect.gameObject.AddComponent<ParticleSystem>();EditorUtility.CopySerialized(original,ps);ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
   var renderer=ps.GetComponent<ParticleSystemRenderer>();EditorUtility.CopySerialized(original.GetComponent<ParticleSystemRenderer>(),renderer);renderer.enabled=false;renderer.alignment=ParticleSystemRenderSpace.Local;
   var main=ps.main;main.playOnAwake=false;main.useUnscaledTime=true;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.scalingMode=ParticleSystemScalingMode.Local;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;main.stopAction=ParticleSystemStopAction.None;main.maxParticles=confetti?64:24;main.loop=!confetti;main.duration=confetti?.25f:1f;
   var shape=ps.shape;shape.shapeType=confetti?ParticleSystemShapeType.Cone:ParticleSystemShapeType.Circle;shape.radius=confetti?.01f:3.2f;shape.rotation=confetti?new Vector3(-90,0,right?25:-25):Vector3.zero;if(confetti)shape.angle=28;
   var emission=ps.emission;emission.enabled=true;emission.rateOverTime=confetti?0:10;
   emission.SetBursts(confetti?new[]{new ParticleSystem.Burst(0,54)}:new ParticleSystem.Burst[0]);
   if(confetti){var limit=ps.limitVelocityOverLifetime;limit.enabled=false;main.startLifetime=new ParticleSystem.MinMaxCurve(2.3f,3f);main.startSpeed=new ParticleSystem.MinMaxCurve(3.5f,5.5f);main.startSize=new ParticleSystem.MinMaxCurve(.10f,.18f);main.gravityModifier=.6f;}
   rect.gameObject.AddComponent<CanvasRenderer>();var graphic=rect.gameObject.AddComponent<VictoryParticleGraphic>();graphic.ParticleTexture=original.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture;graphic.raycastTarget=false;
  }
  /// <summary>Follows the drawn sprite area, including letterboxing on tall portrait screens.</summary>
  public static void PositionReward(Transform presentation){
   var artwork=presentation.Find("VictoryArtwork").GetComponent<Image>();
   var existing=artwork.transform.Find("RewardArea");
   var area=existing!=null?(RectTransform)existing:Rect("RewardArea",artwork.transform,Vector2.zero,Vector2.one);
   var fit=area.GetComponent<AspectRatioFitter>();if(fit==null)fit=area.gameObject.AddComponent<AspectRatioFitter>();
   fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=artwork.sprite.rect.width/artwork.sprite.rect.height;
   var reward=presentation.GetComponentsInChildren<TextMeshProUGUI>(true).Single(t=>t.name=="RewardAmount").rectTransform;
   reward.SetParent(area,false);reward.anchorMin=new Vector2(.63f,.075f);reward.anchorMax=new Vector2(.83f,.235f);
   reward.offsetMin=reward.offsetMax=Vector2.zero;reward.localScale=Vector3.one;reward.localRotation=Quaternion.identity;
  }
  public static string Apply(){
   var root=new GameObject("VictoryCelebration",typeof(RectTransform),typeof(VictoryCelebrationView));var rr=(RectTransform)root.transform;rr.anchorMin=Vector2.zero;rr.anchorMax=Vector2.one;rr.offsetMin=rr.offsetMax=Vector2.zero;
   try{Particle(root.transform,"Prefabs/Environment/Confetti/Blast/ConfettiBlastRainbow.prefab","ConfettiLeft",new Vector2(.10f,.60f),true,false);Particle(root.transform,"Prefabs/Environment/Confetti/Blast/ConfettiBlastRainbow.prefab","ConfettiRight",new Vector2(.90f,.60f),true,true);Particle(root.transform,"Prefabs/Interactive/Sparkle/SparkleArea/SparkleAreaYellow.prefab","GoldenSparkles",new Vector2(.5f,.57f),false,false);PrefabUtility.SaveAsPrefabAsset(root,FxPath);}finally{UnityEngine.Object.DestroyImmediate(root);}
   var popup=PrefabUtility.LoadPrefabContents(PopupPath);
   try{
    var controller=popup.GetComponent<VictoryPopupController>();var presentation=controller.Presentation;
    if(controller.Celebration==null){var fx=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(FxPath),presentation);fx.transform.SetAsFirstSibling();controller.Celebration=fx.GetComponent<VictoryCelebrationView>();}
    var existing=presentation.GetComponentsInChildren<TextMeshProUGUI>(true).SingleOrDefault(t=>t.name=="RewardAmount");var reward=existing!=null?existing:Rect("RewardAmount",presentation,new Vector2(.1f,.175f),new Vector2(.9f,.275f)).gameObject.AddComponent<TextMeshProUGUI>();
    reward.text="60";reward.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Sprites/UI/Fonts/FredokaBoldSDF.asset");reward.fontSize=84;reward.fontStyle=FontStyles.Bold;reward.alignment=TextAlignmentOptions.Center;reward.color=Color.white;reward.raycastTarget=false;reward.enableWordWrapping=false;
    PositionReward(presentation);
    PrefabUtility.SaveAsPrefabAsset(popup,PopupPath);
   }finally{PrefabUtility.UnloadPrefabContents(popup);}
   var importer=(AudioImporter)AssetImporter.GetAtPath("Assets/Resources/Audio/Sfx/Victory.wav");var sample=importer.defaultSampleSettings;sample.loadType=AudioClipLoadType.DecompressOnLoad;sample.compressionFormat=AudioCompressionFormat.PCM;sample.preloadAudioData=true;importer.defaultSampleSettings=sample;importer.forceToMono=false;importer.SaveAndReimport();AssetDatabase.SaveAssets();return FxPath;
  }
 }
}
