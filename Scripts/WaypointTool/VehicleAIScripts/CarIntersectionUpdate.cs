using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Car's AI uses this class to monitor closing in to an intersection and traffic controller uses this class to set the car's
/// intersection yielding status.
/// </summary>
public static class CarIntersectionUpdate
{
    /// <summary>
    /// Monitors car's closing in to an intersection and manages car's checking in to the intersection's traffic controller. Car's
    /// checkin out is managed from CarRouteUpdate.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void CheckUpcomingIntersection(CarDriveData data)
    {
        if (data.inIntersection && data.checkedIn)
        {
            return;
        }
        if (data.intersection == null)
        {
            // find next intersection
            for (int i = 0; i < CarDriveData.nextLanesCount; i++)
            {
                Lane l = data.nextLanes[i];
                Nodes n = l.nodesOnLane[0];
                if (n.ParentLane.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY ||
                    n.ParentLane.laneType == LaneType.INTERSECTION_LANE_YIELDING)
                {
                    data.intersectionLane = l;
                    data.intersection = l.transform.parent.GetComponent<Intersection>();
                    data.intersectionStartPos = l.GetStartPosition();

                    data.distanceToIntersection = Vector2.Distance(data.intersectionStartPos,
                        new Vector2(data.carObject.transform.position.x, data.carObject.transform.position.z));
                    break;
                }
            }
        }
        else
        {
            Vector2 pos = new Vector2(data.carObject.transform.position.x, data.carObject.transform.position.z);
            data.distanceToIntersection = Vector2.Distance(data.intersectionStartPos, pos);

            if (data.checkedIn == false)
            {
                // monitor distance and check in when close enough
                if (data.distanceToIntersection <= CarDriveData.distanceToCheckIn)
                {
                    data.parkingTicket = data.intersection.currentController.CarCheckIn(data);
                    //parkingTicket = ic.CarCheckIn(gameObject, this, intersectionLane, out intersectionYielding);
                    data.checkedIn = true;
                }
            }
            else if (data.inIntersection == false)
            {
                if (data.distanceToIntersection < 3f)
                {
                    data.inIntersection = true;
                    data.intersection.currentController.CarInsideIntersection(data.parkingTicket, data.carObject);
                }
            }
        }
    }
    /// <summary>
    /// Intersection's traffic controller uses this function to set the car's intersection yielding status.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    /// <param name="status">Is car yielding?</param>
    public static void SetIntersectionYieding(CarDriveData data, bool status)
    {
        data.intersectionYielding = status;
    }
}
