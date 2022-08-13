using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallThroughHoop : MonoBehaviour
{
    public Rigidbody ballRb;
    public Transform ballT;
    public Transform hoop;

    public float h;

    public float timeDelay;

    private void Start()
    {
        ballRb = GetComponent<Rigidbody>();
        ballT = transform;

        //Invoke(nameof(DelayedForce), timeDelay);
        Invoke(nameof(LaunchBall), timeDelay);
    }

    private void DelayedForce()
    {
        // Given Variables
        // float h is set in inspector
        float d = hoop.position.z - ballT.position.z;
        float g = Physics.gravity.y;
        float highestYToHoopY = hoop.position.y - h; // some negative value
        highestYToHoopY *= -1f; // making it positive

        // Upwards Motion
        float up_finalVelocity = 0;

        /// using 5th equation
        /// u = -Sqrt(2gh)
        float up_initialVelocity = Mathf.Sqrt(2 * -g * h);

        /// using 4th equation
        /// t = Sqrt(-(2h/g))
        float up_time = Mathf.Sqrt(-((2 * h) / g));

        print("Upwards Motion: initial = " + up_initialVelocity + " time = " + up_time);

        // Downwards Motion
        float down_initialVelocity = 0;

        /// using 3th equation
        /// t = Sqrt(2h/g)
        float down_time = Mathf.Sqrt((2 * highestYToHoopY) / -g);

        /// using 5th equation
        /// v = Sqrt(2g*highestYToHoopY)
        float down_finalVelocity = Mathf.Sqrt(2 * -g * highestYToHoopY);

        print("Downwards Motion: final = " + down_finalVelocity + " time = " + down_time);

        // Right Motion
        float right_accel = 0f;
        float right_time = up_time + down_time;

        /// using 3th equation
        /// u = d/t (since accel = 0 part of the equation falls away)
        float right_initialVelocity = d / right_time;

        /// using 4th equation
        /// v = d/t
        float right_finalVelocity = right_initialVelocity; /// who would have thought you genious


        // add initial velocity
        ballRb.velocity = new Vector3(0f, up_initialVelocity, right_initialVelocity);
    }

    /// cleaner function by sebastian lague
    private void LaunchBall()
    {
        ballRb.velocity = CalculateLaunchVelocity();
    }

    private Vector3 CalculateLaunchVelocity()
    {
        float gravity = Physics.gravity.y;
        float displacementY = hoop.position.y - ballT.position.y;
        Vector3 displacementXZ = new Vector3(hoop.position.x - ballT.position.x, 0f, hoop.position.z - ballT.position.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity));

        return velocityXZ + velocityY;
    }

    /// coming soon
    private void UltimateKinematicEquationSolver(float s, float u, float v, float t, float a)
    {

    }
}
