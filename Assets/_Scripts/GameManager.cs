using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;
using Unity.XR.CoreUtils;

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
    // private DatabaseReference studySettings;
    private ObstacleManager obstacleManager;
    private GoGoDetachAdapterStable3 teleportAdapter;
    
    // private StudyConditions studyConditions;
    private TargetConditions targetConditions;
    private ParticipantConditions participantConditions;
    private AdapterConditions adapterConditions;
    private CameraManager cameraManager;
    
    private LatinSquareManager latinSquareManager = new LatinSquareManager();
    private SelectActionCounter selectActionCounter;
    private List<(int, int, int)> shuffledCombinations;
    private int currentLineIndex = 0;
    public XROrigin xrOrigin;

    private List<float> taskCompletionTimes = new List<float>();
    private float startTime;
    private List<int> numberOfAttempts = new List<int>();

    protected override void Awake()
    {
        base.Awake();
        nextTarget = currentTarget + 1;
        obstacleManager = FindObjectOfType<ObstacleManager>();
        teleportAdapter = FindObjectOfType<GoGoDetachAdapterStable3>( true);
        cameraManager =FindObjectOfType<CameraManager>();
        selectActionCounter =FindObjectOfType<SelectActionCounter>();

        ApplyRandomizedConditions();
    }
    
    private void ApplyRandomizedConditions()
    {
        int[] cameraTypes = Enum.GetValues(typeof(CameraType)).Cast<int>().ToArray();
        int[] panelAnchors = Enum.GetValues(typeof(CameraAnchor)).Cast<int>().ToArray();
        int[] mappingFunction = Enum.GetValues(typeof(GoGoAlgorithm)).Cast<int>().ToArray();
        
        shuffledCombinations = latinSquareManager.GenerateAndApplyLatinSquare(cameraTypes, panelAnchors, mappingFunction);
        
        ApplySettingsFromLine(shuffledCombinations[0]);
        currentLineIndex = 1; // Move to the next line for future calls
    }
    
    private void ApplySettingsFromLine((int, int, int) combination)
    {
        if (cameraManager != null)
        {
            cameraManager.cameraDisplayType = (CameraType)combination.Item1;
            cameraManager.anchor = (CameraAnchor)combination.Item2;
            teleportAdapter.goGoAlgorithm = (GoGoAlgorithm)combination.Item3;

            Debug.Log($"Applying Combination: CameraType={combination.Item1}, PanelAnchor={combination.Item2}, MappingFunction={combination.Item3}");
        }
        else
        {
            Debug.LogError("CameraManager not found in the scene.");
        }
    }
    
    public void ApplySettings<T>(T _values)
    {
        Type type = typeof(T);

        if (type == typeof(TargetConditions))
        {
            targetConditions = _values as TargetConditions;
            SetupObstaclesWithTargetConditions();
        }
        // else if (type == typeof(StudyConditions))
        // {
        //     studyConditions = _values as StudyConditions;
        // }
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
       //Debug.Log("Setting up obstacles with target conditions");
        
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
        //Debug.Log("Setting up adapter with adapter conditions");
        teleportAdapter.SetInitialAdapterValues(adapterConditions.originShoulderDistance,
            adapterConditions.shoulderEllbowDistance, adapterConditions.ellbowWristDistance, adapterConditions.maxVirtDistance);
    }
    public void InitialiseTargets()
    {
        foreach (TeleportationAnchor target in targets)
        {
            target.selectExited.AddListener(EnableNextTarget);
            target.gameObject.SetActive(false);
        }
        targets[currentTarget].gameObject.SetActive(true);
    }

    private void EnableNextTarget(SelectExitEventArgs arg0)
    {
        if (currentTarget == targets.Count-1)
        {
            if (currentTarget == targets.Count - 1)
            {
                if (currentTarget % 2 == 1) 
                {
                    float endTime = Time.time;
                    float completionTime = endTime - startTime; // Calculate the time taken between targets
                    taskCompletionTimes.Add(completionTime); // Store the final completion time
                    int attempt = selectActionCounter.GetSelectActionCount();
                    numberOfAttempts.Add(attempt);
                    Debug.Log($"Time to complete task {currentTarget - 1} to {currentTarget}: {completionTime} seconds, number of attempts: {attempt}");
                }

                Debug.Log("Last target reached, resetting scene.");
                LogData();
                ResetTargetsAndXROrigin();
                return;
            }
        }
        if(nextTarget < targets.Count)
        {
            // STORE TASK COMPLETION TIME
            if (currentTarget % 2 == 0) 
            {
                startTime = Time.time;
            }
            else if (currentTarget % 2 == 1) 
            {
                float endTime = Time.time;
                float completionTime = endTime - startTime; // Calculate the time taken between targets
                taskCompletionTimes.Add(completionTime); // Store the completion time
                int attempt = selectActionCounter.GetSelectActionCount();
                numberOfAttempts.Add(attempt);
                Debug.Log($"Time to complete task {currentTarget - 1} to {currentTarget}: {completionTime} seconds, number of attempts: {attempt}");
                startTime = 0;
            }

            // ENABLE NEXT TARGET
            targets[currentTarget].gameObject.SetActive(false);
            targets[nextTarget].gameObject.SetActive(true);
            currentTarget++;
            nextTarget = currentTarget + 1;
        }
    }
    private void ResetTargetsAndXROrigin()
    {
        ApplySettingsFromLine(shuffledCombinations[currentLineIndex]);
        currentLineIndex++;
        
        float terrainHeight = Terrain.activeTerrain.SampleHeight(Vector3.zero);
        Vector3 newPosition = new Vector3(0, terrainHeight+1.5F, -0.01f);
        xrOrigin.MoveCameraToWorldLocation(newPosition);
        xrOrigin.MatchOriginUpCameraForward(Vector3.up, Vector3.forward);
        
        currentTarget = 0;
        nextTarget = 1;
        SetupObstaclesWithTargetConditions();
    }

    private void LogData()
    {
        if (!participantConditions.recordData) return;

        // Create a log entry with the required data
        string logEntry = $"Participant ID: {participantConditions.participantID}, " +
                          $"Latin Square Combination: {shuffledCombinations[currentLineIndex]}, " +
                          $"Distance Size Combination: {obstacleManager.GetDistanceSizeCombinations()}, " +
                          $"Task Completion Times: {string.Join(", ", taskCompletionTimes)}, " +
                          $"Number of Attempts: {string.Join(", ", numberOfAttempts)}"; // -> tbd

        FirebaseManager.LogData(logEntry);

        // Clear the list for the next block
        taskCompletionTimes.Clear();
        numberOfAttempts.Clear();
    }
}

// [Serializable]
// public class StudyConditions
// {
//     public string[] cameraTypes;
//     public string[] panelAnchors;
//     public string[] handVisualisations;
// }

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
    public float maxVirtDistance;
}

public static class FirebaseDataToPrimitives
{
    // public static StudyConditions ToStudyConditions(DataSnapshot initialSettings)
    // {
    //     StudyConditions studyConditions = new StudyConditions
    //     {
    //         cameraTypes = ParseArray<string>(initialSettings.Child("cameraType")),
    //         panelAnchors = ParseArray<string>(initialSettings.Child("panelAnchor")),
    //         handVisualisations = ParseArray<string>(initialSettings.Child("handVisualisation"))
    //     };
    //     return studyConditions;
    // }
    
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
            shoulderEllbowDistance = Convert.ToSingle(initialSettings.Child("shoulderEllbowDistance").Value.ToString()),
            maxVirtDistance = Convert.ToSingle(initialSettings.Child("maxVirtDistance").Value.ToString())
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

}
