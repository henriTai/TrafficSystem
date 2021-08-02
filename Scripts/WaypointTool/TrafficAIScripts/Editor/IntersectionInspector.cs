using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Author: Henri Tainio

/// <summary>
/// Custom editor script for objects of type Intersection.
/// </summary>
[CustomEditor(typeof(Intersection))]
public class IntersectionInspector : Editor
{
    /// <summary>
    /// Target object.
    /// </summary>
    Intersection intersection;
    /// <summary>
    /// Target object's Road component.
    /// </summary>
    Road road;
    /// <summary>
    /// Is lane information displayed in inspector?
    /// </summary>
    bool showLaneInfos;


    /// <summary>
    /// Overrides the content of the inspectorview.
    /// </summary>
    public override void OnInspectorGUI()
    {
        AddTLCMenu();
        if (GUILayout.Button("Update lane infos"))
        {
            UpdateLaneInfos();
        }
        //Undo.RecordObject(intersection, intersection.name + " has changed");
        if (showLaneInfos)
        {
            if (GUILayout.Button("Hide lane infos"))
            {
                showLaneInfos = false;
            }
            ShowLaneInfos();
        }
        else
        {
            if (GUILayout.Button("Show lane infos"))
            {
                showLaneInfos = true;
            }
        }
        Undo.RecordObject(intersection, intersection.name + " has changed");
        //base.OnInspectorGUI();
    }
    /// <summary>
    /// A menu for adding a traffic light controller in inspector.
    /// </summary>
    private void AddTLCMenu()
    {
        if (intersection.currentController == intersection.noLightsController)
        {
            GUILayout.Label("Current controller: No traffic lights");
        }
        else if (intersection.currentController == intersection.trafficLightController && intersection.currentController != null)
        {
            GUILayout.Label("Current controller: Traffic light controller");
        }
        DrawEditorLine();
        if (intersection.trafficLightController == null)
        {
            GUILayout.Label("No traffic light controller exists");
            if (road != null)
            {
                if (road.passages.Length < 2)
                {
                    ItalicLabel("No passages set in 'Road' component.");
                    ItalicLabel("Please set passages if you want to add");
                    ItalicLabel("a traffic light controller");
                }
                else
                {
                    if (GUILayout.Button("Add traffic light controller"))
                    {
                        AddTrafficLightController();
                    }
                }
            }
        }
        else
        {
            if (intersection.currentController == intersection.noLightsController)
            {
                if (GUILayout.Button("Switch to TRAFFIC LIGHT CONTROLLER"))
                {
                    intersection.currentController = intersection.trafficLightController;
                }
            }
            else if (intersection.currentController == intersection.trafficLightController)
            {
                if (GUILayout.Button("Switch to NO LIGHTS CONTROLLER"))
                {
                    intersection.currentController = intersection.noLightsController;
                }
            }
        }

        DrawEditorLine();
    }
    /// <summary>
    /// Adds a traffic light controller to the intersection.
    /// </summary>
    private void AddTrafficLightController()
    {
        ICTrafficLightController ictl = intersection.gameObject.AddComponent<ICTrafficLightController>();
        ictl.intersection = intersection;
        intersection.trafficLightController = ictl;
        Undo.RecordObject(intersection, intersection.name + " has changed");
        Undo.RecordObject(ictl, intersection.name + " has changed");

    }
    /// <summary>
    /// Displays detailed lane information in inspector.
    /// </summary>
    private void ShowLaneInfos()
    {
        if (intersection.intersectionLaneInfos == null)
        {
            //
            return;
        }
        for (int i = 0; i < intersection.intersectionLaneInfos.Length; i++)
        {
            EditorGUILayout.Separator();
            IntersectionLaneInfo lInfo = intersection.intersectionLaneInfos[i];
            if (lInfo == null)
            {
                continue;
            }
            EditorGUILayout.LabelField(lInfo.lane.gameObject.name + " (" + lInfo.lane.laneType + ")", EditorStyles.boldLabel);
            ItalicLabel("Lanes giving way (" + lInfo.lanesGivingWay.Length + " / " + lInfo.lane.CrossingLanes.Length + ")");
            for (int j = 0; j < lInfo.lanesGivingWay.Length; j++)
            {
                LaneCrossingPoint lcp = lInfo.lanesGivingWay[j];
                EditorGUILayout.LabelField(lcp.otherLane.gameObject.name);
                EditorGUILayout.LabelField("Point (" + lcp.crossingPoint.x + " (x), " + lcp.crossingPoint.y + " (z)");
            }
            EditorGUILayout.Separator();
            ItalicLabel("Lanes to give way (" + lInfo.lanesToGiveWay.Length + " / " + lInfo.lane.CrossingLanes.Length + ")");
            for (int j = 0; j < lInfo.lanesToGiveWay.Length; j++)
            {
                LaneCrossingPoint lcp = lInfo.lanesToGiveWay[j];
                EditorGUILayout.LabelField(lcp.otherLane.gameObject.name);
                EditorGUILayout.LabelField("Point (" + lcp.crossingPoint.x + " (x), " + lcp.crossingPoint.y + " (z)");
            }
            EditorGUILayout.Separator();
        }
    }
    /// <summary>
    /// Updates intersection's lane information regarding crossing lanes and yield order.
    /// </summary>
    public void UpdateLaneInfos()
    {
        Lane[] allLanes = intersection.GetComponentsInChildren<Lane>();
        intersection.intersectionLaneInfos = new IntersectionLaneInfo[allLanes.Length];
        intersection.laneDictionary = new LaneInfoDictionary();

        // this is used for settling order for crossing left turning crossing lanes
        List<Lane> setToYield = new List<Lane>();

        for (int i = 0; i < allLanes.Length; i++)
        {
            Lane l = allLanes[i];
            List<LaneCrossingPoint> givingWay = new List<LaneCrossingPoint>();
            List<LaneCrossingPoint> toGiveWay = new List<LaneCrossingPoint>();
            bool isYielding = false;
            if (l.laneType != LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
            {
                isYielding = true;
            }
            //DriverYield dy = l.LaneYield;
            IntersectionDirection turnDir = l.TurnDirection;
            if (l.CrossingLanes == null)
            {
                l.CrossingLanes = new LaneCrossingPoint[0];
            }
            for (int j = 0; j < l.CrossingLanes.Length; j++)
            {
                LaneCrossingPoint lcp = l.CrossingLanes[j];
                switch (isYielding)
                {
                    case false:
                        if (turnDir == IntersectionDirection.Straight)
                        {
                            if (lcp.otherLane.TurnDirection == IntersectionDirection.Straight
                                && lcp.otherLane.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
                            {
                                Debug.Log("Warning: Crossing lanes " + l.gameObject.name + " and " + lcp.otherLane.gameObject.name +
                                    " are both marked as 'RightOfWay'.");
                            }
                            else
                            {
                                givingWay.Add(lcp);
                            }
                        }
                        if (turnDir == IntersectionDirection.Right)
                        {
                            if (lcp.otherLane.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
                            {
                                if (lcp.otherLane.TurnDirection == IntersectionDirection.Left)
                                {
                                    givingWay.Add(lcp);
                                }
                                else if (lcp.otherLane.TurnDirection == IntersectionDirection.Straight)
                                {
                                    toGiveWay.Add(lcp);
                                }
                                else
                                {
                                    Debug.Log("Right turning lane " + l.gameObject.name + " crosses another RightOfWay-lane " +
                                        lcp.otherLane.gameObject.name);
                                }
                            }
                            else
                            {
                                givingWay.Add(lcp);
                            }
                        }
                        if (turnDir == IntersectionDirection.Left)
                        {
                            if (lcp.otherLane.laneType == LaneType.INTERSECTION_LANE_YIELDING)
                            {
                                givingWay.Add(lcp);
                            }
                            else
                            {
                                if (lcp.otherLane.TurnDirection == IntersectionDirection.Left)
                                {
                                    Debug.Log("Two left turning lanes intersect, forced yield order: " + l.name + ", " + lcp.otherLane.name);
                                    bool found = false;
                                    for (int k = 0; k < setToYield.Count; k++)
                                    {
                                        if (l == setToYield[k])
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (found)
                                    {
                                        toGiveWay.Add(lcp);
                                    }
                                    else
                                    {
                                        setToYield.Add(lcp.otherLane);
                                        givingWay.Add(lcp);
                                    }
                                }
                                else
                                {
                                    toGiveWay.Add(lcp);
                                }
                            }
                        }
                        break;
                    case true:
                        if (lcp.otherLane.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
                        {
                            toGiveWay.Add(lcp);
                        }
                        if (lcp.otherLane.laneType == LaneType.INTERSECTION_LANE_YIELDING)
                        {
                            if (turnDir == IntersectionDirection.Straight)
                            {
                                if (lcp.otherLane.TurnDirection == IntersectionDirection.Straight)
                                {
                                    Debug.Log("Warning: Crossing lanes " + l.gameObject.name + " and " + lcp.otherLane.gameObject.name +
                                    " are both marked as 'GiveWay'.");
                                }
                                else
                                {
                                    givingWay.Add(lcp);
                                }
                            }
                            else
                            {
                                if (lcp.otherLane.TurnDirection == IntersectionDirection.Straight)
                                {
                                    toGiveWay.Add(lcp);
                                }
                                else
                                {
                                    Debug.Log("Two yielding lanes, both turning intersect: " + l.gameObject.name + " (lane 1), " +
                                        lcp.otherLane.gameObject.name + " (lane 2)");
                                    bool found = false;
                                    for (int k = 0; k < setToYield.Count; k++)
                                    {
                                        if (l == setToYield[k])
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (found)
                                    {
                                        toGiveWay.Add(lcp);
                                    }
                                    else
                                    {
                                        setToYield.Add(lcp.otherLane);
                                        givingWay.Add(lcp);
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            IntersectionLaneInfo ili = new IntersectionLaneInfo();
            ili.lane = l;
            ili.lanesToGiveWay = new LaneCrossingPoint[toGiveWay.Count];
            for (int j = 0; j < toGiveWay.Count; j++)
            {
                ili.lanesToGiveWay[j] = toGiveWay[j];
            }
            ili.lanesGivingWay = new LaneCrossingPoint[givingWay.Count];
            for (int j = 0; j < givingWay.Count; j++)
            {
                ili.lanesGivingWay[j] = givingWay[j];
            }
            intersection.intersectionLaneInfos[i] = ili;
            intersection.laneDictionary.Add(l, ili);
        }
        InitializeCarsOnLanes();
    }
    /// <summary>
    /// Initializes intersection's cars on ane information.
    /// </summary>
    public void InitializeCarsOnLanes()
    {
        int rowThruAndRightCount = 0;
        int rowLeftCount = 0;
        int gvThruAndRightCount = 0;
        int gvLeftCount = 0;
        IntersectionLaneInfo[] intersectionLaneInfos = intersection.intersectionLaneInfos;

        for (int i = 0; i < intersectionLaneInfos.Length; i++)
        {
            Lane l = intersectionLaneInfos[i].lane;
            if (l.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
            {
                if (l.TurnDirection == IntersectionDirection.Left)
                {
                    rowLeftCount++;
                }
                else
                {
                    rowThruAndRightCount++;
                }
            }
            else
            {
                if (l.TurnDirection == IntersectionDirection.Left)
                {
                    gvLeftCount++;
                }
                else
                {
                    gvThruAndRightCount++;
                }
            }
        }
        IntersectionYieldGroups yg = new IntersectionYieldGroups();
        yg.ROWThruAndRight = new CarsOnLane[rowThruAndRightCount];
        yg.ROWLeft = new CarsOnLane[rowLeftCount];
        yg.GVThruAndRight = new CarsOnLane[gvThruAndRightCount];
        yg.GVLeft = new CarsOnLane[gvLeftCount];

        rowThruAndRightCount = 0;
        rowLeftCount = 0;
        gvThruAndRightCount = 0;
        gvLeftCount = 0;
        for (int i = 0; i < intersectionLaneInfos.Length; i++)
        {
            Lane l = intersectionLaneInfos[i].lane;

            CarsOnLane c = new CarsOnLane();
            c.startNode = l.nodesOnLane[0];
            c.carsOnLane = new List<CarInIntersection>();
            c.carsInside = 0;
            if (l.laneType == LaneType.INTERSECTION_LANE_RIGHT_OF_WAY)
            {
                if (l.TurnDirection == IntersectionDirection.Left)
                {
                    yg.ROWLeft[rowLeftCount] = c;
                    rowLeftCount++;
                }
                else
                {
                    yg.ROWThruAndRight[rowThruAndRightCount] = c;
                    rowThruAndRightCount++;
                }
            }
            else
            {
                if (l.TurnDirection == IntersectionDirection.Left)
                {
                    yg.GVLeft[gvLeftCount] = c;
                    gvLeftCount++;
                }
                else
                {
                    yg.GVThruAndRight[gvThruAndRightCount] = c;
                    gvThruAndRightCount++;
                }
            }
        }
        intersection.yieldGroups = yg;
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
    /// This function executes when target object is activated.
    /// </summary>
    private void OnEnable()
    {
        intersection = target as Intersection;
        road = intersection.GetComponent<Road>();
        showLaneInfos = intersection.showLaneInfos;
        if (intersection.yieldGroups == null || intersection.laneDictionary.Keys.Count == 0)
        {
            UpdateLaneInfos();
        }
    }
    /// <summary>
    /// This function executes when target object is deactivated.
    /// </summary>
    private void OnDisable()
    {
        // saves setting changes
        if (intersection.showLaneInfos != showLaneInfos)
        {
            Undo.RecordObject(intersection, intersection.name + " settings changed");
            intersection.showLaneInfos = showLaneInfos;
        }
    }
}
