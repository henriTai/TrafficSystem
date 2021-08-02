using UnityEngine;
using UnityEditor;

// Author: Henri Tainio

/// <summary>
/// Editor window UI for creating road types.
/// </summary>
public class WaypointToolWindow : EditorWindow
{
    /// <summary>
    /// Max size of sceneview crosshair.
    /// </summary>
    float crossHairMax = 100f;
    /// <summary>
    /// Min size of sceneview crosshair.
    /// </summary>
    float crosshairMin = 10f;
    /// <summary>
    /// Current size of sceneview crosshair.
    /// </summary>
    float crosshairSize = 10f;
    /// <summary>
    /// Crosshair's size slider value.
    /// </summary>
    float crosshairMultiplier = 1f;
    /// <summary>
    /// Crosshair's speed slider value.
    /// </summary>
    float crosshairSpeedModifier = 1f;
    /// <summary>
    /// Crosshair's current speed.
    /// </summary>
    float crosshairMoveSpeed = 0.3f;
    /// <summary>
    /// Max value of vertical position adjustment.
    /// </summary>
    const float maxVerticalAdjustment = 100f;
    /// <summary>
    /// Max value of horizontal position adjustment.
    /// </summary>
    const float maxHorizontalAdjustment = 100f;
    /// <summary>
    /// Current vertical position adjustment value.
    /// </summary>
    float verticalAdjustment = 0f;
    /// <summary>
    /// Current horizontal position adjustment value.
    /// </summary>
    float horizontalAdjustment = 0f;
    /// <summary>
    /// Determines which menu page is shown in editor window: 0 - start menu, 1 - position menu, 2 - selection menu.
    /// </summary>
    int currentState = 0;
    /// <summary>
    /// Editor window's background color.
    /// </summary>
    Color menuColor;
    /// <summary>
    /// Editor window's button color.
    /// </summary>
    Color buttonColor;
    /// <summary>
    /// Editor window's close-button's color.
    /// </summary>
    Color closeButtonColor;
    /// <summary>
    /// Editor window's reset-button color.
    /// </summary>
    Color resetButtonColor;
    /// <summary>
    /// Is up-button pressed down?
    /// </summary>
    bool upButtonPressed = false;
    /// <summary>
    /// Is down-button pressed down?
    /// </summary>
    bool downButtonPressed = false;
    /// <summary>
    /// Is left-button pressed down?
    /// </summary>
    bool leftButtonPressed = false;
    /// <summary>
    /// Is right-button pressed down?
    /// </summary>
    bool rightButtonPressed = false;
    /// <summary>
    /// Editor window's rect.
    /// </summary>
    Rect rect;
    /// <summary>
    /// Sceneview camera position.
    /// </summary>
    Vector3 camPos;

    /// <summary>
    /// Opens a WaypointTool-editor window.
    /// </summary>
    [MenuItem("Virtulanssi/Open Waypoint Tool")]
    public static void OpenTools()
    {
        WaypointToolWindow window = (WaypointToolWindow)GetWindow(typeof(WaypointToolWindow), true, "WAYPOINT TOOL", true);
        window.minSize = new Vector2(300f, 300f);
        window.maxSize = window.minSize;
        window.ShowUtility();
    }
    /// <summary>
    /// Updates button inputs.
    /// </summary>
    private void Update()
    {
        if (upButtonPressed && verticalAdjustment < maxVerticalAdjustment)
        {
            verticalAdjustment = Mathf.Clamp(verticalAdjustment + crosshairMoveSpeed * crosshairSpeedModifier,
                -maxVerticalAdjustment, maxVerticalAdjustment);
            SceneView.RepaintAll();
        }
        else if (downButtonPressed && verticalAdjustment > -maxVerticalAdjustment)
        {
            verticalAdjustment = Mathf.Clamp(verticalAdjustment - crosshairMoveSpeed * crosshairSpeedModifier,
                -maxVerticalAdjustment, maxVerticalAdjustment);
            SceneView.RepaintAll();
        }
        else if (leftButtonPressed && horizontalAdjustment > -maxHorizontalAdjustment)
        {
            horizontalAdjustment = Mathf.Clamp(horizontalAdjustment - crosshairMoveSpeed * crosshairSpeedModifier,
                -maxHorizontalAdjustment, maxHorizontalAdjustment);
            SceneView.RepaintAll();
        }
        else if (rightButtonPressed && horizontalAdjustment < maxHorizontalAdjustment)
        {
            horizontalAdjustment = Mathf.Clamp(horizontalAdjustment + crosshairMoveSpeed * crosshairSpeedModifier,
                -maxHorizontalAdjustment, maxHorizontalAdjustment);
            SceneView.RepaintAll();
        }
    }
    /// <summary>
    /// When enabled, registers OnSceneGUI-method to Sceneview's duringSceneGui-delegate and initializes values.
    /// </summary>
    private void OnEnable()
    {
        SceneView.duringSceneGui += this.OnSceneGUI;
        menuColor = new Color(0f, 0.1f, 1f, 0.3f);
        buttonColor = new Color(1f, 0.2f, 1f, 1f);
        closeButtonColor = new Color(1f, 0f, 0.2f, 1f);
        resetButtonColor = new Color(1f, 0f, 0.6f, 1f);
        rect = new Rect(0, 0, 300, 300);       
    }
    /// <summary>
    /// When disabled, unregisters OnSceneGUI-method fron Sceneview's duringSceneGui-delegate.
    /// </summary>
    private void OnDisable()
    {
        currentState = 0;
        SceneView.RepaintAll();
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    /// <summary>
    /// Determines what is displayed in tool's editor window.
    /// </summary>
    private void OnGUI()
    {
        GUI.color = menuColor;
        GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
        switch (currentState)
        {
            case 0: StartMenu();
                break;
            case 1: PositionMenu();
                break;
            case 2: SelectionMenu();
                break;
        }
        GUILayout.BeginArea(new Rect(200, 280, 100, 20));
        GUI.color = closeButtonColor;
        if (GUILayout.Button("CLOSE"))
        {
            this.Close();
        }
        GUILayout.EndArea();
    }
    /// <summary>
    /// Start menu editor window content.
    /// </summary>
    private void StartMenu()
    {
        GUILayout.BeginArea(new Rect(50,25,200,200));
        GUI.color = buttonColor;
        if (GUILayout.Button("ADD ITEM"))
        {
            currentState = 1;
            SceneView.RepaintAll();
        }

        GUILayout.EndArea();
        
    }
    /// <summary>
    /// Position menu editor window content.
    /// </summary>
    private void PositionMenu()
    {
        GUILayout.BeginArea(new Rect(50, 25, 200, 200));
        GUILayout.TextArea("Move CROSSHAIR to desired place by using hand tool or by using adjustment buttons below.");
        GUILayout.EndArea();
        GUI.color = buttonColor;

        GUILayout.BeginArea(new Rect(125, 75, 50, 20));
        if (GUILayout.RepeatButton("UP"))
        {
            if (upButtonPressed)
            {
                upButtonPressed = false;
            }
            else
            {
                upButtonPressed = true;
            }
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(125, 125, 50, 20));
        if (GUILayout.RepeatButton("DOWN"))
        {
            if (downButtonPressed)
            {
                downButtonPressed = false;
            }
            else
            {
                downButtonPressed = true;
            }
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(65, 100, 50, 20));
        if (GUILayout.RepeatButton("LEFT"))
        {
            if (leftButtonPressed)
            {
                leftButtonPressed = false;
            }
            else
            {
                leftButtonPressed = true;
            }
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(185, 100, 50, 20));
        if (GUILayout.RepeatButton("RIGHT"))
        {
            if (rightButtonPressed)
            {
                rightButtonPressed = false;
            }
            else
            {
                rightButtonPressed = true;
            }
        }
        GUILayout.EndArea();

        GUI.color = resetButtonColor;
        GUILayout.BeginArea(new Rect(125, 100, 50, 20));
        if (GUILayout.RepeatButton("RESET"))
        {
            horizontalAdjustment = 0f;
            verticalAdjustment = 0f;
            upButtonPressed = false;
            downButtonPressed = false;
            leftButtonPressed = false;
            rightButtonPressed = false;
            SceneView.RepaintAll();
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(20, 150, 70, 20));
        if (GUILayout.Button("CONTINUE"))
        {
            currentState = 2;
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(25, 175, 150, 100));
        GUILayout.Label("Crosshair size", EditorStyles.boldLabel);
        float cm = crosshairMultiplier;
        cm = GUILayout.HorizontalSlider(cm, 1f, 5f);
        if (cm != crosshairMultiplier)
        {
            crosshairMultiplier = cm;
            SceneView.RepaintAll();
        }
        GUILayout.Label("Crosshair speed", EditorStyles.boldLabel);
        float cs = crosshairSpeedModifier;
        cs = GUILayout.HorizontalSlider(cs, 1f, 5f);
        if (cs != crosshairSpeedModifier)
        {
            crosshairSpeedModifier = cs;
        }
        GUILayout.EndArea();
    }
    /// <summary>
    /// Selection menu editor window content.
    /// </summary>
    private void SelectionMenu()
    {
        GUI.color = Color.black;
        GUILayout.Label("Select object type:", EditorStyles.boldLabel);
        GUI.color = closeButtonColor;

        GUILayout.BeginArea(new Rect(10, 50, 150, 20));
        GUILayout.Label("* Road", EditorStyles.boldLabel);
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(150, 50, 100, 20));
        if (GUILayout.Button("Create"))
        {
            GameObject g = new GameObject("new road");
            g.transform.position = new Vector3(camPos.x, 0f, camPos.z);
            g.AddComponent(typeof(ParallelBezierSplines));
            Selection.activeObject = g;
            this.Close();
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, 75, 150, 20));
        GUILayout.Label("* Intersection", EditorStyles.boldLabel);
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(150, 75, 100, 20));
        if (GUILayout.Button("Create"))
        {
            GameObject g = new GameObject("new intersection");
            g.transform.position = new Vector3(camPos.x, 0f, camPos.z);
            g.AddComponent(typeof(IntersectionTool));
            Selection.activeObject = g;
            this.Close();
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, 100, 150, 20));
        GUILayout.Label("* Roundabout", EditorStyles.boldLabel);
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(150, 100, 100, 20));
        if (GUILayout.Button("Create"))
        {

        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, 125, 150, 20));
        GUILayout.Label("* Access road", EditorStyles.boldLabel);
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(150, 125, 100, 20));
        if (GUILayout.Button("Create"))
        {

        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// Sceneview visualization. NOTE: Editor window doesn't have a overridable OnSceneGUI-method (like custom editor tools have),
    /// but this function can be manually registered / unregistered to Sceneview using OnEnable- and OnDisable-methods.
    /// </summary>
    /// <param name="sceneView">Active SceneView.</param>
    private void OnSceneGUI(SceneView sceneView)
    {
        if (currentState == 0)
        {
            return;
        }
        camPos = sceneView.camera.transform.position;
        camPos = new Vector3(camPos.x + horizontalAdjustment, camPos.y, camPos.z + verticalAdjustment);
        Vector3 a1 = new Vector3(camPos.x - crosshairSize * crosshairMultiplier, 0f, camPos.z + crosshairSize * crosshairMultiplier);
        Vector3 a2 = new Vector3(camPos.x + crosshairSize * crosshairMultiplier, 0f, camPos.z - crosshairSize * crosshairMultiplier);
        Vector3 b1 = new Vector3(camPos.x - crosshairSize * crosshairMultiplier, 0f, camPos.z - crosshairSize * crosshairMultiplier);
        Vector3 b2 = new Vector3(camPos.x + crosshairSize * crosshairMultiplier, 0f, camPos.z + crosshairSize * crosshairMultiplier);
        Handles.DrawLine(a1, a2);
        Handles.DrawLine(b1, b2);
    }

}
/*
 * a test
[CustomEditor(typeof(GameObject))]
[CanEditMultipleObjects]
public class WaypointToolButtons : Editor
{
    public void OnSceneGUI()
    {
        Handles.BeginGUI();

        if (GUILayout.Button("Press Me"))
            Debug.Log("Got it to work.");

        Handles.EndGUI();
    }
}*/
