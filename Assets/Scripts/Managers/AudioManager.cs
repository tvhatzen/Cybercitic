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
    public static AudioClip attack;
    [SerializeField] private AudioClip damaged;
    [SerializeField] private AudioClip die;
    [SerializeField] private AudioClip useSkill;

    [Header("Enemy")]
    public static AudioClip enemyAttack;
    [SerializeField] private AudioClip enemyDamaged;
    [SerializeField] private AudioClip enemyDie;
    [SerializeField] private AudioClip enemySpawned;

    #endregion

    public bool debug = false;

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true; // Loop the background music
            musicSource.Play();
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if(sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
            Debug.Log("AudioManager: playing sound - " + clip.name);
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
