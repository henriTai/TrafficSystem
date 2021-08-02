using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Author: Henri Tainio

/// <summary>
/// A menu tool for updating crosswalk encounter information in all Lane-objects.
/// </summary>
public class CrosswalkUpdateTool : MonoBehaviour
{
    /// <summary>
    /// Menu item Virtulanssi / Update Crosswalks.
    /// </summary>
    [MenuItem("Virtulanssi/Update Crosswalks")]
    public static void UpdateCrosswalks()
    {
        Road[] roads = FindObjectsOfType<Road>();
        Crosswalk[] allCrosswalks = FindObjectsOfType<Crosswalk>();
        for (int i = 0; i < roads.Length; i++)
        {
            Lane[] lanes = roads[i].GetComponentsInChildren<Lane>();
            for (int j = 0; j < lanes.Length; j++)
            {
                SetCrosswalksLane(roads[i], lanes[j], allCrosswalks);
            }
        }
    }
    /// <summary>
    /// Sets crosswalk encounter information to a lane.
    /// </summary>
    /// <param name="r">Lane's parent Road object.</param>
    /// <param name="lane">Lane object.</param>
    /// <param name="cws">An array of all crosswalks.</param>
    private static void SetCrosswalksLane(Road r, Lane lane, Crosswalk[] cws)
    {
        lane.crosswalkEncounters = null;
        List<Crosswalk> crosswalks = new List<Crosswalk>();
        for (int i = 0; i < cws.Length; i++)
        {
            Crosswalk c = cws[i];
            for (int j = 0; j < c.lanes.Length; j++)
            {
                if (c.lanes[j] == lane)
                {
                    crosswalks.Add(c);
                    break;
                }
            }
        }
        if (crosswalks.Count == 0)
        {
            return;
        }
        
        // this is done to check possible U-turns (might cross same crosswalk twice)
        List<Crosswalk> cwEncounters = new List<Crosswalk>();
        List<int> indexes = new List<int>();
        for (int i = 0; i < crosswalks.Count; i++)
        {
            Crosswalk c = crosswalks[i];
            for (int j = 0; j < c.lanes.Length; j++)
            {
                Lane l = c.lanes[j];
                if ( l != lane)
                {
                    continue;
                }
                if (indexes.Count == 0)
                {
                    indexes.Add(j);
                    cwEncounters.Add(c);
                }
                else
                {
                    Nodes nodeToAdd = c.beforeNodes[j];
                    bool found = false;
                    int indexToAdd = 0;
                    for (int k = 0; k < indexes.Count; k++)
                    {
                        Nodes toCompare = cwEncounters[k].beforeNodes[indexes[k]];
                        bool isBefore = IsNodeBefore(l, nodeToAdd, toCompare);
                        if (isBefore)
                        {
                            indexToAdd = k;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        indexes.Insert(indexToAdd, j);
                        cwEncounters.Insert(indexToAdd, c);
                    }
                    else
                    {
                        indexes.Add(j);
                        cwEncounters.Add(c);
                    }
                    for (int x = 0; x < cwEncounters.Count; x++)
                    {
                    }
                }
            }
        }
        //
        CrosswalkEncounter[] encounters = new CrosswalkEncounter[cwEncounters.Count];
        for (int i = 0; i < cwEncounters.Count; i++)
        {
            CrosswalkEncounter e = new CrosswalkEncounter();
            e.crosswalk = cwEncounters[i];
            e.index = indexes[i];
            encounters[i] = e;
        }
        lane.crosswalkEncounters = encounters;
        Debug.Log("Crosswalks updated");
    }
    /// <summary>
    /// Compares if a node is before another node on lane.
    /// </summary>
    /// <param name="l">A lane the nodes are supposed to be on.</param>
    /// <param name="newNode">The node in question.</param>
    /// <param name="nodeToCompare">The compared node.</param>
    /// <returns>Is the node in question before the compared node on the lane?</returns>
    private static bool IsNodeBefore(Lane l, Nodes newNode, Nodes nodeToCompare)
    {
        bool compareNodeFound = false;
        Nodes[] ns = l.nodesOnLane;
        for (int i = 0; i < ns.Length; i++)
        {
            Nodes n = l.nodesOnLane[i];
            if (n == newNode)
            {
                if (compareNodeFound)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            if (n == nodeToCompare)
            {
                compareNodeFound = true;
            }
        }
        Debug.Log("Search failed");
        return false;
    }
}
