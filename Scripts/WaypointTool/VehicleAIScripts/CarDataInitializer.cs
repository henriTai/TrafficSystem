using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Car's AI uses this class to initialize and reset car's data.
/// </summary>
public static class CarDataInitializer
{
    /// <summary>
    /// Initializes car's data at start.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void InitCarData(CarDriveData data)
    {
        InitNextLanes(data);
        data.currentLane = data.previousNode.ParentLane;
        if (data.previousNode.OutNode.LaneStartNode)
        {
            data.nextNode = data.nextLanes[0].nodesOnLane[0];
        }
        else
        {
            data.nextNode = data.previousNode.OutNode;
        }
        if (data.nextNode.OutNode.LaneStartNode)
        {
            data.oneAfterNextNode = data.nextLanes[0].nodesOnLane[0];
        }
        else
        {
            data.oneAfterNextNode = data.nextNode.OutNode;
        }

        data.nextNodePos = data.nextNode.transform.position;
        data.oneAfterNextNodePos = data.oneAfterNextNode.transform.position;

        data.carPosition = data.carObject.transform.position;
        data.speedLimit = KmsToMs.Convert(data.nextNode.SpeedLimit);
        data.targetSpeed = data.speedLimit;
        CarIntersectionUpdate.CheckUpcomingIntersection(data);
        CarCrosswalkUpdate.UpdateCrosswalks(data);
    }
    /// <summary>
    /// Initializes car's route for the next three consecutive lanes.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    private static void InitNextLanes(CarDriveData data)
    {
        data.nextLanes = new Lane[CarDriveData.nextLanesCount];
        Nodes n = data.previousNode.ParentLane.nodesOnLane[data.previousNode.ParentLane.nodesOnLane.Length - 1].OutNode;
        data.nextLanes[0] = CarRouteUpdate.NextRandomLane(n);

        if (CarDriveData.nextLanesCount > 1)
        {
            Lane l = data.nextLanes[0];
            for (int i = 1; i < CarDriveData.nextLanesCount; i++)
            {
                n = l.nodesOnLane[l.nodesOnLane.Length - 1].OutNode;
                data.nextLanes[i] = CarRouteUpdate.NextRandomLane(n);
                l = data.nextLanes[i];
            }
        }
    }
    /// <summary>
    /// Reset's cars data.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void ResetCar(CarDriveData data)
    {
        data.previousHitDistance = 0f;
        data.inIntersection = false;
        data.checkedIn = false;
        data.intersectionYielding = false;
        data.intersection = null;
        data.motor = 0f;
        data.appliedBrakeForce = 0f;
        data.overSteering = false;
        data.slowingDownToAllowLaneChange = false;
        data.laneChangeWait = 0f;
        CarLightUpdate.TurnSignalsOff(data);
        InitCarData(data);
        foreach (AxleInfo a in data.carControl.axleInfos)
        {
            a.leftWheel.motorTorque = 0f;
            a.rightWheel.motorTorque = 0f;
        }
        CarLightUpdate.BrakeLightsOff(data);
    }
}
