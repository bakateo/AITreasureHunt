using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LookAroundHudGuide : MonoBehaviour
{
    [Header("References")]
    public PlayerTrackingProvider playerTracking;
    public LookAroundManager lookAroundManager;

    [Header("Look Targets")]
    public List<LookAroundTargetVisual> lookTargets = new List<LookAroundTargetVisual>();

    [Header("Edge Glow References")]
    public CanvasGroup leftGlow;
    public CanvasGroup rightGlow;
    public Image leftGlowImage;
    public Image rightGlowImage;

    [Header("Colors")]
    public Color coldColor = new Color(0.2f, 0.55f, 1.0f, 1f);
    public Color warmColor = new Color(1.0f, 0.28f, 0.08f, 1f);

    [Header("Glow Strength")]
    public float coldMaxAlpha = 0.35f;
    public float warmMaxAlpha = 0.55f;
    public float glowLerpSpeed = 8f;

    [Header("Angle Behavior")]
    [Tooltip("Ab diesem Winkel beginnt der warme rote Bereich.")]
    public float warmAngle = 28f;

    [Tooltip("Wenn der Nutzer fast exakt auf das Target schaut, wird das HUD ausgeblendet.")]
    public float alignedFadeAngle = 10f;

    [Tooltip("Wenn das Target fast genau hinter dem Nutzer liegt, leuchten beide Seiten blau.")]
    public float behindAngle = 150f;

    private void Awake()
    {
        if (playerTracking == null)
        {
            playerTracking = FindObjectOfType<PlayerTrackingProvider>();
        }

        if (lookAroundManager == null)
        {
            lookAroundManager = FindObjectOfType<LookAroundManager>();
        }

        if (lookTargets == null || lookTargets.Count == 0)
        {
            LookAroundTargetVisual[] foundTargets = FindObjectsOfType<LookAroundTargetVisual>();
            lookTargets = new List<LookAroundTargetVisual>(foundTargets);
        }

        ForceHideAll();
    }

    private void OnEnable()
    {
        ForceHideAll();
    }

    private void Update()
    {
        if (playerTracking == null || lookAroundManager == null)
        {
            ForceHideAll();
            return;
        }

        if (!lookAroundManager.IsActive)
        {
            FadeOutAll();
            return;
        }

        LookAroundTargetVisual currentTarget;

        if (!TryGetBestOpenTarget(out currentTarget))
        {
            FadeOutAll();
            return;
        }

        Vector3 playerPosition = playerTracking.Position;

        Vector3 playerForward = Vector3.ProjectOnPlane(
            playerTracking.Forward,
            Vector3.up
        ).normalized;

        if (playerForward.sqrMagnitude < 0.001f)
        {
            playerForward = Vector3.forward;
        }

        Vector3 directionToTarget = Vector3.ProjectOnPlane(
            currentTarget.transform.position - playerPosition,
            Vector3.up
        ).normalized;

        if (directionToTarget.sqrMagnitude < 0.001f)
        {
            FadeOutAll();
            return;
        }

        float signedAngle = Vector3.SignedAngle(
            playerForward,
            directionToTarget,
            Vector3.up
        );

        float absAngle = Mathf.Abs(signedAngle);

        UpdateColdWarmGlow(signedAngle, absAngle);
    }

    private bool TryGetBestOpenTarget(out LookAroundTargetVisual bestTarget)
    {
        bestTarget = null;

        Vector3 playerPosition = playerTracking.Position;

        Vector3 playerForward = Vector3.ProjectOnPlane(
            playerTracking.Forward,
            Vector3.up
        ).normalized;

        if (playerForward.sqrMagnitude < 0.001f)
        {
            playerForward = Vector3.forward;
        }

        float bestAngle = float.MaxValue;

        foreach (LookAroundTargetVisual target in lookTargets)
        {
            if (target == null)
            {
                continue;
            }

            if (lookAroundManager.IsDirectionCompleted(target.direction))
            {
                continue;
            }

            Vector3 directionToTarget = Vector3.ProjectOnPlane(
                target.transform.position - playerPosition,
                Vector3.up
            ).normalized;

            if (directionToTarget.sqrMagnitude < 0.001f)
            {
                continue;
            }

            float angle = Vector3.Angle(playerForward, directionToTarget);

            if (angle < bestAngle)
            {
                bestAngle = angle;
                bestTarget = target;
            }
        }

        return bestTarget != null;
    }

    private void UpdateColdWarmGlow(float signedAngle, float absAngle)
    {
        if (absAngle <= alignedFadeAngle)
        {
            SetGlow(leftGlow, leftGlowImage, warmColor, 0f);
            SetGlow(rightGlow, rightGlowImage, warmColor, 0f);
            return;
        }

        bool targetIsBehind = absAngle >= behindAngle;
        bool isWarm = absAngle <= warmAngle;

        if (isWarm)
        {
            float warmStrength = Mathf.InverseLerp(
                warmAngle,
                alignedFadeAngle,
                absAngle
            );

            warmStrength = 1f - Mathf.Clamp01(warmStrength);

            float alpha = Mathf.Lerp(
                warmMaxAlpha * 0.4f,
                warmMaxAlpha,
                warmStrength
            );

            SetGlow(leftGlow, leftGlowImage, warmColor, alpha);
            SetGlow(rightGlow, rightGlowImage, warmColor, alpha);
            return;
        }

        float coldStrength = Mathf.InverseLerp(
            warmAngle,
            180f,
            absAngle
        );

        coldStrength = Mathf.Clamp01(coldStrength);

        float coldAlpha = Mathf.Lerp(
            coldMaxAlpha * 0.45f,
            coldMaxAlpha,
            coldStrength
        );

        if (targetIsBehind)
        {
            // Ziel liegt hinter dem Nutzer:
            // Beide Seiten blau = "Dreh dich um", aber nicht warm/gefunden.
            SetGlow(leftGlow, leftGlowImage, coldColor, coldAlpha);
            SetGlow(rightGlow, rightGlowImage, coldColor, coldAlpha);
            return;
        }

        if (signedAngle < 0f)
        {
            SetGlow(leftGlow, leftGlowImage, coldColor, coldAlpha);
            SetGlow(rightGlow, rightGlowImage, coldColor, 0f);
        }
        else
        {
            SetGlow(leftGlow, leftGlowImage, coldColor, 0f);
            SetGlow(rightGlow, rightGlowImage, coldColor, coldAlpha);
        }
    }

    private void SetGlow(CanvasGroup group, Image image, Color color, float targetAlpha)
    {
        if (image != null)
        {
            Color targetColor = color;
            targetColor.a = 1f;

            image.color = Color.Lerp(
                image.color,
                targetColor,
                Time.deltaTime * glowLerpSpeed
            );
        }

        if (group != null)
        {
            group.alpha = Mathf.Lerp(
                group.alpha,
                targetAlpha,
                Time.deltaTime * glowLerpSpeed
            );

            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    private void FadeOutAll()
    {
        SetGlow(leftGlow, leftGlowImage, coldColor, 0f);
        SetGlow(rightGlow, rightGlowImage, coldColor, 0f);
    }

    private void ForceHideAll()
    {
        if (leftGlow != null)
        {
            leftGlow.alpha = 0f;
            leftGlow.blocksRaycasts = false;
            leftGlow.interactable = false;
        }

        if (rightGlow != null)
        {
            rightGlow.alpha = 0f;
            rightGlow.blocksRaycasts = false;
            rightGlow.interactable = false;
        }

        if (leftGlowImage != null)
        {
            Color c = coldColor;
            c.a = 1f;
            leftGlowImage.color = c;
        }

        if (rightGlowImage != null)
        {
            Color c = coldColor;
            c.a = 1f;
            rightGlowImage.color = c;
        }
    }
}