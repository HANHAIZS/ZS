namespace BeeHarvest {
 /// <summary>Editor preview and opt-in player builds expose the playtest route.</summary>
 public static class PlaytestBuild {
#if UNITY_EDITOR || BEEHARVEST_PLAYTEST
  public static bool Available=>true;
#else
  public static bool Available=>false;
#endif
 }
}
