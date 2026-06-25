using UnityEngine;

public class HudHotColdFeedback : MonoBehaviour
{
    [Header("Optional UI References")]
    public CanvasGroup hotOverlay;
    public CanvasGroup coldOverlay;

    [Header("Distance Mapping")]
    public float minDistance = 0.5f;
    public float maxDistance = 6.0f;

    [Header("Visual Settings")]
    [Range(0f, 1f)]
    public float maxOverlayAlpha = 0.35f;

    public float HotIntensity { get; private set; }
    public float ColdIntensity { get; private set; }

    public void UpdateFeedback(SearchContext context)
    {
        float closeness = Mathf.InverseLerp(
            maxDistance,
            minDistance,
            context.distanceToTarget
        );

        HotIntensity = Mathf.Clamp01(closeness);
        ColdIntensity = 1.0f - HotIntensity;

        if (hotOverlay != null)
        {
            hotOverlay.alpha = HotIntensity * maxOverlayAlpha;
            hotOverlay.blocksRaycasts = false;
            hotOverlay.interactable = false;
        }

        if (coldOverlay != null)
        {
            coldOverlay.alpha = ColdIntensity * maxOverlayAlpha;
            coldOverlay.blocksRaycasts = false;
            coldOverlay.interactable = false;
        }
    }
}