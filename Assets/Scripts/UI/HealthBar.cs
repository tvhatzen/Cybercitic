using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _healthBarSprite;
    
    [Header("Animation Settings")]
    [SerializeField] private float _lerpSpeed = 5f;
    [SerializeField] private bool _smoothTransition = true;
    
    private Coroutine _healthLerpCoroutine;
    private float _targetFillAmount;
    
    public void UpdateHealthBar(float maxHealth, float currentHealth)
    {
        _targetFillAmount = currentHealth / maxHealth;
        
        if (_smoothTransition)
        {
            // Stop any existing lerp coroutine
            if (_healthLerpCoroutine != null)
            {
                StopCoroutine(_healthLerpCoroutine);
            }
            
            // Start new lerp coroutine
            _healthLerpCoroutine = StartCoroutine(LerpHealthBar());
        }
        else
        {
            _healthBarSprite.fillAmount = _targetFillAmount;
        }
    }
    
    private IEnumerator LerpHealthBar()
    {
        float startFillAmount = _healthBarSprite.fillAmount;
        float elapsedTime = 0f;
        
        while (Mathf.Abs(_healthBarSprite.fillAmount - _targetFillAmount) > 0.001f)
        {
            elapsedTime += Time.deltaTime * _lerpSpeed;
            _healthBarSprite.fillAmount = Mathf.Lerp(startFillAmount, _targetFillAmount, elapsedTime);
            yield return null;
        }
        
        _healthBarSprite.fillAmount = _targetFillAmount;
        _healthLerpCoroutine = null;
    }
    
    // instantly set health bar without animation
    public void SetHealthBarInstant(float maxHealth, float currentHealth)
    {
        _targetFillAmount = currentHealth / maxHealth;
        _healthBarSprite.fillAmount = _targetFillAmount;
        
        // stop any running coroutine
        if (_healthLerpCoroutine != null)
        {
            StopCoroutine(_healthLerpCoroutine);
            _healthLerpCoroutine = null;
        }
    }
}
