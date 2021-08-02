using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Car's AI uses this class to update its steering value.
/// </summary>
public static class CarSteeringUpdate
{
    /// <summary>
    /// Updates steering and checks if the car is oversteering.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void CheckSteering(CarDriveData data)
    {
        Vector3 relativeVector = data.carObject.transform.InverseTransformPoint(data.nextNodePos);
        float newSteer = (relativeVector.x / relativeVector.magnitude) * CarDriveData.maxSteeringAngle;

        if (data.steerAmount < 0)
        {
            if (newSteer < data.steerAmount)
            {
                data.overSteering = true;
            }
            else
            {
                data.overSteering = false;
            }
        }
        else
        {
            if (newSteer > data.steerAmount)
            {
                data.overSteering = true;
            }
            else
            {
                data.overSteering = false;
            }
        }
        data.steerAmount = newSteer;
    }
}
