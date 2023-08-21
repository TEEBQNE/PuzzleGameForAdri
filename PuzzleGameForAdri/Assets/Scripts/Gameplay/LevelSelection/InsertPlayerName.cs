using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

public class InsertPlayerName : MonoBehaviour
{
    [SerializeField] private LevelSelectionManager _levelSelectionManager = null;
    [SerializeField] private TMP_InputField _inputField = null;
    [SerializeField] private TMP_Text _confirmtextHeader = null;

    private string _playerEnteredName = "";

    void OnEnable()
    {
        //Register InputField Event
        _inputField.onValueChanged.AddListener(InputValueChanged);
        _inputField.onSubmit.AddListener(OnSubmit);
    }

    static string CleanInput(string strIn)
    {
        // Replace invalid characters with empty strings.
        return Regex.Replace(strIn,
              @"[^a-zA-Z0-9`!@#$%^&*()_+|\-=\\{}\[\]:"";'<>?,./]", "");
    }
    
    //Called when Input changes
    void InputValueChanged(string attemptedVal)
    {
        _inputField.text = CleanInput(attemptedVal);
    }
    
    void OnDisable()
    {
        //Un-Register InputField Events
        _inputField.onValueChanged.RemoveAllListeners();
        _inputField.onSubmit.RemoveAllListeners();
    }

    void OnSubmit(string str)
    {
        //Confirm name with the user
        _playerEnteredName = str.Trim();

        if(_playerEnteredName.Length == 0)
        {
            // can't have an empty
            _inputField.placeholder.GetComponent<TMP_Text>().text = "Enter non empty name...";
            return;
        }

        _confirmtextHeader.text = "Does the name:\n" + _playerEnteredName + "\nlook good?";
        _confirmtextHeader.transform.parent.gameObject.SetActive(true);
    }

    public void ConfirmNameChoice(bool confirm)
    {
        _confirmtextHeader.transform.parent.gameObject.SetActive(false);

        if (!confirm)
        {
            _inputField.text = "";
            return;
        }

        // send the name to our manager to save the name out / generate a guid for the user
        _levelSelectionManager.SetUsername(_playerEnteredName);
        gameObject.SetActive(false);
    }
}