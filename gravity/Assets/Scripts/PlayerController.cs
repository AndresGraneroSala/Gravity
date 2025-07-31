using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Lectura directa de inputs que usará PlayerMove

    public bool IsAiming()
    {
        Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 gamepadInput = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        
        return keyboardInput.sqrMagnitude > 0.1f || gamepadInput.sqrMagnitude > 0.01f || mousePressed;
    }

    public Vector2 GetAimDirection(Camera cam, Vector3 playerPos)
    {
        Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 gamepadInput = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;
        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current?.position.ReadValue() ?? Vector2.zero);
        Vector2 dirMouse = (mousePos - (Vector2)playerPos).normalized;

        if (gamepadInput.sqrMagnitude > 0.1f) return gamepadInput.normalized;
        if (keyboardInput.sqrMagnitude > 0.1f) return keyboardInput.normalized;
        if (mousePos != Vector2.zero && mousePos != (Vector2)playerPos) return dirMouse;

        return Vector2.zero;
    }

    public bool IsCountering()
    {
        bool shift = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool lb = Gamepad.current != null && Gamepad.current.leftShoulder.isPressed;
        bool l3 = Gamepad.current != null && Gamepad.current.leftStickButton.isPressed;

        return shift || lb || l3;
    }

    public bool IsLaunchPressed()
    {
        bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool aButton = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        return space || aButton;
    }

    public bool IsPausePressed()
    {
        bool scape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool startButton = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;

        return scape || startButton;
    }
}