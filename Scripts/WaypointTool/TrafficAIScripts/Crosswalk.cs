using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Author: Henri Tainio

/// <summary>
/// Crosswalks are ATM created in following manner: Create gameobject -> Add component (Crosswalk) ->
/// In inspector, select a road this crosswalk will be set -> Adjust crosswalk in inspector -> Press 'done' button. IF crosswalk
/// is added to a traffic light intersection, crosswalk should be assigned to the traffic light intersection controller in order to
/// follow intersection traffic light phases.
/// On TO DO-list is moving crosswalk creation as a button in road-component's inspector view and also separating
/// tool / functionality / data.
/// </summary>
[Serializable]
public class Crosswalk : MonoBehaviour
{
    /// <summary>
    /// A road that this crosswalk is associated with.
    /// </summary>
    public Road road;
    /// <summary>
    /// Passage groups of the road this crosswak is associated with.
    /// </summary>
    public InOutPassageWays[] passages;
    /// <summary>
    /// The selected passage group that this crosswalk is associated with.
    /// </summary>
    public int selectedPassage;
    /// <summary>
    /// An array of the lanes that this crosswalk crosses.
    /// </summary>
    public Lane[] lanes;
    /// <summary>
    /// An array of xz-coordinates of points where crosswalk crosses a lane.
    /// </summary>
    public Vector2[] crossingPoints;
    /// <summary>
    /// Four corner point positions of this crosswalk.
    /// </summary>
    public Vector3[] cornerPoints;
    /// <summary>
    /// Last nodes on each lane before the crosswalk.
    /// </summary>
    public Nodes[] beforeNodes;
    /// <summary>
    /// The lane this crosswalk uses as its orientation guide.
    /// </summary>
    public Lane guideLane;
    /// <summary>
    /// Crosswalks length adjusment, left side from pivot point.
    /// </summary>
    public float leftMargin = 0f;
    /// <summary>
    /// Crosswalks length adjustment, right side from pivot point.
    /// </summary>
    public float rightMargin = 0f;
    /// <summary>
    /// Crosswalks pivot point's position adjusment (left / right).
    /// </summary>
    public float adjustment = 0f;
    /// <summary>
    /// This index is used in positioning crosswalk to selected node on guiding lane.
    /// </summary>
    public int nodeIndex = 0;
    /// <summary>
    /// This value (between 0 and 1) positions crosswalk between nodes on guiding lane.
    /// </summary>
    public float positionBetweenNodes = 0f;
    /// <summary>
    /// State of crosswalks edit phase. Is crosswalk already set to a certain passage?
    /// </summary>
    public bool passageSelected = false;
    /// <summary>
    /// Is crosswalk still in edit mode?
    /// </summary>
    public bool inEditMode = true;
    /// <summary>
    /// Can pedestrian cross the road? If a car is crossing the crosswalk, this boolean is false.
    /// </summary>
    public bool canWalk = true;
    /// <summary>
    /// Can car cross the crosswalk? If there is a red traffic light on for cars, this boolean is false.
    /// </summary>
    public bool canDrive = true;
    /// <summary>
    /// Is pedestrian crossing the crosswalk?
    /// </summary>
    public bool pedestrianIsCrossing = false;
    /// <summary>
    /// The timer is used as an apprximation of how much it takes for a pedestrian to cross
    /// the road (10 sec) - should maybe later be replaced with a method to decrease pedestriansWalking count
    /// when pedestrian has crossed the road.
    /// </summary>
    private float pedestrianTimer = 0f;
    /// <summary>
    /// The timer is used as a safe approximation of how much it takes a car to drive
    /// across the crosswalk (2 sec).
    /// </summary>
    public float carCrossingTimer = 0f;
    /// <summary>
    /// Is car driving across the crosswalk.
    /// </summary>
    public bool carIsCrossing = false;
    /// <summary>
    /// Restets crosswalk back to edit mode.
    /// </summary>
    public void Reset()
    {
        road = null;
        passages = null;
        lanes = null;
        beforeNodes = null;
        selectedPassage = 0;
        guideLane = null;
        crossingPoints = null;
        cornerPoints = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        passageSelected = false;
        inEditMode = true;
    }
    /// <summary>
    /// Traffic light controller uses this to set crosswalks status.
    /// </summary>
    /// <param name="pedestriansGo">Are pedestrians allowed to cross the road.</param>
    /// <param name="carsGo">Are cars allowed to cross the crosswalk?</param>
    /// <param name="typeIsCrosswalk">Is this crosswalk set along a straight road (ie. not a crosswalk).</param>
    public void SetWalkAndDriveStatus(bool pedestriansGo, bool carsGo, bool typeIsCrosswalk)
    {
        canWalk = pedestriansGo;
        //this is for crosswalks on straight roads, no ic to control cars
        if (typeIsCrosswalk)
        {
            canDrive = carsGo;
        }
    }
    /*
    public int[] GetCrossingIndexes(Lane l)
    {
        List<int> indexes = new List<int>();
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] == l)
            {
                indexes.Add(i);
            }
        }
        int[] ind = new int[indexes.Count];
        for (int i = 0; i < indexes.Count; i++)
        {
            ind[i] = indexes[i];
        }
        return ind;
    }*/
    /// <summary>
    /// AI car calls this function when it starts crossing the crosswalk.
    /// </summary>
    public void CarStartsCrossing()
    {
        carCrossingTimer = 2f;
        carIsCrossing = true;
    }
    /// <summary>
    /// Pedestrian calls this function when it starts crossing the road.
    /// </summary>
    public void PedestrianIsCrossing()
    {
        pedestrianTimer = 10f;
        pedestrianIsCrossing = true;
    }
    /// <summary>
    /// Updates crosswalks timers.
    /// </summary>
    private void Update()
    {
        if (carIsCrossing)
        {
            carCrossingTimer -= Time.deltaTime;
            if (carCrossingTimer <= 0f)
            {
                carIsCrossing = false;
            }
        }
        if (pedestrianIsCrossing)
        {
            pedestrianTimer -= Time.deltaTime;
            if (pedestrianTimer <= 0f)
            {
                pedestrianIsCrossing = false;
            }
        }
    }
}
/// <summary>
/// The information of the crosswalks along roads are saved as crosswalk encounters. They contain information of which particular
/// crosswalk is in question and index reference to node before the crosswalk on this particular route. This is the point where
/// car should stop if it can't cross the crosswalk.
/// </summary>
[Serializable]
public class CrosswalkEncounter
{
    /// <summary>
    /// The crosswalk of this crosswalk encounter.
    /// </summary>
    [SerializeField]
    public Crosswalk crosswalk;
    /// <summary>
    /// This index references to crosswalks BeforeNodes-arrays. This determines the point where a car should stop before the
    /// crosswalk.
    /// </summary>
    public int index;
}
