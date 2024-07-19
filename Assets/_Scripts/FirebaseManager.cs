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
    
    [SerializeField] private TextAsset dataStructureJSON;
    
    void Start()
    {
        participantID = 1;
        realtimeDB = FirebaseDatabase.DefaultInstance.RootReference;
        StartCoroutine(ApplyInitialSettings());
        
    }
    private IEnumerator ApplyInitialSettings()
    {
        Task<DataSnapshot> retrieveSettings = realtimeDB.Child("State").GetValueAsync();
        yield return new WaitUntil(predicate: () => retrieveSettings.IsCompleted);
        
        DataSnapshot studySettings = retrieveSettings.Result.Child("studySettings");
        DataSnapshot targetSettings = retrieveSettings.Result.Child("targetSettings");
        DataSnapshot participantSettings = retrieveSettings.Result.Child("participantSettings");
        Debug.Log(studySettings.Exists && targetSettings.Exists);
        
        if (studySettings.Exists && targetSettings.Exists && participantSettings.Exists)
        {
            StudyConditions studyConditions = FirebaseDataToPrimitives.ToStudyConditions(studySettings);
            TargetConditions targetConditions = FirebaseDataToPrimitives.ToTargetConditions(targetSettings);
            ParticipantConditions participantConditions = FirebaseDataToPrimitives.ToParticipantConditions(participantSettings);
            
            GameManager.Instance.ApplySettings(studyConditions);
            GameManager.Instance.ApplySettings(targetConditions);
            GameManager.Instance.ApplySettings(participantConditions);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
