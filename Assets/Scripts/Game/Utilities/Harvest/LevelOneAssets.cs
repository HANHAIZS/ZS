using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
namespace BeeHarvest {
 public sealed class LevelOneAssets:IUtility {
  readonly Dictionary<string,GameObject> cache=new Dictionary<string,GameObject>();readonly Dictionary<int,LevelData> levels=new Dictionary<int,LevelData>();
  Material linkMaterial,roundedChainMaterial;
  public Material LinkMaterial(){if(linkMaterial==null)linkMaterial=Resources.Load<Material>("Materials/HarvestLink");if(linkMaterial==null)throw new InvalidOperationException("Missing connected hive material");return linkMaterial;}
  public Material RoundedChainMaterial(){if(roundedChainMaterial==null)roundedChainMaterial=Resources.Load<Material>("Materials/HarvestRoundedChain");if(roundedChainMaterial==null)throw new InvalidOperationException("Missing rounded hive chain material");return roundedChainMaterial;}
  public const int LastLevel=119;
  public const string GameSceneName="Game",GameScenePath="Assets/Scenes/Game.unity";
  [Serializable] sealed class PresentationFile { public int version;public LevelPresentationData[] entries; }
  static LevelPresentationData[] presentations;
  public static LevelPresentationData Presentation(int number){ValidateLevel(number);if(presentations==null)ReloadPresentationCatalog();return presentations[number-1];}
  /// <summary>Editor import/preview can explicitly refresh data; failed validation never publishes a partial catalog.</summary>
  public static void ReloadPresentationCatalog(){
   var asset=Resources.Load<TextAsset>("Levels/PresentationCatalog");if(asset==null)throw new InvalidOperationException("Missing level presentation catalog");
   presentations=ParsePresentationCatalog(asset.text);
  }
  /// <summary>Validates schema before Unity maps data, so omitted false/zero-valued fields cannot silently default.</summary>
  public static LevelPresentationData[] ParsePresentationCatalog(string json){
   if(string.IsNullOrWhiteSpace(json))throw new InvalidOperationException("Missing level presentation JSON");
   JObject root;try{root=JObject.Parse(json);}catch(Newtonsoft.Json.JsonException error){throw new InvalidOperationException("Invalid level presentation JSON",error);}
   if((double)Require(root,"version",JTokenType.Integer)!=1)throw new InvalidOperationException("Unknown level presentation version");
   var entries=(JArray)Require(root,"entries",JTokenType.Array);if(entries.Count!=LastLevel)throw new InvalidOperationException("Incomplete level presentation catalog");
   var seen=new System.Collections.Generic.HashSet<int>();
   foreach(var token in entries){
    var record=token as JObject;if(record==null)throw new InvalidOperationException("Invalid level presentation record");
    double value=(double)Require(record,"number",JTokenType.Integer);if(value<1||value>LastLevel||!seen.Add((int)value))throw new InvalidOperationException("Invalid or duplicate level presentation number");
    string key=(string)Require(record,"modelKey",JTokenType.String);if(string.IsNullOrWhiteSpace(key)||!key.All(c=>char.IsLetterOrDigit(c)||c=='_'||c=='-'))throw new InvalidOperationException("Invalid presentation model key");
    foreach(string field in new[]{"edges","separation","cavity","overrideEdgeColor","sourceHandoff","softShadows"})Require(record,field,JTokenType.Boolean);
    foreach(string field in new[]{"edgeColor","ambientLight"}){var color=(JObject)Require(record,field,JTokenType.Object);foreach(string channel in new[]{"r","g","b","a"})RequireUnitNumber(color,channel);}
    foreach(string field in new[]{"shadowStrength","shadowBias","shadowNormalBias"})RequireUnitNumber(record,field);
    OptionalNumber(record,"modelScaleMultiplier",1,.8,1.5);OptionalNumber(record,"modelCenterY",.66,.5,.75);
    OptionalNumber(record,"slotYOffset",0,-.1,0);OptionalNumber(record,"queueFrontY",0,0,.35);OptionalNumber(record,"queueRowPitch",0,0,.1);
    OptionalNumber(record,"queueColumnPitch",0,0,.30);OptionalNumber(record,"slotColumnPitch",0,0,.195);
    double queueColumn=(double)record["queueColumnPitch"],slotColumn=(double)record["slotColumnPitch"];
    if((queueColumn>0&&queueColumn<.20)||(slotColumn>0&&slotColumn<.15))throw new InvalidOperationException("Unsafe horizontal hive spacing");
    double front=(double)record["queueFrontY"],pitch=(double)record["queueRowPitch"];
    if((front==0)!=(pitch==0)||(front>0&&(pitch<.065||front-2*pitch<.125)))throw new InvalidOperationException("Unsafe hive queue spacing");
   }
   var file=JsonUtility.FromJson<PresentationFile>(root.ToString());var records=new LevelPresentationData[LastLevel];foreach(var entry in file.entries)records[entry.number-1]=entry;return records;
  }
  static void OptionalNumber(JObject source,string name,double fallback,double min,double max){
   var token=source[name];if(token==null){source[name]=fallback;return;}
   if(token.Type!=JTokenType.Float&&token.Type!=JTokenType.Integer)throw new InvalidOperationException("Invalid display number: "+name);
   double n=(double)token;if(double.IsNaN(n)||double.IsInfinity(n)||n<min||n>max)throw new InvalidOperationException("Display number out of range: "+name);
  }
  static JToken Require(JObject source,string name,JTokenType type){var value=source[name];if(value==null||value.Type!=type)throw new InvalidOperationException("Missing or invalid presentation field: "+name);return value;}
  static void RequireUnitNumber(JObject source,string name){var value=source[name];if(value==null||(value.Type!=JTokenType.Float&&value.Type!=JTokenType.Integer))throw new InvalidOperationException("Missing presentation number: "+name);double number=(double)value;if(double.IsNaN(number)||double.IsInfinity(number)||number<0||number>1)throw new InvalidOperationException("Presentation number outside0–1: "+name);}
  public static bool HasVoxelSeparation(int number){return Presentation(number).separation;}
  public static bool HasVoxelCavity(int number){return Presentation(number).cavity;}
  public static bool HasVoxelEdges(int number){return Presentation(number).edges;}
  public static Color? VoxelEdgeColor(int number){var data=Presentation(number);return data.overrideEdgeColor?(Color?)data.edgeColor:null;}
  public static string LevelKey(int number){ValidateLevel(number);return "Level"+number.ToString("00");}
  public static string ModelKey(int number){ValidateLevel(number);return Presentation(number).modelKey;}
  static void ValidateLevel(int number){if(number<1||number>LastLevel)throw new ArgumentOutOfRangeException("number");}
  public LevelData Level(){return Level(1);}
  public LevelData Level(int number){
   LevelData value;if(levels.TryGetValue(number,out value))return value;
   var key="Levels/"+LevelKey(number);var asset=Resources.Load<TextAsset>(key);
   if(asset==null)throw new InvalidOperationException("Missing level data "+key);
   value=JsonUtility.FromJson<LevelData>(asset.text);levels.Add(number,value);return value;
  }
  public GameObject Spawn(string key,Transform parent){GameObject p;if(!cache.TryGetValue(key,out p)){p=Resources.Load<GameObject>("Prefabs/Gameplay/"+key);if(p==null)throw new InvalidOperationException("Missing Blender prefab "+key);cache.Add(key,p);}return UnityEngine.Object.Instantiate(p,parent);}
  public static string PaletteKey(string id){if(id=="pink")return "Red";if(string.IsNullOrEmpty(id))throw new ArgumentException("Missing palette color");return char.ToUpperInvariant(id[0])+id.Substring(1).ToLowerInvariant();}
  /// <summary>Shares the exact Blender nine-color palette across voxels, bees and source hives.</summary>
  public static Color ColorOf(string id){
   Color source;if(CubeLandPalette.TryColor(id,out source))return source;
   string hex;switch(PaletteKey(id)){case "Red":hex="#F23B32";break;case "Orange":hex="#FF8B19";break;case "Yellow":hex="#FFD52A";break;case "Green":hex="#39CC35";break;case "Cyan":hex="#1ADAD0";break;case "Blue":hex="#367EF2";break;case "Purple":hex="#A644ED";break;case "White":hex="#F8F7F2";break;case "Black":hex="#303438";break;default:throw new ArgumentException("Unknown bee palette "+id);}
   Color c;ColorUtility.TryParseHtmlString(hex,out c);return c;
  }
  public static string HiveKey(string id){return "Hive"+PaletteKey(id);}
  public static string BeeKey(string id){return "Bee"+PaletteKey(id);}
 }
}
