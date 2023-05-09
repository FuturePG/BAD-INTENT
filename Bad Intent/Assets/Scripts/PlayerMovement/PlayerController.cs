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

    [SerializeField]AnimationCurve accelerationCurve;

    // Player gravity
    [SerializeField] float gravity = -20f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float groundDistance = 0.4f;

    // Player speed and acceleration
    [SerializeField] float minAcceleration = 0f;
    [SerializeField] float maxAcceleration = 7f;
    [SerializeField] float acceleration = 5f;
    [SerializeField] float currentSpeed =0f;

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
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // The start of player acceleration

        currentSpeed = acceleration * accelerationCurve .Evaluate(Time.deltaTime);
        acceleration = Mathf.Clamp(acceleration, minAcceleration, maxAcceleration);

        if (controller.Move(move * currentSpeed * Time.deltaTime) == CollisionFlags.None)
        {
            accelerationCurve = new AnimationCurve();
        }

        //Gravity

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
}
