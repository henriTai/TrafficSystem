using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Car's AI uses this class to control car's signal lights.
/// </summary>
public static class CarLightUpdate
{
    /// <summary>
    /// Operates car's turn signal lights.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    /// <param name="laneChanged">Is car changing a lane?</param>
    public static void OperateTurnSignals(CarDriveData data, bool laneChanged)
    {
        //if lane has changed and turning, sets signals on.
        if (laneChanged)
        {
            TurnSignalsOn(data, true);
        }
        else if (data.turnSignalsOn > 0)
        {
            UseTurnSignal(data);
        }
    }
    /// <summary>
    /// Set turn signals on.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    /// <param name="laneChanged">Is car changing a lane?</param>
    public static void TurnSignalsOn(CarDriveData data, bool laneChanged)
    {
        data.turnSignalTimer = 0f;
        data.signalIsLit = true;
        if (laneChanged)
        {
            IntersectionDirection direction = data.currentLane.TurnDirection;
            if (direction == IntersectionDirection.Left)
            {
                data.turnSignalsOn = 1;
                data.carControl.SetTurnSignal(true, true);
                data.carControl.SetTurnSignal(false, false);
            }
            else if (direction == IntersectionDirection.Right)
            {
                data.turnSignalsOn = 2;
                data.carControl.SetTurnSignal(false, true);
                data.carControl.SetTurnSignal(true, false);
            }
            else
            {
                data.turnSignalsOn = 0;
                data.carControl.SetTurnSignal(true, false);
                data.carControl.SetTurnSignal(false, false);
            }
        }
        else
        {
            if (data.laneChange == LaneChange.NotChanging)
            {
                data.turnSignalsOn = 0;
                data.carControl.SetTurnSignal(true, false);
                data.carControl.SetTurnSignal(false, false);
            }
            else
            {
                if (data.changingLeft)
                {
                    data.turnSignalsOn = 1;
                    data.carControl.SetTurnSignal(true, true);
                    data.carControl.SetTurnSignal(false, false);
                }
                else
                {
                    data.turnSignalsOn = 2;
                    data.carControl.SetTurnSignal(false, true);
                    data.carControl.SetTurnSignal(true, false);
                }
            }
        }
    }
    /// <summary>
    /// Sets car's turn signal lights off.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void TurnSignalsOff(CarDriveData data)
    {
        data.turnSignalsOn = 0;
        data.carControl.SetTurnSignal(true, false);
        data.carControl.SetTurnSignal(false, false);
    }
    /// <summary>
    /// Delivers current turn light status to the car's control unit.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    private static void UseTurnSignal(CarDriveData data)
    {
        if (data.currentLane.TurnDirection == IntersectionDirection.Straight)
        {
            if (data.laneChange == LaneChange.NotChanging)
            {
                TurnSignalsOff(data);
            }
        }
        data.turnSignalTimer += Time.deltaTime;
        if (data.turnSignalTimer > 0.6f)
        {
            data.turnSignalTimer = 0f;
            data.signalIsLit = !data.signalIsLit;
            bool turnLeft = false;
            if (data.turnSignalsOn == 1)
            {
                turnLeft = true;
            }
            data.carControl.SetTurnSignal(turnLeft, data.signalIsLit);
        }

    }
    /// <summary>
    /// Set's cars brake lights off (This function is public because CarDataInitializer uses it when reseting the car's status).
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void BrakeLightsOff(CarDriveData data)
    {
        if (data.brakeLightsOn)
        {
            data.brakeLightsOn = false;
            data.carControl.SetBrakeLightStatus(false);
        }
    }
    /// <summary>
    /// Set's car's brake lights on.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    private static void BrakeLightsOn(CarDriveData data)
    {
        if (!data.brakeLightsOn)
        {
            data.brakeLightsOn = true;
            data.carControl.SetBrakeLightStatus(true);
        }
    }
    /// <summary>
    /// Operates car's brake lights.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void UseBrakeLights(CarDriveData data)
    {
        bool lightsOn = false;
        if (data.obstacleAhead && data.previousHitDistance < CarDriveData.stoppingDistance)
        {
            lightsOn = true;
        }
        else if (data.intersectionYielding && data.distanceToIntersection < CarDriveData.stoppingDistance)
        {
            lightsOn = true;
        }
        else if (data.crosswalkYielding && data.distanceToCrosswalk < CarDriveData.stoppingDistance)
        {
            lightsOn = true;
        }
        if (lightsOn)
        {
            BrakeLightsOn(data);
        }
        else
        {
            BrakeLightsOff(data);
        }
    }

}
