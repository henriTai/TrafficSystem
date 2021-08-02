using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Author: Henri Tainio

/// <summary>
/// Custom inspector for Road-objects.
/// </summary>
[CustomEditor(typeof(Road))]
public class RoadInspector : Editor
{
    /// <summary>
    /// Target Road-object.
    /// </summary>
    Road road;
    /// <summary>
    /// Vector3 position list for drawing visualization of all the lanes of target road-object.
    /// </summary>
    List<Vector3> lanePositions;
    /// <summary>
    /// Vector3 position list for highlighting selected guidelane while editing start positions in inspector.
    /// </summary>
    List<Vector3> guideLanePositions;
    /// <summary>
    /// Vector3 position list for highlighting all other in-lanes of selected passage while editing start positions in
    /// inspector.
    /// </summary>
    List<Vector3> selectedPassagePositions;
    /// <summary>
    /// Start positions of each child lane of this road-object for visualization in sceneview.
    /// </summary>
    List<Vector3> startPoints;
    /// <summary>
    /// Point where start line intersects guiding lane.
    /// </summary>
    Vector3 selectedNodePos;
    /// <summary>
    /// The direction from guidelane's selected node to its next node, multiplied with angle adjustment. This is used for
    /// setting helper line while editing start positions in inspector.
    /// </summary>
    Vector3 direction;
    /// <summary>
    /// A boolean if crosswalk data is checked.
    /// </summary>
    bool crosswalksChecked = false;
    /// <summary>
    /// Data of crosswalks along this road.
    /// </summary>
    Crosswalk[] crosswalks;
    /// <summary>
    /// Currently selected passage while editing start positions.
    /// </summary>
    int currentPassage = 0;
    /// <summary>
    /// A boolean if start position's is on and these options are shown in inspector.
    /// </summary>
    bool showStartPosMenu = false;

    /// <summary>
    /// Unity's built-in function, determines what is shown in inspector when an object of target's type is selected.
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField(road.name, EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        bool showLanes = road.showLanes;
        showLanes = EditorGUILayout.ToggleLeft("Show lanes?", showLanes);
        if (showLanes != road.showLanes)
        {
            road.showLanes = showLanes;
            SceneView.RepaintAll();
        }
        SerializedProperty passages = serializedObject.FindProperty("passages");
        EditorGUILayout.PropertyField(passages, new GUIContent("Passages"), true);
        serializedObject.ApplyModifiedProperties();
        if (road.passages != null && road.passages.Length > 0)
        {
            EditorGUILayout.LabelField("(intersection) lane start positions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Set lane start positions AFTER the lanes are grouped in passages. (Intersections only)",
                EditorStyles.wordWrappedLabel);
            if (showStartPosMenu == false)
            {
                if (GUILayout.Button("Set start positions"))
                {
                    showStartPosMenu = true;
                    CheckStartPositionArrays();
                    SceneView.RepaintAll();
                }
            }
            else
            {
                if (GUILayout.Button("Hide start position options"))
                {
                    showStartPosMenu = false;
                    SceneView.RepaintAll();
                }
                ShowStartPositionsMenu();
            }
        }
        if (GUILayout.Button("Add Crosswalk"))
        {
            GameObject go = new GameObject();
            int index = 0;
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            while (true)
            {
                string name = "CW_" + road.name + "_" + index;
                bool alreadyExists = false;
                for (int i = 0; i < allObjects.Length; i++)
                {
                    if (allObjects[i].name.Equals(name))
                    {
                        alreadyExists = true;
                        break;
                    }
                }
                if (alreadyExists)
                {
                    index++;
                }
                else
                {
                    go.name = name;
                    break;
                }

            }
            go.transform.position = road.transform.position;
            Crosswalk c = go.AddComponent<Crosswalk>();
            c.road = road;
            Selection.activeObject = go;
        }
        // base.OnInspectorGUI();
    }
    /// <summary>
    /// Gathers data of start positions and guidelanes when entering start position editing mode in inspector.
    /// </summary>
    private void CheckStartPositionArrays()
    {
        bool isValid = true;
        if (road.guideLane == null || road.guideLane.Length != road.passages.Length)
        {
            isValid = false;
        }
        else
        {
            for (int i = 0; i < road.guideLane.Length; i++)
            {
                if (road.guideLane[i] == null)
                {
                    isValid = false;
                    break;
                }
            }
        }
        if (road.selectedLaneIndex == null || road.selectedLaneIndex.Length != road.passages.Length)
        {
            isValid = false;
        }
        else
        {
            for (int i = 0; i < road.selectedLaneIndex.Length; i++)
            {
                if (road.selectedLaneIndex[i] > road.passages[i].inLanes.Length - 1)
                {
                    isValid = false;
                    break;
                }
            }
        }

        if (road.targetNode == null || road.targetNode.Length != road.passages.Length)
        {
            isValid = false;
        }
        else
        {
            for (int i = 0; i < road.targetNode.Length; i++)
            {
                if (road.targetNode[i] == null)
                {
                    isValid = false;
                    break;
                }
            }
        }
        if (road.nodeIndex == null || road.nodeIndex.Length != road.passages.Length)
        {
            isValid = false;
        }
        if (isValid)
        {
            for (int i = 0; i < road.passages.Length; i++)
            {
                if (road.nodeIndex[i] > road.guideLane[i].nodesOnLane.Length - 1)
                {
                    isValid = false;
                }
            }
        }
        if (road.positionBetweenNodes == null || road.positionBetweenNodes.Length != road.passages.Length)
        {
            isValid = false;
        }
        if (road.angleAdjustment == null || road.angleAdjustment.Length != road.passages.Length)
        {
            isValid = false;
        }
        if (isValid == false)
        {
            road.guideLane = new Lane[road.passages.Length];
            road.selectedLaneIndex = new int[road.passages.Length];
            road.targetNode = new Nodes[road.passages.Length];
            road.nodeIndex = new int[road.passages.Length];
            road.positionBetweenNodes = new float[road.passages.Length];
            road.angleAdjustment = new float[road.passages.Length];
            for (int i = 0; i < road.passages.Length; i++)
            {
                if (road.passages[i].inLanes.Length > 0)
                {
                    road.guideLane[i] = road.passages[i].inLanes[0];
                    road.selectedLaneIndex[i] = 0;
                    road.nodeIndex[i] = 0;
                    road.targetNode[i] = road.guideLane[i].nodesOnLane[0];
                    road.positionBetweenNodes[i] = 0f;
                }
            }
        }
        FetchPassageAndGuideLanePositions();
    }
    /// <summary>
    /// Top menu for start position editing options in inspector.
    /// </summary>
    private void ShowStartPositionsMenu()
    {
        ShowPassageSelector();
        ShowGuideLaneSelector();
        ShowNodeSelector();
        ShowAdjustmentSliders();
        if (GUILayout.Button("Set start positions to lanes"))
        {
            SetStartPositionsToLanes();
        }
    }
    /// <summary>
    /// Submenu for start position editing in inspector, for selecting currently edited passage.
    /// </summary>
    private void ShowPassageSelector()
    {
        EditorGUILayout.LabelField("Selected passage: " + (currentPassage + 1));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Passage -"))
        {
            if (currentPassage == 0)
            {
                currentPassage = road.passages.Length - 1;
            }
            else
            {
                currentPassage--;
            }
            FetchPassageAndGuideLanePositions();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Passage +"))
        {
            if (currentPassage == road.passages.Length - 1)
            {
                currentPassage = 0;
            }
            else
            {
                currentPassage++;
            }
            FetchPassageAndGuideLanePositions();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
    }
    /// <summary>
    /// Submenu for start position editing in inspector, for selecting with lane is used as a guidelane on currently edited
    /// passage.
    /// </summary>
    private void ShowGuideLaneSelector()
    {
        int lanesOnPassage = road.passages[currentPassage].inLanes.Length;
        EditorGUILayout.LabelField("Lanes on this passage: " + lanesOnPassage);
        EditorGUILayout.LabelField("Selected lane (" + (road.selectedLaneIndex[currentPassage] + 1) + "): " +
            road.guideLane[currentPassage].name);

        EditorGUILayout.LabelField("Change guide lane");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Lane -"))
        {
            if (road.selectedLaneIndex[currentPassage] == 0)
            {
                int ind = road.passages[currentPassage].inLanes.Length - 1;
                road.selectedLaneIndex[currentPassage] = ind;
                road.guideLane[currentPassage] = road.passages[currentPassage].inLanes[ind];
                CheckStartNodeWhenSwitchingLane();
            }
            else
            {
                road.selectedLaneIndex[currentPassage]--;
                road.guideLane[currentPassage] = road.passages[currentPassage].inLanes[road.selectedLaneIndex[currentPassage]];
                CheckStartNodeWhenSwitchingLane();
            }
            FetchPassageAndGuideLanePositions();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Lane +"))
        {
            if (road.selectedLaneIndex[currentPassage] == road.passages[currentPassage].inLanes.Length - 1)
            {
                road.selectedLaneIndex[currentPassage] = 0;
                road.guideLane[currentPassage] = road.passages[currentPassage].inLanes[0];
                CheckStartNodeWhenSwitchingLane();
            }
            else
            {
                road.selectedLaneIndex[currentPassage]++;
                road.guideLane[currentPassage] = road.passages[currentPassage].inLanes[road.selectedLaneIndex[currentPassage]];
                CheckStartNodeWhenSwitchingLane();
            }
            FetchPassageAndGuideLanePositions();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
    }
    /// <summary>
    /// When guidelane of active passage is changed in inspector, monitors that node index is valid.
    /// </summary>
    private void CheckStartNodeWhenSwitchingLane()
    {
        int ind = road.nodeIndex[currentPassage];
        if (ind > road.guideLane[currentPassage].nodesOnLane.Length - 1)
        {
            road.nodeIndex[currentPassage] = road.guideLane[currentPassage].nodesOnLane.Length - 1;
            road.targetNode[currentPassage] =
                road.guideLane[currentPassage].nodesOnLane[road.guideLane[currentPassage].nodesOnLane.Length - 1];
        }
        else if (ind >= 0)
        {
            road.targetNode[currentPassage] = road.guideLane[currentPassage].nodesOnLane[ind];
        }
        else
        {
            Lane l = road.guideLane[currentPassage].nodesOnLane[0].InNode.ParentLane;
            ind = l.nodesOnLane.Length + ind;
            if (ind < 0)
            {
                road.nodeIndex[currentPassage] = l.nodesOnLane.Length - 1;
                road.targetNode[currentPassage] = l.nodesOnLane[0];
            }
            else
            {
                road.targetNode[currentPassage] = l.nodesOnLane[ind];
            }
        }
    }
    /// <summary>
    /// Submenu for start position editing in inspector, for selecting node for placing start positions.
    /// </summary>
    private void ShowNodeSelector()
    {
        EditorGUILayout.LabelField("Selected node (" + road.nodeIndex[currentPassage] + "): " + road.targetNode[currentPassage]);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Node -"))
        {
            int minVal = -(road.guideLane[currentPassage].nodesOnLane[0].InNode.ParentLane.nodesOnLane.Length);
            if (road.nodeIndex[currentPassage] > minVal)
            {
                int ind = road.nodeIndex[currentPassage] - 1;
                road.nodeIndex[currentPassage] = ind;
                if (ind >= 0)
                {
                    road.targetNode[currentPassage] = road.guideLane[currentPassage].nodesOnLane[ind];
                }
                else
                {
                    Lane l = road.guideLane[currentPassage].nodesOnLane[0].InNode.ParentLane;
                    ind = l.nodesOnLane.Length + ind;
                    road.targetNode[currentPassage] = l.nodesOnLane[ind];
                }
                FetchPassageAndGuideLanePositions();
                SceneView.RepaintAll();
            }
        }
        if (GUILayout.Button("Node +"))
        {
            int maxVal = road.guideLane[currentPassage].nodesOnLane.Length - 1;
            if (road.nodeIndex[currentPassage] < maxVal)
            {
                int ind = road.nodeIndex[currentPassage] + 1;
                road.nodeIndex[currentPassage] = ind;
                if (ind >= 0)
                {
                    road.targetNode[currentPassage] = road.guideLane[currentPassage].nodesOnLane[ind];
                }
                else
                {
                    Lane l = road.guideLane[currentPassage].nodesOnLane[0].InNode.ParentLane;
                    ind = l.nodesOnLane.Length + ind;
                    road.targetNode[currentPassage] = l.nodesOnLane[ind];
                }
                FetchPassageAndGuideLanePositions();
                SceneView.RepaintAll();
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    /// <summary>
    /// Submenu for start position editing in inspector, for fine-tuning start position between nodes.
    /// </summary>
    private void ShowAdjustmentSliders()
    {
        if (road.nodeIndex[currentPassage] < road.guideLane[currentPassage].nodesOnLane.Length - 1)
        {
            EditorGUILayout.LabelField("Position between nodes", EditorStyles.boldLabel);
            float val = road.positionBetweenNodes[currentPassage];
            val = EditorGUILayout.Slider(val, 0f, 1f);
            if (val != road.positionBetweenNodes[currentPassage])
            {
                road.positionBetweenNodes[currentPassage] = val;
                FetchPassageAndGuideLanePositions();
                SceneView.RepaintAll();
            }
        }
        EditorGUILayout.LabelField("Angle adjustment", EditorStyles.boldLabel);
        float angleSliderVal = road.angleAdjustment[currentPassage];
        angleSliderVal = EditorGUILayout.Slider(angleSliderVal, 0f, 180f);
        if (angleSliderVal != road.angleAdjustment[currentPassage])
        {
            road.angleAdjustment[currentPassage] = angleSliderVal;
            FetchPassageAndGuideLanePositions();
            SceneView.RepaintAll();
        }

    }
    /// <summary>
    /// Fetches position data for visualizing lanes of this road in sceneview.
    /// </summary>
    private void FetchLanePositions()
    {
        lanePositions = new List<Vector3>();
        Lane[] allLanes = road.gameObject.GetComponentsInChildren<Lane>();
        for (int i = 0; i < allLanes.Length; i++)
        {
            for (int j = 0; j < allLanes[i].nodesOnLane.Length - 1; j++)
            {
                lanePositions.Add(allLanes[i].nodesOnLane[j].transform.position);
                lanePositions.Add(allLanes[i].nodesOnLane[j + 1].transform.position);
                }
        }
    }
    /// <summary>
    /// When editing start positions, fetches data of active guidelane and other lanes of active passage for visualization.
    /// </summary>
    private void FetchPassageAndGuideLanePositions()
    {
        selectedPassagePositions = new List<Vector3>();
        guideLanePositions = new List<Vector3>();
        if (road.guideLane == null)
        {
            return;
        }
        if (road.passages == null)
        {
            return;
        }
        for (int i = 0; i < road.passages.Length; i++)
        {
            if (road.passages[i] == null)
            {
                return;
            }
        }
        if (currentPassage > road.guideLane.Length - 1 || road.guideLane[currentPassage] == null)
        {
            return;
        }
        for (int i = 0; i < road.passages[currentPassage].inLanes.Length; i++)
        {
            Lane l = road.passages[currentPassage].inLanes[i];
            for (int j = 0; j < l.nodesOnLane.Length - 1; j++)
            {
                if (l == road.guideLane[currentPassage])
                {
                    guideLanePositions.Add(l.nodesOnLane[j].transform.position);
                    guideLanePositions.Add(l.nodesOnLane[j + 1].transform.position);
                }
                else
                {
                    selectedPassagePositions.Add(l.nodesOnLane[j].transform.position);
                    selectedPassagePositions.Add(l.nodesOnLane[j + 1].transform.position);
                }
            }
        }
        Nodes n = road.targetNode[currentPassage];
        selectedNodePos = n.transform.position;
        direction = (n.OutNode.transform.position - n.transform.position).normalized;
        if (road.angleAdjustment[currentPassage] > 0f)
        {
            Vector3 deg = new Vector3(0f, road.angleAdjustment[currentPassage], 0f);
            direction = (Quaternion.Euler(deg) * direction).normalized;
        }
        if (road.nodeIndex[currentPassage] < road.guideLane[currentPassage].nodesOnLane.Length - 1)
        {
            float between = road.positionBetweenNodes[currentPassage];
            if (between > 0f)
            {
                int ind = road.nodeIndex[currentPassage];
                Nodes next = null;
                if (ind == -1)
                {
                    next = road.guideLane[currentPassage].nodesOnLane[0];
                }
                else
                {
                    next = n.OutNode;
                }
                Vector3 AB = next.transform.position - n.transform.position;
                selectedNodePos += AB * road.positionBetweenNodes[currentPassage];
            }
            
        }
    }
    /// <summary>
    /// Saves start position values from inspector to edited lane-objects.
    /// </summary>
    private void SetStartPositionsToLanes()
    {
        for (int i = 0; i < road.passages.Length; i++)
        {
            InOutPassageWays iop = road.passages[i];
            Vector2 targetPos = new Vector2(road.targetNode[i].transform.position.x,
                road.targetNode[i].transform.position.z);
            if (road.positionBetweenNodes[i] > 0f)
            {
                Nodes next = null;
                if (road.nodeIndex[i] == -1)
                {
                    next = road.guideLane[i].nodesOnLane[0];
                }
                else
                {
                    next = road.targetNode[i].OutNode;
                }
                Vector2 nextPos = new Vector2(next.transform.position.x, next.transform.position.z);
                targetPos += (nextPos - targetPos) * road.positionBetweenNodes[i];
            }

            Vector3 dir = (road.targetNode[i].OutNode.transform.position - road.targetNode[i].transform.position).normalized;
            if (road.angleAdjustment[i] > 0f)
            {
                Vector3 deg = new Vector3(0f, road.angleAdjustment[i], 0f);
                dir = (Quaternion.Euler(deg) * dir).normalized;
            }

            Vector2 A1 = new Vector2(-dir.z, dir.x) * 100f + targetPos;
            Vector2 A2 = new Vector2(dir.z, -dir.x) * 100f + targetPos;

            Vector2[] crossingPoints = new Vector2[iop.inLanes.Length];
            for (int j = 0; j < iop.inLanes.Length; j++)
            {
                Lane l = iop.inLanes[j];
                Lane previousLane = l.nodesOnLane[0].InNode.ParentLane;
                bool found = false;
                for (int k = 0; k < l.nodesOnLane.Length - 1; k++)
                {
                    Vector2 B1 = new Vector2(l.nodesOnLane[k].transform.position.x, l.nodesOnLane[k].transform.position.z);
                    Vector2 B2 = new Vector2(l.nodesOnLane[k + 1].transform.position.x, l.nodesOnLane[k + 1].transform.position.z);
                    Vector2 cp = CrosswalkInspector.CrossingPoint(A1, A2, B1, B2);
                    if (cp != Vector2.zero)
                    {
                        found = true;
                        crossingPoints[j] = cp;
                        break;
                    }
                }
                if (!found)
                {
                    for (int k = 0; k < previousLane.nodesOnLane.Length - 1; k++)
                    {
                        Vector2 B1 = new Vector2(previousLane.nodesOnLane[k].transform.position.x,
                            previousLane.nodesOnLane[k].transform.position.z);
                        Vector2 B2 = new Vector2(previousLane.nodesOnLane[k + 1].transform.position.x,
                            previousLane.nodesOnLane[k + 1].transform.position.z);
                        Vector2 cp = CrosswalkInspector.CrossingPoint(A1, A2, B1, B2);
                        if (cp != Vector2.zero)
                        {
                            found = true;
                            crossingPoints[j] = cp;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    Nodes n1 = previousLane.nodesOnLane[previousLane.nodesOnLane.Length - 1];
                    Vector2 B1 = new Vector2(n1.transform.position.x, n1.transform.position.z);
                    Vector2 B2 = new Vector2(l.nodesOnLane[0].transform.position.x, l.nodesOnLane[0].transform.position.z);
                    Vector2 cp = CrosswalkInspector.CrossingPoint(A1, A2, B1, B2);
                    if (cp != Vector2.zero)
                    {
                        crossingPoints[j] = cp;
                    }
                    else
                    {
                        Debug.Log("Didn't find crossing point (lane " + iop.inLanes[j].name + ")");
                    }
                }
            }
            for (int j = 0; j < iop.inLanes.Length; j++)
            {
                iop.inLanes[j].startPosition = crossingPoints[j];
            }
        }
    }
    /// <summary>
    /// Draws point-to-point visualization of lanes of this road.
    /// </summary>
    private void DrawLanePositions()
    {
        Handles.color = Color.yellow;
        for (int i = 0; i < lanePositions.Count; i += 2)
        {
            Handles.DrawLine(lanePositions[i], lanePositions[i + 1]);
        }
    }
    /// <summary>
    /// Draws lane visualization of currently selected passage while editing start positions.
    /// </summary>
    private void DrawStartPositionLanes()
    {
        Handles.color = Color.blue;
        for (int i = 0; i < selectedPassagePositions.Count; i += 2)
        {
            Handles.DrawLine(selectedPassagePositions[i], selectedPassagePositions[i + 1]);
        }
        Handles.color = Color.green;
        for (int i = 0; i < guideLanePositions.Count; i += 2)
        {
            Handles.DrawLine(guideLanePositions[i], guideLanePositions[i + 1]);
        }
        Handles.color = Color.white;
        Handles.DrawSolidDisc(selectedNodePos, Vector3.up, 0.2f);

        Handles.color = Color.magenta;

        Vector3 p1 = new Vector3(-direction.z, direction.y, direction.x) * 40f + selectedNodePos;
        Vector3 p2 = new Vector3(direction.z, direction.y, -direction.x) * 40f + selectedNodePos;
        Handles.DrawLine(p1, p2);
    }
    /// <summary>
    /// Gathers data of crosswalks along this road.
    /// </summary>
    private void CheckCrosswalks()
    {
        Lane[] lanes = road.GetComponentsInChildren<Lane>();
        List<Crosswalk> cws = new List<Crosswalk>();
        for (int i = 0; i < lanes.Length; i++)
        {
            Lane l = lanes[i];
            if (l.crosswalkEncounters== null)
            {
                continue;
            }
            for (int j = 0; j < l.crosswalkEncounters.Length; j++)
            {
                if (!cws.Contains(l.crosswalkEncounters[j].crosswalk))
                {
                    cws.Add(l.crosswalkEncounters[j].crosswalk);
                }
            }
        }
        if (cws.Count == 0)
        {
            crosswalksChecked = true;
            return;
        }
        crosswalks = new Crosswalk[cws.Count];
        for (int i = 0; i < cws.Count; i++)
        {
            crosswalks[i] = cws[i];
            crosswalksChecked = true;
        }
    }
    /// <summary>
    /// Draws visualization of crosswalks along this road.
    /// </summary>
    private void DrawCrosswalks()
    {
        if (crosswalks == null)
        {
            return;
        }
        for (int i = 0; i < crosswalks.Length; i++)
        {
            Handles.DrawSolidRectangleWithOutline(crosswalks[i].cornerPoints,
                new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0f, 0f, 0f, 1f));
        }
    }
    /// <summary>
    /// Updates start point position list for visualization.
    /// </summary>
    private void UpdateStartPointsForVisualization()
    {
        startPoints = new List<Vector3>();
        Lane[] childLanes = road.GetComponentsInChildren<Lane>();
        for (int i = 0; i < childLanes.Length; i++)
        {
            Vector2 p = childLanes[i].GetStartPosition();
            Debug.Log(p + " : " + childLanes[i].name);
            startPoints.Add(new Vector3(p.x, 0f, p.y));
        }
    }
    /// <summary>
    /// Visualizes start positions in inspector.
    /// </summary>
    private void DrawStartPoints()
    {
        if (startPoints == null)
        {
            UpdateStartPointsForVisualization();
        }
        for (int i = 0; i < startPoints.Count; i++)
        {
            Handles.color = Color.blue;
            Handles.DrawSolidDisc(startPoints[i], Vector3.up, 0.5f);
        }
    }
    /// <summary>
    /// Unity's built-in function, directives of what is drawn in sceneview when target object is active.
    /// </summary>
    private void OnSceneGUI()
    {
        if (showStartPosMenu)
        {
            DrawStartPositionLanes();
            DrawStartPoints();
        }
        else if (road.showLanes)
        {
            if (lanePositions == null || lanePositions.Count == 0)
            {
                FetchLanePositions();
            }
            DrawLanePositions();
        }
        if (crosswalksChecked == false)
        {
            CheckCrosswalks();
        }
        DrawCrosswalks();

    }

    /// <summary>
    /// Unity's built-in function, executed when object of target's type is activated.
    /// </summary>
    private void OnEnable()
    {
        road = target as Road;
    }

}
