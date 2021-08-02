using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

// Author: Hneri Tainio

/// <summary>
/// Menu item tools for creation a route utility (a sort of navigation data).
/// </summary>
public class RouteUtilityEditor : MonoBehaviour
{
    /// <summary>
    /// Creates a new or resets existing route utility.
    /// </summary>
    [MenuItem("Virtulanssi/RouteUtility/CreateRouteUtility")]
    public static void CreateUtility()
    {
        RouteUtility routeUtility = FindObjectOfType<RouteUtility>();
        if (routeUtility == null)
        {
            GameObject g = new GameObject("RouteUtility");
            g.AddComponent<RouteUtility>();
        }
        ResetUtility();
    }
    /// <summary>
    /// Resets existing or creates a new route utility.
    /// </summary>
    [MenuItem("Virtulanssi/RouteUtility/ResetUtility")]
    public static void ResetUtility()
    {
        LaneSorter laneSorter = new LaneSorter();
        RoadSorter roadSorter = new RoadSorter();
        RouteUtility routeUtility = FindObjectOfType<RouteUtility>();
        if (routeUtility==null)
        {
            CreateUtility();
            return;
        }
        routeUtility.depth = 0;
        routeUtility.allRoads = FindObjectsOfType<Road>();
        routeUtility.allLanes = FindObjectsOfType<Lane>();
        Array.Sort(routeUtility.allLanes, laneSorter);
        Array.Sort(routeUtility.allRoads, roadSorter);
        routeUtility.roadInfos = new TrackerRoadInfo[routeUtility.allRoads.Length];
        routeUtility.laneInfos = new TrackerLaneInfo[routeUtility.allLanes.Length];
        routeUtility.laneLengths = new float[routeUtility.allLanes.Length];

        for (int i = 0; i < routeUtility.laneInfos.Length; i++)
        {
            routeUtility.laneInfos[i] = new TrackerLaneInfo();
            routeUtility.laneInfos[i].laneIndex = i;
            Lane l = routeUtility.allLanes[i];
            Road r = l.transform.parent.GetComponent<Road>();
            int roadInd = 0;
            for (int j = 0; j < routeUtility.allRoads.Length; j++)
            {
                if (routeUtility.allRoads[j] == r)
                {
                    roadInd = j;
                    break;
                }
            }
            routeUtility.laneInfos[i].roadIndex = roadInd;
            routeUtility.laneInfos[i].nextLaneIndexes = new int[routeUtility.allLanes.Length];
            routeUtility.laneInfos[i].laneCountToDestination = new int[routeUtility.allLanes.Length];
            routeUtility.laneInfos[i].distancesToDestination = new float[routeUtility.allLanes.Length];
            routeUtility.laneInfos[i].followingOrLaneChange = new int[routeUtility.allLanes.Length];

            for (int j = 0; j < routeUtility.laneInfos[i].nextLaneIndexes.Length; j++)
            {
                if (j == i)
                {
                    routeUtility.laneInfos[i].nextLaneIndexes[j] = i;
                    routeUtility.laneInfos[i].laneCountToDestination[j] = 0;
                    routeUtility.laneInfos[i].distancesToDestination[j] = 0f;
                    routeUtility.laneInfos[i].followingOrLaneChange[j] = 0;
                }
                else
                {
                    routeUtility.laneInfos[i].nextLaneIndexes[j] = -1;
                    routeUtility.laneInfos[i].laneCountToDestination[j] = -1;
                    routeUtility.laneInfos[i].distancesToDestination[j] = float.MaxValue;
                    routeUtility.laneInfos[i].followingOrLaneChange[j] = -1;
                }
            }
            float laneLength = 0f;
            for (int j = 0; j < l.nodesOnLane.Length - 1; j++)
            {
                laneLength += Vector3.Distance(l.nodesOnLane[j].transform.position, l.nodesOnLane[j + 1].transform.position);
            }
            routeUtility.laneLengths[i] = laneLength;
        }
        routeUtility.roadInfos = new TrackerRoadInfo[routeUtility.allRoads.Length];
        for (int i = 0; i < routeUtility.allRoads.Length; i++)
        {
            List<TrackerLaneInfo> tls = new List<TrackerLaneInfo>();
            for (int j = 0; j < routeUtility.laneInfos.Length; j++)
            {
                TrackerLaneInfo tli = routeUtility.laneInfos[j];
                if (tli.roadIndex == i)
                {
                    tls.Add(tli);
                }
            }
            routeUtility.roadInfos[i] = new TrackerRoadInfo();
            routeUtility.roadInfos[i].laneInfoIndexes = new int[tls.Count];
            for (int j = 0; j < tls.Count; j++)
            {
                routeUtility.roadInfos[i].laneInfoIndexes[j] = tls[j].laneIndex;
            }
        }
        EditorUtility.SetDirty(routeUtility);
    }
}
/// <summary>
/// A comparator class for lanes.
/// </summary>
public class LaneSorter : IComparer<Lane>
{
    /// <summary>
    /// A comparator implementation.
    /// </summary>
    /// <param name="x">Lane 1.</param>
    /// <param name="y">Lane 2.</param>
    /// <returns>Returns comparison result [int].</returns>
    int IComparer<Lane>.Compare(Lane x, Lane y)
    {

        return string.Compare(x.name, y.name);
    }
}
/// <summary>
/// A comparator class for roads.
/// </summary>
public class RoadSorter : IComparer<Road>
{
    /// <summary>
    /// A comparator implementation.
    /// </summary>
    /// <param name="x">Road 1.</param>
    /// <param name="y">Road 2.</param>
    /// <returns>Returns comparison result [int].</returns>
    int IComparer<Road>.Compare(Road x, Road y)
    {

        return string.Compare(x.name, y.name);
    }
}
