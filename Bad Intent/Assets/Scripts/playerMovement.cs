using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody myRigidbody;
    PlayerInput playerInput;
    PlayerInputs playerInputs;

    // Start is called before the first frame update
    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        playerInputs = new PlayerInputs();
        playerInputs.InGame.Enable();
        playerInputs.InGame.Jump.performed += Jump;
    }

    private void FixedUpdate()
    {
        Vector2 inputVector = playerInputs.InGame.Movement.ReadValue<Vector2>();
        Debug.Log(inputVector);
        float speed = 1f;
        myRigidbody.AddForce(new Vector3(inputVector.x, 0, inputVector.y) * speed, ForceMode.Force);
    }

    // Update is called once per frame
    void Jump(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        if (context.performed)
        {
            Debug.Log("Jump! " + context.phase);
            myRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }
}
