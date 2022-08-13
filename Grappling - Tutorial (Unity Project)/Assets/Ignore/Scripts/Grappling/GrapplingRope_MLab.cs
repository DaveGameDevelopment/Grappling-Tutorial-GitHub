using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Dave MovementLab - GrapplingRope
///
// Content:
/// - drawing and animating the grappling rope
/// 
// Note:
/// This script is assigned to the grappling gun
///
// I learned how to create this script by following along with a YouTube tutorial
// Credits: https://youtu.be/8nENcDnxeVE
///
/// -> if you don't understand the code, just watch the tutorial


public class GrapplingRope_MLab : MonoBehaviour
{
    [Header("References")]
    public Grappling grappling;

    [Header("Settings")]
    public int quality = 200; // how many segments the rope will be split up in
    public float damper = 14; // this slows the simulation down, so that not the entire rope is affected the same
    public float strength = 800; // how hard the simulation tries to get to the target point
    public float velocity = 15; // velocity of the animation
    public float waveCount = 3; // how many waves are being simulated
    public float waveHeight = 1;
    public AnimationCurve affectCurve;

    private Spring_MLab spring; // a custom script that returns the values needed for the animation
    private LineRenderer lr;
    private Vector3 currentGrapplePosition;

    private void Awake()
    {
        // get references
        lr = GetComponent<LineRenderer>();
        spring = new Spring_MLab();
        spring.SetTarget(0);
    }

    //Called after Update
    private void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        // if not grappling, don't draw rope
        if (!grappling.IsGrappling())
        {
            currentGrapplePosition = grappling.gunTip.position;

            // reset the simulation
            spring.Reset();

            // reset the positionCount of the lineRenderer
            if (lr.positionCount > 0)
                lr.positionCount = 0;

            return;
        }

        if(lr.positionCount == 0)
        {
            // set the start velocity of the simulation
            spring.SetVelocity(velocity);

            // set the positionCount of the lineRenderer depending on the quality of the rope
            lr.positionCount = quality + 1;
        }

        // set the spring simulation
        spring.SetDamper(damper);
        spring.SetStrength(strength);
        spring.Update(Time.deltaTime);

        Vector3 grapplePoint = grappling.GetGrapplePoint();
        Vector3 gunTipPosition = grappling.gunTip.position;

        // find the upwards direction relative to the rope
        Vector3 up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        // lerp the currentGrapplePositin towards the grapplePoint
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        // loop through all segments of the rope and animate them
        for (int i = 0; i < quality + 1; i++)
        {
            float delta = i / (float)quality;
            // calculate the offset of the current rope segment
            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spring.Value * affectCurve.Evaluate(delta);

            // lerp the lineRenderer position towards the currentGrapplePosition + the offset you just calculated
            lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
    }
}
