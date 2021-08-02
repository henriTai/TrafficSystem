using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Author: Henri Tainio

/// <summary>
/// A traffic light based traffic controller type.
/// </summary>
[Serializable]
public class ICTrafficLightController : IICBase
{
    /// <summary>
    /// Enum list of traffic light controller types, based on the road type (straight road, T-intersection, 4-way intersection).
    /// </summary>
    public enum TLCType
    {
        NOT_SET,
        CROSSWALK_STOP,
        T_INTERSECTION,
        FOUR_WAY_INTERSECTION
    }
    /// <summary>
    /// Enum list of traffic light controller's activity mode states.
    /// </summary>
    public enum ActivityMode
    {
        SHUT_DOWN,
        NOT_IN_USE,
        ACTIVE
    }
    /// <summary>
    /// Enum list of traffic light cycle states.
    /// </summary>
    public enum LightChangeStatus
    {
        PRE_YELLOW,
        GREEN,
        POST_YELLOW,
        SAFETY_TIME
    }
    /// <summary>
    /// Intersection component attached to the same gameobject.
    /// </summary>
    public Intersection intersection;
    /// <summary>
    /// Has car selection in intersection changed?
    /// </summary>
    public bool carSelectionChanged = true;
    /// <summary>
    /// An [int] array for counting cars: 0 = sum, 1 = cars on phase 1 lanes, 2 = phase 2,
    /// 3 = phase 3 and 4 = phase 4.
    /// </summary>
    public int[] carCounts = new int[] { 0, 0, 0, 0, 0 };
    /// <summary>
    /// Traffic lights current phase.
    /// </summary>
    public int currentPhase = 1;
    /// <summary>
    /// Traffic light's timer.
    /// </summary>
    public float timer = 0f;
    /// <summary>
    /// If traffic lights are shut down, the lights are in blinking mode.
    /// </summary>
    private bool blinkOn = false;
    /// <summary>
    /// Traffic light controller's type.
    /// </summary>
    public TLCType tlcType;
    /// <summary>
    /// Traffic lights settings data.
    /// </summary>
    public TrafficLightSettings tlcSettings;
    /// <summary>
    /// Current traffic light state.
    /// </summary>
    public LightChangeStatus lightChangeStatus = LightChangeStatus.GREEN;
    /// <summary>
    /// Traffic light's activity mode.
    /// </summary>
    public ActivityMode activity;
    /// <summary>
    /// ATM passages must be set manually for an intersection. Basically this means grouping lanes coming from
    /// each direction to its own group. ATM, the system supports max 4 passage groups (4-way intersection). Other available
    /// options are T-intersection (3 passage groups) and straight road (2 passage groups).
    /// </summary>
    [SerializeField]
    public InOutPassageWays[] passages;
    /// <summary>
    /// An array of traffic light crosswalks in this intersection (or road).
    /// </summary>
    [SerializeField]
    public TrafficLightCrosswalk[] crosswalks;
    /// <summary>
    /// Passage 1 traffic light switches.
    /// </summary>
    public TrafficLightSwitch[] passage1Switches;
    /// <summary>
    /// Passage 2 traffic light switches.
    /// </summary>
    public TrafficLightSwitch[] passage2Switches;
    /// <summary>
    /// Passage 3 traffic light switches.
    /// </summary>
    public TrafficLightSwitch[] passage3Switches;
    /// <summary>
    /// Passage 4 traffic light switches.
    /// </summary>
    public TrafficLightSwitch[] passage4Switches;
    /// <summary>
    /// Traffic light controller's phase group data container for keeping track of car counts.
    /// </summary>
    [SerializeField]
    public IntersectionPhaseGroups phaseGroups;

    //inspector variables


    /// <summary>
    /// Are lanes visualized in sceneview?
    /// </summary>
    public bool showLanes;
    /// <summary>
    /// A boolean array of which lanes are shown when visualization is on.
    /// </summary>
    public bool[] lanesShowing = {false, false, false, false, false, false,
                                    false, false, false, false, false, false};
    /// <summary>
    /// Obligatory implementation of abstract class, no current functionality.
    /// </summary>
    public override void CarSelectionChanged()
    {
        //
    }
    /// <summary>
    /// Unity's built-in Awake function.
    /// </summary>
    private void Awake()
    {
        if (intersection == null)
        {
            intersection = gameObject.GetComponent<Intersection>();
        }
        SetMode(activity);
    }
    /// <summary>
    /// Intersection's update function calls this function when this traffic controller type is active.
    /// </summary>
    /// <param name="dTime">Delta time value.</param>
    public override void UpdateActiveController(float dTime)
    {
        if (carSelectionChanged)
        {
            CarSelectionChanged();
        }
        UpdateActivity(dTime);
    }
    /// <summary>
    /// Update's controller's status.
    /// </summary>
    /// <param name="dTime"></param>
    private void UpdateActivity(float dTime)
    {
        timer += dTime;
        bool pedestrianPhase = false;
        bool isCrosswalkStop = false;
        if (tlcType == TLCType.CROSSWALK_STOP)
        {
            isCrosswalkStop = true;
        }
        if (isCrosswalkStop)
        {
            if (crosswalks == null)
            {
                Debug.Log("Crosswalk stop without crosswalks");
                return;
            }
            if (crosswalks[0].phase == currentPhase)
            {
                pedestrianPhase = true;
            }
        }

        switch (lightChangeStatus)
        {
            case LightChangeStatus.PRE_YELLOW:
                if (timer > tlcSettings.preYellowTime)
                {
                    timer = 0;
                    lightChangeStatus = LightChangeStatus.GREEN;
                    SetLights(TrafficLightSwitch.State.GREEN, currentPhase);
                    foreach (TrafficLightCrosswalk t in crosswalks)
                    {
                        if (t.phase == currentPhase)
                        {
                            t.crosswalk.SetWalkAndDriveStatus(true, false, isCrosswalkStop);
                            if (isCrosswalkStop == false)
                            {
                                SetCrosswalkColor(t, true);
                            }
                        }
                        else
                        {
                            t.crosswalk.SetWalkAndDriveStatus(false, true, isCrosswalkStop);
                        }
                    }
                    UnyieldPhaseLanes(currentPhase);
                }
                break;
            case LightChangeStatus.GREEN:
                if (timer > tlcSettings.phaseLengths[currentPhase - 1])
                {
                    timer = 0f;
                    if (isCrosswalkStop && pedestrianPhase)
                    {
                        lightChangeStatus = LightChangeStatus.SAFETY_TIME;
                        foreach (TrafficLightCrosswalk t in crosswalks)
                        {
                            t.crosswalk.SetWalkAndDriveStatus(false, false, isCrosswalkStop);
                            SetCrosswalkColor(t, false);
                        }
                    }
                    else
                    {
                        lightChangeStatus = LightChangeStatus.POST_YELLOW;
                        SetLights(TrafficLightSwitch.State.YELLOW, currentPhase);
                        if (isCrosswalkStop)
                        {
                            foreach (TrafficLightCrosswalk t in crosswalks)
                            {
                                t.crosswalk.SetWalkAndDriveStatus(false, false, isCrosswalkStop);
                            }
                        }
                    }
                    YieldPhaseLanes(currentPhase);
                }
                break;
            case LightChangeStatus.POST_YELLOW:
                if (timer > tlcSettings.postYellowTime)
                {
                    timer = 0f;
                    lightChangeStatus = LightChangeStatus.SAFETY_TIME;
                    SetLights(TrafficLightSwitch.State.RED, currentPhase);
                    foreach (TrafficLightCrosswalk t in crosswalks)
                    {
                        if (t.phase == currentPhase)
                        {
                            t.crosswalk.SetWalkAndDriveStatus(false, false, isCrosswalkStop);
                            SetCrosswalkColor(t, false);
                        }
                    }
                }
                break;
            case LightChangeStatus.SAFETY_TIME:
                if (timer > tlcSettings.safetyTime)
                {
                    timer = 0f;
                    lightChangeStatus = LightChangeStatus.PRE_YELLOW;
                    if (currentPhase == tlcSettings.phases)
                    {
                        currentPhase = 1;
                    }
                    else
                    {
                        currentPhase++;
                    }
                    // the phase is backwards because the update is done after the check
                    if (isCrosswalkStop && !pedestrianPhase)
                    {
                        lightChangeStatus = LightChangeStatus.GREEN;
                        foreach (TrafficLightCrosswalk t in crosswalks)
                        {
                            t.crosswalk.SetWalkAndDriveStatus(true, false, true);
                            SetCrosswalkColor(t, true);
                        }
                    }
                    else
                    {
                        SetLights(TrafficLightSwitch.State.RED_YELLOW, currentPhase);
                    }
                }
                break;
        }
    }
    /// <summary>
    /// An AI car calls this function to check in to an intersection and receives an ID token.
    /// </summary>
    /// <param name="data">Cars data component.</param>
    /// <returns>An identification token.</returns>
    public override int[] CarCheckIn(CarDriveData data)
    {
        int[] carRetrievingIndex = new int[2];
        CarInIntersection c = new CarInIntersection(data);
        Lane lane = data.intersectionLane;
        Nodes start = lane.nodesOnLane[0];
        bool found = false;
        for (int i = 0; i < phaseGroups.phase1Lanes.Length; i++)
        {
            if (phaseGroups.phase1Lanes[i].startNode == start)
            {
                phaseGroups.phase1Lanes[i].carsOnLane.Add(c);
                CalculateClosestIndex(phaseGroups.phase1Lanes[i]);
                carRetrievingIndex[0] = 1;
                carRetrievingIndex[1] = i;
                carCounts[1]++;
                break;
            }
        }
        if (found == false)
        {
            for (int i = 0; i < phaseGroups.phase2Lanes.Length; i++)
            {
                if (phaseGroups.phase2Lanes[i].startNode == start)
                {
                    phaseGroups.phase2Lanes[i].carsOnLane.Add(c);
                    CalculateClosestIndex(phaseGroups.phase2Lanes[i]);
                    carRetrievingIndex[0] = 2;
                    carRetrievingIndex[1] = i;
                    carCounts[2]++;
                    break;
                }
            }
        }
        if (found == false)
        {
            for (int i = 0; i < phaseGroups.phase3Lanes.Length; i++)
            {
                if (phaseGroups.phase3Lanes[i].startNode == start)
                {
                    phaseGroups.phase3Lanes[i].carsOnLane.Add(c);
                    CalculateClosestIndex(phaseGroups.phase3Lanes[i]);
                    carRetrievingIndex[0] = 3;
                    carRetrievingIndex[1] = i;
                    carCounts[3]++;
                    break;
                }
            }
        }
        if (found == false)
        {
            for (int i = 0; i < phaseGroups.phase4Lanes.Length; i++)
            {
                if (phaseGroups.phase4Lanes[i].startNode == start)
                {
                    phaseGroups.phase4Lanes[i].carsOnLane.Add(c);
                    CalculateClosestIndex(phaseGroups.phase4Lanes[i]);
                    carRetrievingIndex[0] = 4;
                    carRetrievingIndex[1] = i;
                    carCounts[4]++;
                    break;
                }
            }
        }
        carCounts[0]++;
        data.intersectionYielding = IsLaneYielding(lane);
        carSelectionChanged = true;
        return carRetrievingIndex;
    }
    /// <summary>
    /// Checks if given lane is currently yielding.
    /// </summary>
    /// <param name="lane">Checked lane.</param>
    /// <returns>Is lane yielding?</returns>
    private bool IsLaneYielding(Lane lane)
    {
        if (lightChangeStatus == LightChangeStatus.SAFETY_TIME)
        {
            return true;
        }
        if (lightChangeStatus == LightChangeStatus.POST_YELLOW)
        {
            return true;
        }
        bool isYielded = true;
        if (currentPhase == 1)
        {
            for (int i = 0; i < phaseGroups.phase1Lanes.Length; i++)
            {
                if (phaseGroups.phase1Lanes[i].startNode == lane.nodesOnLane[0])
                {
                    isYielded = false;
                    break;
                }
            }
        }
        else if (currentPhase == 2)
        {
            for (int i = 0; i < phaseGroups.phase2Lanes.Length; i++)
            {
                if (phaseGroups.phase2Lanes[i].startNode == lane.nodesOnLane[0])
                {
                    isYielded = false;
                    break;
                }
            }
        }
        else if (currentPhase == 3)
        {
            for (int i = 0; i < phaseGroups.phase3Lanes.Length; i++)
            {
                if (phaseGroups.phase3Lanes[i].startNode == lane.nodesOnLane[0])
                {
                    isYielded = false;
                    break;
                }
            }
        }
        else if (currentPhase == 4)
        {
            for (int i = 0; i < phaseGroups.phase4Lanes.Length; i++)
            {
                if (phaseGroups.phase4Lanes[i].startNode == lane.nodesOnLane[0])
                {
                    isYielded = false;
                    break;
                }
            }
        }
        return isYielded;

    }
    /// <summary>
    /// AI car calls this function to check out when it exits the intersection.
    /// </summary>
    /// <param name="carObject">Car's gameobject.</param>
    /// <param name="token">Car's identification token.</param>
    public override void CarCheckOut(GameObject carObject, int[] token)
    {
        carCounts[0]--;
        carCounts[token[0]]--;

        CarsOnLane c = GetCarsOnLane(token);
        RemoveCarFromIntersection(c, carObject);
        carSelectionChanged = true;
    }
    /// <summary>
    /// Gets cars on lane group that the car with given ID token belongs to.
    /// </summary>
    /// <param name="token">An identification token.</param>
    /// <returns>Cars on lane of car with given ID token.</returns>
    private CarsOnLane GetCarsOnLane(int[] token)
    {
        CarsOnLane c = null;
        switch (token[0])
        {
            case 1:
                c = phaseGroups.phase1Lanes[token[1]];
                break;
            case 2:
                c = phaseGroups.phase2Lanes[token[1]];
                break;
            case 3:
                c = phaseGroups.phase3Lanes[token[1]];
                break;
            case 4:
                c = phaseGroups.phase4Lanes[token[1]];
                break;
        }
        return c;
    }
    /// <summary>
    /// Sets traffic light status (off / shut down / on). Doesn't yet support a safe switching during a gameplay.
    /// </summary>
    /// <param name="mode">A mode the traffic light controller is set to.</param>
    private void SetMode(ActivityMode mode)
    {
        timer = 0f;
        switch (activity)
        {
            case ActivityMode.SHUT_DOWN:
                if (intersection != null)
                {
                    intersection.TraficLightsSetOff();
                }
                SetAllLights(TrafficLightSwitch.State.LIGHTS_OFF);
                CrosswalkLightsOff();
                break;
            case ActivityMode.NOT_IN_USE:
                if (intersection != null)
                {
                    intersection.TraficLightsSetOff();
                }
                SetAllLights(TrafficLightSwitch.State.LIGHTS_OFF);
                CrosswalkLightsOff();
                blinkOn = false;
                break;
            case ActivityMode.ACTIVE:
                InitToPhase(1);
                InitActiveLights();
                InitCrosswalkLights();
                break;

        }
    }

    /// <summary>
    /// The main eason for this function is to operate lights in blinking mode and shutting lights down.
    /// </summary>
    /// <param name="lightState"></param>
    private void SetAllLights(TrafficLightSwitch.State lightState)
    {
        foreach (TrafficLightSwitch t in passage1Switches)
        {
            t.SwitchLights(lightState);
        }
        foreach (TrafficLightSwitch t in passage2Switches)
        {
            t.SwitchLights(lightState);
        }
        foreach (TrafficLightSwitch t in passage3Switches)
        {
            t.SwitchLights(lightState);
        }
        foreach (TrafficLightSwitch t in passage4Switches)
        {
            t.SwitchLights(lightState);
        }
    }
    /// <summary>
    /// Turns crosswalk lights off.
    /// </summary>
    private void CrosswalkLightsOff()
    {
        foreach (TrafficLightCrosswalk c in crosswalks)
        {
            c.crosswalk.canWalk = true;
            foreach (TrafficLightSwitch t in c.lightSwitch)
            {
                t.SwitchLights(TrafficLightSwitch.State.LIGHTS_OFF);
            }
        }
    }
    /// <summary>
    /// Initializes traffic lights to the start of given phase.
    /// </summary>
    /// <param name="phase">Phase index.</param>
    private void InitToPhase(int phase)
    {
        currentPhase = phase;
        lightChangeStatus = LightChangeStatus.GREEN;
        if (intersection != null)
        {
            UnyieldPhaseLanes(phase);
        }
    }
    /// <summary>
    /// Unyield cars of given phase.
    /// </summary>
    /// <param name="unyieldedPhase">Unyielded phase.</param>
    public void UnyieldPhaseLanes(int unyieldedPhase)
    {
        currentPhase = unyieldedPhase;
        switch (currentPhase)
        {
            case 1:
                UnyieldPhaseGroup(ref phaseGroups.phase1Lanes);
                break;
            case 2:
                UnyieldPhaseGroup(ref phaseGroups.phase2Lanes);
                break;
            case 3:
                UnyieldPhaseGroup(ref phaseGroups.phase3Lanes);
                break;
            case 4:
                UnyieldPhaseGroup(ref phaseGroups.phase4Lanes);
                break;
        }
    }
    /// <summary>
    /// Unyield cars of a subgroup of a phase.
    /// </summary>
    /// <param name="group">Unyielded cars on lane group.</param>
    private void UnyieldPhaseGroup(ref CarsOnLane[] group)
    {
        for (int i = 0; i < group.Length; i++)
        {
            CarsOnLane col = group[i];
            CalculateClosestIndex(col);
            if (col.carsOnLane.Count > 0 && col.closestIndex >= 0)
            {
                for (int j = 0; j < col.carsOnLane.Count; j++)
                {
                    CarInIntersection cii = col.carsOnLane[j];
                    if (j == col.closestIndex && IsYieldedPhase(cii.driveData.intersectionLane) && !cii.driveData.inIntersection)
                    {
                        CarIntersectionUpdate.SetIntersectionYieding(cii.driveData, true);
                        //cii.carDrive.SetIntersectionYieding(true);
                    }
                    else
                    {
                        CarIntersectionUpdate.SetIntersectionYieding(cii.driveData, false);
                        //cii.carDrive.SetIntersectionYieding(false);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Yield cars of given phase.
    /// </summary>
    /// <param name="phase">Yielded phase.</param>
    public void YieldPhaseLanes(int phase)
    {
        switch (phase)
        {
            case 1:
                YieldPhaseGroup(ref phaseGroups.phase1Lanes);
                break;
            case 2:
                YieldPhaseGroup(ref phaseGroups.phase2Lanes);
                break;
            case 3:
                YieldPhaseGroup(ref phaseGroups.phase3Lanes);
                break;
            case 4:
                YieldPhaseGroup(ref phaseGroups.phase4Lanes);
                break;

        }

    }
    /// <summary>
    /// Yields cars of a subgroup of a phase.
    /// </summary>
    /// <param name="group">Yielded phase.</param>
    private void YieldPhaseGroup(ref CarsOnLane[] group)
    {
        for (int i = 0; i < group.Length; i++)
        {
            CarsOnLane col = group[i];
            CalculateClosestIndex(col);
            if (col.carsOnLane.Count > 0 && col.closestIndex >= 0)
            {
                for (int j = 0; j < col.carsOnLane.Count; j++)
                {
                    CarInIntersection cii = col.carsOnLane[j];
                    if (j == col.closestIndex && !cii.driveData.inIntersection)
                    {
                        CarIntersectionUpdate.SetIntersectionYieding(cii.driveData, true);
                        //cii.carDrive.SetIntersectionYieding(true);
                    }
                    else
                    {
                        CarIntersectionUpdate.SetIntersectionYieding(cii.driveData, false);
                        //cii.carDrive.SetIntersectionYieding(false);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Checks if lane belongs to a yielded phase.
    /// </summary>
    /// <param name="l">Checked lane.</param>
    /// <returns>Is lane yielded?</returns>
    private bool IsYieldedPhase(Lane l)
    {
        bool yielded = true;
        if (currentPhase == 1)
        {
            for (int i = 0; i < phaseGroups.phase1Lanes.Length; i++)
            {
                if (phaseGroups.phase1Lanes[i].startNode == l.nodesOnLane[0])
                {
                    yielded = false;
                    break;
                }
            }
        }
        else if (currentPhase == 2)
        {
            for (int i = 0; i < phaseGroups.phase2Lanes.Length; i++)
            {
                if (phaseGroups.phase2Lanes[i].startNode == l.nodesOnLane[0])
                {
                    yielded = false;
                    break;
                }
            }
        }
        else if (currentPhase == 3)
        {
            for (int i = 0; i < phaseGroups.phase3Lanes.Length; i++)
            {
                if (phaseGroups.phase3Lanes[i].startNode == l.nodesOnLane[0])
                {
                    yielded = false;
                    break;
                }
            }
        }
        else if (currentPhase == 4)
        {
            for (int i = 0; i < phaseGroups.phase4Lanes.Length; i++)
            {
                if (phaseGroups.phase4Lanes[i].startNode == l.nodesOnLane[0])
                {
                    yielded = false;
                    break;
                }
            }
        }
        return yielded;
    }
    /// <summary>
    /// Initializes traffic lights and crosswalks.
    /// </summary>
    private void InitActiveLights()
    {
        lightChangeStatus = LightChangeStatus.GREEN;
        for (int i = 1; i <= tlcSettings.phases; i++)
        {
            if (i == currentPhase)
            {
                SetLights(TrafficLightSwitch.State.GREEN, i);
            }
            else
            {
                SetLights(TrafficLightSwitch.State.RED, i);
            }
        }
        bool isCrosswalk = false;
        if (tlcType == TLCType.CROSSWALK_STOP)
        {
            isCrosswalk = true;
        }
        foreach (TrafficLightCrosswalk t in crosswalks)
        {
            if (t.phase == currentPhase)
            {
                t.crosswalk.SetWalkAndDriveStatus(true, false, isCrosswalk);
                SetCrosswalkColor(t, true);
            }
            else
            {
                t.crosswalk.SetWalkAndDriveStatus(false, true, isCrosswalk);
                SetCrosswalkColor(t, false);
            }
        }
    }
    /// <summary>
    /// Operates switching lights.
    /// </summary>
    /// <param name="lightState">Traffic light state.</param>
    /// <param name="phase">Current phase.</param>
    private void SetLights(TrafficLightSwitch.State lightState, int phase)
    {
        bool[] doesChange = new bool[4];
        for (int i = 0; i < tlcSettings.phaseOfLaneGroup.Length; i++)
        {
            if (tlcSettings.phaseOfLaneGroup[i] == phase)
            {
                doesChange[i / 3] = true;
            }
        }
        if (doesChange[0] == true)
        {
            foreach (TrafficLightSwitch t in passage1Switches)
            {
                t.SwitchLights(lightState);
            }
        }
        if (doesChange[1] == true)
        {
            foreach (TrafficLightSwitch t in passage2Switches)
            {
                t.SwitchLights(lightState);
            }
        }
        if (doesChange[2] == true)
        {
            foreach (TrafficLightSwitch t in passage3Switches)
            {
                t.SwitchLights(lightState);
            }
        }
        if (doesChange[3] == true)
        {
            foreach (TrafficLightSwitch t in passage4Switches)
            {
                t.SwitchLights(lightState);
            }
        }

    }

    /// <summary>
    /// Set's crosswalk's traffic light color.
    /// </summary>
    /// <param name="t">A traffic light crosswalk.</param>
    /// <param name="isGreen">Is crosswalk traffic light green for pedestrians?</param>
    private void SetCrosswalkColor(TrafficLightCrosswalk t, bool isGreen)
    {
        foreach (TrafficLightSwitch tls in t.lightSwitch)
        {
            if (isGreen)
            {
                tls.SwitchLights(TrafficLightSwitch.State.GREEN);
            }
            else
            {
                tls.SwitchLights(TrafficLightSwitch.State.RED);
            }
        }
    }
    /// <summary>
    /// Initializes the state of crosswalk traffic lights.
    /// </summary>
    private void InitCrosswalkLights()
    {
        if (crosswalks == null)
        {
            return;
        }
        for (int i = 0; i < crosswalks.Length; i++)
        {
            TrafficLightCrosswalk c = crosswalks[i];
            for (int j = 0; j < c.lightSwitch.Length; j++)
            {
                if (c.phase != currentPhase)
                {
                    c.lightSwitch[j].SwitchLights(TrafficLightSwitch.State.RED);
                }
                else
                {
                    c.lightSwitch[j].SwitchLights(TrafficLightSwitch.State.GREEN);
                }
            }
        }
    }

    /// <summary>
    /// This function is called when AI car actually enters an intersection.
    /// </summary>
    /// <param name="token">Car's identification token.</param>
    /// <param name="carObject">Car's gameobject.</param>
    public override void CarInsideIntersection(int[] token, GameObject carObject)
    {
        //carSelectionChanged = true;
        CarsOnLane c = GetCarsOnLane(token);
        for (int i = 0; i < c.carsOnLane.Count; i++)
        {
            CarInIntersection cii = c.carsOnLane[i];
            if (cii.driveData.carObject == carObject)
            {
                cii.inIntersection = true;
                break;
            }
        }
        c.carsInside++;
        CalculateClosestIndex(c);
        carSelectionChanged = true;
    }
    /// <summary>
    /// Resets component and sets controller type.
    /// </summary>
    /// <param name="t">Type of this traffic light controller.</param>
    public void Reset(TLCType t)
    {
        tlcType = t;
        lanesShowing = new bool[] {
            false, false, false, false, false, false,
            false, false, false, false, false, false};
        if (t != TLCType.NOT_SET)
        {
            Road r = GetComponent<Road>();
            if (t == TLCType.CROSSWALK_STOP)
            {
                passages = new InOutPassageWays[1];
                passages[0] = r.passages[0];
            }
            else
            {
                passages = r.passages;
            }
            tlcSettings = new TrafficLightSettings();
            DefaultTrafficLightSettings(t);
        }
    }
    /// <summary>
    /// Sets default traffic light settings.
    /// </summary>
    /// <param name="type">Type of traffic light controller.</param>
    public void DefaultTrafficLightSettings(TLCType type)
    {
        switch (type)
        {
            case TLCType.CROSSWALK_STOP:
                tlcSettings.phases = 2;
                tlcSettings.cycleLegth = 60f;
                tlcSettings.phaseLengths = new float[] { 30f, 30f, 0f, 0f };
                break;
            case TLCType.T_INTERSECTION:
                tlcSettings.phases = 3;
                tlcSettings.cycleLegth = 90f;
                tlcSettings.phaseLengths = new float[] { 30f, 30f, 30f, 0f };
                break;
            case TLCType.FOUR_WAY_INTERSECTION:
                tlcSettings.phases = 4;
                tlcSettings.cycleLegth = 120f;
                tlcSettings.phaseLengths = new float[] { 30f, 30f, 30f, 30f };
                break;
        }
    }
}
