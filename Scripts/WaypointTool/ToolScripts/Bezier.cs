using UnityEngine;
/// <summary>
/// This script contains commonplace math operations related to bezier calculations. Source: Catlike coding
/// https://catlikecoding.com/unity/tutorials/curves-and-splines/
/// Modified by: Henri Tainio
/// </summary>
public static class Bezier
{
    /// <summary>
    /// Modes of how bezier's control point's handles work in relation with each other.
    /// </summary>
    public enum ControlPointMode
    {
        Free,
        Aligned,
        Mirrored
    }
    /*
    // 3-point version
    public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        //return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 +
            2f * oneMinusT * t * p1 +
            t * t * p2;
    }*/
    /// <summary>
    /// Returns point on bezier curve
    /// </summary>
    /// <param name="p0">First bezier point (curve's start point position)</param>
    /// <param name="p1">Second bezier point (start point's handle position)</param>
    /// <param name="p2">Third bezier point (end point's handle position)</param>
    /// <param name="p3">Fourth bezier point (curve's end point position)</param>
    /// <param name="t">Calculated point on the curve as a fraction between the start point and the end point (0-1)</param>
    /// <returns>Position of the given point on the curve</returns>
    public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * oneMinusT * p0 +
            3f * oneMinusT * oneMinusT * t * p1 +
            3f * oneMinusT * t * t * p2 +
            t * t * t * p3;
    }
    /*
    // lines tangent, can be used in speed calculation (see BezierCurve:GetVelocity)
    public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return 2f * (1f - t) * (p1 - p0) +
            2f * t * (p2 - p1);
    }*/

    //4-points version
    /// <summary>
    /// Returns tangent of the curve at the given point.
    /// </summary>
    /// <param name="p0">First bezier point (curve's start point position)</param>
    /// <param name="p1">Second bezier point (start point's handle position)</param>
    /// <param name="p2">Third bezier point (end point's handle position)</param>
    /// <param name="p3">Fourth bezier point (curve's end point position)</param>
    /// <param name="t">Point on the curve as a fraction between the start point and the end point (0-1)</param>
    /// <returns>Tangent at the given point on the curve</returns>
    public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return 3f * oneMinusT * oneMinusT * (p1 - p0) +
            6f * oneMinusT * t * (p2 - p1) +
            3f * t * t * (p3 - p2); 
    }
}
