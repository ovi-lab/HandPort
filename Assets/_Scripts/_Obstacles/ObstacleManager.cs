using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;

public class ObstacleManager : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject intermediateObstaclePrefab;
    [SerializeField] private GameObject intermediateObstacleArrowPrefab;
    [SerializeField] private GameObject obstacleArrowPrefab;
    public Transform cameraOffset;

    private Vector3 spawnPosition = new Vector3(0, 0, 0);
    private List<TeleportationAnchor> obstacles = new List<TeleportationAnchor>();

    private float intermediateObstacleSize = 0.2f;
    private float intermediateObstacleDistance = 2;

    private int[] largeDistanceOffsets = new int[] { -10, -5, -2, 2, 5, 10 };
    private int[] smallDistanceOffsets = new int[] { -1, 1 };

    private List<(int distance, float size)> distanceSizePairs = new List<(int, float)>();

    private LatinSquareManager latinSquareManager = new LatinSquareManager();

    public List<TeleportationAnchor> SetObstacleParameters(int[] distances, float[] sizes, int repetition, int intermediateDistance, float intermediateSize)
    {
        intermediateObstacleSize = intermediateSize;
        intermediateObstacleDistance = intermediateDistance;

        // Clear existing obstacles
        ClearObstacles();

        // Create an empty GameObject to hold colliders
        GameObject teleportationAnchors = new GameObject("TeleportationAnchors");
        distanceSizePairs = latinSquareManager.GenerateAndShuffleCombinations(distances, sizes, repetition);

        // Set up new obstacles based on the shuffled pairs
        spawnPosition = Vector3.zero; // Reset spawn position

        float previousHeight = Terrain.activeTerrain.SampleHeight(spawnPosition);

        foreach (var (currentDistance, currentSize) in distanceSizePairs)
        {
            SpawnIntermediateObstacle(ref previousHeight, teleportationAnchors);
            SpawnRandomObstacle(currentDistance, currentSize, ref previousHeight, teleportationAnchors);
        }

        return obstacles;
    }

    private void ClearObstacles()
    {
        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }
        obstacles.Clear();
    }
    
    private void SpawnIntermediateObstacle(ref float previousHeight, GameObject parent)
    {
        Vector3 tempSpawnPosition = spawnPosition + new Vector3(0, 0, intermediateObstacleDistance);
        float terrainHeight = Terrain.activeTerrain.SampleHeight(tempSpawnPosition);
        float heightDifference = terrainHeight - previousHeight;

        float adjustedZ = Mathf.Sqrt(Mathf.Pow(intermediateObstacleDistance, 2) - Mathf.Pow(heightDifference, 2));
        spawnPosition.z += adjustedZ;
        spawnPosition.y = terrainHeight + 0.5f * 0.2f;

        var intermediateObstacle = Instantiate(intermediateObstaclePrefab, spawnPosition, Quaternion.identity);
        intermediateObstacle.transform.localScale = new Vector3(1,0.2f,1) * intermediateObstacleSize;
        var intermediateAnchor = intermediateObstacle.GetComponent<TeleportationAnchor>();
        var intermediateObstacleArrow = Instantiate(intermediateObstacleArrowPrefab, spawnPosition, Quaternion.identity);
        intermediateObstacleArrow.transform.SetParent(intermediateObstacle.transform);
        
        obstacles.Add(intermediateAnchor);
        intermediateObstacle.transform.SetParent(parent.transform);

        previousHeight = terrainHeight;
    }

    private void SpawnRandomObstacle(int currentDistance, float currentSize, ref float previousHeight, GameObject parent)
    {
        int[] offsets = (currentDistance < 5) ? smallDistanceOffsets : largeDistanceOffsets;
        int horizontalOffset = offsets[Random.Range(0, offsets.Length)];

        Vector3 tempSpawnPosition = spawnPosition + new Vector3(horizontalOffset, 0, currentDistance);
        float terrainHeight = Terrain.activeTerrain.SampleHeight(tempSpawnPosition);
        float heightDifference = terrainHeight - previousHeight;

        float adjustedZ = Mathf.Sqrt(Mathf.Pow(currentDistance, 2) - Mathf.Pow(horizontalOffset, 2) - Mathf.Pow(heightDifference, 2));
        spawnPosition += new Vector3(horizontalOffset, 0, adjustedZ);
        spawnPosition.y =  terrainHeight + 0.5f * 0.2f;

        var randomObstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
        randomObstacle.transform.localScale = new Vector3(1,0.2f,1) * currentSize;
        var randomAnchor = randomObstacle.GetComponent<TeleportationAnchor>();
        var obstacleArrow = Instantiate(obstacleArrowPrefab, spawnPosition, Quaternion.identity);
        obstacleArrow.transform.SetParent(randomObstacle.transform);
        
        obstacles.Add(randomAnchor);
        randomObstacle.transform.SetParent(randomObstacle.transform);

        previousHeight = terrainHeight;
    }
    
    private void LogLatinSquare(List<(int distance, float size)> pairs)
    {
        var logString = "Latin Square:\n";
        foreach (var (distance, size) in pairs)
        {
            logString += $"Distance: {distance}, Size: {size}\n";
        }
        Debug.Log(logString);
    }

    public List<(int distance, float size)> GetDistanceSizeCombinations()
    {
        return distanceSizePairs;
    }
}
