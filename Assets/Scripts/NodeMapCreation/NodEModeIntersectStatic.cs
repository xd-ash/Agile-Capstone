using System.Collections.Generic;
using UnityEngine;

public static class LineIntersection
{
    // Returns true if the segments intersect, false otherwise.
    // The intersection point is returned in the 'out' parameter.
    public static bool FindIntersection(Vector2 p1_start, Vector2 p1_end, Vector2 p2_start, Vector2 p2_end, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.zero;

        // Direction vectors of the lines
        Vector2 direction1 = p1_end - p1_start;
        Vector2 direction2 = p2_end - p2_start;

        float denominator = (direction2.y * direction1.x) - (direction2.x * direction1.y);

        // If the denominator is zero, the lines are parallel (or collinear)
        if (Mathf.Abs(denominator) < 0.0001f)
        {
            return false;
        }

        float u_a = ((direction2.x * (p1_start.y - p2_start.y)) - (direction2.y * (p1_start.x - p2_start.x))) / denominator;
        float u_b = ((direction1.x * (p1_start.y - p2_start.y)) - (direction1.y * (p1_start.x - p2_start.x))) / denominator;

        // Check if the intersection point is within the bounds of both line segments (0 to 1)
        if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
        {
            // Calculate the intersection point
            intersectionPoint = p1_start + u_a * direction1;
            return true;
        }

        // Otherwise, the lines do not intersect within their segments
        return false;
    }
    public static void PlaceIntersectionMarkers()
    {
        List<Vector2> positions = new() { new(1,1), new(-1,-1),new(1,-1),new(-1,1) };
        Vector2 intersectPoint = new(5,5);

        if (FindIntersection(positions[0], positions[3], positions[2], positions[1], out intersectPoint))
            Debug.Log($"Intersect exists @ ({intersectPoint.x}, {intersectPoint.y})");
    }
}
