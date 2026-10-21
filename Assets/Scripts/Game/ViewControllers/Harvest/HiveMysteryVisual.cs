using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest
{
    /// <summary>Generation-local concealment and reveal presentation; never changes a hive's identity.</summary>
    public sealed class HiveMysteryVisual : IDisposable
    {
        public const float RevealSeconds = .3f;
        static readonly Color Neutral = new Color(.48f, .50f, .56f);
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int LegacyColor = Shader.PropertyToID("_Color");
        sealed class TintSlot
        {
            public Renderer Renderer;
            public int Index;
            public Color Color;
            public bool HasBase, HasLegacy;
            public readonly MaterialPropertyBlock Original = new MaterialPropertyBlock();
            public readonly MaterialPropertyBlock Animated = new MaterialPropertyBlock();
        }
        readonly List<TintSlot> slots = new List<TintSlot>();
        readonly Text question;
        readonly Renderer[] renderers;
        Text quotaLabel;
        float quotaAlpha;
        bool revealed, restored;
        float elapsed, lastTint = -1;

        public HiveMysteryVisual(int id, Renderer[] renderers, Text label)
        {
            this.renderers=renderers;quotaLabel=label;quotaAlpha=label.canvasRenderer.GetAlpha();
            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials;
                for (int index = 0; index < materials.Length; index++)
                {
                    var material = materials[index];
                    if (material == null || (!material.name.StartsWith("HivePalette", StringComparison.Ordinal)
                        && material.name.IndexOf("_Tint_", StringComparison.Ordinal) < 0)) continue;
                    var slot = new TintSlot { Renderer = renderer, Index = index,
                        HasBase = material.HasProperty(BaseColor), HasLegacy = material.HasProperty(LegacyColor) };
                    if (!slot.HasBase && !slot.HasLegacy) continue;
                    slot.Color = material.GetColor(slot.HasBase ? BaseColor : LegacyColor);
                    renderer.GetPropertyBlock(slot.Original, index);
                    renderer.GetPropertyBlock(slot.Animated, index);
                    slots.Add(slot);
                }
            }
            var root = new GameObject("MysteryHive_" + id, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            root.transform.SetParent(label.transform, false);
            question = root.GetComponent<Text>();
            question.text = "?";
            question.raycastTarget = false;
            question.alignment = TextAnchor.MiddleCenter;
            question.horizontalOverflow = HorizontalWrapMode.Overflow;
            question.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = root.GetComponent<Outline>();
            outline.effectColor = new Color(.10f, .12f, .18f, .95f);
            outline.effectDistance = new Vector2(2, -2);
            ApplyTint(0);
        }

        public void Hide()
        {
            if(question!=null)question.gameObject.SetActive(false);
            if(quotaLabel!=null)quotaLabel.canvasRenderer.SetAlpha(quotaAlpha);
            quotaLabel=null;
        }
        public bool Tick(float seconds)
        {
            if (!revealed || elapsed >= RevealSeconds || seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return false;
            elapsed = Mathf.Min(RevealSeconds, elapsed + seconds);
            return true;
        }

        public void Show(Transform hive, Text label, Camera camera, Canvas canvas, bool eligible)
        {
            if (eligible) revealed = true;
            float t = revealed ? Mathf.Clamp01(elapsed / RevealSeconds) : 0;
            ApplyTint(t * t * (3 - 2 * t));
            if (revealed && t < 1)
            {
                hive.localScale *= 1 + .08f * Mathf.Sin(t * Mathf.PI);
                float wiggle = Mathf.Sin(t * Mathf.PI * 6) * (1 - t) * camera.orthographicSize * .0025f;
                hive.position += camera.transform.right * wiggle;
            }
            if(quotaLabel!=label){
                if(quotaLabel!=null)quotaLabel.canvasRenderer.SetAlpha(quotaAlpha);
                quotaLabel=label;quotaAlpha=label.canvasRenderer.GetAlpha();
            }
            // Renderer alpha keeps the existing Text raycast surface and its child question alive.
            label.canvasRenderer.SetAlpha(quotaAlpha*t);
            if (question.transform.parent != label.transform) question.transform.SetParent(label.transform, false);
            question.font = label.font;
            question.fontStyle = FontStyle.Bold;
            question.fontSize = Mathf.Max(18, Mathf.RoundToInt(label.fontSize * .85f));
            question.color = new Color(1, 1, 1, 1 - t);
            var rect = question.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = Vector2.one * question.fontSize * 2;
            question.gameObject.SetActive(t < 1);
            AlignQuestion(camera);
        }

        public void AlignQuestion(Camera camera)
        {
            if(question==null||!question.gameObject.activeSelf)return;
            var bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
            var center=camera.WorldToScreenPoint(bounds.center);
            question.rectTransform.position=new Vector3(center.x,center.y,0);
        }

        void ApplyTint(float t)
        {
            if (t == lastTint) return;
            lastTint = t;
            if (t >= 1) { Restore(); return; }
            restored = false;
            foreach (var slot in slots)
            {
                var color = Color.Lerp(Neutral, slot.Color, t);
                if (slot.HasBase) slot.Animated.SetColor(BaseColor, color);
                if (slot.HasLegacy) slot.Animated.SetColor(LegacyColor, color);
                slot.Renderer.SetPropertyBlock(slot.Animated, slot.Index);
            }
        }

        void Restore()
        {
            if (restored) return;
            foreach (var slot in slots)
                if (slot.Renderer != null) slot.Renderer.SetPropertyBlock(slot.Original, slot.Index);
            restored = true;
        }

        public void Dispose()
        {
            Restore();
            Hide();
            if (question != null) UnityEngine.Object.Destroy(question.gameObject);
        }
    }
}
