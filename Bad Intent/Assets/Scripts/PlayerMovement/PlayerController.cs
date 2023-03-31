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

    [SerializeField] float minSpeed = 3f;
    [SerializeField] float maxSpeed = 7f;
    [SerializeField] float acceleration = .5f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float groundDistance = 0.4f;

    //Skateboard mechanic 
    [SerializeField] float skateboardBoost = 2.0f;
    [SerializeField] float skateboardCooldown = 30.0f;

    [SerializeField] bool isMoving = false;
    [SerializeField] bool isJumping = false;
    [SerializeField] bool isVaulting = false;
    [SerializeField] bool isSkating = false;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;

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
        controller.Move(move * minSpeed * acceleration * Time.deltaTime);

        velocity.y += -gravity * Time.deltaTime;

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

    public void Boost(InputAction.CallbackContext context)
    {
        if (context.performed && grounded)
        {
           // velocity.z = 
        }
    }

    void FixedUpdate()
    {
        
    }
}
