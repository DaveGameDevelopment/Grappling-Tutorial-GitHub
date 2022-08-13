using UnityEngine;


// Dave MovementLab - Spring
///
// Content:
/// - calculating the values used for the GrapplingRope animation
/// 
// I learned how to create this script by following along with a YouTube tutorial
// Credits: https://youtu.be/8nENcDnxeVE


public class Spring_MLab
{
    // values explained in the GrapplingRope_MLab script
    private float strength;
    private float damper;
    private float target;
    private float velocity;
    private float value;

    public void Update(float deltaTime)
    {
        // calculate the animation values using some formulas I don't understand :D
        var direction = target - value >= 0 ? 1f : -1f;
        var force = Mathf.Abs(target - value) * strength;
        velocity += (force * direction - velocity * damper) * deltaTime;
        value += velocity * deltaTime;
    }

    public void Reset()
    {
        // reset values
        velocity = 0f;
        value = 0f;
    }

    /// here you'll find all functions used to set the variables of the simulation
    #region Setters

    public void SetValue(float value)
    {
        this.value = value;
    }

    public void SetTarget(float target)
    {
        this.target = target;
    }

    public void SetDamper(float damper)
    {
        this.damper = damper;
    }

    public void SetStrength(float strength)
    {
        this.strength = strength;
    }

    public void SetVelocity(float velocity)
    {
        this.velocity = velocity;
    }

    public float Value => value;

    #endregion
}