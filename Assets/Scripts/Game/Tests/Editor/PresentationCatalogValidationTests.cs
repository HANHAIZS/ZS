using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
namespace BeeHarvest.Tests {
 public sealed class PresentationCatalogValidationTests {
  static MethodInfo Parser(){var method=typeof(LevelOneAssets).GetMethod("ParsePresentationCatalog",BindingFlags.Public|BindingFlags.Static);Assert.IsNotNull(method,"Catalog records must be validated before they can enter the runtime cache");return method;}
  static JObject Source()=>JObject.Parse(Resources.Load<TextAsset>("Levels/PresentationCatalog").text);
  static void Reject(JObject json){var parser=Parser();var error=Assert.Throws<TargetInvocationException>(()=>parser.Invoke(null,new object[]{json.ToString()}));Assert.IsInstanceOf<InvalidOperationException>(error.InnerException);}
  [TestCase("ambientLight")] [TestCase("sourceHandoff")] [TestCase("edges")] [TestCase("shadowBias")] [TestCase("modelKey")]
  public void MissingRequiredFieldFailsBeforePublishingCache(string name){var root=Source();((JObject)root["entries"][5]).Property(name).Remove();Reject(root);}
  [TestCase("shadowBias",-1)] [TestCase("shadowStrength",2)] [TestCase("shadowNormalBias",double.NaN)]
  public void InvalidLightingFailsBeforePublishingCache(string name,double value){var root=Source();root["entries"][0][name]=value;Reject(root);}
  [Test] public void DuplicateLevelAndUnsafePrefabKeyAreRejected(){var root=Source();root["entries"][1]["number"]=1;Reject(root);root=Source();root["entries"][0]["modelKey"]="../Flower";Reject(root);}
  [Test] public void PartialColorAndWrongBooleanTypesAreRejected(){var root=Source();((JObject)root["entries"][0]["ambientLight"]).Property("b").Remove();Reject(root);root=Source();root["entries"][0]["sourceHandoff"]="false";Reject(root);}
  [Test] public void ValidCatalogReturnsAllIndependentValidatedRecords(){var records=(LevelPresentationData[])Parser().Invoke(null,new object[]{Source().ToString()});Assert.AreEqual(LevelOneAssets.LastLevel,records.Length);Assert.AreEqual("Chick",records[5].modelKey);Assert.IsTrue(records[5].sourceHandoff);Assert.IsFalse(records[105].sourceHandoff);Assert.Throws<ArgumentOutOfRangeException>(()=>LevelOneAssets.Presentation(0));Assert.Throws<ArgumentOutOfRangeException>(()=>LevelOneAssets.Presentation(LevelOneAssets.LastLevel+1));}
 }
}
