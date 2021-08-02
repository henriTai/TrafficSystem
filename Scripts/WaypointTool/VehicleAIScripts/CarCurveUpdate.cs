using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// Car's AI uses this class to update its info of upcoming steep curve.
/// </summary>
public static class CarCurveUpdate
{
    /// <summary>
    /// Chacks if car is closing in to a steep curve. At the moment, "steep" is hard coded as 30 degrees. If car's speed is
    /// over a certain speed, it slows down to a save speed.
    /// As a note: In future, it would be better to parametrize the "steep" value as well as the speed limits
    /// according to the physics of the given car type.
    /// </summary>
    /// <param name="data">Car's data that will be updated.</param>
    public static void CheckCurve(CarDriveData data)
    {
        bool isAhead = false;
        float dist = 30f;
        Vector2 p0 = new Vector2(data.carPosition.x, data.carPosition.z);
        Vector3 carDir = new Vector3(data.forwardVector.x, 0f, data.forwardVector.z).normalized;

        Nodes n = data.nextNode;
        p0 = new Vector2(data.nextNodePos.x, data.nextNodePos.z);
        int counter = 0;
        float curve = 0f;
        int nextLaneIndex = 0;
        while (true)
        {
            Nodes secondNode = null;
            if (n.LaneStartNode == true)
            {
                if (nextLaneIndex < CarDriveData.nextLanesCount + 1)
                {
                    if (nextLaneIndex == 0)
                    {
                        secondNode = data.currentLane.nodesOnLane[1];
                    }
                    else
                    {
                        secondNode = data.nextLanes[nextLaneIndex - 1].nodesOnLane[1];
                        nextLaneIndex++;
                    }
                }
                else
                {
                    secondNode = n.StartingLanes[0].nodesOnLane[1];
                }
            }
            else
            {
                secondNode = n.OutNode;
            }
            p0 = new Vector2(n.transform.position.x, n.transform.position.z);
            Vector2 p1 = new Vector2(secondNode.transform.position.x, secondNode.transform.position.z);
            dist -= Vector2.Distance(p0, p1);

            if (dist < 0f)
            {
                break;
            }
            Vector3 dir2 = new Vector3(p1.x - p0.x, 0f, p1.y - p0.y).normalized;
            float c = Vector3.Angle(carDir, dir2);
            if (c > curve)
            {
                curve = c;
            }
            // if angle is over 30 degrees
            if (Vector3.Angle(carDir, dir2) > 30f)
            {
                isAhead = true;
                break;
            }
            n = secondNode;
            p0 = p1;

            counter++;
        }
        data.curveAhead = isAhead;
    }
}
