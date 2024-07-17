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
        Task<DataSnapshot> retrieveSettings = realtimeDB.Child("State").Child(participantID.ToString()).GetValueAsync();
        yield return new WaitUntil(predicate: () => retrieveSettings.IsCompleted);
        
        DataSnapshot studySettings = retrieveSettings.Result.Child("studySettings");
        DataSnapshot targetSettings = retrieveSettings.Result.Child("targetSettings");
        
        if (studySettings.Exists && targetSettings.Exists)
        {
            StudyConditions studyConditions = FirebaseDataToPrimitives.ToStudyConditions(studySettings);
            TargetConditions targetConditions = FirebaseDataToPrimitives.ToTargetConditions(targetSettings);
            
            GameManager.Instance.ApplySettings(studyConditions);
            GameManager.Instance.ApplyTargetValues(targetConditions);
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
