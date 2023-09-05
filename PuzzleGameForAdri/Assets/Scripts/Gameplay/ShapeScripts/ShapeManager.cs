using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ShapeNames
{
    SQUARE,
    CIRCLE,
    DIAMOND
}

public class ShapeManager : MonoBehaviour
{
#if UNITY_EDITOR
    [NamedArray(typeof(ShapeNames))]
#endif
    [SerializeField] private List<Sprite> ShapeSprites = new List<Sprite>();
    [SerializeField] private GameObject ShapePrefab = null;
    [SerializeField] private List<ShapeScript> Shapes = new List<ShapeScript>();
    [SerializeField] private GameObject _youWinText = null;
    [SerializeField] private GameObject _youLoseText = null;
    [SerializeField] private Camera _mainCam = null;
    [SerializeField] private GameObject _replayUI = null;

    [SerializeField] private int _goalColor = -1;
    [SerializeField] private int _startingColor = -1;

    private int _currentColor = -1;
    [SerializeField] private List<SpriteRenderer> _borders = new List<SpriteRenderer>();

    [SerializeField] private List<Color> _roundColors = new List<Color>() { Color.white, Color.black };

    private bool endConditionMet = false;

    private int currentZIndex = 1;
    private float resolutionScaleChange = 1.0f;

    private Vector2 _adjustedScreenResolution = Vector2.zero;
    private Vector2 _goalScreenResolution = Vector2.zero;

    private const float BORDER_DISPLAY_ONSCREEN_PERCENT = 0.9f;
    public const int WHITE_INDEX = 0;
    public const int BLACK_INDEX = 1;

    public int GetCurrentBackground() { return _currentColor; }

    private void Start()
    {
        _currentColor = _startingColor;

        _mainCam.backgroundColor = _roundColors[_currentColor];

        foreach(SpriteRenderer rend in _borders)
        {
            rend.color = _roundColors[_goalColor];
        }

        SetBorderScaleAndPosition();
    }

    private void SetBorderScaleAndPosition()
    {
        // grab screen size
        var topRightCorner = _mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, _mainCam.transform.position.z));

        float aspect = (float)Screen.width / Screen.height;
        float worldSpaceHeight = _mainCam.orthographicSize * 2;
        float worldSpaceWidth = worldSpaceHeight * aspect;

        var spriteSize = _borders[0].bounds.size;

        // scale our X to stretch to this value
        var scaleFactorX = worldSpaceWidth / spriteSize.x;

        // scale our Y to stretch to this value
        var scaleFactorY = worldSpaceHeight / spriteSize.y;

        // top -> bottom -> left -> right
        _borders[0].transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
        _borders[0].transform.localPosition = new Vector3(_borders[0].bounds.extents.x, topRightCorner.y + (_borders[0].bounds.extents.y * BORDER_DISPLAY_ONSCREEN_PERCENT), -2f);

        _borders[1].transform.localScale = new Vector3(scaleFactorX, scaleFactorY , 1f);
        _borders[1].transform.localPosition = new Vector3(_borders[1].bounds.extents.x, -_borders[1].bounds.extents.y * BORDER_DISPLAY_ONSCREEN_PERCENT, -2f);

        _borders[2].transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
        _borders[2].transform.localPosition = new Vector3(-_borders[2].bounds.extents.x * BORDER_DISPLAY_ONSCREEN_PERCENT, _borders[2].bounds.extents.y, -2f);

        _borders[3].transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
        _borders[3].transform.localPosition = new Vector3(topRightCorner.x + (_borders[3].bounds.extents.x * BORDER_DISPLAY_ONSCREEN_PERCENT), _borders[3].bounds.extents.y, -2f);

        float xDiff = scaleFactorX * (1.0f - BORDER_DISPLAY_ONSCREEN_PERCENT) * 0.5f;
        float yDiff = scaleFactorY * (1.0f - BORDER_DISPLAY_ONSCREEN_PERCENT) * 0.5f;

        _adjustedScreenResolution = new Vector2(scaleFactorX - (scaleFactorX * (1.0f - BORDER_DISPLAY_ONSCREEN_PERCENT)), scaleFactorY - (scaleFactorY * (1.0f - BORDER_DISPLAY_ONSCREEN_PERCENT)));

        Debug.Log(_adjustedScreenResolution);

        // when we have no goal resolution or when the current is so close to our current - don't adjust
        if (_goalScreenResolution == Vector2.zero)
        {
            AdjustBordersAndCamera(new Vector3(xDiff, yDiff, 0f));
            return;
        }

        // ToDo TJC: try removing the 0.5f and /2f - seems the current screen is NOT 4:3 - should be able to change to 4:3 resolution and it look 1:1 exact
        // Something to do with my scaling between is off - width reduction is too small
        // Need to test going from bigger -> smaller

        float adjustedWidth = (_adjustedScreenResolution.x - _adjustedScreenResolution.y / _goalScreenResolution.y * _goalScreenResolution.x);

        // ignore anything that has a very similar aspect ratio
        if(adjustedWidth <= 0.5f)
        {
            AdjustBordersAndCamera(new Vector3(xDiff, yDiff, 0f));
            return;
        }

        float adjustWidthPercent = adjustedWidth / _adjustedScreenResolution.x;

        // we now have our true aspect (to check, divide the _adjustedScreenResolution.x / _adjustedScreenResolution.y and check to see if the ratio is correct)
        _adjustedScreenResolution = new Vector2(_adjustedScreenResolution.x - adjustedWidth, _adjustedScreenResolution.y);

        print(BORDER_DISPLAY_ONSCREEN_PERCENT - adjustWidthPercent);

        // seems we need to substract the previous X width from these to properly be oriented (?) and shift everything?
        // appears as if both the left and right are not properly aligned to what we need to anchor to

        // if we subtract the previous border widths it should even out - why is this though? Do we need to remove the existing border from the previous setup?
        // it could be that the border from the previous is offseting things so we ONLY should do the offset originally if our aspect is 1:1
        _borders[2].transform.localPosition = new Vector3(-_borders[2].bounds.extents.x * (BORDER_DISPLAY_ONSCREEN_PERCENT - adjustWidthPercent), _borders[2].bounds.extents.y, -2f);
        _borders[3].transform.localPosition = new Vector3(topRightCorner.x + (_borders[2].bounds.extents.x * (BORDER_DISPLAY_ONSCREEN_PERCENT - adjustWidthPercent)), _borders[2].bounds.extents.y, -2f);

        resolutionScaleChange = 1.0f - (_adjustedScreenResolution.x - _goalScreenResolution.x) / _goalScreenResolution.x;

        Debug.Log(resolutionScaleChange);

        AdjustBordersAndCamera(new Vector2(scaleFactorX * (1.0f - (BORDER_DISPLAY_ONSCREEN_PERCENT - adjustWidthPercent)) * 0.5f, yDiff));

        Debug.Log(_adjustedScreenResolution + " " + _goalScreenResolution);

        Debug.Log(_adjustedScreenResolution.x / _adjustedScreenResolution.y);
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

    public int ExpandCallback(int colorIndex, ShapeScript shape)
    {
        int tmpIdx = currentZIndex;
        --currentZIndex;
        _currentColor = colorIndex;
        EvaluateWinCondition(shape);
        return tmpIdx;
    }

    public void EvaluateWinCondition(ShapeScript currentShape)
    {
        if(endConditionMet)
        {
            return;
        }

        int shapesLeft = 0;
        int moveableShapes = 0;
        bool whiteOrBlack = false;

        HashSet<int> colorsPresent = new HashSet<int>();

        foreach(ShapeScript shape in Shapes)
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

        if(shapesLeft > 1 && whiteOrBlack && moveableShapes > 0)
        {
            return;
        }

        endConditionMet = true;

        Debug.Log(shapesLeft + " " + _goalColor + " " + _currentColor);

        _youWinText.transform.parent.gameObject.SetActive(true);

        ToggleRestartUI(true);

        if(shapesLeft == 0 && _goalColor == _currentColor)
        {
            _youWinText.gameObject.SetActive(true);
            return;
        }
       
        _youLoseText.gameObject.SetActive(true);
    }

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
        Dictionary<ShapeScript, int> shapeToId = new Dictionary<ShapeScript, int>();

        foreach(ShapeScript shape in Shapes)
        {
            shapeToId.Add(shape, HelperMethods.GetNextId());
        }

        foreach(ShapeScript shape in Shapes)
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
        currentZIndex = levelData.shapes.Count * 3;
        StartCoroutine(WaitUntilStart(levelData));
    }

    private IEnumerator WaitUntilStart(SaveLoadStructures.Level levelData)
    {
        // wait until after the first update
        yield return new WaitForFixedUpdate();

        Dictionary<int, ShapeScript> idToShape = new Dictionary<int, ShapeScript>();
        HelperMethods.ResetIds();

        _startingColor = levelData.startingBackgroundColor;
        _goalColor = levelData.goalBackgroundColor;
        _roundColors = levelData.shapeColors;

        foreach (SaveLoadStructures.Shape shapeData in levelData.shapes)
        {
            int shapeIdx = HelperMethods.GetNextId();

            ShapeScript shape = Instantiate(ShapePrefab).GetComponent<ShapeScript>();
            Shapes.Add(shape);
            idToShape.Add(shapeIdx, shape);
            shape.LoadShapeData(shapeData);

            shape.transform.UpdateScaleToFitResolution(_adjustedScreenResolution, resolutionScaleChange);
            shape.SetColor(_roundColors[shapeData.colorIndex]);
            shape.SetShape(ShapeSprites[(int)shapeData.shapeIndex]);
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