using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class LevelSevenCavityContentTests {
  [Test] public void CavityScopeIncludesRequestedLevelsAndExistingSeven(){
   var method=typeof(LevelOneAssets).GetMethod("HasVoxelCavity",BindingFlags.Public|BindingFlags.Static);
   Assert.IsNotNull(method,"A deliberate level-seven gate is required");
   for(int i=1;i<=LevelOneAssets.LastLevel;i++)Assert.AreEqual(i<=7||(i>=106&&i<=LevelOneAssets.LastLevel),(bool)method.Invoke(null,new object[]{i}),"Level "+i);
  }
  [Test] public void CavityShaderIsIncludedThroughResourcesAndUsesOneForwardPass(){
   var material=Resources.Load<Material>("Materials/Level07VoxelCavity");Assert.IsNotNull(material,"Build-included cavity template is required");
   Assert.AreEqual("BeeHarvest/VoxelCavity",material.shader.name);Assert.IsTrue(material.shader.isSupported);
   Assert.AreEqual(3,material.passCount,"Reuse Forward/Shadow/Depth, without an extra overlay pass");
   Assert.AreEqual(0,material.GetFloat("_EdgeWidth"),"Level seven keeps its existing unoutlined faces");
  }
 }
}
