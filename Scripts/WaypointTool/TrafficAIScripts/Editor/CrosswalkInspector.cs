using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Author: Henri Tainio

/// <summary>
/// Custom editor script for objects of type Crosswalk.
/// </summary>
[CustomEditor(typeof(Crosswalk))]
public class CrosswalkInspector : Editor
{
    /// <summary>
    /// Reference to the target object.
    /// </summary>
    private Crosswalk crosswalk;
    /// <summary>
    /// Are passages visualized?
    /// </summary>
    bool passagesToShow = false;
    /// <summary>
    /// A list of in-lane node positions for visualization.
    /// </summary>
    List<Vector3> inLanePoints;
    /// <summary>
    /// A list of out-lane node positions for visualization.
    /// </summary>
    List<Vector3> outLanePoints;
    /// <summary>
    /// List of lane start positions for visualization.
    /// </summary>
    List<Vector3> startPoints;
    /// <summary>
    /// List of lane end positions for visualization.
    /// </summary>
    List<Vector3> endPoints;
    /// <summary>
    /// Adjustment increment value (edit mode).
    /// </summary>
    float increment = 1f;

    /// <summary>
    /// Overrides default inspectorview content.
    /// </summary>
    public override void OnInspectorGUI()
    {
        if (crosswalk.inEditMode == false)
        {
            ShowCrosswalkInfo();
            return;
        }
        // setting up road
        if (crosswalk.road == null)
        {
            EditorGUILayout.LabelField("Please start selecting road");
            Road r = EditorGUILayout.ObjectField("Road", crosswalk.road, typeof(Road), true) as Road;
            if (r != null)
            {
                SetPassages(r);
                FetchLanePositions();
                SceneView.RepaintAll();
            }
        }
        // When crosswalk is created via road component's inspector, passages are not set.
        else if (crosswalk.passages == null)
        {
            SetPassages(crosswalk.road);
            FetchLanePositions();
            SceneView.RepaintAll();
        }
        else
        {
            Road r = EditorGUILayout.ObjectField("Road", crosswalk.road, typeof(Road), true) as Road;
            if (r == null)
            {
                crosswalk.Reset();
                inLanePoints.Clear();
                outLanePoints.Clear();
                startPoints.Clear();
                endPoints.Clear();
                SceneView.RepaintAll();
            }
            else if (r != crosswalk.road)
            {
                crosswalk.Reset();
                SetPassages(r);
                FetchLanePositions();
                SceneView.RepaintAll();
            }

            if (crosswalk.passageSelected == false)
            {
                PassageSelectionMenu();
            }
            else
            {
                CrosswalkAdjustmentMenu();
                EditorGUILayout.Separator();
                CrosswalkPositioningMenu();
                EditorGUILayout.Separator();
                if (GUILayout.Button("Done, create crosswalk"))
                {
                    FinishCrosswalk();
                }
            }

        }

        //base.OnInspectorGUI();
    }
    /// <summary>
    /// Passage setting menu content in inspector view during the edit phase.
    /// </summary>
    /// <param name="r"></param>
    private void SetPassages(Road r)
    {
        crosswalk.road = r;
        crosswalk.passages = r.passages;
        if (crosswalk.passages == null)
        {
            Debug.Log("Road has not passageways set, please fix this.");
        }
        else if (crosswalk.passages.Length == 1)
        {
            crosswalk.passageSelected = true;
            ConfirmPassage();
            SceneView.RepaintAll();
        }
    }
    /// <summary>
    /// Passage selection menu content in inspector view during the edit phase.
    /// </summary>
    private void PassageSelectionMenu()
    {
        if (crosswalk.passages == null)
        {
            passagesToShow = false;
            return;
        }
        passagesToShow = true;
        EditorGUILayout.LabelField("Please select passage:", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        int i = crosswalk.selectedPassage;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-"))
        {
            if (i == 0)
            {
                i = crosswalk.passages.Length - 1;
            }
            else
            {
                i--;
            }
            crosswalk.selectedPassage = i;
            FetchLanePositions();
            SceneView.RepaintAll();
        }
        EditorGUILayout.LabelField(" " + crosswalk.selectedPassage + " ", EditorStyles.boldLabel);
        if (GUILayout.Button("+"))
        {
            if (i == crosswalk.passages.Length - 1)
            {
                i = 0;
            }
            else
            {
                i++;
            }
            crosswalk.selectedPassage = i;
            FetchLanePositions();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Confirm lanes"))
        {
            ConfirmPassage();
            SceneView.RepaintAll();
        }
    }
    /// <summary>
    /// Crosswalk adjustment menu content in inspector view during the edit phase.
    /// </summary>
    private void CrosswalkAdjustmentMenu()
    {
        EditorGUILayout.LabelField("Crosswalk adjustment", EditorStyles.boldLabel);
        increment = EditorGUILayout.FloatField("Increment", increment);
        EditorGUILayout.LabelField("Positioning");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(" - "))
        {
            AdjustPositioning(-increment);
        }
        if (GUILayout.Button(" + "))
        {
            AdjustPositioning(increment);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Width");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(" - "))
        {
            AdjustCrosswalkLength(-increment);
        }
        if (GUILayout.Button(" + "))
        {
            AdjustCrosswalkLength(increment);
        }
        EditorGUILayout.EndHorizontal();

    }
    /// <summary>
    /// Crosswalk positioning menu content in inspector view during the edit phase.
    /// </summary>
    private void CrosswalkPositioningMenu()
    {
        EditorGUILayout.LabelField("Crosswalk positioning", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Anchor node: " + crosswalk.nodeIndex);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous"))
        {
            if (crosswalk.nodeIndex > 0)
            {
                crosswalk.nodeIndex--;
                RecalculateBoxPosition();
                SceneView.RepaintAll();
            }
        }
        if (GUILayout.Button("Next"))
        {
            if (crosswalk.nodeIndex < crosswalk.guideLane.nodesOnLane.Length - 1)
            {
                crosswalk.nodeIndex++;
                RecalculateBoxPosition();
                SceneView.RepaintAll();
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Fine tune position");
        float val = crosswalk.positionBetweenNodes;
        val = EditorGUILayout.Slider(val, 0f, 1f);
        if (val != crosswalk.positionBetweenNodes)
        {
            crosswalk.positionBetweenNodes = val;
            RecalculateBoxPosition();
            SceneView.RepaintAll();
        }
    }
    /// <summary>
    /// Crosswalk inspectorview after editing. Includes a button to test Ai vehicles' response to pedestrians crossing the road.
    /// </summary>
    private void ShowCrosswalkInfo()
    {
        if (GUILayout.Button("Test button - pedestrian crosses"))
        {
            crosswalk.PedestrianIsCrossing();
        }
        base.OnInspectorGUI();
        //
    }
    /// <summary>
    /// Confirms selected passage group when creating a crosswalk in intersection.
    /// </summary>
    private void ConfirmPassage()
    {
        Vector3 pos = Vector3.zero;
        if (crosswalk.passages[crosswalk.selectedPassage].inLanes.Length > 0)
        {
            pos = crosswalk.passages[crosswalk.selectedPassage].inLanes[0].nodesOnLane[0].transform.position;
            crosswalk.guideLane = crosswalk.passages[crosswalk.selectedPassage].inLanes[0];
        }
        else
        {
            pos = crosswalk.passages[crosswalk.selectedPassage].outLanes[0].nodesOnLane[0].transform.position;
            crosswalk.guideLane = crosswalk.passages[crosswalk.selectedPassage].outLanes[0];
        }
        crosswalk.transform.position = pos;
        InitPositionBox();
        crosswalk.passageSelected = true;
    }
    /// <summary>
    /// Initializes edit mode to position the crosswalk.
    /// </summary>
    private void InitPositionBox()
    {
        // Count actual lanes counting connecting nodes
        List<Nodes> lNodes = new List<Nodes>();
        for (int i = 0; i < crosswalk.passages[crosswalk.selectedPassage].inLanes.Length; i++)
        {
            Nodes n = crosswalk.passages[crosswalk.selectedPassage].inLanes[i].nodesOnLane[0];
            if (!lNodes.Contains(n))
            {
                lNodes.Add(n);
            }
        }
        int inLanes = lNodes.Count;
        lNodes.Clear();
        for (int i = 0; i < crosswalk.passages[crosswalk.selectedPassage].outLanes.Length; i++)
        {
            Lane l = crosswalk.passages[crosswalk.selectedPassage].outLanes[i];
            Nodes n;
            if (inLanes > 0)
            {
                n = l.nodesOnLane[l.nodesOnLane.Length - 1];
            }
            else
            {
                n = l.nodesOnLane[0];
            }
            if (!lNodes.Contains(n))
            {
                lNodes.Add(n);
            }
        }
        int outLanes = lNodes.Count;
        //First find out most outermost lanes
        Lane guide = crosswalk.guideLane;
        Vector2 p0 = new Vector2(guide.nodesOnLane[0].transform.position.x, guide.nodesOnLane[0].transform.position.z);
        Vector2 p1 = new Vector2(guide.nodesOnLane[1].transform.position.x, guide.nodesOnLane[1].transform.position.z);
        Vector2 dir = (p1 - p0).normalized;
        Vector2 right = new Vector2(dir.y, -dir.x);
        Vector2 left = new Vector2(-dir.y, dir.x);
        float rightMargin = 0f;
        float leftMargin = 0f;

        if (inLanes > 0)
        {
            rightMargin = (inLanes - 0.5f) * 3.5f;
            if (outLanes == 0)
            {
                leftMargin = 0.5f * 3.5f;
            }
        }
        if (outLanes > 0)
        {
            if (inLanes == 0)
            {
                rightMargin = 0.5f * 3.5f;
            }
            leftMargin = (outLanes + 0.5f) * 3.5f;
        }
        Vector2[] v2Points = { p0 + right * rightMargin,
                                p0 + left * leftMargin,
                                p0 + dir * 4f + right * rightMargin,
                                p0 + dir * 4f + left * leftMargin};
        float[] yPositions = { 0f, 0f, 0f, 0f };
        for (int i = 0; i < 4; i++)
        {
            yPositions[i] = GetHeight(v2Points[i]);
        }
        Vector3[] points = {    new Vector3(v2Points[3].x, yPositions[3], v2Points[3].y),
                                new Vector3(v2Points[2].x, yPositions[2], v2Points[2].y),
                                new Vector3(v2Points[0].x, yPositions[0], v2Points[0].y),
                                new Vector3(v2Points[1].x, yPositions[1], v2Points[1].y)
        };
        crosswalk.cornerPoints = points;
        crosswalk.leftMargin = leftMargin;
        crosswalk.rightMargin = rightMargin;
        crosswalk.adjustment = 0f;
        crosswalk.nodeIndex = 0;
        crosswalk.positionBetweenNodes = 0f;
    }
    /// <summary>
    /// Adjusts the positioning of the crosswalk.
    /// </summary>
    /// <param name="val">Increment value to position the crosswalk.</param>
    private void AdjustPositioning(float val)
    {
        crosswalk.adjustment += val;
        RecalculateBoxPosition();
        SceneView.RepaintAll();
    }
    /// <summary>
    /// Adjusts the length of the crosswalk.
    /// </summary>
    /// <param name="val">Increment value to adjust the length of the crosswalk.</param>
    private void AdjustCrosswalkLength(float val)
    {
        if (crosswalk.rightMargin + val * 0.5f > 0f)
        {
            crosswalk.rightMargin += val * 0.5f;
        }
        if (crosswalk.leftMargin + val * 0.5f > 0f)
        {
            crosswalk.leftMargin += val * 0.5f;
        }
        RecalculateBoxPosition();
        SceneView.RepaintAll();
    }
    /// <summary>
    /// Recalculates crosswalks positioning box.
    /// </summary>
    private void RecalculateBoxPosition()
    {
        Lane guide = crosswalk.guideLane;
        float leftMargin = crosswalk.leftMargin;
        float rightMargin = crosswalk.rightMargin;
        float adj = crosswalk.adjustment;
        int nodeIndex = crosswalk.nodeIndex;
        Vector2 p0 = new Vector2(
            guide.nodesOnLane[nodeIndex].transform.position.x,
            guide.nodesOnLane[nodeIndex].transform.position.z);
        Vector2 dir = Vector2.zero;
        if (crosswalk.nodeIndex < guide.nodesOnLane.Length - 1)
        {
            dir = new Vector2(guide.nodesOnLane[nodeIndex + 1].transform.position.x
                - guide.nodesOnLane[nodeIndex].transform.position.x,
                guide.nodesOnLane[nodeIndex + 1].transform.position.z
                - guide.nodesOnLane[nodeIndex].transform.position.z);
            p0 += dir * crosswalk.positionBetweenNodes;
            dir = dir.normalized;
        }
        else
        {
            dir = new Vector2(guide.nodesOnLane[nodeIndex].transform.position.x
                - guide.nodesOnLane[nodeIndex - 1].transform.position.x,
                guide.nodesOnLane[nodeIndex].transform.position.z
                - guide.nodesOnLane[nodeIndex - 1].transform.position.z).normalized;
        }
        Vector2 right = new Vector2(dir.y, -dir.x);
        Vector2 left = new Vector2(-dir.y, dir.x);
        p0 += adj * right;

        Vector2[] v2Points = { p0 + right * rightMargin,
                                p0 + left * leftMargin,
                                p0 + dir * 4f + right * rightMargin,
                                p0 + dir * 4f + left * leftMargin};
        float[] yPositions = { 0f, 0f, 0f, 0f };
        for (int i = 0; i < 4; i++)
        {
            yPositions[i] = GetHeight(v2Points[i]);
        }
        Vector3[] points = {    new Vector3(v2Points[0].x, yPositions[0], v2Points[0].y),
                                new Vector3(v2Points[1].x, yPositions[1], v2Points[1].y),
                                new Vector3(v2Points[3].x, yPositions[3], v2Points[3].y),
                                new Vector3(v2Points[2].x, yPositions[2], v2Points[2].y)
        };
        crosswalk.cornerPoints = points;

    }
    /// <summary>
    /// At the end of the edit phase, finishes the crosswalk.
    /// </summary>
    private void FinishCrosswalk()
    {
        CreateMesh();
        CalculateStopPoints();
        crosswalk.inEditMode = false;
    }
    /// <summary>
    /// Creates crosswalks mesh.
    /// </summary>
    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = crosswalk.name + "_mesh";

        Vector2 p0 = CrossingPoint(
            new Vector2(crosswalk.cornerPoints[1].x, crosswalk.cornerPoints[1].z),
            new Vector2(crosswalk.cornerPoints[3].x, crosswalk.cornerPoints[3].z),
            new Vector2(crosswalk.cornerPoints[2].x, crosswalk.cornerPoints[2].z),
            new Vector2(crosswalk.cornerPoints[0].x, crosswalk.cornerPoints[0].z));

        Vector3 pos = new Vector3(p0.x, 0f, p0.y);

        float length = Vector3.Distance(crosswalk.cornerPoints[0], crosswalk.cornerPoints[1]);
        float width = Vector3.Distance(crosswalk.cornerPoints[0], crosswalk.cornerPoints[3]);
        Vector3 p = crosswalk.transform.position;
        Vector3[] vertices = { new Vector3(-width * 0.5f, 0f, length * 0.5f),
            new Vector3(width * 0.5f, 0f, length * 0.5f),
            new Vector3(-width * 0.5f, 0f, -length * 0.5f),
            new Vector3(width * 0.5f, 0f, -length * 0.5f)};

        /*
        Vector3[] vertices = {crosswalk.cornerPoints[1] - pos + new Vector3(0f, GetHeight(crosswalk.cornerPoints[1]) + 0.01f, 0f),
            crosswalk.cornerPoints[2] - pos + new Vector3(0f, GetHeight(crosswalk.cornerPoints[2]) + 0.01f, 0f),
            crosswalk.cornerPoints[0] - pos + new Vector3(0f, GetHeight(crosswalk.cornerPoints[0]) + 0.01f, 0f),
            crosswalk.cornerPoints[3] - pos + new Vector3(0f, GetHeight(crosswalk.cornerPoints[3]) + 0.01f, 0f) };*/

        /*
        Vector3[] vertices = {crosswalk.cornerPoints[1] - pos + new Vector3(0f, 0.05f, 0f),
            crosswalk.cornerPoints[2] - pos + new Vector3(0f, 0.05f, 0f),
            crosswalk.cornerPoints[0] - pos + new Vector3(0f, 0.05f, 0f),
            crosswalk.cornerPoints[3] - pos + new Vector3(0f, 0.05f, 0f) };
        */
        Vector3[] normals = { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
        Vector2[] uvs = { new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f)
        };
        int[] triangles = { 0, 1, 2, 2, 1, 3};
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        GameObject g = new GameObject(mesh.name);
        MeshFilter meshFilter = (MeshFilter)g.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = mesh;
        MeshRenderer meshRenderer = g.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        BoxCollider collider = g.AddComponent(typeof(BoxCollider)) as BoxCollider;
        float yVal = GetHeight(p0);
        g.transform.position = new Vector3(pos.x, yVal, pos.z);
        crosswalk.transform.position = pos;

        //rotation
        Vector3 p1 = crosswalk.cornerPoints[0];
        Vector3 p2 = crosswalk.cornerPoints[3];

        float RAD2DEG = 180f / Mathf.PI;
        float rotation = Mathf.Atan2(p2.z - p1.z, p2.x - p1.x) * RAD2DEG;

        g.transform.Rotate(new Vector3(0f, -rotation, 0f));
        g.transform.parent = crosswalk.transform;
    }
    /// <summary>
    /// Calculates stopping points for cars before the crosswalk.
    /// </summary>
    private void CalculateStopPoints()
    {
        InOutPassageWays pw = crosswalk.passages[crosswalk.selectedPassage];
        List<Lane> laneList = new List<Lane>();
        for (int i = 0; i < pw.inLanes.Length; i++)
        {
            laneList.Add(pw.inLanes[i]);
        }
        for (int i = 0; i < pw.outLanes.Length; i++)
        {
            if (!laneList.Contains(pw.outLanes[i]))
            {
                laneList.Add(pw.outLanes[i]);
            }
        }

        Vector2 A1 = new Vector2(crosswalk.cornerPoints[0].x, crosswalk.cornerPoints[0].z);
        Vector2 A2 = new Vector2(crosswalk.cornerPoints[1].x, crosswalk.cornerPoints[1].z);
        Vector2 B1 = new Vector2(crosswalk.cornerPoints[2].x, crosswalk.cornerPoints[2].z);
        Vector2 B2 = new Vector2(crosswalk.cornerPoints[3].x, crosswalk.cornerPoints[3].z);

        List<Lane> laneLog = new List<Lane>();
        List<Vector2> crossingPointLog = new List<Vector2>();
        for (int i = 0; i < laneList.Count; i++)
        {
            Lane l = laneList[i];
            bool passedFirst = false;
            for (int j = 0; j < l.nodesOnLane.Length - 1; j++)
            {
                Vector2 C1 = new Vector2(l.nodesOnLane[j].transform.position.x, l.nodesOnLane[j].transform.position.z);
                Vector2 C2 = new Vector2(l.nodesOnLane[j + 1].transform.position.x, l.nodesOnLane[j + 1].transform.position.z);
                Vector2 p1 = CrossingPoint(A1, A2, C1, C2);
                Vector2 p2 = CrossingPoint(B1, B2, C1, C2);
                Vector2 earlier = Vector2.zero;
                Vector2 latter = Vector2.zero;
                Nodes n1 = l.nodesOnLane[j];
                Nodes n2 = l.nodesOnLane[j + 1];
                if (p1 == Vector2.zero)
                {
                    earlier = p2;
                    n1 = n2;
                }
                else if (p2 == Vector2.zero)
                {
                    earlier = p1;
                }
                else
                {
                    if (Vector2.Distance(C1, p1) < Vector2.Distance(C1, p2))
                    {
                        earlier = p1;
                        latter = p2;
                    }
                    else
                    {
                        earlier = p2;
                        latter = p1;
                        n2 = n1;
                        n1 = l.nodesOnLane[j + 1];
                    }
                }

                if (earlier != Vector2.zero)
                {
                    if (crossingPointLog.Count > 0 && (earlier == crossingPointLog[crossingPointLog.Count -1]))
                    {
                        // prevents duplicates if crossing point is exactly at the position of a node
                    }
                    else if (passedFirst)
                    {
                        passedFirst = false;
                    }
                    else
                    {
                        passedFirst = true;
                        laneLog.Add(l);
                        crossingPointLog.Add(earlier);
                    }
                }
                if (latter != Vector2.zero)
                {
                    if (crossingPointLog.Count > 0 && (latter == crossingPointLog[crossingPointLog.Count - 1]))
                    {
                        // prevents duplicates if crossing point is exactly at the position of a node
                    }
                    else if (passedFirst)
                    {
                        passedFirst = false;
                    }
                    else
                    {
                        passedFirst = true;
                        laneLog.Add(l);
                        crossingPointLog.Add(latter);
                    }
                }

            }
        }
        Lane[] lanes = new Lane[laneLog.Count];
        Vector2[] crPoints = new Vector2[crossingPointLog.Count];
        for (int i = 0; i < laneLog.Count; i++)
        {
            lanes[i] = laneLog[i];
        }
        for (int i = 0; i < crossingPointLog.Count; i++)
        {
            crPoints[i] = crossingPointLog[i];
        }
        Nodes[] beforeNodes = new Nodes[lanes.Length];
        for (int i = 0; i < lanes.Length; i++)
        {
            beforeNodes[i] = NodeBeforePoint(lanes[i], crPoints[i]);
        }
        crosswalk.beforeNodes = beforeNodes;
        crosswalk.crossingPoints = crPoints;
        crosswalk.lanes = lanes;
    }
    /// <summary>
    /// Finds the first node on selected lane before the crosswalk
    /// </summary>
    /// <param name="l">Selected lane.</param>
    /// <param name="point">Point where lane crosses the crosswalk.</param>
    /// <returns>Fisrt node on the lane before the crosswalk.</returns>
    private Nodes NodeBeforePoint(Lane l, Vector2 point)
    {
        if (l == crosswalk.guideLane && crosswalk.positionBetweenNodes == 0f)
        {
            return (crosswalk.guideLane.nodesOnLane[crosswalk.nodeIndex]);
        }
        Nodes n = null;

        for (int i = 0; i < l.nodesOnLane.Length - 1; i++)
        {
            Vector2 p1 = new Vector2(l.nodesOnLane[i].transform.position.x, l.nodesOnLane[i].transform.position.z);
            if (Vector2.Distance(point, p1) < 0.05f)
            {
                n = l.nodesOnLane[i];
                break;
            }
            Vector2 p2 = new Vector2(l.nodesOnLane[i + 1].transform.position.x, l.nodesOnLane[i + 1].transform.position.z);
            // floating point inaccuracy, the value is not exactly zero
            if (DistanceLineSegmentPoint(p1, p2, point) < 0.00001f)
            {
                n = l.nodesOnLane[i];
                break;
            }
        }
        if (n == null)
        {
            Debug.Log("Check failed");
        }
        return n;
    }
    /// <summary>
    /// Returns shortest distance from point P to line AB.
    /// </summary>
    /// <param name="a">Line's start point (a).</param>
    /// <param name="b">Line's end point (b)</param>
    /// <param name="p">Point compared to the line.</param>
    /// <returns>Shortest distance from point P to line AB.</returns>
    private float DistanceLineSegmentPoint(Vector2 a, Vector2 b, Vector2 p)
    {
        // If a == b line segment is a point and will cause a divide by zero in the line segment test.
        // Instead return distance from a
        if (a == b)
            return Vector2.Distance(a, p);

        // Line segment to point distance equation
        Vector2 ba = b - a;
        Vector2 pa = a - p;
        float dist = (pa - ba * (Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba))).magnitude;

        return dist;
    }
    /// <summary>
    /// Calculates point where two lines cross.
    /// </summary>
    /// <param name="A1">Line 1 start point.</param>
    /// <param name="A2">Line 1 end point.</param>
    /// <param name="B1">Line 2 start point.</param>
    /// <param name="B2">Line 2 end point.</param>
    /// <returns>Point where two lines cross. If lines don't cross, returns Vector2.zero.</returns>
    public static Vector2 CrossingPoint (Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);
        if (tmp == 0)
        {
            return Vector2.zero;
        }
        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        Vector2 point = new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
            );
        if (point.x > A1.x && point.x > A2.x)
        {
            return Vector2.zero;
        }
        if (point.x > B1.x && point.x > B2.x)
        {
            return Vector2.zero;
        }
        if (point.x < A1.x && point.x < A2.x)
        {
            return Vector2.zero;
        }
        if (point.x < B1.x && point.x < B2.x)
        {
            return Vector2.zero;
        }
        if (point.y > A1.y && point.y > A2.y)
        {
            return Vector2.zero;
        }
        if (point.y > B1.y && point.y > B2.y)
        {
            return Vector2.zero;
        }
        if (point.y < A1.y && point.y < A2.y)
        {
            return Vector2.zero;
        }
        if (point.y < B1.y && point.y < B2.y)
        {
            return Vector2.zero;
        }
        return point;
    }
    /// <summary>
    /// Calculates terrain height at given xz-coordinates.
    /// </summary>
    /// <param name="point">Point's xz-coordinates where terrain height is tested.</param>
    /// <returns>Terrain height at the given point.</returns>
    private float GetHeight(Vector2 point)
    {
        Ray r = new Ray(new Vector3(point.x, 50f, point.y), Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, float.MaxValue))
        {
            return hit.point.y;
        }
        else
        {
            return 0f;
        }
    }
    /// <summary>
    /// Updates lane position lists for visualization.
    /// </summary>
    private void FetchLanePositions()
    {
        inLanePoints = new List<Vector3>();
        outLanePoints = new List<Vector3>();
        startPoints = new List<Vector3>();
        endPoints = new List<Vector3>();
        if (crosswalk.passages == null)
        {
            return;
        }
        if (crosswalk.passages.Length > 0)
        {
            Lane[] ins = crosswalk.passages[crosswalk.selectedPassage].inLanes;
            for (int i = 0; i < ins.Length; i++)
            {
                for (int j = 0; j < ins[i].nodesOnLane.Length - 1; j++)
                {
                    inLanePoints.Add(ins[i].nodesOnLane[j].transform.position);
                    inLanePoints.Add(ins[i].nodesOnLane[j + 1].transform.position);
                }
                startPoints.Add(ins[i].nodesOnLane[0].transform.position);
            }
            Lane[] outs = crosswalk.passages[crosswalk.selectedPassage].outLanes;
            for (int i = 0; i < outs.Length; i++)
            {
                for (int j = 0; j < outs[i].nodesOnLane.Length - 1; j++)
                {
                    outLanePoints.Add(outs[i].nodesOnLane[j].transform.position);
                    outLanePoints.Add(outs[i].nodesOnLane[j + 1].transform.position);
                }
                endPoints.Add(outs[i].nodesOnLane[outs[i].nodesOnLane.Length - 1].transform.position);
            }
        }
    }
    /// <summary>
    /// Visualizes selected lane group.
    /// </summary>
    private void ShowLanes()
    {
        if (inLanePoints.Count > 0)
        {
            Handles.color = Color.blue;
            for (int i = 0; i < inLanePoints.Count; i += 2)
            {
                Handles.DrawLine(inLanePoints[i], inLanePoints[i + 1]);
            }
        }
        if (outLanePoints.Count > 0)
        {
            Handles.color = Color.red;
            for (int i = 0; i < outLanePoints.Count; i += 2)
            {
                Handles.DrawLine(outLanePoints[i], outLanePoints[i + 1]);
            }
        }
    }
    /// <summary>
    /// Visualizes start and end points of the selected lane group.
    /// </summary>
    private void ShowPoints()
    {
        if (crosswalk.inEditMode == false)
        {
            return;
        }
        Handles.color = Color.yellow;
        for (int i = 0; i < startPoints.Count; i++)
        {
            Handles.DrawSolidDisc(startPoints[i], new Vector3(0f, 1f, 0f),
            0.01f * Vector3.Distance(startPoints[i],
            SceneView.lastActiveSceneView.camera.transform.position));
        }
        Handles.color = Color.white;
        for (int i = 0; i < endPoints.Count; i++)
        {
            Handles.DrawSolidDisc(endPoints[i], new Vector3(0f, 1f, 0f),
            0.01f * Vector3.Distance(endPoints[i],
            SceneView.lastActiveSceneView.camera.transform.position));
        }
    }
    /// <summary>
    /// Visualize points where lanes cross the crosswalk.
    /// </summary>
    private void ShowCrossingPoints()
    {
        if (crosswalk.inEditMode)
        {
            return;
        }
        if (crosswalk.crossingPoints == null)
        {
            return;
        }
        Handles.color = Color.yellow;
        for (int i = 0; i < crosswalk.crossingPoints.Length; i++)
        {
            Vector3 p = new Vector3(crosswalk.crossingPoints[i].x, 0f, crosswalk.crossingPoints[i].y);
            Handles.DrawSolidDisc(p, new Vector3(0f, 1f, 0f),
            0.01f * Vector3.Distance(p, SceneView.lastActiveSceneView.camera.transform.position));
            Handles.Label(p, "Point " + i);
        }
        Handles.color = Color.white;
        for (int i = 0; i < crosswalk.beforeNodes.Length; i++)
        {
            Vector3 p = crosswalk.beforeNodes[i].transform.position;
            Handles.DrawSolidDisc(p, new Vector3(0f, 1f, 0f),
            0.01f * Vector3.Distance(p, SceneView.lastActiveSceneView.camera.transform.position));
            Handles.Label(p, "Nodes " + i);
        }
    }
    /// <summary>
    /// Visualizes crosswalks box.
    /// </summary>
    private void ShowRect()
    {
        if (crosswalk.passageSelected == false)
        {
            return;
        }
        Handles.DrawSolidRectangleWithOutline(crosswalk.cornerPoints, new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0f, 0f, 0f, 1f));
    }
    /// <summary>
    /// Sceneview update method.
    /// </summary>
    private void OnSceneGUI()
    {
        ShowLanes();
        ShowPoints();
        ShowRect();
        ShowCrossingPoints();
    }
    /// <summary>
    /// This method is executed when an object of target's type is activated.
    /// </summary>
    private void OnEnable()
    {
        crosswalk = target as Crosswalk;
        FetchLanePositions();
    }
    /// <summary>
    /// This method is executed when an object of target's type is deactivated.
    /// </summary>
    private void OnDisable()
    {
        //CrosswalkUpdateTool.UpdateCrosswalks();
    }
}
