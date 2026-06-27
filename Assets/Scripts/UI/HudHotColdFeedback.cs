using UnityEngine;
using UnityEngine.UI;

public class HudHotColdFeedback : MonoBehaviour
{
    [Header("Overlay References")]
    public Image vignetteImage;
    public CanvasGroup vignetteCanvasGroup;

    [Header("Colors")]
    public Color coldColor = new Color(0.15f, 0.45f, 1.0f, 1f);
    public Color hotColor = new Color(1.0f, 0.22f, 0.05f, 1f);

    [Header("Distance Mapping")]
    public float minDistance = 0.5f;
    public float maxDistance = 6.0f;

    [Header("Visual Strength")]
    [Range(0f, 1f)]
    public float minOverlayAlpha = 0.08f;

    [Range(0f, 1f)]
    public float maxOverlayAlpha = 0.45f;

    [Header("Pulse Near Target")]
    public bool pulseWhenVeryClose = true;
    public float veryCloseDistance = 1.2f;
    public float pulseSpeed = 4.0f;
    public float pulseAmount = 0.12f;

    [Header("Smoothing")]
    public float colorLerpSpeed = 6.0f;
    public float alphaLerpSpeed = 6.0f;

    public float HotIntensity { get; private set; }
    public float ColdIntensity { get; private set; }

    private void Awake()
    {
        if (vignetteCanvasGroup != null)
        {
            vignetteCanvasGroup.alpha = 0f;
            vignetteCanvasGroup.interactable = false;
            vignetteCanvasGroup.blocksRaycasts = false;
        }

        if (vignetteImage != null)
        {
            Color c = coldColor;
            c.a = 1f;
            vignetteImage.color = c;
            vignetteImage.raycastTarget = false;
        }
    }

    public void UpdateFeedback(SearchContext context)
    {
        if (vignetteImage == null || vignetteCanvasGroup == null)
        {
            return;
        }

        float closeness = Mathf.InverseLerp(
            maxDistance,
            minDistance,
            context.distanceToTarget
        );

        closeness = Mathf.Clamp01(closeness);

        HotIntensity = closeness;
        ColdIntensity = 1.0f - closeness;

        Color targetColor = Color.Lerp(coldColor, hotColor, closeness);
        targetColor.a = 1f;

        vignetteImage.color = Color.Lerp(
            vignetteImage.color,
            targetColor,
            Time.deltaTime * colorLerpSpeed
        );

        float targetAlpha = Mathf.Lerp(
            minOverlayAlpha,
            maxOverlayAlpha,
            closeness
        );

        if (pulseWhenVeryClose && context.distanceToTarget <= veryCloseDistance)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            targetAlpha += pulse * pulseAmount;
        }

        targetAlpha = Mathf.Clamp01(targetAlpha);

        vignetteCanvasGroup.alpha = Mathf.Lerp(
            vignetteCanvasGroup.alpha,
            targetAlpha,
            Time.deltaTime * alphaLerpSpeed
        );

        vignetteCanvasGroup.interactable = false;
        vignetteCanvasGroup.blocksRaycasts = false;
    }

    public void HideFeedback()
    {
        if (vignetteCanvasGroup == null)
        {
            return;
        }

        vignetteCanvasGroup.alpha = Mathf.Lerp(
            vignetteCanvasGroup.alpha,
            0f,
            Time.deltaTime * alphaLerpSpeed
        );
    }
}