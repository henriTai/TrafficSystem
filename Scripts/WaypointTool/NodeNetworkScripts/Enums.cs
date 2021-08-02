//Author: Henri Tainio

/// <summary>
/// Each lane has defined traffic size. This is used in weighting traffic flow and spawning AI cars.
/// </summary>
public enum TrafficSize
{
    High,
    MidHigh,
    Average,
    BelowAverage,
    Low
}
/*
/// <summary>
/// Defines yielding hierarchy in intersections. By default in intersection, each lane is set either as RightOfWay or
/// GiveWay. On staright roads (no intersections) DriverYield is Normal.
/// </summary>
public enum DriverYield
{
    Normal,
    RightOfWay,
    GiveWay
}*/
/// <summary>
/// Lane's turn direction in intersection is either Straight (through), Left or Right.
/// </summary>
public enum IntersectionDirection
{
    Straight,
    Left,
    Right
}
/// <summary>
/// Speed limits used in Finnish traffic rules. Each lane has a speed limit.
/// </summary>
public enum SpeedLimits
{
    KMH_20,
    KMH_30,
    KMH_40,
    KMH_50,
    KMH_60,
    KMH_70,
    KMH_80,
    KMH_90,
    KMH_100,
    KMH_120
}
/// <summary>
/// List of different lane types.
/// </summary>
public enum LaneType
{
    ROAD_LANE,
    INTERSECTION_LANE_YIELDING,
    INTERSECTION_LANE_RIGHT_OF_WAY,
    ACCESS_LANE_YIELDING,
    ROUNDABOUT_CIRCLE,
    ROUNDABOUT_INLANE,
    ROUNDABOUT_OUTLANE
    /* ROAD_LANE = a normal road lane
     * INTERSECTION_LANE_YIELDING = lane in intersection, yielding
     * INTERSECTION_LANE_RIGHT_OF_WAY = lane in intersection, has right of way
     * ACCESS_LANE_YIELDING = access lane is a separate way that connects to a road and its status is always yielding.
     * Access roads differ from intersection lanes, because they don't have intersection controllers. Access lanes are not
     * yet implemented; preliminary idea is that vehicle AI monitors traffic on the lane that access lane connects to decide
     * whether it should brake or drive.
     * ROUNDABOUT_CIRCLE = Roundabout's circle, always right of way. Rounabouts are not implemented yet.
     * ROUNDABOUT_INLANE = Lane entering a roundabout, always yielding.
     * ROUNDABOUT_OUTLANE = Roundabouts outlane, always right of way.
     */
}
/// <summary>
/// Car lane change status.
/// </summary>
public enum LaneChange
{
    NotChanging,
    RequestToMove,
    ReadyToMove,
    ChangingLane,
}
