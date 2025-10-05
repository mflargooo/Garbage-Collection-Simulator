using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions inputActions;

    public event Action OnSpaceStarted;
    public event Action OnSpacePerformed;
    public event Action OnSpaceCanceled;

    void Awake()
    {
        if (!Application.isPlaying)
            return;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        if (!Application.isPlaying)
            return;

        inputActions.Player.Enable();

        inputActions.Player.Break.started += ctx => OnSpaceStarted?.Invoke();
        inputActions.Player.Break.performed += ctx => OnSpacePerformed?.Invoke();
        inputActions.Player.Break.canceled += ctx => OnSpaceCanceled?.Invoke();
    }
}
