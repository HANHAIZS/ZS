using System;
using System.Collections.Generic;
using QFramework;
using UnityEngine;
namespace BeeHarvest {
 public sealed class LevelOneAssets:IUtility {
  readonly Dictionary<string,GameObject> cache=new Dictionary<string,GameObject>();readonly Dictionary<int,LevelData> levels=new Dictionary<int,LevelData>();
  Material linkMaterial,roundedChainMaterial;
  public Material LinkMaterial(){if(linkMaterial==null)linkMaterial=Resources.Load<Material>("Materials/HarvestLink");if(linkMaterial==null)throw new InvalidOperationException("Missing connected hive material");return linkMaterial;}
  public Material RoundedChainMaterial(){if(roundedChainMaterial==null)roundedChainMaterial=Resources.Load<Material>("Materials/HarvestRoundedChain");if(roundedChainMaterial==null)throw new InvalidOperationException("Missing rounded hive chain material");return roundedChainMaterial;}
  public const int LastLevel=105;
  static readonly string[] ModelKeys={"Flower","Duck","Banana","BeeModel","Watermelon","Chick","Sheep","Shark","Broccoli","Cactustoy","Carrot","CatHead","Snake","Onion","DodoBird","CupHalf","casper","Rat","Mower","Crab","Santa","Hand_grips","Balloon_rotate","Gaming_microphone","Shuttle","Cat_Minti","Megaphone","World_cup","Spitfire","Hen_2","Oreo","Axe","Mustard","Robot_2","Cat_Luci","Dice","EmpireTower","Launcher","Gamepad","Rattle","Popsicle-chocolate","ChipGreen","Gas_station","Bell","BoxOpen","PizzaSlice","Camera","Earth","PizzaSliced","Paper_Towels","ChickenLeg","Shaker_salt","Hair_brush","Bowling_pins","Radio_4","Heart","Umbrella","Clock","Diamond_2","Saturn","Rolling_pin_2","Animal_Penguin","Tenis_ball","Liquid_soap","Bulb","Barrel","Waffle","Star","Chessboard","Castle","Bull","Bucket","Croissant","Lighthouse","Chocolate_2","Lifebuoy","Orange","pelican","Angel","Goomba","Cone","Avocado","Fluffy","Spices","Koala","Mami","Tractor_Shovel","Bear","Remote","Tree","Teepee","Dollars_container","Brain","Volcano","Toaster","Pigeon_2","Cola","iceberg","Wheelbarrow","Medieval_church","Ladybug","Maple","Willys","Scorpion","mouse_2"};
  public static bool HasVoxelEdges(int number){ValidateLevel(number);return number==2||number==5||number==6||(number>=96&&number<=105);}
  public static string SceneName(int number){ValidateLevel(number);return "Level"+number.ToString("00");}
  public static string ModelKey(int number){ValidateLevel(number);return ModelKeys[number-1];}
  static void ValidateLevel(int number){if(number<1||number>LastLevel)throw new ArgumentOutOfRangeException("number");}
  public LevelData Level(){return Level(1);}
  public LevelData Level(int number){
   LevelData value;if(levels.TryGetValue(number,out value))return value;
   var key="Levels/"+SceneName(number);var asset=Resources.Load<TextAsset>(key);
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
