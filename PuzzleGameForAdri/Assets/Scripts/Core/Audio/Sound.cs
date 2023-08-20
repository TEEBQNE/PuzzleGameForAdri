using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class of all sounds - music AND sound effects derive from this script
/// </summary>
public class Sound : ScriptableObject
{
    [Header("Generic Sound Settings")]
    [Tooltip("SFX - Random Picked / Music - Start -> Alt")]
    public List<AudioClip> clips;
    [Tooltip("Should the clip loop")]
    public bool loop = false;
    [Tooltip("Priority of sound")]
    public int priority = 128;
    [Tooltip("Volume to play at")]
    public float volume = 1;

    [Header("3D Sound Settings")]
    [Tooltip("Should this be a 3D sound")]
    public bool is3D;
    [Tooltip("Minimum distance for 3D sound settings")]
    public float minDistance = 1;
    [Tooltip("Maximum distance for 3D sound settings")]
    public float maxDistance = 500;
}