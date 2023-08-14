using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShapeManager : MonoBehaviour
{
    [SerializeField] private List<ShapeScript> Shapes = new List<ShapeScript>();
    [SerializeField] private GameObject _youWinText = null;
    [SerializeField] private GameObject _youLoseText = null;
    [SerializeField] private Camera _mainCam = null;
    [SerializeField] private GameObject _replayUI = null;

    [SerializeField] private int _goalColor = -1;
    [SerializeField] private int _currentColor = -1;
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
        _borders[0].transform.localPosition = new Vector3(0f, topRightCorner.y + (_borders[0].bounds.extents.y * BORDER_DISPLAY_ONSCREEN_PERCENT), -2f);

        _borders[1].transform.localScale = new Vector3(scaleFactorX, scaleFactorY * BORDER_SCALE, 1f);
        _borders[1].transform.localPosition = new Vector3(0f, -topRightCorner.y - (_borders[0].bounds.extents.y * BORDER_DISPLAY_ONSCREEN_PERCENT), -2f);

        _borders[2].transform.localScale = new Vector3(scaleFactorY * BORDER_SCALE, scaleFactorY, 1f);
        _borders[2].transform.localPosition = new Vector3(-topRightCorner.x - (_borders[2].bounds.extents.x * BORDER_DISPLAY_ONSCREEN_PERCENT), 0f, -2f);

        _borders[3].transform.localScale = new Vector3(scaleFactorY * BORDER_SCALE, scaleFactorY, 1f);
        _borders[3].transform.localPosition = new Vector3(topRightCorner.x + (_borders[2].bounds.extents.x * BORDER_DISPLAY_ONSCREEN_PERCENT), 0f, -2f);
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
            if(shape == currentShape)
            {
                continue;
            }

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
}