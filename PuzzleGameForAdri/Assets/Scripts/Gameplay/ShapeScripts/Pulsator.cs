using System.Collections;
using UnityEngine;

// ToDo TJC: Make a blend for the colors where if they are the same, they do not overlap
// match the rotation of the parent object
public class Pulsator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _render = null;
    [SerializeField] private Transform _transform = null;

    public bool IsPulsating => _pulstateRoutine != default;

    private const float COLOR_ALPHA = 0.35f;
    private const int Z_INDEX = 2;
    private const float TIME_TO_PULSE = 0.75f;
    private const float STARTING_SCALE = 0.8f;
    private const float ENDING_SCALE = 2f;

    private Color _color = Color.clear;
    private Coroutine _pulstateRoutine = null;
    private Transform _transformToFollow = null;
    private Vector3 startScale = Vector3.zero;
    private Vector3 goalScale = Vector3.zero;

    public void InitShape(Color color, Transform parent, Sprite sprite)
    {
        _transformToFollow = parent;
        _render.sprite = sprite;

        UpdateColor(color);

        startScale = new Vector3(parent.transform.localScale.x * STARTING_SCALE, parent.transform.localScale.y * STARTING_SCALE, parent.transform.localScale.z);
        goalScale = new Vector3(parent.transform.localScale.x * ENDING_SCALE, parent.transform.localScale.y * ENDING_SCALE, parent.transform.localScale.z);

        _render.color = _color;
        if (_pulstateRoutine != default)
        {
            StopCoroutine(_pulstateRoutine);
            _pulstateRoutine = default;
        }
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(_transformToFollow.position.x, _transformToFollow.position.y, Z_INDEX);
        transform.rotation = _transformToFollow.transform.rotation;
    }

    public void DestroyPulsator()
    {
        if(_pulstateRoutine != default)
        {
            StopCoroutine(_pulstateRoutine);
            _pulstateRoutine = default;
        }
       
        Destroy(gameObject);
    }

    public void StartPulsating()
    {
        if (_pulstateRoutine != default)
        {
            StopCoroutine(_pulstateRoutine);
            _pulstateRoutine = default;
        }

        _pulstateRoutine = StartCoroutine(Pulsate());
    }

    private void UpdateColor(Color color)
    {
        _color = new Color(color.r, color.g, color.b, COLOR_ALPHA);
    }

    private IEnumerator Pulsate()
    {
        float currentTime = 0.0f;

        while (currentTime <= TIME_TO_PULSE)
        {
            _transform.localScale = Vector3.Lerp(startScale, goalScale, currentTime / TIME_TO_PULSE);
            _render.color = new Color(_color.r, _color.g, _color.b, Mathf.Lerp(1.0f, 0.0f, currentTime / TIME_TO_PULSE));
            currentTime += Time.deltaTime;
            yield return null;
        }

        _render.color = Color.clear;
        _pulstateRoutine = default;
    }
}