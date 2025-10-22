using UnityEngine;
using System.Collections;

public class LifeAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float birthAnimationDuration = 0.15f;
    [SerializeField] private float growAnimationDuration = 0.15f;
    [SerializeField] private float deathAnimationDuration = 0.15f;
    [SerializeField] private float death2AnimationDuration = 0.15f;
    
    [Header("Sprites")]
    [SerializeField] private Sprite birthSprite;
    [SerializeField] private Sprite growSprite;
    [SerializeField] private Sprite aliveSprite;
    [SerializeField] private Sprite deathSprite;
    [SerializeField] private Sprite death2Sprite;
    [SerializeField] private Sprite deadSprite;

    private bool _isAlive = false;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _currentAnimation;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        UpdateSpriteImmediate();
    }

    public void SetAlive(bool state)
    {
        if (_isAlive == state) return;
        
        _isAlive = state;
        
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }
        
        _currentAnimation = StartCoroutine(AnimateStateChange());
    }

    private IEnumerator AnimateStateChange()
    {
        if (_isAlive)
        {
            _spriteRenderer.sprite = birthSprite;
            yield return new WaitForSeconds(birthAnimationDuration);
            _spriteRenderer.sprite = growSprite;
            yield return new WaitForSeconds(growAnimationDuration);
            _spriteRenderer.sprite = aliveSprite;
        }
        else
        {
            _spriteRenderer.sprite = deathSprite;
            yield return new WaitForSeconds(deathAnimationDuration);
            _spriteRenderer.sprite = death2Sprite;
            yield return new WaitForSeconds(death2AnimationDuration);
            _spriteRenderer.sprite = deadSprite;
        }
        
        _currentAnimation = null;
    }

    private void UpdateSpriteImmediate()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = _isAlive ? aliveSprite : deadSprite;
        }
    }
}