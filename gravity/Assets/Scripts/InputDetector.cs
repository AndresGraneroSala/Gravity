using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InputDetector : MonoBehaviour
{
    public enum InputState
    {
        KeyboardMouse,
        Gamepad,
        Mobile
    }

    [SerializeField] private InputState currentState = InputState.KeyboardMouse;

    [Header("UI Indicators")]
    [SerializeField] private GameObject mobileUI;
    [SerializeField] private GameObject pcUI;
    [SerializeField] private GameObject gamepadUI;

    [Header("UI Raycaster")]
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private UnityEngine.UI.GraphicRaycaster raycaster;

    private void Awake()
    {
        // Suscribirse a eventos de conexión/desconexión
        InputSystem.onDeviceChange += OnDeviceChange;

        // Inicializar estado según dispositivo
        if (GameManager.IsMobileDevice())
        {
            SetState(InputState.Mobile);
        }
        else if (Gamepad.current != null)
        {
            SetState(InputState.Gamepad);
        }
        else
        {
            SetState(InputState.KeyboardMouse);
        }
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Update()
    {
        // --- MÓVIL ---
        if (Input.touchCount > 0 && !IsTouchOverUI(Input.GetTouch(0)))
        {
            SetState(InputState.Mobile);
            return;
        }

        // --- TECLADO / RATÓN ---
        if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame))
        {
            SetState(InputState.KeyboardMouse);
            return;
        }

        // --- GAMEPAD ---
        if (Gamepad.current != null && DetectGamepadInput())
        {
            SetState(InputState.Gamepad);
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Gamepad)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    // Nuevo gamepad conectado
                    SetState(InputState.Gamepad);
                    break;
                case InputDeviceChange.Removed:
                    // Gamepad desconectado
                    if (GameManager.IsMobileDevice())
                        SetState(InputState.Mobile);
                    else
                        SetState(InputState.KeyboardMouse);
                    break;
            }
        }
    }

    private void SetState(InputState state)
    {
        if (currentState == state) return;

        currentState = state;

        mobileUI?.SetActive(state == InputState.Mobile);
        pcUI?.SetActive(state == InputState.KeyboardMouse);
        gamepadUI?.SetActive(state == InputState.Gamepad);

        
    }

    private bool DetectGamepadInput()
    {
        if (Gamepad.current == null) return false;

        // Detectar sticks
        if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f ||
            Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f)
            return true;

        // Detectar botones
        foreach (var control in Gamepad.current.allControls)
        {
            if (control is ButtonControl button && button.wasPressedThisFrame)
                return true;
        }

        return false;
    }

    private bool IsTouchOverUI(Touch touch)
    {
        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = touch.position
        };

        List<UnityEngine.EventSystems.RaycastResult> results = new List<UnityEngine.EventSystems.RaycastResult>();
        raycaster.Raycast(pointerData, results);

        return results.Count > 0;
    }
}
