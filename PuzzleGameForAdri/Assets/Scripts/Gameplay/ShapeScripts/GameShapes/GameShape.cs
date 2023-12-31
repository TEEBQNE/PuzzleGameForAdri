using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* General game flow
        Load app
            Set a username (generate a guid for the user + cache their screen resolution (this 'should' never change))
            Reload the last level they had loaded (not exact state, just reload it) / based on pack guid / level index
            Load homescreen
                Settings
                Level creator
                    Shows all authored local packs (based on guid) - can load an existing level pack then update it
                    Create a new pack
                    Delete an existing pack
                Level selector
                    Shows all packs (authored or generated from sent info) - overall pack displays completion status
                        Selecting a pack displays all levels and their completion status
                Load pack data
                    'New' packs or can load in an existing guid which will warn the player it will overwrite an existing save
                     Should dynamically save the packData as guid.txt and have a separate file that uses the readable name + other data
                        Guid text file saves the big chunky level data that we only need when we play that pack
 */

/// <summary>
/// ToDo:
///
/// Do the rest of the flow + save / load for packs first (?) - prolly this
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
/// 
/// Save out resolution a level / pack was made on and scale the level to the new device (Stretch / squeeze objects) - do I want this? Maybe a stretch?
///
/// Level pack / creator
///     > Create a name of a pack
///     > Load the pack / levels, add / move / remove levels from pack
///     > Add / remove packs (No duplicate names - append a number each time this occurs or just add a unique id)
///
///
/// Title screen / load level / level editor
///     > Display level completions in each pack
///     
/// Bugs:
///
/// If you start a color out as the goal, the shape does not get destroyed (It probably should just auto delete)
/// 
/// Sometimes you get a lose state when you win (objects are not being removed fully - usually small yellow ones)
///
/// /// Future ToDo:
/// Determine a good way for the pause menu to be opened (Maybe double tap screen? Hold? Press outline?)
///
/// /// Add new shapes / check polygon colliders for accuracy (look for an online pack to keep scales equal) - Need a lot of uniform shapes for this)
/// </summary>

public class GameShape : BaseShape
{
    #region Variables
    private Pulsator _pulsator = null;

    private float _timeToExpand = 1.5f;
    private float _timeToShrink = 0.5f;

    private Coroutine _changingScaleRoutine = null;
    private Coroutine _bounceStretchAndPulsateRoutine = null;

    // callbacks
    private GetZIndex _getZIndex = default;
    private GetCurrentBackground _getCurrentBackground = default;
    private CheckWinCondition _checkWinCondition = default;

    // list of shapes that should be removed once the expansion is finished
    private List<GameShape> shapesToRemove = new List<GameShape>();
    #endregion

    #region Properties
    public bool IsPhysicsEnabled => _rb.simulated;
    public bool IsExpanding => _changingScaleRoutine != default;
    #endregion

    #region Events
    public delegate int GetZIndex(int colorIndex, GameShape shape);
    public delegate int GetCurrentBackground();
    public delegate void CheckWinCondition(GameShape shape);
    #endregion

    #region Init
    protected override void Start()
    {
        base.Start();

#if UNITY_EDITOR
        if(_isEditor)
        {
            return;
        }
#endif

        if (transform.parent != default && transform.parent.GetComponent<GameShape>() != default)
        {
            TogglePhysics(false);

            // assure each childed shape is on top of the parent
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -1f);
            return;
        }

        TogglePhysics(true);
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
            GameShape shapeScript = collision.gameObject.GetComponent<GameShape>();

            if (shapeScript == default)
            {
                return;
            }

            // don't collide with an already expanded shape nor one that is expanding
            if(IsExpanding || shapeScript.IsExpanding)
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
            GameShape shapeScript = collision.gameObject.GetComponent<GameShape>();

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

        List<GameShape> shapes = new List<GameShape>();

        foreach (Transform child in transform)
        {
            if(child == transform)
            {
                continue;
            }

            GameShape shape = child.GetComponent<GameShape>();
            shapes.Add(shape);

            shape.TogglePhysics(true);
            child.parent = transform.parent;
            child.transform.position = new Vector3(child.transform.position.x, child.transform.position.y, 0f);
        }

        foreach (GameShape shape in shapes)
        {
            if(shape.CheckBackgroundColor())
            {
                continue;
            }

            shape.InitializePulsator();
        }
    }
    #endregion

    #region Change Color / Change Shape
    private void HandleColorInteraction(GameShape otherShapeScript)
    {
        if (otherShapeScript.ColorIndex == BaseShapeManager.BLACK_INDEX || _colorIndex == BaseShapeManager.BLACK_INDEX)
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

        if (otherShapeScript.ColorIndex == BaseShapeManager.WHITE_INDEX)
        {
            otherShapeScript.SetColor(_shapeColor, _colorIndex);
        }

        if (_colorIndex == BaseShapeManager.WHITE_INDEX)
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
    #endregion

    #region Save / Load
    public SaveLoadStructures.Shape SaveShapeData(Dictionary<GameShape, int> idRefs)
    {
        return new SaveLoadStructures.Shape(GetChildShapes(idRefs), transform.position, transform.lossyScale, transform.rotation, _colorIndex, _shapeIndex, _canBeMoved);
    }

    private List<int> GetChildShapes(Dictionary<GameShape, int> idRefs)
    {
        List<int> shapes = new List<int>();

        foreach(Transform trans in transform)
        {
            GameShape shape = trans.GetComponent<GameShape>();

            if(shape == default)
            {
                continue;
            }

            shapes.Add(idRefs[shape]);
        }

        return shapes;
    }

    public void LoadShapeData(SaveLoadStructures.Shape shapeData)
    {
        transform.position = shapeData.position;
        transform.localScale = shapeData.scale;
        transform.rotation = shapeData.rotation;

        _canBeMoved = shapeData.canBeMoved;
        _colorIndex = shapeData.colorIndex;
        _shapeIndex = shapeData.shapeIndex;
    }
    #endregion
}