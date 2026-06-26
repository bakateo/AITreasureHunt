using UnityEngine;

public class LookAroundTargetVisual : MonoBehaviour
{
    [Header("Direction")]
    public LookAroundDirection direction;

    [Header("Visual References")]
    public Renderer targetRenderer;
    public AudioSource audioSource;

    [Header("Materials")]
    public Material inactiveMaterial;
    public Material activeMaterial;
    public Material completedMaterial;

    [Header("Scale Animation")]
    public float inactiveScale = 0.16f;
    public float activeScale = 0.23f;
    public float completedScale = 0.20f;
    public float hiddenScale = 0.01f;
    public float pulseSpeed = 3.0f;
    public float pulseAmount = 0.025f;
    public float scaleLerpSpeed = 8.0f;

    [Header("Completion Animation")]
    public float completedVisibleTime = 0.45f;
    public float fadeOutDuration = 0.45f;

    [Header("Sound")]
    public bool enableSound = true;
    public float activeVolume = 0.12f;
    public float fadeSpeed = 3.0f;

    private bool isActiveTarget;
    private bool isCompleted;
    private bool wasCompletedLastFrame;

    private float completedTime = -1f;
    private bool readyToHide = false;

    private Material runtimeMaterial;
    private Color baseColor;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            runtimeMaterial = targetRenderer.material;
            baseColor = runtimeMaterial.color;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 1.0f;
            audioSource.volume = 0f;
        }
    }

    private void OnEnable()
    {
        ResetVisual();
    }

    private void Update()
    {
        UpdateScaleAnimation();
        UpdateFadeOut();
        UpdateAudio();
    }

    public void SetState(bool activeTarget, bool completed)
    {
        isActiveTarget = activeTarget;
        isCompleted = completed;

        if (completed && !wasCompletedLastFrame)
        {
            completedTime = Time.time;
            readyToHide = false;
            ApplyMaterial(completedMaterial);
        }
        else if (!completed)
        {
            if (activeTarget)
            {
                ApplyMaterial(activeMaterial);
            }
            else
            {
                ApplyMaterial(inactiveMaterial);
            }
        }

        wasCompletedLastFrame = completed;
    }

    public bool IsReadyToHide()
    {
        return readyToHide;
    }

    public void ResetVisual()
    {
        isActiveTarget = false;
        isCompleted = false;
        wasCompletedLastFrame = false;
        completedTime = -1f;
        readyToHide = false;

        transform.localScale = Vector3.one * inactiveScale;

        if (targetRenderer != null)
        {
            ApplyMaterial(inactiveMaterial);
            SetAlpha(1f);
        }
    }

    private void ApplyMaterial(Material material)
    {
        if (targetRenderer == null || material == null)
        {
            return;
        }

        targetRenderer.material = material;
        runtimeMaterial = targetRenderer.material;
        baseColor = runtimeMaterial.color;
    }

    private void UpdateScaleAnimation()
    {
        float targetScale = inactiveScale;

        if (isCompleted)
        {
            targetScale = completedScale;
        }
        else if (isActiveTarget)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            targetScale = activeScale + pulse;
        }

        if (readyToHide)
        {
            targetScale = hiddenScale;
        }

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * scaleLerpSpeed
        );
    }

    private void UpdateFadeOut()
    {
        if (!isCompleted || completedTime < 0f || runtimeMaterial == null)
        {
            return;
        }

        float fadeStartTime = completedTime + completedVisibleTime;
        float fadeEndTime = fadeStartTime + fadeOutDuration;

        if (Time.time < fadeStartTime)
        {
            SetAlpha(1f);
            return;
        }

        float t = Mathf.InverseLerp(fadeStartTime, fadeEndTime, Time.time);
        float alpha = Mathf.Lerp(1f, 0f, t);

        SetAlpha(alpha);

        if (Time.time >= fadeEndTime)
        {
            readyToHide = true;
        }
    }

    private void SetAlpha(float alpha)
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        Color c = runtimeMaterial.color;
        c.a = alpha;
        runtimeMaterial.color = c;
    }

    private void UpdateAudio()
    {
        if (!enableSound || audioSource == null)
        {
            return;
        }

        float targetVolume = isActiveTarget && !isCompleted ? activeVolume : 0f;

        audioSource.volume = Mathf.Lerp(
            audioSource.volume,
            targetVolume,
            Time.deltaTime * fadeSpeed
        );

        if (audioSource.volume > 0.01f && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (audioSource.volume <= 0.01f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}