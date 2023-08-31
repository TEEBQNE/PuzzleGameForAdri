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

    private const float BORDER_SCALE = 0.9f;
    private const float BORDER_DISPLAY_ONSCREEN_PERCENT = 0.95f;
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

        // we descend z layers here so they do not stack
        currentZIndex = Shapes.Count * 3;

        foreach(ShapeScript shape in Shapes)
        {
            shape.SetColor(_roundColors[shape.ColorIndex]);
            shape.SetCallback(ExpandCallback, GetCurrentBackground, EvaluateWinCondition);
        }
    }

    private void SetBorderScaleAndPosition()
    {
        // grab screen size
        var topRightCorner = _mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, _mainCam.transform.position.z));
        var worldSpaceWidth = topRightCorner.x * 2;
        var worldSpaceHeight = topRightCorner.y * 2;

        var spriteSize = _borders[0].bounds.size;

        // scale our X to stretch to this value
        var scaleFactorX = worldSpaceWidth / spriteSize.x;

        // scale our Y to stretch to this value
        var scaleFactorY = worldSpaceHeight / spriteSize.y;

        // top -> bottom -> left -> right
        _borders[0].transform.localScale = new Vector3(scaleFactorX, scaleFactorY * BORDER_SCALE, 1f);
        _borders[0].transform.localPosition = new Vector3(_borders[0].bounds.extents.x / 2f, topRightCorner.y + (_borders[0].bounds.extents.y * BORDER_DISPLAY_ONSCREEN_PERCENT), -2f);

        _borders[1].transform.localScale = new Vector3(scaleFactorX, scaleFactorY * BORDER_SCALE, 1f);
        _borders[1].transform.localPosition = new Vector3(_borders[0].bounds.extents.x / 2f, -_borders[0].bounds.extents.y * BORDER_DISPLAY_ONSCREEN_PERCENT, -2f);

        _borders[2].transform.localScale = new Vector3(scaleFactorY * BORDER_SCALE, scaleFactorY, 1f);
        _borders[2].transform.localPosition = new Vector3(-_borders[2].bounds.extents.x * BORDER_DISPLAY_ONSCREEN_PERCENT, _borders[2].bounds.extents.x / 2f, -2f);

        _borders[3].transform.localScale = new Vector3(scaleFactorY * BORDER_SCALE, scaleFactorY, 1f);
        _borders[3].transform.localPosition = new Vector3(topRightCorner.x + (_borders[2].bounds.extents.x * BORDER_DISPLAY_ONSCREEN_PERCENT), _borders[2].bounds.extents.x / 2f, -2f);
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
        // due to camera being bottom left aligned the size is the screen in world space
        float height = _mainCam.orthographicSize * 2;
        float width = height * _mainCam.aspect;

        return new SaveLoadStructures.Level(GetShapes(), _roundColors, _startingColor, _goalColor, new Vector2(width, height));
    }

    private List<SaveLoadStructures.Shape> GetShapes()
    {
        List<SaveLoadStructures.Shape> allShapeData = new List<SaveLoadStructures.Shape>();
        HelperMethods.ResetIds();

        float height = _mainCam.orthographicSize * 2;
        float width = height * _mainCam.aspect;

        // create an id match dictionary here
        Dictionary<ShapeScript, int> shapeToId = new Dictionary<ShapeScript, int>();

        foreach(ShapeScript shape in Shapes)
        {
            shapeToId.Add(shape, HelperMethods.GetNextId());
        }

        foreach(ShapeScript shape in Shapes)
        {
            SaveLoadStructures.Shape shapeData = shape.SaveShapeData(shapeToId);
            shapeData.position = new Vector2(shapeData.position.x / width, shapeData.position.y / height);
            shapeData.scale = new Vector2(shapeData.scale.x / width, shapeData.scale.y / height);
            allShapeData.Add(shapeData);
        }

        return allShapeData;
    }

    public void LoadLevelData(SaveLoadStructures.Level levelData)
    {
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

            // as the camera is aligned at (0,0) we will use the camera size in world space as its height / width
            float height = _mainCam.orthographicSize * 2;
            float width = height * _mainCam.aspect;

            shape.transform.UpdateScaleToFitResolution(_mainCam, levelData.screenResolution, new Vector2(width, height));
            shape.SetColor(_roundColors[shapeData.colorIndex]);
            shape.SetShape(ShapeSprites[(int)shapeData.shapeIndex]);

            // we set the shapesIdx here to the last idx to use later
            shapeData.childShapes.Add(shapeIdx);
        }

        // iterate all shape data creating a dictionary 

        foreach(SaveLoadStructures.Shape shapeData in levelData.shapes)
        {
            if(shapeData.childShapes == default || shapeData.childShapes.Count == 1)
            {
                continue;
            }

            // the actual shape idx is stored as the last idx
            int shapeIdx = shapeData.childShapes[shapeData.childShapes.Count - 1];

            for(int x = 0; x < shapeData.childShapes.Count - 1; ++x)
            {
                // we store the data as global so we keep its world position, rotation, etc. 
                idToShape[shapeData.childShapes[x]].transform.SetParent(idToShape[shapeIdx].transform, true);
            }
        }
    }
    #endregion
}