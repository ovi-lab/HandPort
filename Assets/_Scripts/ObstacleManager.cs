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
    private List<GameObject> obstacles = new List<GameObject>();

    private void Awake()
    {
        //offset = GameObject.Find("Camera Offset").transform.position.y;
    }

    public void SetObstacleParameters(float distance, int size, int count)
    {
        // Clear existing obstacles
        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle);
        }
        obstacles.Clear();
        
        // Create an empty GameObject to hold colliders
        GameObject runtimeTeleportationArea = new GameObject("RuntimeTeleportationArea");

        // Set up new obstacles based on provided parameters
        spawnPosition = Vector3.zero; // Reset spawn position

        for (int i = 0; i < count; i++)
        {
            spawnPosition.z += distance;

            // Calculate terrain height at spawn position
            float terrainHeight = Terrain.activeTerrain.SampleHeight(spawnPosition);

            // Adjust spawn position based on terrain height
            spawnPosition.y = terrainHeight + 0.5f*size; // Adjust 0.5f to fit your terrain's scale and cube's size
            
            // Instantiate obstacle
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
            obstacle.transform.localScale = new Vector3(size, size, size);
            obstacles.Add(obstacle);
            
            // Make obstacle a child of the colliders parent
            obstacle.transform.SetParent(runtimeTeleportationArea.transform);
        }
        TeleportationArea teleportationArea = runtimeTeleportationArea.AddComponent<TeleportationArea>();
        teleportationArea.interactionLayers = InteractionLayerMask.GetMask("Teleport");

    }
}