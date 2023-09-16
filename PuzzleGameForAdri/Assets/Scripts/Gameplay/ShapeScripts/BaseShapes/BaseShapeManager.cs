using System.Collections.Generic;
using UnityEngine;

public enum ShapeNames
{
    SQUARE,
    CIRCLE,
    DIAMOND
}

public abstract class BaseShapeManager : MonoBehaviour
{
    #region Constants
    protected const float BORDER_DISPLAY_ONSCREEN_PERCENT = 0.9f;
    public const int WHITE_INDEX = 0;
    public const int BLACK_INDEX = 1;
    #endregion

    #region SerializedFields
#if UNITY_EDITOR
    [NamedArray(typeof(ShapeNames))]
#endif
    [SerializeField] protected List<Sprite> _shapeSprites = new List<Sprite>();

    [SerializeField] protected List<Color> _roundColors = new List<Color>() { Color.white, Color.black };
    [SerializeField] protected List<SpriteRenderer> _borders = new List<SpriteRenderer>();
    [SerializeField] protected GameObject ShapePrefab = null;
    [SerializeField] protected Camera _mainCam = null;
    #endregion

    #region Variables
    protected List<GameShape> _shapeScripts = new List<GameShape>();

    protected int _goalColor = 1;
    protected int _startingColor = 0;
    protected int _currentColor = 0;
    protected Vector3 topRightCorner;
    protected float aspect;
    protected float worldSpaceHeight;
    protected float worldSpaceWidth;
    protected float scaleFactorX;
    protected float scaleFactorY;
    #endregion

    #region Init / Start
    protected virtual void Start()
    {
        _currentColor = _startingColor;
        _mainCam.backgroundColor = _roundColors[_currentColor];

        foreach (SpriteRenderer rend in _borders)
        {
            rend.color = _roundColors[_goalColor];
        }

        SetBorderScaleAndPosition();
    }

    protected virtual void SetBorderScaleAndPosition()
    {
        // grab screen size
        topRightCorner = _mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, _mainCam.transform.position.z));

        aspect = (float)Screen.width / Screen.height;
        worldSpaceHeight = _mainCam.orthographicSize * 2;
        worldSpaceWidth = worldSpaceHeight * aspect;

        var spriteSize = _borders[0].bounds.size;

        // scale our X to stretch to this value
        scaleFactorX = worldSpaceWidth / spriteSize.x;

        // scale our Y to stretch to this value
        scaleFactorY = worldSpaceHeight / spriteSize.y;

        // top -> bottom -> left -> right
        _borders[0].transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
        _borders[0].transform.localPosition = new Vector3(_borders[0].bounds.extents.x, topRightCorner.y + (_borders[0].bounds.extents.y * BORDER_DISPLAY_ONSCREEN_PERCENT), -2f);

        _borders[1].transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
        _borders[1].transform.localPosition = new Vector3(_borders[1].bounds.extents.x, -_borders[1].bounds.extents.y * BORDER_DISPLAY_ONSCREEN_PERCENT, -2f);

        _borders[2].transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
        _borders[2].transform.localPosition = new Vector3(-_borders[2].bounds.extents.x * BORDER_DISPLAY_ONSCREEN_PERCENT, _borders[2].bounds.extents.y, -2f);

        _borders[3].transform.localScale = new Vector3(scaleFactorX, scaleFactorY, 1f);
        _borders[3].transform.localPosition = new Vector3(topRightCorner.x + (_borders[3].bounds.extents.x * BORDER_DISPLAY_ONSCREEN_PERCENT), _borders[3].bounds.extents.y, -2f);
    }
    #endregion
}