using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DebugKeyboardInputProvider : MonoBehaviour
{
    public bool WasFoundPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F);
#endif
    }

    public bool WasResetPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }
}