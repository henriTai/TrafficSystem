using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

// Author: Henri Tainio
/// <summary>
/// Custom inspector script for creating intersections.
/// </summary>
[CustomEditor(typeof(IntersectionTool))]
public class IntersectionToolInspector : Editor
{
    /// <summary>
    /// A variable for target Intersection object.
    /// </summary>
    private IntersectionTool intersection;
    /// <summary>
    /// Parent gameobject for created intersection.
    /// </summary>
    private GameObject roadNetwork;
    // naming
    /// <summary>
    /// A boolean if name for intersection is already checked.
    /// </summary>
    private bool nameAutoChecked = true;
    /// <summary>
    /// A boolean if name for intersection is valid.
    /// </summary>
    private bool nameIsValid = false;
    /// <summary>
    /// A variable to hold name for intersection.
    /// </summary>
    private string intersectionName = "";
    /// <summary>
    /// A variable to hold new name before it is validated.
    /// </summary>
    private string renameInfo = "";

    // framing
    /// <summary>
    /// Intersection tool's framing horizontal lines' endpoints for drawing.
    /// </summary>
    private Vector3[] framingHorizontal;
    /// <summary>
    /// Intersection tool's framing vertical lines' endpoints for drawing.
    /// </summary>
    private Vector3[] framingVertical;
    /// <summary>
    /// A value that +/- buttons adjust frame measures.
    /// </summary>
    private float step = 1f;

    // Helper line variables
    /// <summary>
    /// Helper line position.
    /// </summary>
    private Vector3 linePos;
    /// <summary>
    /// Helper line's end point positions.
    /// </summary>
    private Vector3[] linePoints;
    /// <summary>
    /// Helper line's length.
    /// </summary>
    private float lineLength;
    /// <summary>
    /// Helper line's direction.
    /// </summary>
    private Vector3 lineDir;
    /// <summary>
    /// A boolean if helper line is visible in sceneview. Helper line is visible only during creating helper line in/out nodes.
    /// /// </summary>
    private bool showLine = false;
    /// <summary>
    /// Helper line's angle rotation.
    /// </summary>
    private float lineYAngle = 0f;
    /// <summary>
    /// Increment that + / - buttons adjust helper line's angle.
    /// </summary>
    private float lineAngle = 5f;
    /// <summary>
    /// Number of in/out-nodes set along helper line.
    /// </summary>
    private int nodesOnLine = 0;
    /// <summary>
    /// An array of values between 0 - 1. In/out-nodes are placed along the helper line according these values.
    /// </summary>
    private float[] nodePlaces;
    /// <summary>
    /// An array of temporary values when node places along helper line are edited.
    /// </summary>
    private float[] tempPlaces;
    /// <summary>
    /// An array of in/out status of nodes along helper line.
    /// </summary>
    private NodeInOut[] lineNodesInOut;
    /// <summary>
    /// Helper line node positions for drawing in sceneview.
    /// </summary>
    private List<Vector3> pointsToDraw;
    /// <summary>
    /// Helper line node colors for drawing in sceneview. (blue = in-node, red = out-node)
    /// </summary>
    private List<Color> pointsToDrawColors;

    // Existing lanes. These variables are for holding values during editing phase and then saved in target intersection-object.
    /// <summary>
    /// A list of position vector's for drawing point-to-point visualization of existing lanes. Start and end
    /// points are saved for each lane segment.
    /// </summary>
    private List<Vector3> existingLaneLinesToDraw;
    /// <summary>
    /// Turn direction of existing lane.
    /// </summary>
    private IntersectionDirection exTurn;
    /// <summary>
    /// Tarffic size of existing lane.
    /// </summary>
    private TrafficSize exTraffic;
    /// <summary>
    /// Speed limit of existing lane.
    /// </summary>
    private SpeedLimits exSpeedLimit;
    /// <summary>
    /// A boolean if setup for currently selected existing lane is already confirmed.
    /// </summary>
    private bool exConfirmed;
    /// <summary>
    /// A boolean if nodes are currently edited - affects inpector menu.
    /// </summary>
    private bool addingNodes = false;
    /// <summary>
    /// A boolean if confirmation phase is on.
    /// </summary>
    private bool confirming = false;

    // Driving line setup phase, after node setup:

    /// <summary>
    /// A list of in-node positions, derived from target intersection-object after node setup phase.
    /// </summary>
    public List<Vector3> inPositions;
    /// <summary>
    /// A list of out-node positions, derived from target intersection-object after node setup phase.
    /// </summary>
    public List<Vector3> outPositions;
    /// <summary>
    /// A sum of in/out nodes. (this variable is not needed currently)
    /// </summary>
    public int inOutCount;
    /// <summary>
    /// A variable for active handle transform (target intersection object).
    /// </summary>
    Transform handleTransform;
    /// <summary>
    /// A variable for active handle orientation.
    /// </summary>
    Quaternion handleRotation;
    /// <summary>
    /// A float modifier value for active handle point visualization in sceneview.
    /// </summary>
    private const float handleSize = 0.04f;
    /// <summary>
    /// A float modifier value for unselected handle point visualization in sceneview.
    /// </summary>
    private const float pickSize = 0.06f;
    /// <summary>
    /// A Vector2Int index for selected node (selected spline, selected node).
    /// </summary>
    private Vector2Int selectedIndex;
    /// <summary>
    /// A list of node positions on currently selected spline for visualization purposes.
    /// </summary>
    private List<Vector3> currentSplineNodes;
    /// <summary>
    /// A list of node positions on all unselected splines for visualization purposes.
    /// </summary>
    private List<Vector3> otherSplineNodes;
    /// <summary>
    /// A boolean if node positions are fetched from target intersection-object. If true, node positions are visualized
    /// in sceneview.
    /// </summary>
    private bool nodesFetched = false;
    /// <summary>
    /// A boolean if each endpoint of all splines is connected to an out-node.
    /// </summary>
    private bool allEndNodesConnected = true;
    /// <summary>
    /// A boolean if node count for each each spline is set.
    /// </summary>
    private bool allSegmentsHaveNodes = true;


    //*************************** INSPECTOR START
    /// <summary>
    /// Defines what is shown in inspector. 
    /// </summary>
    public override void OnInspectorGUI()
    {
        Undo.RecordObject(intersection, "changed");
        NameMenu();
        if (intersection.splinesSet)
        {
            NodeSetupMenu();
            //base.OnInspectorGUI();
            return;
        }
        if (intersection.allNodesSet)
        {
            if (intersection.existingLanesChecked)
            {
                SplineSetupMenu();
            }
            else
            {
                ConfirmExistingLanesMenu();
            }
        }
        else
        {
            if (!intersection.framed)
            {
                FramingMenu();
            }
            else
            {
                SetupMenu();
            }
        }
        //base.OnInspectorGUI();
    }
    /// <summary>
    /// Inspector naming menu.
    /// </summary>
    private void NameMenu()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Road network:", EditorStyles.boldLabel);
        if (roadNetwork == null)
        {
            EditorGUILayout.LabelField("Not selected");
        }
        else
        {
            EditorGUILayout.LabelField(roadNetwork.name);
        }
        EditorGUILayout.EndHorizontal();
        if (roadNetwork == null)
        {
            if (intersection.roadNetwork != null)
            {
                roadNetwork = intersection.roadNetwork;
                nameAutoChecked = false;
            }
            else
            {
                RoadNetwork[] networks = GameObject.FindObjectsOfType<RoadNetwork>();
                if (networks == null || networks.Length == 0)
                {
                    GameObject g = new GameObject();
                    g.AddComponent<RoadNetwork>();
                    g.name = "NodeNetwork";
                    roadNetwork = g;
                    intersection.roadNetwork = g;
                }
                else if (networks.Length == 1)
                {
                    roadNetwork = networks[0].gameObject;
                    intersection.roadNetwork = roadNetwork;
                    nameAutoChecked = false;
                }
                else
                {
                    EditorGUILayout.LabelField("Select parent network", EditorStyles.boldLabel);
                    for (int i = 0; i < networks.Length; i++)
                    {
                        bool selected = false;
                        selected = EditorGUILayout.Toggle(networks[i].gameObject.name, selected);
                        if (selected)
                        {
                            roadNetwork = networks[i].gameObject;
                            intersection.roadNetwork = networks[i].gameObject;
                            nameAutoChecked = false;
                        }
                    }
                }
            }
        }

        EditorGUILayout.Separator();

        if (roadNetwork != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Intersection name:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(intersection.gameObject.name);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            ItalicLabel("As a suggestion (for easier managing) intersection names should have a prefix, " +
                "for example. 'int_'.");
            if (!nameAutoChecked)
            {
                nameAutoChecked = true;
                nameIsValid = CheckName(intersection.gameObject.name);
                if (nameIsValid)
                {
                    intersectionName = intersection.gameObject.name;
                }
                else
                {
                    intersectionName = "";
                }
            }
            if (nameIsValid)
            {
                ItalicLabel("Name is valid");
            }
            else
            {
                WarningLabel("Invalid name. Name already exists.");
            }
            intersectionName = GUILayout.TextField(intersectionName);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Enter name");
            if (GUILayout.Button("Rename"))
            {
                bool valid = CheckName(intersectionName);
                if (!valid)
                {
                    renameInfo = "New name was not valid.";
                }
                else
                {
                    renameInfo = "Name changed to '" + intersectionName + "'.";
                    intersection.gameObject.name = intersectionName;
                    nameIsValid = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            ItalicLabel(renameInfo);

            EditorGUILayout.Separator();
        }
        DrawEditorLine();
    }
    /// <summary>
    /// Inspector framing menu.
    /// </summary>
    private void FramingMenu()
    {
        EditorGUILayout.LabelField("Framing", EditorStyles.boldLabel);
        step = EditorGUILayout.FloatField("Step:", step);
        EditorGUILayout.Separator();
        // centerpoint
        EditorGUILayout.LabelField("Adjust centerpoint", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Up"))
        {
            intersection.CenterPoint += new Vector3(0f, 0f, step);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Down"))
        {
            intersection.CenterPoint += new Vector3(0f, 0f, -step);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Left"))
        {
            intersection.CenterPoint += new Vector3(-step, 0f, 0f);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Right"))
        {
            intersection.CenterPoint += new Vector3(step, 0f, 0f);
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        // bounding box
        EditorGUILayout.LabelField("Adjust bounding box", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Width+"))
        {
            intersection.FrameWidth += step;
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Width-"))
        {
            float width = intersection.FrameWidth - step;
            if (width > 0f)
            {
                intersection.FrameWidth = width;
            }
            else
            {
                intersection.FrameWidth = 0.1f;
            }
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Height+"))
        {
            intersection.FrameHeight += step;
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Height-"))
        {
            float height = intersection.FrameHeight - step;
            if (height > 0f)
            {
                intersection.FrameHeight = height;
            }
            else
            {
                intersection.FrameHeight = 0f;
            }
            UpdateFramingBox();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Framing done"))
        {
            intersection.framed = true;
            UpdateNodesInBox();
        }
    }
    /// <summary>
    /// Inspector in/out setup-menu, parent for other setup submenus.
    /// </summary>
    private void SetupMenu()
    {
        if (GUILayout.Button("Back to framing"))
        {
            intersection.framed = false;
        }
        DrawEditorLine();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Setup Menu", EditorStyles.boldLabel);

        if (intersection.GetInfoSize() > 0)
        {
            if (!addingNodes)
            {
                if (GUILayout.Button("Open node options"))
                {
                    addingNodes = true;
                    intersection.SetInfoIndexToFirst();
                    SceneView.RepaintAll();
                }
            }
            else
            {
                if (GUILayout.Button("Hide node options"))
                {
                    addingNodes = false;
                }
            }
        }
        if (addingNodes)
        {
            EntryNodesSetupMenu();
        }
        EditorGUILayout.Separator();
        EntryUsingGuideLine();
        EditorGUILayout.Separator();
        DrawEditorLine();
        if (!confirming)
        {
            if (GUILayout.Button("Nodes set, start drawing lanes"))
            {
                confirming = true;
            }
        }
        else
        {
            EditorGUILayout.LabelField("Are you sure?", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                inOutCount = intersection.GetInOutPositions(out inPositions, out outPositions);
                intersection.allNodesSet = true;
                GenerateExistingLanes();
                if (intersection.existingLanesChecked)
                {
                    intersection.inIndex = 0;
                    intersection.outIndex = 0;
                }
                else
                {
                    intersection.existingLaneIndex = 0;
                    SetCurrentExistingLaneValuesToInspector();
                }
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("No"))
            {
                confirming = false;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    /// <summary>
    /// Inspector in/out setup submenu for selecting in/out nodes.
    /// </summary>
    private void EntryNodesSetupMenu()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Select nodes", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        if (intersection.GetInfoSize() == 0)
        {
            ItalicLabel("There are no nodes in the selected area.");
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous"))
            {
                intersection.MoveInfoIndex(-1);
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Next"))
            {
                intersection.MoveInfoIndex(1);
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            Nodes n = intersection.GetSelectedNodeInfo(out NodeInOut inOut);
            if (inOut == NodeInOut.NotUsed)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set as in-node"))
                {
                    intersection.SetInOut(NodeInOut.InNode);
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Set as out-node"))
                {
                    intersection.SetInOut(NodeInOut.OutNode);
                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (n.ParallelLeft || n.ParallelRight)
                {
                    if (GUILayout.Button("Select adjacents also"))
                    {
                        intersection.SelectAdjacents();
                        SceneView.RepaintAll();
                    }
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Deselect"))
                {
                    intersection.SetInOut(NodeInOut.NotUsed);
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Deselect this and adjacents"))
                {
                    intersection.SetInOut(NodeInOut.NotUsed);
                    intersection.SelectAdjacents();
                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("Deselect all"))
            {
                intersection.SetInOutAll(NodeInOut.NotUsed);
                SceneView.RepaintAll();
            }
        }
    }
    /// <summary>
    /// Inspector in/out setup submenu, for using helper guideline.
    /// </summary>
    private void EntryUsingGuideLine()
    {
        DrawEditorLine();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Create new entry points", EditorStyles.boldLabel);
        if (showLine == false)
        {
            if (GUILayout.Button("Create new"))
            {
                linePos = intersection.CenterPoint;
                lineLength = intersection.FrameHeight * 0.5f;
                Vector3 p0 = linePos + new Vector3(0f, 0f, -lineLength / 2f);
                Vector3 p1 = linePos + new Vector3(0f, 0f, lineLength / 2f);
                linePoints = new Vector3[] { p0, p1 };
                lineDir = (p1 - p0).normalized;
                showLine = true;

                nodesOnLine = 0;
                nodePlaces = new float[6];
                tempPlaces = new float[6];
                lineNodesInOut = new NodeInOut[6];
                for (int i = 0; i < 6; i++)
                {
                    lineNodesInOut[i] = NodeInOut.InNode;
                }
                SceneView.RepaintAll();
            }
            EditorGUILayout.Separator();
            if (GUILayout.Button("Discard created points"))
            {
                intersection.RemoveHelperLines();
                UpdatePointsToDraw();
                SceneView.RepaintAll();
            }
        }
        else
        {
            LinePlacementMenu();
            if (GUILayout.Button("Cancel"))
            {
                showLine = false;
                SceneView.RepaintAll();
            }
        }
    }
    /// <summary>
    /// Inspector menu for confirming in/out setup information.
    /// </summary>
    private void ConfirmExistingLanesMenu()
    {
        EditorGUILayout.LabelField("Existing lanes", EditorStyles.boldLabel);
        ItalicLabel("Set and confirm values");
        int unconfirmed = intersection.GetUnconfirmedExistingLaneCount();
        if (unconfirmed == 0)
        {
            ItalicLabel("All lanes confirmed.");
        }
        else
        {
            WarningLabel("" + unconfirmed + " lanes left to confirm.");
        }
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            intersection.MoveExistingLaneIndex(-1);
            SetCurrentExistingLaneValuesToInspector();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Next"))
        {
            intersection.MoveExistingLaneIndex(1);
            SetCurrentExistingLaneValuesToInspector();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        ExistingLane ex = intersection.GetCurrentExistingLane();
        EditorGUILayout.LabelField("Existing lane " + intersection.existingLaneIndex + ":",
            EditorStyles.boldLabel);
        if (ex.confirmed)
        {
            ItalicLabel("Confirmed.");
        }
        else
        {
            WarningLabel("Not confirmed");
        }

        exTurn = (IntersectionDirection)EditorGUILayout.EnumPopup("Turn direction", exTurn);
        if (exTurn != ex.turnDirection)
        {
            ex.turnDirection = exTurn;
            intersection.SetCurrentExistingLane(ex);
        }
        exSpeedLimit = (SpeedLimits)EditorGUILayout.EnumPopup("Speed limit", exSpeedLimit);
        if (exSpeedLimit != ex.speedLimit)
        {
            ex.speedLimit = exSpeedLimit;
            intersection.SetCurrentExistingLane(ex);
        }
        exTraffic = (TrafficSize)EditorGUILayout.EnumPopup("Traffic", exTraffic);
        if (exTraffic != ex.traffic)
        {
            ex.traffic = exTraffic;
            intersection.SetCurrentExistingLane(ex);
        }
        if (!ex.confirmed)
        {
            WarningLabel("Confirm by selecting yield status:");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("RIGHT OF WAY"))
            {
                ex.laneType = LaneType.INTERSECTION_LANE_RIGHT_OF_WAY;
                ex.confirmed = true;               
                intersection.SetCurrentExistingLane(ex);
            }
            if (GUILayout.Button("YIELDING"))
            {
                ex.laneType = LaneType.INTERSECTION_LANE_YIELDING;
                ex.confirmed = true;
                intersection.SetCurrentExistingLane(ex);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            if (GUILayout.Button("Edit information"))
            {
                ex.confirmed = false;
                intersection.SetCurrentExistingLane(ex);
            }
        }
        if (unconfirmed == 0)
        {
            EditorGUILayout.Separator();
            if (GUILayout.Button("Done"))
            {
                intersection.existingLanesChecked = true;
            }
        }
    }
    /// <summary>
    /// Inspector main menu in spline setup phase (after in/out setup), for selecting edited spline.
    /// </summary>
    private void SplineSetupMenu()
    {
        NodeSelectorMenu();
        SplineSelectorMenu();
        SplineOptionsMenu();
        LanePropertiesMenu();
        FinishSplinesMenu();
    }
    /// <summary>
    /// Inspector in/out setup submenu and submenu of helper guideline menu, for helper line placement options.
    /// </summary>
    private void LinePlacementMenu()
    {
        EditorGUILayout.LabelField("Traffic settings", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        step = EditorGUILayout.FloatField("Step", step);
        EditorGUILayout.LabelField("Adjust position", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Up"))
        {
            linePos.z += step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Down"))
        {
            linePos.z -= step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Left"))
        {
            linePos.x -= step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Right"))
        {
            linePos.x += step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Adjust size", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Line size -"))
        {
            if (lineLength - step > 1f)
            {
                lineLength -= step;
            }
            else
            {
                lineLength = 1f;
            }
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Line size +"))
        {
            lineLength += step;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Adjust angle", EditorStyles.boldLabel);
        float angle = lineAngle;
        angle = EditorGUILayout.FloatField("Angle", angle);
        {
            if (angle != lineAngle)
            {
                lineAngle = angle % 360f;
            }
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-"))
        {
            lineYAngle = (360f + lineYAngle - lineAngle) % 360;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("+"))
        {
            lineYAngle = (360f + lineYAngle + lineAngle) % 360;
            UpdateLinePosition();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();

        int nodes = nodesOnLine;
        nodes = EditorGUILayout.IntField("Nodes (" + nodesOnLine + ")", nodesOnLine);
        if (nodes != nodesOnLine)
        {
            if (nodes > -1 && nodes < 7)
            {
                nodesOnLine = nodes;
                SetupLineNodes();
                SceneView.RepaintAll();
            }
        }
        if (nodesOnLine > 0)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Node placement on line (0-1)", EditorStyles.boldLabel);
        }
        for (int i = 0; i < nodesOnLine; i++)
        {
            tempPlaces[i] = EditorGUILayout.FloatField("" + (i+1) + " (" + nodePlaces[i] + ")", tempPlaces[i]);
            if (GUILayout.Button("Set"))
            {
                if (tempPlaces[i] != nodePlaces[i])
                {
                    bool isOk = true;
                    if (i == 0)
                    {
                        if (tempPlaces[i] < 0f)
                        {
                            isOk = false;
                        }
                    }
                    else
                    {
                        if (tempPlaces[i] <= nodePlaces[i - 1])
                        {
                            isOk = false;
                        }
                    }
                    if (i == nodesOnLine - 1)
                    {
                        if (tempPlaces[i] > 1f)
                        {
                            isOk = false;
                        }
                    }
                    else
                    {
                        if (tempPlaces[i] >= nodePlaces[i + 1])
                        {
                            isOk = false;
                        }
                    }
                    if (isOk)
                    {
                        nodePlaces[i] = tempPlaces[i];
                        SceneView.RepaintAll();
                    }
                    else
                    {
                        tempPlaces[i] = nodePlaces[i];
                    }
                }
            }

            bool isOut = false;
            if (lineNodesInOut[i] == NodeInOut.OutNode)
            {
                isOut = true;
            }
            bool check = isOut;
            isOut = EditorGUILayout.ToggleLeft("is out node?", isOut);
            if (isOut != check)
            {
                if (isOut)
                {
                    lineNodesInOut[i] = NodeInOut.OutNode;
                }
                else
                {
                    lineNodesInOut[i] = NodeInOut.InNode;
                }
                SceneView.RepaintAll();
            }
        }
        EditorGUILayout.Separator();
        if (nodesOnLine > 0)
        {
            if (GUILayout.Button("Done"))
            {
                SaveHelperLine();
                showLine = false;
                ResetLineValues();
                UpdatePointsToDraw();
                SceneView.RepaintAll();
            }
        }
    }
    /// <summary>
    /// Inspector node selector section.
    /// </summary>
    private void NodeSelectorMenu()
    {
        //in nodes
        EditorGUILayout.LabelField("Node selector", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Current in-node: " + intersection.inIndex);
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("IN -"))
        {
            int val = intersection.inIndex - 1;
            if (val < 0)
            {
                intersection.inIndex = inPositions.Count - 1;
            }
            else
            {
                intersection.inIndex--;
            }
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("IN +"))
        {
            int val = intersection.inIndex + 1;
            if (val > inPositions.Count - 1)
            {
                intersection.inIndex = 0;
            }
            else
            {
                intersection.inIndex = val;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        //out nodes
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Current out-node: " + intersection.outIndex);
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("OUT -"))
        {
            int val = intersection.outIndex - 1;
            if (val < 0)
            {
                intersection.outIndex = outPositions.Count - 1;
            }
            else
            {
                intersection.outIndex--;
            }
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("OUT +"))
        {
            int val = intersection.outIndex + 1;
            if (val > outPositions.Count - 1)
            {
                intersection.outIndex = 0;
            }
            else
            {
                intersection.outIndex = val;
            }
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        DrawEditorLine();
    }
    /// <summary>
    /// Inspector spline selector section.
    /// </summary>
    private void SplineSelectorMenu()
    {
        EditorGUILayout.LabelField("Spline selector", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            intersection.MoveSplineIndex(-1);
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Next"))
        {
            intersection.MoveSplineIndex(1);
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Add spline to selected in-node (" + intersection.inIndex + ")");
        if (GUILayout.Button("Add spline"))
        {
            CreateBezier();
            SceneView.RepaintAll();
        }
        DrawEditorLine();
    }
    /// <summary>
    /// Inspector spline setup phase submenu for editing splines.
    /// </summary>
    private void SplineOptionsMenu()
    {
        EditorGUILayout.LabelField("Spline options", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        if (GUILayout.Button("Connect to selected outnode (" + intersection.outIndex + ")"))
        {
            intersection.ConnectSplineToOutNode();
            SceneView.RepaintAll();
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("Add segment"))
        {
            intersection.AddSegmentToCurrentSpline();
            SceneView.RepaintAll();
        }
        EditorGUILayout.Separator();
        if (GUILayout.Button("Delete this spline"))
        {
            intersection.RemoveCurrentSpline();
            SceneView.RepaintAll();
        }
        EditorGUILayout.Separator();
        DrawEditorLine();
    }
    /// <summary>
    /// Lane properties section in inspector.
    /// </summary>
    private void LanePropertiesMenu()
    {
        EditorGUILayout.LabelField("Lane properties", EditorStyles.boldLabel);
        if (intersection.splineIndex == -1)
        {
            EditorGUILayout.LabelField("No created splines");
        }
        else
        {
            ItalicLabel("Spline " + intersection.splineIndex);
            intersection.createdSplines[intersection.splineIndex].turnDirection =
                (IntersectionDirection)EditorGUILayout.EnumPopup("Turn direction",
                intersection.createdSplines[intersection.splineIndex].turnDirection);
            EditorGUILayout.Separator();
            intersection.createdSplines[intersection.splineIndex].traffic =
                (TrafficSize)EditorGUILayout.EnumPopup("Traffic", intersection.createdSplines[intersection.splineIndex].traffic);
            EditorGUILayout.Separator();
            intersection.createdSplines[intersection.splineIndex].speedLimit =
                (SpeedLimits)EditorGUILayout.EnumPopup("Speed limit", intersection.createdSplines[intersection.splineIndex].speedLimit);
            EditorGUILayout.Separator();
            LaneType lt = intersection.createdSplines[intersection.splineIndex].laneType;
            if (lt == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
            {
                EditorGUILayout.LabelField("Yield status: RIGHT OF WAY");
                if (GUILayout.Button("CHANGE"))
                {
                    intersection.createdSplines[intersection.splineIndex].laneType = LaneType.INTERSECTION_LANE_YIELDING;
                }
            }
            else if (intersection.createdSplines[intersection.splineIndex].laneType == LaneType.INTERSECTION_LANE_YIELDING)
            {
                EditorGUILayout.LabelField("Yield status: GIVE WAY (Yielding)");
                if (GUILayout.Button("CHANGE"))
                {
                    intersection.createdSplines[intersection.splineIndex].laneType = LaneType.INTERSECTION_LANE_RIGHT_OF_WAY;
                }
            }

        }
        EditorGUILayout.Separator();
        DrawEditorLine();
    }
    /// <summary>
    /// Splines finished section in inspector.
    /// </summary>
    private void FinishSplinesMenu()
    {
        EditorGUILayout.LabelField("Finish splines", EditorStyles.boldLabel);
        ItalicLabel("When ALL splines are set, press 'Done'");
        if (GUILayout.Button("Done"))
        {
            allEndNodesConnected = intersection.AllSplineEndPointsConnected();
            if (allEndNodesConnected)
            {
                intersection.SetSegmentArrays();
                intersection.splinesSet = true;
                SceneView.RepaintAll();
            }

        }
        if (!allEndNodesConnected)
        {
            WarningLabel("End points of all splines must be connected to an out-node before" +
                "continuing");
        }
    }
    /// <summary>
    /// Inspector node setup main menu (after spline setup).
    /// </summary>
    private void NodeSetupMenu()
    {
        if (intersection.splineIndex > -1)
        {
            SplineData sd = intersection.createdSplines[intersection.splineIndex];
            int index = intersection.splineIndex;
            EditorGUILayout.LabelField("Set nodes on splines", EditorStyles.boldLabel);
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous"))
            {
                intersection.MoveSplineIndex(-1);
                intersection.GetSegmentNodePositions(out currentSplineNodes, out otherSplineNodes);
                nodesFetched = true;
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Next"))
            {
                intersection.MoveSplineIndex(1);
                intersection.GetSegmentNodePositions(out currentSplineNodes, out otherSplineNodes);
                nodesFetched = true;
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            ItalicLabel("Spline " + index);
            EditorGUILayout.Separator();
            for (int i = 0; i < sd.segmentNodes.Length; i++)
            {
                int pnts = sd.segmentNodes[i];
                pnts = EditorGUILayout.IntField("Nodes", pnts);
                if (pnts != sd.segmentNodes[i])
                {
                    if (pnts > -1 && pnts < 30)
                    {
                        sd.segmentNodes[i] = pnts;
                        intersection.GetSegmentNodePositions(out currentSplineNodes, out otherSplineNodes);
                        nodesFetched = true;
                    }
                }
            }
        }
        EditorGUILayout.Separator();
        DrawEditorLine();
        if (GUILayout.Button("Create Nodes"))
        {
            allSegmentsHaveNodes = intersection.NodesOnAllSegments();
            nameAutoChecked = false;
            if (allSegmentsHaveNodes && nameIsValid)
            {
                CreateIntersection();
            }
        }
        if (!allSegmentsHaveNodes)
        {
            WarningLabel("Some spline segments don't have nodes set.");
        }
        if (!nameIsValid)
        {
            WarningLabel("Please check the name");
        }
        DrawEditorLine();
        if (GUILayout.Button("Back"))
        {
            intersection.splinesSet = false;
            nodesFetched = false;
            SceneView.RepaintAll();
        }
    }
    /// <summary>
    /// Creates a new bezier spline in spline setup phase.
    /// </summary>
    private void CreateBezier()
    {
        
        if (intersection.createdSplines == null)
        {
            intersection.createdSplines = new SplineData[0];
        }
        SplineData sd = new SplineData();
        Vector3 pos = inPositions[intersection.inIndex];
        Nodes n = intersection.GetInNodeOfCurrentIndex();
        Vector3 dir;
        if (n!= null)
        {
            sd.startNode = n;
            dir = GetBezierStartDirection(pos, n);
        }
        else
        {
            dir = GetBezierStartDirection(pos);
        }
        float length = GetBezierStartLength();
        Vector3 p0 = pos;
        Vector3 p1 = pos + length / 3f * dir;
        Vector3 p2 = pos + length * 2f / 3f * dir;
        Vector3 p3 = pos + length * dir;
        sd.points = new Vector3[] { p0, p1, p2, p3 };
        sd.modes = new Bezier.ControlPointMode[] { Bezier.ControlPointMode.Aligned,
            Bezier.ControlPointMode.Aligned};
        sd.endPointSet = false;
        if (n != null)
        {
            ExistingLane ex = intersection.GetExistingLaneWithInNode();

            sd.turnDirection = ex.turnDirection;
            sd.laneType = ex.laneType;
            sd.speedLimit = ex.speedLimit;
            sd.traffic = ex.traffic;
        }
        else
        {
            sd.turnDirection = IntersectionDirection.Straight;
        }
        Array.Resize(ref intersection.createdSplines, intersection.createdSplines.Length + 1);
        intersection.createdSplines[intersection.createdSplines.Length - 1] = sd;
        intersection.splineIndex = intersection.createdSplines.Length - 1;

    }
    /// <summary>
    /// Returns default length for new bezier spline in spline setup phase. Default length is a half of shorter frame side length.
    /// </summary>
    /// <returns>Default bezier spline length</returns>
    private float GetBezierStartLength()
    {
        if (intersection.FrameHeight < intersection.FrameWidth)
        {
            return 0.5f * intersection.FrameHeight;
        }
        else
        {
            return 0.5f * intersection.FrameWidth;
        }
    }
    /// <summary>
    /// Returns default direction for new bezier spline. If spline is set to start from an existing node, the direction is derived
    /// from direction between bezier's start node and the node before that (in original lane nodes). Otherwise the direction is
    /// assumed to be along x- or z-axis, depending on start points distance from framing's centerpoint.
    /// </summary>
    /// <param name="pos">Start point's position.</param>
    /// <param name="n">Start node, can be null.</param>
    /// <returns></returns>
    private Vector3 GetBezierStartDirection(Vector3 pos, Nodes n = null)
    {
        if (n != null)
        {
            if (n.OutNode != null)
            {
                return (n.OutNode.gameObject.transform.position
                    - n.gameObject.transform.position).normalized;
            }
            else if (n.InNode != null)
            {
                return (n.gameObject.transform.position
                    - n.InNode.gameObject.transform.position).normalized;
            }
        }
        Vector3 d = intersection.CenterPoint - pos;
        if (Mathf.Abs(d.x) > Mathf.Abs(d.z))
        {
            return new Vector3(d.x, 0f, 0f).normalized;
        }
        else
        {
            return new Vector3(0f, 0f, d.z).normalized;
        }
    }
    /// <summary>
    /// Saves helper line information in target intersection-object.
    /// </summary>
    private void SaveHelperLine()
    {
        HelperLine h = new HelperLine();
        h.startPoint = linePoints[0];
        h.direction = lineDir;
        h.lenght = lineLength;
        List<float> pnts = new List<float>();
        List<NodeInOut> ios = new List<NodeInOut>();
        for (int i = 0; i < nodesOnLine; i++)
        {
            pnts.Add(nodePlaces[i]);
            ios.Add(lineNodesInOut[i]);
        }
        h.nodePoints = pnts;
        h.inOut = ios;
        if (intersection.helperLines == null)
        {
            intersection.helperLines = new List<HelperLine>();
        }
        intersection.helperLines.Add(h);
    }
    /// <summary>
    /// Resets helper line values in inspector (doesn't affect target object).
    /// </summary>
    private void ResetLineValues()
    {
        nodePlaces = null;
        tempPlaces = null;
        lineNodesInOut = null;
        lineYAngle = 0f;
        lineAngle = 5f;
    }
    /// <summary>
    /// After in/out setup, generates existing lane info to target intersection-object.
    /// </summary>
    private void GenerateExistingLanes()
    {
        List<ExistingLane> existingLanes = new List<ExistingLane>();
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            Nodes currentNode = intersection.nodesInBox[i].node;
            List<Nodes> laneNodes = new List<Nodes>();
            bool isLane = false;
            int inNodeIndex = -1;
            int outNodeIndex = -1;
            // start node must be in-node
            SpeedLimits speedLimit = currentNode.ParentLane.SpeedLimit;
            TrafficSize traffic = currentNode.ParentLane.Traffic;

            if (intersection.nodesInBox[i].inOut == NodeInOut.InNode)
            {
                //Get in node index
                inNodeIndex = GetInNodeIndex(currentNode);
                while (true)
                {
                    // Add current node to list
                    laneNodes.Add(currentNode);
                    // exit if node is not in the box
                    if (!IsInBoxNodes(currentNode))
                    {
                        break;
                    }
                    // check if the node is out-node
                    if (IsOutNode(currentNode))
                    {
                        // Get out node index
                        outNodeIndex = GetOutNodeIndex(currentNode);
                        isLane = true;
                        break;
                    }
                    // exit if there is not a linked next node
                    if (currentNode.OutNode == null)
                    {
                        break;
                    }
                    else
                    {
                        // iterate next node
                        currentNode = currentNode.OutNode;
                    }
                }
            }
            if (inNodeIndex > -1)
            {
                ExistingLane ex = new ExistingLane();
                ex.nodes = laneNodes;
                ex.confirmed = false;
                ex.laneType = LaneType.INTERSECTION_LANE_RIGHT_OF_WAY;
                ex.inNodeIndex = inNodeIndex;
                ex.outNodeIndex = outNodeIndex;
                ex.turnDirection = IntersectionDirection.Straight;
                ex.traffic = traffic;
                ex.speedLimit = speedLimit;
                ex.isLane = isLane;
                existingLanes.Add(ex);
            }
        }
        intersection.existingLanes = existingLanes;
        // Get positions for drawing existing lanes
        GetExistingLaneLinesToDraw();
        if (existingLanes.Count==0)
        {
            intersection.existingLanesChecked = true;
        }
        else
        {
            intersection.existingLanesChecked = false;
            intersection.existingLaneIndex = 0;
        }
    }
    /// <summary>
    /// Derives existing lane node positions from target intersection-object and generates a position array for
    /// point-to-point drawing in sceneview.
    /// </summary>
    private void GetExistingLaneLinesToDraw()
    {
        existingLaneLinesToDraw = new List<Vector3>();
        if (intersection.existingLanes == null)
        {
            return;
        }
        for (int i = 0; i < intersection.existingLanes.Count; i++)
        {
            ExistingLane ex = intersection.existingLanes[i];
            for (int j = 0; j < ex.nodes.Count - 1; j++)
            {
                existingLaneLinesToDraw.Add(ex.nodes[j].gameObject.transform.position);
                existingLaneLinesToDraw.Add(ex.nodes[j + 1].gameObject.transform.position);
            }
        }
    }
    /// <summary>
    /// Helper function for checking if given node inside framing area.
    /// </summary>
    /// <param name="n">Checked node.</param>
    /// <returns>Is checked node inside framing area?</returns>
    private bool IsInBoxNodes(Nodes n)
    {
        bool isTrue = false;
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            if (intersection.nodesInBox[i].node == n)
            {
                isTrue = true;
                break;
            }
        }
        return isTrue;
    }
    /// <summary>
    /// Checks from the list of nodes inside framing area, if given node is an out-node.
    /// </summary>
    /// <param name="n">Checked node.</param>
    /// <returns>Is node an out-node?</returns>
    private bool IsOutNode(Nodes n)
    {
        bool isTrue = false;
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            if (intersection.nodesInBox[i].node == n)
            {
                if (intersection.nodesInBox[i].inOut == NodeInOut.OutNode)
                {
                    isTrue = true;
                }
                break;
            }
        }
        return isTrue;
    }
    /// <summary>
    /// Returns index of how manyth in-node of the list of all nodes inside the framing area given node is
    /// (starts from 0, returns -1 if search fails).
    /// </summary>
    /// <param name="n">Checked node.</param>
    /// <returns>Index of how manyth in-node given node is (starts from 0, returns -1 if search fails).</returns>
    private int GetInNodeIndex(Nodes n)
    {
        int ind = -1;
        bool found = false;
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            NodeInfo ni = intersection.nodesInBox[i];
            if (ni.inOut == NodeInOut.InNode)
            {
                ind++;
            }
            if (ni.node == n)
            {
                found = true;
                break;
            }
        }
        if (found)
        {
            return ind;
        }
        else
        {
            return -1;
        }
    }
    /// <summary>
    /// Returns index of how manyth out-node of the list of all nodes inside the framing box the given node is
    /// (starting from 0, returns -1 if search fails).
    /// </summary>
    /// <param name="n">Checked node.</param>
    /// <returns>Index of how manyth out-node given node is (starts from 0, returns -1 if search fails).</returns>
    private int GetOutNodeIndex(Nodes n)
    {
        int ind = -1;
        bool found = false;
        for (int i = 0; i < intersection.nodesInBox.Length; i++)
        {
            NodeInfo ni = intersection.nodesInBox[i];
            if (ni.inOut == NodeInOut.OutNode)
            {
                ind++;
            }
            if (ni.node == n)
            {
                found = true;
                break;
            }
        }
        if (found)
        {
            return ind;
        }
        else
        {
            return -1;
        }
    }
    /// <summary>
    /// Retrieves data for currently active existing lane from target intersection-object.
    /// </summary>
    private void SetCurrentExistingLaneValuesToInspector()
    {
        ExistingLane ex = intersection.GetCurrentExistingLane();
        if (ex != null)
        {
            intersection.inIndex = ex.inNodeIndex;
            intersection.outIndex = ex.outNodeIndex;
            exTurn = ex.turnDirection;
            exConfirmed = ex.confirmed;
            exSpeedLimit = ex.speedLimit;
            exTraffic = ex.traffic;
        }
    }
    /// <summary>
    /// Retrieves in/out node positions from target intersection-object.
    /// </summary>
    private void UpdatePointsToDraw()
    {
        pointsToDraw = new List<Vector3>();
        if (intersection.helperLines == null)
        { return; }
        pointsToDrawColors = new List<Color>();
        for (int i = 0; i < intersection.helperLines.Count; i++)
        {
            HelperLine h = intersection.helperLines[i];
            Vector3 dir = h.direction;
            float lenght = h.lenght;
            Vector3 p0 = h.startPoint;
            for (int j = 0; j < h.nodePoints.Count; j++)
            {
                Vector3 pnt = p0 + h.nodePoints[j] * lenght * dir;
                Color c = Color.blue;
                if (h.inOut[j] == NodeInOut.OutNode)
                {
                    c = Color.red;
                }
                pointsToDraw.Add(pnt);
                pointsToDrawColors.Add(c);
            }
        }
    }
    /// <summary>
    /// Sets default positions for helper line nodes, depending on number of nodes along line.
    /// </summary>
    private void SetupLineNodes()
    {
        switch (nodesOnLine)
        {
            case 1:
                nodePlaces[0] = 0.5f;
                nodePlaces[1] = 1.0f;
                nodePlaces[2] = 1.0f;
                nodePlaces[3] = 1.0f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 2:
                nodePlaces[0] = 0.25f;
                nodePlaces[1] = 0.75f;
                nodePlaces[2] = 1.0f;
                nodePlaces[3] = 1.0f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 3:
                nodePlaces[0] = 0.25f;
                nodePlaces[1] = 0.5f;
                nodePlaces[2] = 0.75f;
                nodePlaces[3] = 1.0f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 4:
                nodePlaces[0] = 0.2f;
                nodePlaces[1] = 0.4f;
                nodePlaces[2] = 0.6f;
                nodePlaces[3] = 0.8f;
                nodePlaces[4] = 1.0f;
                nodePlaces[5] = 1.0f;
                break;
            case 5:
                nodePlaces[0] = 0.2f;
                nodePlaces[1] = 0.35f;
                nodePlaces[2] = 0.5f;
                nodePlaces[3] = 0.65f;
                nodePlaces[4] = 0.8f;
                nodePlaces[5] = 1.0f;
                break;
            case 6:
                nodePlaces[0] = 0.125f;
                nodePlaces[1] = 0.275f;
                nodePlaces[2] = 0.425f;
                nodePlaces[3] = 0.575f;
                nodePlaces[4] = 0.725f;
                nodePlaces[5] = 0.875f;
                break;
        }
        tempPlaces[0] = nodePlaces[0];
        tempPlaces[1] = nodePlaces[1];
        tempPlaces[2] = nodePlaces[2];
        tempPlaces[3] = nodePlaces[3];
        tempPlaces[4] = nodePlaces[4];
        tempPlaces[5] = nodePlaces[5];
    }
    /// <summary>
    /// At the end of intersection creation, this function actually creates a new road-object, its child lanes and their child nodes,
    /// plus modifies existing lanes.
    /// </summary>
    private void CreateIntersection()
    {
        string parentName = intersectionName;

        // Create parent gameobject
        GameObject parent = new GameObject(parentName);
        parent.transform.position = intersection.CenterPoint;
        parent.AddComponent<Road>();

        // Existing lanes
        int laneObjectIndex = CreateLanesFromExistingLanes(parent);

        // Lanes from spline data
        CreateLanesFromSplineData(laneObjectIndex, parent);
        //Add info of crossings lanes
        CalculateCrossingLanes(parent);
        // finally parent intersection gameobject to roadnetwork
        parent.transform.parent = intersection.roadNetwork.transform;
        // Add IntersectionController
        Intersection i = parent.AddComponent<Intersection>();
        ICNoLightsController icnl = parent.AddComponent<ICNoLightsController>();
        icnl.intersection = i;
        i.noLightsController = icnl;
        i.currentController = icnl;
    }
    
    // modifies lanes and adds Lane-gameobjects (with list of nodes but no child objects) to parent
    /// <summary>
    /// When a new road-object is created, this function handles creating new lanes from existing lanes.
    /// </summary>
    /// <param name="parent">Parent gameobject for created road.</param>
    /// <returns>A number of created lanes (this is used later in naming when other lanes are created from spline data).</returns>
    private int CreateLanesFromExistingLanes(GameObject parent)
    {
        int index = 0;
        if (intersection.existingLanes == null)
        {
            return index;
        }

        List<Road> splitRoads = new List<Road>();
        List<Lane> splitLanes = new List<Lane>();

        for (int i = 0; i < intersection.existingLanes.Count; i++)
        {
            ExistingLane ex = intersection.existingLanes[i];
            if (ex.outNodeIndex == -1 || ex.inNodeIndex == -1)
            {
                continue;
            }
            // Split roads if necessary
            SplitRoads(ex, ref splitRoads, ref splitLanes);

            // Create parent object (Lane) and assign values
            string laneName = parent.gameObject.name + "_" + i + "_" + "ex";

            GameObject g = new GameObject(laneName);
            g.transform.position = ex.nodes[0].transform.position;
            g.AddComponent(typeof(Lane));

            Lane lane = g.GetComponent<Lane>();
            lane.Traffic = ex.traffic;
            lane.laneType = ex.laneType;
            //lane.LaneYield = ex.laneYield;
            lane.SpeedLimit = ex.speedLimit;
            lane.TurnDirection = ex.turnDirection;
            Lane originalLane = ex.nodes[ex.nodes.Count - 1].transform.parent.GetComponent<Lane>();
            lane.isRightLane = originalLane.isRightLane;

            int originalArraySize = originalLane.nodesOnLane.Length - ex.nodes.Count;
            if (!originalLane.isRightLane)
            {
                int count = ex.nodes.Count;
                for (int j = count; j < originalLane.nodesOnLane.Length; j++)
                {
                    originalLane.nodesOnLane[j - count] = originalLane.nodesOnLane[j];
                }
            }
            Array.Resize(ref originalLane.nodesOnLane, originalArraySize);
            originalLane.nodesOnLane[0].AddLaneStart(originalLane);
            // Iterate nodes
            List<Nodes> nodes = new List<Nodes>();
            for (int j = 0; j < ex.nodes.Count; j++)
            {
                Nodes n = ex.nodes[j];
                n.ParallelLeft = null;
                n.ParallelRight = null;
                n.LaneChangeLeft = null;
                n.LaneChangeRight = null;
                if (j == 0)
                {
                    n.AddLaneStart(lane);
                }
                nodes.Add(n);
            }
            // Add nodes to lane's node array and re-parent them
            lane.nodesOnLane = new Nodes[nodes.Count];
            for (int j = 0; j < nodes.Count; j++)
            {
                lane.nodesOnLane[j] = nodes[j];
                nodes[j].transform.parent = lane.transform;
                nodes[j].ParentLane = lane;
            }
            // parent lane
            lane.transform.parent = parent.transform;
            // Tag nodes
            ObjectTagger.SetLaneIcons(ObjectTagger.TagColorScheme.ByLaneNumber, i % 6, ref lane.nodesOnLane);
            ObjectTagger.SetLaneIcons(ObjectTagger.TagColorScheme.ByLaneNumber, i % 6, ref originalLane.nodesOnLane);
            index++;
        }
        for (int i = 0; i < splitLanes.Count; i++)
        {
            Lane sp = splitLanes[i];
            sp.nodesOnLane[0].AddLaneStart(sp);
            ObjectTagger.SetLaneIcons(ObjectTagger.TagColorScheme.ByLaneNumber, i % 6, ref sp.nodesOnLane);
        }
        return index;
    }
    /// <summary>
    /// Splits roads when lanes are created from existing lanes.
    /// </summary>
    /// <param name="ex">Existing lane data saved in target intersection-object.</param>
    /// <param name="splitRoads">A list of already split roads.</param>
    /// <param name="splitLanes">A list of already split lanes.</param>
    private void SplitRoads(ExistingLane ex, ref List<Road> splitRoads, ref List<Lane> splitLanes)
    {
        Lane l = ex.nodes[0].transform.parent.GetComponent<Lane>();
        Road r = l.transform.parent.GetComponent<Road>();
        bool notSplit = true;
        for (int i = 0; i < splitRoads.Count; i++)
        {
            if (splitRoads[i] == r)
            {
                notSplit = false;
                break;
            }
        }
        if (notSplit)
        {
            int nodeIndex = 0;
            Nodes spliAfterNode = null;
            if (l.isRightLane)
            {
                spliAfterNode = ex.nodes[ex.nodes.Count - 1];
            }
            else
            {
                spliAfterNode = ex.nodes[0];
            }
            for (int i = l.nodesOnLane.Length - 1; i >= 0; i--)
            {
                if (l.nodesOnLane[i] == spliAfterNode)
                {
                    nodeIndex = i;
                    break;
                }
            }
            if (!l.isRightLane)
            {
                nodeIndex = l.nodesOnLane.Length - 1 - nodeIndex;
            }
            GameObject newRoad;
            r.SplitRoadAfterNode(nodeIndex, out newRoad);
            if (newRoad != null)
            {
                Lane[] childLanes = r.GetComponentsInChildren<Lane>();
                Lane[] otherLanes = newRoad.GetComponentsInChildren<Lane>();
                for (int i = 0; i < childLanes.Length; i++)
                {
                    ObjectTagger.SetLaneIcons(ObjectTagger.TagColorScheme.ByLaneNumber, i % 6, ref childLanes[i].nodesOnLane);
                    ObjectTagger.SetLaneIcons(ObjectTagger.TagColorScheme.ByLaneNumber, i % 6, ref otherLanes[i].nodesOnLane);
                }
            }

            splitRoads.Add(r);
            Lane[] rLanes = r.gameObject.GetComponentsInChildren<Lane>();
            for (int j = 0; j < rLanes.Length; j++)
            {
                splitLanes.Add(rLanes[j]);
            }
        }
    }
    /// <summary>
    /// At the end of intersection creation and after lanes are created from existing lanes, the rest of the lanes are created
    /// from spline data.
    /// </summary>
    /// <param name="laneObjectIndex">Current lane index, new lanes are named with incrementing this index.</param>
    /// <param name="parent">Parent gameobject.</param>
    private void CreateLanesFromSplineData(int laneObjectIndex, GameObject parent)
    {
        if (intersection.createdSplines == null)
        {
            return;
        }
        List<Nodes> newInNodes = new List<Nodes>();
        List<Nodes> newOutNodes = new List<Nodes>();
        string parentName = parent.gameObject.name;
        for (int splineInd = 0; splineInd < intersection.createdSplines.Length; splineInd++)
        {
            string laneName = parentName + "_" + laneObjectIndex;
            SplineData sd = intersection.createdSplines[splineInd];

            // Create lane object
            GameObject laneObject = new GameObject(laneName);
            laneObject.transform.position = sd.points[0];
            laneObject.AddComponent(typeof(Lane));
            Lane lane = laneObject.GetComponent<Lane>();
            laneObject.transform.parent = parent.transform;

            Nodes startNode = null;
            Nodes endNode = null;

            // assign start node, generate a new one if it doesn't exist
            if (sd.startNode != null)
            {
                startNode = sd.startNode;
                startNode.ParallelLeft = null;
                startNode.ParallelRight = null;
                startNode.LaneChangeLeft = null;
                startNode.LaneChangeRight = null;
            }
            else
            {
                Vector3 pos = sd.points[0];
                bool found = false;
                for ( int i = 0; i < newInNodes.Count; i++)
                {
                    if (newInNodes[i].transform.position == pos)
                    {
                        startNode = newInNodes[i];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    string nodeName = laneName + "_" + 0;
                    startNode = GenerateNode(pos, nodeName);
                    startNode.ParentLane = lane;
                    startNode.transform.parent = laneObject.transform;
                    newInNodes.Add(startNode);                  
                }
            }
            // assign end node, generate a new one if it doesn't exist
            if (sd.endNode != null)
            {
                endNode = sd.endNode;
                endNode.ParallelLeft = null;
                endNode.ParallelRight = null;
                endNode.LaneChangeLeft = null;
                endNode.LaneChangeRight = null;
            }
            else
            {
                Vector3 pos = sd.points[sd.points.Length - 1];
                bool found = false;
                for (int i = 0; i < newOutNodes.Count; i++)
                {
                    if (newOutNodes[i].transform.position == pos)
                    {
                        endNode = newOutNodes[i];
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    string nodeName = laneName + "_" + (sd.points.Length - 1);
                    endNode = GenerateNode(pos, nodeName);
                    endNode.ParentLane = lane;
                    endNode.transform.parent = laneObject.transform;
                    newOutNodes.Add(endNode);
                }
            }
            // positions for the rest of the nodes
            List<Vector3> inBetweenPositions = intersection.GetNodePositionInBetweenEndPoints(splineInd);
            // Create nodes in between start and end nodes
            List<Nodes> bNodes = new List<Nodes>();
            for (int i = 0; i < inBetweenPositions.Count; i++)
            {
                string nodeName = laneName + "_" + (i + 1);
                Nodes n = GenerateNode(inBetweenPositions[i],  nodeName);
                bNodes.Add(n);
            }
            // Assign values to lane
            lane.TurnDirection = sd.turnDirection;
            lane.Traffic = sd.traffic;
            lane.laneType = sd.laneType;
            lane.SpeedLimit = sd.speedLimit;

            // Assign nodes to lane
            lane.nodesOnLane = new Nodes[inBetweenPositions.Count + 2];
            lane.nodesOnLane[0] = startNode;
            for (int i = 0; i < bNodes.Count; i++)
            {
                lane.nodesOnLane[i + 1] = bNodes[i];
            }
            lane.nodesOnLane[lane.nodesOnLane.Length - 1] = endNode;
            for (int i = 0; i < lane.nodesOnLane.Length; i++)
            {
                Nodes n = lane.nodesOnLane[i];
                if (i == 0)
                {
                    if (!n.transform.parent.transform.parent.name.Equals(parent.name))
                    {
                        Array.Resize(ref n.ParentLane.nodesOnLane, n.ParentLane.nodesOnLane.Length - 1);
                        n.ParentLane = lane;
                        n.transform.parent = lane.transform;
                    }
                    n.AddLaneStart(lane);
                }
                else if (i == lane.nodesOnLane.Length - 1)
                {
                    if (!n.transform.parent.transform.parent.name.Equals(parent.name))
                    {
                        for (int j = 0; j < n.ParentLane.nodesOnLane.Length - 1; j++)
                        {
                            n.ParentLane.nodesOnLane[j] = n.ParentLane.nodesOnLane[j + 1];
                        }
                        Array.Resize(ref n.ParentLane.nodesOnLane, n.ParentLane.nodesOnLane.Length - 1);
                        n.ParentLane = lane;
                        n.transform.parent = lane.transform;
                        n.ClearStartNodes();
                        Lane otherLane = n.OutNode.ParentLane;
                        n.OutNode.AddLaneStart(otherLane);
                        ObjectTagger.SetLaneIcons(ObjectTagger.TagColorScheme.ByLaneNumber, laneObjectIndex % 6, ref n.OutNode.ParentLane.nodesOnLane);
                    }
                }
                else
                {
                    n.ParentLane = lane;
                    n.transform.parent = lane.transform;
                }

                // connect in and out nodes
                if (i > 0)
                {
                    n.InNode = lane.nodesOnLane[i - 1];
                }
                if (i < lane.nodesOnLane.Length - 1)
                {
                    n.OutNode = lane.nodesOnLane[i + 1];
                }
            }
            ObjectTagger.SetLaneIcons(ObjectTagger.TagColorScheme.ByLaneNumber, laneObjectIndex % 6, ref lane.nodesOnLane);
            laneObjectIndex++;
        }
    }
    /// <summary>
    /// Instantiates a new gameobject with Nodes component.
    /// </summary>
    /// <param name="position">Gameobject position.</param>
    /// <param name="goName">Name for new gameobject.</param>
    /// <returns>Instantiated Nodes-object.</returns>
    private Nodes GenerateNode (Vector3 position, string goName)
    {
        GameObject g = new GameObject(goName);
        g.AddComponent(typeof(Nodes));
        g.transform.position = position;
        Nodes n = g.GetComponent<Nodes>();
        return n;
    }
    /// <summary>
    /// Generates lane crossing information for each lane (or driving line) of intersection road-object. 
    /// </summary>
    /// <param name="roadObject">Parent (road) gameobject.</param>
    private void CalculateCrossingLanes(GameObject roadObject)
    {
        Lane[] lanes = roadObject.GetComponentsInChildren<Lane>();
        for (int l_index = 0; l_index < lanes.Length; l_index++)
        {
            Lane l = lanes[l_index];
            l.CrossingLanes = null;

            for (int other_l_index = 0; other_l_index < lanes.Length; other_l_index++)
            {
                Lane otherLane = lanes[other_l_index];
                if (other_l_index == l_index)
                {
                    continue;
                }
                if (l.nodesOnLane[0] == otherLane.nodesOnLane[0])
                {
                    continue;
                }
                bool found = false;
                Vector2 intersectionPoint = Vector2.zero;
                for (int i = 0; i < l.nodesOnLane.Length - 1; i++)
                {
                    Nodes n1 = l.nodesOnLane[i];
                    Nodes n2 = l.nodesOnLane[i + 1];
                    Vector2 A1 = new Vector2(n1.transform.position.x, n1.transform.position.z);
                    Vector2 A2 = new Vector2(n2.transform.position.x, n2.transform.position.z);
                    for (int j = 0; j < otherLane.nodesOnLane.Length - 1; j++)
                    {
                        Nodes n3 = otherLane.nodesOnLane[j];
                        Nodes n4 = otherLane.nodesOnLane[j + 1];
                        Vector2 B1 = new Vector2(n3.transform.position.x, n3.transform.position.z);
                        Vector2 B2 = new Vector2(n4.transform.position.x, n4.transform.position.z);
                        intersectionPoint = GetIntersectionPointCoordinates(A1, A2, B1, B2, out found);
                        if (found)
                        {
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (found)
                {
                    LaneCrossingPoint cp = new LaneCrossingPoint();
                    cp.crossingPoint = intersectionPoint;
                    cp.otherLane = otherLane;
                    l.AddCrossingLane(cp);
                }
            }
        }
    }
    /// <summary>
    /// Calculates the point where two 2D-vectors intersect.
    /// </summary>
    /// <param name="A1">Vector A start point.</param>
    /// <param name="A2">Vector A end point.</param>
    /// <param name="B1">Vector B start point.</param>
    /// <param name="B2">Vector B end point.</param>
    /// <param name="found">Returns true if vectors intersect.</param>
    /// <returns>Point where vectors intersect.</returns>
    private Vector2 GetIntersectionPointCoordinates (Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
        if (tmp == 0)
        {
            found = false;
            return Vector2.zero;
        }
        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        Vector2 point = new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
            );
        bool inArea = true;
        if (point.x > A1.x && point.x > A2.x)
        {
            inArea = false;
        }
        if (point.x > B1.x && point.x > B2.x)
        {
            inArea = false;
        }
        if (point.x < A1.x && point.x < A2.x)
        {
            inArea = false;
        }
        if (point.x < B1.x && point.x < B2.x)
        {
            inArea = false;
        }
        if (point.y > A1.y && point.y > A2.y)
        {
            inArea = false;
        }
        if (point.y > B1.y && point.y > B2.y)
        {
            inArea = false;
        }
        if (point.y < A1.y && point.y < A2.y)
        {
            inArea = false;
        }
        if (point.y < B1.y && point.y < B2.y)
        {
            inArea = false;
        }
        found = inArea;
        if (inArea)
        {
            return point;
        }
        else
        {
            return Vector2.zero;
        }
    }

    /// <summary>
    /// Checks if given name is valid (not in use).
    /// </summary>
    /// <param name="name">Checked string.</param>
    /// <returns>A boolean if name is valid.</returns>
    private bool CheckName(string name)
    {
        if (roadNetwork == null)
        {
            return false;
        }
        Transform t = roadNetwork.transform.Find(name);
        if (t == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// A helper function, draws a 2 points thick separator line in inspector.
    /// </summary>
    private void DrawEditorLine()
    {
        int thickness = 2;
        int padding = 10;
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, Color.black);
    }
    /// <summary>
    /// A helper functions, prints an italic text line in inspector.
    /// </summary>
    /// <param name="message"></param>
    private void ItalicLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.fontStyle = FontStyle.Italic;
        gs.wordWrap = true;
        EditorGUILayout.LabelField(message, gs);
    }
    /// <summary>
    /// A helper function, prints a red warning text in inspector (word wrap).
    /// </summary>
    /// <param name="message"></param>
    private void WarningLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.normal.textColor = Color.red;
        gs.wordWrap = true;
        EditorGUILayout.LabelField(message, gs);
    }

    //************************ SCENE VIEW START

    /// <summary>
    /// Built-in sceneview object drawing function.
    /// </summary>
    private void OnSceneGUI()
    {
        handleTransform = intersection.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;
        Handles.color = Color.white;
        Handles.DrawLines(framingHorizontal);
        Handles.DrawLines(framingVertical);
        HighlightNodes();
        if (showLine)
        {
            ShowMarkerLine();
            if (nodesOnLine > 0)
            {
                ShowNodePlacesOnLine();
            }
        }
        if (pointsToDraw != null || pointsToDraw.Count > 0)
        {
            ShowSavedHelperPoints();
        }
        if (intersection.allNodesSet)
        {
            if (inPositions == null || outPositions == null)
            {
                inOutCount = intersection.GetInOutPositions(out inPositions, out outPositions);
            }
            ShowNodeNumbers();
            ShowExistingLanes();
            ShowBeziers();
            if (intersection.splinesSet)
            {
                ShowSplineNodes();
            }
        }
    }
    /// <summary>
    /// Draws a round disc around the position of given object in sceneview.
    /// </summary>
    /// <param name="targetObject">Highlighted object.</param>
    /// <param name="c">Disc color.</param>
    /// <param name="larger">If true, a larger disc is drawn.</param>
    private void DrawSceneDisc(GameObject targetObject, Color c, bool larger)
    {
        Handles.color = c;
        float m = 0.01f;
        if (larger)
        {
            m = 0.015f;
        }
        Handles.DrawSolidDisc(targetObject.transform.position, new Vector3(0f, 1f, 0f),
            m * Vector3.Distance(targetObject.transform.position,
            SceneView.lastActiveSceneView.camera.transform.position));
    }
    /// <summary>
    /// Draws a round disc at the given position in sceneview.
    /// </summary>
    /// <param name="pos">Highlighted position.</param>
    /// <param name="c">Disc color.</param>
    /// <param name="larger">If true, a larger disc is drawn.</param>
    private void DrawSceneDisc(Vector3 pos, Color c, bool larger)
    {
        Handles.color = c;
        float m = 0.01f;
        if (larger)
        {
            m = 0.015f;
        }
        Handles.DrawSolidDisc(pos, new Vector3(0f, 1f, 0f), m * Vector3.Distance(
            pos, SceneView.lastActiveSceneView.camera.transform.position));
    }
    /// <summary>
    /// Draws numbered labels on in- and out-nodes in sceneview.
    /// </summary>
    private void ShowNodeNumbers()
    {
        GUIStyle g = new GUIStyle();
        g.normal.textColor = Color.white;
        for (int i = 0; i < inPositions.Count; i++)
        {
            Handles.Label(inPositions[i], "In " + i, g);
        }

        for (int i = 0; i < outPositions.Count; i++)
        {
            Handles.Label(outPositions[i], "Out " + i, g);
        }
    }
    /// <summary>
    /// Draws existing lanes in sceneview.
    /// </summary>
    private void ShowExistingLanes()
    {
        if (existingLaneLinesToDraw != null)
        {
            Handles.color = Color.green;
            for (int i = 0; i < existingLaneLinesToDraw.Count; i += 2)
            {
                Handles.DrawLine(existingLaneLinesToDraw[i], existingLaneLinesToDraw[i + 1]);
            }
        }
    }
    /// <summary>
    /// Draws lane (driving line) bezier curve splines in sceneview, currently selected curve is drawn with
    /// other color than the rest.
    /// </summary>
    private void ShowBeziers()
    {
        if (intersection.splineIndex == -1)
        {
            return;
        }
        for (int i = 0; i < intersection.createdSplines.Length; i++)
        {
            SplineData sd = intersection.createdSplines[i];
            Vector3 p0 = sd.points[0];
            for (int j = 1; j < sd.points.Length; j += 3)
            {
                Vector3 p1 = sd.points[j];
                Vector3 p2 = sd.points[j + 1];
                Vector3 p3 = sd.points[j + 2];
                Color c = Color.gray;
                if (i == intersection.splineIndex)
                {
                    c = Color.magenta;
                    DrawControlPoint(i, j);
                    DrawControlPoint(i, j + 1);
                    if (!(j + 3 == sd.points.Length && sd.endPointSet))
                    {
                        DrawControlPoint(i, j + 2);
                    }
                    if (!intersection.splinesSet)
                    {
                        Handles.DrawLine(p0, p1);
                        Handles.DrawLine(p2, p3);
                    }
                }
                Handles.color = Color.gray;
                
                Handles.DrawBezier(p0, p3, p1, p2, c, null, 2f);
                p0 = p3;
            }
        }
    }

    /// <summary>
    /// Draws nodes on bezier curve splines.
    /// </summary>
    private void ShowSplineNodes()
    {
        if (nodesFetched)
        {
            if (currentSplineNodes != null)
            {
                for (int i = 0; i < currentSplineNodes.Count; i++)
                {
                    DrawSceneDisc(currentSplineNodes[i], Color.yellow, true);
                }
            }
            if (otherSplineNodes != null)
            {
                for (int i = 0; i < otherSplineNodes.Count; i++)
                {
                    DrawSceneDisc(otherSplineNodes[i], Color.gray, false);
                }
            }
        }
    }
    /// <summary>
    /// Draws interactive control points in sceneview.
    /// </summary>
    /// <param name="splineIndex">Index of currently active spline</param>
    /// <param name="pointIndex">Index of currently active bezier curve node.</param>
    private void DrawControlPoint(int splineIndex, int pointIndex)
    {
        if (intersection.splinesSet)
        {
            return;
        }
        SplineData sd = intersection.createdSplines[splineIndex];
        Vector3 point = sd.points[pointIndex];
        float size = HandleUtility.GetHandleSize(point);

        Handles.color = Color.cyan;
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedIndex = new Vector2Int (splineIndex, pointIndex);
            Repaint(); // refresh inspector
        }
        if (selectedIndex.x == splineIndex && selectedIndex.y == pointIndex)
        {
            Event e = Event.current;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var eventType = e.GetTypeForControl(controlID);

            EditorGUI.BeginChangeCheck();
            
            point = Handles.DoPositionHandle(point, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(intersection, "MovePoint");
                EditorUtility.SetDirty(intersection);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                intersection.SetSplinePoint(splineIndex, pointIndex, point);
            }
        }
    }
    /// <summary>
    /// Draws a helper line for setting new in/out nodes in sceneview.
    /// </summary>
    private void ShowMarkerLine()
    {
        Handles.color = Color.cyan;
        Handles.DrawLine(linePoints[0], linePoints[1]);
    }
    /// <summary>
    /// Draws projected node places on helper line in sceneview during in/out node setup.
    /// </summary>
    private void ShowNodePlacesOnLine()
    {
        for (int i = 0; i < nodesOnLine; i++)
        {
            Vector3 pos = linePoints[0] + nodePlaces[i] * lineLength * lineDir;
            if (lineNodesInOut[i] == NodeInOut.InNode)
            {
                DrawSceneDisc(pos, Color.blue, false);
            }
            else
            {
                DrawSceneDisc(pos, Color.red, false);
            }
        }
    }
    /// <summary>
    /// Draws saved helper line points.
    /// </summary>
    private void ShowSavedHelperPoints()
    {
        for (int i = 0; i < pointsToDraw.Count; i++)
        {
            DrawSceneDisc(pointsToDraw[i], pointsToDrawColors[i], false);
        }
    }
    /// <summary>
    /// Draws a disc in sceneview to highlight active selected in- and out-nodes.
    /// </summary>
    private void HighlightNodes()
    {
        if (!intersection.allNodesSet)
        {
            Nodes selected = null;
            if (intersection.GetInfoIndex >= 0)
            {
                selected = intersection.GetSelectedNodeInfo(out NodeInOut inOut);
            }
            if (selected != null)
            {
                DrawSceneDisc(selected.gameObject, Color.yellow, true);
            }
        }
        else
        {
            if (intersection.inIndex > -1)
            {
                Vector3 pos = inPositions[intersection.inIndex];
                DrawSceneDisc(pos, Color.yellow, true);
            }

            if (intersection.outIndex > -1)
            {
                Vector3 pos = outPositions[intersection.outIndex];
                DrawSceneDisc(pos, Color.magenta, true);
            }
        }
        int index = intersection.GetInfoIndex;
        if (intersection.nodesInBox != null)
        {
            for (int i = 0; i < intersection.nodesInBox.Length; i++)
            {
                NodeInfo ni = intersection.nodesInBox[i];
                if (ni.inOut == NodeInOut.InNode)
                {
                    DrawSceneDisc(ni.node.gameObject, Color.blue, false);
                }
                else if (ni.inOut == NodeInOut.OutNode)
                {
                    DrawSceneDisc(ni.node.gameObject, Color.red, false);
                }
            }
        }
    }
    /// <summary>
    /// Rotates helper line.
    /// </summary>
    /// <param name="clockwise">Is rotation direction clockwise?</param>
    private void RotateLine (bool clockwise)
    {
        float angle = lineAngle;
        if (!clockwise)
        {
            angle = -angle;
        }
        Vector3 dir0 = linePoints[0] - linePos;
        Vector3 dir1 = linePoints[1] - linePos;
        dir0 = Quaternion.Euler(new Vector3(0f, angle, 0f))*dir0;
        dir1 = Quaternion.Euler(new Vector3(0f, angle, 0f)) * dir1;
        linePoints[0] = dir0 + linePos;
        linePoints[1] = dir1 + linePos;
    }
    /// <summary>
    /// Updates helper line's position.
    /// </summary>
    private void UpdateLinePosition()
    {
        Vector3 p0 = linePos + new Vector3(0f, 0f, -lineLength / 2f);
        Vector3 p1 = linePos + new Vector3(0f, 0f, lineLength / 2f);
        //rotation
        Vector3 dir0 = p0 - linePos;
        Vector3 dir1 = p1 - linePos;
        dir0 = Quaternion.Euler(new Vector3(0f, lineYAngle, 0f)) * dir0;
        dir1 = Quaternion.Euler(new Vector3(0f, lineYAngle, 0f)) * dir1;
        p0 = dir0 + linePos;
        p1 = dir1 + linePos;

        linePoints = new Vector3[] { p0, p1 };
        bool needToAdjust = CheckLinePointsInBounds();
        lineDir = (linePoints[1] - linePoints[0]).normalized;
        if (needToAdjust)
        {
            lineLength = Vector3.Distance(linePoints[0], linePoints[1]);
            linePos = linePoints[0] + lineDir * lineLength * 0.5f;
        }
    }
    /// <summary>
    /// Checks if helper line is in bounds of framing area and adjusts its position accordingly if not.
    /// </summary>
    /// <returns></returns>
    private bool CheckLinePointsInBounds()
    {
        float minX = intersection.CenterPoint.x - intersection.FrameWidth / 2f;
        float maxX = intersection.CenterPoint.x + intersection.FrameWidth / 2f;
        float minZ = intersection.CenterPoint.z - intersection.FrameHeight / 2f;
        float maxZ = intersection.CenterPoint.z + intersection.FrameHeight / 2f;

        bool needToAdjust = false;

        if (linePoints[0].x < minX)
        {
            needToAdjust = true;
            linePoints[0].x = minX;
        }
        else if (linePoints[0].x > maxX)
        {
            needToAdjust = true;
            linePoints[0].x = maxX;
        }
        if (linePoints[1].x < minX)
        {
            needToAdjust = true;
            linePoints[1].x = minX;
        }
        else if (linePoints[1].x > maxX)
        {
            needToAdjust = true;
            linePoints[1].x = maxX;
        }
        if (linePoints[0].z < minZ)
        {
            needToAdjust = true;
            linePoints[0].z = minZ;
        }
        else if (linePoints[0].z > maxZ)
        {
            needToAdjust = true;
            linePoints[0].z = maxZ;
        }
        if (linePoints[1].z < minZ)
        {
            needToAdjust = true;
            linePoints[1].z = minZ;
        }
        else if (linePoints[1].z > maxZ)
        {
            needToAdjust = true;
            linePoints[1].z = maxZ;
        }

        return needToAdjust;
    }
    /// <summary>
    /// Updates framing area.
    /// </summary>
    private void UpdateFramingBox()
    {
        Vector3 corner1 = intersection.CenterPoint;
        corner1 += new Vector3(-intersection.FrameWidth * 0.5f, 0f, -intersection.FrameHeight * 0.5f);
        Vector3 corner2 = intersection.CenterPoint;
        corner2 += new Vector3(intersection.FrameWidth * 0.5f, 0f, -intersection.FrameHeight * 0.5f);
        Vector3 corner3 = intersection.CenterPoint;
        corner3 += new Vector3(intersection.FrameWidth * 0.5f, 0f, intersection.FrameHeight * 0.5f);
        Vector3 corner4 = intersection.CenterPoint;
        corner4 += new Vector3(-intersection.FrameWidth * 0.5f, 0f, intersection.FrameHeight * 0.5f);
        framingHorizontal = new Vector3[] { corner1, corner2, corner3, corner4 };
        framingVertical = new Vector3[] { corner4, corner1, corner3, corner2 };
    }
    /// <summary>
    /// Updates a list of nodes inside the framing area.
    /// </summary>
    private void UpdateNodesInBox()
    {
        if (!intersection.nodesInBoxSet)
        {
            float minX = intersection.CenterPoint.x - intersection.FrameWidth / 2f;
            float maxX = intersection.CenterPoint.x + intersection.FrameWidth / 2f;
            float minZ = intersection.CenterPoint.z - intersection.FrameHeight / 2f;
            float maxZ = intersection.CenterPoint.z + intersection.FrameHeight / 2f;

            List<Nodes> nodes = new List<Nodes>();
            Nodes[] allNodes = GameObject.FindObjectsOfType<Nodes>();
            for (int i = 0; i < allNodes.Length; i++)
            {
                float nodeX = allNodes[i].gameObject.transform.position.x;
                float nodeZ = allNodes[i].gameObject.transform.position.z;
                if (nodeX > minX && nodeX < maxX && nodeZ > minZ && nodeZ < maxZ)
                {
                    nodes.Add(allNodes[i]);
                }
            }
            NodeInfo[] nInfo = new NodeInfo[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                NodeInfo ni = new NodeInfo();
                ni.node = nodes[i];
                ni.inOut = NodeInOut.NotUsed;
                nInfo[i] = ni;
            }
            intersection.nodesInBox = nInfo;

        }
    }
    //************************ ENABLE / DISABLE
    /// <summary>
    /// Changes and hides active tools in editor.
    /// </summary>
    private void DisableTools()
    {
        Tools.current = Tool.View;
        Tools.hidden = true;
    }
    /// <summary>
    /// Built-in function is called when object is selected.
    /// </summary>
    private void OnEnable()
    {
        intersection = target as IntersectionTool;
        UpdateFramingBox();

        if (intersection.framed)
        {
            Tools.current = Tool.View;
            Tools.hidden = true;
            if (intersection.nodesInBox == null)
            {
                UpdateNodesInBox();
            }
        }
        UpdatePointsToDraw();
        GetExistingLaneLinesToDraw();
        if (intersection.allNodesSet)
        {
            inOutCount = intersection.GetInOutPositions(out inPositions, out outPositions);
        }
        if (intersection.existingLanesChecked == false)
        {
            intersection.existingLaneIndex = 0;
            SetCurrentExistingLaneValuesToInspector();
        }
        if (intersection.splinesSet)
        {
            intersection.GetSegmentNodePositions(out currentSplineNodes, out otherSplineNodes);
            nodesFetched = true;
        }

        SetCameraAngle();
    }
    /// <summary>
    /// Built-in function is called when object is de-selected.
    /// </summary>
    private void OnDisable()
    {
        Tools.hidden = false;
    }
    /// <summary>
    /// Sets sceneview camera to bird view position at set height and ortographic mode.
    /// </summary>
    private void SetCameraAngle()
    {
        var sceneView = SceneView.lastActiveSceneView;
        sceneView.AlignViewToObject(intersection.transform);
        sceneView.LookAtDirect(intersection.transform.position, Quaternion.Euler(90, 0, 0), 30f);
        sceneView.orthographic = true;
    }
}
