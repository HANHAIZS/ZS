using UnityEngine;

namespace BeeHarvest
{
    /// <summary>Fits the supplied artwork behind the orthographic gameplay camera.</summary>
    [ExecuteAlways]
    public sealed class LevelBackgroundView : BeeHarvestController
    {
        public Camera ViewCamera;
        public Texture2D BackgroundTexture;
        float previousAspect = -1f;
        float previousHeight = -1f;
        float previousImageAspect = -1f;

        void OnEnable() { Fit(); }
        void LateUpdate() { Fit(); }

        /// <summary>Covers the viewport without stretching, updating only when dimensions change.</summary>
        public void Fit()
        {
            if (ViewCamera == null || BackgroundTexture == null || !ViewCamera.orthographic) return;
            float aspect = ViewCamera.aspect;
            float height = ViewCamera.orthographicSize * 2f;
            float imageAspect = (float)BackgroundTexture.width / BackgroundTexture.height;
            if (aspect <= 0f || height <= 0f || imageAspect <= 0f) return;
            if (previousAspect == aspect && previousHeight == height && previousImageAspect == imageAspect) return;
            float displayedHeight = Mathf.Max(height, height * aspect / imageAspect);
            transform.localScale = new Vector3(displayedHeight * imageAspect, displayedHeight, 1f);
            previousAspect = aspect;
            previousHeight = height;
            previousImageAspect = imageAspect;
        }
    }
}
