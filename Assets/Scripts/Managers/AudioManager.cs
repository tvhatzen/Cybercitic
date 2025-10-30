using UnityEngine;

// dynamically add audio listener component to anything using the PlaySound / music methods
// will need a transform as well to place the source

public class AudioManager : SingletonBase<AudioManager>
{
    #region Clips

    [Header("Audio Settings")]
    [SerializeField] private AudioSource musicSource; // For background music
    [SerializeField] private AudioSource sfxSource; // everything else

    [SerializeField] private AudioClip pickUpCurrency;
    [SerializeField] private AudioClip enterLevel;
    [SerializeField] private AudioClip exitLevel;
    [SerializeField] private AudioClip levelTransition;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip floor1_5;
    [SerializeField] private AudioClip floor6_10;
    [SerializeField] private AudioClip floor11_15;

    [Header("UI")]
    [SerializeField] private AudioClip uiHover;
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip uiNotEnoughCredits;
    [SerializeField] private AudioClip uiPurchaseUpgrade;

    [Header("Screens")]
    [SerializeField] private AudioClip mainMenu;
    [SerializeField] private AudioClip upgradeScreen;
    [SerializeField] private AudioClip winScreen;

    [Header("Feedback")]
    [SerializeField] private AudioClip takeDamage;

    [Header("Player")]
    [SerializeField] private AudioClip attack;
    [SerializeField] private AudioClip damaged;
    [SerializeField] private AudioClip die;
    [SerializeField] private AudioClip useSkill;

    [Header("Enemy")]
    [SerializeField] private AudioClip enemyAttack;
    [SerializeField] private AudioClip enemyDamaged;
    [SerializeField] private AudioClip enemyDie;
    [SerializeField] private AudioClip enemySpawned;

    #endregion

    public bool debug = false;

    // PlayerPrefs keys
    private const string MusicVolumeKey = "audio_music_volume";
    private const string SfxVolumeKey = "audio_sfx_volume";

    private void Start()
    {
        GameEvents.OnSoundRequested += HandleSoundRequest;

        // Ensure sources are 2D so SFX/Music are not distance-attenuated
        if (musicSource != null) musicSource.spatialBlend = 0f;
        if (sfxSource != null) sfxSource.spatialBlend = 0f;
        if (debug) Debug.Log("[AudioManager] Initialized audio sources as 2D (spatialBlend=0)");
    }

    private void OnDestroy()
    {
        GameEvents.OnSoundRequested -= HandleSoundRequest;
    }

    // event Bus handlers
    private void HandleSoundRequest(string soundName)
    {
        PlaySound(soundName);
    }
    
    // play music
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true; // loop the background music
            musicSource.Play();
            if (debug) Debug.Log($"[AudioManager] Now playing music clip: {clip.name}");
        }
    }

    // play music by string name 
    public void PlayMusicTrack(string trackName)
    {
        if (debug) Debug.Log($"[AudioManager] Requested music track: {trackName}");
        AudioClip clip = GetSoundClip(trackName);
        if (clip != null)
        {
            PlayMusic(clip);
        }
        else
        {
            if (debug) Debug.LogWarning($"[AudioManager] No AudioClip found for track: {trackName} (check naming and inspector assignments)");
        }
    }

    // play sound by AudioClip
    public void PlaySound(AudioClip clip)
    {
        if (sfxSource == null)
        {
            if (debug) Debug.LogError("[AudioManager] SFX AudioSource is null - cannot play sound");
            return;
        }

        if (clip == null)
        {
            if (debug) Debug.LogWarning("[AudioManager] Tried to play null AudioClip");
            return;
        }

        if (debug)
        {
            Debug.Log($"[AudioManager] PlayOneShot: {clip.name} | vol={sfxSource.volume:0.###} | mute={sfxSource.mute}");
        }

        if(sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
            //Debug.Log("AudioManager: playing sound - " + clip.name);
        }
    }

    // play sound by string name 
    public void PlaySound(string soundName)
    {
        if (debug) Debug.Log($"[AudioManager] Request PlaySound: {soundName}");
        AudioClip clip = GetSoundClip(soundName);
        if (clip != null)
        {
            PlaySound(clip);
        }
        else
        {
            if (debug) Debug.LogWarning($"[AudioManager] No clip mapped for sound key: {soundName}");
        }
    }

    // get AudioClip by string name
    private AudioClip GetSoundClip(string soundName)
    {
        switch (soundName.ToLower())
        {
            // player sounds
            case "attack":
                return attack;
            case "damaged":
                return damaged;
            case "die":
                return die;
            case "useskill":
                return useSkill;
            
            // enemy sounds
            case "enemyattack":
                return enemyAttack;
            case "enemydamaged":
                return enemyDamaged;
            case "enemydie":
                return enemyDie;
            case "enemyspawned":
                return enemySpawned;
            
            // UI sounds
            case "uihover":
                return uiHover;
            case "uiclick":
                return uiClick;
            case "uinotenoughcredits":
                return uiNotEnoughCredits;
            case "uipurchaseupgrade":
                return uiPurchaseUpgrade;
            
            // feedback sounds
            case "takedamage":
                return takeDamage;
            
            // level sounds
            case "pickupcurrency":
                return pickUpCurrency;
            case "enterlevel":
                return enterLevel;
            case "exitlevel":
                return exitLevel;
            case "leveltransition":
                return levelTransition;

            // music tracks
            case "floor1_5":
                return floor1_5;
            case "floor6_10":
                return floor6_10;
            case "floor11_15":
                return floor11_15;
            case "mainmenu":
                return mainMenu;
            case "upgradescreen":
                return upgradeScreen;
            case "winscreen":
                return winScreen;

            default:
                return null;
        }
    }

    // volume settings
    public void SetMusicVolume(float volume)
    {
        float clamped = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = clamped;
            if (debug) Debug.Log($"[AudioManager] Applied Music volume to AudioSource: {musicSource.volume:0.###} (from slider {volume:0.###})");
        }
    }

    public void SetSFXVolume(float volume)
    {
        float clamped = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = clamped;
            if (debug) Debug.Log($"[AudioManager] Applied SFX volume to AudioSource: {sfxSource.volume:0.###} (from slider {volume:0.###})");
        }
    }

    public void PlayButtonHover()
    {
        PlaySound("uiHover");
    }

    public void PlayButtonClick()
    {
        PlaySound("uiClick");
    }

    // select gameplay background music based on floor ranges
    public void PlayGameplayForFloor(int floor)
    {
        if (floor >= 1 && floor <= 5)
        {
            PlayMusic(floor1_5 != null ? floor1_5 : backgroundMusic);
            return;
        }

        if (floor >= 6 && floor <= 10)
        {
            PlayMusic(floor6_10 != null ? floor6_10 : backgroundMusic);
            return;
        }

        // floors 11+ (or fallback)
        PlayMusic(floor11_15 != null ? floor11_15 : backgroundMusic);
    }

    // UI helpers for sliders (hook to Slider.onValueChanged)
    public void OnMusicSliderChanged(float value)
    {
        if (debug) Debug.Log($"[AudioManager] Music slider changed: raw={value:0.###}");
        SetMusicVolume(value);
    }

    public void OnSfxSliderChanged(float value)
    {
        if (debug) Debug.Log($"[AudioManager] SFX slider changed: raw={value:0.###}");
        SetSFXVolume(value);
    }

    // Getters for initializing slider values
    public float GetMusicVolume()
    {
        return musicSource != null ? musicSource.volume : PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
    }

    public float GetSfxVolume()
    {
        return sfxSource != null ? sfxSource.volume : PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
    }
}
