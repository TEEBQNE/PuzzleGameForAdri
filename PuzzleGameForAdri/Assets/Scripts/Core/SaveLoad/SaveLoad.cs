using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

// ToDo TJC: Do we want a folder that is just for level data? Where we save each pack / level as a guid.txt?
public enum SaveLoadFolderNames
{
    PERSISTENT_DATA_FOLDER,
    TEMP_DATA_FOLDER
}

public enum SaveLoadFileNames
{
    INDIVIDUAL_LEVEL_DATA,
    PLAYER_USER_DATA
}

/// <summary>
/// Saves, loads and deletes all data in the game
/// </summary>
/// <typeparam name="T"></typeparam>
public static class SaveLoad<T>
{
    /// <summary>
    /// Save data to a file (overwrite completely)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="folder"></param>
    /// <param name="file"></param>
    public static void Save(T data, SaveLoadFolderNames folder, SaveLoadFileNames file)
    {
        // get the data path of this save data
        string dataPath = GetFilePath(folder.ToString(), file.ToString());

        string jsonData = JsonUtility.ToJson(data, true);
        byte[] byteData;

#if UNITY_EDITOR
        byteData = Encoding.ASCII.GetBytes(jsonData);
#else
        byteData = AESEncryption.EncryptDataS(jsonData);
#endif

        // create the file in the path if it doesn't exist
        // if the file path or name does not exist, return the default SO
        if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dataPath));
        }

        // attempt to save here data
        try
        {
            // save datahere
            File.WriteAllBytes(dataPath, byteData);
            Debug.Log("Save data to: " + dataPath);
        }
        catch (Exception e)
        {
            // write out error here
            Debug.LogError("Failed to save data to: " + dataPath);
            Debug.LogError("Error " + e.Message);
        }
    }

    /// <summary>
    /// /Save data to a file using the string data - overwrite from cloud data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="folder"></param>
    /// <param name="file"></param>
    public static void Save(string data, SaveLoadFolderNames folder, SaveLoadFileNames file)
    {
        // get the data path of this save data
        string dataPath = GetFilePath(folder.ToString(), file.ToString());

        byte[] byteData;

#if UNITY_EDITOR
        byteData = Encoding.ASCII.GetBytes(data);
#else
        byteData = AESEncryption.EncryptDataS(data);
#endif

        // create the file in the path if it doesn't exist
        // if the file path or name does not exist, return the default SO
        if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dataPath));
        }

        // attempt to save here data
        try
        {
            // save datahere
            File.WriteAllBytes(dataPath, byteData);
            Debug.Log("Save data to: " + dataPath);
        }
        catch (Exception e)
        {
            // write out error here
            Debug.LogError("Failed to save data to: " + dataPath);
            Debug.LogError("Error " + e.Message);
        }
    }

    /// <summary>
    /// Prints JSON data that is generated for tutorial sections
    /// </summary>
    /// <param name="data"></param>
    public static void PrintEncryptedData(T data)
    {
        string jsonData = JsonUtility.ToJson(data, true);
        Debug.Log(data.GetType().Name + ": " + AESEncryption.EncryptDataString(jsonData));
    }

    /// <summary>
    /// Returns tutorial string data as an object to load data into the game
    /// </summary>
    /// <param name="encryptedData"></param>
    /// <returns></returns>
    public static T LoadTutorialData(string encryptedData)
    {
        return (T)Convert.ChangeType(JsonUtility.FromJson<T>(AESEncryption.DecryptData(encryptedData)), typeof(T));
    }

    /// <summary>
    /// Load all data at a specified file and folder location
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static T Load(SaveLoadFolderNames folder, SaveLoadFileNames file)
    {
        // get the data path of this save data
        string dataPath = GetFilePath(folder.ToString(), file.ToString());

        // if the file path or name does not exist, return the default SO
        if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
        {
            Debug.LogWarning("File or path does not exist! " + dataPath);
            return default(T);
        }

        // load in the save data as byte array
        byte[] jsonDataAsBytes = null;

        try
        {
            jsonDataAsBytes = File.ReadAllBytes(dataPath);
            Debug.Log("<color=green>Loaded all data from: </color>" + dataPath);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to load data from: " + dataPath);
            Debug.LogWarning("Error: " + e.Message);
            return default(T);
        }

        if (jsonDataAsBytes == null)
            return default(T);

        // convert the byte array to json
        string jsonData;

        // convert the byte array to json
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        jsonData = Encoding.ASCII.GetString(jsonDataAsBytes);
#else
        jsonData = AESEncryption.DecryptData(jsonDataAsBytes);
#endif

        // convert to the specified object type
        T returnedData = JsonUtility.FromJson<T>(jsonData);

        // return the casted json object to use
        return (T)Convert.ChangeType(returnedData, typeof(T));
    }

    /// <summary>
    /// Delete all save files given a folder
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    public static bool DeleteAllSaveDataInFolder(SaveLoadFolderNames folder)
    {
        // assume failure
        bool successfulDeletion = false;

        // get the root folder directory 
        string filePath = GetFilePath(folder.ToString());

        // check if the folder exists
        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            Debug.LogWarning(filePath + " does not exist!");
            return successfulDeletion;
        }

        try
        {
            string[] filesInDir = Directory.GetFiles(filePath);
            foreach (string file in filesInDir)
            {
                string folderPath = filePath;

                // build working full path with file extension
                folderPath = Path.Combine(folderPath, file);

                File.Delete(folderPath);
                Debug.Log("Deleted: " + folderPath + " successfully");
            }
            successfulDeletion = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to delete data from : " + filePath);
            Debug.LogWarning("Error: " + e.Message);
            return successfulDeletion;
        }
        return successfulDeletion;
    }

    /// <summary>
    /// Delete all files in a sub folder EXCEPT the given file names
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="filesToIgnore"></param>
    /// <returns></returns>
    public static bool DeleteAllFilesExceptParams(SaveLoadFolderNames folder, in HashSet<string> filesToIgnore)
    {
        // assume failure
        bool successfulDeletion = false;

        // get teh root folder directory
        string filePath = GetFilePath(folder.ToString());

        if(!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            Debug.LogWarning(filePath + " does not exist!");
            return successfulDeletion;
        }

        try
        {
            string[] filesInDir = Directory.GetFiles(filePath);
            foreach (string file in filesInDir)
            {
                // skip files that we placed to skip - make sure to delete ANYTHING that does not end in .txt
                if (Path.GetExtension(file) == ".txt" && filesToIgnore.Contains(Path.GetFileName(GetFileName(file))))
                    continue;

                string folderPath = filePath;

                // build working full path with file extension
                folderPath = Path.Combine(folderPath, file);

                File.Delete(folderPath);
                Debug.Log("Deleted: " + folderPath + " successfully");
            }
            successfulDeletion = true;
        }
        catch(Exception e)
        {
            Debug.LogWarning("Failed to delete data from : " + filePath);
            Debug.LogWarning("Error: " + e.Message);
            return successfulDeletion;
        }

        return successfulDeletion;
    }

    /// <summary>
    /// Delete specific file by name
    /// </summary>
    /// <param name="folder"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    public static bool DeleteSpecificFile(SaveLoadFolderNames folder, SaveLoadFileNames file)
    {
        // assume failure
        bool successfulDeletion = false;

        // get the root folder directory 
        string filePath = GetFilePath(folder.ToString(), file.ToString());

        // check if the folder exists
        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            Debug.LogWarning(filePath + " does not exist!");
            return successfulDeletion;
        }

        try
        {
            File.Delete(filePath);
            successfulDeletion = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to delete data from : " + filePath);
            Debug.LogWarning("Error: " + e.Message);
            return successfulDeletion;
        }
        return successfulDeletion;
    }

    /// <summary>
    /// Create file path for where a file is stored on the specific platform given a folder name and file name
    /// </summary>
    /// <param name="FolderName"></param>
    /// <param name="FileName"></param>
    /// <returns></returns>
    private static string GetFilePath(string FolderName, string FileName = "", bool encrypted = true)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        encrypted = false;
#endif

        string filePath;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        // mac
        filePath = Path.Combine(Application.streamingAssetsPath, encrypted ? (Base64String.Encrypt("data") + "/" + Base64String.Encrypt(FolderName)) : ("data/" + FolderName));

        if (FileName != "")
            filePath = Path.Combine(filePath, encrypted ? (Base64String.Encrypt(FileName) + ".txt") : (FileName + ".txt"));
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        // windows
        filePath = Path.Combine(Application.persistentDataPath, encrypted ? (Base64String.Encrypt("data") + "/" + Base64String.Encrypt(FolderName)) : ("data/" + FolderName));

        if(FileName != "")
            filePath = Path.Combine(filePath, encrypted ? (Base64String.Encrypt(FileName) + ".txt") : (FileName + ".txt"));
#elif UNITY_ANDROID
        // android
        filePath = Path.Combine(Application.persistentDataPath, encrypted ? (Base64String.Encrypt("data") + "/" + Base64String.Encrypt(FolderName)) : ("data/" + FolderName));

        if(FileName != "")
            filePath = Path.Combine(filePath, encrypted ? (Base64String.Encrypt(FileName) + ".txt") : (FileName + ".txt"));
#elif UNITY_IOS
        // ios
        filePath = Path.Combine(Application.persistentDataPath, encrypted ? (Base64String.Encrypt("data") + "/" + Base64String.Encrypt(FolderName)) : ("data/" + FolderName));

        if(FileName != "")
            filePath = Path.Combine(filePath, encrypted ? (Base64String.Encrypt(FileName) + ".txt") : (FileName + ".txt"));
#endif
        return filePath;
    }

    /// <summary>
    /// Get a file name based on the OS
    /// </summary>
    /// <param name="FileName"></param>
    /// <returns></returns>
    private static string GetFileName(string FileName)
    {
#if UNITY_EDITOR
        return FileName.Replace(".txt", "");
#else
        return Base64String.Decrypt(Path.GetFileName(FileName).Replace(".txt", ""));
#endif
    }
}