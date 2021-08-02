using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Road network is the highest tier in node network hierarchy.
/// </summary>
public class RoadNetwork : MonoBehaviour
{
    /// <summary>
    /// A boolean if lanes are visualized in sceneview as lines from their start points to their end points.
    /// </summary>
    public bool showLines;
    /// <summary>
    /// A boolean if lanes are visualized in detail.
    /// </summary>
    public bool showDetailed;

    /// <summary>
    /// Unity's built-in function, executed when component is reset.
    /// </summary>
    private void Reset()
    {
        showLines = false;
        showDetailed = false;
    }
}
