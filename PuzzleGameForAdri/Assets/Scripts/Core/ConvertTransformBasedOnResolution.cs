using UnityEngine;

public static class TransformExtensions
{
    public static void UpdateScaleToFitResolution(this Transform transform, Vector2 currentResolution)
    {
        float currentXPos = transform.position.x * currentResolution.x;
        float currentYPos = transform.position.y * currentResolution.y;

        float currentXScale = transform.lossyScale.x * currentResolution.x;
        float currentYScale = transform.lossyScale.y * currentResolution.y;

        transform.position = new Vector3(currentXPos, currentYPos, transform.position.z);
        transform.localScale = new Vector3(currentXScale, currentYScale, 1f);
    }
}