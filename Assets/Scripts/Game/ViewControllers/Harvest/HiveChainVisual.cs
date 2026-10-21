using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace BeeHarvest
{
    /// <summary>Owns one reusable screen-facing, shaded connection and its temporary mesh.</summary>
    public sealed class HiveChainVisual : IDisposable
    {
        const int Segments = 16;
        static readonly int ColorA = Shader.PropertyToID("_ColorA");
        static readonly int ColorB = Shader.PropertyToID("_ColorB");
        static readonly int BodyAspect = Shader.PropertyToID("_BodyAspect");
        readonly GameObject root;
        readonly Mesh mesh;
        readonly MeshRenderer renderer;
        readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
        readonly Vector3[] vertices = new Vector3[(Segments + 1) * 2];

        public HiveChainVisual(string name, Transform parent, Material material, Color a, Color b)
        {
            properties.SetColor(ColorA, a);
            properties.SetColor(ColorB, b);
            root = new GameObject(name);
            root.transform.SetParent(parent, false);
            mesh = new Mesh { name = name + "_Mesh" };
            mesh.MarkDynamic();
            var uv = new Vector2[vertices.Length];
            var triangles = new int[Segments * 6];
            for (int i = 0; i <= Segments; i++)
            {
                uv[i * 2] = new Vector2((float)i / Segments, 0);
                uv[i * 2 + 1] = new Vector2((float)i / Segments, 1);
                if (i == Segments) continue;
                int v = i * 2, t = i * 6;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v + 2; triangles[t + 4] = v + 1; triangles[t + 5] = v + 3;
            }
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            root.AddComponent<MeshFilter>().sharedMesh = mesh;
            renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        public void Hide() { root.SetActive(false); }

        /// <summary>Uses current endpoints and the existing hop clock; never advances game state.</summary>
        public void Show(Camera camera, Vector3 start, Vector3 end, float hopSeconds)
        {
            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length < .01f) { Hide(); return; }
            Vector2 direction = delta / length;
            Vector2 normal = new Vector2(-direction.y, direction.x);
            // Keep the highlight on the upper side even when a group points left.
            if (normal.y < 0) normal = -normal;
            float diameter = camera.pixelHeight * .009f;
            float travel = Mathf.Clamp01(hopSeconds / HiveHopController.TravelSeconds);
            float landing = Mathf.Clamp01((hopSeconds - HiveHopController.TravelSeconds) / HiveHopController.LandingSeconds);
            float bend = diameter * (.38f * Mathf.Sin(travel * Mathf.PI) - .24f * Mathf.Sin(landing * Mathf.PI));
            var plane = new Plane(Vector3.forward, new Vector3(0, 0, -2.4f));
            for (int i = 0; i <= Segments; i++)
            {
                float along = Mathf.Lerp(-diameter * .6f, length + diameter * .6f, (float)i / Segments);
                float t = Mathf.Clamp01(along / length);
                var center = (Vector2)start + direction * along + normal * (Mathf.Sin(t * Mathf.PI) * bend);
                // The shader rounds the end caps; extra padding contains its soft lower shadow.
                for (int side = 0; side < 2; side++)
                {
                    var point = center + normal * ((side == 0 ? -1 : 1) * diameter * .6f);
                    var ray = camera.ScreenPointToRay(point);
                    float distance;
                    plane.Raycast(ray, out distance);
                    vertices[i * 2 + side] = root.transform.InverseTransformPoint(ray.GetPoint(distance));
                }
            }
            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            properties.SetFloat(BodyAspect, length / diameter);
            renderer.SetPropertyBlock(properties);
            root.SetActive(true);
        }

        public void Dispose()
        {
            root.SetActive(false);
            UnityEngine.Object.Destroy(mesh);
            UnityEngine.Object.Destroy(root);
        }
    }
}
