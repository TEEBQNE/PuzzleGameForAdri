using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    #region SerializedFields
    [SerializeField] private Camera _mainCam;
    [SerializeField] private float _increaseSpeed = 50f;
    #endregion


    #region Variables
    private Rigidbody2D _activeRb = default;
    private Collider2D _activeCollider = default;
    private Vector3 _offset = Vector2.zero;
    #endregion

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            DetermineMouseClick();
            return;
        }

        if(Input.GetMouseButtonUp(0))
        {
            _activeRb = default;
            _activeCollider = default;
        }
    }

    // ToDo TJC: Determine if this works for mobile (iOS and Android)
    private void DetermineMouseClick()
    {
        if(_activeRb != default)
        {
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(GetMousePosition(), Vector2.zero);

        if(hit == default || hit.collider.tag != "Shape")
        {
            _activeRb = default;
            _activeCollider = default;
            return;
        }

        // ToDo TJC: Need to determine if we are editing or actually playing - or do we?
        BaseShape shape = hit.collider.GetComponent<BaseShape>();
        if (shape == default || !shape.CanBeMoved)
        {
            _activeRb = default;
            _activeCollider = default;
            return;
        }

        _activeRb = hit.collider.attachedRigidbody;
        _activeCollider = hit.collider;
        _offset = hit.point - (Vector2)hit.transform.position;
    }

    private void FixedUpdate()
    {
        if(_activeRb == default || _activeCollider == default || _activeCollider.isTrigger)
        {
            _activeRb = default;
            _activeCollider = default;
            return;
        }

        Vector3 newPos = GetMousePosition() - _activeRb.transform.position - _offset;
        newPos.z = _activeRb.transform.position.z;

        _activeRb.velocity = newPos.normalized * newPos.magnitude * _increaseSpeed;
    }

    private Vector3 GetMousePosition()
    {
        return _mainCam.ScreenToWorldPoint(Input.mousePosition);
    }
}