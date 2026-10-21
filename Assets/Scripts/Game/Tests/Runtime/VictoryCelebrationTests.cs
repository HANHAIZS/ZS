using System.Collections;
using System.Linq;
using System;
using Object=UnityEngine.Object;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class VictoryCelebrationTests : EconomyTestFixture {
  SettingsPreferences previousPreferences;SettingsModel previousSettings;string prefix;
  [SetUp] public void IsolateSettings(){var app=BeeHarvestApp.Interface;previousPreferences=app.GetUtility<SettingsPreferences>();previousSettings=app.GetModel<SettingsModel>();prefix="BeeHarvest.tests.victory."+Guid.NewGuid().ToString("N");app.RegisterUtility(new SettingsPreferences(prefix));app.RegisterModel(new SettingsModel());}
  [TearDown] public void RestoreSettings(){var app=BeeHarvestApp.Interface;app.RegisterUtility(previousPreferences);app.RegisterModel(previousSettings);foreach(var name in new[]{"Music","Sound","Haptic"})PlayerPrefs.DeleteKey(prefix+"."+name);}
  static IEnumerator Load(){SceneManager.LoadScene("Level01");yield return null;yield return null;}
  static void StartTiny(){var app=BeeHarvestApp.Interface;Object.FindObjectOfType<LevelOneController>().enabled=false;app.SendCommand(new DetachBeeFlightPresentationCommand(Object.FindObjectOfType<BeeFlightView>().GetInstanceID()));app.SendCommand(new StartLevelCommand(new LevelData{cubes=new[]{new CubeData{id=0,color="red"}},hives=new[]{new HiveData{column=0,color="red",quota=1}}}));app.SendCommand(new ActivateHiveCommand(0));}
  static void Win(){BeeHarvestApp.Interface.SendCommand(new AdvanceHarvestCommand(10));}
  static MonoBehaviour Celebration(){var type=typeof(LevelOneController).Assembly.GetType("BeeHarvest.VictoryCelebrationView");Assert.IsNotNull(type,"Victory needs the imported native particle celebration");return (MonoBehaviour)Object.FindObjectOfType(type,true);}
  static bool MeshHasVertices(CanvasRenderer r){var m=r.GetMesh();return m!=null&&m.vertexCount>0;}
  [UnityTest] public IEnumerator FinalDeliveryShowsWhiteRewardAndNativeParticlesThenRetryClearsThem(){
   yield return Load();StartTiny();yield return null;
   Assert.IsFalse(Object.FindObjectOfType<VictoryPopupController>().IsOpen);
   Win();yield return null;yield return null;Canvas.ForceUpdateCanvases();
   var reward=Object.FindObjectsOfType<TMP_Text>(true).SingleOrDefault(t=>t.name=="RewardAmount");Assert.IsNotNull(reward,"The 60 reward belongs below the coin art");Assert.AreEqual("60",reward.text);Assert.AreEqual(Color.white,reward.color);Assert.AreEqual("FredokaBoldSDF",reward.font.name);Assert.IsTrue(reward.gameObject.activeInHierarchy);Assert.IsFalse(reward.raycastTarget);
   var view=Celebration();var ps=view.GetComponentsInChildren<ParticleSystem>(true);Assert.That(ps.Length,Is.InRange(2,4));Assert.IsTrue(ps.Any(s=>s.particleCount>0));Assert.LessOrEqual(ps.Sum(s=>s.main.maxParticles),160);
   yield return new WaitForSecondsRealtime(.35f);Canvas.ForceUpdateCanvases();Assert.IsTrue(view.GetComponentsInChildren<Graphic>().All(g=>!g.raycastTarget));Assert.IsTrue(view.GetComponentsInChildren<CanvasRenderer>().Any(r=>MeshHasVertices(r)),"Native particle mesh must reach the Canvas");
   var popup=Object.FindObjectOfType<VictoryPopupController>();popup.SendMessage("OnApplicationPause",true);var animated=ps.Single(s=>s.main.loop);float time=animated.time;yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(time,animated.time,.001f);popup.SendMessage("OnApplicationPause",false);yield return new WaitForSecondsRealtime(.1f);Assert.AreNotEqual(time,animated.time,"Foreground resumes the looping sparkle simulation");
   Object.FindObjectOfType<LevelOneController>().RestartLevel();yield return null;Assert.IsFalse(view.gameObject.activeInHierarchy);Assert.IsTrue(ps.All(s=>s.particleCount==0));
  }
  [UnityTest] public IEnumerator VictoryAudioIsOneShotAndDoesNotReplayOnPopupReenableOrForeground(){
   yield return Load();StartTiny();Win();yield return null;yield return null;
   var clip=Resources.Load<AudioClip>("Audio/Sfx/Victory");Assert.IsNotNull(clip);var host=Object.FindObjectOfType<GameAudioController>();var voices=host.GetComponents<AudioSource>();Assert.AreEqual(7,voices.Length);var playing=voices.Where(s=>s.isPlaying&&s.clip==clip).ToArray();Assert.AreEqual(1,playing.Length,"Victory must play the provided clip once after delivery");Assert.IsFalse(playing[0].loop);
   BeeHarvestApp.Interface.SendEvent(new VictoryPresentationEvent{Generation=BeeHarvestApp.Interface.GetModel<HarvestModel>().Generation-1,Visible=false});Assert.IsTrue(playing[0].isPlaying,"An old generation cannot interrupt the current victory sound");
   var popup=Object.FindObjectOfType<VictoryPopupController>();popup.enabled=false;yield return null;Assert.IsFalse(playing[0].isPlaying,"Hidden victory stops its sound");popup.enabled=true;yield return null;yield return null;Assert.IsFalse(voices.Any(s=>s.isPlaying&&s.clip==clip),"Reopening one win must not replay it");
   host.SendMessage("OnApplicationPause",true);host.SendMessage("OnApplicationPause",false);yield return null;Assert.IsFalse(voices.Any(s=>s.isPlaying&&s.clip==clip));
  }
  [UnityTest] public IEnumerator MutedVictoryIsConsumedWithoutLatePlaybackWhenSoundIsEnabled(){
   yield return Load();BeeHarvestApp.Interface.SendCommand(new SetPreferenceCommand(GamePreference.Sound,false));StartTiny();Win();yield return null;yield return null;
   var clip=Resources.Load<AudioClip>("Audio/Sfx/Victory");Assert.IsNotNull(clip);var host=Object.FindObjectOfType<GameAudioController>();Assert.IsFalse(host.GetComponents<AudioSource>().Any(s=>s.isPlaying&&s.clip==clip));
   BeeHarvestApp.Interface.SendCommand(new SetPreferenceCommand(GamePreference.Sound,true));yield return null;Assert.IsFalse(host.GetComponents<AudioSource>().Any(s=>s.isPlaying&&s.clip==clip));
  }
 }
}
