using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// A controller to switch traffic light's visual appearance.
/// </summary>
public class TrafficLightSwitch : MonoBehaviour
{
    /// <summary>
    /// A mesh renderer array of light posts lights. The order is: 0 -red, 1 -green, 2 -yellow, 3 -side green.
    /// </summary>
    [Tooltip("Order: red, green, yellow, side green")]
    public MeshRenderer[] lights;
    /// <summary>
    /// A material for blank light.
    /// </summary>
    public Material blankMaterial;
    /// <summary>
    /// A material for green material.
    /// </summary>
    public Material greenMaterial;
    /// <summary>
    /// A material for red material.
    /// </summary>
    public Material redMaterial;
    /// <summary>
    /// A material for yellow material.
    /// </summary>
    public Material yellowMaterial;

    /// <summary>
    /// Enum list of traffic light post types (2 lights, 3 lights, 3+1 (not currently implemented)).
    /// </summary>
    public enum LightType
    {
        TWO_LIGHTS,
        THREE_LIGHTS,
        THREE_PLUS_RIGHT,
        THREE_PLUS_LEFT
    }
    /// <summary>
    /// Enum list of traffic light states. List includes states for traffic light posts with extra lights (3 + 1), although
    /// there is not implementation for those light systems.
    /// </summary>
    public enum State
    {
        LIGHTS_OFF,
        //Normal modes
        GREEN,
        YELLOW,
        RED,
        RED_YELLOW,
        // with right lane extra light
        RED_RIGHT_OFF,
        RED_RIGHT_GREEN,
        RED_YELLOW_RIGHT_GREEN,
        GREEN_RIGHT_GREEN,
        GREEN_RIGHT_OFF,
        YELLOW_RIGHT_OFF,
        // with left extra light
        RED_LEFT_OFF,
        RED_YELLOW_LEFT_OFF,
        GREEN_LEFT_OFF,
        GREEN_LEFT_GREEN,
        YELLOW_LEFT_OFF,
    }
    /// <summary>
    /// Traffic light switches type.
    /// </summary>
    public LightType type;
    /// <summary>
    /// Traffic lights' status.
    /// </summary>
    public State lightState;
    /// <summary>
    /// Switches lights to the given state.
    /// </summary>
    /// <param name="state">A state that the lights are changed to.</param>
    public void SwitchLights(State state)
    {
        lightState = state;
        UpdateMaterials();
    }
    /// <summary>
    /// Updates light materials according the current state.
    /// </summary>
    private void UpdateMaterials()
    {
        switch(lightState)
        {
            case State.LIGHTS_OFF:
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].material = blankMaterial;
                }
                break;
            case State.GREEN:
                for (int i = 0; i < lights.Length; i++)
                {
                    if (i==1)
                    {
                        lights[i].material = greenMaterial;
                    }
                    else
                    {
                        lights[i].material = blankMaterial;
                    }
                }
                break;
            case State.YELLOW:
                for (int i = 0; i < lights.Length; i++)
                {
                    if (i == 2)
                    {
                        lights[i].material = yellowMaterial;
                    }
                    else
                    {
                        lights[i].material = blankMaterial;
                    }
                }
                break;
            case State.RED:
                for (int i = 0; i < lights.Length; i++)
                {
                    if (i == 0)
                    {
                        lights[i].material = redMaterial;
                    }
                    else
                    {
                        lights[i].material = blankMaterial;
                    }
                }
                break;
            case State.RED_YELLOW:
                for (int i = 0; i < lights.Length; i++)
                {
                    if (i == 0)
                    {
                        lights[i].material = redMaterial;
                    }
                    else if (i == 2)
                    {
                        lights[i].material = yellowMaterial;
                    }
                    else
                    {
                        lights[i].material = blankMaterial;
                    }
                }
                break;
            case State.RED_RIGHT_OFF:
                lights[0].material = redMaterial;
                lights[1].material = blankMaterial;
                lights[2].material = blankMaterial;
                lights[3].material = blankMaterial;
                break;
            case State.RED_RIGHT_GREEN:
                lights[0].material = redMaterial;
                lights[1].material = blankMaterial;
                lights[2].material = blankMaterial;
                lights[3].material = greenMaterial;
                break;
            case State.RED_YELLOW_RIGHT_GREEN:
                lights[0].material = redMaterial;
                lights[1].material = blankMaterial;
                lights[2].material = yellowMaterial;
                lights[3].material = greenMaterial;
                break;
            case State.GREEN_RIGHT_GREEN:
                lights[0].material = blankMaterial;
                lights[1].material = greenMaterial;
                lights[2].material = blankMaterial;
                lights[3].material = greenMaterial;
                break;
            case State.GREEN_RIGHT_OFF:
                lights[0].material = blankMaterial;
                lights[1].material = greenMaterial;
                lights[2].material = blankMaterial;
                lights[3].material = blankMaterial;
                break;
            case State.YELLOW_RIGHT_OFF:
                lights[0].material = blankMaterial;
                lights[1].material = blankMaterial;
                lights[2].material = yellowMaterial;
                lights[3].material = blankMaterial;
                break;
            case State.RED_LEFT_OFF:
                lights[0].material = redMaterial;
                lights[1].material = blankMaterial;
                lights[2].material = blankMaterial;
                lights[3].material = blankMaterial;
                break;
            case State.RED_YELLOW_LEFT_OFF:
                lights[0].material = redMaterial;
                lights[1].material = blankMaterial;
                lights[2].material = yellowMaterial;
                lights[3].material = blankMaterial;
                break;
            case State.GREEN_LEFT_OFF:
                lights[0].material = blankMaterial;
                lights[1].material = greenMaterial;
                lights[2].material = blankMaterial;
                lights[3].material = blankMaterial;
                break;
            case State.GREEN_LEFT_GREEN:
                lights[0].material = blankMaterial;
                lights[1].material = greenMaterial;
                lights[2].material = blankMaterial;
                lights[3].material = greenMaterial;
                break;
            case State.YELLOW_LEFT_OFF:
                lights[0].material = blankMaterial;
                lights[1].material = blankMaterial;
                lights[2].material = yellowMaterial;
                lights[3].material = blankMaterial;
                break;
        }
    }
}
