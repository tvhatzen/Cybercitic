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

    private void Start()
    {
        GameEvents.OnSoundRequested += HandleSoundRequest;
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
            //Debug.Log("AudioManager: playing music - " + clip.name);
        }
    }

    // play music by string name 
    public void PlayMusicTrack(string trackName)
    {
        //Debug.Log("AudioManager: Attempting to play music track - " + trackName);
        AudioClip clip = GetSoundClip(trackName);
        if (clip != null)
        {
            PlayMusic(clip);
        }
    }

    // play sound by AudioClip
    public void PlaySound(AudioClip clip)
    {
        if(sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
            //Debug.Log("AudioManager: playing sound - " + clip.name);
        }
    }

    // play sound by string name 
    public void PlaySound(string soundName)
    {
        //Debug.Log("AudioManager: Attempting to play sound - " + soundName);
        AudioClip clip = GetSoundClip(soundName);
        if (clip != null)
        {
            PlaySound(clip);
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
            case "mainMenu":
                return mainMenu;
            case "upgradeScreen":
                return upgradeScreen;
            case "winScreen":
                return winScreen;

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

    public void PlayButtonHover()
    {
        PlaySound("uiHover");
    }

    public void PlayButtonClick()
    {
        PlaySound("uiClick");
    }
}
