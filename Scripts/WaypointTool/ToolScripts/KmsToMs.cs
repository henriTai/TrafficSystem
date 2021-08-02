// Author: Henri Tainio
/// <summary>
/// A helper class, returns equivalent m/s value for each (km/h) SpeedLimit enum.
/// </summary>
public static class KmsToMs
{
    static float[] kmsToMs = { 5.555f, 8.333f, 11.111f, 13.888f, 16.667f, 19.444f, 22.222f, 25f, 27.777f, 33.333f };

    /// <summary>
    /// Returns float value (m/s) for SpeedLimits enum.
    /// </summary>
    /// <param name="limit">Speed limit enum</param>
    /// <returns>Float value of the given speed limit (m/s)</returns>
    public static float Convert(SpeedLimits limit)
    {
        return kmsToMs[(int)limit];
    }
}
