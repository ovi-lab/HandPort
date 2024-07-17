using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    private DatabaseReference studySettings;
    private ObstacleManager obstacleManager;
    private StudyConditions studyConditions;
    private TargetConditions targetConditions;

    protected override void Awake()
    {
        base.Awake();
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
        obstacleManager.SetObstacleParameters(targetConditions.targetDistance, targetConditions.targetSize, targetConditions.targetCount);
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
