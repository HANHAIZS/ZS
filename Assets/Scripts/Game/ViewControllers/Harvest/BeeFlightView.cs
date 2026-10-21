using System.Collections.Generic;
using UnityEngine;
using QFramework;

namespace BeeHarvest
{
    /// <summary>Owns continuous carrier movement; body animation never writes the root position.</summary>
    public sealed class BeeFlightView : BeeHarvestController
    {
        const float ReachDepth=.07f, PickupScale=.12f, DepartureScale=.18f;
        const float EmptySpeed=13f, LoadedSpeed=24f;

        sealed class Actor
        {
            public GameObject Root, Cargo;
            public Transform Body, Grip;
            public BeeGraspPose Grasp;
            public string Color;
            public Quaternion Aim, PickupAim, CargoRotation;
            public Renderer CargoRenderer;
            public MeshFilter CargoFilter;
            public readonly MaterialPropertyBlock CargoBlock=new MaterialPropertyBlock();
            public Bounds Envelope=new Bounds(Vector3.zero,Vector3.one*1.1f);
            public Vector3 Origin, Position, Pickup, ExtractEnd, Exit, EntryStart, SourceScale;
            public int Stage=-1, Phase=-1;
            public float Clock, DepartureProgress;
            public bool HasPose, ReachedDepartureRise, ReachedDepartureSide;
        }

        readonly Dictionary<int,Actor> active=new Dictionary<int,Actor>();
        readonly Dictionary<string,Stack<Actor>> pools=new Dictionary<string,Stack<Actor>>();
        HarvestModel model;
        HarvestBoardView board;
        LevelOneAssets assets;
        Camera levelCamera;
        int generation=-1;
        bool hasProjection;
        Matrix4x4 lastProjection;

        public void Initialize(HarvestModel state,HarvestBoardView view,LevelOneAssets repo)
        {
            model=state;board=view;assets=repo;levelCamera=Camera.main;
            this.SendCommand(new AttachBeeFlightPresentationCommand(GetInstanceID()));
        }

        Stack<Actor> Pool(string color)
        {
            Stack<Actor> pool;
            if(!pools.TryGetValue(color,out pool)){pool=new Stack<Actor>();pools.Add(color,pool);}
            return pool;
        }

        Actor Create(string color)
        {
            var a=new Actor{Root=new GameObject("BlenderBeeCarrier"),Color=color};
            a.Root.transform.SetParent(transform,false);
            a.Body=assets.Spawn(LevelOneAssets.BeeKey(color),a.Root.transform).transform;
            a.Body.localScale=Vector3.one*PickupScale;
            a.Grasp=new BeeGraspPose(a.Body);
            a.Grip=new GameObject("GripAnchor").transform;
            a.Grip.SetParent(a.Body,false);a.Grip.localPosition=new Vector3(0,-1.5f,1.65f);
            a.Cargo=new GameObject("CarriedVoxel");a.Cargo.transform.SetParent(a.Root.transform,false);
            a.CargoFilter=a.Cargo.AddComponent<MeshFilter>();a.CargoFilter.sharedMesh=board.CargoMesh;
            a.CargoRenderer=a.Cargo.AddComponent<MeshRenderer>();a.CargoRenderer.sharedMaterial=board.CargoMaterial;
            return a;
        }

        void Recycle(int id,Actor actor)
        {
            actor.Root.SetActive(false);Pool(actor.Color).Push(actor);active.Remove(id);
        }

        void OnDestroy()
        {
            if(model!=null)this.SendCommand(new DetachBeeFlightPresentationCommand(GetInstanceID()));
            foreach(var a in active.Values){a.Grasp.Dispose();Destroy(a.Root);}
            foreach(var pool in pools.Values)foreach(var a in pool){a.Grasp.Dispose();Destroy(a.Root);}
        }

        Vector3 KeepInsideView(Vector3 position,Bounds envelope)
        {
            var bounds=new Bounds(position+envelope.center,envelope.size);
            var lo=Vector3.one*float.PositiveInfinity;var hi=Vector3.one*float.NegativeInfinity;
            for(int i=0;i<8;i++){
                var p=levelCamera.WorldToViewportPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));
                lo=Vector3.Min(lo,p);hi=Vector3.Max(hi,p);
            }
            var center=levelCamera.WorldToViewportPoint(bounds.center);
            center.x+=lo.x<.01f?.01f-lo.x:hi.x>.99f?.99f-hi.x:0;
            center.y+=lo.y<.01f?.01f-lo.y:hi.y>.99f?.99f-hi.y:0;
            return position+levelCamera.ViewportToWorldPoint(center)-bounds.center;
        }

        static Vector3 Curve(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)
        {float u=1-t;return u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d;}

        void FanControls(Vector3 emergence,Vector3 corner,out Vector3 b,out Vector3 c)
        {
            var rise=levelCamera.transform.up*Mathf.Max(0,Vector3.Dot(corner-emergence,levelCamera.transform.up));
            b=emergence+rise*.6f;c=corner-rise*.35f;
        }

        void SideControls(Actor a,Vector3 corner,Vector3 outside,int id,out Vector3 b,out Vector3 c)
        {
            var rise=outside-corner;
            float side=Vector3.Dot(corner-a.Origin,levelCamera.transform.right)<0?-1:1;
            var bend=levelCamera.transform.right*(side*(.12f+(id%3)*.03f));
            b=corner+rise*.3f+bend;c=outside-rise*.3f+bend;
        }

        static float CurveSeconds(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float minimum)
        {
            // The cubic derivative is bounded by three times its longest control edge.
            return Mathf.Max(minimum,3*Mathf.Max(Vector3.Distance(a,b),Mathf.Max(Vector3.Distance(b,c),Vector3.Distance(c,d)))/EmptySpeed);
        }

        float StepDeparture(Actor a,Vector3 emergence,Vector3 corner,Vector3 outside,float ceiling,float elapsed,int id)
        {
            float end,rate;Vector3 b,c;
            if(a.DepartureProgress<.16f){end=.16f;rate=1/AdvanceHarvestCommand.ApproachSeconds;}
            else if(!a.ReachedDepartureRise)return a.DepartureProgress;
            else if(a.DepartureProgress<.36f){
                end=.36f;FanControls(emergence,corner,out b,out c);
                rate=.20f/CurveSeconds(emergence,b,c,corner,AdvanceHarvestCommand.ApproachSeconds*.20f);
            }else if(!a.ReachedDepartureSide)return a.DepartureProgress;
            else{
                end=.65f;SideControls(a,corner,outside,id,out b,out c);
                rate=.29f/CurveSeconds(corner,b,c,outside,AdvanceHarvestCommand.ApproachSeconds*.29f);
            }
            a.DepartureProgress=Mathf.Min(ceiling,Mathf.Min(end,a.DepartureProgress+elapsed*rate));
            return a.DepartureProgress;
        }

        Vector3 Approach(Actor a,Vector3 emergence,Vector3 corner,Vector3 outside,Vector3 station,float u,int id)
        {
            // Leave the mouth upward first; timers cannot skip either physical departure waypoint.
            if(!a.ReachedDepartureRise)u=Mathf.Min(u,.16f);
            if(u<=.16f)return Vector3.Lerp(a.Origin,emergence,Mathf.SmoothStep(0,1,u/.16f));
            if(!a.ReachedDepartureSide)u=Mathf.Min(u,.36f);
            Vector3 b,c;
            if(u<=.36f){FanControls(emergence,corner,out b,out c);return Curve(emergence,b,c,corner,(u-.16f)/.20f);}
            if(u>=.65f)return Vector3.Lerp(outside,station,Mathf.SmoothStep(0,1,(u-.65f)/.35f));
            SideControls(a,corner,outside,id,out b,out c);
            return Curve(corner,b,c,outside,(u-.36f)/.29f);
        }

        Vector3 Carry(Actor a,Vector3 waiting,float time,int id)
        {
            if(time<=AdvanceHarvestCommand.EscapeSeconds)
                return Vector3.Lerp(a.ExtractEnd,a.Exit,Mathf.SmoothStep(0,1,time/AdvanceHarvestCommand.EscapeSeconds));
            float t=Mathf.Clamp01((time-AdvanceHarvestCommand.EscapeSeconds)/(AdvanceHarvestCommand.ReturnSeconds-AdvanceHarvestCommand.EntrySeconds-AdvanceHarvestCommand.EscapeSeconds));
            var delta=waiting-a.Exit;
            var bend=levelCamera.transform.right*(Mathf.Sin(id*2.39996f)*.38f);
            return Curve(a.Exit,a.Exit+delta*.34f+bend,waiting-delta*.18f+bend*.4f,waiting,Mathf.SmoothStep(0,1,t));
        }

        public void RenderFlights()
        {
            if(model==null)return;
            bool reframed=hasProjection&&lastProjection!=levelCamera.projectionMatrix;
            lastProjection=levelCamera.projectionMatrix;hasProjection=true;
            if(generation!=model.Generation){
                foreach(var old in active.Values){old.Root.SetActive(false);Pool(old.Color).Push(old);}
                active.Clear();generation=model.Generation;
            }
            foreach(var f in model.Flights){
                Actor a;
                if(f.Stage==2){if(active.TryGetValue(f.Id,out a))Recycle(f.Id,a);continue;}
                if(!active.TryGetValue(f.Id,out a)){
                    var color=LevelOneAssets.PaletteKey(model.Hives[f.Hive].Data.color);var pool=Pool(color);
                    a=pool.Count>0?pool.Pop():Create(color);active.Add(f.Id,a);
                    a.Root.name="BlenderBeeCarrier_"+f.Id;a.Root.SetActive(true);
                    a.Root.transform.localScale=Vector3.one;a.Body.localPosition=Vector3.zero;
                    a.Stage=-1;a.Phase=-1;a.HasPose=false;a.ReachedDepartureRise=false;a.ReachedDepartureSide=false;a.DepartureProgress=0;a.Clock=f.Age;
                    a.Origin=board.FlightPoint(board.Source(f.Hive));a.Position=a.Origin;
                }
                if(model.Paused&&a.HasPose)continue;
                if(reframed&&a.HasPose)a.Position=KeepInsideView(a.Position,a.Envelope);
                float elapsed=Mathf.Max(0,f.Age-a.Clock);
                var source=board.PickupPose(f.Cube);var normal=board.PickupOutward(f);
                var tangent=Vector3.ProjectOnPlane(Vector3.down,normal);
                if(tangent.sqrMagnitude<.01f)tangent=Vector3.ProjectOnPlane(levelCamera.transform.forward,normal);
                var pickupAim=Quaternion.LookRotation(tangent.normalized,normal);
                var station=source.position-pickupAim*(a.Grip.localPosition*PickupScale)+Vector3.up*ReachDepth;
                var outside=KeepInsideView(board.ApproachPoint(station,f.Lane,f.Id),a.Envelope);
                var emergence=KeepInsideView(board.FlightPoint(a.Origin+levelCamera.transform.up*.85f),a.Envelope);
                var corner=KeepInsideView(board.DepartureTurn(emergence,outside),a.Envelope);
                if(f.Stage==0&&!a.ReachedDepartureRise&&Vector3.Distance(a.Position,emergence)<.02f)a.ReachedDepartureRise=true;
                if(f.Stage==0&&a.ReachedDepartureRise&&!a.ReachedDepartureSide&&Vector3.Distance(a.Position,corner)<.02f)a.ReachedDepartureSide=true;
                float approachU=Mathf.Clamp01(f.Progress/AdvanceHarvestCommand.ApproachSeconds);
                if(f.Stage==0&&!f.ClearedApproach)approachU=StepDeparture(a,emergence,corner,outside,approachU,elapsed,f.Id);
                var entrance=board.FlightPoint(board.Deposit);
                var waiting=board.EntryWaitingPoint(f.Lane,f.Id);

                if(f.Stage==1&&a.Stage!=1){
                    a.Pickup=source.position;a.ExtractEnd=source.position+normal*.24f;
                    a.SourceScale=board.PickupScale(f.Cube);a.PickupAim=pickupAim;
                    a.CargoRotation=Quaternion.Inverse(pickupAim)*source.rotation;
                    board.CopyPickupVisual(f.Cube,a.CargoFilter,a.CargoRenderer,a.CargoBlock);
                }
                a.Exit=board.CarryLanePoint(a.ExtractEnd,f.Lane,f.Id);
                bool waitingForPickup=f.Stage==0&&f.ClearedApproach&&!f.CollectionGranted;
                bool collecting=f.Stage==0&&f.CollectionGranted&&f.Progress>=AdvanceHarvestCommand.ApproachSeconds;
                float reach=f.Stage==0?Mathf.SmoothStep(0,1,Mathf.Clamp01((f.Progress-AdvanceHarvestCommand.ApproachSeconds-AdvanceHarvestCommand.SettleSeconds)/AdvanceHarvestCommand.ReachSeconds)):0;
                float lift=f.Stage==1&&f.TransportPhase==0?Mathf.SmoothStep(0,1,Mathf.Clamp01(f.PhaseProgress/AdvanceHarvestCommand.LiftSeconds)):1;
                Vector3 path,look;
                if(f.Stage==0){
                    float u=approachU;
                    path=Approach(a,emergence,corner,outside,station,u,f.Id);
                    if(waitingForPickup)path+=levelCamera.transform.right*(Mathf.Sin(f.Age*5+f.Id)*.055f)+levelCamera.transform.up*(Mathf.Sin(f.Age*7+f.Id)*.025f);
                    look=waitingForPickup?station-path:Approach(a,emergence,corner,outside,station,Mathf.Clamp01(u+.025f),f.Id)-a.Position;
                }else{
                    if(f.TransportPhase==0)path=Vector3.Lerp(a.Pickup,a.ExtractEnd,lift);
                    else if(f.TransportPhase==1)path=a.ExtractEnd;
                    else if(f.TransportPhase==2)path=Carry(a,waiting,f.PhaseProgress,f.Id);
                    else if(f.TransportPhase==3)path=waiting+levelCamera.transform.right*(Mathf.Sin(f.Age*8+f.Id)*.025f);
                    else{
                        if(a.Phase!=4)a.EntryStart=a.HasPose?a.Cargo.transform.position:waiting;
                        path=Vector3.Lerp(a.EntryStart,entrance,Mathf.SmoothStep(0,1,Mathf.Clamp01(f.PhaseProgress/AdvanceHarvestCommand.EntrySeconds)));
                    }
                    look=f.TransportPhase==2?Carry(a,waiting,f.PhaseProgress+.02f,f.Id)-path:f.TransportPhase==4?entrance-a.Position:Vector3.up;
                }

                var goal=look.sqrMagnitude>.00001f?Quaternion.LookRotation(-look.normalized,-levelCamera.transform.forward):a.Aim;
                if(f.Stage==0){float blend=Mathf.SmoothStep(0,1,Mathf.Clamp01((approachU-.65f)/.35f));goal=Quaternion.Slerp(goal,pickupAim,blend);}
                if(f.Stage==1&&f.TransportPhase<2)goal=a.PickupAim;
                if(!a.HasPose)a.Aim=goal;else a.Aim=Quaternion.RotateTowards(a.Aim,goal,360*elapsed);
                float carryWeight=f.Stage==1&&f.TransportPhase>=2?Mathf.Min(1,f.PhaseProgress/.08f):0;
                float bank=((f.Lane==0?-1:1)*10+Mathf.Sin(f.Age*8+f.Id)*3)*carryWeight;
                if(waitingForPickup)bank=Mathf.Sin(f.Age*5+f.Id)*9;
                a.Body.localRotation=Quaternion.AngleAxis(bank,levelCamera.transform.forward)*a.Aim*Quaternion.AngleAxis(bank*.4f,Vector3.forward);
                float scale=f.Stage==0?Mathf.Lerp(DepartureScale,PickupScale,Mathf.SmoothStep(0,1,Mathf.Clamp01(approachU/.8f))):PickupScale;
                a.Body.localScale=Vector3.one*scale;
                a.Body.localPosition=Vector3.down*(ReachDepth*(f.Stage==0?reach:f.TransportPhase==0?1-lift:0));
                var gripOffset=a.Body.localRotation*(a.Grip.localPosition*scale)+a.Body.localPosition;
                var desired=f.Stage==1?path-gripOffset:path;
                if(!collecting&&!(f.Stage==1&&f.TransportPhase==0))desired=KeepInsideView(desired,a.Envelope);
                if(!a.HasPose&&f.Stage==1)a.Position=desired;
                else if(a.HasPose)a.Position=Vector3.MoveTowards(a.Position,desired,(f.Stage==0?EmptySpeed:LoadedSpeed)*elapsed);
                a.Root.transform.position=a.Position;a.Root.transform.rotation=Quaternion.identity;a.Root.transform.localScale=Vector3.one;
                a.Cargo.SetActive(f.Stage==1);
                if(f.Stage==1){
                    a.Cargo.transform.position=a.Grip.position;a.Cargo.transform.rotation=a.Body.rotation*a.CargoRotation;
                    var parent=a.Cargo.transform.parent.lossyScale;
                    a.Cargo.transform.localScale=new Vector3(a.SourceScale.x/parent.x,a.SourceScale.y/parent.y,a.SourceScale.z/parent.z);
                }
                var gripPose=f.Stage==0?source:new Pose(a.Grip.position,a.Body.rotation*a.CargoRotation);
                var half=Vector3.Scale(a.CargoFilter.sharedMesh.bounds.extents,f.Stage==0?board.PickupScale(f.Cube):a.SourceScale);
                a.Grasp.Grasp(gripPose,half,f.Stage==0?reach:1);a.Grasp.Wings(f.Age,f.Id);
                bool arrived=f.Stage==0?f.CollectionGranted&&Vector3.Distance(a.Position,station)<.012f&&Quaternion.Angle(a.Body.localRotation,pickupAim)<2.5f:
                    (f.TransportPhase==2||f.TransportPhase==4)&&Vector3.Distance(a.Cargo.transform.position,f.TransportPhase==2?waiting:entrance)<.015f;
                bool outsideReached=f.ClearedSource||(f.Stage==1&&f.TransportPhase==2&&Vector3.Distance(a.Cargo.transform.position,KeepInsideView(a.Exit-gripOffset,a.Envelope)+gripOffset)<.015f);
                this.SendCommand(new ConfirmBeeFlightArrivalCommand(model.Generation,f.Id,f.Stage,f.TransportPhase,arrived,outsideReached,f.ClearedApproach||(f.Stage==0&&Vector3.Distance(a.Position,outside)<.02f)));
                a.Stage=f.Stage;a.Phase=f.TransportPhase;a.Clock=f.Age;a.HasPose=true;
            }
        }
    }
}
