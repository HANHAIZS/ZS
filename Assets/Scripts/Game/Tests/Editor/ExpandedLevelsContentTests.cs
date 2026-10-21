using System;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class ExpandedLevelsContentTests : EconomyTestFixture {
  [TestCase(16,"CupHalf",996,14,3)]
  [TestCase(17,"casper",1339,26,3)]
  [TestCase(18,"Rat",1416,27,3)]
  [TestCase(19,"Mower",1471,23,3)]
  [TestCase(20,"Crab",1585,28,3)]
  [TestCase(21,"Santa",1695,31,3)]
  [TestCase(22,"Hand_grips",1821,30,3)]
  [TestCase(23,"Balloon_rotate",1876,30,3)]
  [TestCase(24,"Gaming_microphone",1909,32,3)]
  [TestCase(25,"Shuttle",1920,72,3)]
  [TestCase(26,"Cat_Minti",1923,29,3)]
  [TestCase(27,"Megaphone",1949,33,3)]
  [TestCase(28,"World_cup",1964,120,3)]
  [TestCase(29,"Spitfire",1968,32,3)]
  [TestCase(30,"Hen_2",2017,31,3)]
  [TestCase(31,"Oreo",2068,36,3)]
  [TestCase(32,"Axe",2096,36,3)]
  [TestCase(33,"Mustard",2144,30,3)]
  [TestCase(34,"Robot_2",2155,127,3)]
  [TestCase(35,"Cat_Luci",2471,106,3)]
  [TestCase(36,"Dice",2197,38,3)]
  [TestCase(37,"EmpireTower",2300,41,3)]
  [TestCase(38,"Launcher",2304,35,3)]
  [TestCase(39,"Gamepad",2307,35,3)]
  [TestCase(40,"Rattle",2308,75,3)]
  [TestCase(41,"Popsicle-chocolate",2320,84,3)]
  [TestCase(42,"ChipGreen",2324,110,3)]
  [TestCase(43,"Gas_station",2339,68,3)]
  [TestCase(44,"Bell",2356,83,3)]
  [TestCase(45,"BoxOpen",2381,46,3)]
  [TestCase(46,"PizzaSlice",2395,63,3)]
  [TestCase(47,"Camera",2398,181,3)]
  [TestCase(48,"Earth",2400,44,3)]
  [TestCase(49,"PizzaSliced",2400,35,3)]
  [TestCase(50,"Paper_Towels",2492,70,3)]
  [TestCase(51,"ChickenLeg",2504,140,3)]
  [TestCase(52,"Shaker_salt",2512,120,3)]
  [TestCase(53,"Hair_brush",2533,54,3)]
  [TestCase(54,"Bowling_pins",2560,129,3)]
  [TestCase(55,"Radio_4",2567,96,3)]
  [TestCase(56,"Heart",2570,124,3)]
  [TestCase(57,"Umbrella",2576,95,3)]
  [TestCase(58,"Clock",2604,37,3)]
  [TestCase(59,"Diamond_2",2625,183,3)]
  [TestCase(60,"Saturn",2652,59,3)]
  [TestCase(61,"Rolling_pin_2",2660,37,3)]
  [TestCase(62,"Animal_Penguin",2690,101,3)]
  [TestCase(63,"Tenis_ball",2717,111,3)]
  [TestCase(64,"Liquid_soap",2764,109,3)]
  [TestCase(65,"Bulb",2765,138,3)]
  [TestCase(66,"Barrel",2772,87,3)]
  [TestCase(67,"Waffle",2801,45,3)]
  [TestCase(68,"Star",2806,45,3)]
  [TestCase(69,"Chessboard",2864,74,3)]
  [TestCase(70,"Castle",2880,39,3)]
  [TestCase(71,"Bull",2891,124,3)]
  [TestCase(72,"Bucket",2902,85,3)]
  [TestCase(73,"Croissant",2944,46,3)]
  [TestCase(74,"Lighthouse",2984,171,3)]
  [TestCase(75,"Chocolate_2",3001,59,3)]
  [TestCase(76,"Lifebuoy",3032,64,3)]
  [TestCase(77,"Orange",3067,121,3)]
  [TestCase(78,"pelican",3098,113,3)]
  [TestCase(79,"Angel",3102,101,3)]
  [TestCase(80,"Goomba",3159,179,3)]
  [TestCase(81,"Cone",3170,199,3)]
  [TestCase(82,"Avocado",3198,58,3)]
  [TestCase(83,"Fluffy",3199,87,3)]
  [TestCase(84,"Spices",3209,129,3)]
  [TestCase(85,"Koala",3215,194,3)]
  [TestCase(86,"Mami",3270,99,3)]
  [TestCase(87,"Tractor_Shovel",3295,95,3)]
  [TestCase(88,"Bear",3300,93,3)]
  [TestCase(89,"Remote",3448,52,3)]
  [TestCase(90,"Tree",3485,107,3)]
  [TestCase(91,"Teepee",3490,94,3)]
  [TestCase(92,"Dollars_container",3516,132,3)]
  [TestCase(93,"Brain",3561,112,3)]
  [TestCase(94,"Volcano",3618,135,3)]
  [TestCase(95,"Toaster",3627,127,3)]
  public void EveryAdditionalModelIsReachableWithItsOwnQuotaAndPalette(int number,string name,int cubes,int hives,int columns){
   var assets=new LevelOneAssets();LevelData data=null;Assert.DoesNotThrow(()=>data=assets.Level(number));
   Assert.AreEqual(name,data.sourceName);Assert.AreEqual(cubes,data.cubes.Length);Assert.AreEqual(hives,data.hives.Length);Assert.AreEqual(columns,data.columns);
   Assert.AreEqual(name,LevelOneAssets.ModelKey(number));Assert.AreEqual(cubes,data.cubes.Select(c=>new Vector3Int(c.x,c.y,c.z)).Distinct().Count());
   foreach(var color in data.cubes.GroupBy(c=>c.color)){Assert.AreEqual(color.Count(),data.hives.Where(h=>h.color==color.Key).Sum(h=>h.quota));Assert.DoesNotThrow(()=>LevelOneAssets.ColorOf(color.Key));Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.BeeKey(color.Key)));Assert.IsNotNull(Resources.Load<GameObject>("Prefabs/Gameplay/"+LevelOneAssets.HiveKey(color.Key)));}
   var prefab=Resources.Load<GameObject>("Prefabs/Gameplay/"+name);Assert.IsNotNull(prefab);var parts=prefab.GetComponentsInChildren<Renderer>(true);Assert.AreEqual(cubes,parts.Length);Assert.IsTrue(parts.All(p=>p.sharedMaterial!=null&&p.GetComponent<MeshFilter>().sharedMesh!=null));
  }
  [TestCase(15,16,false)] [TestCase(16,17,false)] [TestCase(94,95,false)] [TestCase(95,96,false)]
  public void ExpandedProgressionAdvancesOnceAndOnlyTheNewFinalLevelWraps(int from,int to,bool home){
   var app=BeeHarvestApp.Interface;Assert.DoesNotThrow(()=>app.SendCommand(new InitializeMenuNavigationCommand(false,from)));
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));var model=app.GetModel<HarvestModel>();var nav=app.GetModel<MenuNavigationModel>();int stamina=app.GetModel<EconomyModel>().Stamina;
   app.SendCommand(new ContinueLevelCommand(from,model.Generation));Assert.AreEqual(from,nav.CurrentLevel);app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(model.Won);app.SendCommand(new ContinueLevelCommand(from,model.Generation-1));Assert.AreEqual(from,nav.CurrentLevel);
   var command=new ContinueLevelCommand(from,model.Generation);app.SendCommand(command);app.SendCommand(command);Assert.AreEqual(to,nav.CurrentLevel);Assert.AreEqual(home,nav.IsHome);Assert.AreEqual(stamina,app.GetModel<EconomyModel>().Stamina);
  }
  [Test] public void UnknownLevelAfterCatalogIsRejected(){Assert.Throws<ArgumentOutOfRangeException>(()=>new LevelOneAssets().Level(LevelOneAssets.LastLevel+1));}
 }
}
