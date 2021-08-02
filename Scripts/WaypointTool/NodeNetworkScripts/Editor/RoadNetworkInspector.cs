using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Author: Henri Tainio

/// <summary>
/// Custom inspector for RoadNetwork-objects.
/// </summary>
[CustomEditor(typeof(RoadNetwork))]
public class RoadNetworkInspector : Editor
{
    /// <summary>
    /// Variable for target RoadNetwork-object.
    /// </summary>
    RoadNetwork network;
    /// <summary>
    /// List of positions for visualization of start and end nodes of all lanes of all roads.
    /// </summary>
    List<Vector3> points;
    /// <summary>
    /// List of positions for visualizing all lanes of roads point-to-point, from node to node.
    /// </summary>
    List<Vector3> allPoints;
    /// <summary>
    /// In road network's inspector is an option to check that all lanes have no obsolete entries in starting lane arrays -
    /// the return value is stored in this variable.
    /// </summary>
    int startsCorrected = -1;

    /// <summary>
    /// Unity's built-in function, determines what is shown in inspector when object of this type is selected.
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(network.gameObject.name, EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        bool showLines = network.showLines;
        showLines = EditorGUILayout.ToggleLeft("Show lines?", showLines);
        if (showLines != network.showLines)
        {
            network.showLines = showLines;
            if (network.showLines == true)
            {
                network.showDetailed = false;
            }
            SceneView.RepaintAll();
        }
        showLines = network.showDetailed;
        showLines = EditorGUILayout.ToggleLeft("Shoe point-to-point?", showLines);
        if (showLines != network.showDetailed)
        {
            network.showDetailed = showLines;
            if (network.showDetailed == true)
            {
                network.showLines = false;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("Check lane starts"))
        {
            Road[] allRoads = network.gameObject.GetComponentsInChildren<Road>();
            startsCorrected = 0;
            for (int i = 0; i < allRoads.Length; i++)
            {
                startsCorrected += allRoads[i].RemoveFalseLaneStarts();
            }
        }
        if (startsCorrected > -1)
        {
            EditorGUILayout.LabelField("Starts corrected: " + startsCorrected);
        }
    }
    /// <summary>
    /// Updates a list of positions for drawing endpoint visualization of all lanes.
    /// </summary>
    private void FetchPoints()
    {
        points = new List<Vector3>();
        Road[] allRoads = network.gameObject.GetComponentsInChildren<Road>();
        for (int i = 0; i < allRoads.Length; i++)
        {
            Road road = allRoads[i];
            Lane[] allLanes = road.gameObject.GetComponentsInChildren<Lane>();
            for (int j = 0; j < allLanes.Length; j++)
            {
                Lane l = allLanes[j];
                points.Add(l.nodesOnLane[0].transform.position);
                points.Add(l.nodesOnLane[l.nodesOnLane.Length - 1].transform.position);
            }
        }
    }
    /// <summary>
    /// Updates a list of positions for drawing point-to-point visualization of all lanes.
    /// </summary>
    private void FetchAllPoints()
    {
        allPoints = new List<Vector3>();
        Road[] allRoads = network.gameObject.GetComponentsInChildren<Road>();
        for (int i = 0; i < allRoads.Length; i++)
        {
            Road road = allRoads[i];
            Lane[] allLanes = road.gameObject.GetComponentsInChildren<Lane>();
            for (int j = 0; j < allLanes.Length; j++)
            {
                Lane l = allLanes[j];
                for (int k = 0; k < l.nodesOnLane.Length - 1; k++)
                {
                    allPoints.Add(l.nodesOnLane[k].transform.position);
                    allPoints.Add(l.nodesOnLane[k + 1].transform.position);
                }
                Nodes n = l.nodesOnLane[l.nodesOnLane.Length - 1];
                if (n.OutNode != null)
                {
                    allPoints.Add(n.transform.position);
                    allPoints.Add(n.OutNode.transform.position);
                }
            }
        }
    }
    /// <summary>
    /// Draws endpoint visualization of all lanes in sceneview.
    /// </summary>
    private void ShowLines()
    {
        Handles.color = Color.green;
        for (int i = 0; i < points.Count; i += 2)
        {
            Handles.DrawLine(points[i], points[i + 1]);
        }
    }
    /// <summary>
    /// Draws point-to-point visualization of all lanes in sceneview.
    /// </summary>
    private void ShowAllLines()
    {
        Handles.color = Color.green;
        for (int i = 0; i < allPoints.Count; i += 2)
        {
            Handles.DrawLine(allPoints[i], allPoints[i + 1]);
        }
    }
    /// <summary>
    /// Unity's built-in function, directives of what is drawn in sceneview when object of this type is selected.
    /// </summary>
    private void OnSceneGUI()
    {
        if (network.showLines)
        {
            if (points == null || points.Count == 0)
            {
                FetchPoints();
            }
            ShowLines();
        }
        if (network.showDetailed)
        {
            if (allPoints == null || allPoints.Count == 0)
            {
                FetchAllPoints();
            }
            ShowAllLines();
        }
    }
    /// <summary>
    /// Unity's built-in function, executed when object of this type is selected.
    /// </summary>
    private void OnEnable()
    {
        network = target as RoadNetwork;
    }

}
