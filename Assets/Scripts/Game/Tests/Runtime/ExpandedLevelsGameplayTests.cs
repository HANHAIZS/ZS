using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BeeHarvest.Tests {
 public sealed class ExpandedLevelsGameplayTests : EconomyTestFixture {
  static readonly string[] Names={"CupHalf","casper","Rat","Mower","Crab","Santa","Hand_grips","Balloon_rotate","Gaming_microphone","Shuttle","Cat_Minti","Megaphone","World_cup","Spitfire","Hen_2","Oreo","Axe","Mustard","Robot_2","Cat_Luci","Dice","EmpireTower","Launcher","Gamepad","Rattle","Popsicle-chocolate","ChipGreen","Gas_station","Bell","BoxOpen","PizzaSlice","Camera","Earth","PizzaSliced","Paper_Towels","ChickenLeg","Shaker_salt","Hair_brush","Bowling_pins","Radio_4","Heart","Umbrella","Clock","Diamond_2","Saturn","Rolling_pin_2","Animal_Penguin","Tenis_ball","Liquid_soap","Bulb","Barrel","Waffle","Star","Chessboard","Castle","Bull","Bucket","Croissant","Lighthouse","Chocolate_2","Lifebuoy","Orange","pelican","Angel","Goomba","Cone","Avocado","Fluffy","Spices","Koala","Mami","Tractor_Shovel","Bear","Remote","Tree","Teepee","Dollars_container","Brain","Volcano","Toaster"};
  static readonly int[] Counts={996,1339,1416,1471,1585,1695,1821,1876,1909,1920,1923,1949,1964,1968,2017,2068,2096,2144,2155,2471,2197,2300,2304,2307,2308,2320,2324,2339,2356,2381,2395,2398,2400,2400,2492,2504,2512,2533,2560,2567,2570,2576,2604,2625,2652,2660,2690,2717,2764,2765,2772,2801,2806,2864,2880,2891,2902,2944,2984,3001,3032,3067,3098,3102,3159,3170,3198,3199,3209,3215,3270,3295,3300,3448,3485,3490,3516,3561,3618,3627};
  static HarvestModel Model=>BeeHarvestApp.Interface.GetModel<HarvestModel>();
  static IEnumerator Load(int n){string name="Level"+n.ToString("00");Assert.IsTrue(Application.CanStreamedLevelBeLoaded(name),name);SceneManager.LoadScene(name);yield return null;yield return null;}
  [UnityTest] public IEnumerator AllEightyScenesHaveTheirOwnModelWithCurrentUiAudioAndVictoryBindings(){
   for(int i=0;i<80;i++){
    yield return Load(i+16);var level=Object.FindObjectOfType<LevelOneController>();var label=level.Hud.transform.Find("LevelLabel").GetComponent<TMP_Text>();
    Assert.AreEqual(i+16,level.LevelNumber);Assert.AreEqual("Level "+(i+16),label.text);Assert.IsTrue(label.font.HasCharacters(label.text));Assert.AreEqual(Counts[i],Model.Remaining);Assert.AreEqual(3,Model.ColumnCount);Assert.AreEqual(5,Model.Slots.Count);
    var board=level.GetComponent<HarvestBoardView>();Assert.IsTrue(board.AutoRotate);Assert.AreEqual(Counts[i],board.GetComponentsInChildren<Renderer>().Count(r=>r.name.StartsWith(Names[i]+"V2_Voxel_")));
    for(int c=0;c<5;c++)Assert.AreEqual(c<3,level.Hud.QueueButtons[c].gameObject.activeSelf,"All production levels use three queue columns");
    var audio=Object.FindObjectOfType<GameAudioController>();Assert.IsNotNull(audio);Assert.AreEqual(7,audio.GetComponents<AudioSource>().Length);Assert.AreEqual("LevelMusic",audio.GetComponents<AudioSource>().Single(s=>s.loop).clip.name);
    var victory=Object.FindObjectOfType<VictoryPopupController>(true);Assert.IsNotNull(victory.Celebration);Assert.IsFalse(victory.IsOpen);Assert.AreEqual("60",victory.GetComponentsInChildren<TMP_Text>(true).Single(t=>t.name=="RewardAmount").text);
    var settings=Object.FindObjectsOfType<SettingsPopupController>(true).Single(p=>p.Level==level);Assert.AreEqual(level,settings.Level);Assert.IsNotNull(level.Hud.Boosters.PurchasePopup);
   }
  }
  [UnityTest] public IEnumerator FifteenthVictoryLoadsTheFirstNewLevelOnce(){
   yield return Load(15);var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;var app=BeeHarvestApp.Interface;
   app.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new AdvanceHarvestCommand(10));Assert.IsTrue(Model.Won);yield return null;
   var popup=Object.FindObjectOfType<VictoryPopupController>();float deadline=Time.realtimeSinceStartup+3;
   while(!popup.ContinueButton.interactable&&Time.realtimeSinceStartup<deadline)yield return null;
   Assert.IsTrue(popup.ContinueButton.interactable);popup.ContinueButton.onClick.Invoke();popup.ContinueButton.onClick.Invoke();deadline=Time.realtimeSinceStartup+5;
   while(SceneManager.GetActiveScene().name!="Level16"&&Time.realtimeSinceStartup<deadline)yield return null;
   yield return null;yield return null;Assert.AreEqual("Level16",SceneManager.GetActiveScene().name);Assert.AreEqual(996,Model.Remaining);Assert.IsFalse(Model.Won);
  }
  [UnityTest] public IEnumerator FirstAndLastNewScenesTransportRealCargoAndRetryTheirOwnSource(){
   foreach(int number in new[]{16,20,35,42,62,95}){
    yield return Load(number);while(BeeHarvestApp.Interface.GetModel<EconomyModel>().Stamina<3)BeeHarvestApp.Interface.SendCommand(new AddStaminaCommand());var level=Object.FindObjectOfType<LevelOneController>();var app=BeeHarvestApp.Interface;var rules=app.GetSystem<HarvestSystem>();var board=level.GetComponent<HarvestBoardView>();var shape=board.transform.Find(Names[number-16]+"(Clone)");int selected=-1;
    for(int turn=0;turn<8&&selected<0;turn++){
     board.PublishCameraView();for(int c=0;c<Model.ColumnCount;c++){int hive=rules.Front(c);if(hive>=0&&rules.CanSelectHive(hive)&&rules.FindTarget(Model.Hives[hive].Data.color)>=0){selected=c;break;}}
     if(selected<0)shape.Rotate(0,45,0,Space.Self);
    }
    Assert.GreaterOrEqual(selected,0,Names[number-16]+" must expose a playable front hive");yield return HiveInputFixture.Click(level.Hud,selected);BeeSceneSteps.Until(()=>Model.Deposited>0,40);Assert.Greater(Model.Deposited,0);Assert.AreEqual(Counts[number-16],Model.Remaining+Model.InTransit+Model.Deposited);
    HiveInputFixture.RetryLevelForTest();yield return null;Assert.AreEqual(Counts[number-16],Model.Remaining);Assert.AreEqual(0,Model.Deposited);Assert.AreEqual(number,level.LevelNumber);
   }
  }
 }
}
