using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Author: Henri Tainio

/// <summary>
/// Traffic controller for intersections without traffic lights. This is a default traffic controller and is automatically
/// added to a created intersection.
/// </summary>
[Serializable]
public class ICNoLightsController : IICBase
{
    public Intersection intersection;
    public bool carSelectionChanged = true;
    public int[] carCounts = new int[] { 0, 0, 0, 0, 0 };

    /// <summary>
    /// Enum list of traffic controller's states based on yield groups.
    /// </summary>
    private enum State
    {
        ROWThruAndRightGo,
        ROWLeftGo,
        GVThruAndRightGo,
        GVLeftGo
    }

    /// <summary>
    /// This function is called when car selection changes.
    /// </summary>
    public override void CarSelectionChanged()
    {
        carSelectionChanged = false;
        ControlWithoutTrafficLights();
    }
    /// <summary>
    /// Ensures that target intersection in set.
    /// </summary>
    private void Awake()
    {
        if (intersection == null)
        {
            intersection = gameObject.GetComponent<Intersection>();
        }
    }
    /// <summary>
    /// Intersection's update function calls this function when this traffic controller type is active.
    /// </summary>
    /// <param name="dTime">Delta time value.</param>
    public override void UpdateActiveController(float dTime)
    {
        if (carSelectionChanged)
        {
            CarSelectionChanged();
        }
    }
    /// <summary>
    /// Controls the traffic flow in intesection. This is called every time the car selection in intersection changes and
    /// this controller type is active.
    /// </summary>
    private void ControlWithoutTrafficLights()
    {
        // No cars in intersection
        if (carCounts[0] == 0)
        {
            return;
        }

        List<Lane> newYielded = new List<Lane>();

        CarsOnLane[] crs = null;
        // right of way straight and right, all can go if within area
        if (carCounts[1] > 0)
        {
            crs = GetCarsOnLane(State.ROWThruAndRightGo);
            for (int i = 0; i < crs.Length; i++)
            {
                CarsOnLane col = crs[i];
                for (int j = 0; j < col.carsOnLane.Count; j++)
                {
                    CarInIntersection cii = col.carsOnLane[j];
                    if (cii.inIntersection == true)
                    {
                        IntersectionLaneInfo ili = intersection.laneDictionary[cii.driveData.intersectionLane];
                        for (int k = 0; k < ili.lanesGivingWay.Length; k++)
                        {

                            if (!newYielded.Contains(ili.lanesGivingWay[k].otherLane))
                            {
                                newYielded.Add(ili.lanesGivingWay[k].otherLane);
                            }
                        }
                    }
                    else
                    {
                        if (!newYielded.Contains(cii.driveData.intersectionLane))
                        {
                            if (cii.driveData.distanceToIntersection < 30f)
                            {
                                IntersectionLaneInfo ili = intersection.laneDictionary[cii.driveData.intersectionLane];
                                for (int k = 0; k < ili.lanesGivingWay.Length; k++)
                                {

                                    if (!newYielded.Contains(ili.lanesGivingWay[k].otherLane))
                                    {
                                        newYielded.Add(ili.lanesGivingWay[k].otherLane);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        //right of way, left turning - must check crossing lanes
        if (carCounts[2] > 0)
        {
            crs = GetCarsOnLane(State.ROWLeftGo);
            for (int i = 0; i < crs.Length; i++)
            {
                CarsOnLane col = crs[i];
                for (int j = 0; j < col.carsOnLane.Count; j++)
                {
                    CarInIntersection cii = col.carsOnLane[j];
                    if (cii.inIntersection == true)
                    {
                        IntersectionLaneInfo ili = intersection.laneDictionary[cii.driveData.intersectionLane];
                        for (int k = 0; k < ili.lanesGivingWay.Length; k++)
                        {
                            if (!newYielded.Contains(ili.lanesGivingWay[k].otherLane))
                            {
                                newYielded.Add(ili.lanesGivingWay[k].otherLane);
                            }
                        }
                    }
                    else
                    {
                        if (!newYielded.Contains(cii.driveData.intersectionLane))
                        {
                            if (cii.driveData.distanceToIntersection < 15f)
                            {
                                IntersectionLaneInfo ili = intersection.laneDictionary[cii.driveData.intersectionLane];
                                for (int k = 0; k < ili.lanesGivingWay.Length; k++)
                                {
                                    if (!newYielded.Contains(ili.lanesGivingWay[k].otherLane))
                                    {
                                        newYielded.Add(ili.lanesGivingWay[k].otherLane);
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
        // give way lanes, straight and right (no need to do if there is no cars on left turning gv-lanes)
        if (carCounts[3] > 0 && carCounts[4] > 0)
        {
            crs = GetCarsOnLane(State.GVThruAndRightGo);
            for (int i = 0; i < crs.Length; i++)
            {
                CarsOnLane col = crs[i];
                for (int j = 0; j < col.carsOnLane.Count; j++)
                {
                    CarInIntersection cii = col.carsOnLane[j];
                    if (cii.inIntersection == true)
                    {
                        IntersectionLaneInfo ili = intersection.laneDictionary[cii.driveData.intersectionLane];
                        for (int k = 0; k < ili.lanesGivingWay.Length; k++)
                        {
                            if (!newYielded.Contains(ili.lanesGivingWay[k].otherLane))
                            {
                                newYielded.Add(ili.lanesGivingWay[k].otherLane);
                            }
                        }
                    }
                    else
                    {
                        if (!newYielded.Contains(cii.driveData.intersectionLane))
                        {
                            if (cii.driveData.distanceToIntersection < 15f)
                            {
                                IntersectionLaneInfo ili = intersection.laneDictionary[cii.driveData.intersectionLane];
                                for (int k = 0; k < ili.lanesGivingWay.Length; k++)
                                {
                                    if (!newYielded.Contains(ili.lanesGivingWay[k].otherLane))
                                    {
                                        newYielded.Add(ili.lanesGivingWay[k].otherLane);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        // give way lanes turning left, check intersecting lanes
        if (carCounts[4] > 0)
        {
            crs = GetCarsOnLane(State.GVLeftGo);
            for (int i = 0; i < crs.Length; i++)
            {
                CarsOnLane col = crs[i];
                for (int j = 0; j < col.carsOnLane.Count; j++)
                {
                    CarInIntersection cii = col.carsOnLane[j];
                    if (cii.inIntersection == true)
                    {
                        IntersectionLaneInfo ili = intersection.laneDictionary[cii.driveData.intersectionLane];
                        for (int k = 0; k < ili.lanesGivingWay.Length; k++)
                        {
                            if (!newYielded.Contains(ili.lanesGivingWay[k].otherLane))
                            {
                                newYielded.Add(ili.lanesGivingWay[k].otherLane);
                            }
                        }
                    }
                    else
                    {
                        if (!newYielded.Contains(cii.driveData.intersectionLane))
                        {
                            if (cii.driveData.distanceToIntersection < 15f)
                            {
                                IntersectionLaneInfo ili = intersection.laneDictionary[cii.driveData.intersectionLane];
                                for (int k = 0; k < ili.lanesGivingWay.Length; k++)
                                {
                                    if (!newYielded.Contains(ili.lanesGivingWay[k].otherLane))
                                    {
                                        newYielded.Add(ili.lanesGivingWay[k].otherLane);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        UpdateCarYields(newYielded);
    }
    /// <summary>
    /// Returns information of cars on lane of selected yield group.
    /// </summary>
    /// <param name="status">Selected yield group.</param>
    /// <returns>Information of cars on lane.</returns>
    private CarsOnLane[] GetCarsOnLane(State status)
    {
        CarsOnLane[] crs = null;
        switch (status)
        {
            case State.ROWThruAndRightGo:
                crs = intersection.yieldGroups.ROWThruAndRight;
                break;
            case State.ROWLeftGo:
                crs = intersection.yieldGroups.ROWLeft;
                break;
            case State.GVThruAndRightGo:
                crs = intersection.yieldGroups.GVThruAndRight;
                break;
            case State.GVLeftGo:
                crs = intersection.yieldGroups.GVLeft;
                break;
        }
        return crs;
    }
    

    /// <summary>
    /// AI vehicle calls this function to inform traffic controller that it has entered the intersection.
    /// </summary>
    /// <param name="token">An identification token. Traffic controller gives this to an AI car when it informs it is
    /// approaching.</param>
    /// <param name="carObject">Reference to AI car's gameobject.</param>
    public override void CarInsideIntersection(int[] token, GameObject carObject)
    {
        //carSelectionChanged = true;
        CarsOnLane c = GetCarsOnLane(token);
        for (int i = 0; i < c.carsOnLane.Count; i++)
        {
            CarInIntersection cii = c.carsOnLane[i];
            if (cii.driveData.carObject == carObject)
            {
                cii.inIntersection = true;
                break;
            }
        }
        c.carsInside++;
        CalculateClosestIndex(c);
        carSelectionChanged = true;
    }
    /// <summary>
    /// Gets cars on lane group that the car with given ID token belongs to.
    /// </summary>
    /// <param name="token">An identification token.</param>
    /// <returns>Cars on lane of car with given ID token.</returns>
    private CarsOnLane GetCarsOnLane(int[] token)
    {
        CarsOnLane c = null;
        switch (token[0])
        {
            case 1:
                c = intersection.yieldGroups.ROWThruAndRight[token[1]];
                break;
            case 2:
                c = intersection.yieldGroups.ROWLeft[token[1]];
                break;
            case 3:
                c = intersection.yieldGroups.GVThruAndRight[token[1]];
                break;
            case 4:
                c = intersection.yieldGroups.GVLeft[token[1]];
                break;
        }
        return c;
    }
    /// <summary>
    /// Updates intersections yield status.
    /// </summary>
    /// <param name="yielded">A list of yielded lanes.</param>
    private void UpdateCarYields(List<Lane> yielded)
    {
        if (carCounts[1] > 0)
        {
            YieldGroup(yielded, State.ROWThruAndRightGo);
        }
        if (carCounts[2] > 0)
        {
            YieldGroup(yielded, State.ROWLeftGo);
        }
        if (carCounts[3] > 0)
        {
            YieldGroup(yielded, State.GVThruAndRightGo);
        }
        if (carCounts[4] > 0)
        {
            YieldGroup(yielded, State.GVLeftGo);
        }
    }
    /// <summary>
    /// Yields lanes on given list belonging to given yield status group.
    /// </summary>
    /// <param name="yielded">A list of yielded lanes.</param>
    /// <param name="status">Yield status group.</param>
    private void YieldGroup(List<Lane> yielded, State status)
    {
        CarsOnLane[] crs = GetCarsOnLane(status);
        for (int i = 0; i < crs.Length; i++)
        {
            CarsOnLane col = crs[i];
            for (int j = 0; j < col.carsOnLane.Count; j++)
            {
                CarInIntersection cii = col.carsOnLane[j];
                if (yielded.Contains(cii.driveData.intersectionLane))
                {
                    if (cii.driveData.inIntersection)
                    {
                        CarIntersectionUpdate.SetIntersectionYieding(cii.driveData, false);
                        //cii.carDrive.SetIntersectionYieding(false);
                    }
                    else
                    {
                        CarIntersectionUpdate.SetIntersectionYieding(cii.driveData, true);
                        //cii.carDrive.SetIntersectionYieding(true);
                    }
                }
                else
                {
                    CarIntersectionUpdate.SetIntersectionYieding(cii.driveData, false);
                    //cii.carDrive.SetIntersectionYieding(false);
                }
            }
        }
    }

    /// <summary>
    /// An AI car calls this function to check in to an intersection and receives an ID token.
    /// </summary>
    /// <param name="data">Cars data component.</param>
    /// <returns>An identification token.</returns>
    public override int[] CarCheckIn(CarDriveData data)
    {
        int[] carRetrievingIndex = new int[2];
        CarInIntersection c = new CarInIntersection(data);
        Lane lane = data.intersectionLane;
        Nodes start = lane.nodesOnLane[0];
        if (lane.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
        {
            if (lane.TurnDirection == IntersectionDirection.Left)
            {
                for (int i = 0; i < intersection.yieldGroups.ROWLeft.Length; i++)
                {
                    if (intersection.yieldGroups.ROWLeft[i].startNode == start)
                    {
                        intersection.yieldGroups.ROWLeft[i].carsOnLane.Add(c);
                        CalculateClosestIndex(intersection.yieldGroups.ROWLeft[i]);
                        carRetrievingIndex[0] = 2;
                        carRetrievingIndex[1] = i;
                        carCounts[2]++;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < intersection.yieldGroups.ROWThruAndRight.Length; i++)
                {
                    if (intersection.yieldGroups.ROWThruAndRight[i].startNode == start)
                    {
                        intersection.yieldGroups.ROWThruAndRight[i].carsOnLane.Add(c);
                        CalculateClosestIndex(intersection.yieldGroups.ROWThruAndRight[i]);
                        carRetrievingIndex[0] = 1;
                        carRetrievingIndex[1] = i;
                        carCounts[1]++;
                        break;
                    }
                }
            }
        }
        else
        {
            if (lane.TurnDirection == IntersectionDirection.Left)
            {
                for (int i = 0; i < intersection.yieldGroups.GVLeft.Length; i++)
                {
                    if (intersection.yieldGroups.GVLeft[i].startNode == start)
                    {
                        intersection.yieldGroups.GVLeft[i].carsOnLane.Add(c);
                        CalculateClosestIndex(intersection.yieldGroups.GVLeft[i]);
                        carRetrievingIndex[0] = 4;
                        carRetrievingIndex[1] = i;
                        carCounts[4]++;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < intersection.yieldGroups.GVThruAndRight.Length; i++)
                {
                    if (intersection.yieldGroups.GVThruAndRight[i].startNode == start)
                    {
                        intersection.yieldGroups.GVThruAndRight[i].carsOnLane.Add(c);
                        CalculateClosestIndex(intersection.yieldGroups.GVThruAndRight[i]);
                        carRetrievingIndex[0] = 3;
                        carRetrievingIndex[1] = i;
                        carCounts[3]++;
                        break;
                    }
                }
            }
        }

        carCounts[0]++;
        data.intersectionYielding = IsLaneYielding(lane);
        carSelectionChanged = true;
        return carRetrievingIndex;
    }
    /// <summary>
    /// Returns information if given lane is currently yielding.
    /// </summary>
    /// <param name="lane">Lane.</param>
    /// <returns>Is lane yielding?</returns>
    private bool IsLaneYielding(Lane lane)
    {
        if (lane.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
        {
            if (lane.TurnDirection == IntersectionDirection.Straight || lane.TurnDirection == IntersectionDirection.Right)
            {
                return false;
            }
            else
            {
                return IsYieldedByGroup(ref intersection.yieldGroups.ROWThruAndRight);
            }
        }
        else
        {
            if (lane.TurnDirection == IntersectionDirection.Straight || lane.TurnDirection == IntersectionDirection.Right)
            {
                if (IsYieldedByGroup(ref intersection.yieldGroups.ROWThruAndRight))
                {
                    return true;
                }
                if (IsYieldedByGroup(ref intersection.yieldGroups.ROWLeft))
                {
                    return true;
                }
            }
            else
            {
                if (IsYieldedByGroup(ref intersection.yieldGroups.ROWThruAndRight))
                {
                    return true;
                }
                if (IsYieldedByGroup(ref intersection.yieldGroups.ROWLeft))
                {
                    return true;
                }
                if (IsYieldedByGroup(ref intersection.yieldGroups.GVThruAndRight))
                {
                    return true;
                }
            }
        }
        return false;
        
    }
    /// <summary>
    /// Checks if there are cars of certain yield group currently in intersection.
    /// </summary>
    /// <param name="group">Selected yield group.</param>
    /// <returns>Is there cars of given yield group in intersection.</returns>
    private bool IsYieldedByGroup(ref CarsOnLane[] group)
    {
        if (group.Length > 0)
        {
            for (int i = 0; i < group.Length; i++)
            {
                CarsOnLane col = group[i];
                // If there cars on these other lanes...
                if (col.carsOnLane.Count > 0)
                {
                    // 1st check: if car is closer than 10f from intersection return true
                    if (col.closestIndex >= 0)
                    {
                        if (col.carsOnLane[col.closestIndex].driveData.distanceToIntersection < 10f)
                        {
                            return true;
                        }
                    }
                    // 2nd check: if car is already inside the intersection, return true
                    for (int j = 0; j < col.carsOnLane.Count; j++)
                    {
                        CarInIntersection c = col.carsOnLane[j];
                        if (c.inIntersection == true)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    /// <summary>
    /// AI car calls this function to check out when it exits the intersection.
    /// </summary>
    /// <param name="carObject">Car's gameobject.</param>
    /// <param name="token">Car's identification token.</param>
    public override void CarCheckOut(GameObject carObject, int[] token)
    {
        carCounts[0]--;
        carCounts[token[0]]--;

        CarsOnLane c = GetCarsOnLane(token);
        RemoveCarFromIntersection(c, carObject);
        carSelectionChanged = true;
    }

}
