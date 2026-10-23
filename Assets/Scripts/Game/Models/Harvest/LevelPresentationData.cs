using System;
using UnityEngine;
namespace BeeHarvest {
 [Serializable] public sealed class LevelPresentationData {
  public int number;
  public string modelKey;
  public bool edges,separation,cavity,overrideEdgeColor,sourceHandoff,softShadows;
  public Color edgeColor,ambientLight;
  public float shadowStrength,shadowBias,shadowNormalBias;
  public float modelScaleMultiplier=1,modelCenterY=.66f,slotYOffset,queueFrontY,queueRowPitch,queueColumnPitch,slotColumnPitch;
 }
}
