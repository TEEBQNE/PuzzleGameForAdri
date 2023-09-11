using UnityEngine;

public abstract class BaseShape : MonoBehaviour
{
    #region Constants
    protected const int MAX_SCALE = 50;
    protected Vector3 MAX_SCALE_VECTOR = new Vector3(MAX_SCALE, MAX_SCALE, MAX_SCALE);
    protected const float SHRINK_SCALE_DOWN = 0.9f;
    protected const float TIME_TO_SHRINK_AND_GROW = 0.5f;
    protected const float TIME_BETWEEN_PULSATE = 0.45f;
    #endregion

    #region SerializedFields
    [SerializeField] protected SpriteRenderer _render = null;
    [SerializeField] protected Rigidbody2D _rb = null;
    [SerializeField] protected PolygonCollider2D _collider = null;
    [SerializeField] protected GameObject _pulsatingPrefab = null;
    #endregion

    #region Variables
    protected int _colorIndex = 0;
    protected ShapeNames _shapeIndex;
    protected bool _canBeMoved = true;
    protected Color _shapeColor = Color.white;

#if UNITY_EDITOR
    protected bool _isEditor = false;
#endif
    #endregion

    #region Properties
    public Color ShapeColor => _shapeColor;
    public int ColorIndex => _colorIndex;
    public bool CanBeMoved => _canBeMoved;
    #endregion

    #region Start / Init
    protected virtual void Start()
    {
#if UNITY_EDITOR
        if (_isEditor)
        {
            TogglePhysics(false);
            return;
        }
#endif


    }
    #endregion

    #region Update Physics
    public void TogglePhysics(bool toggle)
    {
        if (!toggle)
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

    #region Change Color / Change Shape
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

    public void SetShape(Sprite sprite)
    {
        _render.sprite = sprite;
        _collider.TryUpdateShapeToAttachedSprite();
    }
    #endregion
}