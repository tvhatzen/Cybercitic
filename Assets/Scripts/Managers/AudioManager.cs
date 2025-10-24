using UnityEngine;

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

    [Header("UI")]
    [SerializeField] private AudioClip uiHover;
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip uiNotEnoughCredits;
    [SerializeField] private AudioClip uiPurchaseUpgrade;

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

    private void Start()
    {
        // Subscribe to Event Bus for audio requests
        GameEvents.OnSoundRequested += HandleSoundRequest;
        GameEvents.OnMusicRequested += HandleMusicRequest;
    }

    private void OnDestroy()
    {
        // Unsubscribe from Event Bus
        GameEvents.OnSoundRequested -= HandleSoundRequest;
        GameEvents.OnMusicRequested -= HandleMusicRequest;
    }

    // Event Bus handlers
    private void HandleSoundRequest(string soundName)
    {
        PlaySound(soundName);
    }

    private void HandleMusicRequest(AudioClip musicClip)
    {
        PlayMusic(musicClip);
    }

    // Play music
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true; // Loop the background music
            musicSource.Play();
            if (debug) Debug.Log("AudioManager: playing music - " + clip.name);
        }
    }

    // Play sound by AudioClip
    public void PlaySound(AudioClip clip)
    {
        if(sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
            if (debug) Debug.Log("AudioManager: playing sound - " + clip.name);
        }
    }

    // Play sound by string name 
    public void PlaySound(string soundName)
    {
        AudioClip clip = GetSoundClip(soundName);
        if (clip != null)
        {
            PlaySound(clip);
        }
        else
        {
            if (debug) Debug.LogWarning($"AudioManager: Sound '{soundName}' not found!");
        }
    }

    // Get AudioClip by string name
    private AudioClip GetSoundClip(string soundName)
    {
        switch (soundName.ToLower())
        {
            // Player sounds
            case "attack":
                return attack;
            case "damaged":
                return damaged;
            case "die":
                return die;
            case "useskill":
                return useSkill;
            
            // Enemy sounds
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
            
            // Feedback sounds
            case "takedamage":
                return takeDamage;
            
            // Level sounds
            case "pickupcurrency":
                return pickUpCurrency;
            case "enterlevel":
                return enterLevel;
            case "exitlevel":
                return exitLevel;
            case "leveltransition":
                return levelTransition;
            
            default:
                return null;
        }
    }

    // volume settings
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
