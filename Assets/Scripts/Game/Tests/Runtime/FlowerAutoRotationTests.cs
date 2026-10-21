using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BeeHarvest.Tests
{
    public sealed class FlowerAutoRotationTests : EconomyTestFixture
    {
        static Transform Flower => GameObject.Find("Flower(Clone)").transform;
        static LevelOneController Level => Object.FindObjectOfType<LevelOneController>();
        static IEnumerator Load()
        {
            SceneManager.LoadScene("Level01"); yield return null; yield return null;
        }
        // Detect a disabled default or missing shared update in any authored production scene.
        [UnityTest]
        public IEnumerator AllFifteenModelsRotateByDefaultWithoutAnOptIn()
        {
            for(int number=1;number<=15;number++){
                SceneManager.LoadScene("Level"+number.ToString("00"));yield return null;yield return null;
                var shape=GameObject.Find(LevelOneAssets.ModelKey(number)+"(Clone)").transform;
                float before=shape.eulerAngles.y;
                yield return new WaitForSeconds(.2f);
                Assert.Greater(Mathf.DeltaAngle(before,shape.eulerAngles.y),.3f,"Level "+number+" must rotate clockwise without a caller enabling it");
            }
        }
        // Catch a missing Update call, a stopped rotation, or a reversed Y direction using real scene motion.
        [UnityTest]
        public IEnumerator ActiveFlowerTurnsClockwiseWhenViewedFromAbove()
        {
            yield return Load();
            var before=Flower.forward;
            yield return new WaitForSeconds(.25f);
            Assert.Greater(Vector3.Cross(before,Flower.forward).y,.015f,"From above, the forward axis must turn toward the right");
        }
        // Catch modal/navigation pause leaks and failing to resume rotation at Start Game.
        [UnityTest]
        public IEnumerator SettingsAndHomeFreezeRotationThenStartResumes()
        {
            yield return Load();
            var popup=Object.FindObjectOfType<SettingsPopupController>();
            popup.OpenButton.onClick.Invoke(); yield return null;
            var held=Flower.rotation;
            yield return new WaitForSeconds(.15f);
            Assert.Less(Quaternion.Angle(held,Flower.rotation),.01f,"Settings freezes the flower");
            popup.CloseButton.onClick.Invoke(); yield return PopupMotionTestWait.Close();
            held=Flower.rotation; yield return new WaitForSeconds(.2f);
            Assert.Greater(Quaternion.Angle(held,Flower.rotation),1f,"Closing settings resumes rotation");
            popup.OpenButton.onClick.Invoke(); popup.HomeButton.onClick.Invoke();popup.LeaveButton.onClick.Invoke(); yield return PopupMotionTestWait.Close();
            held=Flower.rotation; yield return new WaitForSeconds(.15f);
            Assert.Less(Quaternion.Angle(held,Flower.rotation),.01f,"Home freezes the retained flower");
            GameObject.Find("StartGameButton").GetComponent<Button>().onClick.Invoke(); yield return null;
            held=Flower.rotation; yield return new WaitForSeconds(.2f);
            Assert.Greater(Quaternion.Angle(held,Flower.rotation),1f,"Start resumes retained rotation");
        }
        // Catch background pause leaks and a Retry that leaves the previous orientation active.
        [UnityTest]
        public IEnumerator BackgroundFreezesRotationAndRetryRestoresInitialDirection()
        {
            yield return Load();
            Flower.rotation=Quaternion.Euler(0,60,0);
            Level.SendMessage("OnApplicationPause",true); yield return null;
            var held=Flower.rotation; yield return new WaitForSeconds(.15f);
            Assert.Less(Quaternion.Angle(held,Flower.rotation),.01f);
            Level.SendMessage("OnApplicationPause",false); yield return null;
            held=Flower.rotation; yield return new WaitForSeconds(.2f);
            Assert.Greater(Quaternion.Angle(held,Flower.rotation),1f);
            Level.enabled=false; Level.RestartLevel();
            Assert.Less(Quaternion.Angle(Quaternion.identity,Flower.rotation),.01f,"Retry restores the original direction");
        }
        // Catch auto movement during a drag or resuming from a stale manual angle.
        [UnityTest]
        public IEnumerator DragGateHoldsAngleAndReleaseContinuesFromCurrentPose()
        {
            yield return Load(); Level.enabled=false;
            var board=Object.FindObjectOfType<HarvestBoardView>();
            var tick=typeof(HarvestBoardView).GetMethod("TickAutoRotation",BindingFlags.Instance|BindingFlags.Public);
            Assert.IsNotNull(tick,"Flower rotation needs a controlled frame step");
            var gate=typeof(HarvestBoardView).GetField("rotating",BindingFlags.Instance|BindingFlags.NonPublic);
            Flower.rotation=Quaternion.Euler(0,70,0);gate.SetValue(board,true);
            tick.Invoke(board,new object[]{.5f});Assert.AreEqual(70,Flower.eulerAngles.y,.01f);
            gate.SetValue(board,false);tick.Invoke(board,new object[]{.5f});
            Assert.AreEqual(75,Flower.eulerAngles.y,.01f,"Half a second adds 5 degrees from the actual current pose");
            board.SetUiInputBlocked(true);tick.Invoke(board,new object[]{.5f});Assert.AreEqual(75,Flower.eulerAngles.y,.01f);
            board.SetUiInputBlocked(false);
        }
    }
}
