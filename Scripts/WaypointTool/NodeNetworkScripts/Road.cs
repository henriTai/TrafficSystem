using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Author: Henri Tainio

/// <summary>
/// In node network hierarchy, the highest tier road network consists of roads, which in turn consist of lanes, and they in turn
/// consist of nodes. Road-entities are either plain road sections from point A to B with one or more lanes or they can be
/// intersections.
/// </summary>
[Serializable]
public class Road : MonoBehaviour
{
    /// <summary>
    /// A boolean if lanes are drawn in scene view when object is selected.
    /// </summary>
    public bool showLanes;
    /// <summary>
    /// Entry directions are grouped as passages. Plain road section from point A to B has two passages; T-intersection 3
    /// passages and so on. This information is used for controlling traffic flow in intersections.
    /// </summary>
    [SerializeField]
    public InOutPassageWays[] passages;

    // for setting start positions
    /// <summary>
    /// Guidelanes are used for setting line where cars stop before entering an intersection. By default, the first node of
    /// the first in-lane of each passage is selected as stop point, but this can be set manually in inspector. 
    /// </summary>
    [SerializeField]
    public Lane[] guideLane;
    /// <summary>
    /// Index array of which lane of each passage is selected as a guidelane.
    /// </summary>
    [SerializeField]
    public int[] selectedLaneIndex;
    /// <summary>
    /// Nodes of each passage currently selected for setting position of line where vehicles stop before entering an intersection.
    /// </summary>
    [SerializeField]
    public Nodes[] targetNode;
    // can be negative - meaning that start position is somewhere along the previous lane
    /// <summary>
    /// Array of indexes. Selected lane index for each passage determines which lane is used for setting a line where vehicles
    /// stop before entering an intersection from given passage direction. Node index determines which node is used as position.
    /// NOTE: Index CAN be negative, meaning node doesn't belong to lane in intersection but the lane of the previous road-section.
    /// This is necessary, because sometimes there may occur need of placing stopping point further away from the intersection
    /// that has been planned (after placing a crosswalk, for example).
    /// </summary>
    [SerializeField]
    public int[] nodeIndex;
    // value between 0 - 1
    /// <summary>
    /// An array for each passageway. These values adjust the placement of stopping lines (for vehicles before entering an
    /// intersection) between nodes. The value range is between 0 and 1.
    /// </summary>
    [SerializeField]
    public float[] positionBetweenNodes;
    /// <summary>
    /// By default, the direction of stopping line for each passage is determined by their guidelanes direction. Angle adjustment
    /// values are used for manually fine-tuning lines orientation.
    /// </summary>
    [SerializeField]
    public float[] angleAdjustment;

#if UNITY_EDITOR

    /// <summary>
    /// Unity's built-in function, executed when component is reset.
    /// </summary>
    private void Reset()
    {
        showLanes = true;
        passages = new InOutPassageWays[0];
    }

    // intersection inspector calls this
    /// <summary>
    /// Separates the section of the road, starting from node after given index, into a new road object. This function is called
    /// when an intersection is cutting a road in half to form a new road of the end section.
    /// </summary>
    /// <param name="index">Index of node. The portion of the road after this node is separated into a new road.</param>
    /// <param name="newRoad">New road object created from the end section.</param>
    public void SplitRoadAfterNode(int index, out GameObject newRoad)
    {
        Lane[] childLanes = GetComponentsInChildren<Lane>();
        if (index >= childLanes[0].nodesOnLane.Length - 1 || index == 0)
        {
            newRoad = null;
            return;
        }
        GameObject roadObject = new GameObject(gameObject.name);
        roadObject.AddComponent<Road>();
        string nameNumberString = "x";
        for (int i = 0; i < childLanes.Length; i++)
        {
            if (childLanes[i].isRightLane)
            {
                roadObject.transform.position = childLanes[i].nodesOnLane[index + 1].transform.position;
                nameNumberString = NameNumberEndString(childLanes[i].nodesOnLane[index + 1]);
                break;
            }
            if (i == childLanes.Length - 1)
            {
                roadObject.transform.position = childLanes[i].nodesOnLane[childLanes[i].nodesOnLane.Length - 2 - index].transform.position;
                nameNumberString = NameNumberEndString(childLanes[i].nodesOnLane[childLanes[i].nodesOnLane.Length - 2 - index]);
            }
        }
        roadObject.name = roadObject.name + nameNumberString;

        for (int i = 0; i < childLanes.Length; i++)
        {
            Lane chLane = childLanes[i];
            string laneName = NewLaneName(chLane, nameNumberString);
            GameObject laneObject = new GameObject(laneName);
            Lane l = laneObject.AddComponent<Lane>();

            l.Traffic = chLane.Traffic;
            l.laneType = chLane.laneType;

            l.SpeedLimit = chLane.SpeedLimit;
            l.TurnDirection = chLane.TurnDirection;
            l.pointToPointLine = chLane.pointToPointLine;
            l.drawAllLanes = chLane.drawAllLanes;
            l.isRightLane = chLane.isRightLane;

            int count = chLane.nodesOnLane.Length - index - 1;
            Nodes[] newNodes = new Nodes[count];

            if (chLane.isRightLane)
            {
                for (int j = 0; j < count; j++)
                {
                    newNodes[j] = chLane.nodesOnLane[index + j + 1];
                    newNodes[j].transform.parent = laneObject.transform;
                    newNodes[j].ParentLane = l;
                }
            }
            else
            {
                for (int j = 0; j < count; j++)
                {
                    newNodes[j] = chLane.nodesOnLane[j];
                    newNodes[j].transform.parent = laneObject.transform;
                    newNodes[j].ParentLane = l;
                }
                for (int j = count; j < chLane.nodesOnLane.Length; j++)
                {
                    chLane.nodesOnLane[j - count] = chLane.nodesOnLane[j];
                }
            }
            l.nodesOnLane = newNodes;
            l.nodesOnLane[0].ReplaceLaneStart(l, chLane);

            Array.Resize(ref chLane.nodesOnLane, index + 1);
            laneObject.transform.parent = roadObject.transform;
            roadObject.transform.parent = gameObject.transform.parent;

        }
        newRoad = roadObject;
    }
    /// <summary>
    /// A helper function for generating a name suffix for a new road object when a road is split.
    /// </summary>
    /// <param name="n">The first node of the latter part of the road when road is split.</param>
    /// <returns>Suffix for a new road's name.</returns>
    private string NameNumberEndString (Nodes n)
    {
        char c = '_';
        string[] subs = n.name.Split(c);
        if (subs == null || subs.Length == 0)
        {
            return "x";
        }
        return "_n" + subs[subs.Length - 1];
    }
    /// <summary>
    /// A helper function for generating a name for a new lane object when a road is split.
    /// </summary>
    /// <param name="l">Lane that is split.</param>
    /// <param name="toAdd">Generated suffix that will be added to the lane's name.</param>
    /// <returns>Name for a new lane object.</returns>
    private string NewLaneName (Lane l, string toAdd)
    {
        char c = '_';
        string[] subs = l.name.Split(c);
        string newName = "";
        for (int i = 0; i < subs.Length; i++)
        {
            if (i == subs.Length - 1)
            {
                newName += toAdd;
                newName += "_";
                newName += subs[i];
            }
            else
            {
                newName += subs[i];
                if (i < subs.Length - 2)
                {
                    newName += "_";
                }
            }
        }
        return newName;
    }
    /// <summary>
    /// This function is a tool for developer use. It can be called from road network's inspector for all road objects to
    /// check that their starting lane arrays are up to date. By now, there has not been issues.
    /// </summary>
    /// <returns>Number of obsolate entries removed from starting lane arrays.</returns>
    public int RemoveFalseLaneStarts()
    {
        int falseLaneStarts = 0;
        Lane[] allLanes = GetComponentsInChildren<Lane>();
        if (allLanes == null)
        {
            return 0;
        }
        for (int i = 0; i < allLanes.Length; i++)
        {
            Nodes n = allLanes[i].nodesOnLane[0];

            if (allLanes[i].laneType != LaneType.ROAD_LANE)
            {
                continue;
            }
            else
            {
                if (n.StartingLanes.Length > 1)
                {
                    falseLaneStarts += n.StartingLanes.Length - 1;
                    n.RemoveObsoleteLaneStarts();
                    Debug.Log("node " + n.name + ", lane: " + n.ParentLane);
                }
            }
        }
        return falseLaneStarts;
    }
#endif
}

/// <summary>
/// A helper class for roads. For plain road, there are 2 entry directions (2 InOutPassageWays), T-intersection has 3
/// InOutPassageWays and so on. InOutPassageWays group info of in-lanes and out-lanes of given entry direction in arrays.
/// This information is used in traffic control in intersections. For plain roads, InOutPassageWays-information is generated
/// automatically, for intersections, this must be done manually ATM.
/// </summary>
[Serializable]
public class InOutPassageWays
{
    /// <summary>
    /// Array of lanes coming in from this entry direction.
    /// </summary>
    [SerializeField]
    public Lane[] inLanes;
    /// <summary>
    /// Array of lanes going out from this entry direction.
    /// </summary>
    [SerializeField]
    public Lane[] outLanes;
}