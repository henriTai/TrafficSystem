using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio

/// <summary>
/// An interface for traffic controllers.
/// </summary>
public abstract class IICBase : MonoBehaviour
{
    /// <summary>
    /// This function is called when car selection in intersection changes.
    /// </summary>
    public abstract void CarSelectionChanged();
    /// <summary>
    /// Intersection calls this on Update-method for active controller.
    /// </summary>
    public abstract void UpdateActiveController(float dTime);
    /// <summary>
    /// This function is called when car checks in to an intersection.
    /// </summary>
    /// <param name="carData">Car's data component.</param>
    /// <returns>An identification token.</returns>
    public abstract int[] CarCheckIn(CarDriveData carData);
    /// <summary>
    /// This function is called when car checks out from an intersection.
    /// </summary>
    /// <param name="carObject">Car's game object.</param>
    /// <param name="token">Car's identification token.</param>
    public abstract void CarCheckOut(GameObject carObject, int[] token);
    /// <summary>
    /// This function is called when car actually enters an intersection.
    /// </summary>
    /// <param name="token">Car's identification token.</param>
    /// <param name="carObject">Car's gameobject.</param>
    public abstract void CarInsideIntersection(int[] token, GameObject carObject);
    /// <summary>
    /// This function is called when car is abruptly for one reason or another removed from an intersection.
    /// </summary>
    /// <param name="c">Cars on lane group that the car is checked in.</param>
    /// <param name="carObject">Car's gameobject.</param>
    protected void RemoveCarFromIntersection(CarsOnLane c, GameObject carObject)
    {
        //c.carsInside--;
        if (c.carsOnLane[c.closestIndex].driveData.carObject == carObject)
        {
            if (c.carsOnLane[c.closestIndex].driveData.inIntersection)
            {
                c.carsInside--;
            }
            c.carsOnLane.RemoveAt(c.closestIndex);
            if (c.carsOnLane.Count > 0)
            {
                CalculateClosestIndex(c);
            }
            return;
        }
        int index = -1;
        for (int i = 0; i < c.carsOnLane.Count; i++)
        {
            if (c.carsOnLane[i].driveData.carObject == carObject)
            {
                index = i;
                break;
            }
        }
        if (index > -1)
        {
            if (c.carsOnLane[index].driveData.inIntersection)
            {
                c.carsInside--;
            }
            c.carsOnLane.RemoveAt(index);
            if (c.carsOnLane.Count > 0)
            {
                CalculateClosestIndex(c);
            }
        }
    }
    /// <summary>
    /// Updates the closest car in selected group approaching the intersection.
    /// </summary>
    /// <param name="col">Cars on lane group.</param>
    protected void CalculateClosestIndex(CarsOnLane col)
    {
        int index = 0;
        if (col.carsOnLane.Count == 0)
        {
            return;
        }
        Vector2 p0 = new Vector2(col.startNode.transform.position.x, col.startNode.transform.position.z);
        Vector2 p1 = new Vector2(col.carsOnLane[0].driveData.carObject.transform.position.x,
            col.carsOnLane[0].driveData.carObject.transform.position.z);
        float dist = Vector2.Distance(p0, p1);
        for (int i = 1; i < col.carsOnLane.Count; i++)
        {
            CarInIntersection cIn = col.carsOnLane[i];
            if (cIn.inIntersection == true)
            {
                continue;
            }
            p1 = new Vector2(cIn.driveData.carObject.transform.position.x, cIn.driveData.carObject.transform.position.z);
            float dist2 = Vector2.Distance(p0, p1);
            if (dist2 < dist)
            {
                dist = dist2;
                index = i;
            }
        }
        col.closestIndex = index;
    }

}
