using UnityEngine;

public class PlayerDataSaveLoad : RequiresSaveLoad
{
    [SerializeField] private LevelSelectionManager _levelSelectionManager = null;

    public override void Load(SaveLoadFolderNames folder, SaveLoadFileNames file)
    {
        _levelSelectionManager.LoadUsernameData(SaveLoad<SaveLoadStructures.PlayerUserData>.Load(folder, file) ?? new SaveLoadStructures.PlayerUserData());
    }

    public override void PrintCurrentSaveData()
    {
        Debug.Log("Not implemented");
    }

    public override void Save(SaveLoadFolderNames folder, SaveLoadFileNames file)
    {
        SaveLoad<SaveLoadStructures.PlayerUserData>.Save(_levelSelectionManager.SaveUsernameData(), folder, file);
    }
}