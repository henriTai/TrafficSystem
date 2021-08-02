using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Author: Henri Tainio

/// <summary>
/// A navigation data utility that contains precalculated shortest routes from each location to every other location.
/// </summary>
[Serializable]
public class RouteUtility : MonoBehaviour
{
    /// <summary>
    /// An array of all roads, works as an index.
    /// </summary>
    public Road[] allRoads;
    /// <summary>
    /// An array of all lanes, works as an index.
    /// </summary>
    public Lane[] allLanes;
    /// <summary>
    /// An array of lengths of all lanes.
    /// </summary>
    public float[] laneLengths;
    /// <summary>
    /// An array of length of all roads. When searching data of certain road, roads index in roadInfos is the same as in allRoads. 
    /// </summary>
    public TrackerRoadInfo[] roadInfos;
    /// <summary>
    /// An array of length of all lanes.
    /// </summary>
    public TrackerLaneInfo[] laneInfos;
    public int depth = 0;
}
/// <summary>
/// ROUTE UTILITY's data class. TrackerRoadInfo contains index references to all lanes of one road.
/// </summary>
[Serializable]
public class TrackerRoadInfo
{
    /// <summary>
    /// An array, length of all lanes on road, contains index references to these lanes. Object references can be found in
    /// Route Utility's allLanes-array.
    /// </summary>
    public int[] laneInfoIndexes;
}
/// <summary>
/// ROUTE UTILITY's data class. TrackerLane info contains data of shortest routes to every other location.
/// </summary>
[Serializable]
public class TrackerLaneInfo
{
    /// <summary>
    /// Index of lane's parent road. Object reference can be found in RouteUtility's array of all roads.
    /// </summary>
    public int roadIndex;
    /// <summary>
    /// Index of this lane. Object reference can be found in RouteUtility's array of all lanes.
    /// </summary>
    public int laneIndex;
    /// <summary>
    /// An array, length of RouteUtility's array of all lanes. Contains index references of which lane should be selected in order
    /// to get to target lane.
    /// </summary>
    public int[] nextLaneIndexes;
    /// <summary>
    /// Contains information if next lane to destination using shortest way is following lane or a lane change lane: 0 = following,
    /// 1 = change to left, 2 = change to right.
    /// </summary>
    public int[] followingOrLaneChange;
    /// <summary>
    /// Count of lanes to travel to get to target locations in array.
    /// </summary>
    public int[] laneCountToDestination;
    /// <summary>
    /// Shortest distances to target locations using the shortest route in array.
    /// </summary>
    public float[] distancesToDestination;
}
