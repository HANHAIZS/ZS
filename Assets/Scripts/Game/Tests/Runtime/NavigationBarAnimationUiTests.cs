using System.Collections;
using NUnit.Framework;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BeeHarvest.Tests {
 public sealed class NavigationBarAnimationUiTests : EconomyTestFixture {
  static float Center(MainMenuNavigationBarView view)=>(view.Selection.anchorMin.x+view.Selection.anchorMax.x)*.5f;
  static IEnumerator Settled(MainMenuNavigationBarView view,float target,int selected) {
   float deadline=Time.realtimeSinceStartup+2;
   while((Mathf.Abs(Center(view)-target)>.0001f||view.Labels[selected].alpha<1)&&Time.realtimeSinceStartup<deadline)yield return null;
   Assert.AreEqual(target,Center(view),.0001f,"UI transition must finish using unscaled time");
   Assert.AreEqual(1,view.Labels[selected].alpha,"Wait for the exact final pose, including icon size");
  }

  [UnityTest] public IEnumerator ClickAnimatesWithPausedTimeAndKeepsCurrentArtAndHitRegions() {
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   var page=Object.FindObjectOfType<MainMenuPage>();var view=page.Tabs.NavigationBar;
   var buttons=new[]{(RectTransform)page.ShopButton.transform,(RectTransform)page.HomeButton.transform,(RectTransform)page.GalleryButton.transform};
   var anchors=new Vector2[3];var sizes=new Vector2[3];var sprites=new Sprite[3];
   for(int i=0;i<3;i++){anchors[i]=buttons[i].anchorMin;sizes[i]=buttons[i].sizeDelta;sprites[i]=view.Icons[i].GetComponent<UnityEngine.UI.Image>().sprite;}
   var plateSprite=view.Selection.GetComponent<UnityEngine.UI.Image>().sprite;
   float oldScale=Time.timeScale;
   try {
    Time.timeScale=0;page.ShopButton.onClick.Invoke();
    Assert.AreEqual(.5f,Center(view),.0001f,"A real click starts at the displayed pose");
    Assert.IsTrue(page.Tabs.ShopContent.activeSelf);Assert.IsFalse(page.StartGameButton.gameObject.activeSelf);
    yield return Settled(view,1f/6,0);
    Assert.AreEqual(142,view.Icons[0].sizeDelta.x,.001f);Assert.AreEqual(126,view.Icons[0].anchoredPosition.y,.001f);
    Assert.AreEqual(1f/3,view.Selection.anchorMax.x-view.Selection.anchorMin.x,.0001f);
    Assert.AreSame(plateSprite,view.Selection.GetComponent<UnityEngine.UI.Image>().sprite);
    for(int i=0;i<3;i++){Assert.AreEqual(anchors[i],buttons[i].anchorMin);Assert.AreEqual(sizes[i],buttons[i].sizeDelta);Assert.AreSame(sprites[i],view.Icons[i].GetComponent<UnityEngine.UI.Image>().sprite);}
    page.GalleryButton.onClick.Invoke();yield return Settled(view,5f/6,2);
    Assert.IsTrue(page.Tabs.GalleryContent.activeSelf);Assert.IsFalse(page.Tabs.ShopContent.activeSelf);
   } finally {Time.timeScale=oldScale;}
  }

  [UnityTest] public IEnumerator ReopenAndBackgroundSettleToModelAndPreserveInputGates() {
   SceneManager.LoadScene("MainMenu");yield return null;yield return null;
   var page=Object.FindObjectOfType<MainMenuPage>();var view=page.Tabs.NavigationBar;
   page.ShopButton.onClick.Invoke();Assert.AreEqual(.5f,Center(view),.0001f);
   page.Hide();page.Show(1);
   Assert.AreEqual(1f/6,Center(view),.0001f);Assert.IsFalse(page.ShopButton.interactable);
   yield return null;yield return null;
   page.GalleryButton.onClick.Invoke();float before=Center(view);
   Assert.AreEqual(1f/6,before,.0001f);
   page.Tabs.SendMessage("OnApplicationPause",true);view.SendMessage("OnApplicationPause",true);
   Assert.AreEqual(5f/6,Center(view),.0001f);Assert.IsFalse(page.HomeButton.interactable);
   page.HomeButton.onClick.Invoke();Assert.AreEqual(MainMenuTab.Gallery,BeeHarvestApp.Interface.GetModel<MenuNavigationModel>().SelectedTab);
   page.Tabs.SendMessage("OnApplicationPause",false);view.SendMessage("OnApplicationPause",false);
   yield return null;yield return null;
   Assert.IsTrue(page.HomeButton.interactable);page.HomeButton.onClick.Invoke();yield return Settled(view,.5f,1);
   Assert.IsTrue(page.StartGameButton.gameObject.activeSelf);
  }
 }
}
