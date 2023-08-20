using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MusicTypes
{
    DEFAULT_MUSIC
}

/// <summary>
/// Manages all sound / music in the scene
/// </summary>
public class SoundManager : Singleton<SoundManager>
{
    //private AudioSource _as; // Audio source on the manager
    public float masterMusicVolume = 1.0f;
    public float masterSfxVolume = 1.0f;

    // reference for our audio mixer (?) - is this even needed
    [SerializeField] private AudioSource ado = null;

    // object reference of what we are going to spawn for any new sound
    [SerializeField] private GameObject SoundObject = null;

    [Header("All sound effects used by this scene")]
    [SerializeField] private List<SoundEffects> SoundEffectObjects = new List<SoundEffects>();

    // only kept around for the town music - no reason to load it in from elsewhere
    [SerializeField] private Music[] TownMusic = new Music[2];

    // our scene music - regardless of the scene, there is at most only 2 songs playing at once
    private SceneSoundObject[] SceneMusic = new SceneSoundObject[2];

    // current sound that is played
    private Dictionary<SoundEffectID, Queue<SceneSoundObject>> SoundEffectData = new Dictionary<SoundEffectID, Queue<SceneSoundObject>>();

    // sound effect reference tie to sound effect object
    private Dictionary<SoundEffectID, SoundEffects> SceneSoundEffects = new Dictionary<SoundEffectID, SoundEffects>();

    // all current sound objects - queued to not continually delete objects
    private Queue<SceneSoundObject> OpenSoundObjects = new Queue<SceneSoundObject>();

    // determines which of the two music tracks are currently being played
    private bool playingFirstTrack = true;

    // determines if there is currently a track being faded - if there is, STOP it as the fade is occuring too quickly
    private Coroutine FadingMusic = null;

    private void Awake()
    {
        // create our reference list using the scene objects
        foreach (SoundEffects effect in SoundEffectObjects)
        {
            SceneSoundEffects.Add(effect.soundID, effect);
            SoundEffectData.Add(effect.soundID, new Queue<SceneSoundObject>());
        }

        // use this for town as the music does not change based on the course you are on
        if(TownMusic[0] != null)
            LoadMusicTracks(TownMusic[0], TownMusic[1]);
    }

    #region EVENT_FUNCTIONS
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
       
    }
    #endregion

    #region SOUND_FUNCTIONS
    /// <summary>
    /// Called when the setting manager sound volume is updated
    /// </summary>
    public void UpdateMusicVolume()
    {
        // if we are fading, do NOT update the music live!
        if (FadingMusic != null)
            return;

        //if (playingFirstTrack)
        //    SceneMusic[0].UpdateVolume(SettingsManager.Instance.MusicVolume);
        //else
        //    SceneMusic[1].UpdateVolume(SettingsManager.Instance.MusicVolume);
    }

    /// <summary>
    /// Loads music from golf course
    /// </summary>
    /// <param name="msc"></param>
    /// <param name="isHC"></param>
    public void LoadCourseMusic(in Music[] msc, bool isHC)
    {
        // music for courses is stored as follows
            // NORMAL_GOLF
            // NORMAL_BATTLE
            // HC_GOLF
            // HC_BATTLE

        // music ALWAYS starts with GOLF then fades to BATTLE
        if(isHC)
            LoadMusicTracks(msc[2], msc[3]);
        else
            LoadMusicTracks(msc[0], msc[1]);
    }

    /// <summary>
    /// Loads a track 
    /// </summary>
    /// <param name="msc"></param>
    private void LoadMusicTracks(in Music msc1, in Music msc2)
    {
        playingFirstTrack = true;

        // create our two music track objects
        SceneMusic[0] = Instantiate(SoundObject).GetComponent<SceneSoundObject>();
        SceneMusic[0].InitAudioSource(ado.outputAudioMixerGroup);

        SceneMusic[1] = Instantiate(SoundObject).GetComponent<SceneSoundObject>();
        SceneMusic[1].InitAudioSource(ado.outputAudioMixerGroup);

        // load our music into them
        SceneMusic[0].InitMusic(msc1);
        SceneMusic[1].InitMusic(msc2);

        // silence our second track as it is not the current one played
        SceneMusic[1].UpdateVolume(0);
    }

    /// <summary>
    /// Event that stops all music
    /// </summary>
    /// <param name="emptyEvent"></param>
    public void StopMusic(string emptyEvent)
    {
        //FadeTrackMusic(true, 1.25f);
    }

    /// <summary>
    /// Event called to fade music tracks from one song to the other in any scene
    /// </summary>
    /// <param name="emptyEvent"></param>
    /*public void FadeTrackMusic(bool stopMusic = false, float timeToFade = AllGameConstants.FADE_DURATION)
    {
        // deterine which track is being played by sampling which track is active vs. which track is which index wise
        if (FadingMusic != null)
            StopCoroutine(FadingMusic);

        FadingMusic = StartCoroutine(FadeMusicTrack(stopMusic, timeToFade));
    }*/

    /// <summary>
    /// Used to fade between the two active tracks - assuming every scene has two tracks
    /// </summary>
    /// <returns></returns>
    /*private IEnumerator FadeMusicTrack(bool fullStopMusic, float timeToFade = AllGameConstants.FADE_DURATION)
    {
        // grab our goal and current volume, then fade them over a set time
        float timer = 0.0f;

        float msc1Start = SceneMusic[0].GetCurrentVolume();
        float msc2Start = SceneMusic[1].GetCurrentVolume();
        float msc1End;
        float msc2End;

        if(playingFirstTrack)
        {
            msc1End = 0.0f;
            //msc2End = SceneMusic[1].GetVolume() * SettingsManager.Instance.MusicVolume;
        }
        else
        {
           //msc1End = SceneMusic[0].GetVolume() * SettingsManager.Instance.MusicVolume;
            msc2End = 0.0f;
        }

        if(fullStopMusic)
        {
            msc1End = 0.0f;
            msc2End = 0.0f;
        }

        playingFirstTrack = !playingFirstTrack;

        while (timer <= timeToFade)
        {

            //SceneMusic[0].FadeVolume(Mathf.Lerp(msc1Start, msc1End, timer / AllGameConstants.FADE_DURATION));
            //SceneMusic[1].FadeVolume(Mathf.Lerp(msc2Start, msc2End, timer / AllGameConstants.FADE_DURATION));
            timer += Time.deltaTime;
            yield return null;
        }

        //SceneMusic[0].FadeVolume(msc1End);
        //SceneMusic[1].FadeVolume(msc2End);
        yield return null;

        FadingMusic = null;
    }*/

    /// <summary>
    /// Plays a sound effect at a specific location
    /// </summary>
    /// <param name="sound"></param>
    /// <param name="location"></param>
    public void PlaySoundEffect(SoundEffectID sound, Vector3 location = default)
    {
        SceneSoundEffects.TryGetValue(sound, out SoundEffects effect);

        if(effect != null)
        {
            SceneSoundObject soundObj;

            // first determine if the current sound effect has too many sounds active
            if (effect.duplicateSounds != -1 && SoundEffectData[sound].Count > effect.duplicateSounds)
            {
                // need to clear the oldest effect and use that instead
                soundObj = SoundEffectData[sound].Dequeue();
                soundObj.RemoveSoundBeforeCompletion();
            }
            else
            {
                // check if we have another free slot or if we need to generate one
                if(OpenSoundObjects.Count > 0)
                {
                    soundObj = OpenSoundObjects.Dequeue();
                }
                else
                {
                    // spawn a new sound effect and set our callback
                    soundObj = Instantiate(SoundObject).GetComponent<SceneSoundObject>();
                    soundObj.InitAudioSource(ado.outputAudioMixerGroup);
                }
            }

            // set the location, play the sound and add it to our queue of specific sounds
            if (soundObj != null)
            {
                soundObj.transform.position = location;
                soundObj.InitSoundEffect(effect);
                SoundEffectData[sound].Enqueue(soundObj);
            }
            else
            {
                Debug.LogError("Error: Sound is unable to play - no sound object found or loaded!");
            }
        }
        else
        {
            Debug.LogError("Error: Unknown sound effect being called in scene! Name: " + sound);
        }
    }

    /// <summary>
    /// Sound of a specific type is released from it's queue
    /// </summary>
    /// <param name="sound"></param>
    public void ReleaseSoundObject(SoundEffectID sound)
    {
        OpenSoundObjects.Enqueue(SoundEffectData[sound].Dequeue());
    }

    /// <summary>
    /// Removes all active sounds of a particular sound effect
    /// </summary>
    /// <param name="sound"></param>
    public void ClearAllSoundsOfType(SoundEffectID sound)
    {
        while(SoundEffectData[sound].Count > 0)
            StartCoroutine(SlowlyFadeAndRemoveSFX(SoundEffectData[sound].Dequeue()));

        SoundEffectData[sound].Clear();
    }

    /// <summary>
    /// Gradually fades volume
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="sound"></param>
    /// <returns></returns>
    private IEnumerator SlowlyFadeAndRemoveSFX(SceneSoundObject obj)
    {
        // begin the fade transition
        obj.FadeOutSound();
        float timer = 0.0f;
        /*float startingVolume = obj.GetVolume() * SettingsManager.Instance.SFXVolume;

        while(timer <= AllGameConstants.FADE_REMOVAL)
        {
            timer += Time.deltaTime;
            obj.FadeVolume(Mathf.Lerp(startingVolume, 0.0f, timer / AllGameConstants.FADE_REMOVAL));
            yield return null;
        }*/

        obj.FadeVolume(0.0f);
        OpenSoundObjects.Enqueue(obj);
        yield return null;
    }

    /// <summary>
    /// Update the sound effect of a certain ID in the scene - mainly used for swapping the battle into sound
    /// </summary>
    /// <param name="newSound"></param>
    /// <param name="soundID"></param>
    public void UpdateSoundEffectSound(SoundEffects newSFX, SoundEffectID soundID)
    {
        SceneSoundEffects[soundID] = newSFX;
    }

    // determines if the first track is being played - used for just battle mode music
    public bool IsPlayingFirstTrack() { return playingFirstTrack; }
    #endregion
}