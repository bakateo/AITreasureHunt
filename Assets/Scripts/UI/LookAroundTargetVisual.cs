using UnityEngine;

public class LookAroundTargetVisual : MonoBehaviour
{
    [Header("Direction")]
    public LookAroundDirection direction;

    [Header("Visual References")]
    public Renderer targetRenderer;

    [Header("Materials")]
    public Material inactiveMaterial;
    public Material activeMaterial;
    public Material completedMaterial;

    [Header("Animation")]
    public float inactiveScale = 0.15f;
    public float activeScale = 0.22f;
    public float completedScale = 0.12f;
    public float pulseSpeed = 3.0f;
    public float pulseAmount = 0.04f;

    private bool isActiveTarget;
    private bool isCompleted;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }
    }

    private void Update()
    {
        UpdateScaleAnimation();
    }

    public void SetState(bool activeTarget, bool completed)
    {
        isActiveTarget = activeTarget;
        isCompleted = completed;

        if (targetRenderer == null)
        {
            return;
        }

        if (isCompleted && completedMaterial != null)
        {
            targetRenderer.material = completedMaterial;
        }
        else if (isActiveTarget && activeMaterial != null)
        {
            targetRenderer.material = activeMaterial;
        }
        else if (inactiveMaterial != null)
        {
            targetRenderer.material = inactiveMaterial;
        }
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

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * 8f
        );
    }
}