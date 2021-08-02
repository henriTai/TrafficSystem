using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Author: Henri Tainio

/// <summary>
/// Data container of all data of the AI car.
/// </summary>
[System.Serializable]
public class CarDriveData : MonoBehaviour
{
    /// <summary>
    /// AI car's gameobject
    /// </summary>
    [Header("Car ID")]
    public GameObject carObject;
    /// <summary>
    /// AI car's control unit component.
    /// </summary>
    public CarControlUnit carControl;

    /// <summary>
    /// Car's intersection yielding status (Is car yielding for an upcoming intersection?).
    /// </summary>
    [Header("Yielding")]
    public bool intersectionYielding = false;
    /// <summary>
    /// Car's crosswalk yielding status (Is car yielding for an upcoming crosswalk?).
    /// </summary>
    public bool crosswalkYielding = false;

    /// <summary>
    /// Is there a steep curve ahead?
    /// </summary>
    [Header("Obstacles")]
    public bool curveAhead = false;
    /// <summary>
    /// Is there an obstacle ahead?
    /// </summary>
    public bool obstacleAhead = false;
    /// <summary>
    /// The distance to the closest obstacle on the previous raycast test.
    /// </summary>
    public float previousHitDistance = float.MaxValue;
    /// <summary>
    /// Current distance to the next crosswalk.
    /// </summary>
    public float distanceToCrosswalk;

    /// <summary>
    /// Reference to the gameobject of the closest obstacle ahead.
    /// </summary>
    public GameObject closestObstacle;
    /// <summary>
    /// Index of which raycast hit the obstacle during the previous raycast test.
    /// </summary>
    public int previousHitIndex = 0;
    /// <summary>
    /// A timer that sweep scanning type raycasts use.
    /// </summary>
    public float sweepTime;
    /// <summary>
    /// A variable controlling sweep scanning type raycast speed.
    /// </summary>
    public float sweepLength = 5f;
    /// <summary>
    /// Sweep scanning raycast moves in back and forth motion. Is scan currently moving forth?
    /// </summary>
    public bool sweepingForth;
    /// <summary>
    /// Maximum anfgle that the sweep scanning raycast rotates.
    /// </summary>
    public float sweepMaxAngle = 60f;

    /// <summary>
    /// Car's current speed.
    /// </summary>
    [Header("Car speed and position")]
    public float carSpeed = 0f;
    /// <summary>
    /// Car's current position in world space.
    /// </summary>
    public Vector3 carPosition = Vector3.zero;
    /// <summary>
    /// Car's current direction vector.
    /// </summary>
    public Vector3 forwardVector = Vector3.zero;

    /// <summary>
    /// Current speed limit that this car is obeying.
    /// </summary>
    [Header("Speed control")]
    public float speedLimit;
    /// <summary>
    /// Current target speed this car is aiming to keep.
    /// </summary>
    public float targetSpeed;
    /// <summary>
    /// Stopping speed multiplier has value between 0 and 1, depending on the distance to the next cause for stopping. The correct
    /// situational speed is calculated inputing this value to the stopping speed (animation) curve.
    /// </summary>
    public float stoppingSpeedMultiplier = 1f;
    /// <summary>
    /// With stopping speed multiplier value, this curve determines the maximum speed at the current situation.
    /// </summary>
    public AnimationCurve stoppingSpeedCurve;
    /// <summary>
    /// Current steer amount in degrees.
    /// </summary>
    [Header("Car steering")]
    public float steerAmount = 0f;
    /// <summary>
    /// Is car oversteering?
    /// </summary>
    public bool overSteering = false;
    /// <summary>
    /// Torque value that will be applied to motor.
    /// </summary>
    [Header("Torque and Braking")]
    public float motor;
    /// <summary>
    /// Force that will be applied to car's brakes.
    /// </summary>
    public float appliedBrakeForce;

    /// <summary>
    /// The previous node this car has passed.
    /// </summary>
    [Header("Nodes")]
    public Nodes previousNode;
    /// <summary>
    /// The next node that this car is heading to.
    /// </summary>
    public Nodes nextNode;
    /// <summary>
    /// The one after the next node this car is heading to.
    /// </summary>
    public Nodes oneAfterNextNode;
    /// <summary>
    /// Vector3 position of the next node this car is heading to.
    /// </summary>
    public Vector3 nextNodePos = Vector3.zero;
    /// <summary>
    /// Vector3 position of the one after the next node this car is heading to.
    /// </summary>
    public Vector3 oneAfterNextNodePos = Vector3.zero;

    /// <summary>
    /// The current lane this car is following.
    /// </summary>
    [Header("Lanes")]
    public Lane currentLane;
    /// <summary>
    /// an array of the next lanes of this car's current route.
    /// </summary>
    public Lane[] nextLanes;
    /// <summary>
    /// An array of the next crosswalk encounters on this car's current route.
    /// </summary>
    [Header("Crosswalk info")]
    public CrosswalkEncounter[] nextCrosswalks;
    /// <summary>
    /// The index of the next crosswalk encounter in nextCrosswalks-array.
    /// </summary>
    public int crosswalkIndex = -1;
    /// <summary>
    /// The next upcoming crosswalk on this car's current route.
    /// </summary>
    public Crosswalk nextCrosswalk;
    /// <summary>
    /// The point where car should stop when yielding to the next crosswalk.
    /// </summary>
    public Vector2 nextCrossingPoint;
    /// <summary>
    /// The intersection this car is currently signed in.
    /// </summary>
    [Header("Intersection status")]
    public Intersection intersection;
    /// <summary>
    /// The lane this car uses while driving through the current intersection.
    /// </summary>
    public Lane intersectionLane;
    /// <summary>
    /// The point where car should stop when yielding to the next intersection.
    /// </summary>
    public Vector2 intersectionStartPos;
    /// <summary>
    /// Current distance to the next intersection.
    /// </summary>
    public float distanceToIntersection;
    /// <summary>
    /// An ID ticket exchanged with intersection's traffic controller.
    /// </summary>
    public int[] parkingTicket = { 0, 0 };
    /// <summary>
    /// Is car checked in to an intersection? When car is closing in to an intersection, it checks in to intersection's
    /// traffic controller.
    /// </summary>
    public bool checkedIn = false;
    /// <summary>
    /// Is car currently within an intersection?
    /// </summary>
    public bool inIntersection = false;

    /// <summary>
    /// A timer used for blinking car's turn signal lights.
    /// </summary>
    [Header("Lights")]
    public float turnSignalTimer = 0f;
    /// <summary>
    /// Car's current turn signal status:
    /// 0 = Off
    /// 1 = Left side on
    /// 2 = Right side on
    /// </summary>
    public int turnSignalsOn = 0; // 0 = Off, 1 = left, 2 = right
    /// <summary>
    /// Is turn signal currently on (when blinking)?
    /// </summary>
    public bool signalIsLit = false;
    /// <summary>
    /// Are car's brake light on?
    /// </summary>
    public bool brakeLightsOn = false;
    /// <summary>
    /// Car's current lane change status.
    /// </summary>
    [Header("Lane changing")]
    public LaneChange laneChange = LaneChange.NotChanging;
    /// <summary>
    /// Is car changing to the left (or right)?
    /// </summary>
    public bool changingLeft;
    /// <summary>
    /// A timer used in lane change operation.
    /// </summary>
    public float laneChangeWait = 0f;
    /// <summary>
    /// The node on the adjacent lane that this car is heading to while changing the lane.
    /// </summary>
    public Nodes laneChangeNode = null;
    /// <summary>
    /// Is this car slowing down to allow another car to change the lane?
    /// </summary>
    public bool slowingDownToAllowLaneChange = false;
    /// <summary>
    /// The car data of the another AI car that has requested this AI car to slow down while changing lane.
    /// </summary>
    public CarDriveData carChangingLane;
    /// <summary>
    /// Stopping distance value. This value is used in all situational speed adjustments (ie. stopping or slowing down
    /// because of an obstacle, a curve or an upcoming intersection or a crosswalk).
    /// </summary>
    [Header("Constants")]
    public const float stoppingDistance = 30f;
    /// <summary>
    /// The number of lanes in next lanes array.
    /// </summary>
    public const int nextLanesCount = 4;
    /// <summary>
    /// The distance to an intersection when car checks in to intersection's traffic controller.
    /// </summary>
    public const float distanceToCheckIn = 50f;
    /// <summary>
    /// The turn signal lights' blink length.
    /// </summary>
    public const float signalLength = 0.6f;
    /// <summary>
    /// The maximum angle that car's steering wheels turn.
    /// </summary>
    public const float maxSteeringAngle = 40f;
    /// <summary>
    /// The maximum value of car's motor torque.
    /// </summary>
    public const float maxMotorTorque = 600f;
    /// <summary>
    /// The maximum speed that car can drive to a steep curve.
    /// </summary>
    public const float curveMaxSpeed = 5.6f;
}
