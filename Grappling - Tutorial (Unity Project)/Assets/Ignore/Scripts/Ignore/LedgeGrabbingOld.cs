using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabbingOld : MonoBehaviour
{
    public Transform orientation;
    public Transform cam;
    public Rigidbody rb;

    [Header("Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;

    [Header("Ledge Grabbing")]
    public float maxLedgeGrabDistance;
    public float moveToLedgeSpeed;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpForce;

    private bool holding;

    public Transform currLedge;

    private RaycastHit ledgeHit;
    private Vector3 directionToLedge;
    private float distanceToLedge;

    private void Update()
    {
        StateMachine();
        WallCheck();
    }

    private void StateMachine()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);

        // State - Ledge Holding
        if (holding)
        {
            FreezeRigidbodyOnLedge();

            Vector2 flatDirectionToLedge = new Vector2(directionToLedge.x, directionToLedge.z);
            float dotProduct = Vector2.Dot(flatDirectionToLedge, inputVector);

            // -1 means 180* away, 0 means a right angle (so limit the angle somewhere between 0 and -1)
            if (dotProduct < 0) ExitLedgeHold();
        }

        // if(!holding) ClimbJump();
        // else LedgeJump();
    }

    private void WallCheck()
    {
        RaycastHit ledgeHit;
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, orientation.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);

        directionToLedge = ledgeHit.transform.position - transform.position;
        distanceToLedge = directionToLedge.magnitude;

        if(ledgeDetected && distanceToLedge < maxLedgeGrabDistance && !holding) EnterLedgeHold();
    }

    private void LedgeJump()
    {
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + cam.up * ledgeJumpUpForce;
        // evt. noch + orientation.forward * ledgeJumpForwardForce * 0.5f

        rb.AddForce(forceToAdd, ForceMode.Impulse);

        ExitLedgeHold();
    }

    private void EnterLedgeHold()
    {
        holding = true;
        // pm.freeze = true;
    }

    private void FreezeRigidbodyOnLedge()
    {
        // Move player towards ledge
        if(distanceToLedge > 0.6f)
        {
            rb.velocity = directionToLedge * moveToLedgeSpeed;
        }

        // Hold onto ledge
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    private void ExitLedgeHold()
    {
        holding = false;
    }
}
