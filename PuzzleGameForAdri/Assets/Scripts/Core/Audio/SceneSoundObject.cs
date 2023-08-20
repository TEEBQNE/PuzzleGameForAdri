using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public enum SoundEffectID
{
    GENERIC_BUTTON
}

/// <summary>
/// Script that is spawned when a new sound effect or music is played
/// </summary>
public class SceneSoundObject : MonoBehaviour
{
    [SerializeField] private AudioSource aSrc = null;
    private SoundEffectID id;
    private float volume = 1.0f;

    private Coroutine DeleteCoroutine = null;

    public void InitAudioSource(in AudioMixerGroup mixer)
    {
        // set other permanent data on our audio source
        aSrc.outputAudioMixerGroup = mixer;
        aSrc.rolloffMode = AudioRolloffMode.Linear;
    }

    /// <summary>
    /// Initialize the object as a sound effect
    /// </summary>
    public void InitSoundEffect(SoundEffects sfx)
    {
        // set our volume
        volume = sfx.volume;
        // aSrc.volume = volume * (SettingsManager.HasInstance() ? SettingsManager.Instance.SFXVolume : 1.0f);

        // set our ID
        id = sfx.soundID;

        // set remaining sound related data
        SetGenericSoundData(sfx);
        aSrc.pitch = RandomNumber.Instance.GetRandomFloat(GetType().Name, sfx.minPitch, sfx.maxPitch);
        aSrc.clip = sfx.clips[RandomNumber.Instance.GetRandomInt(GetType().Name, 0, sfx.clips.Count)];

        // start a coroutine to remove this sound after a set duration
        //if (!sfx.loop)
        //    DeleteCoroutine = StartCoroutine(DeleteEffect(aSrc.clip.length + AllGameConstants.SOUND_DELETION_BUFFER));

        // now play our sound
        aSrc.Play();
    }

    /// <summary>
    /// Initializes the object as music
    /// </summary>
    /// <param name="music"></param>
    public void InitMusic(Music msc)
    {
        // set our volume
        volume = msc.volume;
        //aSrc.volume = volume * SettingsManager.Instance.MusicVolume;

        // set the remaining sound related data
        SetGenericSoundData(msc);
        aSrc.pitch = msc.pitch;
        aSrc.clip = msc.clips[0];

        // now play our music
        aSrc.Play();
    }

    /// <summary>
    /// Set our generic data for any sound
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="minDist"></param>
    /// <param name="maxDist"></param>
    /// <param name="vol"></param>
    /// <param name="pch"></param>
    /// <param name="lp"></param>
    /// <param name="sptlBlend"></param>
    private void SetGenericSoundData(in Sound sound)
    {
        aSrc.minDistance = sound.minDistance;
        aSrc.maxDistance = sound.maxDistance;
        aSrc.priority = sound.priority;
        aSrc.loop = sound.loop;
        aSrc.spatialBlend = sound.is3D ? 1 : 0;
    }

    /// <summary>
    /// Used to update the volume of both a sound effect / music
    /// </summary>
    /// <param name="masterVolume"></param>
    public void UpdateVolume(float masterVolume)
    {
        aSrc.volume = volume * masterVolume;
    }

    /// <summary>
    /// Update the volume of the music when a fade occurs
    /// </summary>
    /// <param name="volume"></param>
    public void FadeVolume(float volume)
    {
        aSrc.volume = volume;
    }

    /// <summary>
    /// Deletes and removes this particular sound to make room for a new sound
    /// </summary>
    public void RemoveSoundBeforeCompletion()
    {
        FadeOutSound();
        aSrc.Stop();
    }

    /// <summary>
    /// Used to quietly fade out the sound
    /// </summary>
    public void FadeOutSound()
    {
        StopCoroutine(DeleteCoroutine);
        DeleteCoroutine = null;
    }

    /// <summary>
    /// Deletes the object after a set amount of time
    /// </summary>
    /// <param name="timeToDelete"></param>
    /// <returns></returns>
    private IEnumerator DeleteEffect(float timeToDelete)
    {
        // wait the amount of time that the clip is
        yield return new WaitForSeconds(timeToDelete);

        // use our callback to let the manager know this is being deleted
        if (SoundManager.HasInstance())
            SoundManager.Instance.ReleaseSoundObject(id);

        // unset or routine and stop our current sound
        DeleteCoroutine = null;
        aSrc.Stop();
    }

    public float GetVolume() { return volume; }
    public float GetCurrentVolume() { return aSrc.volume; }
}