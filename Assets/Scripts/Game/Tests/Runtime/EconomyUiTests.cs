using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BeeHarvest.Tests {
 public sealed class EconomyUiTests : EconomyTestFixture {
  Component Header(){var t=typeof(HarvestModel).Assembly.GetType("BeeHarvest.EconomyHeaderView");Assert.IsNotNull(t,"Economy header is required");var view=UnityEngine.Object.FindObjectOfType(t) as Component;Assert.IsNotNull(view);return view;}
  T Ref<T>(string name) where T:class{return Header().GetType().GetField(name).GetValue(Header()) as T;}
  IEnumerator Load(string scene){SceneManager.LoadScene(scene);yield return null;yield return null;yield return null;}
  void SetEnergy(int value){var m=BeeHarvestApp.Interface.GetModel<EconomyModel>();typeof(EconomyModel).GetProperty("Stamina").SetValue(m,value);typeof(EconomyModel).GetProperty("NextRecoveryAt").SetValue(m,0L);}
  [UnityTest] public IEnumerator HeaderLabelsAndPlusButtonsRenderSavedState(){
   yield return Load("MainMenu");Assert.AreEqual("1600",Ref<TMP_Text>("CoinsLabel").text);Assert.AreEqual("5",Ref<TMP_Text>("StaminaLabel").text);Assert.AreEqual("full",Ref<TMP_Text>("RecoveryLabel").text);
   Ref<Button>("CoinsPlus").onClick.Invoke();Assert.AreEqual("2100",Ref<TMP_Text>("CoinsLabel").text);
   SetEnergy(3);Header().SendMessage("Refresh");Assert.AreEqual("10:00",Ref<TMP_Text>("RecoveryLabel").text);Ref<Button>("StaminaPlus").onClick.Invoke();Assert.AreEqual("4",Ref<TMP_Text>("StaminaLabel").text);Ref<Button>("StaminaPlus").onClick.Invoke();Assert.AreEqual("full",Ref<TMP_Text>("RecoveryLabel").text);Assert.IsFalse(Ref<Button>("StaminaPlus").interactable);
  }
  [UnityTest] public IEnumerator ModalBlocksGrantsAndShopKeepsCounters(){
   yield return Load("MainMenu");var page=UnityEngine.Object.FindObjectOfType<MainMenuPage>();page.SettingsButton.onClick.Invoke();yield return null;int coins=BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins;Ref<Button>("CoinsPlus").onClick.Invoke();Assert.AreEqual(coins,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);page.SettingsPopup.Close();yield return null;yield return null;
   page.ShopButton.onClick.Invoke();yield return null;yield return null;Assert.IsTrue(Header().gameObject.activeInHierarchy);Ref<Button>("CoinsPlus").onClick.Invoke();Assert.AreEqual(coins+500,BeeHarvestApp.Interface.GetModel<EconomyModel>().Coins);page.GalleryButton.onClick.Invoke();yield return null;yield return null;Assert.IsFalse(page.Economy.gameObject.activeInHierarchy);
  }
  [UnityTest] public IEnumerator InternalRetryChargesOnceAndQuitAtZeroDisablesStart(){
   yield return Load("Level02");var app=BeeHarvestApp.Interface;var level=UnityEngine.Object.FindObjectOfType<LevelOneController>();var popup=UnityEngine.Object.FindObjectOfType<SettingsPopupController>();SetEnergy(1);int g=app.GetModel<HarvestModel>().Generation;
   Assert.IsTrue(level.TryRetryLevel());Assert.IsFalse(level.TryRetryLevel());Assert.AreEqual(0,app.GetModel<EconomyModel>().Stamina);Assert.AreEqual(g+1,app.GetModel<HarvestModel>().Generation);yield return null;yield return null;
   popup.OpenButton.onClick.Invoke();yield return null;Assert.IsNotNull(popup.RetryButton);popup.HomeButton.onClick.Invoke();popup.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;
   var page=UnityEngine.Object.FindObjectOfType<MainMenuPage>();Assert.IsFalse(page.StartGameButton.interactable);page.StartGameButton.onClick.Invoke();Assert.IsTrue(app.GetModel<MenuNavigationModel>().IsHome,"Home: "+HiveInputFixture.ObservedInputState());Ref<Button>("StaminaPlus").onClick.Invoke();yield return null;Assert.IsTrue(page.StartGameButton.interactable,"StartInteractable: "+HiveInputFixture.ObservedInputState());page.StartGameButton.onClick.Invoke();yield return null;Assert.IsFalse(app.GetModel<MenuNavigationModel>().IsHome);Assert.AreEqual(g+2,app.GetModel<HarvestModel>().Generation);
  }
  [UnityTest] public IEnumerator SettingsHomeSpendsOnceAndCounterShowsRemainingTime(){
   yield return Load("Level01");var popup=UnityEngine.Object.FindObjectOfType<SettingsPopupController>();popup.OpenButton.onClick.Invoke();yield return null;popup.HomeButton.onClick.Invoke();popup.LeaveButton.onClick.Invoke();popup.HomeButton.onClick.Invoke();popup.LeaveButton.onClick.Invoke();yield return PopupMotionTestWait.Close();yield return null;
   Assert.AreEqual("4",Ref<TMP_Text>("StaminaLabel").text);Assert.AreEqual(4,BeeHarvestApp.Interface.GetModel<EconomyModel>().Stamina);Assert.That(Ref<TMP_Text>("RecoveryLabel").text,Does.StartWith("09:").Or.EqualTo("10:00"));
  }
 }
}
