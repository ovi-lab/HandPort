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
    private GoGoDetachAdapterStable3 teleportAdapter;
    
    private StudyConditions studyConditions;
    private TargetConditions targetConditions;
    private ParticipantConditions participantConditions;
    private AdapterConditions adapterConditions;

    protected override void Awake()
    {
        base.Awake();
        nextTarget = currentTarget + 1;
        obstacleManager = FindObjectOfType<ObstacleManager>();
        teleportAdapter = FindObjectOfType<GoGoDetachAdapterStable3>( true);
    }

    public void ApplySettings<T>(T _values)
    {
        Type type = typeof(T);

        if (type == typeof(TargetConditions))
        {
            targetConditions = _values as TargetConditions;
            SetupObstaclesWithTargetConditions();
        }
        else if (type == typeof(StudyConditions))
        {
            studyConditions = _values as StudyConditions;
        }
        else if (type == typeof(ParticipantConditions))
        {
            participantConditions = _values as ParticipantConditions;
        }
        else if (type == typeof(AdapterConditions))
        {
            adapterConditions = _values as AdapterConditions;
            SetupAdapterWithAdapterConditions();
        }
        else
        {
            Debug.LogWarning("Unsupported settings type.");
        }
    }
    
    private void SetupObstaclesWithTargetConditions()
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
        Debug.Log("Setting up obstacles with target conditions");
        
        targets = obstacleManager.SetObstacleParameters(targetConditions.targetDistances, targetConditions.targetSizes, targetConditions.targetCount);
        InitialiseTargets();
    }
    private void SetupAdapterWithAdapterConditions()
    {
        if (adapterConditions == null)
        {
            Debug.LogWarning("AdapterConditions not set.");
            return;
        }
        if (teleportAdapter == null)
        {
            Debug.LogWarning("TeleportAdapter not found in scene.");
            return;
        }
        Debug.Log("Setting up adapter with adapter conditions");
        teleportAdapter.SetArmMeasurements(adapterConditions.originShoulderDistance,
            adapterConditions.shoulderEllbowDistance, adapterConditions.ellbowWristDistance);
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

[Serializable]
public class AdapterConditions
{
    public float originShoulderDistance;
    public float ellbowWristDistance;
    public float shoulderEllbowDistance;
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
    
    public static AdapterConditions ToAdapterConditions(DataSnapshot initialSettings)
    {
        AdapterConditions adapterConditions = new AdapterConditions
        {
            originShoulderDistance = Convert.ToSingle(initialSettings.Child("originShoulderDistance").Value.ToString()),
            ellbowWristDistance = Convert.ToSingle(initialSettings.Child("ellbowWristDistance").Value.ToString()),
            shoulderEllbowDistance = Convert.ToSingle(initialSettings.Child("shoulderEllbowDistance").Value.ToString())
        };
        return adapterConditions;
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
        
    
    // before study:
    
    // tbd: if recordData is true log analytics, else no
    // tbd: if reset is true, set participantID to 1

    // tbd: if maxParticipant == participantID set reset to true

}
