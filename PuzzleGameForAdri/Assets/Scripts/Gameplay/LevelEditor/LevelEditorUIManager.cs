using System.Collections;
using UnityEngine;

public class LevelEditorUIManager : MonoBehaviour
{
    #region Constants
    private const float TIME_TO_MOVE = 0.15f;
    #endregion

    #region SerializedFields
    [SerializeField] private CanvasGroup _cg = null;
    [SerializeField] private RectTransform _levelEditorOuterContainer = null;
    [SerializeField] private RectTransform _levelEditorMainUI = null;
    #endregion

    #region Variables
    private bool _isLevelEditorUIActive = false;
    private Coroutine _movingLevelEditorUI = null;
    #endregion

    private void Start()
    {
        _levelEditorOuterContainer.anchoredPosition = new Vector2(_levelEditorOuterContainer.anchoredPosition.x, -_levelEditorMainUI.rect.height);
    }

    public void ToggleLevelEditorUI()
    {
        if(_movingLevelEditorUI != default)
        {
            return;
        }

        _isLevelEditorUIActive = !_isLevelEditorUIActive;

        // we immediatelly block all interaction to prevent inputs when moving
        // re-enable after the Coroutine if we are toggling on
        _cg.interactable = false;

        _movingLevelEditorUI = StartCoroutine(MoveLevelEditorUI());
    }

    private IEnumerator MoveLevelEditorUI()
    {
        float currentYPosition = _levelEditorOuterContainer.anchoredPosition.y;
        float goalYPosition = _isLevelEditorUIActive ? 0.0f : -_levelEditorMainUI.rect.height;
        float currentTime = 0.0f;

        while(currentTime < TIME_TO_MOVE)
        {
            _levelEditorOuterContainer.anchoredPosition = new Vector2(_levelEditorOuterContainer.anchoredPosition.x, Mathf.Lerp(currentYPosition, goalYPosition, currentTime / TIME_TO_MOVE));

            currentTime += Time.deltaTime;
            yield return null;
        }

        _levelEditorOuterContainer.anchoredPosition = new Vector2(_levelEditorOuterContainer.anchoredPosition.x, goalYPosition);
        _cg.interactable = true;
        _movingLevelEditorUI = null;
    }
}