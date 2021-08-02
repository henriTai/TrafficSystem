using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Car's AI uses this class to update its info of the closest obtacle ahead of it.
/// </summary>
public static class CarFrontCheck
{
    /// <summary>
    /// Monitors obstacles ahead on the car's route.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void CheckFront(CarDriveData data)
    {
        //RayScan(data);
        
        if (data.inIntersection || data.distanceToIntersection < 5f)
        {
            CheckFrontInIntersection(data);
        }
        else
        {
            CheckFrontOnStraightRoad(data);
        }
    }
    /// <summary>
    /// Sweep canning type raycast test of obstacles ahead on car's route. THIS FUNCTION IS CURRENTLY NOT IN USE BECAUSE THE
    /// OBSTACLE CHECK FUNCTION IT USES IS UNFINISHED.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    private static void RayScan(CarDriveData data)
    {
        RaycastHit hit;
        if (data.closestObstacle != null)
        {
            Vector3 dir = Vector3.zero;
            switch(data.previousHitIndex)
            {
                case 0:
                    dir = (data.closestObstacle.transform.position - data.carControl.frontMiddle.transform.position).normalized;
                    break;
                case 1:
                    dir = (data.closestObstacle.transform.position - data.carControl.leftFront.transform.position).normalized;
                    break;
                case 2:
                    dir = (data.closestObstacle.transform.position - data.carControl.rightFront.transform.position).normalized;
                    break;

            }
            if (Physics.Raycast(data.carControl.frontMiddle.transform.position, dir, out hit, CarDriveData.stoppingDistance))
            {
                Debug.DrawLine(data.carControl.frontMiddle.transform.position, hit.point, Color.yellow);
                if (hit.collider.gameObject == data.closestObstacle)
                {
                    data.previousHitDistance = hit.distance;
                }
                else
                {
                    data.closestObstacle = null;
                    data.previousHitDistance = float.MaxValue;
                }
            }
        }
        if (data.sweepingForth)
        {
            data.sweepTime += Time.deltaTime;
            if (data.sweepTime > data.sweepLength)
            {
                data.sweepTime = 2 * data.sweepLength - data.sweepTime;
                data.sweepingForth = false;
            }
        }
        else
        {
            data.sweepTime -= Time.deltaTime;
            if (data.sweepTime < 0f)
            {
                data.sweepTime = -data.sweepTime;
                data.sweepingForth = true;
            }
        }
        float yVal = (data.sweepTime / data.sweepLength - 0.5f) * data.sweepMaxAngle;

        Vector3 leftRayDirection = data.carControl.leftFront.transform.TransformDirection(Vector3.forward);
        leftRayDirection = (Quaternion.AngleAxis(yVal - 45f, Vector3.up) * leftRayDirection).normalized;
        Vector3 rightRayDirection = data.carControl.rightFront.transform.TransformDirection(Vector3.forward);
        rightRayDirection = (Quaternion.AngleAxis(45f - yVal, Vector3.up) * rightRayDirection).normalized;
        Vector3 middleRayDirection = data.carControl.frontMiddle.transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(data.carControl.leftFront.transform.position, leftRayDirection, out hit, CarDriveData.stoppingDistance))
        {
            if (hit.distance < data.previousHitDistance)
            {

                if ((data.closestObstacle == null || data.closestObstacle != hit.collider.gameObject)
                    && ObstacleIsOnPath(data, hit.collider.gameObject))
                {
                    data.closestObstacle = hit.collider.gameObject;
                    data.previousHitDistance = hit.distance;
                    data.previousHitIndex = 1;
                }
                Debug.DrawLine(data.carControl.leftFront.transform.position, hit.point, Color.blue);
            }
        }

        if (Physics.Raycast(data.carControl.rightFront.transform.position, rightRayDirection, out hit, CarDriveData.stoppingDistance))
        {
            if (hit.distance < data.previousHitDistance)
            {
                if (data.closestObstacle != hit.collider.gameObject && ObstacleIsOnPath(data, hit.collider.gameObject))
                {
                    data.closestObstacle = hit.collider.gameObject;
                    data.previousHitDistance = hit.distance;
                    data.previousHitIndex = 2;
                }
            }
            Debug.DrawLine(data.carControl.rightFront.transform.position, hit.point, Color.red);
        }

        if (Physics.Raycast(data.carControl.frontMiddle.transform.position, middleRayDirection, out hit, CarDriveData.stoppingDistance))
        {
            if (hit.distance < data.previousHitDistance)
            {
                if (data.closestObstacle != hit.collider.gameObject && ObstacleIsOnPath(data, hit.collider.gameObject))
                {
                    data.closestObstacle = hit.collider.gameObject;
                    data.previousHitDistance = hit.distance;
                    data.previousHitIndex = 0;
                }
            }
            Debug.DrawLine(data.carControl.frontMiddle.transform.position, hit.point, Color.green);
        }
        data.obstacleAhead = data.closestObstacle;
    }
    /// <summary>
    /// Checks if given object is an actual obstacle on car's path. THIS FUNCTION IS UNFINISHED AND IS NOT IN USE.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    /// <param name="otherObject">The gameobject of the possible obstacle.</param>
    /// <returns></returns>
    private static bool ObstacleIsOnPath(CarDriveData data, GameObject otherObject)
    {
        if (otherObject.tag == "car" || otherObject.tag == "ambulance" || otherObject.tag == "pedestrian")
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Checks obstacles ahead of the car on straight road (ie. not an intersection).
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    private static void CheckFrontOnStraightRoad(CarDriveData data)
    {
        GameObject other = null;
        RaycastHit hit;
        float hitDistance = float.MaxValue;

        Vector3 dir = data.forwardVector;

        // steering less than 5 degreese to either directions
        if (Mathf.Abs(data.steerAmount) < 5f)
        {
            if (Physics.Raycast(data.carControl.frontMiddle.transform.position, dir, out hit, CarDriveData.stoppingDistance))
            {
                other = hit.collider.gameObject;
                hitDistance = hit.distance;
            }
        }
        // steering amount is more
        else
        {
            // steering left
            if (data.steerAmount < 0f)
            {
                if (Physics.Raycast(data.carControl.leftFront.transform.position, dir, out hit, CarDriveData.stoppingDistance))
                {
                    other = hit.collider.gameObject;
                    hitDistance = hit.distance;
                }
                if (Physics.Raycast(data.carControl.frontMiddle.transform.position, dir, out hit, CarDriveData.stoppingDistance))
                {
                    if (hit.distance < hitDistance)
                    {
                        other = hit.collider.gameObject;
                        hitDistance = hit.distance;
                    }
                }
            }
            // steering right
            {
                if (Physics.Raycast(data.carControl.leftFront.transform.position, dir, out hit, CarDriveData.stoppingDistance))
                {
                    other = hit.collider.gameObject;
                    hitDistance = hit.distance;
                }
                if (Physics.Raycast(data.carControl.rightFront.transform.position, dir, out hit, CarDriveData.stoppingDistance))
                {
                    if (hit.distance < hitDistance)
                    {
                        other = hit.collider.gameObject;
                        hitDistance = hit.distance;
                    }
                }
            }
        }
        if (other == null)
        {
            data.obstacleAhead = false;
            return;
        }
        // ATM checks only cars, in future should also check pedestrians, ambulance... checking object tag
        // works then better
        CarDriveData otherDriver = other.GetComponent<CarDriveData>();
        if (otherDriver == null)
        {
            // checks only cars
            data.obstacleAhead = false;
            return;
        }
        // aknowledge only cars driving the same or a lane starting after this lane
        if (otherDriver.currentLane == data.currentLane || LaneChecks.InStartingLanes(data.nextLanes[0], otherDriver.currentLane))
        {
            data.previousHitDistance = hitDistance;
            data.obstacleAhead = true;
        }
        else
        {
            data.obstacleAhead = false;
        }
    }
    /// <summary>
    /// Checks obstacles ahead of the car in an intersection.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    private static void CheckFrontInIntersection(CarDriveData data)
    {
        GameObject other = null;
        RaycastHit hit;
        float hitDistance = float.MaxValue;
        bool obs = false;

        Vector3 dir = data.forwardVector;

        // car is just about to enter the intersection
        if (data.currentLane.nodesOnLane[0].ParentLane.laneType != LaneType.INTERSECTION_LANE_RIGHT_OF_WAY ||
            data.currentLane.nodesOnLane[0].ParentLane.laneType != LaneType.INTERSECTION_LANE_YIELDING)
        {
            if (Physics.Raycast(data.carControl.leftFront.transform.position, dir, out hit, CarDriveData.stoppingDistance))
            {
                other = hit.collider.gameObject;
                //CPUCarDrive otherDriver = other.GetComponent<CPUCarDrive>();
                CarAIMain otherDriver = other.GetComponent<CarAIMain>();
                if (otherDriver != null)
                {
                    if (data.currentLane == otherDriver.carData.currentLane || LaneChecks.InStartingLanes(data.nextLanes[0],
                        otherDriver.carData.currentLane))
                    {
                        data.obstacleAhead = true;
                        hitDistance = hit.distance;
                    }
                    else if (LaneChecks.IsInCrossingLanes(data.nextLanes[0], otherDriver.carData) &&
                        otherDriver.carData.intersectionYielding == false)
                    {
                        data.obstacleAhead = true;
                        hitDistance = data.distanceToIntersection;
                    }
                }
            }
            if (Physics.Raycast(data.carControl.frontMiddle.transform.position, dir, out hit, CarDriveData.stoppingDistance))
            {
                other = hit.collider.gameObject;
                CarAIMain otherDriver = other.GetComponent<CarAIMain>();
                //CPUCarDrive otherDriver = other.GetComponent<CPUCarDrive>();
                if (otherDriver != null)
                {
                    if (otherDriver.carData.currentLane == data.currentLane ||
                        LaneChecks.InStartingLanes(data.nextLanes[0], otherDriver.carData.currentLane))
                    {
                        if (hit.distance < hitDistance)
                        {
                            data.obstacleAhead = true;
                            hitDistance = hit.distance;
                        }
                    }
                    else if (LaneChecks.IsInCrossingLanes(data.nextLanes[0], otherDriver.carData)
                        && otherDriver.carData.intersectionYielding == false)
                    {
                        if (hit.distance < hitDistance)
                        {
                            data.obstacleAhead = true;
                            hitDistance = hit.distance;
                        }
                    }
                }
            }

            if (Physics.Raycast(data.carControl.rightFront.transform.position, dir, out hit, CarDriveData.stoppingDistance))
            {
                other = hit.collider.gameObject;
                CarAIMain otherDriver = other.GetComponent<CarAIMain>();
                //CPUCarDrive otherDriver = other.GetComponent<CPUCarDrive>();
                if (otherDriver != null)
                {
                    if (otherDriver.carData.currentLane == data.currentLane ||
                        LaneChecks.InStartingLanes(data.nextLanes[0], otherDriver.carData.currentLane))
                    {
                        if (hit.distance < hitDistance)
                        {
                            data.obstacleAhead = true;
                            hitDistance = hit.distance;
                        }
                    }
                    else if (otherDriver != data.intersectionYielding)
                    {
                        if (otherDriver.carData.currentLane.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
                        {
                            if (LaneChecks.IsInCrossingLanes(data.nextLanes[0], otherDriver.carData))
                            {
                                if (hit.distance < hitDistance)
                                {
                                    data.obstacleAhead = true;
                                    hitDistance = hit.distance;
                                }
                            }
                        }
                    }
                }
            }
        }
        // car is in intersection
        else
        {
            if (Physics.Raycast(data.carControl.frontMiddle.transform.position, dir, out hit, CarDriveData.stoppingDistance))
            {
                other = hit.collider.gameObject;
                CarAIMain otherDriver = other.GetComponent<CarAIMain>();
                //CPUCarDrive otherDriver = other.GetComponent<CPUCarDrive>();
                if (otherDriver != null)
                {
                    if (otherDriver.carData.currentLane == data.currentLane ||
                        LaneChecks.InStartingLanes(data.nextLanes[0], otherDriver.carData.currentLane)
                        || LaneChecks.InStartingLanes(data.currentLane, otherDriver.carData.currentLane))
                    {
                        obs = true;
                        hitDistance = hit.distance;
                    }
                    else if (otherDriver.carData.intersectionYielding == false &&
                        LaneChecks.IsInCrossingLanes(data.currentLane, otherDriver.carData))
                    {
                        obs = true;
                        hitDistance = hit.distance;
                    }
                }
            }
            if (Physics.Raycast(data.carControl.leftFront.transform.position, dir, out hit, CarDriveData.stoppingDistance))
            {
                other = hit.collider.gameObject;
                CarAIMain otherDriver = other.GetComponent<CarAIMain>();
                //CPUCarDrive otherDriver = other.GetComponent<CPUCarDrive>();
                if (otherDriver != null)
                {
                    if (otherDriver.carData.currentLane == data.currentLane ||
                        LaneChecks.InStartingLanes(data.nextLanes[0], otherDriver.carData.currentLane))
                    {
                        if (hit.distance < hitDistance)
                        {
                            obs = true;
                            hitDistance = hit.distance;
                        }
                    }
                    else if (otherDriver.carData.intersectionYielding == false
                        && LaneChecks.IsInCrossingLanes(data.currentLane, otherDriver.carData))
                    {
                        if (hit.distance < hitDistance)
                        {
                            obs = true;
                            hitDistance = hit.distance;
                        }
                    }
                }
            }
            if (Physics.Raycast(data.carControl.rightFront.transform.position, dir, out hit, CarDriveData.stoppingDistance))
            {
                other = hit.collider.gameObject;
                CarAIMain otherDriver = other.GetComponent<CarAIMain>();
                //CPUCarDrive otherDriver = other.GetComponent<CPUCarDrive>();
                if (otherDriver != null)
                {
                    if (otherDriver.carData.currentLane == data.currentLane || otherDriver.carData.nextLanes[0] == data.nextLanes[0]
                        || otherDriver.carData.currentLane == data.nextLanes[0])
                    {
                        if (hit.distance < hitDistance)
                        {
                            obs = true;
                            hitDistance = hit.distance;
                        }
                    }
                }
            }
        }
        if (obs)
        {
            data.obstacleAhead = true;
            data.previousHitDistance = hitDistance;
        }
        else
        {
            data.previousHitDistance = 0f;
            data.obstacleAhead = false;
        }
    }
}
