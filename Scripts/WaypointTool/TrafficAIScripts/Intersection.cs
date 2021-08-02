using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Author: Henri Tainio

/// <summary>
/// Intersection is automatically added to a new intersection-type Road-object. It's main function is to administrate traffic
/// controllers attached to it via IICBase interface.
/// </summary>
[Serializable]
public class Intersection : MonoBehaviour
{
    /// <summary>
    /// Data of interrelated yielding orders of intersection's lanes.
    /// </summary>
    [SerializeField]
    public IntersectionLaneInfo[] intersectionLaneInfos;
    /// <summary>
    /// Does lane information show in inspector?
    /// </summary>
    public bool showLaneInfos = false;
    /// <summary>
    /// A dictionary of lanes. Dictionary's key = lane, result = data of crossing with other lanes.
    /// </summary>
    [SerializeField]
    public LaneInfoDictionary laneDictionary;
    /// <summary>
    /// Intersections lanes grouped by their yielding status.
    /// </summary>
    [SerializeField]
    public IntersectionYieldGroups yieldGroups;
    /// <summary>
    /// Attached basic intersection controller (no traffic lights).
    /// </summary>
    public ICNoLightsController noLightsController;
    /// <summary>
    /// Attached traffic light controller (optional).
    /// </summary>
    public ICTrafficLightController trafficLightController;
    /// <summary>
    /// Currently active intersection controller.
    /// </summary>
    public IICBase currentController;
    /// <summary>
    /// Updates active controller.
    /// </summary>
    private void Update()
    {
        currentController.UpdateActiveController(Time.deltaTime);
    }
    /// <summary>
    /// If traffic light controller is set of, should set control mode as no-lights. Note: should still update tlc for visuals.
    /// Not implemented.
    /// </summary>
    public void TraficLightsSetOff()
    {

    }
}
