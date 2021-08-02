using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Nodes (ie. 'node'. Node is a reserved name, therefore Nodes) is smallest unit in node network hierarchy (Node network > Road >
/// Lane > Nodes). Nodes are linked. Each node contains information of previous and next node and to the neighboring nodes
/// on parallel lanes on both sides of its parent lane. The first node of a lane or lanes (in intersection: in this sense, word
/// 'lane' is ambiguous, meaning also driving line) is a connecting node. In that case node also contains information of starting
/// lanes. Node also contains a reference up in hierarchy to its parent lane. Node also contains various traffic rules related
/// information. Nodes are automatically generated along with lanes when roads are created using tool.
/// </summary>
[Serializable]
public class Nodes : MonoBehaviour
{
    /// <summary>
    /// Reference to previous linked node.
    /// </summary>
    [SerializeField]
    private Nodes inNode;
    /// <summary>
    /// Reference to the following linked node.
    /// </summary>
    [SerializeField]
    private Nodes outNode;
    /// <summary>
    /// Reference to neighbouring node on the left-side parallel lane.
    /// </summary>
    [SerializeField]
    private Nodes parallelLeft;
    /// <summary>
    /// Reference to neighbouring node on the right-side parallel lane.
    /// </summary>
    [SerializeField]
    private Nodes parallelRight;
    /// <summary>
    /// Reference to a target node on the left-side lane when doing a lane change from this lane.
    /// </summary>
    [SerializeField]
    private Nodes laneChangeLeft;
    /// <summary>
    /// Reference to a target node on the right-side lane when doing a lane change from this lane.
    /// </summary>
    [SerializeField]
    private Nodes laneChangeRight;
    /// <summary>
    /// A boolean if this is node is the first node of a lane / lanes (in intersection).
    /// </summary>
    [SerializeField]
    bool isLaneStartNode;
    /// <summary>
    /// An array of possible starting lanes.
    /// </summary>
    [SerializeField]
    Lane[] startingLanes;
    /// <summary>
    /// A boolean if this part of the lane is marked as a buslane.
    /// </summary>
    [SerializeField]
    bool isBusLane;

    /// <summary>
    /// Reference to the parent lane of this node.
    /// </summary>
    [SerializeField]
    Lane parentLane;

    /// <summary>
    /// Getter/setter for traffic size of this node. Traffic size is an attribute of its parent lane (ie. setting a new value
    /// affects the whole lane).
    /// </summary>
    public TrafficSize Traffic
    {
        get
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            return parentLane.Traffic;
        }
        set
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            parentLane.Traffic = value;
        }
    }
    /// <summary>
    /// Getter for nodes turn direction. Turn direction is its parent lane's attribute.
    /// </summary>
    public IntersectionDirection TurnDirection
    {
        get
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            return parentLane.TurnDirection;
        }
    }
    /// <summary>
    /// Getter for boolean if node is a lane starting node.
    /// </summary>
    public bool LaneStartNode
    {
        get
        {
            return isLaneStartNode;
        }
    }
    /// <summary>
    /// Getter for an array of lanes starting from this node (can be NULL).
    /// </summary>
    public Lane[] StartingLanes
    {
        get
        {
            return startingLanes;
        }
    }
    /// <summary>
    /// Getter/setter for boolean if this part of node's parent lane is marked as a buslane. 
    /// </summary>
    public bool BusLane
    {
        get
        {
            return isBusLane;
        }
        set
        {
            isBusLane = value;

        }
    }

    /// <summary>
    /// Getter/setter for neighboring node on parallel left-side lane.
    /// </summary>
    public Nodes ParallelLeft
    {
        get
        {
            return parallelLeft;
        }
        set
        {
            parallelLeft = value;
        }
    }
    /// <summary>
    /// Getter/setter for neighboring node on parallel right-side lane.
    /// </summary>
    public Nodes ParallelRight
    {
        get
        {
            return parallelRight;
        }
        set
        {
            parallelRight = value;
        }
    }
    /// <summary>
    /// Getter/setter for target node on parallel left-side lane when making a lane change. If lane change is not available
    /// or permitted, returns NULL.
    /// </summary>
    public Nodes LaneChangeLeft
    {
        get
        {
            return laneChangeLeft;
        }
        set
        {
            laneChangeLeft = value;
        }
    }
    /// <summary>
    /// Getter/setter for target node on parallel right-side lane when making a lane change. If lane change is not available
    /// or permitted, returns NULL.
    /// </summary>
    public Nodes LaneChangeRight
    {
        get
        {
            return laneChangeRight;
        }
        set
        {
            laneChangeRight = value;
        }
    }

    /// <summary>
    /// Getter/setter for speed limit of this node. Speed limit is an attribute of node's parent, ie. changes made affect all
    /// nodes of this lane.
    /// </summary>
    public SpeedLimits SpeedLimit
    {
        get
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            return parentLane.SpeedLimit;
        }
        set
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            parentLane.SpeedLimit = value;
        }
    }
    /// <summary>
    /// Getter/setter for reference to the previous node.
    /// </summary>
    public Nodes InNode
    {
        get
        {
            return inNode;
        }
        set
        {
            inNode = value;
        }
    }
    /// <summary>
    /// Getter/setter for reference to the following node.
    /// </summary>
    public Nodes OutNode
    {
        get
        {
            return outNode;
        }
        set
        {
            outNode = value;
        }
    }
    /// <summary>
    /// Getter/setter for node's parent lane.
    /// </summary>
    public Lane ParentLane
    {
        get
        {
            if (parentLane == null)
            {
                parentLane = transform.parent.GetComponent<Lane>();
            }
            return parentLane;
        }
        set
        {
            parentLane = value;
        }
    }

    // EDITOR STUFF

#if UNITY_EDITOR

    /// <summary>
    /// If node has an out-node, returns normalized direction vector from this node to its out-node.
    /// </summary>
    /// <param name="direction">Normalized direction vector.</param>
    /// <returns>Is direction information available?</returns>
    public bool GetDirectionOut(out Vector3 direction)
    {
        if (outNode == null)
        {
            direction = Vector3.forward;
            return false;
        }
        else
        {
            direction = (outNode.transform.position - transform.position).normalized;
            return true;
        }
    }
    /// <summary>
    /// If node has an in-node, returns normalized direction vector from this nodes in-node to this node.
    /// </summary>
    /// <param name="direction">Normalized direction vector.</param>
    /// <returns>Is direction information available?</returns>
    public bool GetDirectionIn(out Vector3 direction)
    {
        if (inNode == null)
        {
            direction = Vector3.forward;
            return false;
        }
        else
        {
            direction = (transform.position - inNode.transform.position).normalized;
            return true;
        }
    }
    /// <summary>
    /// Replaces given lane with another from starting lanes-array.
    /// </summary>
    /// <param name="laneToAdd">Added lane.</param>
    /// <param name="laneToRemove">Removed lane.</param>
    public void ReplaceLaneStart(Lane laneToAdd, Lane laneToRemove)
    {
        if (startingLanes == null)
        {
            startingLanes = new Lane[] { laneToAdd };
            isLaneStartNode = true;
        }
        else
        {
            for (int i = 0; i < startingLanes.Length; i++)
            {
                if (startingLanes[i] == laneToRemove)
                {
                    startingLanes[i] = null;
                    break;
                }
            }
            AddLaneStart(laneToAdd);
        }
    }
    /// <summary>
    /// Removes starting lanes-array and marks node as an ordinary node (not lane starting node).
    /// </summary>
    public void ClearStartNodes()
    {
        startingLanes = null;
        isLaneStartNode = false;
    }
    /// <summary>
    /// If lane is NOT in intersection, resets node's parent lane by retrieving the component from its parent gameobject and
    /// resets node's starting lanes list to contain only this lane.
    /// </summary>
    public void RemoveObsoleteLaneStarts()
    {
        if (parentLane.laneType != LaneType.ROAD_LANE)
        {
            return;
        }
        Lane parent = transform.parent.GetComponent<Lane>();
        startingLanes = new Lane[] { parent };
    }
    /// <summary>
    /// Adds a lane to starting lanes-array (also removes nulls from the array and checks if given lane is already in list).
    /// </summary>
    /// <param name="lane">Added lane.</param>
    public void AddLaneStart(Lane lane)
    {
        RemoveNullsFromLaneStarts();
        isLaneStartNode = true;
        if (startingLanes == null)
        {
            startingLanes = new Lane[] { lane };
        }
        else
        {
            bool isNew = true;
            {
                for (int i = 0; i < startingLanes.Length; i++)
                {
                    if (lane == startingLanes[i])
                    {
                        isNew = false;
                        break;
                    }
                }
            }
            if (isNew)
            {
                Array.Resize(ref startingLanes, startingLanes.Length + 1);
                startingLanes[startingLanes.Length - 1] = lane;
            }
        }
    }
    /// <summary>
    /// Removes nulls from starting lanes-array.
    /// </summary>
    private void RemoveNullsFromLaneStarts()
    {
        if (startingLanes == null)
        {
            return;
        }
        int nullIndex = 0;
        int startIndex = 0;
        while (nullIndex > -1)
        {
            nullIndex = -1;
            if (startIndex < startingLanes.Length)
            {
                for (int i = startIndex; i < startingLanes.Length; i++)
                {
                    if (startingLanes[i] == null)
                    {
                        nullIndex = i;
                        startIndex = i;
                        break;
                    }
                }
                if (nullIndex > -1)
                {
                    for (int i = nullIndex; i < startingLanes.Length - 1; i++)
                    {
                        startingLanes[i] = startingLanes[i + 1];
                    }
                    Array.Resize(ref startingLanes, startingLanes.Length - 1);
                }
            }
        }
        if (startingLanes.Length == 0)
        {
            isLaneStartNode = false;
        }
    }
    /// <summary>
    /// Unity's built-in reset function executes when component is reset. Initializes default values.
    /// </summary>
    private void Reset()
    {
        inNode = null;
        outNode = null;
        parallelLeft = null;
        parallelRight = null;
        laneChangeLeft = null;
        laneChangeRight = null;
        isLaneStartNode = false;
        startingLanes = null;
        isBusLane = false;
    }
#endif
}
