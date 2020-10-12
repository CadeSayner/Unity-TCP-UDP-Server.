using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum animationState {idle, forward}
public class Player : MonoBehaviour
{   
    public animationState AnimationState;
    public GameObject head;
    public GameObject model;

    public int id;
    public string username;

    public float maximum_Velocity = 10f;
    public float MovementSpeed = 5f;
    public float maxGrappleToDistance = 35f;
    public float GrappleSpeed = 5f;
    public Rigidbody rb;
    public float stoppingVelocity = 0.1f;
    public float counterConstant = 0.5f;
    public float minBoostThreshold = 0.01f;
    public float maxBoostThreshold = 100f; // The x - intercept of the function.

    private float moveSpeed;
    private int[] inputs;

    // Movement fields.
    private int verticalInput;
    private int horizontalInput;
    private int counterInput;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();

        moveSpeed = MovementSpeed / Constants.TICKS_PER_SEC;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;

        inputs = new int[3];
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void Update()
    {
        // Get input
        verticalInput = inputs[0];
        horizontalInput = inputs[1];
        counterInput = inputs[2];

        // Add forces based on input.
        Move();

        // Simulate physics.
        Physics.Simulate(Time.fixedDeltaTime);

        // Send snapshot back to all clients.
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
        ServerSend.PlayerAnimation(this);
    }

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    private void Move()
    {   
        float Boost = 1 + lowVelBoost();
        rb.AddForce(head.transform.forward * verticalInput * moveSpeed * Boost, ForceMode.Impulse);

        rb.AddForce(head.transform.right * horizontalInput * moveSpeed * Boost, ForceMode.Impulse);

        if (rb.velocity.sqrMagnitude > stoppingVelocity)
        {
            rb.AddForce(-rb.velocity * counterInput * counterConstant, ForceMode.Impulse);
        }
    }

    private float lowVelBoost()
    {
        if (rb.velocity.sqrMagnitude < maxBoostThreshold && rb.velocity.sqrMagnitude > minBoostThreshold)
        {
            return calcBoost(rb.velocity.sqrMagnitude);

            // TODO:
            // Boosting animation would be accessed here...
        }

        else
        {
            return 0;
        }
    }

    private float calcBoost(float velocityMagnitude)
    {
        float m = -0.02f;
        float c = 2.002f;

        float boostMagnitude = m * rb.velocity.sqrMagnitude + c;
        // Debug.Log(boostMagnitude.ToString());

        return boostMagnitude;
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>

    // After fetching the correct player from the server side players dictionary, the players SetInput() is called.
    public void SetInput(int[] _inputs, Quaternion head_rotation, Quaternion model_rotation, animationState animState)
    {
        inputs = _inputs;

        head.transform.rotation = head_rotation;
        model.transform.rotation = model_rotation;

        // Set the player instance animation state to what it is on the client - side.
        AnimationState = animState;
    }

}
