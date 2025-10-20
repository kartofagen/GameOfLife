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

    private bool isAlive = false;
    private SpriteRenderer spriteRenderer;
    private Coroutine currentAnimation;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        UpdateSpriteImmediate();
    }

    public void SetAlive(bool state)
    {
        if (isAlive == state) return;
        
        isAlive = state;
        
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        currentAnimation = StartCoroutine(AnimateStateChange());
    }

    private IEnumerator AnimateStateChange()
    {
        if (isAlive)
        {
            spriteRenderer.sprite = birthSprite;
            yield return new WaitForSeconds(birthAnimationDuration);
            spriteRenderer.sprite = growSprite;
            yield return new WaitForSeconds(growAnimationDuration);
            spriteRenderer.sprite = aliveSprite;
        }
        else
        {
            spriteRenderer.sprite = deathSprite;
            yield return new WaitForSeconds(deathAnimationDuration);
            spriteRenderer.sprite = death2Sprite;
            yield return new WaitForSeconds(death2AnimationDuration);
            spriteRenderer.sprite = deadSprite;
        }
        
        currentAnimation = null;
    }

    private void UpdateSpriteImmediate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = isAlive ? aliveSprite : deadSprite;
        }
    }
}