using UnityEngine;

/// <summary>
/// Class that all save/load files need to inherit from
/// </summary>
public abstract class RequiresSaveLoad : MonoBehaviour
{
    public abstract void Load(SaveLoadFolderNames folder, SaveLoadFileNames file);

    public abstract void Save(SaveLoadFolderNames folder, SaveLoadFileNames file);

    public abstract void PrintCurrentSaveData();
}