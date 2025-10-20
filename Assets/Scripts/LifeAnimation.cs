using UnityEngine;
using System.Collections;

public class LifeAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float birthAnimationDuration = 0.3f;
    [SerializeField] private float deathAnimationDuration = 0.3f;
    
    [Header("Materials")]
    [SerializeField] private Material birthMaterial;
    [SerializeField] private Material aliveMaterial;
    [SerializeField] private Material deathMaterial;
    [SerializeField] private Material deadMaterial;

    private bool isAlive = false;

    private void Start()
    {
        
    }

    public void SetAlive(bool state)
    {
        isAlive = state;
    }
}