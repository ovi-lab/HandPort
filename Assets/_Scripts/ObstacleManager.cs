using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ObstacleManager : MonoBehaviour
{   
    [SerializeField] private int obstacleQuantity;
    [SerializeField] private float obstacleSpawnOffset;
    [SerializeField] private GameObject obstaclePrefab;
    
    
    private Vector3 spawnPosition = new Vector3(0,0.01f,0);
    private List<GameObject> obstacles;

    private void Awake()
    {
        for (int i = 0; i < obstacleQuantity; i++)
        {
            spawnPosition.z += obstacleSpawnOffset;
            GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, transform);
        }
    }

    private void Start()
    {
        GameManager.Instance.InitialiseTargets();
    }

    private void OnDestroy()
    {
        
    }
}