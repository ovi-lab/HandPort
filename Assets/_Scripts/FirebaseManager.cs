using System.Collections;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;



public class FirebaseManager : MonoBehaviour
{
    public int ParticipantID => participantID;
    public static DatabaseReference RealtimeDB => realtimeDB;
    private static DatabaseReference realtimeDB;
    private static int participantID;
    
    void Start()
    {
        participantID = 1;
        realtimeDB = FirebaseDatabase.DefaultInstance.RootReference;
        StartCoroutine(ApplyInitialSettings());
        realtimeDB.ValueChanged += OnValueChanged;
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
        }
    }
    
    public static void LogData(string logData)
    {
        // Create a unique key for each log entry
        string key = realtimeDB.Child("log").Push().Key;
        
        // Prepare the data to push
        var data = new
        {
            participantID = FirebaseManager.participantID,
            logData = logData
        };
        
        // Push data to Firebase
        realtimeDB.Child("log").Child(key).SetValueAsync(data).ContinueWith(task =>
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
