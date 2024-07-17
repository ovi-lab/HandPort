using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using UnityEngine;



public class FirebaseManager : MonoBehaviour
{
    public int ParticipantID => participantID;

    public static DatabaseReference RealtimeDB => realtimeDB;
    private static DatabaseReference realtimeDB;
    private static int participantID;

    // Start is called before the first frame update
    void Start()
    {
        realtimeDB = FirebaseDatabase.DefaultInstance.RootReference;
        StartCoroutine(ApplyInitialSettings());
    }
    private IEnumerator ApplyInitialSettings()
    {
        Task<DataSnapshot> retrieveSettings = realtimeDB.Child("State").Child(participantID.ToString()).GetValueAsync();
        yield return new WaitUntil(predicate: () => retrieveSettings.IsCompleted);
        
        DataSnapshot handVisualisation = retrieveSettings.Result.Child("HandVisualisation");
        DataSnapshot studySettings = retrieveSettings.Result.Child("studySettings");
        DataSnapshot targetSettings = retrieveSettings.Result.Child("targetSettings");
        
        
        if (handVisualisation.Exists && wimMode.Exists)
        {
            StudyConditions settings = FirebaseDataToPrimitives.ToStudyConditions(initialSettings);
            SpawnIVs spawnIVs = FirebaseDataToPrimitives.ToSpawnIVs(spawnValues);
            
            GameManager.Instance.ApplySettings(settings);
            GameManager.Instance.ApplyIVs(spawnIVs);
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
