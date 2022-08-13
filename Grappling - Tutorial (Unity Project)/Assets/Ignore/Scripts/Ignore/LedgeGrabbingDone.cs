using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dave's Tutorials - LedgeGrabbing
///
// Content:
/// - detecting and moving towards ledges
/// - holding onto ledges
/// - jumping away from ledges
///
// Note:
/// This script is kind of like an extension of the Climbing script, 
/// this way it's easier to understand and I can keep the script shorter 
/// 

public class LedgeGrabbingDone : MonoBehaviour
{
    [Header("References")]
    private PlayerMovementAdvanced pm;
    public Transform orientation;
    public Transform cam;
    private Rigidbody rb;

    [Header("Ledge Grabbing")]
    public KeyCode jumpKey = KeyCode.Space;

    public float moveToLedgeSpeed;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpForce;
    public float maxLedgeGrabDistance;

    public float minTimeOnLedge;
    private float timeOnLedge;

    public bool holding;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;

    private Transform lastLedge;
    public Transform currLedge;

    private RaycastHit ledgeHit;

    [Header("Exiting")]
    public bool exitingLedge;
    public float exitLedgeTime;
    private float exitLedgeTimer = 0.2f;


    private void Start()
    {
        // get references
        pm = GetComponent<PlayerMovementAdvanced>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        LedgeDetection();
        SubStateMachine();
    }

    // a very simple state machine which takes care of the ledge grabbing state
    private void SubStateMachine()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool anyInputKeyPressed = (horizontalInput != 0 || verticalInput != 0);

        // SubState 1 - Holding onto ledge
        if (holding)
        {
            FreezeRigidbodyOnLedge();

            timeOnLedge += Time.deltaTime;

            if (timeOnLedge > minTimeOnLedge && anyInputKeyPressed) ExitLedgeHold();

            if(Input.GetKeyDown(jumpKey)) LedgeJump();
        }

        // SubState 2 - Exiting Ledge
        else if (exitingLedge)
        {
            if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
            else exitingLedge = false;
        }
    }

    private void LedgeDetection()
    {
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);

        if (!ledgeDetected) return;

        float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);

        if (ledgeHit.transform == lastLedge) return;

        if (distanceToLedge < maxLedgeGrabDistance && !holding) EnterLedgeHold();
    }

    private void LedgeJump()
    {
        ExitLedgeHold();

        Invoke(nameof(DelayedForce), 0.05f);
    }

    private void DelayedForce()
    {
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpForce;
        rb.velocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    private void EnterLedgeHold()
    {
        if (exitingLedge) return;

        holding = true; //

        pm.restricted = true; 
        pm.unlimited = true;

        currLedge = ledgeHit.transform; //
        lastLedge = ledgeHit.transform; //

        rb.useGravity = false; //
        rb.velocity = Vector3.zero; //
    }

    private void FreezeRigidbodyOnLedge()
    {
        rb.useGravity = false;

        Vector3 directionToLedge = currLedge.position - transform.position;
        float distanceToLedge = Vector3.Distance(transform.position, currLedge.position);

        // Move player towards ledge
        if (distanceToLedge > 1f)
        {
            if (rb.velocity.magnitude < moveToLedgeSpeed)
                rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);

            print("moving to ledge");
        }

        // Hold onto ledge
        else
        {
            if (pm.unlimited) pm.unlimited = false;
            if (!pm.freeze) pm.freeze = true;
            ///rb.velocity = Vector3.zero;
            print("hanging on ledge");
        }

        // Exit ledge hold (bug fixing)
        if (distanceToLedge > maxLedgeGrabDistance && holding) ExitLedgeHold();
    }

    private void ExitLedgeHold()
    {
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        holding = false; //
        timeOnLedge = 0; //

        pm.freeze = false; //
        pm.unlimited = false; 
        pm.restricted = false;

        rb.useGravity = true;

        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }
}
