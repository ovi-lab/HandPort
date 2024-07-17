using System;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public List<TeleportationAnchor> targets = new List<TeleportationAnchor>();
    public GameObject CurrentTarget => targets[currentTarget].gameObject;

    private int currentTarget = 0, nextTarget;
    private DatabaseReference studySettings;
    private ObstacleManager obstacleManager;
    private StudyConditions studyConditions;
    private SpawnIVs spawnIVs;

    protected override void Awake()
    {
        base.Awake();
        nextTarget = currentTarget + 1;
    }

    public void ApplyIVs(SpawnIVs _values)
    {
        spawnIVs = _values;
    }

    public void ApplySettings(StudyConditions _settings)
    {
        studyConditions = _settings;
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
        if (nextTarget >= targets.Count) return;
        targets[currentTarget].gameObject.SetActive(false);
        targets[nextTarget].gameObject.SetActive(true);
        currentTarget++;
        nextTarget = currentTarget + 1;
    }

    private void DestroyTargets()
    {
        foreach (TeleportationAnchor target in targets)
        {
            target.selectExited.RemoveListener(EnableNextTarget);
        }
    }

    private void OnDestroy()
    {
        DestroyTargets();
    }
}

[Serializable]
public class StudyConditions
{
    public int AnchorPoint;
    public int CameraType;
    public bool restart;
    public bool recordData;
}

[Serializable]
public class SpawnIVs
{
    public int extremeTargets;
    public int diagonalTargets;
    public int simpleTargets;
    public int repeatTimes;
}

public static class FirebaseDataToPrimitives
{
    public static StudyConditions ToStudyConditions(DataSnapshot initialSettings)
    {
        StudyConditions studyConditions = new StudyConditions
        {
            AnchorPoint = Convert.ToInt32(initialSettings.Child("AnchorPoint").Value.ToString()),
            CameraType = Convert.ToInt32(initialSettings.Child("CameraType").Value.ToString()),
            restart = Convert.ToBoolean(initialSettings.Child("Restart").Value.ToString()),
            recordData = Convert.ToBoolean(initialSettings.Child("RecordData").Value.ToString())
        };
        return studyConditions;
    }

    public static SpawnIVs ToSpawnIVs(DataSnapshot IVs)
    {
        SpawnIVs spawnIVs = new SpawnIVs
        {
            extremeTargets = Convert.ToInt32(IVs.Child("ExtremeTargets").Value.ToString()),
            diagonalTargets = Convert.ToInt32(IVs.Child("DiagonalTargets").Value.ToString()),
            simpleTargets = Convert.ToInt32(IVs.Child("SimpleTargets").Value.ToString()),
            repeatTimes = Convert.ToInt32(IVs.Child("RepeatTimes").Value.ToString())
        };
        return spawnIVs;
    }
}
