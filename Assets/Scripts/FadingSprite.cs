using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadingSprite : MonoBehaviour
{
    public SpriteRenderer sprite;
    public float fadeTime = 1f;

    void Start()
    {
        FadeIn();
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInIEnum());
    }
    public void FadeOut()
    {
        StartCoroutine(FadeOutIEnum());
    }

    private IEnumerator FadeInIEnum()
    {
        Color originalColor = sprite.color;
        for (float t = 0.01f; t < fadeTime; t += Time.deltaTime)
        {
            sprite.color = Color.Lerp(originalColor, Color.white, Mathf.Min(1, t / fadeTime));
            yield return null;
        }
    }

    private IEnumerator FadeOutIEnum()
    {
        Color originalColor = sprite.color;
        for (float t = 0.01f; t < fadeTime; t += Time.deltaTime)
        {
            sprite.color = Color.Lerp(originalColor, Color.clear, Mathf.Min(1, t / fadeTime));
            yield return null;
        }
    }
}