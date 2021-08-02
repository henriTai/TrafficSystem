using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// This class executes given values to AI vehicle's wheel collider physics, brake lights and turn signals and
/// sets given color as car's main color.
/// </summary>
public class CarControlUnit : MonoBehaviour
{
    /// <summary>
    /// Car's wheel collider wheel axis (right and left wheel pair).
    /// </summary>
    public List<AxleInfo> axleInfos;
    /// <summary>
    /// Gameobject of car's chassis.
    /// </summary>
    [Header("Car color")]
    public GameObject carBody;
    /// <summary>
    /// Material index of car's main color.
    /// </summary>
    public int mainColorIndex;
    /// <summary>
    /// Shared mesh renderer of car's brake lights.
    /// </summary>
    [Header("Car light mesh renderers")]
    public MeshRenderer brake_mr;
    /// <summary>
    /// Shared mesh rendere of car's left turn signal lights.
    /// </summary>
    public MeshRenderer leftSignal_mr;
    /// <summary>
    /// Shared mesh rendere of car's right turn signal lights.
    /// </summary>
    public MeshRenderer rightSignal_mr;
    /// <summary>
    /// Material of unlit brake light.
    /// </summary>
    [Header("Car light materials")]
    public Material brakeLight_OFF;
    /// <summary>
    /// Material of lit brake light.
    /// </summary>
    public Material brakeLight_ON;
    /// <summary>
    /// Material of unlit turn signal light.
    /// </summary>
    public Material turnSignal_OFF;
    /// <summary>
    /// Material of lit turn signal light.
    /// </summary>
    public Material turnSignal_ON;
    /// <summary>
    /// Raycast point gameobject, in the middle, in front of the car.
    /// </summary>
    [Header("Raycast points")]
    public GameObject frontMiddle;
    /// <summary>
    /// Raycast point gameobject, on the left side, in front of the car.
    /// </summary>
    public GameObject leftFront;
    /// <summary>
    /// Raycast point gameobject, on the left side, half way at the side of the car.
    /// </summary>
    public GameObject leftMiddle;
    /// <summary>
    /// Raycast point gameobject, back of the car, on the left side.
    /// </summary>
    public GameObject leftRear;
    /// <summary>
    /// Raycast point gameobject, on the right side, in front of the car.
    /// </summary>
    public GameObject rightFront;
    /// <summary>
    /// Raycast point gameobject, on the right side, half way at the side of the car.
    /// </summary>
    public GameObject rightMiddle;
    /// <summary>
    /// Raycast point gameobject, back of the car, on the right side.
    /// </summary>
    public GameObject rightRear;

    /// <summary>
    /// Delivers given value to car's wheel collider physics.
    /// </summary>
    /// <param name="brakeForce">Applied brake force.</param>
    /// <param name="motorForce">Applied motor force.</param>
    /// <param name="steerAmount">Applied steer angle.</param>
    public void DriveCar(float brakeForce, float motorForce, float steerAmount)
    {
        foreach (AxleInfo a in axleInfos)
        {
            a.leftWheel.brakeTorque = brakeForce;
            a.rightWheel.brakeTorque = brakeForce;

            if (a.steering)
            {
                a.leftWheel.steerAngle = steerAmount;
                a.rightWheel.steerAngle = steerAmount;
            }
            if (a.motor)
            {
                a.leftWheel.motorTorque = motorForce;
                a.rightWheel.motorTorque = motorForce;
            }
            ApplyLocalPositionsToVisuals(a);
        }
    }
    /// <summary>
    /// Updates wheel's visual appearance accordingly when wheel turns.
    /// </summary>
    /// <param name="a">Axleinfo reference which visual's are updated.</param>
    private void ApplyLocalPositionsToVisuals(AxleInfo a)
    {
        if (a.leftVisuals == null || a.rightVisuals == null)
        {
            Debug.Log("wheel transforms not assigned to axle infos");
            return;
        }
        Vector3 position;
        Quaternion rotation;
        a.leftWheel.GetWorldPose(out position, out rotation);
        a.leftVisuals.transform.position = position;
        a.rightVisuals.transform.position = position;
        a.rightWheel.GetWorldPose(out position, out rotation);
        a.rightVisuals.transform.position = position;
        a.rightVisuals.transform.rotation = rotation;
    }
    /// <summary>
    /// Set's brake lights on or off.
    /// </summary>
    /// <param name="lightsOn">New brake light status (is brake lights on?).</param>
    public void SetBrakeLightStatus(bool lightsOn)
    {
        if (lightsOn)
        {
            brake_mr.material = brakeLight_ON;
        }
        else
        {
            brake_mr.material = brakeLight_OFF;
        }
    }
    /// <summary>
    /// Sets given status to turn signals of given side.
    /// </summary>
    /// <param name="leftSide">Is light status set to the left side turn signals (or right side)?</param>
    /// <param name="lightsOn">Are these turn signal lights set on (or off)?</param>
    public void SetTurnSignal(bool leftSide, bool lightsOn)
    {
        if (leftSide)
        {
            if (lightsOn)
            {
                leftSignal_mr.material = turnSignal_ON;
            }
            else
            {
                leftSignal_mr.material = turnSignal_OFF;
            }
        }
        else
        {
            if (lightsOn)
            {
                rightSignal_mr.material = turnSignal_ON;
            }
            else
            {
                rightSignal_mr.material = turnSignal_OFF;
            }
        }
    }
    /// <summary>
    /// Sets car's rigid body's center of mass to given height relative to the car.
    /// </summary>
    /// <param name="value">Height of the car's center of mass.</param>
    public void SetCenterOfMassHeight(float value)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb)
        {
            Vector3 com = rb.centerOfMass;
            com.y = value;
            rb.centerOfMass = com;
        }
    }
    /// <summary>
    /// Applies given color as car's main color.
    /// </summary>
    /// <param name="color">Car's main color.</param>
    public void SetCarColor(Color color)
    {
        MeshRenderer rend = carBody.GetComponent<MeshRenderer>();
        if (rend)
        {
            rend.materials[mainColorIndex].color = color;
        }
    }

}
/// <summary>
/// Data class of wheel collider's wheel pair (left and right).
/// </summary>
[System.Serializable]
public class AxleInfo
{
    /// <summary>
    /// Left wheel's wheel collider component.
    /// </summary>
    public WheelCollider leftWheel;
    /// <summary>
    /// Right wheel's wheel collider component.
    /// </summary>
    public WheelCollider rightWheel;
    /// <summary>
    /// Transform of car model's left wheel. This is used for updating visuals.
    /// </summary>
    public Transform leftVisuals;
    /// <summary>
    /// Transform of car model's right wheel. This is used for updating visuals.
    /// </summary>
    public Transform rightVisuals;
    /// <summary>
    /// Is this axle connected to the motor?
    /// </summary>
    public bool motor;
    /// <summary>
    /// Is this axle connected to the steering?
    /// </summary>
    public bool steering;
}
