using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Author: Henri Tainio

/// <summary>
/// Custom editor script for object(s) of type RouteUtility.
/// </summary>
[CustomEditor(typeof(RouteUtility))]
public class RouteUtilityInspector : Editor
{
    /// <summary>
    /// Target object
    /// </summary>
    RouteUtility utility;
    /// <summary>
    /// An index of which road's info is shown in inspector. -1 means that none is expanded.
    /// </summary>
    int roadExpanded = -1;
    /// <summary>
    /// An index of which lane's info in selected road is shown in inspector. -1 means none is expanded.
    /// </summary>
    int laneInfoExpanded = -1;
    /// <summary>
    /// An index of which destination lane is selected and shown in sceneview. -1 means none is selected.
    /// </summary>
    int routeSelected = -1;
    /// <summary>
    /// Positions for visualizing the selected route.
    /// </summary>
    List<Vector3> routePos;
    /// <summary>
    /// Positions for visualizing lane changes during the selected route.
    /// </summary>
    List<Vector3> laneChangePos;
    /// <summary>
    /// Count of uncalculated routes.
    /// </summary>
    int uncalculatedRoutes;
    /// <summary>
    /// Count of calculated routes
    /// </summary>
    int calculatedRoutes;
    /// <summary>
    /// An artificial 1 meter is added to route distance each time the lane is changed so that AI would avoid unnecessary changes.
    /// </summary>
    const float laneChangeDistanceAddition = 1f;
    /// <summary>
    /// Width option for an inspector mini button.
    /// </summary>
    private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);
    /// <summary>
    /// Overrides inspector's default content.
    /// </summary>
    public override void OnInspectorGUI()
    {
        Undo.RecordObject(utility, "Route Utility changed");
        ItalicLabel("Uncalculated routes: " + uncalculatedRoutes);
        ItalicLabel("Calculated routes: " + calculatedRoutes);
        ItalicLabel("Current depth: " + utility.depth);
        if (uncalculatedRoutes > 0)
        {
            if (GUILayout.Button("Iterate depth"))
            {
                IterarteDepth();
            }
        }


        for (int i = 0; i < utility.roadInfos.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (roadExpanded != i)
            {
                if (GUILayout.Button("+", EditorStyles.miniButton, miniButtonWidth))
                {
                    roadExpanded = i;
                    laneInfoExpanded = -1;
                    routeSelected = -1;
                    routePos = null;
                    SceneView.RepaintAll();
                }
            }
            else
            {
                if (GUILayout.Button("-", EditorStyles.miniButton, miniButtonWidth))
                {
                    roadExpanded = -1;
                    laneInfoExpanded = -1;
                    routeSelected = -1;
                    routePos = null;
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.LabelField(utility.allRoads[i].name, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            if (roadExpanded == i)
            {
                for (int j = 0; j < utility.roadInfos[i].laneInfoIndexes.Length; j++)
                {
                    TrackerLaneInfo tli = utility.laneInfos[utility.roadInfos[i].laneInfoIndexes[j]];
                    int laneIndex = tli.laneIndex;
                    EditorGUILayout.BeginHorizontal();
                    if (laneInfoExpanded != laneIndex)
                    {
                        if (GUILayout.Button("+", EditorStyles.miniButton, miniButtonWidth))
                        {
                            laneInfoExpanded = laneIndex;
                            routeSelected = -1;
                            routePos = null;
                            SceneView.RepaintAll();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("-", EditorStyles.miniButton, miniButtonWidth))
                        {
                            laneInfoExpanded = -1;
                            routeSelected = -1;
                            routePos = null;
                            SceneView.RepaintAll();
                        }
                    }

                    EditorGUILayout.LabelField(utility.allLanes[laneIndex].name + "(" + laneIndex + ") D: " + 
                        utility.laneLengths[laneIndex], EditorStyles.whiteLabel);
                    EditorGUILayout.EndHorizontal();
                    if (laneInfoExpanded == laneIndex)
                    {
                        EditorGUI.indentLevel++;
                        for (int k = 0; k < utility.allLanes.Length; k++)
                        {
                            EditorGUILayout.Separator();
                            if (tli.nextLaneIndexes[k] == -1)
                            {
                                EditorGUILayout.LabelField(" * " + utility.allLanes[k].name + " (" + k + ")");
                            }
                            else if (k == tli.laneIndex)
                            {
                                EditorGUILayout.LabelField(" * " + utility.allLanes[k].name + " (" + k + ")", EditorStyles.whiteLabel);
                            }
                            else
                            {
                                EditorGUILayout.BeginHorizontal();
                                if (k == routeSelected)
                                {
                                    if (GUILayout.Button("-", EditorStyles.miniButton, miniButtonWidth))
                                    {
                                        routeSelected = -1;
                                        routePos = null;
                                        SceneView.RepaintAll();
                                    }
                                }
                                else
                                {
                                    if (GUILayout.Button("+", EditorStyles.miniButton, miniButtonWidth))
                                    {
                                        routeSelected = k;
                                        FetchRoute();
                                    }
                                }
                                EditorGUILayout.LabelField(utility.allLanes[k].name + " (" + k + ")", EditorStyles.whiteLabel);
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.LabelField("Next lane: " + utility.allLanes[tli.nextLaneIndexes[k]],
                                    EditorStyles.whiteLabel);
                                EditorGUILayout.LabelField("Lanes to destination: " + tli.laneCountToDestination[k],
                                    EditorStyles.whiteLabel);
                                EditorGUILayout.LabelField("Distance: " + tli.distancesToDestination[k], EditorStyles.whiteLabel);
                                int dirI = tli.followingOrLaneChange[k];
                                if (dirI == 1)
                                {
                                    EditorGUILayout.LabelField("Lane change LEFT", EditorStyles.whiteLabel);
                                }
                                else if (dirI == 2)
                                {
                                    EditorGUILayout.LabelField("Lane change RIGHT", EditorStyles.whiteLabel);
                                }
                            }
                            if (tli.nextLaneIndexes[k] == -1)
                            {
                                ItalicLabel("Next lane: NA");
                                ItalicLabel("Lanes to destination: NA");
                                ItalicLabel("Distance: NA");
                            }
                            else if (k == tli.laneIndex)
                            {
                                EditorGUILayout.LabelField("Next lane: -", EditorStyles.whiteLabel);
                                EditorGUILayout.LabelField("Lanes to destination: -", EditorStyles.whiteLabel);
                                EditorGUILayout.LabelField("Distance: -", EditorStyles.whiteLabel);
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            EditorGUI.indentLevel--;
        }
        base.OnInspectorGUI();
    }
    /// <summary>
    /// Iterate's one layer of depth searching shortest routes.
    /// </summary>
    private void IterarteDepth()
    {
        for (int i = 0; i < utility.laneInfos.Length; i++)
        {
            TrackerLaneInfo tliOld = utility.laneInfos[i];
            for (int destinationIndex = 0; destinationIndex < tliOld.laneCountToDestination.Length; destinationIndex++)
            {
                if (tliOld.laneCountToDestination[destinationIndex]== utility.depth)
                {
                    float oldDistance = tliOld.distancesToDestination[destinationIndex];
                    

                    // check lane changes left and right
                    Lane leftChangingLane = LaneChangeToCurrentLane(true, utility.allLanes[tliOld.laneIndex]);
                    Lane rightChangingLane = LaneChangeToCurrentLane(false, utility.allLanes[tliOld.laneIndex]);
                    if (leftChangingLane || rightChangingLane)
                    {
                        Debug.Log("old lane: " + utility.allLanes[tliOld.laneIndex].name);
                    }
                    if (leftChangingLane != null)
                    {
                        Debug.Log("LEFT changing lane: " + leftChangingLane.name);
                    }
                    if (rightChangingLane != null)
                    {
                        Debug.Log("RIGHT changing lane: " + rightChangingLane.name);
                    }

                    if (leftChangingLane != null)
                    {
                        int leftIndex = GetLaneIndex(leftChangingLane);
                        float newDistance = oldDistance + laneChangeDistanceAddition;
                        TrackerLaneInfo leftTli = utility.laneInfos[leftIndex];
                        if (leftTli.distancesToDestination[destinationIndex] > newDistance)
                        {
                            leftTli.distancesToDestination[destinationIndex] = newDistance;
                            leftTli.followingOrLaneChange[destinationIndex] = 1;
                            leftTli.nextLaneIndexes[destinationIndex] = i;
                            leftTli.laneCountToDestination[destinationIndex] = utility.depth + 1;
                        }
                    }
                    if (rightChangingLane != null)
                    {
                        int rightIndex = GetLaneIndex(rightChangingLane);
                        float newDistance = oldDistance + laneChangeDistanceAddition;
                        TrackerLaneInfo rightTli = utility.laneInfos[rightIndex];
                        if (rightTli.distancesToDestination[destinationIndex] > newDistance)
                        {
                            rightTli.distancesToDestination[destinationIndex] = newDistance;
                            rightTli.followingOrLaneChange[destinationIndex] = 1;
                            rightTli.nextLaneIndexes[destinationIndex] = i;
                            rightTli.laneCountToDestination[destinationIndex] = utility.depth + 1;
                        }
                    }

                    // Search lanes that lead to this lane
                    for (int previousLaneIndex = 0; previousLaneIndex < utility.laneInfos.Length; previousLaneIndex++)
                    {
                        if (previousLaneIndex != tliOld.laneIndex)
                        {
                            bool found = false;
                            Lane other = utility.allLanes[previousLaneIndex];
                            Lane[] followingLanes = other.nodesOnLane[other.nodesOnLane.Length - 1].OutNode.StartingLanes;
                            for (int k = 0; k < followingLanes.Length; k++)
                            {
                                if (followingLanes[k] == utility.allLanes[tliOld.laneIndex])
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (found)
                            {
                                float newDistance = oldDistance + utility.laneLengths[previousLaneIndex];
                                if (newDistance < utility.laneInfos[previousLaneIndex].distancesToDestination[destinationIndex])
                                {
                                    utility.laneInfos[previousLaneIndex].distancesToDestination[destinationIndex] = newDistance;
                                    utility.laneInfos[previousLaneIndex].followingOrLaneChange[destinationIndex] = 0;
                                    utility.laneInfos[previousLaneIndex].nextLaneIndexes[destinationIndex] = tliOld.laneIndex;
                                    utility.laneInfos[previousLaneIndex].laneCountToDestination[destinationIndex] = utility.depth + 1;
                                }
                            }
                        }
                    }
                }
            }
        }
        CountUncalculatedRoutes();
        utility.depth++;
    }
    /// <summary>
    /// Searches if there is a lane that can make lane change to this lane from given direction.
    /// </summary>
    /// <param name="left">Is lane change from left side?</param>
    /// <param name="currentLane">Lane change target lane.</param>
    /// <returns>A lane change start lane.</returns>
    private Lane LaneChangeToCurrentLane(bool left, Lane currentLane)
    {
        Lane other = null;
        if (left)
        {
            for (int i = 0; i < currentLane.nodesOnLane.Length; i++)
            {
                if (currentLane.nodesOnLane[i].ParallelRight != null)
                {
                    other = currentLane.nodesOnLane[i].ParallelRight.ParentLane;
                    break;
                }
            }
            if (other != null)
            {
                for (int i = 0; i < other.nodesOnLane.Length; i++)
                {
                    if (other.nodesOnLane[i].LaneChangeLeft != null)
                    {
                        if (other.nodesOnLane[i].LaneChangeLeft.ParentLane == currentLane)
                        {
                            return other;
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < currentLane.nodesOnLane.Length; i++)
            {
                if (currentLane.nodesOnLane[i].ParallelLeft != null)
                {
                    other = currentLane.nodesOnLane[i].ParallelLeft.ParentLane;
                    break;
                }
            }
            if (other != null)
            {
                for (int i = 0; i < other.nodesOnLane.Length; i++)
                {
                    if (other.nodesOnLane[i].LaneChangeRight != null)
                    {
                        if (other.nodesOnLane[i].LaneChangeRight.ParentLane == currentLane)
                        {
                            return other;
                        }
                    }
                }
            }
        }
        return null;
    }
    /// <summary>
    /// Fetches the selected route.
    /// </summary>
    private void FetchRoute()
    {
        routePos = null;
        laneChangePos = null;
        SceneView.RepaintAll();
        List<int> lanesToDestination = new List<int>();
        int currentIndex = laneInfoExpanded;
        int destinationIndex = routeSelected;
        int failsafeCounter = 0;
        bool success = true;
        while (true)
        {
            failsafeCounter++;
            if (failsafeCounter > 500)
            {
                success = false;
                break;
            }
            lanesToDestination.Add(currentIndex);
            currentIndex = utility.laneInfos[currentIndex].nextLaneIndexes[destinationIndex];
            if (currentIndex == destinationIndex)
            {
                break;
            }
        }
        if (!success)
        {
            return;
        }
        List<Vector3> newPos = new List<Vector3>();
        List<Vector3> newLaneChanges = new List<Vector3>();
        for (int i = 0; i < lanesToDestination.Count; i++)
        {
            Lane l = utility.allLanes[lanesToDestination[i]];
            if (i > 0)
            {
                newLaneChanges.Add(l.nodesOnLane[0].transform.position);
            }
            //check lane change
            bool laneChangeComing = false;
            bool leftChecked = false;
            bool rightChecked = false;
            int oldLaneNodeIndex = 0;
            int newLaneNodeIndex = 0;
            if (i < lanesToDestination.Count - 1)
            {
                Lane nextLane = utility.allLanes[lanesToDestination[i + 1]];
                for (int nodeIndex = 0; nodeIndex < l.nodesOnLane.Length; nodeIndex++)
                {
                    if (!leftChecked)
                    {
                        if (l.nodesOnLane[nodeIndex].LaneChangeLeft != null)
                        {
                            leftChecked = true;
                            if (l.nodesOnLane[nodeIndex].LaneChangeLeft.ParentLane == nextLane)
                            {
                                laneChangeComing = true;
                                oldLaneNodeIndex = nodeIndex;
                                for (int n = 0; n < nextLane.nodesOnLane.Length; n++)
                                {
                                    if (nextLane.nodesOnLane[n] == l.nodesOnLane[nodeIndex].LaneChangeLeft)
                                    {
                                        newLaneNodeIndex = n;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (!rightChecked)
                    {
                        if (l.nodesOnLane[nodeIndex].LaneChangeRight != null)
                        {
                            rightChecked = true;
                            if (l.nodesOnLane[nodeIndex].LaneChangeRight.ParentLane == nextLane)
                            {
                                laneChangeComing = true;
                                oldLaneNodeIndex = nodeIndex;
                                for (int n = 0; n < nextLane.nodesOnLane.Length; n++)
                                {
                                    if (nextLane.nodesOnLane[n] == l.nodesOnLane[nodeIndex].LaneChangeRight)
                                    {
                                        newLaneNodeIndex = n;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (leftChecked && rightChecked)
                    {
                        break;
                    }
                }
            }
            if (laneChangeComing)
            {
                for (int j = 0; j < oldLaneNodeIndex; j++)
                {
                    newPos.Add(l.nodesOnLane[j].transform.position);
                    newPos.Add(l.nodesOnLane[j + 1].transform.position);
                }
                newLaneChanges.Add(l.nodesOnLane[oldLaneNodeIndex].transform.position);
                i++;
                l = utility.allLanes[lanesToDestination[i]];
                newLaneChanges.Add(l.nodesOnLane[newLaneNodeIndex].transform.position);
                for (int j = newLaneNodeIndex; j < l.nodesOnLane.Length - 1; j++)
                {
                    newPos.Add(l.nodesOnLane[j].transform.position);
                    newPos.Add(l.nodesOnLane[j + 1].transform.position);
                }
            }
            else
            {
                for (int j = 0; j < l.nodesOnLane.Length - 1; j++)
                {
                    newPos.Add(l.nodesOnLane[j].transform.position);
                    newPos.Add(l.nodesOnLane[j + 1].transform.position);
                }
            }
            if (i < lanesToDestination.Count - 1)
            {
                newLaneChanges.Add(l.nodesOnLane[l.nodesOnLane.Length - 1].transform.position);
            }
        }
        routePos = newPos;
        laneChangePos = newLaneChanges;
        SceneView.RepaintAll();

    }
    /// <summary>
    /// Get lane's index from array of all lanes.
    /// </summary>
    /// <param name="lane">Searched lane.</param>
    /// <returns>Lane's index.</returns>
    private int GetLaneIndex(Lane lane)
    {
        for (int i = 0; i < utility.allLanes.Length; i++)
        {
            if (utility.allLanes[i] == lane)
            {
                return i;
            }
        }
        return -1;
    }
    /// <summary>
    /// Counts uncalculated and calculated routes.
    /// </summary>
    private void CountUncalculatedRoutes()
    {
        int count = 0;
        int count2 = 0;
        for (int i = 0; i < utility.laneInfos.Length; i++)
        {
            TrackerLaneInfo tli = utility.laneInfos[i];
            for (int j = 0; j < tli.nextLaneIndexes.Length; j++)
            {
                if (tli.nextLaneIndexes[j]==-1)
                {
                    count++;
                }
                else
                {
                    count2++;
                }
            }
        }
        uncalculatedRoutes = count;
        calculatedRoutes = count2;
    }
    /// <summary>
    /// Prints message in italic in inspector.
    /// </summary>
    /// <param name="message">Printed message.</param>
    private void ItalicLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.fontStyle = FontStyle.Italic;
        EditorGUILayout.LabelField(message, gs);
    }
    /// <summary>
    /// This function executes when target object is activated.
    /// </summary>
    private void OnEnable()
    {
        utility = target as RouteUtility;
        CountUncalculatedRoutes();
    }
    /// <summary>
    /// Visualizes selected route in sceneview.
    /// </summary>
    private void DrawRoute()
    {
        Handles.color = Color.green;
        for (int i = 0; i < routePos.Count; i += 2)
        {
            Handles.DrawLine(routePos[i], routePos[i + 1]);
        }
        Handles.color = Color.yellow;
        for (int i = 0; i < laneChangePos.Count; i += 2)
        {
            Handles.DrawLine(laneChangePos[i], laneChangePos[i + 1]);
        }
    }
    /// <summary>
    /// Updates sceneview visualzation and gizmos.
    /// </summary>
    private void OnSceneGUI()
    {
        if (routePos != null)
        {
            DrawRoute();
        }
    }
}
