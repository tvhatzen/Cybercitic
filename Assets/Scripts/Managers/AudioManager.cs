using UnityEngine;

public class AudioManager : SingletonBase<AudioManager>
{
    [SerializeField] private AudioClip pickUpCurrency;
    [SerializeField] private AudioClip backgroundMusic;

    [Header("UI")]
    [SerializeField] private AudioClip uiHover;
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private AudioClip uiNotEnoughCredits;
    [SerializeField] private AudioClip uiPurchaseUpgrade;

    [Header("Feedback")]
    [SerializeField] private AudioClip takeDamage;

    public bool debug = false;
    public void PlaySFX(AudioClip clip)
    {
        
    }
}
