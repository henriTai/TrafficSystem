using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Author: Henri Tainio

/// <summary>
/// A custom editor script for CarSpawner-class.
/// </summary>
[CustomEditor(typeof(CarSpawner))]
public class CarSpawnerInspector : Editor
{
    /// <summary>
    /// The target CarSpawner object.
    /// </summary>
    CarSpawner cSpawner;
    /// <summary>
    /// An array of all nodes.
    /// </summary>
    Nodes[] allNodes;
    /// <summary>
    /// Are node labels displayed?
    /// </summary>
    bool showNodes = false;

    /// <summary>
    /// Overrides the inspector view for this type of object.
    /// </summary>
    public override void OnInspectorGUI()
    {
        //This was used in multimaterial assigning
        if (GUILayout.Button("Press here"))
        {
            Debug.Log("Not in use currently.");
            /*
            CPUCarDrive cDrive = cSpawner.car.GetComponent<CPUCarDrive>();
            Debug.Log(cDrive.axleInfos[0].leftVisuals.GetComponent<MeshRenderer>().sharedMaterials.Length);
            Material cap = cDrive.capMat;
            Material rubber = cDrive.rubberMat;
            Material[] mats = new Material[] { rubber, cap };
            foreach (AxleInfo a in cDrive.axleInfos)
            {
                MeshRenderer ml = a.leftVisuals.GetComponent<MeshRenderer>();
                MeshRenderer mr = a.rightVisuals.GetComponent<MeshRenderer>();
                ml.sharedMaterials = mats;
                mr.sharedMaterials = mats;
            }*/
        }
        base.OnInspectorGUI();

        bool show = showNodes;
        show = EditorGUILayout.ToggleLeft("Show Node labels?", show);
        {
            if (show != showNodes)
            {
                if (show == true && allNodes == null)
                {
                    allNodes = FindObjectsOfType<Nodes>();
                }
                showNodes = show;
            }
        }
    }
    /// <summary>
    /// Sceneview operations when an object of this type is selected.
    /// </summary>
    private void OnSceneGUI()
    {
        if (showNodes)
        {
            Handles.color = Color.black;
            foreach (Nodes n in allNodes)
            {
                Handles.Label(n.transform.position, n.gameObject.name);
            }
            SceneView.RepaintAll();
        }
    }
    /// <summary>
    /// Executes when an object of this type is selected.
    /// </summary>
    private void OnEnable()
    {
        cSpawner = target as CarSpawner;
    }
}
