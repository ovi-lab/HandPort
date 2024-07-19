using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Reflection;

public class ObstacleManager : MonoBehaviour
{   
    [SerializeField] private GameObject obstaclePrefab;
    public Transform cameraOffset;
    private Vector3 spawnPosition = new Vector3(0, 0.01f, 0);
    private List<TeleportationAnchor> obstacles = new List<TeleportationAnchor>();

    private void Awake()
    {
        //offset = GameObject.Find("Camera Offset").transform.position.y;
    }

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

        // Generate all possible (size, distance) pairs
        List<(int distance, float size)> pairs = new List<(int distance, float size)>();
        for (int i = 0; i < sizes.Length; i++)
        {
            for (int j = 0; j < distances.Length; j++)
            {
                pairs.Add((distances[j], sizes[i]));
            }
        }

        // Shuffle the pairs list
        Shuffle(pairs);

        // Set up new obstacles based on provided parameters
        spawnPosition = Vector3.zero; // Reset spawn position

        int totalPairs = pairs.Count;
        int obstaclesPerPair = count / totalPairs;
        int remainingObstacles = count % totalPairs;

        foreach (var pair in pairs)
        {
            int currentObstacleCount = obstaclesPerPair;

            if (remainingObstacles > 0)
            {
                currentObstacleCount++;
                remainingObstacles--;
            }

            for (int k = 0; k < currentObstacleCount; k++)
            {
                spawnPosition.z += pair.distance;

                // Calculate terrain height at spawn position
                float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPosition);

                // Adjust spawn position based on terrain height
                spawnPosition.y = terrainHeight + 0.5f * pair.size; // Adjust 0.5f to fit your terrain's scale and cube's size

                // Instantiate obstacle
                GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
                obstacle.transform.localScale = new Vector3(pair.size, pair.size, pair.size);
                TeleportationAnchor anchor = obstacle.GetComponent<TeleportationAnchor>();
                obstacles.Add(anchor);

                // Make obstacle a child of the colliders parent
                obstacle.transform.SetParent(teleportationAnchors.transform);
            }
        }
        return obstacles;
    }
    
    private void Shuffle<T>(IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}