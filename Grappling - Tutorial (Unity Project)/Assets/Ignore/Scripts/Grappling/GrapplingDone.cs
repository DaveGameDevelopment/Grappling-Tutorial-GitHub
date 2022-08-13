using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingDone : MonoBehaviour
{
    [Header("References")]
    private PlayerMovementGrappling pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;

    [Header("Grappling")]
    public float maxGrappleDistance = 25f;
    public float grappleDelayTime = 0.5f;
    public float overshootYAxis = 2f;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd = 2.5f;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    private bool grappling;

    private void Start()
    {
        pm = GetComponent<PlayerMovementGrappling>();
    }

    private void Update()
    {
        // input
        if (Input.GetKeyDown(grappleKey)) StartGrapple();

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (grappling)
            lr.SetPosition(0, gunTip.position);
    }

    public void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        grappling = true;

        pm.freeze = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExcecuteGrapple), grappleDelayTime);
        }

        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
    }

    public void ExcecuteGrapple()
    {
        pm.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOfArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOfArc = overshootYAxis;

        pm.JumpToPosition(grapplePoint, highestPointOfArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        pm.freeze = false;

        grappling = false;

        grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }

    public void OnObjectTouch()
    {
        ///if (pm.activeGrapple) StopGrapple();
    }


    public bool IsGrappling()
    {
        return grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

    /*
    void some()
    {
        // Mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            rb.velocity = Vector3.zero;
            moveSpeed = 0f;
        }

        else if (activeGrapple)
        {
            state = MovementState.grappling;
            moveSpeed = sprintSpeed;
        }
    }

    // Uses Vector Maths to make the player jump exactly to a desired position
    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        //activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
        enableMovementOnNextTouch = true;
    }
    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        rb.velocity = velocityToSet;
        print("velocityToSet: " + velocityToSet + " velocity: " + rb.velocity);
    }

    public void ResetRestrictions()
    {
        //activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling_MLab>().StopGrapple();
        }
    }

    */
}