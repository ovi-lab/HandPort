using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Events;


public class FirebaseManager : MonoBehaviour
{
    public int ParticipantID => participantID;
    public static UnityEvent TriggerNextBlock = new();
    public static DatabaseReference RealtimeDB => realtimeDB;
    private static DatabaseReference realtimeDB;
    private static int participantID;

    void Start()
    {

        realtimeDB = FirebaseDatabase.DefaultInstance.RootReference;
        StartCoroutine(ApplyInitialSettings());
        realtimeDB.Child("State").ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(object sender, ValueChangedEventArgs e)
    {
        StartCoroutine(ApplyInitialSettings());
    }
    private IEnumerator ApplyInitialSettings()
    {
        Task<DataSnapshot> retrieveSettings = realtimeDB.Child("State").GetValueAsync();
        yield return new WaitUntil(predicate: () => retrieveSettings.IsCompleted);

        // DataSnapshot studySettings = retrieveSettings.Result.Child("studySettings");
        DataSnapshot targetSettings = retrieveSettings.Result.Child("targetSettings");
        DataSnapshot participantSettings = retrieveSettings.Result.Child("participantSettings");
        DataSnapshot adapterSettings = retrieveSettings.Result.Child("adapterSettings");

        if (targetSettings.Exists && participantSettings.Exists && adapterSettings.Exists)
        {
            // StudyConditions studyConditions = FirebaseDataToPrimitives.ToStudyConditions(studySettings);
            TargetConditions targetConditions = FirebaseDataToPrimitives.ToTargetConditions(targetSettings);
            ParticipantConditions participantConditions = FirebaseDataToPrimitives.ToParticipantConditions(participantSettings);
            AdapterConditions adapterConditions = FirebaseDataToPrimitives.ToAdapterConditions(adapterSettings);

            // GameManager.Instance.ApplySettings(studyConditions);
            GameManager.Instance.ApplySettings(targetConditions);
            GameManager.Instance.ApplySettings(participantConditions);
            GameManager.Instance.ApplySettings(adapterConditions);
            GameManager.Instance.LogHeader();
            participantID = GameManager.Instance.ParticipantID;

            if (participantConditions.nextBlock)
            {
                TriggerNextBlock?.Invoke();
                FirebaseDatabase.DefaultInstance.RootReference.Child("State").Child("participantSettings").Child("nextBlock")
                    .SetValueAsync(false);

            }
        }


    }

    public static void LogData(string logData, string key)
    {

        var logEntryDict = new Dictionary<string, object>
        {
            { key, logData }
        };

        // Push data to Firebase

        Debug.Log(">>>>>>>" + logData);

        RealtimeDB.Child("log").Child(participantID.ToString()).Child(key).SetValueAsync(logData).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error writing to Firebase: " + task.Exception);
            }
            else
            {
                Debug.Log("Data logged successfully.");
            }
        });
    }


}
[System.Serializable]
public class LogEntry
{
    public string logData;
}