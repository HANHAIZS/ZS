using System;
namespace BeeHarvest {
 [Serializable] public sealed class LevelData { public string sourceName,sourceContentHash; public int columns=5; public CubeData[] cubes; public HiveData[] hives; }
 [Serializable] public sealed class CubeData { public int id,x,y,z,sourceColor,sourceDepth,sourceX,sourceY,sourceZ; public string name,color; }
 [Serializable] public sealed class HiveData { public bool isHidden; public int column,quota,sourceId,sourceColor; public string color,sourceRid; public int[] connected; }
 public sealed class CubeState { public CubeData Data; public int Stage,Task=-1; }
 public sealed class HiveState { public HiveData Data; public int Remaining,Reserved,Busy,Slot=-1; public bool Started; internal float DepartureCooldown; internal int[] group; public System.Collections.Generic.IReadOnlyList<int> Group=>group; }
 public sealed class FlightState { public int Id,Cube,Hive,Slot,Stage; public float Progress,Age,PhaseProgress,ReturnProgress; public int Lane,TransportPhase; public bool CollectionGranted,Reached,ClearedSource,ClearedApproach,Returned,ClearedEntry,ReturnCornerReached; public UnityEngine.Vector3 PickupNormal,ArrivalView; }
}
