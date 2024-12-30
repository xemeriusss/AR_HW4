using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
public class ObjectRayEmitter : MonoBehaviour
{
    [Header("Ray Generation")]
    public int numberOfRays = 5;
    public float straightRayLength = 5f;

    [Header("Black Hole Logic")]
    public Transform blackHole;
    public float angleThreshold = 10f;
    private MeshFilter mf;
    private Vector3[] randomDirections;   

    void Start()
    {
        mf = GetComponent<MeshFilter>();
        GenerateRandomDirections(); // Generate random directions for the rays
    }

    void Update()
    {
        Debug.Log("Casting rays from vertices.");
        CastRaysFromVertices();
    }

    private void GenerateRandomDirections()
    {
        randomDirections = new Vector3[numberOfRays];
        for (int i = 0; i < numberOfRays; i++)
        {
            randomDirections[i] = Random.onUnitSphere; // Random direction on object's surface
        }
    }

    private void CastRaysFromVertices()
    {
        if (mf == null || mf.sharedMesh == null) return;

        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;

        int raysToCast = Mathf.Min(numberOfRays, vertices.Length); // Get number of rays to cast
        HashSet<int> sampledIndices = new HashSet<int>();

        for (int i = 0; i < raysToCast; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, vertices.Length); // Randomly pick a vertex
            } while (!sampledIndices.Add(randomIndex)); // Ensure no duplicate vertices

            Vector3 vertex = vertices[randomIndex]; // Get the vertex position
            Vector3 worldPos = transform.TransformPoint(vertex); // Convert to world space
            Vector3 dir = (worldPos - transform.position).normalized; // Get direction to vertex

            // if (Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.95f)
            // {
            //     continue;
            // }

            TraceRay(worldPos, dir.normalized); // After getting the direction, trace the ray
        }
    }

    private void TraceRay(Vector3 origin, Vector3 direction)
    {

        Ray ray = new Ray(origin, direction);

        // If the ray hits something (like plane), draw a white line up to the hit point
        if (Physics.Raycast(ray, out RaycastHit hit, straightRayLength))
        {
            Debug.DrawLine(origin, hit.point, Color.white);
            return;
        }
        
        // If there's no black hole, just draw a straight white line
        if (blackHole == null)
        {
            Debug.Log("No black hole found. Drawing a straight ray.");
            DrawStraightRay(origin, direction, straightRayLength, Color.white);
            return;
        }

        // If we do have a black hole, measure the angle
        Vector3 toBlackHole = blackHole.position - origin;
        float angle = Vector3.Angle(direction, toBlackHole);

        // If the angle is below the threshold, draw a curved ray
        if (angle < angleThreshold)
        {
            Debug.Log("Angle to black hole is below threshold. Drawing a curved ray.");
            DrawCurvedRay(origin, blackHole.position, Color.red);
        }
        else
        {
            Debug.Log("Angle to black hole is above threshold. Drawing a straight ray.");
            DrawStraightRay(origin, direction, straightRayLength, Color.white);
        }
    }

    private void DrawStraightRay(Vector3 origin, Vector3 direction, float length, Color color)
    {
        Vector3 endPoint = origin + direction.normalized * length; // Calculate the end point by adding direction * length
        Debug.DrawLine(origin, endPoint, color);
    }
    private void DrawCurvedRay(Vector3 start, Vector3 end, Color color)
    {
        Vector3 midpoint = (start + end) * 0.5f + Vector3.down; // Calculate the midpoint between start and end

        int steps = 30;
        float dt = 1f / steps;

        Vector3 prevPoint = start;
        for (int i = 1; i <= steps; i++)
        {
            float t = i * dt;

            // Compute the quadratic curve
            Vector3 currPoint = (1 - t)*(1 - t)*start + 2*(1 - t)*t*midpoint + t*t*end;

            Debug.DrawLine(prevPoint, currPoint, color);
            prevPoint = currPoint;
        }
    }
}
