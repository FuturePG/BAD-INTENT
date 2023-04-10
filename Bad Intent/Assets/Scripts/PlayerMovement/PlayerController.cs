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

    //velocity for the movement
    [SerializeField] float startingVelocity = 5f;
    [SerializeField] float decelerationVelocity = 3f;
    [SerializeField] float currentVelocity = 0f;
    [SerializeField] float minVelocity = 5f;
    [SerializeField] float maxVelocity = 7f;
    [SerializeField] float accelerationVelocity = .5f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] float groundDistance = 0.4f;

    //Dash mechanic 
    [SerializeField] float dash = 2.0f;
    [SerializeField] float dashCooldown = 30.0f;

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
            currentVelocity = currentVelocity + (decelerationVelocity * Time.deltaTime);
        }

        move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentVelocity * Time.deltaTime);

        velocity.y += -gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        currentVelocity = currentVelocity + (accelerationVelocity * Time.deltaTime);
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
          
        }
    }

    void FixedUpdate()
    {
        
    }
}
