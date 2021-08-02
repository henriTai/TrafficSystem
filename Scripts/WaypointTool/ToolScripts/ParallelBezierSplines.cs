using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/* Author: Henri Tainio
 * Source for Bezier content: Catlike coding
 * https://catlikecoding.com/unity/tutorials/curves-and-splines/
 */

/// <summary>
/// Waypoint tool use this for creating roads, its lanes and nodes for each lane.
/// Add this script as a component to an empty gameobject.
/// The tool functionality is in custom inspector script.
/// For bezier's this scripts start points was a bezier tutorial at
/// https://catlikecoding.com/unity/tutorials/curves-and-splines/
/// </summary>
[Serializable]
public class ParallelBezierSplines : MonoBehaviour
{
    /// <summary>
    /// When component is added or reseted, this bool is false and initialization is executed.
    /// </summary>
    [SerializeField]
    private bool initialized;
    /// <summary>
    /// The first phaze of creating a road is to set lanes. Custom inspector uses this bool to save the curret state.
    /// </summary>
    [SerializeField]
    private bool lanesSet;
    /// <summary>
    /// The second phaze of creating a road is to set nodes. Custom inspector uses this bool to save the current state.
    /// When nodes are set, road can be created.
    /// </summary>
    [SerializeField]
    private bool nodesSet;

    /// <summary>
    /// Array of positions of guiding line for road's direction.
    /// </summary>
    [SerializeField]
    private Vector3[] points;
    /// <summary>
    /// Array of lane positions for left side (opposite direction) lane 1.
    /// </summary>
    [SerializeField]
    private Vector3[] leftLanePoints1;
    /// <summary>
    /// Array of lane positions for left side (opposite direction) lane 2.
    /// </summary>
    [SerializeField]
    private Vector3[] leftLanePoints2;
    /// <summary>
    /// Array of lane positions for left side (opposite direction) lane 3.
    /// </summary>
    [SerializeField]
    private Vector3[] leftLanePoints3;
    /// <summary>
    /// Array of lane positions for right side lane 1.
    /// </summary>
    [SerializeField]
    private Vector3[] rightLanePoints1;
    /// <summary>
    /// Array of lane positions for right side lane 2.
    /// </summary>
    [SerializeField]
    private Vector3[] rightLanePoints2;
    /// <summary>
    /// Array of lane positions for right side lane 3.
    /// </summary>
    [SerializeField]
    private Vector3[] rightLanePoints3;

    /// <summary>
    /// Array of lengths of each road segment.
    /// </summary>
    [SerializeField]
    private float[] segmentLengths;
    /// <summary>
    /// Number of nodes positioned for each road segment. 
    /// </summary>
    [SerializeField]
    private int[] nodesOnSegment;
    /// <summary>
    /// Overall node count.
    /// </summary>
    [SerializeField]
    private int nodeCount;
    /// <summary>
    /// An array of segment lengths of left side (opposite direction) lane 1.
    /// </summary>
    [SerializeField]
    private float[] leftSegmentLengths1;
    /// <summary>
    /// An array of segment lengths of left side (opposite direction) lane 2.
    /// </summary>
    [SerializeField]
    private float[] leftSegmentLengths2;
    /// <summary>
    /// An array of segment lengths of left side (opposite direction) lane 3.
    /// </summary>
    [SerializeField]
    private float[] leftSegmentLengths3;
    /// <summary>
    /// An array of segment lengths of right side lane 1.
    /// </summary>
    [SerializeField]
    private float[] rightSegmentLengths1;
    /// <summary>
    /// An array of segment lengths of right side lane 2.
    /// </summary>
    [SerializeField]
    private float[] rightSegmentLengths2;
    /// <summary>
    /// An array of segment lengths of right side lane 3.
    /// </summary>
    [SerializeField]
    private float[] rightSegmentLengths3;
    /// <summary>
    /// Overall length of guiding line spline.
    /// </summary>
    [SerializeField]
    private float splineLength;
    /// <summary>
    /// Lengths of left side (opposite direction) lane splines.
    /// </summary>
    [SerializeField]
    private float[] leftSplineLengths;
    /// <summary>
    /// Lengths of right side lane splines.
    /// </summary>
    [SerializeField]
    private float[] rightSplineLengths;
    
    /// <summary>
    /// Spacings of each node on left lane 1. The spacing value is the distance from node to guiding line.
    /// </summary>
    [SerializeField]
    private float[] leftSpacings1;
    /// <summary>
    /// Spacings of each node on left lane 2. The spacing value is the distance from node to node on left lane 1.
    /// </summary>
    [SerializeField]
    private float[] leftSpacings2;
    /// <summary>
    /// Spacings of each node on left lane 3. The spacing value is the distance from node to node on left lane 2.
    /// </summary>
    [SerializeField]
    private float[] leftSpacings3;
    /// <summary>
    /// Spacings of each node on right lane 1. The spacing value is the distance from node to guiding line.
    /// </summary>
    [SerializeField]
    private float[] rightSpacings1;
    /// <summary>
    /// Spacings of each node on right lane 2. The spacing value is the distance from node to node on right lane 1.
    /// </summary>
    [SerializeField]
    private float[] rightSpacings2;
    /// <summary>
    /// Spacings of each node on right lane 3. The spacing value is the distance from node to node on right lane 2.
    /// </summary>
    [SerializeField]
    private float[] rightSpacings3;
    /// <summary>
    /// An array of control point handle modes for each control point on guiding line spline.
    /// </summary>
    [SerializeField]
    private Bezier.ControlPointMode[] modes;
    /// <summary>
    /// An array of control point handle modes for each control on left lane 1 spline.
    /// </summary>
    [SerializeField]
    private Bezier.ControlPointMode[] leftModes1;
    /// <summary>
    /// An array of control point handle modes for each control on left lane 2 spline.
    /// </summary>
    [SerializeField]
    private Bezier.ControlPointMode[] leftModes2;
    /// <summary>
    /// An array of control point handle modes for each control on left lane 3 spline.
    /// </summary>
    [SerializeField]
    private Bezier.ControlPointMode[] leftModes3;
    /// <summary>
    /// An array of control point handle modes for each control on right lane 1 spline.
    /// </summary>
    [SerializeField]
    private Bezier.ControlPointMode[] rightModes1;
    /// <summary>
    /// An array of control point handle modes for each control on right lane 2 spline.
    /// </summary>
    [SerializeField]
    private Bezier.ControlPointMode[] rightModes2;
    /// <summary>
    /// An array of control point handle modes for each control on right lane 3 spline.
    /// </summary>
    [SerializeField]
    private Bezier.ControlPointMode[] rightModes3;
    /// <summary>
    /// Traffic size of this road. This value is assigned to each created lane.
    /// </summary>
    [SerializeField]
    private TrafficSize traffic;
    /// <summary>
    /// Speed limit on this road. This value is assigned to each lane.
    /// </summary>
    [SerializeField]
    private SpeedLimits speedLimit;
    /// <summary>
    /// Reference to a road network gameobject that this road belongs. If no gameobject with a RoadNetwork component exist,
    /// a new one is created automatically. By default, there is only one road network, but for further development, there is an
    /// option to select the used road network.
    /// </summary>
    public GameObject roadNetwork;
    /// <summary>
    /// An array of pre-existing nodes of other road / intersection used as start nodes. This array is used for automatically
    /// snap connecting roads and update info in these nodes.
    /// </summary>
    public Nodes[] startNodes;
    /// <summary>
    /// An array of pre-existing nodes of other road / intersection used as end nodes. This array is used for automatically
    /// snap connecting roads and update info in these nodes.
    /// </summary>
    public Nodes[] endNodes;
    /// <summary>
    /// Number of left side (opposite direction) lanes.
    /// </summary>
    [SerializeField]
    private int leftLaneCount;
    /// <summary>
    /// Number of right side lanes.
    /// </summary>
    [SerializeField]
    private int rightLaneCount;
    /// <summary>
    /// Is the leftmost lane of left side lanes (opposite direction) a bus lane?
    /// </summary>
    [SerializeField]
    private bool busLaneLeft;
    /// <summary>
    /// The node index of where the bus lane starts on left side lanes.
    /// </summary>
    [SerializeField]
    private int busLeftStart;
    /// <summary>
    /// The node index of where the bus lane ends on right left lanes.
    /// </summary>
    [SerializeField]
    private int busLeftEnd;
    /// <summary>
    /// The node index of where the bus lane starts on right side lanes.
    /// </summary>
    [SerializeField]
    private int busRightStart;
    /// <summary>
    /// The node index of where the bus lane ends on right side lanes.
    /// </summary>
    [SerializeField]
    private int busRightEnd;
    /// <summary>
    /// Is the rightmost lane of right side lanes a bus lane?
    /// </summary>
    [SerializeField]
    private bool busLaneRight;

    /// <summary>
    /// A boolean array of permitted lane changes. The order of the array is:
    /// 0 = right side, lane 1 change to lane 2
    /// 1 = right side, lane 2 change to lane 1
    /// 2 = right side, lane 2 change to lane 3
    /// 3 = right side, lane 3 change to lane 2
    /// 4 = left side, lane 1 change to lane 2
    /// 5 = left side, lane 2 change to lane 1
    /// 6 = left side, lane 2 change to lane 3
    /// 7 = left side, lane 3 change to lane 2
    /// </summary>
    public bool[] permittedLaneChanges;
    /// <summary>
    /// Array of lane change permitted start indexes for each lane. The order of the array is:
    /// 0 = right side, lane 1 change to lane 2
    /// 1 = right side, lane 2 change to lane 1
    /// 2 = right side, lane 2 change to lane 3
    /// 3 = right side, lane 3 change to lane 2
    /// 4 = left side, lane 1 change to lane 2
    /// 5 = left side, lane 2 change to lane 1
    /// 6 = left side, lane 2 change to lane 3
    /// 7 = left side, lane 3 change to lane 2
    /// </summary>
    public int[] laneChangeStartIndex;
    /// <summary>
    /// Array of lane change permitted end indexes for each lane. The order of the array is:
    /// 0 = right side, lane 1 change to lane 2
    /// 1 = right side, lane 2 change to lane 1
    /// 2 = right side, lane 2 change to lane 3
    /// 3 = right side, lane 3 change to lane 2
    /// 4 = left side, lane 1 change to lane 2
    /// 5 = left side, lane 2 change to lane 1
    /// 6 = left side, lane 2 change to lane 3
    /// 7 = left side, lane 3 change to lane 2
    /// </summary>
    public int[] laneChangeEndIndex;

    /// <summary>
    /// Get and set left lane cout.
    /// </summary>
    public int LeftLaneCount
    {
        get { return leftLaneCount; }
        set
        {
            int v = Mathf.Clamp(value, 0, 3);
            if (leftLaneCount != v)
            {
                leftLaneCount = v;
            }
        }
    }
    /// <summary>
    /// Get and set right lane count.
    /// </summary>
    public int RightLaneCount
    {
        get { return rightLaneCount; }
        set
        {
            int v = Mathf.Clamp(value, 0, 3);
            if (rightLaneCount != v)
            {
                rightLaneCount = v;

            }
        }
    }
    /// <summary>
    /// Get and set the rightmost lane of left side lanes as a bus lane.
    /// </summary>
    public bool BusLaneLeft
    {
        get
        {
            return busLaneLeft;
        }
        set
        {
            busLaneLeft = value;
        }
    }
    /// <summary>
    /// Get and set the rightmost lane of the right side lanes as a bus lane.
    /// </summary>
    public bool BusLaneRight
    {
        get
        {
            return busLaneRight;
        }
        set
        {
            busLaneRight = value;
        }
    }
    /// <summary>
    /// Get and set the node index of where the bus lane starts on the left side lanes.
    /// </summary>
    public int BusLeftStart
    {
        get
        {
            return busLeftStart;
        }
        set
        {
            busLeftStart = value;
        }
    }
    /// <summary>
    /// Get and set the node index of where the bus lane ends on the right side lanes.
    /// </summary>
    public int BusLeftEnd
    {
        get
        {
            return busLeftEnd;
        }
        set
        {
            busLeftEnd = value;
        }
    }
    /// <summary>
    /// Get and set the speedlimit for this road.
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
    /// Get and set the node index of where the bus lane starts on right side lanes.
    /// </summary>
    public int BusRightStart
    {
        get
        {
            return busRightStart;
        }
        set
        {
            busRightStart = value;
        }
    }
    /// <summary>
    /// Get and set the node index of where the bus lane ends on right side lanes.
    /// </summary>
    public int BusRightEnd
    {
        get
        {
            return busRightEnd;
        }
        set
        {
            busRightEnd = value;
        }
    }
    /// <summary>
    /// Get and set a flag for this ParallelBezierSplines to mark it as initialized. Custom inspector uses this value.
    /// </summary>
    public bool Initialized
    {
        get
        {
            return initialized;
        }
        set
        {
            initialized = value;
        }
    }
    /// <summary>
    /// Get and set a flag for this ParallelBezierSplines to mark its lanes as set. CustomInspector uses this value.
    /// </summary>
    public bool LanesSet
    {
        get
        {
            return lanesSet;
        }
        set
        {
            lanesSet = value;
        }
    }
    /// <summary>
    /// Get and set a flag for this ParallelBezierSplines to mark its nodes as set. CustomInspector uses this value.
    /// </summary>
    public bool NodesSet
    {
        get
        {
            return nodesSet;
        }
        set
        {
            nodesSet = value;
        }
    }
    /// <summary>
    /// Get and set the node count of this road.
    /// </summary>
    public int NodeCount
    {
        get
        {
            return nodeCount;
        }
        set
        {
            nodeCount = value;
        }
    }
    /// <summary>
    /// Get and set the traffic size of this road.
    /// </summary>
    public TrafficSize Traffic
    {
        get
        {
            return traffic;
        }
        set
        {
            traffic = value;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        LeftLaneCount = leftLaneCount;
        RightLaneCount = rightLaneCount;
        BusLaneLeft = busLaneLeft;
        BusLaneRight = busLaneRight;
        BusLeftStart = busLeftStart;
        BusLeftEnd = busLeftEnd;
        BusRightStart = busRightStart;
        BusRightEnd = busRightEnd;
        Initialized = initialized;
        LanesSet = lanesSet;
        NodesSet = nodesSet;
        NodeCount = nodeCount;
        Traffic = traffic;
    }
#endif
    /// <summary>
    /// Get starting point of this road.
    /// </summary>
    /// <returns>Vector3 starting point of this road.</returns>
    public Vector3 GetStartPoint()
    {
        return points[0];
    }
    /// <summary>
    /// Get node count on selected segment.
    /// </summary>
    /// <param name="segment">Segment index</param>
    /// <returns>Node count on segment</returns>
    public int GetNodesOnSegment(int segment)
    {
        if (segment > nodesOnSegment.Length - 1)
        {
            return -1;
        }
        return nodesOnSegment[segment];
    }
    /// <summary>
    /// Set node count on selected segment.
    /// </summary>
    /// <param name="segment">Segment index</param>
    /// <param name="amount">Node count on segment</param>
    public void SetNodesOnSegment(int segment, int amount)
    {
        if (segment > nodesOnSegment.Length - 1)
        {
            return;
        }
        nodesOnSegment[segment] = amount;
    }
    /// <summary>
    /// Get segment count.
    /// </summary>
    public int SegmentCount
    {
        get
        {
            return segmentLengths.Length;
        }
    }
    /// <summary>
    /// Get Bezier control point count.
    /// </summary>
    public int ControlPointCount
    {
        get
        {
            return points.Length;
        }
    }
    /// <summary>
    /// Get spline length.
    /// </summary>
    public float SplineLength
    {
        get
        {
            return splineLength;
        }
    }
    
    /// <summary>
    /// Get the length of selected lane. This function is currently not used.
    /// </summary>
    /// <param name="lane">Lane index</param>
    /// <param name="rightLane">Is lane a right side lane</param>
    /// <returns>Lane length is a sum of it's segment lengths</returns>
    public float GetLaneLength(int lane, bool rightLane)
    {
        float length = 0f;
        if (rightLane)
        {
            if (lane < rightLaneCount)
            {
                length = rightSplineLengths[lane];
            }
        }
        else
        {
            if (lane < leftLaneCount)
            {
                length = leftSplineLengths[lane];
            }
        }
        return length;
    }
    /// <summary>
    /// Getter for guide line spline's bezier point positions.
    /// </summary>
    /// <param name="index">Control point's index</param>
    /// <returns>Vector3 position</returns>
    public Vector3 GetControlPoint(int index)
    {
        return points[index];
    }
    /// <summary>
    /// Getter for lane's bezier point positions.
    /// </summary>
    /// <param name="lane">Lane index</param>
    /// <param name="index">Point index</param>
    /// <param name="rightLane">Is lane a right side lane (or left)</param>
    /// <returns></returns>
    public Vector3 GetLanePoint(int lane, int index, bool rightLane)
    {
        Vector3 v = Vector3.zero;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    v = rightLanePoints1[index];
                    break;
                case 1:
                    v = rightLanePoints2[index];
                    break;
                case 2:
                    v = rightLanePoints3[index];
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    v = leftLanePoints1[index];
                    break;
                case 1:
                    v = leftLanePoints2[index];
                    break;
                case 2:
                    v = leftLanePoints3[index];
                    break;
            }
        }
        return v;
    }
    /// <summary>
    /// Returns bezier curve count.
    /// </summary>
    public int CurveCount
    {
        get
        {
            return (points.Length - 1) / 3;
        }
    }
    /// <summary>
    /// Get spacing of selected lane at selected node index.
    /// </summary>
    /// <param name="lane">Lane index</param>
    /// <param name="pointIndex">Control point index.</param>
    /// <param name="rightLane">Is lane a right side lane</param>
    /// <returns></returns>
    public float GetLaneSpacing(int lane, int pointIndex, bool rightLane)
    {
        float space = 0f;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    space = rightSpacings1[pointIndex / 3];
                    break;
                case 1:
                    space = rightSpacings2[pointIndex / 3];
                    break;
                case 2:
                    space = rightSpacings3[pointIndex / 3];
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    space = leftSpacings1[pointIndex / 3];
                    break;
                case 1:
                    space = leftSpacings2[pointIndex / 3];
                    break;
                case 2:
                    space = leftSpacings3[pointIndex / 3];
                    break;
            }
        }
        return space;
    }
    /// <summary>
    /// Set lane spacing of selected lane at selected node index.
    /// </summary>
    /// <param name="lane">Lane index</param>
    /// <param name="node">Node index</param>
    /// <param name="spacing">Spacing value to be set</param>
    /// <param name="rightLane">Is lane a right side lane</param>
    public void SetLaneSpacing(int lane, int node, float spacing, bool rightLane)
    {
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    rightSpacings1[node / 3] = spacing;
                    break;
                case 1:
                    rightSpacings2[node / 3] = spacing;
                    break;
                case 2:
                    rightSpacings3[node / 3] = spacing;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    leftSpacings1[node / 3] = spacing;
                    break;
                case 1:
                    leftSpacings2[node / 3] = spacing;
                    break;
                case 2:
                    leftSpacings3[node / 3] = spacing;
                    break;
            }
        }
    }
    /// <summary>
    /// Sets the position of a point on guide line bezier, affects lanes accordingly.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="point"></param>
    public void SetControlPoint(int index, Vector3 point)
    {
        //******** this adjustment is made so that when a middle point is moved, it
        // affects points on both sides of it
        if (index % 3 == 0)
        {
            Vector3 delta = point - points[index];
            if (index > 0)
            {
                points[index - 1] += delta;
            }
            if (index + 1 < points.Length)
            {
                points[index + 1] += delta;
            }
        }
        //***********
        points[index] = point;
        EnforceMode(index);
    }
    /// <summary>
    /// Sets the position of a point on lane's bezier. 
    /// </summary>
    /// <param name="lane">Lane index</param>
    /// <param name="index">Point index</param>
    /// <param name="point">New position for point</param>
    /// <param name="rightLane">Is lane a right side lane</param>
    public void SetLanePoint(int lane, int index, Vector3 point, bool rightLane)
    {
        //******** this adjustment is made so that when a middle point is moved, it
        // affects points on both sides of it
        Vector3[] pointArray = rightLanePoints1;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    pointArray = rightLanePoints1;
                    break;
                case 1:
                    pointArray = rightLanePoints2;
                    break;
                case 2:
                    pointArray = rightLanePoints3;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    pointArray = leftLanePoints1;
                    break;
                case 1:
                    pointArray = leftLanePoints2;
                    break;
                case 2:
                    pointArray = leftLanePoints3;
                    break;
            }
        }

        if (index % 3 == 0)
        {
            Vector3 delta = point - pointArray[index];
            if (index > 0)
            {
                pointArray[index - 1] += delta;
            }
            if (index + 1 < ControlPointCount)
            {
                pointArray[index + 1] += delta;
            }
        }
        //***********
        pointArray[index] = point;
        EnforceLaneMode(lane, index, rightLane);
    }
    /// <summary>
    /// This function is called for each affected lane when one lane's bezier point is moved in sceneview.
    /// </summary>
    /// <param name="lane">Lane index</param>
    /// <param name="index">Bezier point index</param>
    /// <param name="moved">Vector3 value of movement</param>
    /// <param name="rightLane">Is lane a right lane</param>
    public void AdjustLane(int lane, int index, Vector3 moved, bool rightLane)
    {
        Vector3[] pointArray = rightLanePoints1;
        int ind;

        if (rightLane)
        {
            ind = index;
            switch (lane)
            {
                case 0:
                    pointArray = rightLanePoints1;
                    break;
                case 1:
                    pointArray = rightLanePoints2;
                    break;
                case 2:
                    pointArray = rightLanePoints3;
                    break;
            }
        }
        else
        {
            ind = ControlPointCount - 1 - index;
            switch (lane)
            {
                case 0:
                    pointArray = leftLanePoints1;
                    break;
                case 1:
                    pointArray = leftLanePoints2;
                    break;
                case 2:
                    pointArray = leftLanePoints3;
                    break;
            }
        }
        if (index % 3 == 0)
        {
            pointArray[ind] = GetControlPoint(index) + moved;
            if (ind > 0)
            {
                if (rightLane)
                {
                    pointArray[index - 1] = GetControlPoint(index - 1) + moved;
                }
                else
                {
                    pointArray[ind - 1] = GetControlPoint(index + 1) + moved;
                }
            }
            if (ind < pointArray.Length - 1)
            {
                if (rightLane)
                {
                    pointArray[index + 1] = GetControlPoint(index + 1) + moved;
                }
                else
                {
                    pointArray[ind + 1] = GetControlPoint(index - 1) + moved;
                }
            }
        }
        else
        {
            pointArray[ind] += moved;
            EnforceLaneMode(lane, ind, rightLane);
        }
    }
    /// <summary>
    /// Enforces the bezier control mode to moved guide line bezier point's handles.
    /// </summary>
    /// <param name="index">Point index</param>
    private void EnforceMode(int index)
    {
        int modeIndex = (index + 1) / 3;
        Bezier.ControlPointMode mode = modes[modeIndex];
        EnforceLane(index, mode, ref points);
        return;
    }
    /// <summary>
    /// Enforces the bezier control mode to handles of a lanes bezier point.
    /// </summary>
    /// <param name="lane">Lane index</param>
    /// <param name="index">Point index</param>
    /// <param name="rightLane">Is lane a right lane</param>
    private void EnforceLaneMode(int lane, int index, bool rightLane)
    {
        int modeIndex = (index + 1) / 3;
        Bezier.ControlPointMode mode = GetLaneMode(lane, index, rightLane);
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    EnforceLane(index, mode, ref rightLanePoints1);
                    break;
                case 1:
                    EnforceLane(index, mode, ref rightLanePoints2);
                    break;
                case 2:
                    EnforceLane(index, mode, ref rightLanePoints3);
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    EnforceLane(index, mode, ref leftLanePoints1);
                    break;
                case 1:
                    EnforceLane(index, mode, ref leftLanePoints2);
                    break;
                case 2:
                    EnforceLane(index, mode, ref leftLanePoints3);
                    break;
            }
        }
    }
    /// <summary>
    /// Functions EnforceMode and EnforceLaneMode call this functions call this function to enforce a bezier
    /// handle control mode to a selected point.
    /// </summary>
    /// <param name="index">Point index</param>
    /// <param name="mode">Enforced mode</param>
    /// <param name="pointArray">Point array to be modified (guide line point array or a lane point array)</param>
    private void EnforceLane(int index, Bezier.ControlPointMode mode, ref Vector3[] pointArray)
    {
        int modeIndex = (index + 1) / 3;
        // We don't enforce if we are at end points or the current mode is set to 'FREE'.
        if (mode == Bezier.ControlPointMode.Free || modeIndex == 0 || modeIndex == modes.Length - 1)
        {
            return;
        }
        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (index <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = ControlPointCount - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= ControlPointCount)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= ControlPointCount)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = ControlPointCount - 2;
            }
        }

        Vector3 middle = pointArray[middleIndex];
        Vector3 enforcedTangent = middle - pointArray[fixedIndex];
        if (mode == Bezier.ControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, pointArray[enforcedIndex]);
        }
        pointArray[enforcedIndex] = middle + enforcedTangent;
    }

    /// <summary>
    /// Get tangent at selected point of a bezier segment.
    /// </summary>
    /// <param name="segment">Segment index</param>
    /// <param name="fraction">Fraction of the distance (0f - 1f) from the start of the segment</param>
    /// <returns>Normalized direction vector (tangent) at the given point of the curve</returns>
    private Vector3 GetSegmentedDirection(int segment, float fraction)
    {

        return GetSegmentedVelocity(segment, fraction).normalized;
    }
    /// <summary>
    /// Returns a velocity vector at the given point of a segment. This vector normalized is tangent direction at the given point on the curve.
    /// </summary>
    /// <param name="segment">Bezier segment index</param>
    /// <param name="fraction">Fraction of the segment from its start (0f - 1f).</param>
    /// <returns>Velocity vector at the given point of a segment</returns>
    private Vector3 GetSegmentedVelocity(int segment, float fraction)
    {
        int i = segment * 3;
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], fraction))
            - transform.position;
    }
    /// <summary>
    /// Returns the position of a point on bezier curve segment.
    /// </summary>
    /// <param name="segment">Segment index</param>
    /// <param name="fraction">Fraction from the start of the segment (0f - 1f).</param>
    /// <returns>Vector3 position</returns>
    public Vector3 GetSegmentedPoint(int segment, float fraction)
    {
        if (segment == segmentLengths.Length)
        {
            segment -= 1;
        }
        int i = segment * 3;
        return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], fraction));
    }
    /// <summary>
    /// Returns the position of a point on selected lane's bezier curve segment.
    /// </summary>
    /// <param name="lane">Lane index (0 - 2).</param>
    /// <param name="segment">Segment index</param>
    /// <param name="fraction">Fraction from the start of the segment (0f - 1f).</param>
    /// <param name="rightLane">Is this lane a right side or left side (opposite direction) lane?</param>
    /// <returns>A position of the given point on the selected lane's bezier curve segment.</returns>
    public Vector3 GetSegmentedPointLane(int lane, int segment, float fraction, bool rightLane)
    {
        if (segment == segmentLengths.Length)
        {
            segment -= 1;
        }
        int i = segment * 3;
        Vector3 v = Vector3.zero;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints1[i], rightLanePoints1[i + 1],
                        rightLanePoints1[i + 2], rightLanePoints1[i + 3], fraction));
                    break;
                case 1:
                    v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints2[i], rightLanePoints2[i + 1],
                        rightLanePoints2[i + 2], rightLanePoints2[i + 3], fraction));
                    break;
                case 2:
                    v = transform.TransformPoint(Bezier.GetPoint(rightLanePoints3[i], rightLanePoints3[i + 1],
                        rightLanePoints3[i + 2], rightLanePoints3[i + 3], fraction));
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints1[i], leftLanePoints1[i + 1],
                        leftLanePoints1[i + 2], leftLanePoints1[i + 3], fraction));
                    break;
                case 1:
                    v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints2[i], leftLanePoints2[i + 1],
                        leftLanePoints2[i + 2], leftLanePoints2[i + 3], fraction));
                    break;
                case 2:
                    v = transform.TransformPoint(Bezier.GetPoint(leftLanePoints3[i], leftLanePoints3[i + 1],
                        leftLanePoints3[i + 2], leftLanePoints3[i + 3], fraction));
                    break;
            }
        }
        return v;
    }
    /// <summary>
    /// Returns a normalized direction vector (tangent) of the road's guideline bezier at the given point.
    /// </summary>
    /// <param name="t">Fraction of the distance from the start (0-1).</param>
    /// <returns>Normalized direction vector</returns>
    public Vector3 GetDirection(float t)
    {
        return GetVelocity(t).normalized;
    }
    /// <summary>
    /// Returns a normalized direction vector of the given lane at the given point.
    /// </summary>
    /// <param name="lane">Lane index (0 - 2)</param>
    /// <param name="t">Fraction of the distance from the start (0-1)</param>
    /// <param name="rightLane">Is this a right side lane or a left side (opposite direction) lane?</param>
    /// <returns>Normalized direction vector</returns>
    public Vector3 GetDirectionLane(int lane, float t, bool rightLane)
    {
        return GetVelocityLane(lane, t, rightLane).normalized;
    }
    /// <summary>
    /// Returns velocity at the given point of road's guideline.
    /// </summary>
    /// <param name="t">Fraction from the start (0-1)</param>
    /// <returns>Velocity Vector3 at the given point.</returns>
    public Vector3 GetVelocity(float t)
    {
        int i = GetI(ref t);
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t))
            - transform.position;
    }
    /// <summary>
    /// Returns velocity at the given point on a lane.
    /// </summary>
    /// <param name="lane">Lane index (0-2)</param>
    /// <param name="t">Fraction from the start (0-1)</param>
    /// <param name="rightLane">Is this a right side lane or left side (opposite direction) lane?</param>
    /// <returns>Vector3 velocity value at the given point.</returns>
    public Vector3 GetVelocityLane(int lane, float t, bool rightLane)
    {
        Vector3 v = Vector3.zero;
        int i = GetI(ref t);
        if (rightLane)
        {
            if (lane >= RightLaneCount)
            {
                return v;
            }
            switch (lane)
            {
                case 0:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints1[i],
                        rightLanePoints1[i + 1], rightLanePoints1[i + 2], rightLanePoints1[i + 3], t))
                        - transform.position;
                    break;
                case 1:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints2[i],
                        rightLanePoints2[i + 1], rightLanePoints2[i + 2], rightLanePoints2[i + 3], t))
                        - transform.position;
                    break;
                case 2:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(rightLanePoints3[i],
                        rightLanePoints3[i + 1], rightLanePoints3[i + 2], rightLanePoints3[i + 3], t))
                        - transform.position;
                    break;
            }
        }
        else
        {
            if (lane >= LeftLaneCount)
            {
                return v;
            }
            switch (lane)
            {
                case 0:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints1[i],
                        leftLanePoints1[i + 1], leftLanePoints1[i + 2], leftLanePoints1[i + 3], t))
                        - transform.position;
                    break;
                case 1:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints2[i],
                        leftLanePoints2[i + 1], leftLanePoints2[i + 2], leftLanePoints2[i + 3], t))
                        - transform.position;
                    break;
                case 2:
                    v = transform.TransformPoint(Bezier.GetFirstDerivative(leftLanePoints3[i],
                        leftLanePoints3[i + 1], leftLanePoints3[i + 2], leftLanePoints3[i + 3], t))
                        - transform.position;
                    break;
            }
        }
        return v;
    }
    /// <summary>
    /// This function calculates the index of the first bezier point of the segment in question. Float t is passed to this function
    /// as a fraction of the whole length of all segments and returned as a fraction of the specific segment in question.
    /// </summary>
    /// <param name="t">Fraction value. Referenced in-value is a fraction of the whole length of all segments. The out-value is a fraction
    /// of the segment in question.</param>
    /// <returns></returns>
    private int GetI(ref float t)
    {
        int i;
        if (t >= 1f)
        {
            t = 1f;
            i = points.Length - 4;
        }
        else
        {
            t = Mathf.Clamp01(t) * CurveCount;
            i = (int)t;
            t -= i;
            i *= 3;
        }
        return i;
    }
    /// <summary>
    /// Recalculates the length of a segment or segments.
    /// </summary>
    /// <param name="index">Control point index</param>
    public void RecalculateLength(int index)
    {
        int segment = index / 3;
        if (segment == segmentLengths.Length)
        {
            segment -= 1;
        }
        float dist = 0f;
        Vector3 prev = GetSegmentedPoint(segment, 0f);
        for (int i = 1; i <= 1000; i++)
        {
            Vector3 next = GetSegmentedPoint(segment, (float)i / 1000f);
            dist += Vector3.Distance(prev, next);
            prev = next;
        }
        segmentLengths[segment] = dist;
        if (segment == 0)
        {
            UpdateSplineLength();
            return;
        }
        else
        {
            if (segment == 0)
            {
                segment = segmentLengths.Length - 1;
            }
            else
            {
                segment -= 1;
            }
            dist = 0f;
            prev = GetSegmentedPoint(segment, 0f);
            for (int i = 1; i <= 1000; i++)
            {
                Vector3 next = GetSegmentedPoint(segment, i / 1000f);
                dist += Vector3.Distance(prev, next);
                prev = next;
            }
            segmentLengths[segment] = dist;
            UpdateSplineLength();
        }
    }
    /// <summary>
    /// Recalculates the length of a segment or segments of a lane.
    /// </summary>
    /// <param name="lane">Lane index (0 - 2)</param>
    /// <param name="index">Control point index</param>
    /// <param name="rightLane">Is this a right side lane or a left side lane (opposite direction)?</param>
    public void RecalculateLaneLength(int lane, int index, bool rightLane)
    {
        int segment = index / 3;
        if (segment == segmentLengths.Length)
        {
            segment -= 1;
        }
        float dist = 0f;
        float[] lengthArray = rightSegmentLengths1;
        if (rightLane)
        {
            switch(lane)
            {
                case 0:
                    lengthArray = rightSegmentLengths1;
                    break;
                case 1:
                    lengthArray = rightSegmentLengths2;
                    break;
                case 2:
                    lengthArray = rightSegmentLengths3;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    lengthArray = leftSegmentLengths1;
                    break;
                case 1:
                    lengthArray = leftSegmentLengths2;
                    break;
                case 2:
                    lengthArray = leftSegmentLengths3;
                    break;
            }
        }
        Vector3 prev = GetSegmentedPointLane(lane, segment, 0f, rightLane);
        for (int i = 1; i <= 1000; i++)
        {
            Vector3 next = GetSegmentedPointLane(lane, segment, i / 1000f, rightLane);
            dist += Vector3.Distance(prev, next);
            prev = next;
        }
        lengthArray[segment] = dist;

        if (segment == 0)
        {
            UpdateLaneLength(lane, rightLane);
            return;
        }
        else
        {
            if (segment == 0)
            {
                segment = segmentLengths.Length - 1;
            }
            else
            {
                segment -= 1;
            }
            dist = 0f;
            prev = GetSegmentedPointLane(lane, segment, 0f, rightLane);
            for (int i = 1; i <= 1000; i++)
            {
                Vector3 next = GetSegmentedPointLane(lane, segment, i / 1000f, rightLane);
                dist += Vector3.Distance(prev, next);
                prev = next;
            }
            lengthArray[segment] = dist;
            UpdateLaneLength(lane, rightLane);
        }
    }
    /// <summary>
    /// Updates the value of splineLength.
    /// </summary>
    private void UpdateSplineLength()
    {
        float length = 0;
        for (int i = 0; i < segmentLengths.Length; i++)
        {
            length += segmentLengths[i];
        }
        splineLength = length;
    }
    /// <summary>
    /// Updates the length value of the given lane.
    /// </summary>
    /// <param name="lane">Lane index (0 - 2)</param>
    /// <param name="rightLane">Is this a right side lane or a left side (opposite direction) lane?</param>
    private void UpdateLaneLength(int lane, bool rightLane)
    {
        float length = 0f;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    for (int i = 0; i < rightSegmentLengths1.Length; i++)
                    {
                        length += rightSegmentLengths1[i];
                    }
                    rightSplineLengths[lane] = length;
                    break;
                case 1:
                    for (int i = 0; i < rightSegmentLengths2.Length; i++)
                    {
                        length += rightSegmentLengths2[i];
                    }
                    rightSplineLengths[lane] = length;
                    break;
                case 2:
                    for (int i = 0; i < rightSegmentLengths3.Length; i++)
                    {
                        length += rightSegmentLengths3[i];
                    }
                    rightSplineLengths[lane] = length;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    for (int i = 0; i < leftSegmentLengths1.Length; i++)
                    {
                        length += leftSegmentLengths1[i];
                    }
                    leftSplineLengths[lane] = length;
                    break;
                case 1:
                    for (int i = 0; i < leftSegmentLengths2.Length; i++)
                    {
                        length += leftSegmentLengths2[i];
                    }
                    leftSplineLengths[lane] = length;
                    break;
                case 2:
                    for (int i = 0; i < leftSegmentLengths3.Length; i++)
                    {
                        length += leftSegmentLengths3[i];
                    }
                    leftSplineLengths[lane] = length;
                    break;
            }
        }
    }
    /// <summary>
    /// Adds a new segment to the road.
    /// </summary>
    public void AddCurve()
    {
        // First, update the main spline
        Vector3 point = points[points.Length - 1];
        //use the length of the previous segment as a measure for the new one
        float length = segmentLengths[segmentLengths.Length - 1] / 3f;
        //continue to the direction of the previous segment
        Vector3 dir = GetSegmentedDirection(segmentLengths.Length - 1, 1f);
        // Array requires System-namespace. points is passed as a REFERENCE (not a copy)
        // 1. Add new points
        Array.Resize(ref points, points.Length + 3);
        point += length * dir;
        points[points.Length - 3] = point;
        point += length * dir;
        points[points.Length - 2] = point;
        point += length * dir;
        points[points.Length - 1] = point;
        // 2. Resize segmentLengths
        Array.Resize(ref segmentLengths, segmentLengths.Length + 1);
        float lastSegmentLegth = Vector3.Distance(points[points.Length - 4], points[points.Length - 1]);
        segmentLengths[segmentLengths.Length - 1] = lastSegmentLegth;
        // 3. Resize nodesOnSegment
        Array.Resize(ref nodesOnSegment, nodesOnSegment.Length + 1);
        // 4. Resize modes
        Array.Resize(ref modes, modes.Length + 1);
        modes[modes.Length - 1] = modes[modes.Length - 2];
        EnforceMode(points.Length - 4);

        // 5. Add new right lane points
        //int size = points.Length;
        Vector3 rightDir = new Vector3(dir.z, dir.y, -dir.x);
        //***********
        float space = 0f;
        Vector3 spacing;
        //rightLanePoints1
        Array.Resize(ref rightLanePoints1, rightLanePoints1.Length + 3);
        space += rightSpacings1[rightSpacings1.Length - 1];
        spacing = space * rightDir;
        rightLanePoints1[rightLanePoints1.Length - 3] = points[points.Length - 3] + spacing;
        rightLanePoints1[rightLanePoints1.Length - 2] = points[points.Length - 2] + spacing;
        rightLanePoints1[rightLanePoints1.Length - 1] = points[points.Length - 1] + spacing;
        //rightLanePoints2
        Array.Resize(ref rightLanePoints2, rightLanePoints2.Length + 3);
        space += rightSpacings2[rightSpacings2.Length - 1];
        spacing = space * rightDir;
        rightLanePoints2[rightLanePoints2.Length - 3] = points[points.Length - 3] + spacing;
        rightLanePoints2[rightLanePoints2.Length - 2] = points[points.Length - 2] + spacing;
        rightLanePoints2[rightLanePoints2.Length - 1] = points[points.Length - 1] + spacing;
        //rightLanePoints3
        Array.Resize(ref rightLanePoints3, rightLanePoints3.Length + 3);
        space += rightSpacings3[rightSpacings3.Length - 1];
        spacing = space * rightDir;
        rightLanePoints3[rightLanePoints3.Length - 3] = points[points.Length - 3] + spacing;
        rightLanePoints3[rightLanePoints3.Length - 2] = points[points.Length - 2] + spacing;
        rightLanePoints3[rightLanePoints3.Length - 1] = points[points.Length - 1] + spacing;

        // 6. Add new left lane points
        space = 0f;
        //leftLanePoints1
        Array.Resize(ref leftLanePoints1, leftLanePoints1.Length + 3);
        for (int i = leftLanePoints1.Length - 4; i >= 0; i--)
        {
            leftLanePoints1[i + 3] = leftLanePoints1[i];
        }
        space += leftSpacings1[0];
        spacing = rightDir * space;
        leftLanePoints1[0] = points[points.Length - 1] - spacing;
        leftLanePoints1[1] = points[points.Length - 2] - spacing;
        leftLanePoints1[2] = points[points.Length - 3] - spacing;
        //leftLanePoints2
        Array.Resize(ref leftLanePoints2, leftLanePoints2.Length + 3);
        for (int i = leftLanePoints2.Length - 4; i >= 0; i--)
        {
            leftLanePoints2[i + 3] = leftLanePoints2[i];
        }
        space += leftSpacings2[0];
        spacing = rightDir * space;
        leftLanePoints2[0] = points[points.Length - 1] - spacing;
        leftLanePoints2[1] = points[points.Length - 2] - spacing;
        leftLanePoints2[2] = points[points.Length - 3] - spacing;
        //leftLanePoints3
        Array.Resize(ref leftLanePoints3, leftLanePoints3.Length + 3);
        for (int i = leftLanePoints3.Length - 4; i >= 0; i--)
        {
            leftLanePoints3[i + 3] = leftLanePoints3[i];
        }
        space += leftSpacings3[0];
        spacing = rightDir * space;
        leftLanePoints3[0] = points[points.Length - 1] - spacing;
        leftLanePoints3[1] = points[points.Length - 2] - spacing;
        leftLanePoints3[2] = points[points.Length - 3] - spacing;

        // Add new spacings, copy previous value
        // 7. Update right spacings
        //rightSpacings1
        Array.Resize(ref rightSpacings1, rightSpacings1.Length + 1);
        rightSpacings1[rightSpacings1.Length - 1] = rightSpacings1[rightSpacings1.Length - 2];
        //rightSpacings2
        Array.Resize(ref rightSpacings2, rightSpacings2.Length + 1);
        rightSpacings2[rightSpacings2.Length - 1] = rightSpacings2[rightSpacings2.Length - 2];
        //rightSpacings3
        Array.Resize(ref rightSpacings3, rightSpacings3.Length + 1);
        rightSpacings3[rightSpacings3.Length - 1] = rightSpacings3[rightSpacings3.Length - 2];

        // 8. Update left spacings
        //leftSpacings1
        Array.Resize(ref leftSpacings1, leftSpacings1.Length + 1);
        for (int i = leftSpacings1.Length - 2; i >= 0; i--)
        {
            leftSpacings1[i + 1] = leftSpacings1[i];
        }
        leftSpacings1[0] = leftSpacings1[1];
        //leftSpacings2
        Array.Resize(ref leftSpacings2, leftSpacings2.Length + 1);
        for (int i = leftSpacings2.Length - 2; i >= 0; i--)
        {
            leftSpacings2[i + 1] = leftSpacings2[i];
        }
        leftSpacings2[0] = leftSpacings2[1];
        //leftSpacings3
        Array.Resize(ref leftSpacings3, leftSpacings3.Length + 1);
        for (int i = leftSpacings3.Length - 2; i >= 0; i--)
        {
            leftSpacings3[i + 1] = leftSpacings3[i];
        }
        leftSpacings3[0] = leftSpacings3[1];

        // 9. Resize right segments
        //rightSegmentLengths1
        Array.Resize(ref rightSegmentLengths1, rightSegmentLengths1.Length + 1);
        lastSegmentLegth = Vector3.Distance(rightLanePoints1[rightLanePoints1.Length - 4],
            rightLanePoints1[rightLanePoints1.Length - 1]);
        rightSegmentLengths1[rightSegmentLengths1.Length - 1] = lastSegmentLegth;
        //rightSegmentLengths2
        Array.Resize(ref rightSegmentLengths2, rightSegmentLengths2.Length + 1);
        lastSegmentLegth = Vector3.Distance(rightLanePoints2[rightLanePoints2.Length - 4],
            rightLanePoints2[rightLanePoints2.Length - 1]);
        rightSegmentLengths2[rightSegmentLengths2.Length - 1] = lastSegmentLegth;
        //rightSegmentLengths3
        Array.Resize(ref rightSegmentLengths3, rightSegmentLengths3.Length + 1);
        lastSegmentLegth = Vector3.Distance(rightLanePoints3[rightLanePoints3.Length - 4],
            rightLanePoints3[rightLanePoints3.Length - 1]);
        rightSegmentLengths3[rightSegmentLengths3.Length - 1] = lastSegmentLegth;

        // 10. Resize left segments
        //leftSegmentLengths1
        Array.Resize(ref leftSegmentLengths1, leftSegmentLengths1.Length + 1);
        for (int i = leftSegmentLengths1.Length -1; i > 0; i--)
        {
            leftSegmentLengths1[i] = leftSegmentLengths1[i - 1];
        }
        lastSegmentLegth = Vector3.Distance(leftLanePoints1[0], leftLanePoints1[3]);
        leftSegmentLengths1[0] = lastSegmentLegth;
        //leftSegmentlengths2
        Array.Resize(ref leftSegmentLengths2, leftSegmentLengths2.Length + 1);
        for (int i = leftSegmentLengths2.Length - 1; i > 0; i--)
        {
            leftSegmentLengths2[i] = leftSegmentLengths2[i - 1];
        }
        lastSegmentLegth = Vector3.Distance(leftLanePoints2[0], leftLanePoints2[3]);
        leftSegmentLengths2[0] = lastSegmentLegth;
        //leftSegmentLengths3
        Array.Resize(ref leftSegmentLengths3, leftSegmentLengths3.Length + 1);
        for (int i = leftSegmentLengths3.Length - 1; i > 0; i--)
        {
            leftSegmentLengths3[i] = leftSegmentLengths3[i - 1];
        }
        lastSegmentLegth = Vector3.Distance(leftLanePoints3[0], leftLanePoints3[3]);
        leftSegmentLengths3[0] = lastSegmentLegth;

        // 11. Update right modes
        //rightModes1
        Array.Resize(ref rightModes1, rightModes1.Length + 1);
        rightModes1[rightModes1.Length - 1] = rightModes1[rightModes1.Length - 2];
        //rightModes2
        Array.Resize(ref rightModes2, rightModes2.Length + 1);
        rightModes2[rightModes2.Length - 1] = rightModes2[rightModes2.Length - 2];
        //rightModes3
        Array.Resize(ref rightModes3, rightModes3.Length + 1);
        rightModes3[rightModes3.Length - 1] = rightModes3[rightModes3.Length - 2];

        // 12. Update left modes
        //leftModes1
        Array.Resize(ref leftModes1, leftModes1.Length + 1);
        for (int i = leftModes1.Length - 1; i > 0; i--)
        {
            leftModes1[i] = leftModes1[i - 1];
        }
        leftModes1[0] = leftModes1[1];
        //leftModes2
        Array.Resize(ref leftModes2, leftModes2.Length + 1);
        for (int i = leftModes2.Length - 1; i > 0; i--)
        {
            leftModes2[i] = leftModes2[i - 1];
        }
        leftModes2[0] = leftModes2[1];
        //leftModes1
        Array.Resize(ref leftModes3, leftModes3.Length + 1);
        for (int i = leftModes3.Length - 1; i > 0; i--)
        {
            leftModes3[i] = leftModes3[i - 1];
        }
        leftModes3[0] = leftModes3[1];
    }
    /// <summary>
    /// Returns the mode (enum) of the given lane at the given bezier point index.
    /// </summary>
    /// <param name="lane">Lane index (0 - 2)</param>
    /// <param name="index">Bezier point index</param>
    /// <param name="rightLane">Is this lane a right side lane or a left side (opposite direction) lane?</param>
    /// <returns>Bezier control point mode (enum) at the given index</returns>
    public Bezier.ControlPointMode GetLaneMode(int lane, int index, bool rightLane)
    {
        Bezier.ControlPointMode mode = Bezier.ControlPointMode.Aligned;
        int modeIndex = (index + 1) / 3;
        if (rightLane)
        {
            switch(index)
            {
                case 0:
                    mode = rightModes1[modeIndex];
                    break;
                case 1:
                    mode = rightModes2[modeIndex];
                    break;
                case 2:
                    mode = rightModes3[modeIndex];
                    break;
            }
        }
        else
        {
            switch (index)
            {
                case 0:
                    mode = leftModes1[modeIndex];
                    break;
                case 1:
                    mode = leftModes2[modeIndex];
                    break;
                case 2:
                    mode = leftModes3[modeIndex];
                    break;
            }
        }
        return mode;
    }
    /// <summary>
    /// Returns the control point mode (enum) at the given index of the road's guideline bezier.
    /// </summary>
    /// <param name="index">Bezier point index</param>
    /// <returns>Bezier control point mode (enum) at the given index</returns>
    public Bezier.ControlPointMode GetControlPointMode(int index)
    {
        return modes[(index + 1) / 3];
    }
    /// <summary>
    /// Sets the control point mode of the given index and enforces the mode to other affected indexes.
    /// </summary>
    /// <param name="index">Bezier point index</param>
    /// <param name="mode">Bezier control point mode (enum)</param>
    public void SetControlPointMode(int index, Bezier.ControlPointMode mode)
    {
        int modeIndex = (index + 1) / 3;
        modes[modeIndex] = mode;
        EnforceMode(index);
    }
    /// <summary>
    /// Sets the control point mode of the selected lane at the selected control point index.
    /// </summary>
    /// <param name="lane">Lane index (0 - 2)</param>
    /// <param name="index">Control point index</param>
    /// <param name="mode">Control point mode (enum)</param>
    /// <param name="rightLane">Is this lane a right side lane or left side (opposite direction) lane?</param>
    public void SetLaneMode(int lane, int index, Bezier.ControlPointMode mode, bool rightLane)
    {
        int modeIndex = (index + 1) / 3;
        if (rightLane)
        {
            switch (lane)
            {
                case 0:
                    rightModes1[modeIndex] = mode;
                    break;
                case 1:
                    rightModes2[modeIndex] = mode;
                    break;
                case 2:
                    rightModes3[modeIndex] = mode;
                    break;
            }
        }
        else
        {
            switch (lane)
            {
                case 0:
                    leftModes1[modeIndex] = mode;
                    break;
                case 1:
                    leftModes2[modeIndex] = mode;
                    break;
                case 2:
                    leftModes3[modeIndex] = mode;
                    break;
            }
        }
        EnforceLaneMode(lane, index, rightLane);
    }
    /// <summary>
    /// When resested, sets default parameter values and initializes arrays.
    /// </summary>
    public void Reset()
    {
        initialized = false;
        lanesSet = false;
        nodesSet = false;
        busLaneLeft = false;
        busLeftStart = 0;
        busLeftEnd = 0;
        busLaneRight = false;
        busRightStart = 0;
        busRightEnd = 0;
        traffic = TrafficSize.Average;
        speedLimit = SpeedLimits.KMH_40;
        permittedLaneChanges = new bool[] { false, false, false, false, false, false, false, false };

        roadNetwork = null;

        startNodes = new Nodes[] { null, null, null, null, null, null };
        endNodes = new Nodes[] { null, null, null, null, null, null };

        points = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(3f, 0f, 0f)
        };
        modes = new Bezier.ControlPointMode[]
        {
            Bezier.ControlPointMode.Aligned,
            Bezier.ControlPointMode.Aligned
        };

        splineLength = Vector3.Distance(points[0], points[3]);
        segmentLengths = new float[] { splineLength };
        nodesOnSegment = new int[] { 0 };

        NodeCount = 0;
        LeftLaneCount = 0;
        RightLaneCount = 0;


        leftLanePoints1 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        leftLanePoints2 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        leftLanePoints3 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        rightLanePoints1 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        rightLanePoints2 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        rightLanePoints3 = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        leftSegmentLengths1 = new float[] { 0 };
        leftSegmentLengths2 = new float[] { 0 };
        leftSegmentLengths3 = new float[] { 0 };
        rightSegmentLengths1 = new float[] { 0 };
        rightSegmentLengths2 = new float[] { 0 };
        rightSegmentLengths3 = new float[] { 0 };

        leftSplineLengths = new float[] {0f ,0f ,0f };
        rightSplineLengths = new float[] {0f ,0f ,0f };

        leftSpacings1 = new float[] { 0f, 0f };
        leftSpacings2 = new float[] { 0f, 0f };
        leftSpacings3 = new float[] { 0f, 0f };
        rightSpacings1 = new float[] { 0f, 0f };
        rightSpacings2 = new float[] { 0f, 0f };
        rightSpacings3 = new float[] { 0f, 0f };

        laneChangeStartIndex = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        laneChangeEndIndex = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };


        leftModes1 = new Bezier.ControlPointMode[]
        { Bezier.ControlPointMode.Aligned, Bezier.ControlPointMode.Aligned };
        leftModes2 = new Bezier.ControlPointMode[]
        { Bezier.ControlPointMode.Aligned, Bezier.ControlPointMode.Aligned };
        leftModes3 = new Bezier.ControlPointMode[]
        { Bezier.ControlPointMode.Aligned, Bezier.ControlPointMode.Aligned };
        rightModes1 = new Bezier.ControlPointMode[]
        { Bezier.ControlPointMode.Aligned, Bezier.ControlPointMode.Aligned };
        rightModes2 = new Bezier.ControlPointMode[]
        { Bezier.ControlPointMode.Aligned, Bezier.ControlPointMode.Aligned };
        rightModes3 = new Bezier.ControlPointMode[]
        { Bezier.ControlPointMode.Aligned, Bezier.ControlPointMode.Aligned };
    }
}