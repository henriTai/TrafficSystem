using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Author: Henri Tainio

/// <summary>
/// Custom inspector script for Lane-objects.
/// </summary>
[CustomEditor(typeof(Lane))]
public class LaneInspector : Editor
{
    /// <summary>
    /// A variable for target lane-object.
    /// </summary>
    public Lane lane;
    /// <summary>
    /// A list of Vector3 positions for drawing node-to-node visualization of the lane.
    /// </summary>
    List<Vector3> lines;
    /// <summary>
    /// A list of Vector3 positions for drawing node-to-node visualization of all the rest of the lanes of the same road object.
    /// </summary>
    List<Vector3> otherLines;
    /// <summary>
    /// A boolean if node-to-node lane visualization is on.
    /// </summary>
    bool showNodesOnLane;
    /// <summary>
    /// Data of crosswalks along target lane's path.
    /// </summary>
    Crosswalk[] crosswalks;
    bool editingYieldStatus = false;

    /// <summary>
    /// Unity's built-in function, determines inspector view for objects of its target's type.
    /// </summary>
    public override void OnInspectorGUI()
    {
        Undo.RecordObject(lane, lane.name + " has changed");
        EditorGUILayout.LabelField(lane.gameObject.name, EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        if (lane.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
        {
            if (editingYieldStatus == false)
            {
                EditorGUILayout.LabelField("Lane type: Right of way (INTERSECTION)");
                if (GUILayout.Button("EDIT"))
                {
                    editingYieldStatus = true;
                }
            }
            else
            {
                EditorGUILayout.LabelField("Change status to YIELDING?", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("YES"))
                {
                    lane.laneType = LaneType.INTERSECTION_LANE_YIELDING;
                }
                if (GUILayout.Button("BACK"))
                {
                    editingYieldStatus = false;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else if (lane.laneType == LaneType.INTERSECTION_LANE_YIELDING)
        {
            if (editingYieldStatus == false)
            {
                EditorGUILayout.LabelField("Lane type: Yielding (INTERSECTION)");
                if (GUILayout.Button("EDIT"))
                {
                    editingYieldStatus = true;
                }
            }
            else
            {
                EditorGUILayout.LabelField("Change status to RIGHT-OF-WAY?", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("YES"))
                {
                    lane.laneType = LaneType.INTERSECTION_LANE_RIGHT_OF_WAY;
                }
                if (GUILayout.Button("BACK"))
                {
                    editingYieldStatus = false;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.LabelField("Lane type: " + lane.laneType);
            EditorGUILayout.Separator();
        }
        lane.Traffic = (TrafficSize)EditorGUILayout.EnumPopup("Traffic", lane.Traffic);
        lane.SpeedLimit = (SpeedLimits)EditorGUILayout.EnumPopup("Speed limit", lane.SpeedLimit);
        //lane.LaneYield = (DriverYield)EditorGUILayout.EnumPopup("Driver yield", lane.LaneYield);
        lane.TurnDirection = (IntersectionDirection)EditorGUILayout.EnumPopup("Turn direction", lane.TurnDirection);
        EditorGUILayout.Separator();

        bool drawPoints = lane.pointToPointLine;
        drawPoints = EditorGUILayout.ToggleLeft("Draw point-to-point?", drawPoints);
        if (drawPoints != lane.pointToPointLine)
        {
            lane.pointToPointLine = drawPoints;
            SceneView.RepaintAll();
        }
        drawPoints = lane.drawAllLanes;
        drawPoints = EditorGUILayout.ToggleLeft("Draw other lines?", drawPoints);
        if (drawPoints != lane.drawAllLanes)
        {
            lane.drawAllLanes = drawPoints;
            SceneView.RepaintAll();
        }
        if (lane.CrossingLanes != null && lane.CrossingLanes.Length > 0)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Crossing lanes:", EditorStyles.boldLabel);
            for (int i = 0; i < lane.CrossingLanes.Length; i++)
            {
                string s = "Crossing lane " + lane.CrossingLanes[i].otherLane.gameObject.name + " at x: " +
                    lane.CrossingLanes[i].crossingPoint.x + ", z: " + lane.CrossingLanes[i].crossingPoint.y;
                EditorGUILayout.LabelField(s, EditorStyles.wordWrappedLabel);
            }
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Nodes on lane: " + lane.nodesOnLane.Length, EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        showNodesOnLane = EditorGUILayout.ToggleLeft("Show nodes on lane?", showNodesOnLane);
        if (showNodesOnLane)
        {
            for (int i = 0; i < lane.nodesOnLane.Length; i++)
            {
                lane.nodesOnLane[i] = EditorGUILayout.ObjectField("N.o.l.  " + i, lane.nodesOnLane[i], typeof(Nodes), true) as Nodes;
            }
        }
        for (int i = 0; i < lane.nodesOnLane.Length; i++)
        {
            EditorGUILayout.LabelField(lane.nodesOnLane[i].gameObject.name);
        }

        if (lane.crosswalkEncounters != null && lane.crosswalkEncounters.Length > 0)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Crosswalks on lane", EditorStyles.boldLabel);
            for (int i = 0; i < lane.crosswalkEncounters.Length; i++)
            {
                if(lane.crosswalkEncounters[i]==null)
                {
                    break;
                }
                CrosswalkEncounter e = lane.crosswalkEncounters[i];
                EditorGUILayout.LabelField(e.crosswalk.name + ": crossed at point (" + e.crosswalk.crossingPoints[e.index] + ")");
            }
        }
        base.OnInspectorGUI();
    }

    /// <summary>
    /// Generates a Vector3 list for drawing node-to-node line visualization of the lane.
    /// </summary>
    private void FetchLinePositions()
    {
        lines = new List<Vector3>();
        for (int i = 0; i < lane.nodesOnLane.Length - 1; i++)
        {
            lines.Add(lane.nodesOnLane[i].transform.position);
            lines.Add(lane.nodesOnLane[i + 1].transform.position);
        }
    }
    /// <summary>
    /// Generates a Vector3 list of all the other lanes of this road for visualization.
    /// </summary>
    private void FetchOtherLines()
    {
        otherLines = new List<Vector3>();
        Lane[] allLanes = lane.gameObject.transform.parent.GetComponentsInChildren<Lane>();
        for (int i = 0; i < allLanes.Length; i++)
        {
            if (allLanes[i] != lane)
            {
                for (int j = 0; j < allLanes[i].nodesOnLane.Length - 1; j++)
                {
                    otherLines.Add(allLanes[i].nodesOnLane[j].transform.position);
                    otherLines.Add(allLanes[i].nodesOnLane[j + 1].transform.position);
                }
            }
        }
    }
    /// <summary>
    /// Visualizes lane in sceneview.
    /// </summary>
    private void DrawLines()
    {
        Handles.color = Color.yellow;
        for (int i = 0; i < lines.Count; i += 2)
        {
            Handles.DrawLine(lines[i], lines[i + 1]);
        }
    }
    /// <summary>
    /// From this lane's parent road object, visualizes all the rest lanes in sceneview.
    /// </summary>
    private void DrawOtherLines()
    {
        Handles.color = Color.grey;
        for (int i = 0; i < otherLines.Count; i += 2)
        {
            Handles.DrawLine(otherLines[i], otherLines[i + 1]);
        }
    }
    /// <summary>
    /// Visualizes crosswalks along target lane.
    /// </summary>
    private void DrawCrossWalks()
    {
        if (lane.crosswalkEncounters == null)
        {
            return;
        }
        if (lane.crosswalkEncounters.Length == 0)
        {
            return;
        }
        if (crosswalks == null)
        {
            FetchCrosswalks();
        }
        for (int i = 0; i < crosswalks.Length; i++)
        {
            Handles.DrawSolidRectangleWithOutline(crosswalks[i].cornerPoints,
                new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0f, 0f, 0f, 1f));
        }
    }
    /// <summary>
    /// Updates custom inspector's crosswalk data.
    /// </summary>
    private void FetchCrosswalks()
    {
        CrosswalkEncounter[] es = lane.crosswalkEncounters;
        List<Crosswalk> cws = new List<Crosswalk>();
        for (int i = 0; i < es.Length; i++)
        {
            if (!cws.Contains(es[i].crosswalk))
            {
                cws.Add(es[i].crosswalk);
            }
        }
        crosswalks = new Crosswalk[cws.Count];
        for (int i = 0; i < cws.Count; i++)
        {
            crosswalks[i] = cws[i];
        }
    }
    /// <summary>
    /// Unity's built-in function, directives to draw in sceneview.
    /// </summary>
    private void OnSceneGUI()
    {
        if (lane.pointToPointLine)
        {
            if (lines == null || lines.Count == 0)
            {
                FetchLinePositions();
            }
            DrawLines();
        }
        if (lane.drawAllLanes)
        {
            if (otherLines == null || otherLines.Count == 0)
            {
                FetchOtherLines();
            }
            DrawOtherLines();
        }
        DrawCrossWalks();
    }
    /// <summary>
    /// Unity's built-in function, executes when object of target's type is activated.
    /// </summary>
    private void OnEnable()
    {
        lane = target as Lane;
    }
    /// <summary>
    /// Unity's built-in function, executes when object of target's type is deselected.
    /// </summary>
    private void OnDisable()
    {
        
    }
}
