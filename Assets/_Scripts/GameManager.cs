using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public List<TeleportationAnchor> targets;
    public GameObject CurrentTarget
    {
        get
        {
            if (targets != null && targets.Count > 0 && currentTarget >= 0 && currentTarget < targets.Count)
            {
                return targets[currentTarget].gameObject;
            }
            else
            {
                return null;
            }
        }
    }

    private int currentTarget = 0, nextTarget;
    private DatabaseReference studySettings;
    private ObstacleManager obstacleManager;
    private StudyConditions studyConditions;
    private TargetConditions targetConditions;

    protected override void Awake()
    {
        base.Awake();
        nextTarget = currentTarget + 1;
        obstacleManager = FindObjectOfType<ObstacleManager>();
    }

    public void ApplyTargetValues(TargetConditions _values)
    {
        targetConditions = _values;
        SetupSceneWithTargetConditions();
    }

    public void ApplySettings(StudyConditions _settings)
    {
        studyConditions = _settings;
    }
    
    private void SetupSceneWithTargetConditions()
    {
        if (targetConditions == null)
        {
            Debug.LogWarning("TargetConditions not set.");
            return;
        }
        if (obstacleManager == null)
        {
            Debug.LogWarning("ObstacleManager not found in scene.");
            return;
        }
        Debug.Log("Setting up scene with target conditions");

        // Example: Adjusting obstacle properties based on target conditions
        targets = obstacleManager.SetObstacleParameters(targetConditions.targetDistance, targetConditions.targetSize, targetConditions.targetCount);
        InitialiseTargets();
        
    }
    public void InitialiseTargets()
    {
        Debug.Log("Initialisation time");
        foreach (TeleportationAnchor target in targets)
        {
            target.selectExited.AddListener(EnableNextTarget);
            target.gameObject.SetActive(false);
        }
        targets[currentTarget].gameObject.SetActive(true);
    }

    private void EnableNextTarget(SelectExitEventArgs arg0)
    {
        if(nextTarget == targets.Count) return;
        targets[currentTarget].gameObject.SetActive(false);
        targets[nextTarget].gameObject.SetActive(true);
        currentTarget++;
        nextTarget = currentTarget + 1;
    }

}

[Serializable]
public class StudyConditions
{
    public int cameraType;
    public int handVisualisation;
    public bool restart;
    public bool recordData;
}

[Serializable]
public class TargetConditions
{
    public int targetDistance;
    public int targetSize;
    public int targetCount;
}

public static class FirebaseDataToPrimitives
{
    public static StudyConditions ToStudyConditions(DataSnapshot initialSettings)
    {
        StudyConditions studyConditions = new StudyConditions
        {
            cameraType = Convert.ToInt32(initialSettings.Child("cameraType").Value.ToString()),
            handVisualisation = Convert.ToInt32(initialSettings.Child("handVisualisation").Value.ToString()),
            restart = Convert.ToBoolean(initialSettings.Child("restart").Value.ToString()),
            recordData = Convert.ToBoolean(initialSettings.Child("recordData").Value.ToString()),
        };
        return studyConditions;

    }

    public static TargetConditions ToTargetConditions(DataSnapshot IVs)
    {
        TargetConditions targetConditions = new TargetConditions
        {
            targetDistance = Convert.ToInt32(IVs.Child("targetDistance").Value.ToString()),
            targetSize = Convert.ToInt32(IVs.Child("targetSize").Value.ToString()),
            targetCount = Convert.ToInt32(IVs.Child("targetCount").Value.ToString()),
        };
        return targetConditions;
    }
}
