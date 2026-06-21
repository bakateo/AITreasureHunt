using UnityEngine;

public class EditorPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.0f;
    public float verticalMoveSpeed = 1.5f;

    [Header("Look")]
    public float lookSpeed = 3.0f;
    public bool rotateOnlyWithRightMouse = true;

    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        if (pitch > 180f)
        {
            pitch -= 360f;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        float horizontal = GetHorizontalInput();
        float vertical = GetVerticalInput();

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 movement = (right * horizontal + forward * vertical) * moveSpeed;

        if (IsKeyPressed(KeyCode.E))
        {
            movement += Vector3.up * verticalMoveSpeed;
        }

        if (IsKeyPressed(KeyCode.Q))
        {
            movement += Vector3.down * verticalMoveSpeed;
        }

        transform.position += movement * Time.deltaTime;
    }

    private void HandleLook()
    {
        if (rotateOnlyWithRightMouse && !Input.GetMouseButton(1))
        {
            return;
        }

        yaw += Input.GetAxis("Mouse X") * lookSpeed;
        pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private float GetHorizontalInput()
    {
        float value = 0f;

        if (IsKeyPressed(KeyCode.A))
        {
            value -= 1f;
        }

        if (IsKeyPressed(KeyCode.D))
        {
            value += 1f;
        }

        return value;
    }

    private float GetVerticalInput()
    {
        float value = 0f;

        if (IsKeyPressed(KeyCode.W))
        {
            value += 1f;
        }

        if (IsKeyPressed(KeyCode.S))
        {
            value -= 1f;
        }

        return value;
    }

    private bool IsKeyPressed(KeyCode key)
    {
        return Input.GetKey(key);
    }
}