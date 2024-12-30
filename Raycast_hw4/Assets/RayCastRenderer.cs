using UnityEngine;
using System.IO;

[RequireComponent(typeof(Camera))]
public class RayCastRenderer : MonoBehaviour
{
    [Header("Render Resolution")]
    public int imageWidth = 640;
    public int imageHeight = 480;

    private Texture2D renderTexture;

    [Header("Black Hole Settings")]
    public Transform blackHole;           
    public float blackHolePullStrength = 1.0f;

    [Header("Lighting")]
    public Light[] sceneLights;          

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Create a texture to store the rendered image
        renderTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);

        // Build the image once at Start
        BuildRayCastImage();

        SaveImageToPNG(renderTexture, "test_st1_2.png");
    }

    void BuildRayCastImage()
    {
        for (int y = 0; y < imageHeight; y++)
        {
            for (int x = 0; x < imageWidth; x++)
            {
                // Generate a ray from camera through pixel
                Ray pixelRay = GenerateCameraRay(x, y);

                // Trace the ray and get the color
                Color color = TraceRay(pixelRay);

                // Write the color into the Texture2D
                renderTexture.SetPixel(x, y, color);
            }
        }

        // Apply all pixel changes
        renderTexture.Apply();
    }

    Ray GenerateCameraRay(int px, int py)
    {
        // Convert (x,y) pixel coordinates to u,v coordinates in [0,1]
        float u = (px + 0.5f) / imageWidth; 
        float v = (py + 0.5f) / imageHeight;
        
        float ndcX = 2f * u - 1f;
        float ndcY = 2f * v - 1f;

        float aspect = (float)imageWidth / imageHeight;
        float tanHalfFov = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        // Find the camera direction
        Vector3 cameraDir = new Vector3(
            ndcX * aspect * tanHalfFov,
            ndcY * tanHalfFov,
            1f
        ).normalized;

        // Transform direction to world space
        Vector3 worldOrigin = cam.transform.position;
        Vector3 worldDir = cam.transform.TransformDirection(cameraDir);

        return new Ray(worldOrigin, worldDir);
    }

    Color TraceRay(Ray ray)
    {
        // If a black hole is present, check if the ray is close to it
        if (blackHole != null)
        {
            Vector3 toBH = blackHole.position - ray.origin; // Subtract black hole position from ray origin
            float angle = Vector3.Angle(ray.direction, toBH);

            // If angle is below the threshold, do a curved ray
            if (angle < 10f) 
            {
                return TraceQuadraticPath(ray.origin, blackHole.position);
            }
        }

        // Otherwise, do a normal straight ray
        return TraceStraightRay(ray);
    }

    Color TraceStraightRay(Ray ray)
    {
        // If the ray hits something, compute color
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            return ComputeLambertian(hit);
        }

        // If no hit, return black
        return Color.black;
    }

    Color TraceQuadraticPath(Vector3 origin, Vector3 blackHolePos)
    {
        // Compute the midpoint between origin and black hole
        Vector3 midpoint = (origin + blackHolePos) * 0.5f + Vector3.down * blackHolePullStrength;

        int steps = 50;
        float dt = 1f / steps;

        Vector3 prevPoint = origin;
        for (int i = 1; i <= steps; i++)
        {
            float t = i * dt;
            // Compute the quadratic curve
            Vector3 currPoint = (1 - t)*(1 - t)*origin + 2*(1 - t)*t*midpoint + t*t*blackHolePos;

            // Check intersection on each small segment
            Vector3 segmentDir = currPoint - prevPoint;
            float segDist = segmentDir.magnitude;
            Ray segmentRay = new Ray(prevPoint, segmentDir.normalized);

            if (Physics.Raycast(segmentRay, out RaycastHit hit, segDist))
            {
                // If we hit something, compute color
                return ComputeLambertian(hit);
            }

            prevPoint = currPoint;
        }

        // If we made it to the black hole with no hits, return black
        return Color.black;
    }

    Color ComputeLambertian(RaycastHit hitInfo)
    {
        Color baseColor = Color.gray; 
        Renderer rend = hitInfo.collider.GetComponent<Renderer>();

        // Get the base color of the hit object
        if (rend && rend.material.HasProperty("_Color"))
        {
            baseColor = rend.material.color;
        }

        Vector3 hitPoint = hitInfo.point;
        Vector3 normal = hitInfo.normal;

        Color finalColor = Color.black; // Final color to return

        // For each light in the scene
        foreach (Light lt in sceneLights)
        {
            if (lt == null) continue;

            Vector3 toLight = (lt.transform.position - hitPoint).normalized; // Direction to light
            float distanceToLight = Vector3.Distance(hitPoint, lt.transform.position); // Distance to light

            // Check if the hit point is in shadow of this light
            bool isInShadow = Physics.Raycast(hitPoint + normal * 0.001f, toLight, distanceToLight);
            if (isInShadow)
            {
                continue;
            }

            // Compute the diffuse contribution 
            float ndotl = Mathf.Max(0f, Vector3.Dot(normal, toLight));
            finalColor += baseColor * lt.color * lt.intensity * ndotl;
        }

        return finalColor;
    }

    void SaveImageToPNG(Texture2D tex, string filename)
    {
        // Encode texture to PNG
        byte[] bytes = tex.EncodeToPNG();
        string path = Application.dataPath + "/../" + filename;

        File.WriteAllBytes(path, bytes);
        Debug.Log("Saved RayCast image to: " + path);
    }
}


