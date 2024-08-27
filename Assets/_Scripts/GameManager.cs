using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;

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
    private List<List<(int, int, int)>> combinations;
    private List<List<int>> mappingCombinations;
    private int currentRowIndex = 0;
    public XROrigin xrOrigin;

    private List<float> taskCompletionTimes = new List<float>();
    private float startTime;
    private List<int> numberOfAttempts = new List<int>();

    public int GetCurrentTarget => currentTarget;
    public int GetTargetCount => targets.Count;
    
    private float trialDuration = 15f; // Set this to the desired duration for each trial
    private Coroutine trialTimerCoroutine;
    public TeleportationProvider teleportationProvider;

    protected override void Awake()
    {
        base.Awake();
        nextTarget = currentTarget + 1;
        obstacleManager = FindObjectOfType<ObstacleManager>();
        teleportAdapter = FindObjectOfType<GoGoDetachAdapterStable3>( true);
        cameraManager =FindObjectOfType<CameraManager>();
        selectActionCounter =FindObjectOfType<SelectActionCounter>();
        teleportationProvider = FindObjectOfType<TeleportationProvider>();
        
        int[] cameraTypes = Enum.GetValues(typeof(CameraType)).Cast<int>().ToArray();
        int[] panelAnchors = Enum.GetValues(typeof(CameraAnchor)).Cast<int>().ToArray();
        int[] mappingFunctions = Enum.GetValues(typeof(GoGoAlgorithm)).Cast<int>().ToArray();
        
        combinations = latinSquareManager.GenerateCombinations(cameraTypes, panelAnchors, mappingFunctions);
        // mappingCombinations = latinSquareManager.GenerateMappingCombinations(mappingFunctions);
    }
    
    
    private void ApplySettingsFromLine((int, int, int) combination)
    {
        if (cameraManager != null)
        {
            cameraManager.cameraDisplayType = (CameraType)combination.Item1;
            cameraManager.anchor = (CameraAnchor)combination.Item2;
            teleportAdapter.goGoAlgorithm = (GoGoAlgorithm)combination.Item3;

            cameraManager.UpdateCameraTypeAndAnchor();
            
            Debug.Log($"Applying Combination: CameraType={combination.Item1}, PanelAnchor={combination.Item2}, MappingFunction={combination.Item3}");
            FindObjectOfType<DisplayVariantText>().DisplayVariant(combination);
        }
        else
        {
            Debug.LogError("CameraManager not found in the scene.");
        }
    }
    
    private void ApplySettingsFromLine(int combination)
    {
        if (combination > -1)
        {
            teleportAdapter.goGoAlgorithm = (GoGoAlgorithm)combination;
            
            Debug.Log($"Applying Algorithm={combination}");
            FindObjectOfType<DisplayVariantText>().DisplayVariant(combination);
        }
        else
        {
            Debug.Log("BASELINE");
        }
    }
    
    public void ApplySettings<T>(T _values)
    {
        Type type = typeof(T);

        if (type == typeof(TargetConditions))
        {
            targetConditions = _values as TargetConditions;
            if (SceneManager.GetActiveScene().name != "Test")
            {
                SetupObstaclesWithTargetConditions();
            }
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
            if (SceneManager.GetActiveScene().name == "ART")
            {
                ApplySettingsFromLine(combinations[participantConditions.participantID% 6 - 1][currentRowIndex]);  
            } else if (SceneManager.GetActiveScene().name == "ART2")
            {
                // ApplySettingsFromLine(mappingCombinations[participantConditions.participantID - 1][currentRowIndex]); 
            }
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
        
        targets = obstacleManager.SetObstacleParameters(targetConditions.targetDistances, targetConditions.targetSizes, targetConditions.repetition, targetConditions.intermedidateObstacleDistance, targetConditions.intermedidateObstacleSize);
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

        // Start the trial timer when the first target is enabled
        StartTrialTimer();
    }

    public void EnableNextTarget(SelectExitEventArgs arg0)
    {
        if (trialTimerCoroutine != null)
        {
            StopCoroutine(trialTimerCoroutine);
        }

        if (currentTarget == targets.Count - 1)
        {
            if (currentTarget % 2 == 1)
            {
                float endTime = Time.time;
                float completionTime = endTime - startTime;
                if (completionTime > trialDuration)
                {
                    completionTime = -1;
                }
                taskCompletionTimes.Add(completionTime);
                int attempt = selectActionCounter.GetSelectActionCount();
                numberOfAttempts.Add(attempt);
                Debug.Log($"Time to complete task {currentTarget-1} to {currentTarget}: {completionTime} seconds, number of attempts: {attempt}");
            }

            LogData();
            ResetTargetsAndXROrigin();
            return;
        }

        if (nextTarget < targets.Count)
        {
            if (currentTarget % 2 == 0)
            {
                startTime = Time.time;
            }
            else if (currentTarget % 2 == 1)
            {
                float endTime = Time.time;
                float completionTime = endTime - startTime;
                if (completionTime > trialDuration)
                {
                    completionTime = -1;
                }
                taskCompletionTimes.Add(completionTime);
                int attempt = selectActionCounter.GetSelectActionCount();
                numberOfAttempts.Add(attempt);
                Debug.Log($"Time to complete task {currentTarget - 1} to {currentTarget}: {completionTime} seconds, number of attempts: {attempt}");
                startTime = 0;
            }

            targets[currentTarget].gameObject.SetActive(false);
            targets[nextTarget].gameObject.SetActive(true);
            currentTarget++;
            nextTarget = currentTarget + 1;

            // Start the timer for the next target
            StartTrialTimer();
        }
    }
    private void StartTrialTimer()
    {
        if (trialTimerCoroutine != null)
        {
            StopCoroutine(trialTimerCoroutine);
        }
        trialTimerCoroutine = StartCoroutine(TrialTimer());
    }

    private IEnumerator TrialTimer()
    {
        yield return new WaitForSeconds(trialDuration);
        //Debug.Log($"Time ran out for task {currentTarget}, moving to the next target.");

        Vector3 targetDestination = targets[currentTarget].transform.position;
        targetDestination.y += targets[currentTarget].transform.localScale.y/2;
        
        var teleportRequest = new TeleportRequest
        {
            destinationPosition = targetDestination,
            matchOrientation = MatchOrientation.WorldSpaceUp
        };
        teleportationProvider.QueueTeleportRequest(teleportRequest);
        
        EnableNextTarget(new SelectExitEventArgs()); 
    }
    public void ResetTargetsAndXROrigin()
    {
        // if (SceneManager.GetActiveScene().name != "ART"  && SceneManager.GetActiveScene().name != "ART2"|| currentRowIndex >= mappingCombinations.Count)
        if (SceneManager.GetActiveScene().name != "ART"  && SceneManager.GetActiveScene().name != "ART2"|| currentRowIndex >= combinations.Count)
        {
            FindObjectOfType<DisplayVariantText>().DisplayEndText();
            return;
        }
        
        currentRowIndex++;

        ApplySettingsFromLine(combinations[participantConditions.participantID% 6-1][currentRowIndex]);
        // ApplySettingsFromLine(mappingCombinations[participantConditions.participantID - 1][currentRowIndex]); 


        float terrainHeight = Terrain.activeTerrain.SampleHeight(Vector3.zero);
        Vector3 newPosition = new Vector3(0, terrainHeight+1.5F, -0.01f);
        xrOrigin.MoveCameraToWorldLocation(newPosition);
        xrOrigin.MatchOriginUpCameraForward(Vector3.up, Vector3.forward);
        
        currentTarget = 0;
        nextTarget = 1;
        SetupObstaclesWithTargetConditions();
        
        StartTrialTimer();
    }

    private void LogData()
    {
        if (!participantConditions.recordData) return;
        
        // Convert DistanceSizeCombinations to a string format
        string distanceSizeCombinationString = string.Join(", ", 
            obstacleManager.GetDistanceSizeCombinations()
                .Select(pair => $"{pair.distance},{pair.size}")
        );
        string logEntry = "";
        if (SceneManager.GetActiveScene().name == "ART")
        {
            logEntry = $"Participant ID: {participantConditions.participantID}, " +
                       $"Scene: {SceneManager.GetActiveScene().name}, " +
                       $"ART Variant: {combinations[participantConditions.participantID% 6-1][currentRowIndex]}, " +
                       $"Distance Size Combination: {distanceSizeCombinationString}, " +
                       $"Task Completion Times: {string.Join(", ", taskCompletionTimes)}, " +
                       $"Number of Attempts: {string.Join(", ", numberOfAttempts)}"; 
        }
        else if (SceneManager.GetActiveScene().name == "ART2")
        {
            logEntry = $"Participant ID: {participantConditions.participantID}, " +
                       $"Scene: {SceneManager.GetActiveScene().name}, " +
                       $"ART Variant: {mappingCombinations[participantConditions.participantID-1][currentRowIndex]}, " +
                       $"Distance Size Combination: {distanceSizeCombinationString}, " +
                       $"Task Completion Times: {string.Join(", ", taskCompletionTimes)}, " +
                       $"Number of Attempts: {string.Join(", ", numberOfAttempts)}"; 
        }
        else {
            logEntry = $"Participant ID: {participantConditions.participantID}, " +
                       $"Scene: {SceneManager.GetActiveScene().name}, "+
                       $"Distance Size Combination: {distanceSizeCombinationString}, " +
                       $"Task Completion Times: {string.Join(", ", taskCompletionTimes)}, " +
                       $"Number of Attempts: {string.Join(", ", numberOfAttempts)}"; 
        }
        
        Debug.Log(logEntry);
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
    public int repetition;

    public float intermedidateObstacleSize;
    public int intermedidateObstacleDistance;
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
            repetition = Convert.ToInt32(initialSettings.Child("repetition").Value.ToString()),
            intermedidateObstacleSize = Convert.ToSingle(initialSettings.Child("intermedidateObstacleSize").Value.ToString()),
            intermedidateObstacleDistance = Convert.ToInt32(initialSettings.Child("intermedidateObstacleDistance").Value.ToString())
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
