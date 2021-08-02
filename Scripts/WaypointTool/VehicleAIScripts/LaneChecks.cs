using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// A helper class, includes some commonplace lane comparison operations that are used when analysing obstacles.
/// </summary>
public static class LaneChecks
{
    /// <summary>
    /// Compares if two lanes have same starting node.
    /// </summary>
    /// <param name="current">Lane 1.</param>
    /// <param name="laneToCompare">Lane 2.</param>
    /// <returns>Do these lanes start from the same node?</returns>
    public static bool InStartingLanes(Lane current, Lane laneToCompare)
    {
        bool inLanes = false;
        Nodes n = current.nodesOnLane[0];
        for (int i = 0; i < n.StartingLanes.Length; i++)
        {
            if (n.StartingLanes[i] == laneToCompare)
            {
                inLanes = true;
                break;
            }
        }
        return inLanes;
    }
    /// <summary>
    /// Compares if given lane crosses with other car's current lane.
    /// </summary>
    /// <param name="l">Checked lane.</param>
    /// <param name="otherCar">Other car's data.</param>
    /// <returns></returns>
    public static bool IsInCrossingLanes(Lane l, CarDriveData otherCar)
    {
        if (otherCar.inIntersection == false)
        {
            return false;
        }
        if (l.CrossingLanes.Length == 0)
        {
            return false;
        }
        for (int i = 0; i < l.CrossingLanes.Length; i++)
        {
            if (l.CrossingLanes[i].otherLane == otherCar.currentLane)
            {
                return true;
            }
        }
        return false;
    }
}
