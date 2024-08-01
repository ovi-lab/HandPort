using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;

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
    private int handVis; 

    protected override void Awake()
    {
        base.Awake();
        nextTarget = currentTarget + 1;
        obstacleManager = FindObjectOfType<ObstacleManager>();
        teleportAdapter = FindObjectOfType<GoGoDetachAdapterStable3>( true);
        cameraManager =FindObjectOfType<CameraManager>();

        //ApplyRandomizedConditions();
    }
    
    
    private void ApplyRandomizedConditions()
    {
        var cameraTypes = Enum.GetNames(typeof(CameraType));
        var panelAnchors = Enum.GetNames(typeof(CameraAnchor));
        int[] handVisualisations = { 0, 1 };

        var combinations = GenerateAllCombinations(cameraTypes, panelAnchors, handVisualisations);

        // Generate Latin square
        var latinSquare = GenerateLatinSquare(combinations.Count);

        // Apply Latin square to shuffle combinations
        var shuffledCombinations = ApplyLatinSquare(combinations, latinSquare);

        // Apply the shuffled settings (replace with actual application logic)
        foreach (var combination in shuffledCombinations)
        {
            
        }

        Debug.Log(shuffledCombinations.Count);
    }

    private List<(string, string, int)> GenerateAllCombinations(string[] cameraTypes, string[] panelAnchors, int[] handVisualisations)
    {
        var combinations = from cameraType in cameraTypes
                           from panelAnchor in panelAnchors
                           from handVis in handVisualisations
                           select (cameraType, panelAnchor, handVis);

        return combinations.ToList();
    }

    private int[][] GenerateLatinSquare(int n)
    {
        int[][] latinSquare = new int[n][];
        for (int i = 0; i < n; i++)
        {
            latinSquare[i] = new int[n];
            for (int j = 0; j < n; j++)
            {
                latinSquare[i][j] = (i + j) % n;
            }
        }
        return latinSquare;
    }

    private List<(T1, T2, T3)> ApplyLatinSquare<T1, T2, T3>(List<(T1, T2, T3)> list, int[][] latinSquare)
    {
        List<(T1, T2, T3)> result = new List<(T1, T2, T3)>();
        for (int i = 0; i < latinSquare.Length; i++)
        {
            for (int j = 0; j < latinSquare[i].Length; j++)
            {
                int position = latinSquare[i][j];
                result.Add(list[position]);
            }
        }
        return result;
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
        teleportAdapter.SetInitialAdapterValues(adapterConditions.originShoulderDistance,
            adapterConditions.shoulderEllbowDistance, adapterConditions.ellbowWristDistance, adapterConditions.maxVirtDistance);
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
        
    
    // before study:
    
    // tbd: if recordData is true log, else no
    // tbd: if reset is true, set participantID to 1

    // tbd: if maxParticipant == participantID set reset to true

}
