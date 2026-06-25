using UnityEngine;

public class PlayerTrackingProvider : MonoBehaviour
{
    [Header("Optional")]
    [Tooltip("Hier die Kamera aus dem MRTK XR Rig reinziehen. Wenn leer, wird automatisch Camera.main gesucht.")]
    public Transform playerHead;

    private Transform cachedHead;

    public Vector3 Position
    {
        get
        {
            return GetHeadTransform().position;
        }
    }

    public Vector3 Forward
    {
        get
        {
            return GetHeadTransform().forward;
        }
    }

    private Transform GetHeadTransform()
    {
        if (playerHead != null)
        {
            return playerHead;
        }

        if (cachedHead != null)
        {
            return cachedHead;
        }

        if (Camera.main != null)
        {
            cachedHead = Camera.main.transform;
            return cachedHead;
        }

        Camera anyCamera = FindObjectOfType<Camera>();

        if (anyCamera != null)
        {
            cachedHead = anyCamera.transform;
            return cachedHead;
        }

        Debug.LogWarning("PlayerTrackingProvider: Keine Kamera gefunden. Fallback auf GameManager-Transform.");
        return transform;
    }
}