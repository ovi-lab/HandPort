using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;

public class ObstacleManager : MonoBehaviour
{   
    [SerializeField] private GameObject obstaclePrefab;
    public Transform cameraOffset;
    private Vector3 spawnPosition = new Vector3(0, 0.01f, 0);
    private List<TeleportationAnchor> obstacles = new List<TeleportationAnchor>();

    private float intermedidateObstacleSize = 0.2f;
    private float intermedidateObstacleDistance = 2;
    
    private int[] largeDistanceOffsets = new int[] { -10, -5, -2, 0, 2, 5, 10 };
    private int[] smallDistanceOffsets = new int[] { -1, 0, 1 };

    public List<TeleportationAnchor> SetObstacleParameters(int[] distances, float[] sizes, int count)
    {
        // Clear existing obstacles
        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }
        obstacles.Clear();

        // Create an empty GameObject to hold colliders
        GameObject teleportationAnchors = new GameObject("TeleportationAnchors");
        
         // Create a new list of all possible size-distance pairs, each repeated 'count' times
        List<(int distance, float size)> pairs = new List<(int, float)>();
        foreach (int distance in distances)
        {
            foreach (float size in sizes)
            {
                for (int i = 0; i < count; i++)
                {
                    pairs.Add((distance, size));
                }
            }
        }

        // Shuffle the pairs list
        System.Random rand = new System.Random();
        pairs = pairs.OrderBy(x => rand.Next()).ToList();

        // Set up new obstacles based on the shuffled pairs
        spawnPosition = Vector3.zero; // Reset spawn position
        
        float previousHeight = Terrain.activeTerrain.SampleHeight(spawnPosition);
        
        for (int i = 0; i < pairs.Count; i++)
        {
            var (currentDistance, currentSize) = pairs[i];
            
            // INTERMEDIATE OBSTACLE
            float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPosition);
            float heightDifference = terrainHeight - previousHeight;
            float adjustedZIntermediate = Mathf.Sqrt(Mathf.Pow(intermedidateObstacleDistance, 2) - Mathf.Pow(heightDifference, 2));
            spawnPosition.z += adjustedZIntermediate;
            spawnPosition.x += smallDistanceOffsets[Random.Range(0, smallDistanceOffsets.Length)];
            spawnPosition.y = Terrain.activeTerrain.SampleHeight(spawnPosition) + 0.5f * intermedidateObstacleSize;

            GameObject intermediateObstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
            intermediateObstacle.transform.localScale = new Vector3(intermedidateObstacleSize, intermedidateObstacleSize, intermedidateObstacleSize);
            TeleportationAnchor intermediateAnchor = intermediateObstacle.GetComponent<TeleportationAnchor>();
            obstacles.Add(intermediateAnchor);
            intermediateObstacle.transform.SetParent(teleportationAnchors.transform);
            previousHeight = terrainHeight;
            
            
            // RANDOM OBSTACLE
            int[] offsets = (currentDistance < 5) ? smallDistanceOffsets : largeDistanceOffsets;
            int horizontalOffset = offsets[Random.Range(0, offsets.Length)];
            
            // Place random distance obstacle
            terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPosition);
            heightDifference = terrainHeight - previousHeight;
            float adjustedZ = Mathf.Sqrt(Mathf.Pow(currentDistance, 2) - Mathf.Pow(heightDifference, 2));
            spawnPosition.z += adjustedZ;
            spawnPosition.x += horizontalOffset;
            spawnPosition.y = Terrain.activeTerrain.SampleHeight(spawnPosition) + 0.5f * currentSize;

            GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
            obstacle.transform.localScale = new Vector3(currentSize, currentSize, currentSize);
            TeleportationAnchor anchor = obstacle.GetComponent<TeleportationAnchor>();
            obstacles.Add(anchor);
            obstacle.transform.SetParent(teleportationAnchors.transform);
            previousHeight = terrainHeight;
        }

        return obstacles;
    }
    
}