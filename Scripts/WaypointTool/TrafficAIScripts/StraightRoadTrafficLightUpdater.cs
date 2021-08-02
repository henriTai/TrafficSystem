using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A patch script for unfinished traffic light / crosswalk system. Attach this script to a straight road-object with
/// crosswalks and traffic lights. This simply handles updating traffic light controller (which is usually handled by
/// intersection controller).
/// </summary>
public class StraightRoadTrafficLightUpdater : MonoBehaviour
{
    /// <summary>
    /// Traffic light controller attached to this gameobject.
    /// </summary>
    private ICTrafficLightController trafficLightController;
    /// <summary>
    /// Is there a traffic light controller attached to this gameobject?
    /// </summary>
    bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        trafficLightController = GetComponent<ICTrafficLightController>();
        if (trafficLightController != null)
        {
            active = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            trafficLightController.UpdateActiveController(Time.deltaTime);
        }
    }
}
