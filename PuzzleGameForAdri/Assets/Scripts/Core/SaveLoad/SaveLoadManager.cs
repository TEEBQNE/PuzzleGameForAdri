using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.SceneManagement;
#endif

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SaveLoadManager))]
public class EditorSave : Editor
{
    /// <summary>
    /// Used to generate JSON data from calendar data to the cloud data
    /// </summary>
    public override void OnInspectorGUI()
    {
        SaveLoadManager saveManager = (SaveLoadManager)target;

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Force Save", GUILayout.Height(50f)))
        {
            saveManager.EditorSave();
        }
        if(GUILayout.Button("Print tutorial save data", GUILayout.Height(50f)))
        {
            saveManager.PrintAllTutorialData();
        }
        GUI.backgroundColor = Color.white;

        // draw default inspector
        DrawDefaultInspector();
    }
}
#endif

[System.Serializable]
public class SaveData
{
    // script the save/load functions are on
    public RequiresSaveLoad ScriptToSaveLoadTo;

    // the file path that this data is in
    public SaveLoadFolderNames FolderName;
    public SaveLoadFileNames FileName;
    public bool shouldSaveData = true;
    public bool shouldLoadData = true;
}

[DefaultExecutionOrder(-49)]
public class SaveLoadManager : MonoBehaviour
{
    [SerializeField] private List<SaveData> AllSceneSaveLoadData = new List<SaveData>();

    private bool ErrorThrownDoNotSave = false;
    private static ILogger logger = Debug.unityLogger;

    private HashSet<SaveLoadFileNames> filesToIgnore = new HashSet<SaveLoadFileNames>() { };
    private HashSet<SaveLoadFileNames> tempFiles = new HashSet<SaveLoadFileNames>() { };

    // Start is called before the first frame update
    private void Awake()
    {
#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
        SaveLoad<SaveLoadManager>.EncryptOldData();
#endif

        // only enable errors and exceptions when in a real build - everything when in editor
        logger.filterLogType = Debug.isDebugBuild ? LogType.Log : LogType.Exception;

        // load in all game data for each object in the scene
        foreach (SaveData data in AllSceneSaveLoadData)
        {
            if(data.shouldLoadData)
                data.ScriptToSaveLoadTo.Load(data.FolderName, data.FileName);
        }

        Application.logMessageReceived += HandleException;
    }

    /// <summary>
    /// Handles any error being thrown - preserves player save data in case an error occurs
    /// </summary>
    /// <param name="logString"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    void HandleException(string logString, string stackTrace, LogType type)
    {
        // when there is an actual error, print it and do NOT save data as data can now be corrupted
        if (type == LogType.Exception)
        {
            //Debug.LogError("Error: " + logString + " " + stackTrace);
            ErrorThrownDoNotSave = true;
        }
    }

    public void AssureSaveDataIsNotSaved()
    {
        ErrorThrownDoNotSave = true;
    }

    /// <summary>
    /// Game is closing, so send out save for all data
    /// </summary>
    private void OnApplicationQuit()
    {
        // do NOT save if there is an error
        if (ErrorThrownDoNotSave)
            return;

        SaveAllGameData();
    }

    public void SceneChangingSaveData(string emptyData)
    {
        SaveAllGameData();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Force save to simulate the app in pause mode
    /// </summary>
    public void EditorSave()
    {
        SaveAllGameData();
    }
#endif

    private void SaveAllGameData()
    {
        foreach (SaveData data in AllSceneSaveLoadData)
        {
            if(data.shouldSaveData && !ErrorThrownDoNotSave)
            {
                data.ScriptToSaveLoadTo.Save(data.FolderName, data.FileName);
            }
        }
    }

    /// <summary>
    /// Print all of the active save data to input for tutorial data
    /// </summary>
    public void PrintAllTutorialData()
    {
        var saveObjects = Resources.FindObjectsOfTypeAll(typeof(RequiresSaveLoad));

        foreach (RequiresSaveLoad obj in saveObjects)
            obj.PrintCurrentSaveData();
    }

    // might be needed for android when you click menu then swipe an application away
    // need testing!! (def needed on iOS)
#if UNITY_ANDROID || UNITY_IOS
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // actively quiting
            foreach (SaveData data in AllSceneSaveLoadData)
            {
                if (data.shouldSaveData && !ErrorThrownDoNotSave)
                {
                    data.ScriptToSaveLoadTo.Save(data.FolderName, data.FileName);
                }
            }
        }
        else if(!pause)
        {
            // re-opened
        }
    }
#endif
}