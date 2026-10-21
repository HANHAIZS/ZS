using QFramework;
using UnityEngine;
using System.Collections.Generic;
namespace BeeHarvest {
 public sealed class HarvestModel : AbstractModel {
  internal readonly List<CubeState> cubes=new List<CubeState>();
  internal readonly List<HiveState> hives=new List<HiveState>();
  internal readonly List<FlightState> flights=new List<FlightState>();
  internal readonly List<int> slots=new List<int>{-1,-1,-1,-1,-1};
  internal readonly List<int>[] waiting={new List<int>(),new List<int>(),new List<int>(),new List<int>(),new List<int>()};
  internal readonly int[] boosterCounts={1,1,1,1};
  public int BoosterCount(int index)=>index>=0&&index<4?boosterCounts[index]:0;
  public float SpeedSeconds {get;internal set;}
  public bool SpeedBoost=>SpeedSeconds>0;
  /// <summary>One pending unrestricted waiting-hive choice, consumed only by successful activation.</summary>
  public bool FreeSelection {get;internal set;}
  public float SpeedMultiplier=>SpeedBoost?2f:1f;
  public IReadOnlyList<int> WaitingColumn(int column)=>waiting[column];
  public IReadOnlyList<CubeState> Cubes=>cubes;
  public IReadOnlyList<HiveState> Hives=>hives;
  public IReadOnlyList<FlightState> Flights=>flights;
  public IReadOnlyList<int> Slots=>slots;
  public int ColumnCount {get;internal set;}=5;
  public bool HasLinkedGroups {get;internal set;}
  internal float EntryClock;
  internal readonly float[] DeparturePorts=new float[6];
  public bool RequireFlightArrival {get;internal set;}
  internal int FlightPresentationOwner;
  public int Generation {get;internal set;}
  public int Deposited {get;internal set;}
  public int Remaining {get;internal set;}
  public int InTransit {get;internal set;}
  public int Revision {get;internal set;}
  public int BoardRevision {get;internal set;}
  public Vector3 ViewDirection {get;internal set;}=Vector3.back;
  public Vector3 ViewCameraPosition {get;internal set;}=new Vector3(0,0,-1000);
  public bool OrthographicView {get;internal set;}=true;
  public int ViewRevision {get;internal set;}
  public bool Won {get;internal set;}
  public bool Deadlocked {get;internal set;}
  public bool Paused {get;internal set;}
  /// <summary>Leaves level state empty until the explicit start command.</summary>
  protected override void OnInit() {}
 }
}
