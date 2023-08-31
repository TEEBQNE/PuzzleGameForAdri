using UnityEngine;

public class CameraAdjustment : MonoBehaviour
{
    [SerializeField] private Camera _mainCam = null;

    void Awake()
    {
        float aspectRatio = _mainCam.aspect;        // (width divided by height)
        float camSize = _mainCam.orthographicSize;
        float correctPositionX = aspectRatio * camSize;
        transform.position = new Vector3(correctPositionX, camSize, transform.position.z);
    }
}