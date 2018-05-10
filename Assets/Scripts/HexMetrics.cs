using System;
using UnityEngine;

public static class HexMetrics
{
    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f;
    public const float solidFactor = 0.75f;

    static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };

    public static Vector3 GetCorner(HexDirection direction, bool IsSecond)
    {
        return corners[(int)direction + Convert.ToInt32(IsSecond)];
    }

    public static Vector3 GetSolidCorner(HexDirection direction, bool IsSecond)
    {
        return corners[(int)direction + Convert.ToInt32(IsSecond)] * solidFactor;
    }

    public static Vector3 GetBridge(HexDirection direction)
    {
        return (GetCorner(direction, false) + GetCorner(direction, true)) * (1f - solidFactor);
    }
}
