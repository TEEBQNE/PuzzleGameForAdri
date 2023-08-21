using UnityEngine;
using System;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] private GameObject _usernameCreationUI = null;
    [SerializeField] private GameObject _titleScreenSelectionUI = null;

    private string _playerUsername;
    private string _playerGuid;

    private void Start()
    {
        if (_playerGuid == "")
        {
            // we have no player data so set it
            _usernameCreationUI.SetActive(true);
            return;
        }

        _titleScreenSelectionUI.SetActive(true);
    }

    public void SetUsername(string username)
    {
        _playerUsername = username;
        _playerGuid = Guid.NewGuid().ToString();
    }

    public void LoadUsernameData(SaveLoadStructures.PlayerUserData playerData)
    {
        _playerUsername = playerData.username;
        _playerGuid = playerData.userId;
    }

    public SaveLoadStructures.PlayerUserData SaveUsernameData()
    {
        return new SaveLoadStructures.PlayerUserData(_playerGuid, _playerUsername);
    }
}