using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class ObstacleManager : MonoBehaviour
{   
    [SerializeField] private GameObject obstaclePrefab;

    private Vector3 spawnPosition = new Vector3(0, 0.01f, 0);
    private List<GameObject> obstacles = new List<GameObject>();

    private void Awake()
    {
        // Initial obstacle setup can be done here or dynamically based on target conditions
    }

    public void SetObstacleParameters(float distance, int size, int count)
    {
        // Clear existing obstacles
        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle);
        }
        obstacles.Clear();

        // Set up new obstacles based on provided parameters
        spawnPosition = Vector3.zero; // Reset spawn position

        for (int i = 0; i < count; i++)
        {
            spawnPosition.z += distance;

            // Calculate terrain height at spawn position
            float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPosition);

            // Adjust spawn position based on terrain height
            spawnPosition.y = terrainHeight + 0.5f; // Adjust 0.5f to fit your terrain's scale and cube's size

            // Instantiate obstacle
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, transform);
            obstacle.transform.localScale = new Vector3(size, size, size);
            obstacles.Add(obstacle);
            
            TeleportationArea teleportArea = FindObjectOfType<TeleportationArea>();
            if (teleportArea != null)
            {
                Collider obstacleCollider = obstacle.GetComponent<Collider>();
                if (obstacleCollider != null)
                {
                    teleportArea.colliders.Add(obstacleCollider);
                }
                else
                {
                    Debug.LogWarning("Obstacle is missing a Collider component.");
                }
                obstacle.transform.SetParent(teleportArea.transform);
            }
            else
            {
                Debug.LogWarning("TeleportationArea not found in the scene.");
            }
        }
    }
}