using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// This is the Ai vehicle's main AI unit operating AI functionalities.
/// </summary>
public class CarAIMain : MonoBehaviour
{
    /// <summary>
    /// Data of the car this AI is controlling.
    /// </summary>
    public CarDriveData carData;
    /// <summary>
    /// Reference to traffic manager unit.
    /// </summary>
    public CarSpawner spawner;

    /// <summary>
    /// Initializes car's data.
    /// </summary>
    private void Start()
    {
        CarDataInitializer.InitCarData(carData);
    }

    /// <summary>
    /// Updates car's AI.
    /// </summary>
    private void FixedUpdate()
    {
        UpdateSpeedAndPosition();

        bool changedLane = CarRouteUpdate.UpdateNode(carData);
        CarLaneChangeUpdate.CheckLaneChange(carData);
        CarLightUpdate.OperateTurnSignals(carData, changedLane);
        CarIntersectionUpdate.CheckUpcomingIntersection(carData);
        CarSteeringUpdate.CheckSteering(carData);
        CarCurveUpdate.CheckCurve(carData);
        CarFrontCheck.CheckFront(carData);
        CarCrosswalkUpdate.CheckCrosswalks(carData);
        CarLightUpdate.UseBrakeLights(carData);
        UpdateBrakingAndMotorValues();
        carData.carControl.DriveCar(carData.appliedBrakeForce, carData.motor, carData.steerAmount);
    }

    /// <summary>
    /// Decides values for car's physics.
    /// </summary>
    private void UpdateBrakingAndMotorValues()
    {
        UpdateStoppingSpeedMultiplier();

        bool braking = false;
        float tSpeed = carData.targetSpeed;
        if (carData.curveAhead)
        {
            tSpeed = CarDriveData.curveMaxSpeed;
        }
        if (carData.slowingDownToAllowLaneChange)
        {
            tSpeed *= 0.75f;
        }
        tSpeed *= carData.stoppingSpeedMultiplier;
        // update braking status
        if (carData.obstacleAhead && carData.previousHitDistance < 5f)
        {
            braking = true;
            carData.motor = 0f;
            carData.appliedBrakeForce = float.MaxValue;
        }
        else if (carData.carSpeed < tSpeed)
        {
            braking = false;
            carData.motor = CarDriveData.maxMotorTorque;
            carData.appliedBrakeForce = 0f;
        }
        else if (carData.carSpeed > tSpeed || carData.overSteering)
        {
            braking = true;
            carData.motor = 0f;
        }
        if (carData.intersectionYielding)
        {
            if (carData.carSpeed > 3f || carData.distanceToIntersection < 5f)
            {
                carData.motor = 0f;
            }
        }

        if (braking)
        {
            carData.appliedBrakeForce = float.MaxValue;
        }
    }
    /// <summary>
    /// Decides stopping speed multiplier based on the closest obstacle and stopping speed curve.
    /// </summary>
    private void UpdateStoppingSpeedMultiplier()
    {
        float dist = float.MaxValue;
        bool found = false;
        if (carData.intersectionYielding && carData.distanceToIntersection < CarDriveData.stoppingDistance)
        {
            found = true;
            dist = carData.distanceToIntersection;
            if (dist < 5f)
            {
                dist = 0f;
            }
        }
        if (carData.crosswalkYielding && carData.distanceToCrosswalk < CarDriveData.stoppingDistance)
        {
            if (carData.distanceToCrosswalk < dist)
            {
                found = true;
                dist = carData.distanceToCrosswalk;
                if (carData.distanceToCrosswalk < 1f)
                {
                    dist = 0f;
                }
            }
        }
        if (carData.obstacleAhead && carData.previousHitDistance < CarDriveData.stoppingDistance)
        {
            found = true;
            dist = carData.previousHitDistance;
            if (dist < 1f)
            {
                dist = 0f;
            }
        }
        if (found)
        {
            carData.stoppingSpeedMultiplier = carData.stoppingSpeedCurve.Evaluate(1f - (dist / CarDriveData.stoppingDistance));
            if (carData.stoppingSpeedMultiplier < 0.1f)
            {
                carData.stoppingSpeedMultiplier = 0f;
            }
        }
        else
        {
            carData.stoppingSpeedMultiplier = 1f;
        }
    }
    /// <summary>
    /// Calculates car's current speed and saves car's current position in world space.
    /// </summary>
    private void UpdateSpeedAndPosition()
    {
        Vector3 pos = carData.carObject.transform.position;
        carData.carSpeed = Vector2.Distance(new Vector2(pos.x, pos.z),
            new Vector2(carData.carPosition.x, carData.carPosition.z)) / Time.deltaTime;
        carData.carPosition = pos;
        carData.forwardVector = carData.carObject.transform.forward;
    }
    /// <summary>
    /// Other AI car on adjacent lane trying to change to the smae lane as this AI car unit unit can use this function to
    /// ask this car to slow down to enable succesful lane change.
    /// </summary>
    /// <param name="data">CarDriveData component of the car wanting to change lane.</param>
    public void SlowDownToAllowLaneChange(CarDriveData data)
    {
        carData.slowingDownToAllowLaneChange = true;
        carData.carChangingLane = data;
    }

    /// <summary>
    /// This sets car to lane change mode, if given lane change is possible, ie. if Nodes nesNode has
    /// Nodes set to LaneChangeLeft or LaneChangeRight, depending on the requested lane change direction. If possible,
    /// laneChange mode is set to either LaneChange.Request to left or LaneChange.RequestToRight and turn signal is set on.
    /// After a fixed amount of time, function UseTurnSignal will trigger laneChange to the next phaze of transition.
    /// </summary>
    /// <param name="toLane">enum direction of the requested lane change</param>
    public void TryChangeLane(IntersectionDirection toLane)
    {
        if (toLane == IntersectionDirection.Left)
        {
            if (carData.nextNode.LaneChangeLeft != null)
            {
                Debug.Log(gameObject.name + " requests to change lane to left");
                carData.laneChange = LaneChange.RequestToMove;
                carData.changingLeft = true;
                CarLightUpdate.TurnSignalsOn(carData, false);
            }
        }
        else if (toLane == IntersectionDirection.Right)
        {
            if (carData.nextNode.LaneChangeRight != null)
            {
                Debug.Log(gameObject.name + " requests to change lane to right");
                carData.laneChange = LaneChange.RequestToMove;
                carData.changingLeft = false;
                CarLightUpdate.TurnSignalsOn(carData, false);
            }
        }

    }
}
