using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* Author: Henri Tainio
 * Source for Bezier content: Catlike coding
 * https://catlikecoding.com/unity/tutorials/curves-and-splines/
 */

/// <summary>
/// Intersection tool uses this enum for setting which nodes in intersection area are used as in-nodes
/// (the first nodes of each lane from incoming direction) and out-nodes (the last ones), the reast are
/// marked as not in use.
/// </summary>
public enum NodeInOut
{
    NotUsed,
    InNode,
    OutNode
}

/// <summary>
/// Tool script for creating node network intersections. The tool functionality is in custom inspector script. Add this component
/// to an empty gameobject to create an intersection.
/// </summary>
[Serializable]
public class IntersectionTool : MonoBehaviour
{
    /// <summary>
    /// Intersection's position.
    /// </summary>
    [SerializeField]
    private Vector3 centerPoint;
    /// <summary>
    /// Width value for framing the intersection area.
    /// </summary>
    [SerializeField]
    private float frameWidth;
    /// <summary>
    /// Height value for framing the intersection.
    /// </summary>
    [SerializeField]
    private float frameHeight;
    /// <summary>
    /// Default intersection frame size length value.
    /// </summary>
    private const float defaultFrameMeasure = 5f;
    /// <summary>
    /// A boolean if framing is completed and comfirmed.
    /// </summary>
    public bool framed;
    /// <summary>
    /// A boolean if start node settings are done.
    /// </summary>
    public bool allNodesSet;
    /// <summary>
    /// Parent nodenetwork gameobject for created intersection.
    /// </summary>
    public GameObject roadNetwork;
    /// <summary>
    /// An array for selecting and deselecting in- and out-nodes.
    /// </summary>
    public NodeInfo[] nodesInBox;
    /// <summary>
    /// A boolean if in- and out-nodes setup is completed.
    /// </summary>
    public bool nodesInBoxSet = false;
    /// <summary>
    /// Index of currently selected node in in/out node setting phase.
    /// </summary>
    [SerializeField]
    private int infoIndex = -1;
    /// <summary>
    /// Heperlines are used for setting up new entry directions to (and from) an intersection if there is no existing road.
    /// </summary>
    [SerializeField]
    public List<HelperLine> helperLines;
    /// <summary>
    /// If intersection is created over existing road sections, existing lanes are added to this list.
    /// </summary>
    [SerializeField]
    public List<ExistingLane> existingLanes;
    /// <summary>
    /// Index of currently selected existing lane.
    /// </summary>
    public int existingLaneIndex = 0;
    /// <summary>
    /// A boolean if the info about existing lanes is checked and confirmed by user.
    /// </summary>
    public bool existingLanesChecked = false;
    /// <summary>
    /// An array of created driving lines (in this context synonymous for 'lane').
    /// </summary>
    public SplineData[] createdSplines;
    /// <summary>
    /// An index of currently selected driving line.
    /// </summary>
    public int splineIndex = -1;
    /// <summary>
    /// A boolean if driving line splines are completed and confirmed.
    /// </summary>
    public bool splinesSet = false;
    /// <summary>
    /// Index of currently selected in-node index.
    /// </summary>
    public int inIndex = 0;
    /// <summary>
    /// Index of currently selected out-node.
    /// </summary>
    public int outIndex = 0;
    /// <summary>
    /// Getter / setter for the centerpoint of the intersection.
    /// </summary>
    public Vector3 CenterPoint
    {
        get
        {
            return centerPoint;
        }
        set
        {
            centerPoint = value;
        }
    }
    /// <summary>
    /// Getter / setter for intersection area frame width.
    /// </summary>
    public float FrameWidth
    {
        get
        {
            return frameWidth;
        }
        set
        {
            frameWidth = value;
        }
    }
    /// <summary>
    /// Getter / setter for intersection area frame height.
    /// </summary>
    public float FrameHeight
    {
        get
        {
            return frameHeight;
        }
        set
        {
            frameHeight = value;
        }
    }
    /// <summary>
    /// Returns the index of currently selected node during start in/out-node setting phase.
    /// </summary>
    public int GetInfoIndex
    {
        get
        {
            return infoIndex;
        }
    }
    /// <summary>
    /// If there are nodes inside intersection's framed area, sets index as 0 - otherwise sets it as -1.
    /// </summary>
    public void SetInfoIndexToFirst()
    {
        if (nodesInBox == null || nodesInBox.Length == 0)
        {
            infoIndex = -1;
        }
        else
        {
            infoIndex = 0;
        }
    }
    /// <summary>
    /// Sets same NodeInOut-enum value to all nodes inside intersection's framed area.
    /// This is used for resetting all nodeInfos as 'NotInUse'.
    /// </summary>
    /// <param name="inOut">NodeInOut enum value to be set to all nodes in framed intersection area.</param>
    public void SetInOutAll(NodeInOut inOut)
    {
        if (nodesInBox != null)
        {
            for (int i = 0; i < nodesInBox.Length; i++)
            {
                nodesInBox[i].inOut = inOut;
            }
        }
    }
    /// <summary>
    /// Sets given NodeInfo-enum value for the node of selected index (infoIndex) in nodesInBox array.
    /// </summary>
    /// <param name="inOut">NodeInOut enum value</param>
    public void SetInOut(NodeInOut inOut)
    {
        if (nodesInBox == null || nodesInBox.Length != 0)
        {
            nodesInBox[infoIndex].inOut = inOut;
        }
    }
    /// <summary>
    /// Adds given positive or negative value to index of currently selected framed node index.
    /// </summary>
    /// <param name="val">Value added to current index</param>
    public void MoveInfoIndex (int val)
    {
        if (nodesInBox == null || nodesInBox.Length == 0)
        {
            return;
        }
        int index = infoIndex + val;
        if (index < 0)
        {
            infoIndex = nodesInBox.Length - 1;
        }
        else if (index > nodesInBox.Length - 1)
        {
            infoIndex = 0;
        }
        else
        {
            infoIndex = index;
        }
    }
    /// <summary>
    /// Returns the number of nodes inside intersection's framed area.
    /// </summary>
    /// <returns>Number of nodes inside intersection's framed area.</returns>
    public int GetInfoSize()
    {
        if (nodesInBox == null)
        {
            return 0;
        }
        else
        {
            return nodesInBox.Length;
        }
    }
    /// <summary>
    /// From nodes inside intersection's framed area, finds the next NodeInfo with NodeInOut-enum value 'NotInUse' and 
    /// returns its index (otherwise returns -1).
    /// </summary>
    /// <returns>Index of next NodeInfo marked as 'not in use'.</returns>
    public int SelectNextAvailable()
    {
        if (infoIndex == nodesInBox.Length - 1)
        {
            bool found = false;
            for (int i = 0; i < nodesInBox.Length - 1; i++)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    found = true;
                    infoIndex = i;
                    break;
                }
                if (!found)
                {
                    infoIndex = -1;
                }
            }
        }
        else
        {
            bool found = false;
            int val = -1;
            for (int i = infoIndex + 1; i < nodesInBox.Length; i++)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    val = i;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                for (int i = 0; i <= infoIndex; i++)
                {
                    if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                    {
                        val = i;
                        break;
                    }
                }
            }
            infoIndex = val;
        }
        return infoIndex;
    }
    /// <summary>
    /// From nodes inside intersection's framed area, finds the previous NodeInfo with NodeInOut-enum value 'NotInUse' and 
    /// returns its index (otherwise returns -1).
    /// </summary>
    /// <returns>Index of previous NodeInfo marked as 'not in use'</returns>
    public int SelectPreviousAvailable()
    {
        if (infoIndex == 0)
        {
            bool found = false;
            for (int i = nodesInBox.Length - 1; i >= 0; i--)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    found = true;
                    infoIndex = i;
                    break;
                }
            }
            if (!found)
            {
                infoIndex = -1;
            }
        }
        else
        {
            bool found = false;
            int val = -1;
            for (int i = infoIndex - 1; i >= 0; i--)
            {
                if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                {
                    val = i;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                for (int i = nodesInBox.Length - 1; i >= infoIndex; i--)
                {
                    if (nodesInBox[i].inOut == NodeInOut.NotUsed)
                    {
                        val = i;
                        break;
                    }
                }
            }
            infoIndex = val;
        }
        return infoIndex;
    }
    /// <summary>
    /// Sets same NodeInfo-enum value to adjacent neighbouring lane nodes as is set to the NodeInfo of current index.
    /// </summary>
    public void SelectAdjacents()
    {
        NodeInOut inOut = nodesInBox[infoIndex].inOut;
        Nodes n = nodesInBox[infoIndex].node;
        if (n.ParallelRight)
        {
            Nodes r1 = n.ParallelRight;
            int index = FindNodeInfoIndex(r1);
            if (index > -1)
            {
                nodesInBox[index].inOut = inOut;
                if (r1.ParallelRight)
                {
                    Nodes r2 = r1.ParallelRight;
                    index = FindNodeInfoIndex(r2);
                    if (index > -1)
                    {
                        nodesInBox[index].inOut = inOut;
                    }
                }
            }
        }
        if (n.ParallelLeft)
        {
            Nodes l1 = n.ParallelLeft;
            int index = FindNodeInfoIndex(l1);
            if (index > -1)
            {
                nodesInBox[index].inOut = inOut;
                if (l1.ParallelLeft)
                {
                    Nodes l2 = l1.ParallelLeft;
                    index = FindNodeInfoIndex(l2);
                    if (index > -1)
                    {
                        nodesInBox[index].inOut = inOut;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Returns the index of given node in nodesInBox-array. This array keeps track of all nodes inside intersection's framed
    /// area and is used for setting the start and end nodes for existing lanes.
    /// </summary>
    /// <param name="node">Searched node</param>
    /// <returns>Index of the node in nodesInBox-array</returns>
    private int FindNodeInfoIndex(Nodes node)
    {
        int ind = -1;
        for (int i = 0; i < nodesInBox.Length; i++)
        {
            if (nodesInBox[i].node == node)
            {
                ind = i;
                break;
            }
        }
        return ind;
    }
    /// <summary>
    /// Returns curretly selected node inside intersection's framed area. Out-value is its NodeInOut-enum value.
    /// </summary>
    /// <param name="inOut">NodeInOut-enum value of currently selected node</param>
    /// <returns>Currently selected node</returns>
    public Nodes GetSelectedNodeInfo(out NodeInOut inOut)
    {
        if (infoIndex == -1)
        {
            inOut = NodeInOut.NotUsed;
            return null;
        }
        else
        {
            inOut = nodesInBox[infoIndex].inOut;
            return nodesInBox[infoIndex].node;
        }
    }
    /// <summary>
    /// Returns the currently selected in-node.
    /// </summary>
    /// <returns>Currently selected in-node</returns>
    public Nodes GetInNodeOfCurrentIndex()
    {
        Nodes n = null;
        int count = 0;
        for (int i = 0; i < nodesInBox.Length; i++)
        {
            if (nodesInBox[i].inOut == NodeInOut.InNode)
            {
                if (count == inIndex)
                {
                    n = nodesInBox[i].node;
                    break;
                }
                else
                {
                    count++;
                }
            }
        }
        return n;
    }
    /// <summary>
    /// Returns currently selected out-node.
    /// </summary>
    /// <returns>Currently selected out-node</returns>
    public Nodes GetOutNodeOfCurrentIndex()
    {
        Nodes n = null;
        int count = 0;
        for (int i = 0; i < nodesInBox.Length; i++)
        {
            if (nodesInBox[i].inOut == NodeInOut.OutNode)
            {
                if (count == outIndex)
                {
                    n = nodesInBox[i].node;
                    break;
                }
                else
                {
                    count++;
                }
            }
        }
        return n;
    }
    // Helperlines
    /// <summary>
    /// Resets and removes helperlines.
    /// </summary>
    public void RemoveHelperLines()
    {
        helperLines = new List<HelperLine>();
        inIndex = 0;
        outIndex = 0;
    }
    // Helperlines AND nodes-in-box
    /// <summary>
    /// Returns a sum of all in-and out-nodes and lists of their positions separately.
    /// </summary>
    /// <param name="ins">Positions of all in-nodes</param>
    /// <param name="outs">Positions of all out-nodes</param>
    /// <returns>Sum of all in-and out-nodes</returns>
    public int GetInOutPositions(out List<Vector3> ins, out List<Vector3> outs)
    {
        ins = new List<Vector3>();
        outs = new List<Vector3>();
        for (int i = 0; i < nodesInBox.Length; i++)
        {
     
            NodeInfo n = nodesInBox[i];
            if (n.inOut== NodeInOut.InNode)
            {
                ins.Add(n.node.transform.position);
            }
            else if (n.inOut == NodeInOut.OutNode)
            {
                outs.Add(n.node.transform.position);
            }
        }
        if (helperLines == null)
        {
            helperLines = new List<HelperLine>();
        }
        for (int i = 0; i < helperLines.Count; i++)
        {
            HelperLine h = helperLines[i];
            Vector3 p0 = h.startPoint;
            Vector3 dir = h.direction;
            float lenght = h.lenght;
            for (int j = 0; j < h.nodePoints.Count; j++)
            {
                Vector3 pnt = p0 + h.nodePoints[j] * lenght * dir;
                if (h.inOut[j] == NodeInOut.InNode)
                {
                    ins.Add(pnt);
                }
                else
                {
                    outs.Add(pnt);
                }
            }
        }
        return ins.Count + outs.Count;
    }
    /// <summary>
    /// Returns helperline index of node in given position.
    /// </summary>
    /// <param name="position">Position of a in/out-node</param>
    /// <returns>Node's helperline index</returns>
    public int GetHelperLineIndex(Vector3 position)
    {
        int index = 0;
        for (int i = 0; i < helperLines.Count; i++)
        {
            bool found = false;
            HelperLine h = helperLines[i];
            Vector3 p0 = h.startPoint;
            Vector3 dir = h.direction;
            float lenght = h.lenght;
            for (int j = 0; j < h.nodePoints.Count; j++)
            {
                Vector3 pnt = p0 + h.nodePoints[j] * lenght * dir;
                if (pnt == position)
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                index = i;
                break;
            }
        }
        return index;
    }
    // Existing lane
    /// <summary>
    /// Returns currently selected existing lane.
    /// </summary>
    /// <returns>Currently selected existing lane</returns>
    public ExistingLane GetCurrentExistingLane()
    {
        if (existingLanes==null || existingLanes.Count == 0)
        {
            return null;
        }
        return existingLanes[existingLaneIndex];
    }
    /// <summary>
    /// Adds given positive or negative value to current index of selected existing lane.
    /// </summary>
    /// <param name="val">Value added to current index</param>
    public void MoveExistingLaneIndex(int val)
    {
        int v = existingLaneIndex + val;
        if (v < 0)
        {
            existingLaneIndex = existingLanes.Count - 1;
        }
        else if (v > existingLanes.Count - 1)
        {
            existingLaneIndex = 0;
        }
        else
        {
            existingLaneIndex = v;
        }
    }
    /// <summary>
    /// Sets the currently selected existing lane.
    /// </summary>
    /// <param name="ex">Value to be set to the existing lanes-list in current index</param>
    public void SetCurrentExistingLane(ExistingLane ex)
    {
        existingLanes[existingLaneIndex] = ex;
    }
    /// <summary>
    /// Returns existing lane with currently selected in-node index
    /// </summary>
    /// <returns>Existing lane with currently selected in-node index</returns>
    public ExistingLane GetExistingLaneWithInNode()
    {
        ExistingLane ex = null;
        for (int i = 0; i < existingLanes.Count; i++)
        {
            if (existingLanes[i].inNodeIndex == inIndex)
            {
                ex = existingLanes[i];
                break;
            }
        }
        return ex;
    }
    /// <summary>
    /// Returns the number of unconfirmed existing lanes. User have to confirm existing lanes before proceeding to the next
    /// phase with intersection tool.
    /// </summary>
    /// <returns>Number of unconfirmed existing lanes</returns>
    public int GetUnconfirmedExistingLaneCount()
    {
        int v = 0;
        for (int i=0; i < existingLanes.Count; i++)
        {
            if (!existingLanes[i].confirmed)
            {
                v++;
            }
        }
        return v;
    }
    // Spline data
    /// <summary>
    /// Calculates positions when spline handles are moved in sceneview.
    /// </summary>
    /// <param name="splineIndex">Manipulated spline</param>
    /// <param name="pointIndex">Manipulated spline node index</param>
    /// <param name="point">New position</param>
    public void SetSplinePoint(int splineIndex, int pointIndex, Vector3 point)
    {
        SplineData sp = createdSplines[splineIndex];
        if (pointIndex % 3 == 0)
        {
            Vector3 delta = point - sp.points[pointIndex];
            if (pointIndex > 0)
            {
                sp.points[pointIndex - 1] += delta;
            }
            if (pointIndex + 1 < sp.points.Length)
            {
                sp.points[pointIndex + 1] += delta;
            }
        }
        //***********
        sp.points[pointIndex] = point;
        EnforceMode(splineIndex, pointIndex);
    }
    /// <summary>
    /// Add given positive or negative value to current index of selected spline.
    /// </summary>
    /// <param name="val">Added value</param>
    public void MoveSplineIndex(int val)
    {
        int v = splineIndex + val;
        if (v < 0 && createdSplines.Length > 0)
        {
            splineIndex = createdSplines.Length - 1;
        }
        else if (v > createdSplines.Length - 1)
        {
            splineIndex = 0;
        }
        else
        {
            splineIndex = v;
        }
    }
    /// <summary>
    /// Adds a new bezier segment to currently selected spline.
    /// </summary>
    public void AddSegmentToCurrentSpline()
    {
        SplineData sd = createdSplines[splineIndex];
        Vector3 point = sd.points[sd.points.Length - 1];
        float length;
        if (frameHeight < frameWidth)
        {
            length = frameHeight * 0.1f;
        }
        else
        {
            length = frameWidth * 0.1f;
        }
        Vector3 dir = GetSegmentedDirection((sd.points.Length - 1) % 3, 1f);
        Array.Resize(ref sd.points, sd.points.Length + 3);
        point += length * dir;
        sd.points[sd.points.Length - 3] = point;
        point += length * dir;
        sd.points[sd.points.Length - 2] = point;
        point += length * dir;
        sd.points[sd.points.Length - 1] = point;

        Array.Resize(ref sd.modes, sd.modes.Length + 1);
        sd.modes[sd.modes.Length - 1] = sd.modes[sd.modes.Length - 2];
        EnforceMode(splineIndex, sd.points.Length - 4);
    }
    /// <summary>
    /// Attach the end-node of curretly selected spline to the currently selected out-node.
    /// </summary>
    public void ConnectSplineToOutNode()
    {
        if (splineIndex < 0)
        {
            Debug.Log("Spline index " + splineIndex);
            return;
        }
        Nodes n = GetOutNodeOfCurrentIndex();

        SplineData sd = createdSplines[splineIndex];
        if (n != null)
        {
            sd.endNode = n;
            sd.points[sd.points.Length - 1] = n.transform.position;
        }
        else
        {
            List<Vector3> ins, outs;
            GetInOutPositions(out ins, out outs);
            sd.points[sd.points.Length - 1] = outs[outIndex];
        }
        sd.endPointSet = true;
    }
    /// <summary>
    /// When splines are set, initializes a SplineData-array for created splines.
    /// </summary>
    public void SetSegmentArrays()
    {
        if (createdSplines == null)
        {
            return;
        }
        for (int i = 0; i < createdSplines.Length; i++)
        {
            SplineData sd = createdSplines[i];
            int segs = (sd.points.Length - 1) / 3;
            sd.segmentNodes = new int[segs];
            for (int j = 0; j < segs; j++)
            {
                sd.segmentNodes[j] = 0;
            }
        }
    }
    /// <summary>
    /// Returns a list of node positions of the spline of given index.
    /// </summary>
    /// <param name="splineInd">Spline index</param>
    /// <returns>List of node positions</returns>
    public List<Vector3> GetNodePositionInBetweenEndPoints (int splineInd)
    {
        List<Vector3> pnts = new List<Vector3>();
        SplineData sd = createdSplines[splineInd];
        for (int seg = 0; seg < sd.segmentNodes.Length; seg++)
        {
            int nodesOnSeg = sd.segmentNodes[seg];
            for (int pnt = 1; pnt <= nodesOnSeg; pnt++)
            {
                Vector3 pos = GetSegmentedPoint(splineInd, seg, (float)pnt / nodesOnSeg);
                pnts.Add(pos);
            }
        }
        // Remove the last one
        pnts.RemoveAt(pnts.Count - 1);
        return pnts;
    }
    /// <summary>
    /// Returns a list of positons of the nodes of currently selected spline and a list of positions of the nodes
    /// of all the rest of the splines. These are used for visualization.
    /// </summary>
    /// <param name="current">Positions of currently selected spline's nodes</param>
    /// <param name="other">Positions of unselected splines' nodes</param>
    public void GetSegmentNodePositions(out List<Vector3> current, out List<Vector3> other)
    {
        current = new List<Vector3>();
        other = new List<Vector3>();
        if (createdSplines == null)
        {
            return;
        }
        for (int i = 0; i < createdSplines.Length; i++)
        {
            SplineData sd = createdSplines[i];
            Vector3 pos = sd.points[0];
            if (i == splineIndex)
            {
                current.Add(pos);
            }
            else
            {
                other.Add(pos);
            }
            for (int seg = 0; seg < sd.segmentNodes.Length; seg++)
            {
                int nodesOnSeg = sd.segmentNodes[seg];
                for (int pnt = 1; pnt <= nodesOnSeg; pnt++)
                {
                    pos = GetSegmentedPoint(i, seg, (float)pnt / nodesOnSeg);
                    if (i == splineIndex)
                    {
                        current.Add(pos);
                    }
                    else
                    {
                        other.Add(pos);
                    }
                }
            }
        }
    }
    /// <summary>
    /// A boolean if end points of all splines are connected out-nodes.
    /// </summary>
    /// <returns>Are end points of all splines connected to out-nodes?</returns>
    public bool AllSplineEndPointsConnected()
    {
        bool connected = true;
        if (createdSplines == null)
        {
            return connected;
        }
        for (int i = 0; i < createdSplines.Length; i++)
        {
            SplineData sd = createdSplines[i];
            if (!sd.endPointSet)
            {
                connected = false;
                break;
            }
        }
        return connected;
    }
    /// <summary>
    /// A boolean if node counts are set to all splines.
    /// </summary>
    /// <returns>Are node counts set to all splines?</returns>
    public bool NodesOnAllSegments()
    {
        bool isTrue = true;
        if (createdSplines == null)
        {
            return isTrue;
        }
        for (int i = 0; i < createdSplines.Length; i++)
        {
            SplineData sd = createdSplines[i];
            bool b = true;
            for (int seg = 0; seg < sd.segmentNodes.Length; seg++)
            {
                if (sd.segmentNodes[seg]==0)
                {
                    b = false;
                    break;
                }
            }
            if (b == false)
            {
                isTrue = false;
                break;
            }
        }
        return isTrue;
    }
    /// <summary>
    /// Returns a fraction position (0-1) on selected segment on selected spline.
    /// </summary>
    /// <param name="splineInd">Spline index</param>
    /// <param name="segment">Segment index</param>
    /// <param name="fraction">Fraction from the start of selected segment to its end</param>
    /// <returnsPosition on the spline</returns>
    private Vector3 GetSegmentedPoint(int splineInd, int segment, float fraction)
    {
        SplineData sd = createdSplines[splineInd];
        int i = segment * 3;
        return Bezier.GetPoint(
            sd.points[i], sd.points[i + 1], sd.points[i + 2], sd.points[i + 3], fraction);
    }
    /// <summary>
    /// Spline tangent direction at the selected position (0-1) on selected segment of the currently selected spline.
    /// </summary>
    /// <param name="segment">Index of segment</param>
    /// <param name="fraction">Fraction of the segment (0-1)</param>
    /// <returns>Spline's tangent direction at given position</returns>
    private Vector3 GetSegmentedDirection(int segment, float fraction)
    {

        return GetSegmentedVelocity(segment, fraction).normalized;
    }
    /// <summary>
    /// Bezier velocity at given position of currently selected spline.
    /// </summary>
    /// <param name="segment">index of spline segment</param>
    /// <param name="fraction">fraction (0-1) of segment</param>
    /// <returns>bezier velocity</returns>
    private Vector3 GetSegmentedVelocity(int segment, float fraction)
    {
        SplineData sd = createdSplines[splineIndex];
        int i = segment * 3;
        return transform.TransformPoint(
            Bezier.GetFirstDerivative(sd.points[i], sd.points[i + 1],
            sd.points[i + 2], sd.points[i + 3], fraction))
            - transform.position;
    }
    /// <summary>
    /// Removes currently selected spline.
    /// </summary>
    public void RemoveCurrentSpline()
    {
        if (createdSplines == null || createdSplines.Length == 0)
        {
            return;
        }
        for (int i = splineIndex; i < createdSplines.Length - 1; i++)
        {
            createdSplines[i] = createdSplines[i + 1];
        }
        Array.Resize(ref createdSplines, createdSplines.Length - 1);
        splineIndex--;
        if (splineIndex < 0 && createdSplines.Length > 0)
        {
            splineIndex = createdSplines.Length - 1;
        }
    }
    /// <summary>
    /// Enforces the effects of changed spline handle position to affected nodes.
    /// </summary>
    /// <param name="splineIndex">Index of handled spline</param>
    /// <param name="pointIndex">Index of manipulated handle</param>
    private void EnforceMode(int splineIndex, int pointIndex)
    {
        SplineData sp = createdSplines[splineIndex];
        int modeIndex = (pointIndex + 1) / 3;
        Bezier.ControlPointMode mode = sp.modes[modeIndex];
        // We don't enforce if we are at end points or the current mode is set to 'FREE'.
        if (mode == Bezier.ControlPointMode.Free || modeIndex == 0 || modeIndex == sp.modes.Length - 1)
        {
            return;
        }
        int middleIndex = modeIndex * 3;
        int fixedIndex, enforcedIndex;
        if (pointIndex <= middleIndex)
        {
            fixedIndex = middleIndex - 1;
            if (fixedIndex < 0)
            {
                fixedIndex = sp.points.Length - 2;
            }
            enforcedIndex = middleIndex + 1;
            if (enforcedIndex >= sp.points.Length)
            {
                enforcedIndex = 1;
            }
        }
        else
        {
            fixedIndex = middleIndex + 1;
            if (fixedIndex >= sp.points.Length)
            {
                fixedIndex = 1;
            }
            enforcedIndex = middleIndex - 1;
            if (enforcedIndex < 0)
            {
                enforcedIndex = sp.points.Length - 2;
            }
        }

        Vector3 middle = sp.points[middleIndex];
        Vector3 enforcedTangent = middle - sp.points[fixedIndex];
        if (mode == Bezier.ControlPointMode.Aligned)
        {
            enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, sp.points[enforcedIndex]);
        }
        sp.points[enforcedIndex] = middle + enforcedTangent;
    }

    /// <summary>
    /// Default values are applied when this component is resets.
    /// </summary>
    public void Reset()
    {
        CenterPoint = transform.position;
        FrameWidth = defaultFrameMeasure;
        FrameHeight = defaultFrameMeasure;
        framed = false;
    }
}

/// <summary>
/// A helper data container class. Intersection-tool uses this to define which nodes in its vincinity are lanes' in-and -out-nodes.
/// </summary>
[Serializable]
public class NodeInfo
{
    /// <summary>
    /// Reference to a node.
    /// </summary>
    public Nodes node;
    /// <summary>
    /// Nodes role in intersection (in/out-node or irrelevant).
    /// </summary>
    public NodeInOut inOut;
}
/// <summary>
/// A helper data container class. Intersection-tool uses helper lines to define new entry directions if there are no existing
/// roads.
/// </summary>
[Serializable]
public class HelperLine
{
    /// <summary>
    /// Start point of this helper line. Lanes' start/end point nodes are set along this line.
    /// </summary>
    public Vector3 startPoint;
    /// <summary>
    /// Helper line's direction from start point.
    /// </summary>
    public Vector3 direction;
    /// <summary>
    /// Helper line's length.
    /// </summary>
    public float lenght;
    /// <summary>
    /// List of lanes' start/end points' positions along this helper line as fraction between its start and end (0-1).
    /// </summary>
    public List<float> nodePoints;
    /// <summary>
    /// List of lanes' start/end points as NodeInOut-data. 
    /// </summary>
    public List<NodeInOut> inOut;
}

/// <summary>
/// A helper data class. Intersection-tool uses this class to store information of existing lanes during the creation process.
/// </summary>
[Serializable]
public class ExistingLane
{
    /// <summary>
    /// A boolean if existing lane runs through the intersection as one of its lanes. There
    /// is also an option that existing lane data only consist of a starting or and end point for a lane.
    /// </summary>
    public bool isLane;
    /// <summary>
    /// A list of nodes on this lane.
    /// </summary>
    public List<Nodes> nodes;
    /// <summary>
    /// Index of the intersection in-node on lanes nodes-list.
    /// </summary>
    public int inNodeIndex;
    /// <summary>
    /// Index of the intersection out-node on lanes nodes-list.
    /// </summary>
    public int outNodeIndex;
    /// <summary>
    /// Lane's turn direction.
    /// </summary>
    public IntersectionDirection turnDirection;
    /// <summary>
    /// Lane's traffic size.
    /// </summary>
    public TrafficSize traffic = TrafficSize.Average;
    /// <summary>
    /// Lane's speed limit.
    /// </summary>
    public SpeedLimits speedLimit = SpeedLimits.KMH_30;
    /// <summary>
    /// A boolean if lane is confirmed by the user.
    /// </summary>
    public bool confirmed;
    /// <summary>
    /// Lane's type, mainly used for selecting yielding status in inspector.
    /// </summary>
    public LaneType laneType = LaneType.INTERSECTION_LANE_RIGHT_OF_WAY;
}
/// <summary>
/// A helper data class. Intersection tool uses this class to store information of created lanes (in this context synonymous to
/// driving lines).
/// </summary>
[Serializable]
public class SplineData
{
    /// <summary>
    /// Spline's bezier control point positions.
    /// </summary>
    public Vector3[] points;
    /// <summary>
    /// Spline's bezier control point modes. Mode affects how each control point behaves.
    /// </summary>
    public Bezier.ControlPointMode[] modes;
    /// <summary>
    /// Spline's start point node.
    /// </summary>
    public Nodes startNode;
    /// <summary>
    /// Spline's end point node.
    /// </summary>
    public Nodes endNode;
    /// <summary>
    /// A boolean if spline's end point is connected to an out-node.
    /// </summary>
    public bool endPointSet;
    /// <summary>
    /// Lane's turn direction.
    /// </summary>
    public IntersectionDirection turnDirection;
    /// <summary>
    /// Lane's traffic size.
    /// </summary>
    public TrafficSize traffic = TrafficSize.Average;
    /// <summary>
    /// Lane's speed limit.
    /// </summary>
    public SpeedLimits speedLimit = SpeedLimits.KMH_30;
    /// <summary>
    /// Number of nodes on each segment of the spline.
    /// </summary>
    public int[] segmentNodes;
    /// <summary>
    /// Type of lane, used for settin lane's yielding status
    /// </summary>
    public LaneType laneType = LaneType.INTERSECTION_LANE_RIGHT_OF_WAY;
}