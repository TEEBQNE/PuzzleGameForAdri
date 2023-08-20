using UnityEngine;

/// <summary>
/// Class that hold data relevent to any music in the game
/// </summary>
[CreateAssetMenu(fileName = "New Music Clip", menuName = "Sound/Create a new Music Clip")]
public class Music : Sound
{
    [Header("Music Settings")]
    [Tooltip("Pitch of music clips")]
    public float pitch = 1;
}