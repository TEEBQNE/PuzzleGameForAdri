using UnityEngine;

public static class TransformExtensions
{
    public static void UpdateScaleToFitResolution(this Transform transform, Vector2 currentResolution, float scaleChange)
    {
        float currentXPos = transform.position.x * currentResolution.x * scaleChange;
        float currentYPos = transform.position.y * currentResolution.y * scaleChange;

        float currentXScale = transform.lossyScale.x * currentResolution.x * scaleChange;
        float currentYScale = transform.lossyScale.y * currentResolution.y * scaleChange;

        transform.position = new Vector3(currentXPos, currentYPos, transform.position.z);
        transform.localScale = new Vector3(currentXScale, currentYScale, 1f);
    }
}