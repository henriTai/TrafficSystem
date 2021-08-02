using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Author: Henri Tainio

/// <summary>
/// In node network hierarchy, Lane is between Nodes and Roads. Word 'lane' in this context is ambiguous, as on straight roads
/// its meaning is straightforward but in intersections it rather means driving line. In short, here 'lane' means a route a car
/// can drive.
/// </summary>
[Serializable]
public class Lane : MonoBehaviour
{
    /// <summary>
    /// Type of this lane
    /// </summary>
    public LaneType laneType;
    /// <summary>
    /// Amount of traffic on this lane.
    /// </summary>
    [SerializeField]
    private TrafficSize trafficSize;

    /// <summary>
    /// Speed limit of this lane.
    /// </summary>
    [SerializeField]
    private SpeedLimits speedLimit;
    /// <summary>
    /// Turn direction of this lane. This is used in intersection to determine driving order.
    /// </summary>
    [SerializeField]
    private IntersectionDirection turnDirection;
    /// <summary>
    /// An ordered array of linked nodes that this lane consists of.
    /// </summary>
    [SerializeField]
    public Nodes[] nodesOnLane;
    /// <summary>
    /// An ordered array of crosswalks along this lanes path.
    /// </summary>
    [SerializeField]
    public CrosswalkEncounter[] crosswalkEncounters;
    /// <summary>
    /// A boolean if this lane is drawn in sceneview.
    /// </summary>
    public bool pointToPointLine;
    /// <summary>
    /// A boolean if other lanes of this road are visualized also in sceneview.
    /// </summary>
    public bool drawAllLanes;
    /// <summary>
    /// A boolean if this a right-side lane.
    /// </summary>
    public bool isRightLane;
    /// <summary>
    /// Array of data about other lanes crossing this lane.
    /// </summary>
    [SerializeField]
    private LaneCrossingPoint[] crossingLanes;
    /// <summary>
    /// XZ-coordinates of this lane's starting position.
    /// </summary>
    [SerializeField]
    public Vector2 startPosition;

    /// <summary>
    /// Getter/setter of this lane's traffic amount.
    /// </summary>
    public TrafficSize Traffic
    {
        get
        {
            return trafficSize;
        }
        set
        {
            trafficSize = value;
        }
    }

    /// <summary>
    /// Getter/setter of this lane's speed limit.
    /// </summary>
    public SpeedLimits SpeedLimit
    {
        get
        {
            return speedLimit;
        }
        set
        {
            speedLimit = value;
        }
    }
    /// <summary>
    /// Getter/setter of this lane's lane crossing data array.
    /// </summary>
    public LaneCrossingPoint[] CrossingLanes
    {
        get
        {
            return crossingLanes;
        }
        set
        {
            crossingLanes = value;
        }
    }

    /// <summary>
    /// Getter/setter of this lane's turn direction.
    /// </summary>
    public IntersectionDirection TurnDirection
    {
        get
        {
            return turnDirection;
        }
        set
        {
            turnDirection = value;
        }
    }
    /// <summary>
    /// Returns XZ-coordinates of this lane's first node.
    /// </summary>
    /// <returns>XZ-coordinates.</returns>
    public Vector2 GetStartPosition()
    {
        if (startPosition == Vector2.zero)
        {
            return new Vector2(nodesOnLane[0].transform.position.x, nodesOnLane[0].transform.position.z);
        }
        else
        {
            return startPosition;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Adds a new lane crossing to this lane's lane crossing data array.
    /// </summary>
    /// <param name="crossing">Added LaneCrossingPoint entry.</param>
    public void AddCrossingLane(LaneCrossingPoint crossing)
    {
        if (crossingLanes == null || crossingLanes.Length == 0)
        {
            crossingLanes = new LaneCrossingPoint[1];
            crossingLanes[0] = crossing;
        }
        else
        {
            Array.Resize(ref crossingLanes, crossingLanes.Length + 1);
            crossingLanes[crossingLanes.Length - 1] = crossing;
        }
    }
#endif
    /// <summary>
    /// Unity's built-in reset function. Sets default values when component is reset.
    /// </summary>
    private void Reset()
    {
        Traffic = TrafficSize.Average;
        SpeedLimit = SpeedLimits.KMH_40;
        TurnDirection = IntersectionDirection.Straight;
        nodesOnLane = new Nodes[0];
        crosswalkEncounters = null;
        Debug.Log("Reset");
        pointToPointLine = true;
        drawAllLanes = true;

    }
}
/// <summary>
/// A helper data container class used by Lane-class. Contains information about a lane crossing another lane.
/// </summary>
[Serializable]
public class LaneCrossingPoint
{
    /// <summary>
    /// XZ-coordinates of point where lanes cross each other.
    /// </summary>
    public Vector2 crossingPoint;
    /// <summary>
    /// The other lane that this lane crosses.
    /// </summary>
    public Lane otherLane;
}
