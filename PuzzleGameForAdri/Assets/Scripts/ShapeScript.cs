using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ToDo:
/// Add a canMove flag where a player can or can't move an object
///     > Add a condition that determines if no more shapes can move in the winCondition
///     > Add a pulsating circle around the object at a scale slightly bigger than it
///     > Bounce / stretch animation 
/// 
/// Add a reset for the level when losing / a UI to reset at anypoint
///
/// Have the outer blocks scale to the outline instead of placing them
///
/// Add new shapes / check polygon colliders for accuracy (look for an online pack to keep scales equal)
///
/// Design a level editor
///     > Shape selection
///     > Allow prefabs (scale / color)
///     > List selection as well as screen selection
///     > When selecting add an outline, add sliders for scale / rotation
///     > Create a color pallete (Default the white / black without allow changes - do not allow these to be other colors)
///     > Delete / move shapes
///     > Childing objects and lists
///     > Disable colliders at the time of editing (use triggers)
///         > If an OnTriggerStay is activated display an issue (as they should not start overlapping)
///
/// Level pack / creator
///     > Create a name of a pack
///     > Load the pack / levels, add / move / remove levels from pack
///     > Add / remove packs (No duplicate names - append a number each time this occurs or just add a unique id)
///
/// Save / Load levels / packs / serialize this data out
///     > Send the data as json - properly serialize / deserialize the color, id, positions, shapes, shape ids, etc.
///
/// Title screen / load level / level editor
///     > Display level completions in each pack
/// Bug:
/// Black or white shape hitting two shapes at once triggers them both
///     > It should not work this way (It should only destroy one)
/// Randomly the backgroundColor / shape index or scale will reset
/// </summary>

public class ShapeScript : MonoBehaviour
{
    // ToDo TJC: Remove this as a serialize and have this be loaded
    [SerializeField] private int _colorIndex = 0;
    [SerializeField] private SpriteRenderer _render = null;
    [SerializeField] private Rigidbody2D _rb = null;
    [SerializeField] private PolygonCollider2D _collider = null;
    [SerializeField] private GameObject _pulsatingPrefab = null;
    [SerializeField] private bool _canBeMoved = true;

    private const int MAX_SCALE = 50;
    private Vector3 MAX_SCALE_VECTOR = new Vector3(MAX_SCALE, MAX_SCALE, MAX_SCALE);
    private const float SHRINK_SCALE_DOWN = 0.9f;
    private const float TIME_TO_SHRINK_AND_GROW = 0.5f;
    private const float TIME_BETWEEN_PULSATE = 0.45f;

    public Color ShapeColor => _shapeColor;
    public bool IsPhysicsEnabled => _rb.simulated;
    public int ColorIndex => _colorIndex;
    public bool IsExpanding => _changingScaleRoutine != default;
    public bool CanBeMoved => _canBeMoved;


    private float _timeToExpand = 1.5f;
    private float _timeToShrink = 0.5f;
    private Coroutine _changingScaleRoutine = null;
    private Coroutine _bounceStretchAndPulsateRoutine = null;
    private Color _shapeColor = Color.white;
    private Pulsator _pulsator = null;

    // list of shapes that should be removed once the expansion is finished
    private List<ShapeScript> shapesToRemove = new List<ShapeScript>();
    public delegate int GetZIndex(int colorIndex, ShapeScript shape);
    private GetZIndex _getZIndex = default;
    public delegate int GetCurrentBackground();
    private GetCurrentBackground _getCurrentBackground;
    public delegate void CheckWinCondition(ShapeScript shape);
    private CheckWinCondition _checkWinCondition;

    #region Init
    private void Start()
    {
        if (transform.parent.GetComponent<ShapeScript>() != default)
        {
            TogglePhysics(false);

            // assure each childed shape is on top of the parent
            transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
            return;
        }
        
        // ToDo TJC: Should children shapes inherit their parent moveable status?

        InitializePulsator();
    }

    public void SetCallback(GetZIndex getZIndex, GetCurrentBackground getCurrentBackground, CheckWinCondition checkWinCondition)
    {
        _getZIndex = getZIndex;
        _getCurrentBackground = getCurrentBackground;
        _checkWinCondition = checkWinCondition;
    }

    private void InitializePulsator()
    {
        if(!_canBeMoved)
        {
            return;
        }

        if(_pulsator != default)
        {
            _pulsator.DestroyPulsator();
            return;
        }

        _pulsator = Instantiate(_pulsatingPrefab).GetComponent<Pulsator>();
        _pulsator.InitShape(_shapeColor, transform, _render.sprite);
        _bounceStretchAndPulsateRoutine = StartCoroutine(StretchBounceAndPulse());
    }
    #endregion

    #region Collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_changingScaleRoutine != default)
        {
            return;
        }

        if (collision.gameObject.tag == "Shape")
        {
            ShapeScript shapeScript = collision.gameObject.GetComponent<ShapeScript>();

            if (shapeScript == default)
            {
                return;
            }

            HandleColorInteraction(shapeScript);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Shape")
        {
            ShapeScript shapeScript = collision.gameObject.GetComponent<ShapeScript>();

            if (shapeScript == default)
            {
                return;
            }

            if (_colorIndex != shapeScript.ColorIndex)
            {
                return;
            }

            if(shapeScript.IsExpanding)
            {
                return;
            }

            shapeScript.RemoveShape();
            shapeScript.TogglePhysics(false);
            shapesToRemove.Add(shapeScript);
        }
    }
    #endregion

    #region Scaling
    public void StartScaling(bool isGrowing)
    {
        RemoveShape(isGrowing);

        if (isGrowing)
        {
            _collider.isTrigger = true;
            _collider.enabled = true;
            _rb.simulated = true;
        }

        _changingScaleRoutine = StartCoroutine(ChangeShape(transform.localScale, isGrowing ? MAX_SCALE_VECTOR : Vector3.zero, isGrowing ? _timeToExpand : _timeToShrink));
    }

    private IEnumerator ChangeShape(Vector3 startScale, Vector3 endScale, float timeToChange)
    {
        yield return null;

        float currentTime = 0;

        ZeroOutPhysics();

        while (currentTime < timeToChange)
        {
            transform.localScale = new Vector3(Mathf.Lerp(startScale.x, endScale.x, currentTime / timeToChange), Mathf.Lerp(startScale.y, endScale.y, currentTime / timeToChange), startScale.z);
            currentTime += Time.deltaTime;
            yield return null;
        }

        _changingScaleRoutine = null;

        foreach (var shape in shapesToRemove)
        {
            // make shape invisible
            shape.SetColor(Color.clear);
        }

        shapesToRemove.Clear();
        TogglePhysics(false);
        _checkWinCondition.Invoke(this);
    }

    /// <summary>
    /// Used to perform the pulsate on the pulse object + bounce and stretch the parent object
    /// </summary>
    /// <returns></returns>
    private IEnumerator StretchBounceAndPulse()
    {
        // shrink
        float currentTime = 0.0f;
        Vector3 originalScale = transform.localScale;
        Vector3 shrinkScale = new Vector3(originalScale.x * SHRINK_SCALE_DOWN, originalScale.y * SHRINK_SCALE_DOWN, originalScale.z);

        while(true)
        {
            while (currentTime <= TIME_TO_SHRINK_AND_GROW)
            {
                transform.localScale = Vector3.Lerp(originalScale, shrinkScale, currentTime / TIME_TO_SHRINK_AND_GROW);
                currentTime += Time.deltaTime;
                yield return null;
            }

            currentTime = 0.0f;
            _pulsator.StartPulsating();

            while (currentTime <= TIME_TO_SHRINK_AND_GROW)
            {
                transform.localScale = Vector3.Lerp(shrinkScale, originalScale, currentTime / TIME_TO_SHRINK_AND_GROW);
                currentTime += Time.deltaTime;
                yield return null;
            }

            yield return new WaitUntil(() => !_pulsator.IsPulsating);

            transform.localScale = originalScale;

            yield return new WaitForSeconds(TIME_BETWEEN_PULSATE);
        }
    }
    #endregion

    #region Clean Up
    public void RemoveShape(bool isGrowing = false)
    {
        _rb.freezeRotation = true;

        if (_bounceStretchAndPulsateRoutine != default)
        {
            StopCoroutine(_bounceStretchAndPulsateRoutine);
            _bounceStretchAndPulsateRoutine = null;
        }

        if (_pulsator != default)
        {
            _pulsator.DestroyPulsator();
            _pulsator = default;
        }

        // move our shape behind
        transform.position = new Vector3(transform.position.x, transform.position.y, _getZIndex.Invoke(isGrowing ? _colorIndex : _getCurrentBackground(), this));

        if (transform.childCount == 0)
        {
            return;
        }

        List<ShapeScript> shapes = new List<ShapeScript>();

        foreach (Transform child in transform)
        {
            ShapeScript shape = child.GetComponent<ShapeScript>();
            shapes.Add(shape);

            shape.TogglePhysics(true);
            child.parent = transform.parent;
            child.transform.position = new Vector3(child.transform.position.x, child.transform.position.y, 0f);
        }

        foreach (ShapeScript shape in shapes)
        {
            if(shape.CheckBackgroundColor())
            {
                continue;
            }

            shape.InitializePulsator();
        }
    }
    #endregion

    #region Change Color
    private void HandleColorInteraction(ShapeScript otherShapeScript)
    {
        if (otherShapeScript.ColorIndex == ShapeManager.BLACK_INDEX || _colorIndex == ShapeManager.BLACK_INDEX)
        {
            // to avoid a block hitting to blocks
            if (otherShapeScript.IsExpanding || IsExpanding)
            {
                return;
            }

            TogglePhysics(false);
            StartScaling(false);

            otherShapeScript.TogglePhysics(false);
            otherShapeScript.StartScaling(false);
            return;
        }

        if (otherShapeScript.ColorIndex == ShapeManager.WHITE_INDEX)
        {
            otherShapeScript.SetColor(_shapeColor, _colorIndex);
        }

        if (_colorIndex == ShapeManager.WHITE_INDEX)
        {
            SetColor(otherShapeScript.ShapeColor, otherShapeScript.ColorIndex);
        }

        if (_colorIndex == otherShapeScript.ColorIndex)
        {
            // expand shape outward
            StartScaling(true);
            otherShapeScript.StartScaling(true);
        }
    }

    public bool CheckBackgroundColor()
    {
        if (_getCurrentBackground.Invoke() != _colorIndex)
        {
            return false;
        }

        RemoveShape(true);
        TogglePhysics(false);
        return true;
    }

    public void SetColor(Color color, int overrideIndex = -1)
    {
        _shapeColor = color;
        _render.color = _shapeColor;

        if (overrideIndex == -1)
        {
            return;
        }

        _colorIndex = overrideIndex;
    }
    #endregion

    #region Update Physics
    public void TogglePhysics(bool toggle)
    {
        if(!toggle)
        {
            ZeroOutPhysics();
        }

        _rb.simulated = toggle;
        _collider.enabled = toggle;
    }

    public void ZeroOutPhysics()
    {
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0.0f;
    }
    #endregion
}