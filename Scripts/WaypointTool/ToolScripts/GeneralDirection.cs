using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Author: Henri Tainio
/// <summary>
/// A helper class to translate general direction enums to vectors in world space.
/// </summary>
public static class GeneralDirection
{
    /// <summary>
    /// Translates a general direction enum to a world space Vector3.
    /// </summary>
    /// <param name="dir">Enum general direction</param>
    /// <returns>General direction translated to a Vector3 in world space.</returns>

    /// <summary>
    /// General directions, used to define approximate start direction in bezier tool.
    /// </summary>
    public enum Directions
    {
        North,
        NorthWest,
        West,
        SouthWest,
        South,
        SouthEast,
        East,
        NorthEast
    }

    public static Vector3 DirectionVector(Directions dir)
    {
        Vector3 d = Vector3.zero;
        switch(dir)
        {
            case Directions.North:
                d = Vector3.forward;
                break;
            case Directions.West:
                d = Vector3.left;
                break;
            case Directions.South:
                d = Vector3.back;
                break;
            case Directions.East:
                d = Vector3.right;
                break;
            case Directions.NorthWest:
                d = new Vector3(-1f, 0f, 1f).normalized;
                break;
            case Directions.SouthWest:
                d = new Vector3(-1f, 0f, -1f).normalized;
                break;
            case Directions.SouthEast:
                d = new Vector3(1f, 0f, -1f).normalized;
                break;
            case Directions.NorthEast:
                d = new Vector3(1f, 0f, 1f).normalized;
                break;
        }
        return d;
    }
    /// <summary>
    /// Reurns a direction vector to the right of given vector3.
    /// </summary>
    /// <param name="direction">A Vector3</param>
    /// <returns>Direction vector to the right in relation to the given vector3</returns>
    public static Vector3  DirectionRight(Vector3 direction)
    {
        Vector3 right = new Vector3(direction.z, direction.y, -direction.x).normalized;
        return right;
    }
    /// <summary>
    /// Returns a direction vector to the left of given vector3
    /// </summary>
    /// <param name="direction">A Vector3</param>
    /// <returns>Direction vector to the left in relation to the given vector3</returns>
    public static Vector3 DirectionLeft(Vector3 direction)
    {
        Vector3 left = new Vector3(-direction.z, direction.y, direction.x).normalized;
        return left;
    }

}
