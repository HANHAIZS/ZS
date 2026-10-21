using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace BeeHarvest.Tests {
 public sealed class GameAudioTests : EconomyTestFixture {
  SettingsPreferences previousPreferences;SettingsModel previousSettings;string prefix;Type audioType;
  [SetUp] public void IsolateSettings(){var app=BeeHarvestApp.Interface;previousPreferences=app.GetUtility<SettingsPreferences>();previousSettings=app.GetModel<SettingsModel>();prefix="BeeHarvest.tests.audio."+Guid.NewGuid().ToString("N");app.RegisterUtility(new SettingsPreferences(prefix));app.RegisterModel(new SettingsModel());audioType=typeof(LevelOneController).Assembly.GetType("BeeHarvest.GameAudioController");}
  [TearDown] public void RestoreSettings(){var app=BeeHarvestApp.Interface;app.RegisterUtility(previousPreferences);app.RegisterModel(previousSettings);foreach(var name in new[]{"Music","Sound","Haptic"})PlayerPrefs.DeleteKey(prefix+"."+name);}
  Component Host(){Assert.IsNotNull(audioType,"The supplied music and sound clips need their scene-owned audio presentation");var hosts=Object.FindObjectsOfType(audioType);Assert.AreEqual(1,hosts.Length);return (Component)hosts[0];}
  AudioSource Music()=>Host().GetComponents<AudioSource>().Single(s=>s.loop);
  AudioSource[] Effects()=>Host().GetComponents<AudioSource>().Where(s=>!s.loop).ToArray();
  static IEnumerator Load(string scene){SceneManager.LoadScene(scene);yield return null;yield return null;}
  static IEnumerator Playing(AudioSource source){float deadline=Time.realtimeSinceStartup+3;while(!source.isPlaying&&Time.realtimeSinceStartup<deadline)yield return null;Assert.IsTrue(source.isPlaying,"The selected background track must actually play");}
  [UnityTest] public IEnumerator HomeMusicLoopsAcrossTabsAndSwitchesToLevelMusic(){
   yield return Load("MainMenu");var music=Music();yield return Playing(music);Assert.AreEqual(Resources.Load<AudioClip>("Audio/Music/HomeMusic"),music.clip);Assert.That(music.clip.length,Is.InRange(21f,23f));Assert.AreEqual(0,music.spatialBlend);
   music.time=2;var page=Object.FindObjectOfType<MainMenuPage>();page.ShopButton.onClick.Invoke();yield return null;yield return null;Assert.AreEqual("HomeMusic",music.clip.name);Assert.That(music.time,Is.GreaterThanOrEqualTo(1.9f));
   yield return Load("Level01");music=Music();yield return Playing(music);Assert.AreEqual(Resources.Load<AudioClip>("Audio/Music/LevelMusic"),music.clip);Assert.That(music.clip.length,Is.InRange(133f,135f));Assert.IsTrue(music.loop);
   music.time=4;Object.FindObjectOfType<LevelOneController>().RestartLevel();yield return null;Assert.That(music.time,Is.GreaterThanOrEqualTo(3.9f));
   BeeHarvestApp.Interface.SendCommand(new ReturnHomeCommand(false));yield return null;Assert.AreEqual("HomeMusic",music.clip.name);yield return Playing(music);
   page=Object.FindObjectOfType<MainMenuPage>();yield return null;page.StartGameButton.onClick.Invoke();yield return null;Assert.AreEqual("LevelMusic",music.clip.name);
  }
  [UnityTest] public IEnumerator SlotSoundStartsAtCommittedLandingAndCancelledHopsStaySilent(){
   yield return Load("Level01");Host();var level=Object.FindObjectOfType<LevelOneController>();var hop=level.GetComponent<HiveHopController>();var model=BeeHarvestApp.Interface.GetModel<HarvestModel>();
   hop.Click(0);Assert.IsFalse(Effects().Any(s=>s.isPlaying));var popup=Object.FindObjectOfType<SettingsPopupController>();popup.OpenButton.onClick.Invoke();yield return new WaitForSecondsRealtime(.4f);Assert.IsFalse(model.Hives[0].Started);Assert.IsFalse(Effects().Any(s=>s.isPlaying));popup.Close();yield return null;yield return null;
   yield return HiveInputFixture.Click(level.Hud,0);var slot=Resources.Load<AudioClip>("Audio/Sfx/HiveSlot");Assert.AreEqual(1,Effects().Count(s=>s.isPlaying&&s.clip==slot));
   BeeHarvestApp.Interface.SendCommand(new ActivateWaitingHiveCommand(0,0));Assert.AreEqual(1,Effects().Count(s=>s.isPlaying&&s.clip==slot),"A rejected duplicate cannot create a second slot voice");
  }
  [UnityTest] public IEnumerator CarrySoundIsTriggeredByPickupAndMusicSoundSwitchesAreIndependent(){
   yield return Load("Level01");var app=BeeHarvestApp.Interface;Host();var level=Object.FindObjectOfType<LevelOneController>();level.enabled=false;app.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
   app.SendCommand(new SetPreferenceCommand(GamePreference.Music,false));yield return null;Assert.IsFalse(Music().isPlaying);
   app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));app.SendCommand(new ActivateHiveCommand(0));
   var carry=Resources.Load<AudioClip>("Audio/Sfx/BeeCarry");Assert.IsFalse(Effects().Any(s=>s.isPlaying&&s.clip==carry));
   app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+.01f));Assert.IsTrue(Effects().Any(s=>s.isPlaying&&s.clip==carry));Assert.IsFalse(Music().isPlaying);
   app.SendCommand(new SetPreferenceCommand(GamePreference.Sound,false));yield return null;Assert.IsFalse(Effects().Any(s=>s.isPlaying));app.SendCommand(new SetPreferenceCommand(GamePreference.Music,true));yield return Playing(Music());Assert.IsFalse(Effects().Any(s=>s.isPlaying));
  }
  [UnityTest] public IEnumerator BackgroundDisableAndReloadNeverLeaveStaleOrDuplicateSources(){
   yield return Load("Level02");var host=(Behaviour)Host();var music=Music();yield return Playing(music);music.time=3;host.SendMessage("OnApplicationPause",true);Assert.IsFalse(music.isPlaying);Assert.IsFalse(Effects().Any(s=>s.isPlaying));host.SendMessage("OnApplicationPause",false);yield return Playing(music);Assert.That(music.time,Is.GreaterThanOrEqualTo(2.9f));
   for(int i=0;i<2;i++){host.enabled=false;Assert.IsFalse(music.isPlaying);host.enabled=true;yield return Playing(music);Assert.AreEqual(7,host.GetComponents<AudioSource>().Length);}
   yield return Load("Level03");yield return Playing(Music());Assert.AreEqual(1,Object.FindObjectsOfType<AudioSource>().Count(s=>s.loop));Assert.AreEqual(7,Host().GetComponents<AudioSource>().Length);Assert.AreEqual(1,Object.FindObjectsOfType<AudioListener>().Length);
  }
  [UnityTest] public IEnumerator DenseHarvestUsesBoundedVoicesAndOldEffectsNeverReplay(){
   yield return Load("Level01");var app=BeeHarvestApp.Interface;Host();Object.FindObjectOfType<LevelOneController>().enabled=false;app.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));
   var data=new LevelData{cubes=Enumerable.Range(0,20).Select(i=>new CubeData{id=i,x=i,color="red"}).ToArray(),hives=Enumerable.Range(0,5).Select(i=>new HiveData{column=i,color="red",quota=4}).ToArray()};
   app.SendCommand(new StartLevelCommand(data));for(int i=0;i<5;i++)app.SendCommand(new ActivateHiveCommand(i));app.SendCommand(new AdvanceHarvestCommand(AdvanceHarvestCommand.OutboundSeconds+.01f));
   Assert.IsTrue(Effects().Any(s=>s.isPlaying));Assert.LessOrEqual(Effects().Count(s=>s.isPlaying),6);Assert.AreEqual(7,Host().GetComponents<AudioSource>().Length);
   yield return new WaitForSecondsRealtime(.3f);Assert.IsFalse(Effects().Any(s=>s.isPlaying),"A saturated burst cannot queue late audio");
   app.SendCommand(new StartLevelCommand(data));app.SendCommand(new ActivateHiveCommand(0));Assert.IsTrue(Effects().Any(s=>s.isPlaying));app.SendCommand(new PauseHarvestCommand(true));yield return null;Assert.IsFalse(Effects().Any(s=>s.isPlaying));app.SendCommand(new PauseHarvestCommand(false));yield return null;Assert.IsFalse(Effects().Any(s=>s.isPlaying));
   app.SendCommand(new ActivateHiveCommand(1));Assert.IsTrue(Effects().Any(s=>s.isPlaying));app.SendCommand(new StartLevelCommand(data));yield return null;Assert.IsFalse(Effects().Any(s=>s.isPlaying));
   app.SendEvent(new HiveEnteredSlotEvent{Generation=app.GetModel<HarvestModel>().Generation-1,Hive=0});Assert.IsFalse(Effects().Any(s=>s.isPlaying));
   app.SendCommand(new ActivateHiveCommand(0));app.SendCommand(new ReturnHomeCommand(false));Assert.IsFalse(Effects().Any(s=>s.isPlaying));Assert.AreEqual("HomeMusic",Music().clip.name);
  }
 }
}
