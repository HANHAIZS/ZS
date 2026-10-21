namespace BeeHarvest {
 /// <summary>Reports one committed pickup or delivery without owning state.</summary>
 public struct HarvestChangedEvent { public int Task; public bool Picked; }
}
