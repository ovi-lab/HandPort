using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
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
    private ParticipantConditions participantConditions;

    protected override void Awake()
    {
        base.Awake();
        nextTarget = currentTarget + 1;
        obstacleManager = FindObjectOfType<ObstacleManager>();
    }

    public void ApplySettings<T>(T _values)
    {
        Type type = typeof(T);

        if (type == typeof(TargetConditions))
        {
            targetConditions = _values as TargetConditions;
            SetupSceneWithTargetConditions();
        }
        else if (type == typeof(StudyConditions))
        {
            studyConditions = _values as StudyConditions;
        }
        else if (type == typeof(ParticipantConditions))
        {
            participantConditions = _values as ParticipantConditions;
        }
        else
        {
            Debug.LogWarning("Unsupported settings type.");
        }
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
        targets = obstacleManager.SetObstacleParameters(targetConditions.targetDistances, targetConditions.targetSizes, targetConditions.targetCount);
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
    public string[] cameraTypes;
    public string[] panelAnchors;
    public string[] handVisualisations;
}

[Serializable]
public class TargetConditions
{
    public int[] targetDistances;
    public float[] targetSizes;
    public int targetCount;
}

[Serializable]
public class ParticipantConditions
{
    public int participantID;
    public int maxParticipant;
    public bool reset;
    public bool recordData;
}

public static class FirebaseDataToPrimitives
{
    public static StudyConditions ToStudyConditions(DataSnapshot initialSettings)
    {
        StudyConditions studyConditions = new StudyConditions
        {
            cameraTypes = ParseArray<string>(initialSettings.Child("cameraType")),
            panelAnchors = ParseArray<string>(initialSettings.Child("panelAnchor")),
            handVisualisations = ParseArray<string>(initialSettings.Child("handVisualisation"))
        };
        return studyConditions;
    }
    
    public static TargetConditions ToTargetConditions(DataSnapshot initialSettings)
    {
        TargetConditions targetConditions = new TargetConditions
        {
            targetDistances = ParseArray<int>(initialSettings.Child("targetDistance")),
            targetSizes = ParseArray<float>(initialSettings.Child("targetSize")),
            targetCount = Convert.ToInt32(initialSettings.Child("targetCount").Value.ToString())
        };
        return targetConditions;
    }
    
    public static ParticipantConditions ToParticipantConditions(DataSnapshot initialSettings)
    {
        ParticipantConditions participantConditions = new ParticipantConditions
        {
            participantID = Convert.ToInt32(initialSettings.Child("participantID").Value.ToString()),
            maxParticipant = Convert.ToInt32(initialSettings.Child("maxParticipant").Value.ToString()),
            reset = Convert.ToBoolean(initialSettings.Child("reset").Value.ToString()),
            recordData = Convert.ToBoolean(initialSettings.Child("recordData").Value.ToString())
        };
        return participantConditions;
    }
    private static T[] ParseArray<T>(DataSnapshot settingsSnapshot)
    {
        List<T> settingsList = new List<T>();
        foreach (DataSnapshot childSnapshot in settingsSnapshot.Children)
        {
            settingsList.Add((T)Convert.ChangeType(childSnapshot.Value.ToString(), typeof(T)));
        }
        return settingsList.ToArray();
    }
        
    
    // tbd: if recordData is true log analytics, else no
    // tbd: if reset is true, set participantID to 0

}
