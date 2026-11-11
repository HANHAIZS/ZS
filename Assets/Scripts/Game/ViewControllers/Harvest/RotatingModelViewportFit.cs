using UnityEngine;

namespace BeeHarvest
{
    /// <summary>Caps an enlarged model against the screen and slot tiles throughout a full yaw rotation.</summary>
    internal sealed class RotatingModelViewportFit
    {
        struct Orbit
        {
            public Vector2 Center, Radius;
            public float Padding;
        }

        readonly Transform root;
        readonly Camera camera;
        readonly LevelOneHud hud;
        readonly Canvas canvas;
        readonly Vector3 desiredScale, desiredPosition, worldCenter;
        Vector2 viewCenter;
        readonly Orbit[] orbits;
        readonly Vector4 envelope;
        float previousAspect = -1, previousOrthoSize = -1;
        int previousSlots = -1;
        Rect previousFirstSlot, previousExtraSlot;

        public RotatingModelViewportFit(Transform model, Camera view, LevelOneHud controls, Bounds bounds)
        {
            root = model;
            camera = view;
            hud = controls;
            canvas = controls.GetComponentInParent<Canvas>();
            desiredScale = model.localScale;
            desiredPosition = model.position;
            worldCenter = bounds.center;
            viewCenter = camera.WorldToViewportPoint(worldCenter);
            var filters = model.GetComponentsInChildren<MeshFilter>();
            orbits = new Orbit[filters.Length];
            var limits = new Vector4(float.PositiveInfinity, float.NegativeInfinity,
                float.PositiveInfinity, float.NegativeInfinity);
            var right = camera.transform.right;
            var up = camera.transform.up;
            for (int index = 0; index < filters.Length; index++)
            {
                var filter = filters[index];
                var mesh = filter.sharedMesh.bounds;
                var center = filter.transform.TransformPoint(mesh.center);
                var orbit = ProjectOrbit(center, right, up);
                float radius = 0;
                for (int corner = 0; corner < 8; corner++)
                {
                    var point = filter.transform.TransformPoint(mesh.center + Vector3.Scale(mesh.extents,
                        new Vector3((corner & 1) == 0 ? -1 : 1,
                            (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1)));
                    radius = Mathf.Max(radius, Vector3.Distance(center, point));
                    var vertex = ProjectOrbit(point, right, up);
                    limits.x = Mathf.Min(limits.x, vertex.Center.x - vertex.Radius.x);
                    limits.y = Mathf.Max(limits.y, vertex.Center.x + vertex.Radius.x);
                    limits.z = Mathf.Min(limits.z, vertex.Center.y - vertex.Radius.y);
                    limits.w = Mathf.Max(limits.w, vertex.Center.y + vertex.Radius.y);
                }
                // A circumscribed sphere covers the voxel at every orientation.
                orbit.Padding = radius;
                orbits[index] = orbit;
            }
            envelope = limits;
        }

        Orbit ProjectOrbit(Vector3 point, Vector3 right, Vector3 up)
        {
            var relative = point - desiredPosition;
            var vertical = desiredPosition + Vector3.up * relative.y - worldCenter;
            var cosine = new Vector3(relative.x, 0, relative.z);
            var sine = new Vector3(relative.z, 0, -relative.x);
            float xCos = Vector3.Dot(right, cosine), xSin = Vector3.Dot(right, sine);
            float yCos = Vector3.Dot(up, cosine), ySin = Vector3.Dot(up, sine);
            return new Orbit
            {
                Center = new Vector2(Vector3.Dot(right, vertical), Vector3.Dot(up, vertical)),
                Radius = new Vector2(Mathf.Sqrt(xCos * xCos + xSin * xSin), Mathf.Sqrt(yCos * yCos + ySin * ySin))
            };
        }

        Rect SlotRect(int index)
        {
            var target = hud.SlotTargets[index];
            Vector2 center = camera.ScreenToViewportPoint(RectTransformUtility.WorldToScreenPoint(null, target.position));
            float height = target.rect.height * canvas.scaleFactor / camera.pixelHeight;
            var size = new Vector2(height / camera.aspect, height);
            return new Rect(center - size * .5f, size);
        }

        public void Refresh(int slots)
        {
            var first = SlotRect(0);
            var sixth = SlotRect(5);
            if (previousSlots == slots && previousAspect == camera.aspect &&
                previousOrthoSize == camera.orthographicSize && previousFirstSlot == first && previousExtraSlot == sixth) return;
            previousFirstSlot = first;
            previousExtraSlot = sixth;
            viewCenter = camera.WorldToViewportPoint(worldCenter);
            previousSlots = slots;
            previousAspect = camera.aspect;
            previousOrthoSize = camera.orthographicSize;
            float height = 2 * camera.orthographicSize, width = height * camera.aspect;
            float bottom = 0;
            for (int slot = 0; slot < 5; slot++) bottom = Mathf.Max(bottom, SlotRect(slot).yMax + .015f);
            float factor = Mathf.Min(1, (viewCenter.x - .05f) * width / -envelope.x,
                (.95f - viewCenter.x) * width / envelope.y,
                (viewCenter.y - bottom) * height / -envelope.z,
                (.85f - viewCenter.y) * height / envelope.w);
            if(factor < 1) factor *= .9995f;
            if (slots > 5)
            {
                var extra = sixth;
                extra.xMin -= .005f; extra.xMax += .005f;
                extra.yMin -= .005f; extra.yMax += .005f;
                while (factor > .1f && IntersectsExtraSlot(extra, factor, width, height)) factor -= .005f;
            }
            factor = Mathf.Clamp(factor, .1f, 1);
            root.localScale = desiredScale * factor;
            root.position = worldCenter + (desiredPosition - worldCenter) * factor;
        }

        bool IntersectsExtraSlot(Rect slot, float factor, float width, float height)
        {
            foreach (var orbit in orbits)
            {
                var center = viewCenter + Vector2.Scale(orbit.Center, new Vector2(factor / width, factor / height));
                var radius = Vector2.Scale(orbit.Radius, new Vector2(factor / width, factor / height));
                var padding = new Vector2(orbit.Padding * factor / width, orbit.Padding * factor / height);
                float left = slot.xMin - padding.x - center.x, right = slot.xMax + padding.x - center.x;
                float bottom = slot.yMin - padding.y - center.y, top = slot.yMax + padding.y - center.y;
                if (radius.x < .000001f)
                {
                    if (left <= 0 && right >= 0 && bottom <= radius.y && top >= -radius.y) return true;
                    continue;
                }
                if (radius.y < .000001f)
                {
                    if (bottom <= 0 && top >= 0 && left <= radius.x && right >= -radius.x) return true;
                    continue;
                }
                left /= radius.x; right /= radius.x; bottom /= radius.y; top /= radius.y;
                float nearX = Mathf.Clamp(0, left, right), nearY = Mathf.Clamp(0, bottom, top);
                float farX = Mathf.Max(Mathf.Abs(left), Mathf.Abs(right));
                float farY = Mathf.Max(Mathf.Abs(bottom), Mathf.Abs(top));
                // With the scene's unrolled camera, each voxel center traces an axis-aligned ellipse.
                if (nearX * nearX + nearY * nearY <= 1 && farX * farX + farY * farY >= 1) return true;
            }
            return false;
        }
    }
}
