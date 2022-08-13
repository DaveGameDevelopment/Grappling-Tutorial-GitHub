using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashingDone : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovementDashingDone pm;

    [Header("Dashing")]
    public float dashForce = 70f;
    public float dashUpwardForce = 2f;
    public float maxDashYSpeed;
    public float dashDuration = 0.4f;

    [Header("CameraEffects")]
    public PlayerCam cam;
    public float dashFov;

    [Header("Settings")]
    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = true;

    [Header("Cooldown")]
    public float dashCd = 1.5f; // cooldown of your dash ability
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.E;

    private void Start()
    {
        if (playerCam == null)
            playerCam = Camera.main.transform;

        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementDashingDone>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
            Dash();

        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;
    }

    private void Dash()
    {
        // cooldown implementation
        if (dashCdTimer > 0) return;
        else dashCdTimer = dashCd;

        cam.DoFov(dashFov);

        // this will cause the PlayerMovement script to change to MovementMode.dashing
        pm.dashing = true;
        pm.maxYSpeed = maxDashYSpeed;

        Transform forwardT;

        // decide wheter you want to use the playerCam or the playersOrientation as forward direction
        if (useCameraForward)
            forwardT = playerCam; // where you're looking
        else
            forwardT = orientation; // where you're facing (no up or down)

        // call the GetDirection() function below to calculate the direction
        Vector3 direction = GetDirection(forwardT);

        // calculate the forward and upward force
        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        // disable gravity of the players rigidbody if needed
        if (disableGravity)
            rb.useGravity = false;

        // add the dash force (deayed)
        delayedForceToApply = forceToApply;

        // limit y speed
        //if (delayedForceToApply.y > maxDashYSpeed)
        //    delayedForceToApply = new Vector3(delayedForceToApply.x, maxDashYSpeed, delayedForceToApply.z);

        print("dashForce: " + delayedForceToApply);
        Invoke(nameof(DelayedDashForce), 0.025f);

        // make sure the dash stops after the dashDuration is over
        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        if(resetVel)
            rb.velocity = Vector3.zero;

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.dashing = false;
        pm.maxYSpeed = 0;

        cam.DoFov(85f);

        // if you disabled it before, activate the gravity of the rigidbody again
        if (disableGravity)
            rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        // get the W,A,S,D input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // 2 Vector3 for the forward and right velocity
        Vector3 direction = new Vector3();

        if (allowAllDirections)
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        else
            direction = forwardT.forward;

        if (verticalInput == 0 && horizontalInput == 0) 
            direction = forwardT.forward;

        return direction.normalized;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCam.position, playerCam.forward);
    }
}
