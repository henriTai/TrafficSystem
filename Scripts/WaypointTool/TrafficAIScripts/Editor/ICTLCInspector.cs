using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

// Author: Henri Tainio

/// <summary>
/// Custom editor script for objects of type ICTrafficLightController.
/// </summary>
[CustomEditor(typeof(ICTrafficLightController))]
public class ICTLCInspector : Editor
{
    /// <summary>
    /// Reference to the target object.
    /// </summary>
    private ICTrafficLightController ictlc;

    //Tests
    /// <summary>
    /// Is there a Road-component attached to the target's gameobject.
    /// </summary>
    private bool attachedToRoad = true;
    /// <summary>
    /// Are intersections passage set up done?
    /// </summary>
    private bool passagesSet = true;
    /// <summary>
    /// Is intersection controller component missing?
    /// </summary>
    private bool intersectionControllerMissing = false;
    /// <summary>
    /// Is there too many passages set?
    /// </summary>
    private bool tooManyPassages = false;
    /// <summary>
    /// Are all lanes assigned to passages?
    /// </summary>
    private bool lanesAssigned = false;
    /// <summary>
    /// Are crosswalk assignment options active?
    /// </summary>
    private bool assigningCrosswalks = false;
    /// <summary>
    /// Are crosswalk labels shown in sceneview?
    /// </summary>
    private bool showCrosswalkLabels = false;
    /// <summary>
    /// Are traffic light assignment options active?
    /// </summary>
    private bool assigningTrafficLights = false;
    /// <summary>
    /// List of lane positions for visualization.
    /// </summary>
    List<Vector3> lanePositions;
    /// <summary>
    /// List of assigned lane positions for visualization.
    /// </summary>
    List<Vector3> assignedLanePositions;
    /// <summary>
    /// Start node positions for visualization.
    /// </summary>
    List<Vector3> startNodePositions;
    /// <summary>
    /// Positions of traffic light's phase 1 lanes for visualization.
    /// </summary>
    List<Vector3> phase1Pos;
    /// <summary>
    /// Positions of traffic light's phase 2 lanes for visualization.
    /// </summary>
    List<Vector3> phase2Pos;
    /// <summary>
    /// Positions of traffic light's phase 3 lanes for visualization.
    /// </summary>
    List<Vector3> phase3Pos;
    /// <summary>
    /// Positions of traffic light's phase 4 lanes for visualization.
    /// </summary>
    List<Vector3> phase4Pos;
    /// <summary>
    /// Index of currently selected phase.
    /// </summary>
    int phaseIndex;
    /// <summary>
    /// Positions of passage 1 lanes for visualization.
    /// </summary>
    List<Vector3> passage1Pos;
    /// <summary>
    /// Positions of passage 2 lanes for visualization.
    /// </summary>
    List<Vector3> passage2Pos;
    /// <summary>
    /// Positions of passage 3 lanes for visualization.
    /// </summary>
    List<Vector3> passage3Pos;
    /// <summary>
    /// Positions of passage 4 lanes for visualization.
    /// </summary>
    List<Vector3> passage4Pos;
    /// <summary>
    /// Currently selected passage index.
    /// </summary>
    int passageIndex;
    /// <summary>
    /// Passage count.
    /// </summary>
    int passageCount;
    /// <summary>
    /// A list of crosswalks in intersection.
    /// </summary>
    List<Crosswalk> availableCrosswalks;
    /// <summary>
    /// A list of booleans of which crosswalks in availableCrosswalks-list are not assigned yet.
    /// </summary>
    List<bool> selectedCrosswalks;
    /// <summary>
    /// A list of phases of which crosswalks in availableCrosswalks-list are assigned.
    /// </summary>
    List<int> crosswalkPhases;
    /// <summary>
    /// Road component attached to the target object.
    /// </summary>
    Road road;
    /// <summary>
    /// Left turning lanes of passage 1.
    /// </summary>
    public Lane[] passage1Left;
    /// <summary>
    /// Right turning lanes of passage 1.
    /// </summary>
    public Lane[] passage1Right;
    /// <summary>
    /// Lanes through of passage 1.
    /// </summary>
    public Lane[] passage1Straight;
    /// <summary>
    /// Left turning lanes of passage 2.
    /// </summary>
    public Lane[] passage2Left;
    /// <summary>
    /// Right turning lanes of passage 2.
    /// </summary>
    public Lane[] passage2Right;
    /// <summary>
    /// Lanes through of passage 2.
    /// </summary>
    public Lane[] passage2Straight;
    /// <summary>
    /// Left turning lanes of passage 3.
    /// </summary>
    public Lane[] passage3Left;
    /// <summary>
    /// Right turning lanes of passage 3.
    /// </summary>
    public Lane[] passage3Right;
    /// <summary>
    /// Lanes through of passage 3.
    /// </summary>
    public Lane[] passage3Straight;
    /// <summary>
    /// Left turning lanes of passage 4.
    /// </summary>
    public Lane[] passage4Left;
    /// <summary>
    /// Right turning lanes of passage 4.
    /// </summary>
    public Lane[] passage4Right;
    /// <summary>
    /// Lanes through of passage 4.
    /// </summary>
    public Lane[] passage4Straight;
    /// <summary>
    /// Are positions for visualization fetched?
    /// </summary>
    bool positionsFetched = false;
    /// <summary>
    /// Currently selected phase.
    /// </summary>
    int selectedPhase = -1;
    /// <summary>
    /// Serialized property field of target objects crosswalks.
    /// </summary>
    SerializedProperty tlcCrosswalks;
    
    /// <summary>
    /// Overrides target object's inspectorview.
    /// </summary>
    public override void OnInspectorGUI()
    {
        if (attachedToRoad == false)
        {
            EditorGUILayout.LabelField("This component should be attached to an object with Road-component.",
                EditorStyles.wordWrappedLabel);
            return;
        }
        if (passagesSet == false)
        {
            EditorGUILayout.LabelField("Please set passages to Road component.",
                EditorStyles.wordWrappedLabel);
            return;
        }
        if (intersectionControllerMissing == true)
        {
            EditorGUILayout.LabelField("There is more than one passages in Road-component but no IntersectionController " +
             "attached to gameobject.",
                EditorStyles.wordWrappedLabel);
            return;
        }
        if (tooManyPassages == true)
        {
            EditorGUILayout.LabelField("Please check passages in Road-component: For CrosswalkStop (straight road)," +
                " there shouls be 1 passage containing all lanes divided to in and out going directions (i.e. direction A and B). " +
                "For T-intersection there should be 3 passages and for 4-way intersection 4.",
                EditorStyles.wordWrappedLabel);
            return;
        }
        serializedObject.Update();
        EditorGUILayout.Separator();
        TrafficLightSettingsMenu();
        DrawEditorLine();
        PassageInfoMenu();
        DrawEditorLine();
        CrosswalkAssignMenu();
        DrawEditorLine();
        PhaseLengthSettingsMenu();
        DrawEditorLine();
        TrafficLightAssignmentMenu();
        DrawEditorLine();
        base.OnInspectorGUI();
    }
    /// <summary>
    /// Contents of inspectors traffic light settings menu.
    /// </summary>
    private void TrafficLightSettingsMenu()
    {
        ictlc.activity = (ICTrafficLightController.ActivityMode)EditorGUILayout.EnumPopup("Activity", ictlc.activity);
        EditorGUILayout.LabelField("Traffic light situation", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("" + ictlc.tlcType);
        string defaultText = "Default phase count: ";
        if (ictlc.tlcType == ICTrafficLightController.TLCType.NOT_SET)
        {
            defaultText += "-";
        }
        else
        {
            defaultText += DefaultPhaseCount(ictlc.tlcType);
        }
        ItalicLabel(defaultText);
        if (ictlc.tlcType == ICTrafficLightController.TLCType.CROSSWALK_STOP ||
            ictlc.tlcSettings.phaseCountHidden)
        {
            EditorGUILayout.LabelField("Number of phases: " + ictlc.tlcSettings.phases, EditorStyles.boldLabel);

            if (ictlc.tlcSettings.phaseCountHidden && ictlc.tlcType != ICTrafficLightController.TLCType.CROSSWALK_STOP)
            {
                if (GUILayout.Button("Edit Phase count"))
                {
                    ictlc.tlcSettings.phaseCountHidden = false;
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("Phases", EditorStyles.label);
            int phases = ictlc.tlcSettings.phases;
            phases = EditorGUILayout.IntSlider(phases, 2, 4);
            if (phases != ictlc.tlcSettings.phases)
            {
                ictlc.tlcSettings.phases = phases;
                ResetPhasesFrom(phases);
            }
            if (GUILayout.Button("Hide Phases"))
            {
                ictlc.tlcSettings.phaseCountHidden = true;
            }
        }
    }
    /// <summary>
    /// Contents of inspectors passage settings menu.
    /// </summary>
    private void PassageInfoMenu()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Passages", EditorStyles.boldLabel);
        int passageCount = DirectionCount(ictlc.tlcType);

        if (ictlc.showLanes == false)
        {
            EditorGUILayout.LabelField("Passage count: " + passageCount);
            if (lanesAssigned == false)
            {
                ItalicLabel("There are some unassigned lanes.");
            }
            else
            {
                ItalicLabel("All lanes are assigned");
            }
            if (GUILayout.Button("Set lanes to phases"))
            {
                ResetLanesShowing();
                ictlc.showLanes = true;
                ictlc.GetComponent<Road>().showLanes = false;
                SceneView.RepaintAll();
            }
            return;
        }
        if (GUILayout.Button("Default assign pattern (dir -> phase)"))
        {
            DefaultAssignPhases();
            CheckLanesAssigned();
            FetchLanePositions();
            FetchPhasePositions();
            FetchPassagePositions();
            UpdateICLanePhases();
            SceneView.RepaintAll();
            ictlc.showLanes = false;
        }
        if (GUILayout.Button("Close"))
        {
            ictlc.showLanes = false;
            SceneView.RepaintAll();
        }
        ItalicLabel("Please note!!!!");
        EditorGUILayout.LabelField("ATM TrafficLightController doesn't support advanced lightpatterns" +
            "with separated left / right direction lights. Default assign pattern is recommended.", EditorStyles.wordWrappedLabel);

        for (int i = 0; i < passageCount; i++)
        {
            EditorGUILayout.Separator();
            ItalicLabel("PASSAGE " + (i + 1));
            ShowLaneInfos(i);
        }
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Selected phase", EditorStyles.boldLabel);
        for (int i = 0; i < ictlc.tlcSettings.phases; i++)
        {
            bool v = false;
            if (selectedPhase == i)
            {
                v = true;
            }
            v = EditorGUILayout.ToggleLeft("Phase " + (i + 1), v);
            if (v == true)
            {
                selectedPhase = i;
            }
        }
        if (GUILayout.Button("Assign selected lanes to selected phase"))
        {
            if (selectedPhase != -1)
            {
                bool changed = false;
                for (int i = 0; i < ictlc.lanesShowing.Length; i++)
                {
                    if (ictlc.lanesShowing[i] == true && ictlc.tlcSettings.phaseOfLaneGroup[i] == 0)
                    {
                        changed = true;
                        ictlc.lanesShowing[i] = false;
                        ictlc.tlcSettings.phaseOfLaneGroup[i] = selectedPhase + 1;
                    }
                }
                if (changed)
                {
                    CheckLanesAssigned();
                    FetchLanePositions();
                    FetchPhasePositions();
                    FetchPassagePositions();
                    UpdateICLanePhases();
                    SceneView.RepaintAll();
                }
                selectedPhase = -1;
            }
        }
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();
        if (GUILayout.Button("Close"))
        {
            ictlc.showLanes = false;
            SceneView.RepaintAll();
        }
    }
    /// <summary>
    /// Shows lane information of selected passage in inspector.
    /// </summary>
    /// <param name="passageIndex">Selected passage</param>
    private void ShowLaneInfos(int passageIndex)
    {
        switch (passageIndex)
        {
            case (0):
                ListLanes(ref passage1Straight, IntersectionDirection.Straight);
                ListLanes(ref passage1Left, IntersectionDirection.Left);
                ListLanes(ref passage1Right, IntersectionDirection.Right);

                ListLaneToggles(ref passage1Straight, IntersectionDirection.Straight, 0);
                ListLaneToggles(ref passage1Left, IntersectionDirection.Left, 1);
                ListLaneToggles(ref passage1Right, IntersectionDirection.Right, 2);
                break;
            case (1):
                ListLanes(ref passage2Straight, IntersectionDirection.Straight);
                ListLanes(ref passage2Left, IntersectionDirection.Left);
                ListLanes(ref passage2Right, IntersectionDirection.Right);

                ListLaneToggles(ref passage2Straight, IntersectionDirection.Straight, 3);
                ListLaneToggles(ref passage2Left, IntersectionDirection.Left, 4);
                ListLaneToggles(ref passage2Right, IntersectionDirection.Right, 5);
                break;
            case (2):
                ListLanes(ref passage3Straight, IntersectionDirection.Straight);
                ListLanes(ref passage3Left, IntersectionDirection.Left);
                ListLanes(ref passage3Right, IntersectionDirection.Right);

                ListLaneToggles(ref passage3Straight, IntersectionDirection.Straight, 6);
                ListLaneToggles(ref passage3Left, IntersectionDirection.Left, 7);
                ListLaneToggles(ref passage3Right, IntersectionDirection.Right, 8);
                break;
            case (3):
                ListLanes(ref passage4Straight, IntersectionDirection.Straight);
                ListLanes(ref passage4Left, IntersectionDirection.Left);
                ListLanes(ref passage4Right, IntersectionDirection.Right);

                ListLaneToggles(ref passage4Straight, IntersectionDirection.Straight, 9);
                ListLaneToggles(ref passage4Left, IntersectionDirection.Left, 10);
                ListLaneToggles(ref passage4Right, IntersectionDirection.Right, 11);
                break;
        }
    }
    /// <summary>
    /// Shows lane iformation of selected lane yield group going to selected general direction.
    /// </summary>
    /// <param name="laneGroup">Selected lane yield group.</param>
    /// <param name="direction">General direction.</param>
    private void ListLanes(ref Lane[] laneGroup, IntersectionDirection direction)
    {
        if (laneGroup != null && laneGroup.Length > 0)
        {
            EditorGUILayout.LabelField("Lanes going " + direction + ":");
            for (int i = 0; i < laneGroup.Length; i++)
            {
                EditorGUILayout.LabelField("\t* " + laneGroup[i].name);
            }
        }
    }
    /// <summary>
    /// Shows visualized lanes selection options.
    /// </summary>
    /// <param name="laneGroup">An array of lanes.</param>
    /// <param name="direction">General direction.</param>
    /// <param name="laneGroupIndex">Lane group's index.</param>
    private void ListLaneToggles(ref Lane[] laneGroup, IntersectionDirection direction, int laneGroupIndex)
    {
        if (laneGroup != null && laneGroup.Length > 0)
        {
            int phase = ictlc.tlcSettings.phaseOfLaneGroup[laneGroupIndex];
            if (phase == 0)
            {
                string toggleText = "SHOW LANES ";
                switch (direction)
                {
                    case IntersectionDirection.Straight:
                        toggleText += "STRAIGHT";
                        break;
                    case IntersectionDirection.Left:
                        toggleText += "LEFT";
                        break;
                    case IntersectionDirection.Right:
                        toggleText += "RIGHT";
                        break;
                }

                bool v = ictlc.lanesShowing[laneGroupIndex];
                v = EditorGUILayout.ToggleLeft(toggleText, v);
                if (v != ictlc.lanesShowing[laneGroupIndex])
                {
                    ictlc.lanesShowing[laneGroupIndex] = v;
                    FetchLanePositions();
                    FetchPhasePositions();
                    FetchPassagePositions();
                    SceneView.RepaintAll();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                string t = "(" + direction + " - assigned phase " + phase + ")";
                ItalicLabel(t);
                if (GUILayout.Button("Unassign"))
                {
                    ictlc.tlcSettings.phaseOfLaneGroup[laneGroupIndex] = 0;
                    FetchLanePositions();
                    FetchPhasePositions();
                    FetchPassagePositions();
                    CheckLanesAssigned();
                    UpdateICLanePhases();
                    SceneView.RepaintAll();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    /// <summary>
    /// Crosswal assignment menu content in inspector.
    /// </summary>
    private void CrosswalkAssignMenu()
    {
        if (ictlc.crosswalks == null)
        {
            ictlc.crosswalks = new TrafficLightCrosswalk[0];
        }
        if (assigningCrosswalks == false)
        {
            if (GUILayout.Button("Assign crosswalks"))
            {
                FetchAvailableCrosswalks();
                assigningCrosswalks = true;
                showCrosswalkLabels = true;
                SceneView.RepaintAll();
            }
        }
        else
        {
            if (GUILayout.Button("Hide crosswalk assign menu"))
            {
                assigningCrosswalks = false;
                showCrosswalkLabels = false;
                SceneView.RepaintAll();
            }
        }
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Crosswalks", EditorStyles.boldLabel);
        if (phaseIndex == -1)
        {
            ItalicLabel("Phases are not assigned");
        }
        else if (assigningCrosswalks)
        {
            EditorGUILayout.LabelField("Highlighted phase:");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-"))
            {
                if (phaseIndex == 0)
                {
                    phaseIndex = ictlc.tlcSettings.phases - 1;
                }
                else
                {
                    phaseIndex--;
                }
                SceneView.RepaintAll();
            }

            EditorGUILayout.LabelField("\t Phase " + (phaseIndex + 1));

            if (GUILayout.Button("+"))
            {
                if (phaseIndex == ictlc.tlcSettings.phases - 1)
                {
                    phaseIndex = 0;
                }
                else
                {
                    phaseIndex++;
                }
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
        }
        if (ictlc.crosswalks == null || ictlc.crosswalks.Length == 0)
        {
            ItalicLabel("No assigned crosswalks");
        }
        else
        {
            ItalicLabel("Assigned croswalks: " + ictlc.crosswalks.Length);
        }
        if (assigningCrosswalks == true)
        {
            if (GUILayout.Button("Update available crosswalks"))
            {
                availableCrosswalks = null;
                FetchAvailableCrosswalks();
                SceneView.RepaintAll();
            }
            if (availableCrosswalks.Count > 0)
            {
                EditorGUILayout.LabelField("Select crosswalks to assign:");
                for (int i = 0; i < availableCrosswalks.Count; i++)
                {
                    selectedCrosswalks[i] = EditorGUILayout.ToggleLeft(availableCrosswalks[i].name, selectedCrosswalks[i]);
                    if (selectedCrosswalks[i] == true)
                    {
                        EditorGUILayout.LabelField("Select walk phase:");
                        int current = crosswalkPhases[i];
                        int phasesCount = ictlc.tlcSettings.phases;
                        for (int j = 1; j <= phasesCount; j++)
                        {
                            bool selected = false;
                            if (current == j)
                            {
                                selected = true;
                            }
                            bool v = selected;
                            v = EditorGUILayout.ToggleLeft("Phase " + j, v);
                            if (v != selected)
                            {
                                if (v == true)
                                {
                                    crosswalkPhases[i] = j;
                                }
                            }
                        }
                    }
                }
            }
            if (GUILayout.Button("Assign selected crosswalks"))
            {
                AssignCrosswalksToTLC();
                assigningCrosswalks = false;
                SceneView.RepaintAll();
            }
        }
        if (ictlc.crosswalks.Length > 0)
        {
            EditorGUILayout.LabelField("Pedestrian trafficlights", EditorStyles.boldLabel);
            if (tlcCrosswalks == null)
            {
                tlcCrosswalks = serializedObject.FindProperty("crosswalks");
            }
            for (int i = 0; i < ictlc.crosswalks.Length; i++)
            {
                EditorGUILayout.PropertyField(tlcCrosswalks.GetArrayElementAtIndex(i), new GUIContent("Crosswalk " + (i + 1)), true);
            }
            serializedObject.ApplyModifiedProperties();

        }
    }
    /// <summary>
    /// Inspector menu for setting phase lengths.
    /// </summary>
    private void PhaseLengthSettingsMenu()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Phase lengths", EditorStyles.boldLabel);
        for (int i = 0; i < ictlc.tlcSettings.phases; i++)
        {
            float val = ictlc.tlcSettings.phaseLengths[i];
            val = EditorGUILayout.DelayedFloatField("Phase " + (i + 1) + " length:", val);
            if (val >= 0f && val != ictlc.tlcSettings.phaseLengths[i])
            {
                ictlc.tlcSettings.phaseLengths[i] = val;
                RecalculateCycleLength();
            }
        }
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("CYCLE LENGTH: " + ictlc.tlcSettings.cycleLegth);
    }
    /// <summary>
    /// Inspector menu for assigning traffic lights.
    /// </summary>
    private void TrafficLightAssignmentMenu()
    {
        if (assigningTrafficLights == false)
        {
            InitializeTrafficLightListsIfNecessary();
            if (GUILayout.Button("Assign trafficlights"))
            {
                assigningTrafficLights = true;
                SceneView.RepaintAll();
            }
        }
        else
        {
            if (GUILayout.Button("Hide trafficlight assignment menu"))
            {
                assigningTrafficLights = false;
                SceneView.RepaintAll();
            }
        }
        EditorGUILayout.LabelField("Traffic lights", EditorStyles.boldLabel);
        if (assigningTrafficLights)
        {
            ItalicLabel("Select passage");
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("-"))
            {
                if (passageIndex == 1)
                {
                    passageIndex = passageCount;
                }
                else
                {
                    passageIndex--;
                }
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("+"))
            {
                if (passageIndex == passageCount)
                {
                    passageIndex = 1;
                }
                else
                {
                    passageIndex++;
                }
                SceneView.RepaintAll();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Passage " + passageIndex, EditorStyles.boldLabel);
            string listName = "";
            switch (passageIndex)
            {
                case 1:
                    listName = "passage1Switches";
                    break;
                case 2:
                    listName = "passage2Switches";
                    break;
                case 3:
                    listName = "passage3Switches";
                    break;
                case 4:
                    listName = "passage4Switches";
                    break;
            }
            SerializedProperty switches = serializedObject.FindProperty(listName);
            EditorGUILayout.PropertyField(switches, new GUIContent("Trafficlights"), true);
            serializedObject.ApplyModifiedProperties();
        }
    }
    /// <summary>
    /// Handles assigning crosswalks to the target traffic light controller.
    /// </summary>
    private void AssignCrosswalksToTLC()
    {
        for (int i = 0; i < selectedCrosswalks.Count; i++)
        {
            // Check crosswalk list and remove if necessary
            int index = -1;
            if (selectedCrosswalks[i] == false)
            {
                for (int j = 0; j < ictlc.crosswalks.Length; j++)
                {
                    if (ictlc.crosswalks[j].crosswalk == availableCrosswalks[i])
                    {
                        index = j;
                    }
                }
                if (index > -1)
                {
                    ictlc.crosswalks[index] = null;
                    for (int j = index; j < ictlc.crosswalks.Length - 1; j++)
                    {
                        ictlc.crosswalks[j] = ictlc.crosswalks[j + 1];
                    }
                    Array.Resize(ref ictlc.crosswalks, ictlc.crosswalks.Length - 1);
                }
            }
            else
            {
                if (CrosswalkIsSelected(availableCrosswalks[i]) == false)
                {
                    Array.Resize(ref ictlc.crosswalks, ictlc.crosswalks.Length + 1);
                    TrafficLightCrosswalk t = new TrafficLightCrosswalk();
                    t.crosswalk = availableCrosswalks[i];
                    t.phase = crosswalkPhases[i];
                    ictlc.crosswalks[ictlc.crosswalks.Length - 1] = t;
                }
                else
                {
                    ChangeCrosswalkPhase(availableCrosswalks[i], crosswalkPhases[i]);
                }
            }
        }
    }
    /// <summary>
    /// Updates the list of crosswalks in this intersection.
    /// </summary>
    private void FetchAvailableCrosswalks()
    {
        if (ictlc.crosswalks == null)
        {
            ictlc.crosswalks = new TrafficLightCrosswalk[0];
        }
        if (availableCrosswalks == null)
        {
            availableCrosswalks = new List<Crosswalk>();
            selectedCrosswalks = new List<bool>();
            crosswalkPhases = new List<int>();
        }
        List<Lane> relevantLanes = new List<Lane>();
        Lane[] parentLanes = ictlc.GetComponentsInChildren<Lane>();
        for (int i = 0; i < parentLanes.Length; i++)
        {
            Lane l = parentLanes[i];
            if (l.crosswalkEncounters != null && l.crosswalkEncounters.Length > 0)
            {
                relevantLanes.Add(l);
            }
            Nodes n = l.nodesOnLane[0].InNode;
            Lane l2;
            if (n != null)
            {
                l2 = n.ParentLane;
                if (l2.crosswalkEncounters != null && l2.crosswalkEncounters.Length > 0)
                {
                    if (!relevantLanes.Contains(l2))
                    {
                        relevantLanes.Add(l2);
                    }
                }
            }
            n = l.nodesOnLane[l.nodesOnLane.Length - 1].OutNode;
            if (n != null)
            {
                l2 = n.ParentLane;
                if (l2.crosswalkEncounters != null && l2.crosswalkEncounters.Length > 0)
                {
                    if (!relevantLanes.Contains(l2))
                    {
                        relevantLanes.Add(l2);
                    }
                }
            }
        }
        for (int i = 0; i < relevantLanes.Count; i++)
        {
            Lane l = relevantLanes[i];
            // checks if crosswalkencounters is uninitialized or some items have been removed
            // from array
            CheckCrosswalkEncounters(l);
            for (int j = 0; j < l.crosswalkEncounters.Length; j++)
            {
                CrosswalkEncounter ce = l.crosswalkEncounters[j];
                if (!availableCrosswalks.Contains(l.crosswalkEncounters[j].crosswalk))
                {
                    Crosswalk c = l.crosswalkEncounters[j].crosswalk;
                    availableCrosswalks.Add(c);

                    if (CrosswalkIsSelected(c))
                    {
                        selectedCrosswalks.Add(true);
                        crosswalkPhases.Add(CrosswalkPhase(c));
                    }
                    else
                    {
                        selectedCrosswalks.Add(false);
                        crosswalkPhases.Add(0);
                    }
                }
            }
        }

    }
    /// <summary>
    /// Checks if given crosswalk already assigned to some phase.
    /// </summary>
    /// <param name="crosswalk">Checked crosswalk.</param>
    /// <returns>Is given crosswalk already assigned to some phase?</returns>
    private bool CrosswalkIsSelected(Crosswalk crosswalk)
    {
        bool val = false;
        if (ictlc.crosswalks == null)
        {
            ictlc.crosswalks = new TrafficLightCrosswalk[0];
            return false;
        }
        for (int i = 0; i < ictlc.crosswalks.Length; i++)
        {
            if (ictlc.crosswalks[i].crosswalk == crosswalk)
            {
                val = true;
                break;
            }
        }
        return val;
    }
    /// <summary>
    /// Assigns crosswalk to the given phase.
    /// </summary>
    /// <param name="crosswalk">Selected crosswalk.</param>
    /// <param name="newPhase">The phase that crosswalk is assigned to.</param>
    private void ChangeCrosswalkPhase(Crosswalk crosswalk, int newPhase)
    {
        for (int i = 0; i < ictlc.crosswalks.Length; i++)
        {
            if (ictlc.crosswalks[i].crosswalk == crosswalk)
            {
                ictlc.crosswalks[i].phase = newPhase;
                break;
            }
        }
    }
    /// <summary>
    /// Returns the phase index of the given crosswalk.
    /// </summary>
    /// <param name="crosswalk">A crosswalk.</param>
    /// <returns>Which phase crosswalk is assigned to?</returns>
    private int CrosswalkPhase(Crosswalk crosswalk)
    {
        int val = 0;
        for (int i = 0; i < ictlc.crosswalks.Length; i++)
        {
            if (ictlc.crosswalks[i].crosswalk == crosswalk)
            {
                val = ictlc.crosswalks[i].phase;
                break;
            }
        }
        return val;
    }
    /// <summary>
    /// Checks crosswalk encounters along a given lane.
    /// </summary>
    /// <param name="l">A lane.</param>
    private void CheckCrosswalkEncounters(Lane l)
    {
        List<CrosswalkEncounter> encounters = new List<CrosswalkEncounter>();
        for (int i = 0; i < l.crosswalkEncounters.Length; i++)
        {
            if (l.crosswalkEncounters[i] != null)
            {
                if (l.crosswalkEncounters[i].crosswalk != null)
                {
                    encounters.Add(l.crosswalkEncounters[i]);
                }
            }
        }
        List<CrosswalkEncounter> arrangedEncounters = new List<CrosswalkEncounter>();
        bool reversed = false;
        for (int i = 0; i < l.nodesOnLane.Length; i++)
        {
            List<CrosswalkEncounter> onThisNode = new List<CrosswalkEncounter>();
            for (int j = 0; j < encounters.Count; j++)
            {
                if (encounters[j].crosswalk.nodeIndex == i)
                {
                    onThisNode.Add(encounters[j]);
                }
            }
            for (int j = 0; j < onThisNode.Count; j++)
            {
                arrangedEncounters.Add(onThisNode[j]);
            }
            // this check is for reversed direction lanes
            if (arrangedEncounters.Count > 1)
            {
                int lastIndex = arrangedEncounters.Count - 1;
                Vector2 p0 = new Vector2(l.nodesOnLane[0].transform.position.x, l.nodesOnLane[0].transform.position.z);
                Vector2 p1 = arrangedEncounters[0].crosswalk.crossingPoints[arrangedEncounters[0].index];
                Vector2 p2 = arrangedEncounters[lastIndex].crosswalk.crossingPoints[arrangedEncounters[lastIndex].index];
                if (Vector2.Distance(p0, p2) < Vector2.Distance(p0, p1))
                {
                    reversed = true;
                }
            }
        }
        CrosswalkEncounter[] checkedEncounters = new CrosswalkEncounter[arrangedEncounters.Count];
        if (reversed == false)
        {
            for (int i = 0; i < arrangedEncounters.Count; i++)
            {
                checkedEncounters[i] = arrangedEncounters[i];
            }
        }
        else
        {
            for (int i = 0; i < arrangedEncounters.Count; i++)
            {
                checkedEncounters[i] = arrangedEncounters[arrangedEncounters.Count - 1 - i];
            }
        }
        l.crosswalkEncounters = checkedEncounters;
    }
    /// <summary>
    /// Assigns phases automatically, grouping them by using general directions.
    /// </summary>
    private void DefaultAssignPhases()
    {
        int[] phaseGroups = new int[12];
        switch (ictlc.tlcType)
        {
            case ICTrafficLightController.TLCType.CROSSWALK_STOP:
                if (passage1Straight != null && passage1Straight.Length > 0)
                {
                    phaseGroups[0] = 1;
                }
                if (passage1Left != null && passage1Left.Length > 0)
                {
                    phaseGroups[1] = 1;
                }
                if (passage1Right != null && passage1Right.Length > 0)
                {
                    phaseGroups[2] = 1;
                }
                if (passage2Straight != null && passage2Straight.Length > 0)
                {
                    phaseGroups[3] = 1;
                }
                if (passage2Left != null && passage2Left.Length > 0)
                {
                    phaseGroups[4] = 1;
                }
                if (passage2Right != null && passage2Right.Length > 0)
                {
                    phaseGroups[5] = 1;
                }
                break;
            case ICTrafficLightController.TLCType.T_INTERSECTION:
                if (passage1Straight != null && passage1Straight.Length > 0)
                {
                    phaseGroups[0] = 1;
                }
                if (passage1Left != null && passage1Left.Length > 0)
                {
                    phaseGroups[1] = 1;
                }
                if (passage1Right != null && passage1Right.Length > 0)
                {
                    phaseGroups[2] = 1;
                }
                if (passage2Straight != null && passage2Straight.Length > 0)
                {
                    phaseGroups[3] = 2;
                }
                if (passage2Left != null && passage2Left.Length > 0)
                {
                    phaseGroups[4] = 2;
                }
                if (passage2Right != null && passage2Right.Length > 0)
                {
                    phaseGroups[5] = 2;
                }
                if (passage3Straight != null && passage3Straight.Length > 0)
                {
                    phaseGroups[6] = 3;
                }
                if (passage3Left != null && passage3Left.Length > 0)
                {
                    phaseGroups[7] = 3;
                }
                if (passage3Right != null && passage3Right.Length > 0)
                {
                    phaseGroups[8] = 3;
                }
                break;
            case ICTrafficLightController.TLCType.FOUR_WAY_INTERSECTION:
                if (passage1Straight != null && passage1Straight.Length > 0)
                {
                    phaseGroups[0] = 1;
                }
                if (passage1Left != null && passage1Left.Length > 0)
                {
                    phaseGroups[1] = 1;
                }
                if (passage1Right != null && passage1Right.Length > 0)
                {
                    phaseGroups[2] = 1;
                }
                if (passage2Straight != null && passage2Straight.Length > 0)
                {
                    phaseGroups[3] = 2;
                }
                if (passage2Left != null && passage2Left.Length > 0)
                {
                    phaseGroups[4] = 2;
                }
                if (passage2Right != null && passage2Right.Length > 0)
                {
                    phaseGroups[5] = 2;
                }
                if (passage3Straight != null && passage3Straight.Length > 0)
                {
                    phaseGroups[6] = 3;
                }
                if (passage3Left != null && passage3Left.Length > 0)
                {
                    phaseGroups[7] = 3;
                }
                if (passage3Right != null && passage3Right.Length > 0)
                {
                    phaseGroups[8] = 3;
                }
                if (passage4Straight != null && passage4Straight.Length > 0)
                {
                    phaseGroups[9] = 4;
                }
                if (passage4Left != null && passage4Left.Length > 0)
                {
                    phaseGroups[10] = 4;
                }
                if (passage4Right != null && passage4Right.Length > 0)
                {
                    phaseGroups[11] = 4;
                }
                break;
        }
        ictlc.tlcSettings.phaseOfLaneGroup = phaseGroups;
    }
    /// <summary>
    /// Updates controllers lane phases.
    /// </summary>
    private void UpdateICLanePhases()
    {
        List<Lane> ls1 = new List<Lane>();
        List<Lane> ls2 = new List<Lane>();
        List<Lane> ls3 = new List<Lane>();
        List<Lane> ls4 = new List<Lane>();
        for (int i = 0; i < ictlc.tlcSettings.phaseOfLaneGroup.Length; i++)
        {
            int phase = ictlc.tlcSettings.phaseOfLaneGroup[i];
            if (phase == 0)
            {
                continue;
            }
            else if (phase == 1)
            {
                FetchFromPhaseGroupsToList(ref ls1, i);
            }
            else if (phase == 2)
            {
                FetchFromPhaseGroupsToList(ref ls2, i);
            }
            else if (phase == 3)
            {
                FetchFromPhaseGroupsToList(ref ls3, i);
            }
            else if (phase == 4)
            {
                FetchFromPhaseGroupsToList(ref ls4, i);
            }
        }
        Undo.RecordObject(ictlc, "phases assigned to " + ictlc.name);
        UpdatePhaseLanes(ls1, ls2, ls3, ls4);
    }
    /// <summary>
    /// Updates phase lane groups.
    /// </summary>
    /// <param name="phase1">A list of phase 1 lanes.</param>
    /// <param name="phase2">A list of phase 2 lanes.</param>
    /// <param name="phase3">A list of phase 3 lanes.</param>
    /// <param name="phase4">A list of phase 4 lanes.</param>
    public void UpdatePhaseLanes(List<Lane> phase1, List<Lane> phase2, List<Lane> phase3, List<Lane> phase4)
    {
        List<Nodes> startNodes = new List<Nodes>();
        IntersectionPhaseGroups pg = new IntersectionPhaseGroups();
        for (int i = 0; i < phase1.Count; i++)
        {
            Nodes start = phase1[i].nodesOnLane[0];
            if (!startNodes.Contains(start))
            {
                startNodes.Add(start);
            }
        }
        pg.phase1Lanes = new CarsOnLane[startNodes.Count];
        for (int i = 0; i < startNodes.Count; i++)
        {
            pg.phase1Lanes[i] = new CarsOnLane();
            pg.phase1Lanes[i].carsOnLane = new List<CarInIntersection>();
            pg.phase1Lanes[i].startNode = startNodes[i];
        }
        startNodes.Clear();
        for (int i = 0; i < phase2.Count; i++)
        {
            Nodes start = phase2[i].nodesOnLane[0];
            if (!startNodes.Contains(start))
            {
                startNodes.Add(start);
            }
        }
        pg.phase2Lanes = new CarsOnLane[startNodes.Count];
        for (int i = 0; i < startNodes.Count; i++)
        {
            pg.phase2Lanes[i] = new CarsOnLane();
            pg.phase2Lanes[i].carsOnLane = new List<CarInIntersection>();
            pg.phase2Lanes[i].startNode = startNodes[i];
        }
        startNodes.Clear();
        for (int i = 0; i < phase3.Count; i++)
        {
            Nodes start = phase3[i].nodesOnLane[0];
            if (!startNodes.Contains(start))
            {
                startNodes.Add(start);
            }
        }
        pg.phase3Lanes = new CarsOnLane[startNodes.Count];
        for (int i = 0; i < startNodes.Count; i++)
        {
            pg.phase3Lanes[i] = new CarsOnLane();
            pg.phase3Lanes[i].carsOnLane = new List<CarInIntersection>();
            pg.phase3Lanes[i].startNode = startNodes[i];
        }
        startNodes.Clear();
        for (int i = 0; i < phase4.Count; i++)
        {
            Nodes start = phase4[i].nodesOnLane[0];
            if (!startNodes.Contains(start))
            {
                startNodes.Add(start);
            }
        }
        pg.phase4Lanes = new CarsOnLane[startNodes.Count];
        for (int i = 0; i < startNodes.Count; i++)
        {
            pg.phase4Lanes[i] = new CarsOnLane();
            pg.phase4Lanes[i].carsOnLane = new List<CarInIntersection>();
            pg.phase4Lanes[i].startNode = startNodes[i];
        }
        ictlc.phaseGroups = pg;
        
    }
    /// <summary>
    /// Fetches lanes belonging to a certain phase group.
    /// </summary>
    /// <param name="listToAdd">A list where the lanes are added.</param>
    /// <param name="groupIndex">Phase groups index.</param>
    private void FetchFromPhaseGroupsToList(ref List<Lane> listToAdd, int groupIndex)
    {
        if (groupIndex == 0)
        {
            for (int i = 0; i < passage1Straight.Length; i++)
            {
                listToAdd.Add(passage1Straight[i]);
            }
        }
        else if (groupIndex == 1)
        {
            for (int i = 0; i < passage1Left.Length; i++)
            {
                listToAdd.Add(passage1Left[i]);
            }
        }
        else if (groupIndex == 2)
        {
            for (int i = 0; i < passage1Right.Length; i++)
            {
                listToAdd.Add(passage1Right[i]);
            }
        }
        else if (groupIndex == 3)
        {
            for (int i = 0; i < passage2Straight.Length; i++)
            {
                listToAdd.Add(passage2Straight[i]);
            }
        }
        else if (groupIndex == 4)
        {
            for (int i = 0; i < passage2Left.Length; i++)
            {
                listToAdd.Add(passage2Left[i]);
            }
        }
        else if (groupIndex == 5)
        {
            for (int i = 0; i < passage2Right.Length; i++)
            {
                listToAdd.Add(passage2Right[i]);
            }
        }
        else if (groupIndex == 6)
        {
            for (int i = 0; i < passage3Straight.Length; i++)
            {
                listToAdd.Add(passage3Straight[i]);
            }
        }
        else if (groupIndex == 7)
        {
            for (int i = 0; i < passage3Left.Length; i++)
            {
                listToAdd.Add(passage3Left[i]);
            }
        }
        else if (groupIndex == 8)
        {
            for (int i = 0; i < passage3Right.Length; i++)
            {
                listToAdd.Add(passage3Right[i]);
            }
        }
        else if (groupIndex == 9)
        {
            for (int i = 0; i < passage4Straight.Length; i++)
            {
                listToAdd.Add(passage4Straight[i]);
            }
        }
        else if (groupIndex == 10)
        {
            for (int i = 0; i < passage4Left.Length; i++)
            {
                listToAdd.Add(passage4Left[i]);
            }
        }
        else if (groupIndex == 11)
        {
            for (int i = 0; i < passage4Right.Length; i++)
            {
                listToAdd.Add(passage4Right[i]);
            }
        }
    }
    /// <summary>
    /// Initialize traffic light arrays if necessary.
    /// </summary>
    private void InitializeTrafficLightListsIfNecessary()
    {
        if (ictlc.passage1Switches == null)
        {
            ictlc.passage1Switches = new TrafficLightSwitch[0];
        }
        if (ictlc.passage2Switches == null)
        {
            ictlc.passage2Switches = new TrafficLightSwitch[0];
        }
        if (ictlc.passage3Switches == null)
        {
            ictlc.passage3Switches = new TrafficLightSwitch[0];
        }
        if (ictlc.passage4Switches == null)
        {
            ictlc.passage4Switches = new TrafficLightSwitch[0];
        }
    }
    /// <summary>
    /// Resets visualized lanes.
    /// </summary>
    private void ResetLanesShowing()
    {
        ictlc.lanesShowing = new bool[] {false, false, false, false, false, false,
                                        false, false, false, false, false, false};
    }
    /// <summary>
    /// Resets phase lengths from given phase index.
    /// </summary>
    /// <param name="index">Phase's index.</param>
    private void ResetPhasesFrom(int index)
    {
        for (int i = index; i < 4; i++)
        {
            ictlc.tlcSettings.phaseLengths[i] = 0f;
        }
        RecalculateCycleLength();
    }
    /// <summary>
    /// Calculates the overall length of traffic light's phase cycle.
    /// </summary>
    private void RecalculateCycleLength()
    {
        float cycle = 0f;
        for (int i = 0; i < ictlc.tlcSettings.phases; i++)
        {
            cycle += ictlc.tlcSettings.phaseLengths[i];
        }
        ictlc.tlcSettings.cycleLegth = cycle;
    }
    /// <summary>
    /// Returns default phase count based on the traffic light type.
    /// </summary>
    /// <param name="type">Traffic light type, based on how many roads are connecting.</param>
    /// <returns>Default phase count.</returns>
    private int DefaultPhaseCount(ICTrafficLightController.TLCType type)
    {
        int v = 0;
        switch (type)
        {
            case ICTrafficLightController.TLCType.CROSSWALK_STOP:
                v = 2;
                break;
            case ICTrafficLightController.TLCType.T_INTERSECTION:
                v = 3;
                break;
            case ICTrafficLightController.TLCType.FOUR_WAY_INTERSECTION:
                v = 4;
                break;
        }
        return v;
    }
    /// <summary>
    /// Returns direction count based on the traffic light type.
    /// </summary>
    /// <param name="t">Traffic light type, based on how many roads are connecting.</param>
    /// <returns>Direction count.</returns>
    private int DirectionCount(ICTrafficLightController.TLCType t)
    {
        int val = 0;
        switch (t)
        {
            case ICTrafficLightController.TLCType.NOT_SET:
                val = 0;
                break;
            case ICTrafficLightController.TLCType.CROSSWALK_STOP:
                val = 2;
                break;
            case ICTrafficLightController.TLCType.T_INTERSECTION:
                val = 3;
                break;
            case ICTrafficLightController.TLCType.FOUR_WAY_INTERSECTION:
                val = 4;
                break;

        }
        return val;
    }
    /// <summary>
    /// Prints message in italic in inspector.
    /// </summary>
    /// <param name="message">Printed message.</param>
    private void ItalicLabel(string message)
    {
        GUIStyle gs = new GUIStyle(EditorStyles.label);
        gs.fontStyle = FontStyle.Italic;
        gs.wordWrap = true;
        EditorGUILayout.LabelField(message, gs);
    }
    /// <summary>
    /// Draws a 2 point thick divider line in inspector.
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
    /// Draws scene view visualization and gizmos.
    /// </summary>
    private void OnSceneGUI()
    {
        if (ictlc.showLanes)
        {
            if (positionsFetched == false)
            {
                FetchLanePositions();
                FetchPhasePositions();
                FetchPassagePositions();
                positionsFetched = true;
            }
            DrawLanePositions();
            DrawStartNodes();
        }
        DrawAvailableCrosswalkLabels();
        if (assigningCrosswalks)
        {
            DrawPhaseLanePositions();
            DrawStartNodes();
        }
        if (assigningTrafficLights)
        {
            DrawPassageLanePositions();
            DrawStartNodes();
        }
    }
    /// <summary>
    /// Fetches lane positions for visualization.
    /// </summary>
    private void FetchLanePositions()
    {
        if (road == null)
        {
            return;
        }
        lanePositions = new List<Vector3>();
        assignedLanePositions = new List<Vector3>();
        startNodePositions = new List<Vector3>();
        // fetch selected lanes' positions
        if (ictlc.lanesShowing[0] && ictlc.tlcSettings.phaseOfLaneGroup[0] == 0)
        {
            FetchPositions(passage1Straight, ref lanePositions);
        }
        if (ictlc.lanesShowing[1] && ictlc.tlcSettings.phaseOfLaneGroup[1] == 0)
        {
            FetchPositions(passage1Left, ref lanePositions);
        }
        if (ictlc.lanesShowing[2] && ictlc.tlcSettings.phaseOfLaneGroup[2] == 0)
        {
            FetchPositions(passage1Right, ref lanePositions);
        }
        if (ictlc.lanesShowing[3] && ictlc.tlcSettings.phaseOfLaneGroup[3] == 0)
        {
            FetchPositions(passage2Straight, ref lanePositions);
        }
        if (ictlc.lanesShowing[4] && ictlc.tlcSettings.phaseOfLaneGroup[4] == 0)
        {
            FetchPositions(passage2Left, ref lanePositions);
        }
        if (ictlc.lanesShowing[5] && ictlc.tlcSettings.phaseOfLaneGroup[5] == 0)
        {
            FetchPositions(passage2Right, ref lanePositions);
        }
        if (ictlc.lanesShowing[6] && ictlc.tlcSettings.phaseOfLaneGroup[6] == 0)
        {
            FetchPositions(passage3Straight, ref lanePositions);
        }
        if (ictlc.lanesShowing[7] && ictlc.tlcSettings.phaseOfLaneGroup[7] == 0)
        {
            FetchPositions(passage3Left, ref lanePositions);
        }
        if (ictlc.lanesShowing[8] && ictlc.tlcSettings.phaseOfLaneGroup[8] == 0)
        {
            FetchPositions(passage3Right, ref lanePositions);
        }
        if (ictlc.lanesShowing[9] && ictlc.tlcSettings.phaseOfLaneGroup[9] == 0)
        {
            FetchPositions(passage4Straight, ref lanePositions);
        }
        if (ictlc.lanesShowing[10] && ictlc.tlcSettings.phaseOfLaneGroup[10] == 0)
        {
            FetchPositions(passage4Left, ref lanePositions);
        }
        if (ictlc.lanesShowing[11] && ictlc.tlcSettings.phaseOfLaneGroup[11] == 0)
        {
            FetchPositions(passage4Right, ref lanePositions);
        }
        // fetch assigned lanes' positions
        if (ictlc.tlcSettings.phaseOfLaneGroup[0] != 0)
        {
            FetchPositions(passage1Straight, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[1] != 0)
        {
            FetchPositions(passage1Left, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[2] != 0)
        {
            FetchPositions(passage1Right, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[3] != 0)
        {
            FetchPositions(passage2Straight, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[4] != 0)
        {
            FetchPositions(passage2Left, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[5] != 0)
        {
            FetchPositions(passage2Right, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[6] != 0)
        {
            FetchPositions(passage3Straight, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[7] != 0)
        {
            FetchPositions(passage3Left, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[8] != 0)
        {
            FetchPositions(passage3Right, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[9] != 0)
        {
            FetchPositions(passage4Straight, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[10] != 0)
        {
            FetchPositions(passage4Left, ref assignedLanePositions);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[11] != 0)
        {
            FetchPositions(passage4Right, ref assignedLanePositions);
        }
    }
    /// <summary>
    /// Fetches phase positions for visualization.
    /// </summary>
    private void FetchPhasePositions()
    {
        phase1Pos = new List<Vector3>();
        phase2Pos = new List<Vector3>();
        phase3Pos = new List<Vector3>();
        phase4Pos = new List<Vector3>();
        phaseIndex = ictlc.tlcSettings.phases - 1;
        if (ictlc.tlcSettings.phaseOfLaneGroup[0] != 0)
        {
            FetchPositions(passage1Straight, ref phase1Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[1] != 0)
        {
            FetchPositions(passage1Left, ref phase1Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[2] != 0)
        {
            FetchPositions(passage1Right, ref phase1Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[3] != 0)
        {
            FetchPositions(passage2Straight, ref phase2Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[4] != 0)
        {
            FetchPositions(passage2Left, ref phase2Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[5] != 0)
        {
            FetchPositions(passage2Right, ref phase2Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[6] != 0)
        {
            FetchPositions(passage3Straight, ref phase3Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[7] != 0)
        {
            FetchPositions(passage3Left, ref phase3Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[8] != 0)
        {
            FetchPositions(passage3Right, ref phase3Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[9] != 0)
        {
            FetchPositions(passage4Straight, ref phase4Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[10] != 0)
        {
            FetchPositions(passage4Left, ref phase4Pos);
        }
        if (ictlc.tlcSettings.phaseOfLaneGroup[11] != 0)
        {
            FetchPositions(passage4Right, ref phase4Pos);
        }

    }
    /// <summary>
    /// Fetches passage positions for visualization.
    /// </summary>
    private void FetchPassagePositions()
    {
        passage1Pos = new List<Vector3>();
        passage2Pos = new List<Vector3>();
        passage3Pos = new List<Vector3>();
        passage4Pos = new List<Vector3>();
        if (passage1Straight != null && passage1Straight.Length > 0)
        {
            FetchPositions(passage1Straight, ref passage1Pos);
        }
        if (passage1Left != null && passage1Left.Length > 0)
        {
            FetchPositions(passage1Left, ref passage1Pos);
        }
        if (passage1Right != null && passage1Right.Length > 0)
        {
            FetchPositions(passage1Right, ref passage1Pos);
        }

        if (passage2Straight != null && passage2Straight.Length > 0)
        {
            FetchPositions(passage2Straight, ref passage2Pos);
        }
        if (passage2Left != null && passage2Left.Length > 0)
        {
            FetchPositions(passage2Left, ref passage2Pos);
        }
        if (passage2Right != null && passage2Right.Length > 0)
        {
            FetchPositions(passage2Right, ref passage2Pos);
        }

        if (passage3Straight != null && passage3Straight.Length > 0)
        {
            FetchPositions(passage3Straight, ref passage3Pos);
        }
        if (passage3Left != null && passage3Left.Length > 0)
        {
            FetchPositions(passage3Left, ref passage3Pos);
        }
        if (passage3Right != null && passage3Right.Length > 0)
        {
            FetchPositions(passage3Right, ref passage3Pos);
        }

        if (passage4Straight != null && passage4Straight.Length > 0)
        {
            FetchPositions(passage4Straight, ref passage4Pos);
        }
        if (passage1Left != null && passage1Left.Length > 0)
        {
            FetchPositions(passage4Left, ref passage4Pos);
        }
        if (passage4Right != null && passage4Right.Length > 0)
        {
            FetchPositions(passage4Right, ref passage4Pos);
        }

        passageIndex = 0;
        if (passage1Pos.Count > 0)
        {
            passageIndex = 1;
            return;
        }
        if (passage2Pos.Count > 0)
        {
            passageIndex = 2;
            return;
        }
        if (passage3Pos.Count > 0)
        {
            passageIndex = 3;
            return;
        }
        if (passage4Pos.Count > 0)
        {
            passageIndex = 4;
            return;
        }
    }
    /// <summary>
    /// Fetches positions from given lane group.
    /// </summary>
    /// <param name="laneGroup">A lane group that positions are fetched from.</param>
    /// <param name="positions">A list that fetched positions are added.</param>
    private void FetchPositions(Lane[] laneGroup, ref List<Vector3> positions)
    {
        for (int i = 0; i < laneGroup.Length; i++)
        {
            startNodePositions.Add(laneGroup[i].nodesOnLane[0].transform.position);
            for (int j = 0; j < laneGroup[i].nodesOnLane.Length - 1; j++)
            {
                positions.Add(laneGroup[i].nodesOnLane[j].transform.position);
                positions.Add(laneGroup[i].nodesOnLane[j + 1].transform.position);
            }
        }
    }
    /// <summary>
    /// Draws lane positions in sceneview.
    /// </summary>
    private void DrawLanePositions()
    {
        Handles.color = Color.gray;
        for (int i = 0; i < assignedLanePositions.Count; i += 2)
        {
            Handles.DrawLine(assignedLanePositions[i], assignedLanePositions[i + 1]);
        }
        Handles.color = Color.yellow;
        for (int i = 0; i < lanePositions.Count; i += 2)
        {
            Handles.DrawLine(lanePositions[i], lanePositions[i + 1]);
        }
    }
    /// <summary>
    /// Draws lane positions of selected phase in sceneview.
    /// </summary>
    private void DrawPhaseLanePositions()
    {
        switch (phaseIndex)
        {
            case -1:
                return;
            case 0:
                Handles.color = Color.gray;
                for (int i = 0; i < phase2Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase2Pos[i], phase2Pos[i + 1]);
                }
                for (int i = 0; i < phase3Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase3Pos[i], phase3Pos[i + 1]);
                }
                for (int i = 0; i < phase4Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase4Pos[i], phase4Pos[i + 1]);
                }
                Handles.color = Color.yellow;
                for (int i = 0; i < phase1Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase1Pos[i], phase1Pos[i + 1]);
                }
                break;
            case 1:
                Handles.color = Color.gray;
                for (int i = 0; i < phase1Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase1Pos[i], phase1Pos[i + 1]);
                }
                for (int i = 0; i < phase3Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase3Pos[i], phase3Pos[i + 1]);
                }
                for (int i = 0; i < phase4Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase4Pos[i], phase4Pos[i + 1]);
                }
                Handles.color = Color.yellow;
                for (int i = 0; i < phase2Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase2Pos[i], phase2Pos[i + 1]);
                }
                break;
            case 2:
                Handles.color = Color.gray;
                for (int i = 0; i < phase1Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase1Pos[i], phase1Pos[i + 1]);
                }
                for (int i = 0; i < phase2Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase2Pos[i], phase2Pos[i + 1]);
                }
                for (int i = 0; i < phase4Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase4Pos[i], phase4Pos[i + 1]);
                }
                Handles.color = Color.yellow;
                for (int i = 0; i < phase3Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase3Pos[i], phase3Pos[i + 1]);
                }
                break;
            case 3:
                Handles.color = Color.gray;
                for (int i = 0; i < phase1Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase1Pos[i], phase1Pos[i + 1]);
                }
                for (int i = 0; i < phase2Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase2Pos[i], phase2Pos[i + 1]);
                }
                for (int i = 0; i < phase3Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase3Pos[i], phase3Pos[i + 1]);
                }
                Handles.color = Color.yellow;
                for (int i = 0; i < phase4Pos.Count; i += 2)
                {
                    Handles.DrawLine(phase4Pos[i], phase4Pos[i + 1]);
                }
                break;


        }
    }
    /// <summary>
    /// Draws lane positions of selected passage in sceneview.
    /// </summary>
    private void DrawPassageLanePositions()
    {
        Handles.color = Color.yellow;
        switch (passageIndex)
        {
            case 0:
                break;
            case 1:
                for (int i = 0; i < passage1Pos.Count; i += 2)
                {
                    Handles.DrawLine(passage1Pos[i], passage1Pos[i + 1]);
                }
                break;
            case 2:
                for (int i = 0; i < passage2Pos.Count; i += 2)
                {
                    Handles.DrawLine(passage2Pos[i], passage2Pos[i + 1]);
                }
                break;
            case 3:
                for (int i = 0; i < passage3Pos.Count; i += 2)
                {
                    Handles.DrawLine(passage3Pos[i], passage3Pos[i + 1]);
                }
                break;
            case 4:
                for (int i = 0; i < passage4Pos.Count; i += 2)
                {
                    Handles.DrawLine(passage4Pos[i], passage4Pos[i + 1]);
                }
                break;
        }
    }
    /// <summary>
    /// Visualizes lanes' start nodes in sceneview.
    /// </summary>
    private void DrawStartNodes()
    {
        Handles.color = Color.white;
        for (int i = 0; i < startNodePositions.Count; i++)
        {
            Vector3 pos = startNodePositions[i];
            Handles.DrawSolidDisc(pos, new Vector3(0f, 1f, 0f), 0.003f * Vector3.Distance(
            pos, SceneView.lastActiveSceneView.camera.transform.position));
        }
    }
    /// <summary>
    /// Draws labels for available crosswalks in sceneview.
    /// </summary>
    private void DrawAvailableCrosswalkLabels()
    {
        if (showCrosswalkLabels == false)
        {
            return;
        }
        for (int i = 0; i < availableCrosswalks.Count; i++)
        {
            Crosswalk c = availableCrosswalks[i];
            Vector3 pos = new Vector3(c.transform.position.x, 0f, c.transform.position.z);
            Handles.Label(pos, c.name);
        }
    }
    /// <summary>
    /// Checks that target object has a Road-component and traffic light type is correctly set.
    /// </summary>
    private void CheckCorrectType()
    {
        Road r = ictlc.GetComponent<Road>();
        if (r == null)
        {
            return;
        }
        if (r.passages.Length == 1 || r.passages.Length == 2)
        {
            if (ictlc.tlcType != ICTrafficLightController.TLCType.CROSSWALK_STOP)
            {
                ictlc.Reset(ICTrafficLightController.TLCType.CROSSWALK_STOP);
            }
        }
        else if (r.passages.Length == 3)
        {
            if (ictlc.tlcType != ICTrafficLightController.TLCType.T_INTERSECTION)
            {
                ictlc.Reset(ICTrafficLightController.TLCType.T_INTERSECTION);
            }
        }
        else if (r.passages.Length == 4)
        {
            if (ictlc.tlcType != ICTrafficLightController.TLCType.FOUR_WAY_INTERSECTION)
            {
                ictlc.Reset(ICTrafficLightController.TLCType.FOUR_WAY_INTERSECTION);
            }
        }
        SetLaneGroups();
    }
    /// <summary>
    /// Sets lane groups for passages.
    /// </summary>
    private void SetLaneGroups()
    {
        ResetLaneGroups();
        switch (ictlc.tlcType)
        {
            case ICTrafficLightController.TLCType.CROSSWALK_STOP:
                Lane[] p1Lanes = ictlc.passages[0].inLanes;
                Lane[] p2Lanes = ictlc.passages[0].outLanes;
                passage1Left = LanesToDirection(ref ictlc.passages[0].inLanes, IntersectionDirection.Left);
                passage1Right = LanesToDirection(ref ictlc.passages[0].inLanes, IntersectionDirection.Right);
                passage1Straight = LanesToDirection(ref ictlc.passages[0].inLanes, IntersectionDirection.Straight);
                passage2Left = LanesToDirection(ref ictlc.passages[0].outLanes, IntersectionDirection.Left);
                passage2Right = LanesToDirection(ref ictlc.passages[0].outLanes, IntersectionDirection.Right);
                passage2Straight = LanesToDirection(ref ictlc.passages[0].outLanes, IntersectionDirection.Straight);
                break;
            case ICTrafficLightController.TLCType.T_INTERSECTION:
                passage1Left = LanesToDirection(ref ictlc.passages[0].inLanes, IntersectionDirection.Left);
                passage1Right = LanesToDirection(ref ictlc.passages[0].inLanes, IntersectionDirection.Right);
                passage1Straight = LanesToDirection(ref ictlc.passages[0].inLanes, IntersectionDirection.Straight);

                passage2Left = LanesToDirection(ref ictlc.passages[1].inLanes, IntersectionDirection.Left);
                passage2Right = LanesToDirection(ref ictlc.passages[1].inLanes, IntersectionDirection.Right);
                passage2Straight = LanesToDirection(ref ictlc.passages[1].inLanes, IntersectionDirection.Straight);

                passage3Left = LanesToDirection(ref ictlc.passages[2].inLanes, IntersectionDirection.Left);
                passage3Right = LanesToDirection(ref ictlc.passages[2].inLanes, IntersectionDirection.Right);
                passage3Straight = LanesToDirection(ref ictlc.passages[2].inLanes, IntersectionDirection.Straight);
                break;
            case ICTrafficLightController.TLCType.FOUR_WAY_INTERSECTION:
                passage1Left = LanesToDirection(ref ictlc.passages[0].inLanes, IntersectionDirection.Left);
                passage1Right = LanesToDirection(ref ictlc.passages[0].inLanes, IntersectionDirection.Right);
                passage1Straight = LanesToDirection(ref ictlc.passages[0].inLanes, IntersectionDirection.Straight);

                passage2Left = LanesToDirection(ref ictlc.passages[1].inLanes, IntersectionDirection.Left);
                passage2Right = LanesToDirection(ref ictlc.passages[1].inLanes, IntersectionDirection.Right);
                passage2Straight = LanesToDirection(ref ictlc.passages[1].inLanes, IntersectionDirection.Straight);

                passage3Left = LanesToDirection(ref ictlc.passages[2].inLanes, IntersectionDirection.Left);
                passage3Right = LanesToDirection(ref ictlc.passages[2].inLanes, IntersectionDirection.Right);
                passage3Straight = LanesToDirection(ref ictlc.passages[2].inLanes, IntersectionDirection.Straight);

                passage4Left = LanesToDirection(ref ictlc.passages[3].inLanes, IntersectionDirection.Left);
                passage4Right = LanesToDirection(ref ictlc.passages[3].inLanes, IntersectionDirection.Right);
                passage4Straight = LanesToDirection(ref ictlc.passages[3].inLanes, IntersectionDirection.Straight);
                break;
        }
    }
    /// <summary>
    /// Fetches lanes heading to a given general direction from an array of lanes.
    /// </summary>
    /// <param name="lanes">An array of lanes from which the lanes are fetched.</param>
    /// <param name="dir">General direction.</param>
    /// <returns>A subgroup of lanes, heading to given direction.</returns>
    private Lane[] LanesToDirection(ref Lane[] lanes, IntersectionDirection dir)
    {
        List<Lane> ls = new List<Lane>();
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i].TurnDirection == dir)
            {
                ls.Add(lanes[i]);
            }
        }
        if (ls.Count == 0)
        {
            return null;
        }
        else
        {
            Lane[] lns = new Lane[ls.Count];
            for (int i = 0; i < ls.Count; i++)
            {
                lns[i] = ls[i];
            }
            return lns;
        }
    }
    /// <summary>
    /// Clears passage lane groups.
    /// </summary>
    private void ResetLaneGroups()
    {
        passage1Left = null;
        passage1Right = null;
        passage1Straight = null;

        passage2Left = null;
        passage2Right = null;
        passage2Straight = null;

        passage3Left = null;
        passage3Right = null;
        passage3Straight = null;

        passage4Left = null;
        passage4Right = null;
        passage4Straight = null;
    }
    /// <summary>
    /// Checks if lanes are assigned.
    /// </summary>
    private void CheckLanesAssigned()
    {
        if (passage1Straight != null && passage1Straight.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[0] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage1Left != null && passage1Left.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[1] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage1Right != null && passage1Right.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[2] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage2Straight != null && passage2Straight.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[3] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage2Left != null && passage2Left.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[4] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage2Right != null && passage2Right.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[5] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage3Straight != null && passage3Straight.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[6] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage3Left != null && passage3Left.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[7] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage3Right != null && passage3Right.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[8] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage4Straight != null && passage4Straight.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[9] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage4Left != null && passage4Left.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[10] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        if (passage4Right != null && passage4Right.Length > 0)
        {
            if (ictlc.tlcSettings.phaseOfLaneGroup[11] == 0)
            {
                lanesAssigned = false;
                return;
            }
        }
        lanesAssigned = true;
    }
    /// <summary>
    /// This function executes when an object of target's type is activated.
    /// </summary>
    private void OnEnable()
    {
        ictlc = target as ICTrafficLightController;
        tlcCrosswalks = serializedObject.FindProperty("crosswalks");
        road = ictlc.GetComponent<Road>();
        if (road == null)
        {
            attachedToRoad = false;
            ictlc.tlcType = ICTrafficLightController.TLCType.NOT_SET;
        }
        else if (road.passages == null || road.passages.Length == 0)
        {
            passagesSet = false;
            ictlc.tlcType = ICTrafficLightController.TLCType.NOT_SET;
        }
        else if (road.passages.Length > 1)
        {
            if (ictlc.intersection == null)
            {
                Intersection intersection = road.GetComponent<Intersection>();
                if (intersection == null)
                {
                    intersectionControllerMissing = true;
                    ictlc.tlcType = ICTrafficLightController.TLCType.NOT_SET;
                }
                else
                {
                    ictlc.intersection = intersection;
                    ictlc.tlcType = ICTrafficLightController.TLCType.NOT_SET;
                }
            }
        }
        else if (road.passages.Length > 4)
        {
            tooManyPassages = true;
            ictlc.tlcType = ICTrafficLightController.TLCType.NOT_SET;
        }
        CheckCorrectType();
        CheckLanesAssigned();
        FetchLanePositions();
        FetchPhasePositions();
        FetchPassagePositions();
        switch (ictlc.tlcType)
        {
            case ICTrafficLightController.TLCType.NOT_SET:
                passageCount = 0;
                break;
            case ICTrafficLightController.TLCType.CROSSWALK_STOP:
                passageCount = 2;
                break;
            case ICTrafficLightController.TLCType.T_INTERSECTION:
                passageCount = 3;
                break;
            case ICTrafficLightController.TLCType.FOUR_WAY_INTERSECTION:
                passageCount = 4;
                break;
        }
    }
}
