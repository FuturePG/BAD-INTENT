using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class mouseLook : MonoBehaviour
{
    [SerializeField] float minViewDistance = -25f;
    [SerializeField] float maxViewDistance = 25f;
    [SerializeField] Transform playerBody;

    [SerializeField] CharacterController controller;
    [SerializeField] PlayerInputs playerControls;
    public float sensitivity = 100f;

    Vector2 lookValue;
    Vector2 lookVector;
    Quaternion activeVector;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        lookVector += lookValue * sensitivity * Time.deltaTime;
        lookVector.y = Mathf.Clamp(lookVector.y, minViewDistance, maxViewDistance);
        transform.rotation = Quaternion.Euler(lookVector.x, 0, 0);
        transform.rotation = Quaternion.Euler(0, lookVector.y, 0);
    }

    public void Look(InputAction.CallbackContext context)
    {
        lookValue = context.ReadValue<Vector2>();
    }
}
