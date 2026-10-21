using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest
{
    public sealed class HiveModelView : BeeHarvestController
    {
        public Material SlotMaterial;
        const float SlotHeight = .06f, TileRatio = 1.18f;
        sealed class Visual
        {
            public GameObject Root;
            public Renderer[] Renderers;
            public Renderer Plaque;
            public Transform Exit, NumberAnchor;
            public Vector3 Source;
            public bool HasSource;
            public float WaitingElapsed=-1;
            public int WaitingGeneration, WaitingPage, WaitingColumn, WaitingRow;
        }

        readonly Dictionary<int, Visual> visuals = new Dictionary<int, Visual>();
        readonly Vector3[] slotCenters = new Vector3[6];
        readonly Transform[] slotTiles = new Transform[6];
        HarvestModel model;
        HarvestSystem rules;
        LevelOneAssets assets;
        LevelOneHud hud;
        Camera levelCamera;
        Canvas canvas;
        int revision = -1, generation = -1, queuePage = -1, hudLayoutVersion = -1;
        Vector2 canvasSize, safeMin, safeMax;
        float aspect = -1, height = -1;
        Vector3 cameraPosition;
        Quaternion cameraRotation;
        int[] hoppingHives,hoppingSlots;
        Vector2[] hopStarts;
        float hopElapsed;
        Material linkMaterial;
        readonly Dictionary<int,HiveChainVisual> groupLinks=new Dictionary<int,HiveChainVisual>();

        readonly Dictionary<int,HiveMysteryVisual> mysteries = new Dictionary<int,HiveMysteryVisual>();
        bool hasMysteries;
        int waitingBounces;

        /// <summary>Caches the local model, resources, controls and projection once.</summary>
        public void Initialize(HarvestModel state, HarvestSystem system, LevelOneAssets repo, LevelOneHud controls)
        {
            model = state;
            rules = system;
            assets = repo;
            hud = controls;
            levelCamera = Camera.main;
            canvas = hud.GetComponentInParent<Canvas>();
            if (levelCamera == null || canvas == null) throw new InvalidOperationException("Hive models require the level camera and HUD canvas.");
            if (SlotMaterial == null || hud.SlotTargets == null || hud.SlotTargets.Length != 6)
                throw new InvalidOperationException("Hive slots require the tile material and six slot centers.");
            for (int i = 0; i < 6; i++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.name = "HiveSlotTile_" + i;
                tile.transform.SetParent(transform, false);
                Destroy(tile.GetComponent<Collider>());
                tile.GetComponent<Renderer>().sharedMaterial = SlotMaterial;
                slotTiles[i] = tile.transform;
            }
            if(model.HasLinkedGroups)linkMaterial=repo.RoundedChainMaterial();
            Canvas.ForceUpdateCanvases();
            Sync();
        }

        /// <summary>Returns the actual exit saved for the hive, even after its slot is released.</summary>
        public Vector3 Source(int hive)
        {
            Visual visual;
            if (!visuals.TryGetValue(hive, out visual) || !visual.HasSource) Sync();
            if (!visuals.TryGetValue(hive, out visual) || !visual.HasSource)
                throw new InvalidOperationException("Hive departure point was not initialized: " + hive);
            return visual.Source;
        }

        /// <summary>Starts a presentation-only hop with the existing model and quota label.</summary>
        public void BeginHop(int hive,int column,int slot){BeginHops(new[]{hive},new[]{column},new[]{slot});}
        public void BeginHops(int[] hives,int[] columns,int[] destinations){
            Sync();hoppingHives=hives;hoppingSlots=destinations;hopStarts=new Vector2[hives.Length];hopElapsed=0;
            for(int i=0;i<hives.Length;i++)hopStarts[i]=levelCamera.WorldToScreenPoint(GetVisual(hives[i]).Root.transform.position);
            RenderHop();RenderConnections();
        }
        public void ShowHop(float seconds){hopElapsed=seconds;RenderHop();RenderConnections();}
        public void EndHop(){hoppingHives=hoppingSlots=null;hopStarts=null;revision=-1;Sync();}
        void RenderHop(){
            if(hoppingHives==null)return;
            // The brief press is part of the original travel budget, so command timing stays unchanged.
            const float pressSeconds=.045f;
            float press=Mathf.Clamp01(hopElapsed/pressSeconds);
            float travel=Mathf.Clamp01((hopElapsed-pressSeconds)/(HiveHopController.TravelSeconds-pressSeconds));
            float eased=travel*travel*(3-2*travel);
            float landing=Mathf.Clamp01((hopElapsed-HiveHopController.TravelSeconds)/HiveHopController.LandingSeconds);
            float compression=hopElapsed<pressSeconds?.04f*(1-press)+.10f*Mathf.Sin(press*Mathf.PI):0;
            float stretch=-compression+.12f*Mathf.Sin(travel*Mathf.PI)-.09f*Mathf.Sin(landing*Mathf.PI);
            for(int i=0;i<hoppingHives.Length;i++){
                int hive=hoppingHives[i],slot=hoppingSlots[i];var screen=Vector2.Lerp(hopStarts[i],slotCenters[slot],eased);screen.y+=(Mathf.Sin(travel*Mathf.PI)*.045f-compression*.008f)*levelCamera.pixelHeight;
                var control=hud.WaitingControl(hive);float startHeight=control.rect.height*canvas.scaleFactor/levelCamera.pixelHeight*.9f;var visual=GetVisual(hive);
                Place(visual,control,Mathf.Lerp(startHeight,SlotHeight,eased),screen,-2);
                visual.Root.transform.localScale=Vector3.Scale(visual.Root.transform.localScale,new Vector3(1-stretch*.5f,1+stretch,1-stretch*.5f));AlignLabel(hud.WaitingLabel(hive),visual);
                ApplyMystery(hive,visual,hud.WaitingLabel(hive));
            }
        }

        /// <summary>Plays feedback only for a visible hive behind its actual queue front.</summary>
        public void BeginWaitingBounce(int hive)
        {
            int column=rules.WaitingColumnOf(hive);
            if(column<0||rules.Front(column)==hive||hoppingHives!=null)return;
            int row=-1;for(int r=0;r<hud.VisibleRows;r++)if(hud.DisplayedHive(column,r)==hive){row=r;break;}
            if(row<0)return;
            var visual=GetVisual(hive);if(visual.WaitingElapsed>=0)return;
            visual.WaitingElapsed=0;visual.WaitingGeneration=model.Generation;visual.WaitingPage=hud.QueuePage;
            visual.WaitingColumn=column;visual.WaitingRow=row;waitingBounces++;revision=-1;Sync();
        }
        public void TickWaitingBounces(float seconds)
        {
            if(waitingBounces==0)return;
            foreach(var item in visuals){var visual=item.Value;if(visual.WaitingElapsed<0)continue;
                visual.WaitingElapsed+=Mathf.Max(0,seconds);
                if(!WaitingBounceValid(item.Key,visual)||visual.WaitingElapsed>=HiveHopController.TravelSeconds+HiveHopController.LandingSeconds){visual.WaitingElapsed=-1;waitingBounces--;}
            }
            revision=-1;Sync();
        }
        public void CancelWaitingBounces()
        {
            if(waitingBounces==0)return;
            foreach(var visual in visuals.Values)visual.WaitingElapsed=-1;
            waitingBounces=0;revision=-1;Sync();
        }
        bool WaitingBounceValid(int hive,Visual visual)
        {
            return visual.WaitingGeneration==model.Generation&&visual.WaitingPage==hud.QueuePage
                &&hud.DisplayedHive(visual.WaitingColumn,visual.WaitingRow)==hive&&rules.Front(visual.WaitingColumn)!=hive;
        }
        void RenderWaitingBounces()
        {
            if(waitingBounces==0)return;
            foreach(var item in visuals){var visual=item.Value;if(visual.WaitingElapsed<0)continue;
                if(!WaitingBounceValid(item.Key,visual)){visual.WaitingElapsed=-1;waitingBounces--;continue;}
                float time=visual.WaitingElapsed,press=Mathf.Clamp01(time/.045f);
                float travel=Mathf.Clamp01((time-.045f)/(HiveHopController.TravelSeconds-.045f));
                float landing=Mathf.Clamp01((time-HiveHopController.TravelSeconds)/HiveHopController.LandingSeconds);
                float compression=time<.045f?.04f*(1-press)+.10f*Mathf.Sin(press*Mathf.PI):0;
                float stretch=-compression+.12f*Mathf.Sin(travel*Mathf.PI)-.09f*Mathf.Sin(landing*Mathf.PI);
                // Normal placement above supplies the baseline every refresh; never accumulate offsets.
                var root=visual.Root.transform;
                root.position+=levelCamera.transform.up*((Mathf.Sin(travel*Mathf.PI)*.0125f-compression*.003f)*levelCamera.orthographicSize*2);
                root.localScale=Vector3.Scale(root.localScale,new Vector3(1-stretch*.5f,1+stretch,1-stretch*.5f));
                var label=hud.RowLabel(visual.WaitingColumn,visual.WaitingRow);AlignLabel(label,visual);
                if(visual.WaitingRow==2)label.rectTransform.position+=Vector3.up*levelCamera.pixelHeight*.018f;
                HiveMysteryVisual mystery;if(mysteries.TryGetValue(item.Key,out mystery))mystery.AlignQuestion(levelCamera);
            }
        }

        /// <summary>Reuses Blender hive instances and follows their count plaques with HUD labels.</summary>
        public void Sync()
        {
            if (model == null || levelCamera == null || canvas == null || hud == null) return;
            var safe = (RectTransform)hud.transform;
            bool layout = hudLayoutVersion != hud.LayoutVersion || canvasSize != canvas.pixelRect.size || aspect != levelCamera.aspect || height != levelCamera.orthographicSize
                || safeMin != safe.anchorMin || safeMax != safe.anchorMax
                || cameraPosition != levelCamera.transform.position || cameraRotation != levelCamera.transform.rotation;
            if (revision == model.Revision && !layout && queuePage == hud.QueuePage) return;
            queuePage = hud.QueuePage;
            hudLayoutVersion = hud.LayoutVersion;
            if (layout) Canvas.ForceUpdateCanvases();
            revision = model.Revision;
            canvasSize = canvas.pixelRect.size;
            aspect = levelCamera.aspect;
            height = levelCamera.orthographicSize;
            safeMin = safe.anchorMin;
            safeMax = safe.anchorMax;
            cameraPosition = levelCamera.transform.position;
            cameraRotation = levelCamera.transform.rotation;
            if (generation != model.Generation || layout)
            {
                if (generation != model.Generation){hoppingHives=hoppingSlots=null;hopStarts=null;ClearConnections();ClearMysteries();
                    hasMysteries=false;foreach(var hive in model.Hives)hasMysteries|=hive.Data.isHidden;}
                generation = model.Generation;
                foreach (var visual in visuals.Values) visual.HasSource = false;
            }
            foreach (var mystery in mysteries.Values) mystery.Hide();
            foreach (var visual in visuals.Values) visual.Root.SetActive(false);
            for (int i = 0; i < 6; i++)
            {
                slotTiles[i].gameObject.SetActive(i < model.Slots.Count);
                hud.SlotLabels[i].rectTransform.anchoredPosition = Vector2.zero;
                if (i < 5) { hud.QueueLabels[i].rectTransform.anchoredPosition = Vector2.zero; hud.NextLabels[i].rectTransform.anchoredPosition = Vector2.zero;if(hud.VisibleRows==3)hud.ThirdLabels[i].rectTransform.anchoredPosition=Vector2.zero; }
                slotCenters[i] = hud.SlotLabels[i].rectTransform.position;
                LayoutSlot(i);
            }
            for (int s = 0; s < model.Slots.Count; s++)
            {
                int hive = model.Slots[s];
                if (hive < 0) continue;
                var visual = GetVisual(hive);
                Place(visual, hud.SlotLabels[s].rectTransform, SlotHeight, slotCenters[s]);
                visual.Source = visual.Exit.position;
                visual.HasSource = true;
                AlignLabel(hud.SlotLabels[s], visual);
                ApplyMystery(hive,visual,hud.SlotLabels[s]);
            }
            for (int column = 0; column < 5; column++)
            {
                for(int row=0;row<hud.VisibleRows;row++)
                {
                    int hive=rules.WaitingAt(column,hud.QueuePage*hud.VisibleRows+row);
                    if(hive<0)continue;
                    var visual=GetVisual(hive);
                    Place(visual,hud.RowControl(column,row),0);
                    AlignLabel(hud.RowLabel(column,row),visual);
                    // Row three is behind the toolbar; keep its quota in the exposed upper half.
                    if(row==2)hud.RowLabel(column,row).rectTransform.position+=Vector3.up*levelCamera.pixelHeight*.018f;
                    ApplyMystery(hive,visual,hud.RowLabel(column,row));
                }
            }
            // A quota-one hive can release its slot before its first view refresh.
            foreach (var flight in model.Flights)
            {
                if (flight.Stage == 2) continue;
                var visual = GetVisual(flight.Hive);
                if (visual.HasSource) continue;
                Place(visual, hud.SlotLabels[flight.Slot].rectTransform, SlotHeight, slotCenters[flight.Slot]);
                visual.Source = visual.Exit.position;
                visual.HasSource = true;
                visual.Root.SetActive(false);
            }
            if(layout&&hoppingHives!=null)for(int i=0;i<hoppingHives.Length;i++)hopStarts[i]=RectTransformUtility.WorldToScreenPoint(null,hud.WaitingControl(hoppingHives[i]).position);
            RenderHop();RenderWaitingBounces();RenderConnections();
        }

        void ClearConnections()
        {
            foreach (var link in groupLinks.Values) link.Dispose();
            groupLinks.Clear();
        }

        void OnDestroy() { ClearConnections(); ClearMysteries(); }

        void ClearMysteries()
        {
            foreach (var mystery in mysteries.Values) mystery.Dispose();
            mysteries.Clear();
        }

        void ApplyMystery(int hive, Visual visual, Text label)
        {
            if (!model.Hives[hive].Data.isHidden) return;
            HiveMysteryVisual mystery;
            if (!mysteries.TryGetValue(hive,out mystery))
            {
                mystery = new HiveMysteryVisual(hive, visual.Renderers, label);
                mysteries.Add(hive,mystery);
            }
            int column = rules.WaitingColumnOf(hive);
            bool eligible = model.Hives[hive].Started || (column >= 0 && rules.Front(column) == hive);
            mystery.Show(visual.Root.transform,label,levelCamera,canvas,eligible && !model.Paused);
        }

        /// <summary>Advances only local reveal presentation, using the level's existing pause-aware clock.</summary>
        public void TickMysteries(float seconds)
        {
            if (!hasMysteries || !isActiveAndEnabled || model.Paused || model.Won || model.Deadlocked) return;
            bool changed = false;
            foreach (var mystery in mysteries.Values) changed |= mystery.Tick(seconds);
            if (changed) { revision=-1; Sync(); }
        }

        void RenderConnections()
        {
            foreach (var link in groupLinks.Values) link.Hide();
            if (!model.HasLinkedGroups) return;
            if (linkMaterial == null) linkMaterial = assets.RoundedChainMaterial();
            for (int id = 0; id < model.Hives.Count; id++)
            {
                var members = model.Hives[id].Group;
                if (members.Count < 2 || id != members[0]) continue;
                Visual visual;
                if (!visuals.TryGetValue(id, out visual) || !visual.Root.activeSelf) continue;
                for (int j = 1; j < members.Count; j++)
                {
                    int member = members[j];
                    Visual other;
                    if (!visuals.TryGetValue(member, out other) || !other.Root.activeSelf) continue;
                    int key = id * model.Hives.Count + member;
                    HiveChainVisual link;
                    if (!groupLinks.TryGetValue(key, out link))
                    {
                        link = new HiveChainVisual("HiveChainLink_" + id + "_" + member, transform, linkMaterial,
                            LevelOneAssets.ColorOf(model.Hives[id].Data.color), LevelOneAssets.ColorOf(model.Hives[member].Data.color));
                        groupLinks.Add(key, link);
                    }
                    var a = levelCamera.WorldToScreenPoint(visual.Root.transform.position);
                    var b = levelCamera.WorldToScreenPoint(other.Root.transform.position);
                    a.y += levelCamera.pixelHeight * .014f;
                    b.y += levelCamera.pixelHeight * .014f;
                    bool moving = false;
                    if (hoppingHives != null)
                        for (int h = 0; h < hoppingHives.Length; h++)
                            if (hoppingHives[h] == id || hoppingHives[h] == member) { moving = true; break; }
                    link.Show(levelCamera, a, b, moving ? hopElapsed : 0);
                }
            }
        }

        void LayoutSlot(int slot)
        {
            float pixels = levelCamera.pixelHeight * SlotHeight * TileRatio;
            hud.SlotTargets[slot].position = slotCenters[slot];
            hud.SlotTargets[slot].sizeDelta = Vector2.one * pixels / canvas.scaleFactor;
            var ray = levelCamera.ScreenPointToRay(slotCenters[slot]);
            float distance;
            if (!new Plane(Vector3.forward, new Vector3(0, 0, -.3f)).Raycast(ray, out distance))
                throw new InvalidOperationException("Slot tile placement plane is outside the camera view.");
            slotTiles[slot].position = ray.GetPoint(distance) + levelCamera.transform.forward * 1.5f;
            slotTiles[slot].rotation = levelCamera.transform.rotation;
            slotTiles[slot].localScale = Vector3.one * levelCamera.orthographicSize * 2 * SlotHeight * TileRatio;
        }

        Visual GetVisual(int hive)
        {
            Visual visual;
            if (visuals.TryGetValue(hive, out visual)) return visual;
            var root = assets.Spawn(LevelOneAssets.HiveKey(model.Hives[hive].Data.color), transform);
            root.name = "SmallHive_" + hive;
            root.transform.rotation = Quaternion.Euler(0, 180, 0);
            visual = new Visual { Root = root, Renderers = root.GetComponentsInChildren<Renderer>() };
            foreach (var renderer in visual.Renderers)
                if (renderer.name.StartsWith("Count_Plaque")) visual.Plaque = renderer;
            foreach (var item in root.GetComponentsInChildren<Transform>())
                if (item.name.StartsWith("SpawnPoint") || item.name.EndsWith("_Mouth")) visual.Exit = item;
                else if(item.name.EndsWith("_NumberAnchor")) visual.NumberAnchor=item;
            if (visual.Exit == null)
                throw new InvalidOperationException("The Blender hive requires a SpawnPoint marker.");
            visuals.Add(hive, visual);
            return visual;
        }

        void Place(Visual visual, RectTransform control, float fixedHeight, Vector3? center = null, float planeZ = -.3f)
        {
            var screen = RectTransformUtility.WorldToScreenPoint(null, center ?? control.position);
            var ray = levelCamera.ScreenPointToRay(screen);
            float distance;
            if (!new Plane(Vector3.forward, new Vector3(0, 0, planeZ)).Raycast(ray, out distance))
                throw new InvalidOperationException("The hive placement plane is outside the camera view.");
            visual.Root.SetActive(true);
            visual.Root.transform.position = ray.GetPoint(distance);
            visual.Root.transform.localScale = Vector3.one;
            var bounds = visual.Renderers[0].bounds;
            foreach (var renderer in visual.Renderers) bounds.Encapsulate(renderer.bounds);
            float x0 = float.MaxValue, x1 = float.MinValue, y0 = float.MaxValue, y1 = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                var corner = bounds.center + Vector3.Scale(bounds.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var point = levelCamera.WorldToViewportPoint(corner);
                x0 = Mathf.Min(x0, point.x); x1 = Mathf.Max(x1, point.x);
                y0 = Mathf.Min(y0, point.y); y1 = Mathf.Max(y1, point.y);
            }
            float width = control.rect.width * canvas.scaleFactor / levelCamera.pixelWidth * .9f;
            float targetHeight = fixedHeight > 0 ? fixedHeight : control.rect.height * canvas.scaleFactor / levelCamera.pixelHeight * .9f;
            visual.Root.transform.localScale = Vector3.one * Mathf.Min(width / (x1 - x0), targetHeight / (y1 - y0));
        }

        void AlignLabel(Text label, Visual visual)
        {
            // The original green FBX has no plaque. Its face uses the same
            // model-space anchor as the pink/yellow plaque, above the exit.
            var anchor = visual.NumberAnchor != null ? visual.NumberAnchor.position : visual.Plaque != null ? visual.Plaque.bounds.center
                : visual.Root.transform.TransformPoint(new Vector3(0, .22f, .37f));
            var point = levelCamera.WorldToScreenPoint(anchor);
            label.rectTransform.position = new Vector3(point.x, point.y, 0);
        }
    }
}
