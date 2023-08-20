using UnityEngine;

/// <summary>
/// Class that holds data relevent to any sound effect in the game
/// </summary>
[CreateAssetMenu(fileName = "New Sound Effect", menuName = "Sound/Create a new Sound Effect")]
public class SoundEffects : Sound
{
    [Header("Sound Effect Settings")]
    [Tooltip("ID that identifies this sound effect")]
    public SoundEffectID soundID = SoundEffectID.GENERIC_BUTTON;

    [Tooltip("Minimum possible pitch")]
    public float minPitch = 1;

    [Tooltip("Max possible pitch")]
    public float maxPitch = 1;

    [Tooltip("Number of this sound that can exist (-1 is inf)")]
    public int duplicateSounds = -1;
}