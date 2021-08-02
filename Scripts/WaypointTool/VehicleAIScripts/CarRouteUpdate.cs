using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Car's AI uses this class to update car's route.
/// </summary>
public static class CarRouteUpdate
{
    /// <summary>
    /// Updates car's route, returns true when car is about to change a lane. 
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    /// <returns>Returns true if the next node is a lane starting node.</returns>
    public static bool UpdateNode(CarDriveData data)
    {
        // this is saved for checking out from intersection
        Lane prev = data.previousNode.ParentLane;
        bool onNewLane = false;

        if (NodePassed(data))
        {
            data.previousNode = data.nextNode;
            data.nextNode = data.oneAfterNextNode;

            //check moving in / out of intersection
            if (data.previousNode.LaneStartNode == true)
            {
                if (data.previousNode.ParentLane.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY ||
                    data.previousNode.ParentLane.laneType == LaneType.INTERSECTION_LANE_YIELDING)
                {
                    // Moved this to CarIntersectionUpdate!
                    //data.inIntersection = true;
                    //data.intersection.currentController.CarInsideIntersection(data.parkingTicket, data.carObject);
                }
                else
                {
                    if (data.checkedIn)
                    {
                        IntersectionCheckOut(data);
                    }
                }
                UpdateCrosswalks(data);
            }

            if (data.nextNode.LaneStartNode == true)
            {
                onNewLane = true;
                // This is because the first node may be shared between multiple lanes, direction info is derived
                // from the second node
                data.oneAfterNextNode = data.nextLanes[0].nodesOnLane[1];
                data.currentLane = data.nextLanes[0];

                data.speedLimit = KmsToMs.Convert(data.currentLane.SpeedLimit);

                for (int i = 1; i < CarDriveData.nextLanesCount; i++)
                {
                    data.nextLanes[i - 1] = data.nextLanes[i];
                }
                Lane l = data.nextLanes[CarDriveData.nextLanesCount - 1];
                data.nextLanes[CarDriveData.nextLanesCount - 1] = NextRandomLane(l.nodesOnLane[l.nodesOnLane.Length - 1].OutNode);
            }
            else
            {
                data.oneAfterNextNode = data.nextNode.OutNode;
            }
            data.nextNodePos = data.oneAfterNextNodePos;
            data.oneAfterNextNodePos = data.oneAfterNextNode.transform.position;

        }
        return onNewLane;
    }
    /// <summary>
    /// Randomly selects a lane from lanes starting from the given node.
    /// </summary>
    /// <param name="n">Starting lane is randomly selected from lanes starting from this node.</param>
    /// <returns>Randomly selected lane starting from this node.</returns>
    public static Lane NextRandomLane(Nodes n)
    {
        if (n.StartingLanes.Length == 0)
        {
            Debug.Log(n.name);
        }
        int index = Random.Range(0, n.StartingLanes.Length);
        return n.StartingLanes[index];
    }
    /// <summary>
    /// Manages car's checkin out from an intersection. Checking in is managed from CarIntersectionUpdate.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void IntersectionCheckOut(CarDriveData data)
    {
        data.intersection.currentController.CarCheckOut(data.carObject, data.parkingTicket);
        data.checkedIn = false;
        data.inIntersection = false;
        data.intersectionLane = null;
        data.intersection = null;
    }

    /// <summary>
    /// Updates car's list of upcoming crosswalks.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    private static void UpdateCrosswalks(CarDriveData data)
    {
        List<CrosswalkEncounter> encs = new List<CrosswalkEncounter>();
        CrosswalkEncounter[] es = data.currentLane.crosswalkEncounters;
        CrosswalkEncounter[] nextEs = data.nextLanes[0].crosswalkEncounters;
        CrosswalkEncounter[] afterNextEs = data.nextLanes[1].crosswalkEncounters;
        int arrayLength = 0;
        if (es != null)
        {
            arrayLength += es.Length;
        }
        if (nextEs != null)
        {
            arrayLength += nextEs.Length;
        }
        if (afterNextEs != null)
        {
            arrayLength += afterNextEs.Length;
        }
        if (arrayLength == 0)
        {
            data.nextCrosswalks = null;
            data.crosswalkIndex = -1;
        }
        else
        {
            int index = 0;
            data.crosswalkIndex = 0;
            data.nextCrosswalks = new CrosswalkEncounter[arrayLength];
            bool checkIfPassed = true;
            int checkFrom = 0;
            int startIndex = 0;
            if (es != null)
            {
                for (int i = 0; i < es.Length; i++)
                {
                    if (checkIfPassed)
                    {
                        Nodes n = es[i].crosswalk.beforeNodes[es[i].index];
                        for (int j = checkFrom; j < data.currentLane.nodesOnLane.Length; j++)
                        {
                            if (data.currentLane.nodesOnLane[j] == data.previousNode)
                            {
                                checkIfPassed = false;
                                break;
                            }
                            if (data.currentLane.nodesOnLane[j] == n)
                            {
                                checkFrom = j + 1;
                                startIndex = i + 1;
                                break;
                            }

                        }
                    }
                    data.nextCrosswalks[index] = es[i];
                    index++;
                }
            }
            if (nextEs != null)
            {
                for (int i = 0; i < nextEs.Length; i++)
                {
                    data.nextCrosswalks[index] = nextEs[index];
                    index++;
                }
            }
            if (afterNextEs != null)
            {
                for (int i = 0; i < afterNextEs.Length; i++)
                {
                    data.nextCrosswalks[index] = afterNextEs[i];
                    index++;
                }
            }
            if (startIndex > index - 1)
            {
                data.crosswalkIndex = -1;
                data.nextCrosswalk = null;
            }
            else
            {
                data.crosswalkIndex = startIndex;
                data.nextCrosswalk = data.nextCrosswalks[data.crosswalkIndex].crosswalk;
                data.nextCrossingPoint = data.nextCrosswalk.crossingPoints[data.nextCrosswalks[data.crosswalkIndex].index];
            }
        }
    }

    /// <summary>
    /// Checks if car has passed the next node.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    /// <returns>Returns true if the car has already passed the next node.</returns>
    private static bool NodePassed(CarDriveData data)
    {
        bool found = false;
        Vector2 carPos = new Vector2(data.carPosition.x, data.carPosition.z);
        Vector2 inverseDir = new Vector2(data.forwardVector.z, -data.forwardVector.x).normalized;
        Vector2 A1 = new Vector2(data.nextNodePos.x, data.nextNodePos.z);
        Vector2 A2 = new Vector2(data.oneAfterNextNodePos.x, data.oneAfterNextNodePos.z);
        Vector2 B1 = carPos - 5f * inverseDir;
        Vector2 B2 = carPos + 5f * inverseDir;
        Vector2 intersectionPoint = GetIntersectionPointCoordinates(A1, A2, B1, B2, out found);
        if (found)
        {
            return true;
        }
        else
        {
            if (Vector2.Distance(carPos, A1) < Vector2.Distance(carPos, A2))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    /// <summary>
    /// Calculates if car is driving between two points. The calculation is done testing if a transversal line from the left side
    /// of the car to its right side crosses a line from previous node position to the next position. 'NodePassed' function uses
    /// this method to test if car is between the next node and the one after the next node.
    /// </summary>
    /// <param name="A1">Vector2 position of the first node's position.</param>
    /// <param name="A2">Vector2 position of the second node's position.</param>
    /// <param name="B1">Vector2 position of the travelsal line's left end point.</param>
    /// <param name="B2">Vector2 position of the travelsal line's right end point.</param>
    /// <param name="found">Returns true if line A1-A2 crosses line B1-B2.</param>
    /// <returns>Returns Vector2 position where line A1-A2 and line B1-B2 cross each others.</returns>
    public static Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
        if (tmp == 0)
        {
            found = false;
            return Vector2.zero;
        }
        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        Vector2 point = new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
            );
        if (point.x > A1.x && point.x > A2.x)
        {
            found = false;
            return Vector2.zero;
        }
        if (point.x > B1.x && point.x > B2.x)
        {
            found = false;
            return Vector2.zero;
        }
        if (point.x < A1.x && point.x < A2.x)
        {
            found = false;
            return Vector2.zero;
        }
        if (point.x < B1.x && point.x < B2.x)
        {
            found = false;
            return Vector2.zero;
        }
        if (point.y > A1.y && point.y > A2.y)
        {
            found = false;
            return Vector2.zero;
        }
        if (point.y > B1.y && point.y > B2.y)
        {
            found = false;
            return Vector2.zero;
        }
        if (point.y < A1.y && point.y < A2.y)
        {
            found = false;
            return Vector2.zero;
        }
        if (point.y < B1.y && point.y < B2.y)
        {
            found = false;
            return Vector2.zero;
        }
        found = true;
        return point;
    }
}
