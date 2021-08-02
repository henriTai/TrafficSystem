using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Car's AI uses this class to execute lane changing.
/// </summary>
public static class CarLaneChangeUpdate
{
    /// <summary>
    /// Manages car's lane changing state machine routine.
    /// </summary>
    public static void CheckLaneChange(CarDriveData data)
    {
        //resets slowing down for other car changing lane if status is left on unintentionally
        if (data.slowingDownToAllowLaneChange)
        {
            if (data.carChangingLane.laneChange == LaneChange.NotChanging)
            {
                data.slowingDownToAllowLaneChange = false;
            }
        }
        if (data.laneChange == LaneChange.NotChanging)
        {
            return;
        }
        else
        {
            switch (data.laneChange)
            {
                case LaneChange.RequestToMove:
                    data.laneChangeWait += Time.deltaTime;
                    if (data.laneChangeWait > 1.5f)
                    {
                        data.laneChange = LaneChange.ReadyToMove;
                        data.laneChangeWait = 0f;
                    }
                    break;
                case LaneChange.ReadyToMove:
                    if (CanChangeLane(data) == true)
                    {
                        data.laneChange = LaneChange.ChangingLane;
                        LaneChangeUpdateNodes(data);
                    }
                    break;
                case LaneChange.ChangingLane:
                    if (data.nextNode != data.laneChangeNode && data.nextNode != data.laneChangeNode.OutNode && 
                        data.nextNode != data.laneChangeNode.OutNode.OutNode)
                    {
                        CarLightUpdate.TurnSignalsOff(data);
                        data.laneChange = LaneChange.NotChanging;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Checks if lane change is possible to make to the given direction of laneChange parameter using raycasts. If there is a car on
    /// adjacent lane partially behind this car, requests it to drop it's speed to give way for the lane change.
    /// </summary>
    /// <returns> Is lane change possible. </returns>
    private static bool CanChangeLane(CarDriveData data)
    {
        bool canChange = true;
        RaycastHit[] hits = new RaycastHit[3];
        Vector3[] dirs = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
        Vector3[] positions = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
        if (data.laneChange == LaneChange.ReadyToMove)
        {
            Vector3 fwd = data.forwardVector;
            Vector3 back = new Vector3(-fwd.x, fwd.y, -fwd.z);
            if (data.changingLeft)
            {
                Vector3 leftDir = new Vector3(-fwd.z, fwd.y, fwd.x);
                dirs[0] = (fwd * 0.6f + leftDir).normalized;
                dirs[1] = leftDir;
                dirs[2] = (back * 0.6f + leftDir).normalized;
                positions[0] = data.carControl.leftFront.transform.position;

                positions[1] = data.carControl.leftMiddle.transform.position;
                positions[2] = data.carControl.leftRear.transform.position;
            }
            else
            {
                Vector3 rightDir = new Vector3(fwd.z, fwd.y, -fwd.x);
                dirs[0] = (fwd * 0.6f + rightDir).normalized;
                dirs[1] = rightDir;
                dirs[2] = (back * 0.6f + rightDir).normalized;
                positions[0] = data.carControl.rightFront.transform.position;
                positions[1] = data.carControl.rightMiddle.transform.position;
                positions[2] = data.carControl.rightRear.transform.position;
            }
        }
        for (int i = 2; i >= 0; i--)
        {
            if (Physics.Raycast(positions[i], dirs[i], out hits[i], 6f))
            {
                Transform other = hits[i].transform;
                if (other.tag == "ambulance")
                {
                    canChange = false;
                    data.laneChange = LaneChange.NotChanging;
                    break;
                }
                if (other.tag == "car")
                {
                    if (i == 2)
                    {
                        CarAIMain otherAI = other.GetComponent<CarAIMain>();
                        if (otherAI)
                        {
                            otherAI.SlowDownToAllowLaneChange(data);
                        }
                    }
                    canChange = false;
                    break;
                }
            }
        }
        return canChange;
    }

    /// <summary>
    /// If lane change is still possible, updates route. Otherwise resets lane changing.
    /// </summary>
    private static void LaneChangeUpdateNodes(CarDriveData data)
    {
        if (data.laneChange == LaneChange.ChangingLane)
        {
            if (data.changingLeft)
            {
                data.laneChangeNode = data.nextNode.LaneChangeLeft;
                if (data.nextNode.LaneChangeLeft == null)
                {
                    if (!data.inIntersection)
                    {
                        CarLightUpdate.TurnSignalsOff(data);
                    }
                    data.laneChange = LaneChange.NotChanging;
                    return;
                }
                data.nextNode = data.nextNode.LaneChangeLeft;
            }
            else
            {
                data.laneChangeNode = data.nextNode.LaneChangeRight;
                if (data.nextNode.LaneChangeRight == null)
                {
                    if (!data.inIntersection)
                    {
                        CarLightUpdate.TurnSignalsOff(data);
                    }
                    data.laneChange = LaneChange.NotChanging;
                    return;
                }
                data.nextNode = data.nextNode.LaneChangeRight;
            }
        }
        data.previousNode = data.nextNode.InNode;
        CarDataInitializer.InitCarData(data);
    }
}
