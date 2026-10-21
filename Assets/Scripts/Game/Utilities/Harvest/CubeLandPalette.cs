using System;
using UnityEngine;
namespace BeeHarvest {
 // Independent source color identities. Display tones sampled from the supplied Icon textures; source materials were not included.
 public static class CubeLandPalette {
  public static readonly int[] Ids={1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20};
  public static Color ColorOf(int id){
   string hex;switch(id){case 1:hex="#F8D8A8";break;case 2:hex="#383838";break;case 3:hex="#2868A8";break;case 4:hex="#885828";break;case 5:hex="#187808";break;case 6:hex="#08C858";break;case 7:hex="#F89808";break;case 8:hex="#F858E8";break;case 9:hex="#9828D8";break;case 10:hex="#D81818";break;case 11:hex="#78F8F8";break;case 12:hex="#F8F858";break;case 13:hex="#F8F8F8";break;case 14:hex="#A89898";break;case 19:hex="#28A888";break;case 15:hex="#584848";break;case 16:hex="#C878E8";break;case 18:hex="#880808";break;case 17:hex="#D80868";break;case 20:hex="#F85808";break;default:throw new ArgumentException("Unknown Cube Land color: "+id);}
   Color color;ColorUtility.TryParseHtmlString(hex,out color);return color;
  }
  public static bool TryColor(string key,out Color color){int id;if(key!=null&&key.StartsWith("Palette",StringComparison.Ordinal)&&int.TryParse(key.Substring(7),out id)){color=ColorOf(id);return true;}color=default(Color);return false;}
 }
}
