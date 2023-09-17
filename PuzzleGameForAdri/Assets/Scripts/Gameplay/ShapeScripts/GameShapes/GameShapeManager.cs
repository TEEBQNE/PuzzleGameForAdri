using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameShapeManager : BaseShapeManager
{
    #region SerializedFields
    [SerializeField] private GameObject _youWinText = null;
    [SerializeField] private GameObject _youLoseText = null;
    [SerializeField] private GameObject _replayUI = null;
    #endregion

    #region Variables
    private bool _endConditionMet = false;
    private int _currentZIndex = 1;
    private Vector2 _adjustedScreenResolution = Vector2.zero;
    private Vector2 _goalScreenResolution = Vector2.zero;
    #endregion

    #region Inline Methods
    public int GetCurrentBackground() { return _currentColor; }
    #endregion

    #region Resolution Border Scaling
    protected override void SetBorderScaleAndPosition()
    {
        base.SetBorderScaleAndPosition();

        // find the change in our X / Y based on the borders
        float xDiff = scaleFactorX * (1.0f - BORDER_DISPLAY_ONSCREEN_PERCENT) * 0.5f;
        float yDiff = scaleFactorY * (1.0f - BORDER_DISPLAY_ONSCREEN_PERCENT) * 0.5f;

        // adjust our screen resolution based on our border
        _adjustedScreenResolution = new Vector2(scaleFactorX - (scaleFactorX * (1.0f - BORDER_DISPLAY_ONSCREEN_PERCENT)), scaleFactorY - (scaleFactorY * (1.0f - BORDER_DISPLAY_ONSCREEN_PERCENT)));

        // when we have no goal resolution or when the current is so close to our current - don't adjust
        if (_goalScreenResolution == Vector2.zero)
        {
            AdjustBordersAndCamera(new Vector3(xDiff, yDiff, 0f));
            return;
        }

        // determine the resolution change compared to our goal
        float adjustedWidth = _adjustedScreenResolution.x - _adjustedScreenResolution.y / _goalScreenResolution.y * _goalScreenResolution.x;

        // ignore anything that has a very similar aspect ratio
        if(adjustedWidth <= 0.5f)
        {
            if(adjustedWidth < 0.0f)
            {
                float adjustedHeight = _adjustedScreenResolution.y - _adjustedScreenResolution.x / _goalScreenResolution.x * _goalScreenResolution.y;
                AdjustHeightResolutionToGoalResolution(adjustedHeight, topRightCorner, scaleFactorY, xDiff);
                return;
            }

            AdjustBordersAndCamera(new Vector3(xDiff, yDiff, 0f));
            return;
        }

        // scale based on width
        AdjustWidthResolutionToGoalResolution(adjustedWidth, topRightCorner, scaleFactorX, yDiff);
    }

    private void AdjustWidthResolutionToGoalResolution(float adjustedWidth, Vector2 topRightCorner, float scaleFactorX, float yDiff)
    {
        float adjustWidthPercent = adjustedWidth / _adjustedScreenResolution.x;

        // we now have our true aspect (to check, divide the _adjustedScreenResolution.x / _adjustedScreenResolution.y and check to see if the ratio is correct)
        _adjustedScreenResolution = new Vector2(_adjustedScreenResolution.x - adjustedWidth, _adjustedScreenResolution.y);

        // update our left / right borders to adjust our screen resolution
        _borders[2].transform.localPosition = new Vector3(-_borders[2].bounds.extents.x * (CalculateBorderPercent() - adjustWidthPercent), _borders[2].bounds.extents.y, -2f);
        _borders[3].transform.localPosition = new Vector3(topRightCorner.x + (_borders[2].bounds.extents.x * (CalculateBorderPercent() - adjustWidthPercent)), _borders[2].bounds.extents.y, -2f);

        AdjustBordersAndCamera(new Vector2(scaleFactorX * (1.0f - (CalculateBorderPercent() - adjustWidthPercent)) * 0.5f, yDiff));
    }

    private void AdjustHeightResolutionToGoalResolution(float adjustedHeight, Vector2 topRightCorner, float scaleFactorY, float xDiff)
    {
        float adjustHeightPercent = adjustedHeight / _adjustedScreenResolution.y;

        // we now have our true aspect (to check, divide the _adjustedScreenResolution.x / _adjustedScreenResolution.y and check to see if the ratio is correct)
        _adjustedScreenResolution = new Vector2(_adjustedScreenResolution.x, _adjustedScreenResolution.y - adjustedHeight);

        _borders[0].transform.localPosition = new Vector3(_borders[0].bounds.extents.x, topRightCorner.y + (_borders[0].bounds.extents.y * (CalculateBorderPercent() - adjustHeightPercent)), -2f);
        _borders[1].transform.localPosition = new Vector3(_borders[1].bounds.extents.x, -_borders[1].bounds.extents.y * (CalculateBorderPercent() - adjustHeightPercent), -2f);

        AdjustBordersAndCamera(new Vector2(xDiff, scaleFactorY * (1.0f - (CalculateBorderPercent() - adjustHeightPercent)) * 0.5f));
    }

    private float CalculateBorderPercent()
    {
        // we only do 50% of the border for each display as the original display factored this in
        return 1.0f - ((1.0f - BORDER_DISPLAY_ONSCREEN_PERCENT) / 2f);
    }

    private void AdjustBordersAndCamera(Vector3 offset)
    {
        foreach (var border in _borders)
        {
            border.transform.localPosition -= offset;
        }

        // transform to add to get to our 'true' 0,0 point with boarders added from the transform of our camera
        _mainCam.transform.position -= offset;
    }
    #endregion

    #region Shape Callbacks
    public int ExpandCallback(int colorIndex, GameShape shape)
    {
        int tmpIdx = _currentZIndex;
        --_currentZIndex;
        _currentColor = colorIndex;
        EvaluateWinCondition(shape);
        return tmpIdx;
    }

    public void EvaluateWinCondition(GameShape currentShape)
    {
        if(_endConditionMet)
        {
            return;
        }

        int shapesLeft = 0;
        int moveableShapes = 0;
        bool whiteOrBlack = false;

        HashSet<int> colorsPresent = new HashSet<int>();

        foreach(GameShape shape in _shapeScripts)
        {
            if(shape.IsExpanding)
            {
                return;
            }

            if(!shape.IsPhysicsEnabled)
            {
                continue;
            }

            // if a black or white exists then keep going
            if(shape.ColorIndex <= 1)
            {
                whiteOrBlack = true;
            }

            // we store the moveable objects as positive color and the not moveable as negative color
            // not moveable needs a moveable, moveable can do either
            bool hasMatchingMoveable = colorsPresent.Contains(shape.ColorIndex);
            bool hasMatchingNoneMoveable = colorsPresent.Contains(-shape.ColorIndex);

            if (shape.CanBeMoved ? (hasMatchingMoveable || hasMatchingNoneMoveable) : hasMatchingMoveable)
            {
                return;
            }

            colorsPresent.Add(shape.CanBeMoved ? shape.ColorIndex : -shape.ColorIndex);

            ++shapesLeft;

            if(shape.CanBeMoved)
            {
                ++moveableShapes;
            }
        }

        DisplayWinCondition(shapesLeft, whiteOrBlack, moveableShapes);
    }
    #endregion

    #region Helper Methods
    private void DisplayWinCondition(int shapesLeft, bool whiteOrBlack, int moveableShapes)
    {
        if (shapesLeft > 1 && whiteOrBlack && moveableShapes > 0)
        {
            return;
        }

        _endConditionMet = true;

        Debug.Log(shapesLeft + " " + _goalColor + " " + _currentColor);

        _youWinText.transform.parent.gameObject.SetActive(true);

        ToggleRestartUI(true);

        if (shapesLeft == 0 && _goalColor == _currentColor)
        {
            _youWinText.gameObject.SetActive(true);
            return;
        }

        _youLoseText.gameObject.SetActive(true);
    }
    #endregion

    #region Debug / Temp
    private void Update()
    {
        // ToDo TJC: This is just for debugging remove this later - need a permanent solution for mobile
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ToggleRestartUI(true);
        }
    }

    /// <summary>
    /// ToDo TJC: Have this reload a serialized level not a level (save load, etc.)
    /// </summary>
    public void RestartActiveLevel()
    {
        SceneManager.LoadScene(0);
    }

    public void ToggleRestartUI(bool toggle)
    {
        _replayUI.SetActive(toggle);
    }
    #endregion

    #region Save / Load
    public SaveLoadStructures.Level SaveLevelData()
    {
        return new SaveLoadStructures.Level(GetShapes(), _roundColors, _startingColor, _goalColor, _adjustedScreenResolution, new Vector2(Screen.width, Screen.height));
    }

    private List<SaveLoadStructures.Shape> GetShapes()
    {
        List<SaveLoadStructures.Shape> allShapeData = new List<SaveLoadStructures.Shape>();
        HelperMethods.ResetIds();

        // create an id match dictionary here
        Dictionary<GameShape, int> shapeToId = new Dictionary<GameShape, int>();

        foreach(GameShape shape in _shapeScripts)
        {
            shapeToId.Add(shape, HelperMethods.GetNextId());
        }

        foreach(GameShape shape in _shapeScripts)
        {
            SaveLoadStructures.Shape shapeData = shape.SaveShapeData(shapeToId);
            shapeData.position = new Vector2(shapeData.position.x / _adjustedScreenResolution.x, shapeData.position.y / _adjustedScreenResolution.y);
            shapeData.scale = new Vector2(shapeData.scale.x / _adjustedScreenResolution.x, shapeData.scale.y / _adjustedScreenResolution.y);
            allShapeData.Add(shapeData);
        }

        return allShapeData;
    }

    public void LoadLevelData(SaveLoadStructures.Level levelData)
    {
        _goalScreenResolution = levelData.screenResolution;
        // we descend z layers here so they do not stack
        _currentZIndex = levelData.shapes.Count * 3;

        _startingColor = levelData.startingBackgroundColor;
        _goalColor = levelData.goalBackgroundColor;
        _roundColors = levelData.shapeColors;

        StartCoroutine(WaitUntilStart(levelData));
    }

    private IEnumerator WaitUntilStart(SaveLoadStructures.Level levelData)
    {
        // wait until after the first update
        yield return new WaitForFixedUpdate();

        Dictionary<int, GameShape> idToShape = new Dictionary<int, GameShape>();
        HelperMethods.ResetIds();

        foreach (SaveLoadStructures.Shape shapeData in levelData.shapes)
        {
            int shapeIdx = HelperMethods.GetNextId();

            GameShape shape = Instantiate(ShapePrefab).GetComponent<GameShape>();
            _shapeScripts.Add(shape);
            idToShape.Add(shapeIdx, shape);
            shape.LoadShapeData(shapeData);

            shape.transform.UpdateScaleToFitResolution(_adjustedScreenResolution);
            shape.SetColor(_roundColors[shapeData.colorIndex]);
            shape.SetShape(_shapeSprites[(int)shapeData.shapeIndex]);
            shape.SetCallback(ExpandCallback, GetCurrentBackground, EvaluateWinCondition);

            // we set the shapesIdx here to the last idx to use later
            shapeData.childShapes.Add(shapeIdx);
        }

        // iterate all shape data creating a dictionary 
        foreach (SaveLoadStructures.Shape shapeData in levelData.shapes)
        {
            if (shapeData.childShapes == default || shapeData.childShapes.Count == 1)
            {
                continue;
            }

            // the actual shape idx is stored as the last idx
            int shapeIdx = shapeData.childShapes[shapeData.childShapes.Count - 1];

            for (int x = 0; x < shapeData.childShapes.Count - 1; ++x)
            {
                // we store the data as global so we keep its world position, rotation, etc. 
                idToShape[shapeData.childShapes[x]].transform.SetParent(idToShape[shapeIdx].transform, true);
            }
        }
    }
    #endregion
}