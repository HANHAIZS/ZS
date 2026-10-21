using UnityEngine;
using QFramework;
using System.Collections.Generic;
namespace BeeHarvest {
 public sealed class HarvestBoardView:BeeHarvestController {
  [Range(-.2f,.2f)] public float LayoutViewportYOffset;
  HarvestModel model;LevelOneAssets assets;Transform flower;Transform[] cubes;Renderer[] cubeRenderers;
  HiveModelView hiveView;
  VoxelEdgeVisuals voxelEdges;
  public bool AutoRotate {get;set;}=true;
  Transform collectorRoot;Vector3 collectorRest;float deliveryPulse;IUnRegister deliverySubscription;
  Camera flightCamera;float flightPlaneZ;Matrix4x4 gridLocal;Bounds modelLocalBounds;
  int revision=-1,visualGeneration=-1;Vector2 press;bool rotating;float rotation;
  bool uiInputBlocked;
  public void SetUiInputBlocked(bool value){uiInputBlocked=value;if(value)rotating=false;}
  public Vector3 Deposit {get;private set;}
  public Mesh CargoMesh=>cubeRenderers[0].GetComponent<MeshFilter>().sharedMesh;
  public Material CargoMaterial=>cubeRenderers[0].sharedMaterial;
  public Vector3 CargoScale=>cubes[0].lossyScale;
  /// <summary>Preserves the source block's on-screen pose while extracting it onto the safe flight plane.</summary>
  public Pose PickupPose(int id){return new Pose(FlightPoint(cubes[id].position),cubes[id].rotation);}
  public Vector3 PickupOutward(FlightState task){var normal=task.PickupNormal;return normal.sqrMagnitude>.1f?(flower.localToWorldMatrix*gridLocal).MultiplyVector(normal).normalized:-flightCamera.transform.forward;}
  public Vector3 PickupScale(int id){return cubes[id].lossyScale;}
  /// <summary>Copies the exact source visual once at pickup, with no new material instance.</summary>
  public void CopyPickupVisual(int id,MeshFilter mesh,Renderer destination,MaterialPropertyBlock block){
   var source=cubeRenderers[id];mesh.sharedMesh=source.GetComponent<MeshFilter>().sharedMesh;
   destination.sharedMaterial=source.sharedMaterial;source.GetPropertyBlock(block);destination.SetPropertyBlock(block);
  }
  /// <summary>Instantiates Blender art and resolves stable voxel identities once.</summary>
  public void Initialize(HarvestModel state,LevelOneAssets repo,HiveModelView sources,string modelKey="Flower",bool outlineEdges=false){
   model=state;assets=repo;hiveView=sources;flower=repo.Spawn(modelKey,transform).transform;flower.localScale=Vector3.one*.86f;flower.position=new Vector3(0,7.85f,0);
   flightCamera=Camera.main;if(flightCamera==null)throw new System.InvalidOperationException("Level requires a MainCamera");
   cubes=new Transform[model.Cubes.Count];cubeRenderers=new Renderer[cubes.Length];var all=flower.GetComponentsInChildren<Transform>(true);
   var byName=new Dictionary<string,Transform>(all.Length);foreach(var t in all)byName[t.name]=t;
   for(int i=0;i<cubes.Length;i++){Transform cube;if(!byName.TryGetValue(model.Cubes[i].Data.name,out cube))throw new System.InvalidOperationException("Missing source cube "+i);cubes[i]=cube;cubeRenderers[i]=cube.GetComponent<Renderer>();}
   if(outlineEdges){voxelEdges=new VoxelEdgeVisuals(cubes[0].GetComponent<MeshFilter>().sharedMesh.bounds);foreach(var renderer in cubeRenderers)voxelEdges.Apply(renderer);}
   Bounds shell=cubeRenderers[0].bounds;foreach(var renderer in cubeRenderers)shell.Encapsulate(renderer.bounds);
   // Center the entire voxel art between the top collector and slot row, including off-center stems/leaves.
   var center=flightCamera.WorldToViewportPoint(shell.center);center.x=.5f;center.y=.66f+LayoutViewportYOffset;
   var shift=flightCamera.ViewportToWorldPoint(center)-shell.center;flower.position+=shift;shell.center+=shift;
   modelLocalBounds=new Bounds(flower.InverseTransformPoint(shell.center),shell.size/flower.lossyScale.x);
   var collector=repo.Spawn("Collector",transform);collectorRoot=collector.transform;collector.transform.localScale=Vector3.one*.62f;collector.transform.position=CollectorPosition();collector.transform.rotation=Quaternion.Euler(0,180,0);
   Deposit=collector.transform.position+new Vector3(0,.45f,-.55f);foreach(var t in collector.GetComponentsInChildren<Transform>())if(t.name.StartsWith("DepositPoint")||t.name.EndsWith("_Mouth"))Deposit=t.position;
   // A rotation-invariant XZ radius keeps carriers in front at every flower angle.
   float radius=Mathf.Sqrt(shell.extents.x*shell.extents.x+shell.extents.z*shell.extents.z);
   flightPlaneZ=flower.position.z-radius-.8f;
   foreach(var renderer in collector.GetComponentsInChildren<Renderer>())flightPlaneZ=Mathf.Min(flightPlaneZ,renderer.bounds.min.z-.8f);
   collectorRest=collectorRoot.localScale;deliverySubscription=this.RegisterEvent<HarvestChangedEvent>(OnHarvestChanged);
   BuildGridMapping();PublishCameraView();Sync();
  }
  void OnHarvestChanged(HarvestChangedEvent e){if(e.Picked||collectorRoot==null)return;deliveryPulse=.20f;collectorRoot.localScale=collectorRest*1.06f;}
  void LateUpdate(){if(collectorRoot==null||model==null||model.Paused||deliveryPulse<=0)return;deliveryPulse=Mathf.Max(0,deliveryPulse-Time.deltaTime);collectorRoot.localScale=collectorRest*(1+.06f*Mathf.Sin(deliveryPulse/.20f*Mathf.PI*.5f));}
  void OnDestroy(){deliverySubscription?.UnRegister();voxelEdges?.Dispose();}
  void BuildGridMapping(){
   var cells=new Dictionary<Vector3Int,int>();for(int i=0;i<cubes.Length;i++){var d=model.Cubes[i].Data;cells.Add(new Vector3Int(d.x,d.y,d.z),i);}
   var axes=new[]{Vector3Int.right,Vector3Int.up,new Vector3Int(0,0,1)};var basis=new Vector3[3];
   for(int a=0;a<3;a++)foreach(var pair in cells){int j;if(cells.TryGetValue(pair.Key+axes[a],out j)){basis[a]=flower.InverseTransformPoint(cubes[j].position)-flower.InverseTransformPoint(cubes[pair.Value].position);break;}}
   for(int a=0;a<3;a++)if(basis[a].sqrMagnitude<.00001f)throw new System.InvalidOperationException("Flower must have a regular three-dimensional voxel grid");
   var first=model.Cubes[0].Data;var origin=flower.InverseTransformPoint(cubes[0].position)-basis[0]*first.x-basis[1]*first.y-basis[2]*first.z;
   gridLocal=Matrix4x4.identity;for(int a=0;a<3;a++)gridLocal.SetColumn(a,new Vector4(basis[a].x,basis[a].y,basis[a].z,0));gridLocal.SetColumn(3,new Vector4(origin.x,origin.y,origin.z,1));
  }
  /// <summary>Snapshots an outside lane from the intact model silhouette, so cargo leaves its source side before climbing.</summary>
  public Vector3 CargoExit(Vector3 pickup,int task){
   float left=1,right=0;
   for(int i=0;i<8;i++){
    var local=modelLocalBounds.center+Vector3.Scale(modelLocalBounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
    var screen=flightCamera.WorldToViewportPoint(flower.TransformPoint(local));left=Mathf.Min(left,screen.x);right=Mathf.Max(right,screen.x);
   }
   var point=flightCamera.WorldToViewportPoint(pickup);float center=(left+right)*.5f;
   bool exitLeft=Mathf.Abs(point.x-center)<.025f?(task&1)==0:point.x<center;
   point.x=exitLeft?Mathf.Clamp(left-.065f,.075f,.925f):Mathf.Clamp(right+.065f,.075f,.925f);
   point.y=Mathf.Clamp(point.y+.018f+(task%3)*.008f,.10f,.88f);
   return FlightPoint(flightCamera.ViewportToWorldPoint(point));
  }
  Vector3 ScreenRoutePoint(Vector3 reference,float x,float y){var point=flightCamera.WorldToViewportPoint(reference);point.x=x;point.y=y;return FlightPoint(flightCamera.ViewportToWorldPoint(point));}
  public Vector3 ApproachPoint(Vector3 target,int lane){return ApproachPoint(target,lane,0);}
  /// <summary>Empty bees stage outside the intact model's left or right silhouette before approaching their block.</summary>
  public Vector3 ApproachPoint(Vector3 target,int lane,int task){
   float left=1,right=0;
   for(int i=0;i<8;i++){
    var local=modelLocalBounds.center+Vector3.Scale(modelLocalBounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
    var screen=flightCamera.WorldToViewportPoint(flower.TransformPoint(local));left=Mathf.Min(left,screen.x);right=Mathf.Max(right,screen.x);
   }
   var p=flightCamera.WorldToViewportPoint(target);float margin=.065f+(task%3)*.006f;
   p.x=Mathf.Clamp(lane==0?left-margin:right+margin,.075f,.925f);
   p.y=Mathf.Clamp(p.y+.018f+(task%3)*.008f,.43f,.86f);
   return FlightPoint(flightCamera.ViewportToWorldPoint(p));
  }
  /// <summary>Fan upward below the model before joining its outside corridor, rather than crossing a flat shelf.</summary>
  public Vector3 DepartureTurn(Vector3 emergence,Vector3 outside){
   float bottom=float.PositiveInfinity;
   for(int i=0;i<8;i++){
    var local=modelLocalBounds.center+Vector3.Scale(modelLocalBounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
    bottom=Mathf.Min(bottom,flightCamera.WorldToViewportPoint(flower.TransformPoint(local)).y);
   }
   var from=flightCamera.WorldToViewportPoint(emergence);var side=flightCamera.WorldToViewportPoint(outside);
   return ScreenRoutePoint(outside,side.x,Mathf.Max(from.y,Mathf.Min(side.y,bottom-.025f)));
  }
  public Vector3 CarryLanePoint(Vector3 pickup,int lane){return CarryLanePoint(pickup,lane,0);}
  /// <summary>Only a short local surface escape precedes the direct collector curve.</summary>
  public Vector3 CarryLanePoint(Vector3 pickup,int lane,int task){
   var offset=flightCamera.transform.right*((lane==0?-1:1)*(.48f+(task%3)*.06f))+flightCamera.transform.up*(.16f+(task%4)*.035f);
   return FlightPoint(pickup+offset);
  }
  public Vector3 EntryWaitingPoint(int lane){return EntryWaitingPoint(lane,0);}
  public Vector3 EntryWaitingPoint(int lane,int task){
   return FlightPoint(Deposit)+flightCamera.transform.right*((lane==0?-1:1)*(.16f+(task%3)*.045f))-flightCamera.transform.up*(.32f+(task%2)*.045f);
  }
  // Retained for existing callers; completed carriers no longer use an empty-return route.
  public Vector3 EmptyExitPoint(Vector3 from){return FlightPoint(from);}
  public Vector3 EmptyReturnCorner(Vector3 from,Vector3 origin){return FlightPoint(origin);}
  /// <summary>Publishes a read-only camera snapshot through a command before target reservation or pickup.</summary>
  public void PublishCameraView(){
   if(model==null||flightCamera==null)return;var inverse=(flower.localToWorldMatrix*gridLocal).inverse;
   this.SendCommand(new UpdateHarvestViewCommand(inverse.MultiplyVector(-flightCamera.transform.forward),inverse.MultiplyPoint3x4(flightCamera.transform.position),flightCamera.orthographic));
  }
  /// <summary>Centers the collector directly above the flower in the angled camera's viewport.</summary>
  static Vector3 CollectorPosition(){
   var camera=Camera.main;if(camera==null)throw new System.InvalidOperationException("LevelOne requires a MainCamera");
   var ray=camera.ViewportPointToRay(new Vector3(.5f,.94f,0));float distance;
   var plane=new Plane(Vector3.forward,new Vector3(0,0,-.45f));
   if(!plane.Raycast(ray,out distance))throw new System.InvalidOperationException("Collector placement plane is outside the camera view");
   return ray.GetPoint(distance);
  }
  /// <summary>Resolves the actual hive exit after an exhausted hive releases its slot.</summary>
  public Vector3 Source(int hive){return hiveView.Source(hive);}
  /// <summary>Returns the front approach point of an independently harvestable cube.</summary>
  public Vector3 Target(int id){return cubes[id].position+new Vector3(0,0,-.28f);}
  /// <summary>Projects a logical endpoint onto the safe foreground plane without changing its screen position.</summary>
  public Vector3 FlightPoint(Vector3 world){
   var forward=flightCamera.transform.forward;
   return world+forward*((flightPlaneZ-world.z)/forward.z);
  }
  /// <summary>Applies model visibility and slot ownership without changing gameplay state.</summary>
  public void Sync(){
   if(model==null||revision==model.Revision)return;revision=model.Revision;
   if(visualGeneration!=model.Generation){visualGeneration=model.Generation;deliveryPulse=0;if(collectorRoot!=null)collectorRoot.localScale=collectorRest;}
   for(int i=0;i<cubes.Length;i++)cubeRenderers[i].enabled=model.Cubes[i].Stage<2;
  }
  /// <summary>Turns clockwise from above; manual and modal input retain control of the angle.</summary>
  public void TickAutoRotation(float dt){
   if(!AutoRotate||model==null||model.Paused||model.Won||model.Deadlocked||uiInputBlocked||rotating||dt<=0||float.IsNaN(dt)||float.IsInfinity(dt))return;
   rotation=Mathf.Repeat(flower.eulerAngles.y+10f*dt,360f);flower.rotation=Quaternion.Euler(0,rotation,0);
  }
  /// <summary>Restores the flower's initial direction when retrying the level.</summary>
  public void ResetRotation(){rotation=0;rotating=false;flower.rotation=Quaternion.identity;}
  /// <summary>Rotates only presentation when dragging inside the model area.</summary>
  public void ObserveInput(){
   if(model==null||model.Paused||uiInputBlocked){rotating=false;return;}
   float y=Input.mousePosition.y/Screen.height;
   if(Input.GetMouseButtonDown(0)&&y>.4f&&y<.88f){press=Input.mousePosition;rotation=flower.eulerAngles.y;rotating=true;}
   if(!Input.GetMouseButton(0))rotating=false;
   if(rotating){Vector2 current=Input.mousePosition;rotation+=(current.x-press.x)*.18f;press=current;flower.rotation=Quaternion.Euler(0,rotation,0);}
  }
 }
}
