using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TouchPhase = UnityEngine.TouchPhase;

public class PlayerController : MonoBehaviour
{
    // Lectura directa de inputs que usará PlayerMove

    [SerializeField] private GameObject mobileInputs;
    
    private Camera _cam;
    private bool _uiLaunchRequested;
    private bool _uiCounteringRequested;


    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;
    
    private void Awake()
    {
        mobileInputs.SetActive(false);
    }

    private void Start()
    {
        _cam = Camera.main;

        if (GameManager.IsMobileDevice())
        {
            mobileInputs.SetActive(true);
        }
    }

    
    bool IsTouchOverUI(Touch touch)
    {
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = touch.position;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        return results.Count > 0;
    }

    private bool _isAimingBefore = false;
    public bool IsAiming()
    {
        if (IsCountering())
        {
            return false;
        }

        Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 gamepadInput = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;

        bool touchPressed = Input.touchCount > 0;
        if (touchPressed)
        {
            Touch touch = Input.GetTouch(0);

            if (!_isAimingBefore)
            {
                touchPressed = !IsTouchOverUI(touch);
                _isAimingBefore = !IsTouchOverUI(touch);
            }
            
        }
        else
        {
            _isAimingBefore = false;
        }


        return keyboardInput.sqrMagnitude > 0.1f || gamepadInput.sqrMagnitude > 0.01f || mousePressed || touchPressed;
    }

    public Vector2 GetAimDirection()
    {
        Vector2 playerPos = (Vector2)transform.position;

        Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 gamepadInput = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;
        Vector2 mousePos = _cam.ScreenToWorldPoint(Mouse.current?.position.ReadValue() ?? Vector2.zero);
        Vector2 dirMouse = (mousePos - (Vector2)playerPos).normalized;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Vector2 touchWorld = _cam.ScreenToWorldPoint(touch.position);
                return (touchWorld - playerPos).normalized;
            }
        }


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

        bool wasPressed = shift || lb || l3 || _uiCounteringRequested;
        
        return wasPressed;
    }

    public bool IsLaunchPressed()
    {
        bool space = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool aButton = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        bool wasPressed = space || aButton || _uiLaunchRequested;

        _uiLaunchRequested = false;

        return wasPressed;
    }

    public bool IsPausePressed()
    {
        bool scape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool startButton = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;

        return scape || startButton;
    }

    public void ButtonLaunchPressed()
    {
        _uiLaunchRequested = true;

    }

    public void ButtonCounterPressed()
    {
        _uiCounteringRequested = true;

    }
    
    public void ButtonCounterUp()
    {
        _uiCounteringRequested = false;

    }
}