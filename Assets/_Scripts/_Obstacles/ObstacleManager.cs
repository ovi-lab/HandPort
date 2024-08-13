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

    private List<(int distance, float size)> distanceSizePairs = new List<(int, float)>();

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
        distanceSizePairs = pairs;

        // Set up new obstacles based on the shuffled pairs
        spawnPosition = Vector3.zero; // Reset spawn position
        
        float previousHeight = Terrain.activeTerrain.SampleHeight(spawnPosition);
        
        for (int i = 0; i < pairs.Count; i++)
        {
            var (currentDistance, currentSize) = pairs[i];
            
            // INTERMEDIATE OBSTACLE
            // Determine horizontal offset based on distance category
            int[] offsets = (currentDistance < 5) ? smallDistanceOffsets : largeDistanceOffsets;
            int horizontalOffset = offsets[Random.Range(0, offsets.Length)];

            // Create temporary spawn position with horizontal offset and preliminary Z value
            Vector3 tempSpawnPositionRandom = new Vector3(spawnPosition.x + horizontalOffset, spawnPosition.y, spawnPosition.z + currentDistance);
            float terrainHeightAtTempPositionRandom = Terrain.activeTerrain.SampleHeight(tempSpawnPositionRandom);
            float heightDifferenceRandom = terrainHeightAtTempPositionRandom - previousHeight;

            // Adjust Z value based on height difference
            float horizontalOffsetSquared = Mathf.Pow(horizontalOffset, 2);
            float heightDifferenceSquaredRandom = Mathf.Pow(heightDifferenceRandom, 2);
            float adjustedZRandom = Mathf.Sqrt(Mathf.Pow(currentDistance, 2) - horizontalOffsetSquared - heightDifferenceSquaredRandom);

            // Update spawn position with the final Z value
            spawnPosition.x += horizontalOffset;
            spawnPosition.z += adjustedZRandom;
            spawnPosition.y = terrainHeightAtTempPositionRandom + 0.5f * currentSize;

            // Instantiate random obstacle
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
            obstacle.transform.localScale = new Vector3(currentSize, currentSize, currentSize);
            TeleportationAnchor anchor = obstacle.GetComponent<TeleportationAnchor>();
            obstacles.Add(anchor);
            obstacle.transform.SetParent(teleportationAnchors.transform);

            // Update previousHeight for future calculations
            previousHeight = terrainHeightAtTempPositionRandom;
        }

        return obstacles;
    }

    public List<(int distance, float size)> GetDistanceSizeCombinations(){
        return distanceSizePairs;
    }
}