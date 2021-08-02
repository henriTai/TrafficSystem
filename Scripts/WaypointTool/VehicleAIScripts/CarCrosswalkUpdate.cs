using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Car's AI uses this class to update its info of upcoming crosswalks.
/// </summary>
public static class CarCrosswalkUpdate
{
    /// <summary>
    /// Updates data of upcoming crosswalks.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void UpdateCrosswalks(CarDriveData data)
    {
        List<CrosswalkEncounter> encs = new List<CrosswalkEncounter>();
        CrosswalkEncounter[] es = data.currentLane.crosswalkEncounters;
        CrosswalkEncounter[] nextEs = data.nextLanes[0].crosswalkEncounters;
        CrosswalkEncounter[] afterNextEs = data.nextLanes[1].crosswalkEncounters;
        int arrayLength = 0;
        if (es != null)
        {
            arrayLength += es.Length;
        }
        if (nextEs != null)
        {
            arrayLength += nextEs.Length;
        }
        if (afterNextEs != null)
        {
            arrayLength += afterNextEs.Length;
        }
        if (arrayLength == 0)
        {
            data.nextCrosswalks = null;
            data.crosswalkIndex = -1;
        }
        else
        {
            int index = 0;
            data.crosswalkIndex = 0;
            data.nextCrosswalks = new CrosswalkEncounter[arrayLength];
            bool checkIfPassed = true;
            int checkFrom = 0;
            int startIndex = 0;
            if (es != null)
            {
                for (int i = 0; i < es.Length; i++)
                {
                    if (checkIfPassed)
                    {
                        Nodes n = es[i].crosswalk.beforeNodes[es[i].index];
                        for (int j = checkFrom; j < data.currentLane.nodesOnLane.Length; j++)
                        {
                            if (data.currentLane.nodesOnLane[j] == data.previousNode)
                            {
                                checkIfPassed = false;
                                break;
                            }
                            if (data.currentLane.nodesOnLane[j] == n)
                            {
                                checkFrom = j + 1;
                                startIndex = i + 1;
                                break;
                            }

                        }
                    }
                    data.nextCrosswalks[index] = es[i];
                    index++;
                }
            }
            if (nextEs != null)
            {
                for (int i = 0; i < nextEs.Length; i++)
                {
                    data.nextCrosswalks[index] = nextEs[index];
                    index++;
                }
            }
            if (afterNextEs != null)
            {
                for (int i = 0; i < afterNextEs.Length; i++)
                {
                    data.nextCrosswalks[index] = afterNextEs[i];
                    index++;
                }
            }
            if (startIndex > index - 1)
            {
                data.crosswalkIndex = -1;
                data.nextCrosswalk = null;
            }
            else
            {
                data.crosswalkIndex = startIndex;
                data.nextCrosswalk = data.nextCrosswalks[data.crosswalkIndex].crosswalk;
                data.nextCrossingPoint = data.nextCrosswalk.crossingPoints[data.nextCrosswalks[data.crosswalkIndex].index];
            }
        }
    }
    /// <summary>
    /// Monitors upcoming crosswalks. If car passes a crosswalk, updates the index of the next crosswalk. If car is closing in
    /// to a crosswalk that is occupied by an pedestrian or there is a red light on for cars, sets car's crosswalk yielding status
    /// as true.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void CheckCrosswalks(CarDriveData data)
    {
        if (data.nextCrosswalk == null)
        {
            return;
        }
        data.distanceToCrosswalk = Vector2.Distance(new Vector2(data.carPosition.x, data.carPosition.z),
            data.nextCrossingPoint) - 2;
        if (data.distanceToCrosswalk > CarDriveData.stoppingDistance)
        {
            if (data.crosswalkYielding)
            {
                data.crosswalkYielding = false;
            }
            return;
        }
        // update status on short distance
        if (data.nextCrosswalk.canDrive == true && data.nextCrosswalk.pedestrianIsCrossing == false)
        {
            data.crosswalkYielding = false;
            if (data.distanceToCrosswalk < -1f)
            {
                data.nextCrosswalk.CarStartsCrossing();
                if (data.crosswalkIndex == data.nextCrosswalks.Length - 1)
                {
                    data.nextCrosswalk = null;
                    data.crosswalkIndex = -1;
                }
                else
                {
                    data.crosswalkIndex++;
                    data.nextCrosswalk = data.nextCrosswalks[data.crosswalkIndex].crosswalk;
                    data.nextCrossingPoint = data.nextCrosswalk.crossingPoints[data.nextCrosswalks[data.crosswalkIndex].index];
                    data.distanceToCrosswalk = Vector2.Distance(new Vector2(data.carPosition.x, data.carPosition.z),
                        data.nextCrossingPoint) - 2f;
                }
            }
        }
        else
        {
            data.crosswalkYielding = true;
        }
    }
}
