using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;

    bool grounded;
    Vector3 velocity;
    Vector2 moveInput;
    Vector3 move;
    Vector2 lookInput;
    Vector2 lookVector;
    float lookRotation = 0;

    [SerializeField]AnimationCurve accelerationCurve;

    // Player gravity
    [SerializeField] float gravity = -20f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float groundDistance = 0.4f;

    // Player speed and acceleration
    [SerializeField] float minAcceleration = 0f;
    [SerializeField] float maxAcceleration = 7f;
    [SerializeField] float acceleration = 5f;
    [SerializeField] float currentSpeed =5f;
    [SerializeField] float lookSensitivity = 1f;

    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;

    //Spraying
    [SerializeField] public GameObject spray;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Debug.Log(lookInput);

        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // The start of player acceleration

        // currentSpeed = acceleration * accelerationCurve .Evaluate(Time.deltaTime);
        // acceleration = Mathf.Clamp(acceleration, minAcceleration, maxAcceleration);

        //Gravity

        lookRotation -= lookInput.y;
        lookRotation = Mathf.Clamp(lookRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(lookRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);         
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Spray(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            print("we started spraying");
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>() * lookSensitivity;
    }
}
