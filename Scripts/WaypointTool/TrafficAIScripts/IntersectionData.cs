using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Author: Henri Tainio

/// <summary>
/// In intersections, lanes (driving lines, to be more specific) may cross each other. For each lane, traffic control
/// needs information of which lanes should give way and which lanes this lane should give way in turn. This information
/// is automatically calculated. In equal intersecting situations (i.e. two equal opposite, left-turning lanes), one of
/// the lanes is set to give way to the other automatically, too.
/// </summary>
[Serializable]
public class IntersectionLaneInfo
{
    /// <summary>
    /// Reference to a lane-object.
    /// </summary>
    public Lane lane;
    /// <summary>
    /// Array of information of intersection points where this lane should give way to other lanes.
    /// </summary>
    public LaneCrossingPoint[] lanesToGiveWay;
    /// <summary>
    /// Array of information of intersection points where other lanes should give way to this lane.
    /// </summary>
    public LaneCrossingPoint[] lanesGivingWay;
}

/// <summary>
/// Contains data of a car in intersection for intersection controller. It has references to its game object and
/// controller script, to the lane car will drive while in intersection and a Boolean if car has already entered
/// the intersection or still closing in.
/// </summary>
[Serializable]
public class CarInIntersection
{
    /*
    /// <summary>
    /// Reference to car’s game object.
    /// </summary>
    [SerializeField]
    public GameObject car;
    /// <summary>
    /// Reference to the lane that car will drive while in intersection.
    /// </summary>
    [SerializeField]
    public Lane usedLane;
    /// <summary>
    /// Reference to car’s controller script.
    /// </summary>
    [SerializeField]
    public CPUCarDrive carDrive;*/
    public CarDriveData driveData;
    /// <summary>
    /// A Boolean if car has already entered the intersection (or still closing in).
    /// </summary>
    public bool inIntersection;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="carObject">Car's gameobject.</param>
    /// <param name="cDrive">Car's controller script.</param>
    /// <param name="carsLane">The lane car will occupy while driving in intersection.</param>
    public CarInIntersection(CarDriveData data)
    {
        /*
        car = carObject;
        carDrive = cDrive;
        usedLane = carsLane;*/
        driveData = data;
        inIntersection = false;
    }
}
/// <summary>
/// Contains data of cars on one lane in intersection, used for controlling traffic flow (yielding and unyielding).
/// </summary>
[Serializable]
public class CarsOnLane
{
    /// <summary>
    /// The first node of a lane in intersection. In intersection, start node is often shared with multiple lanes
    /// (driving lines to varying directions). In intersection controller, these cars must be handled as a group as
    /// yielding a car on one of these lanes affects the others, too.
    /// </summary>
    [SerializeField]
    public Nodes startNode;
    /// <summary>
    /// List of data of cars on this lane.
    /// </summary>
    [SerializeField]
    public List<CarInIntersection> carsOnLane;
    /// <summary>
    /// Counter of cars currently crossing the intersection.
    /// </summary>
    public int carsInside = 0;
    /// <summary>
    /// Index reference to the carsOnLane-list, referring to the closest car closing into the intersection but still
    /// not inside. When the traffic on this lane is yielded, this car gets the order to yield.
    /// </summary>
    public int closestIndex = 0;
}

/// <summary>
/// A data container class for keeping count of cars in intersection without traffic lights.
/// </summary>
[Serializable]
public class IntersectionYieldGroups
{
    /// <summary>
    /// Cars on lanes with right of way yield status driving through and turning right.
    /// </summary>
    public CarsOnLane[] ROWThruAndRight;
    /// <summary>
    /// Cars on lanes with right of way yield status turning left.
    /// </summary>
    public CarsOnLane[] ROWLeft;
    /// <summary>
    /// Cars on lanes with yielding status driving through and turning right.
    /// </summary>
    public CarsOnLane[] GVThruAndRight;
    /// <summary>
    /// Cars on lanes with yielding status turning left.
    /// </summary>
    public CarsOnLane[] GVLeft;
}

/// <summary>
/// A data container class for keeping count of car in traffic light controlled intersection.
/// </summary>
[Serializable]
public class IntersectionPhaseGroups
{
    /// <summary>
    /// Cars on lanes of phase 1.
    /// </summary>
    public CarsOnLane[] phase1Lanes;
    /// <summary>
    /// Cars on lanes of phase 2.
    /// </summary>
    public CarsOnLane[] phase2Lanes;
    /// <summary>
    /// Cars on lanes of phase 3.
    /// </summary>
    public CarsOnLane[] phase3Lanes;
    /// <summary>
    /// Cars on lanes of phase 4.
    /// </summary>
    public CarsOnLane[] phase4Lanes;
}

/// <summary>
/// In intersection controller, intersection lane info-data is stored in this dictionary form.
/// </summary>
[Serializable]
public class LaneInfoDictionary : SeriazableDictionary<Lane, IntersectionLaneInfo> { }

// https://answers.unity.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html
/// <summary>
/// A generic serializable dictionary.
/// </summary>
/// <typeparam name="TKey">Type of the key in dictionary, must be serializable.</typeparam>
/// <typeparam name="TValue">Type of the value in dictionary, must serializable.</typeparam>
[Serializable]
public class SeriazableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    /// <summary>
    /// List of dictionary keys.
    /// </summary>
    [SerializeField]
    private List<TKey> keys = new List<TKey>();
    /// <summary>
    /// List of dictionary values.
    /// </summary>
    [SerializeField]
    private List<TValue> values = new List<TValue>();
    /// <summary>
    /// Unity’s callback function, executed when object is deserialized.
    /// </summary>
    public void OnAfterDeserialize()
    {
        this.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            this.Add(keys[i], values[i]);
        }
    }
    /// <summary>
    /// Unity’s callback function, executed when object is serialized.
    /// </summary>
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
}

/// <summary>
/// A data container class for traffic light controller's traffic light settings.
/// </summary>
[Serializable]
public class TrafficLightSettings
{
    /// <summary>
    /// Number of phases.
    /// </summary>
    [Range(2, 4)]
    public int phases = 2;
    /// <summary>
    /// Is phase count hidden (on straight roads)?
    /// </summary>
    public bool phaseCountHidden = false;
    /// <summary>
    /// Phase cycle overall length.
    /// </summary>
    public float cycleLegth = 0f;
    /// <summary>
    /// The length of all phases.
    /// </summary>
    public float[] phaseLengths = new float[] { 0f, 0f, 0f, 0f };
    /// <summary>
    /// Phases of each lane group.
    /// </summary>
    public int[] phaseOfLaneGroup = { 0, 0, 0, 0, 0, 0,
                                        0, 0, 0, 0, 0, 0 };
    /// <summary>
    /// The length of the safety time.
    /// </summary>
    public float safetyTime = 5f;
    /// <summary>
    /// The length of the pre-yellow time.
    /// </summary>
    public float preYellowTime = 1f;
    /// <summary>
    /// The length of the post-yellow time.
    /// </summary>
    public float postYellowTime = 5f;
}

/// <summary>
/// A data container for traffic light controlled crosswalk.
/// </summary>
[Serializable]
public class TrafficLightCrosswalk
{
    /// <summary>
    /// The crosswalk of this traffic light crosswalk.
    /// </summary>
    public Crosswalk crosswalk = null;
    /// <summary>
    /// Light switches controlling traffic lights connected to this crosswalk.
    /// </summary>
    public TrafficLightSwitch[] lightSwitch = new TrafficLightSwitch[0];
    /// <summary>
    /// Green light phase index of this crosswalk.
    /// </summary>
    public int phase = 0;
}