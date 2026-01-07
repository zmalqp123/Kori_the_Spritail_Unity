using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootstrapSlideshow : MonoBehaviour
{
    [Header("UI Image in order")]
    [SerializeField] private Image[] images;

    [Header("Timing")]
    [SerializeField] private float holdTime = 0.5f;
    [SerializeField] private float crossFadeTime = 0.6f;
    [SerializeField] private float endDelayTime = 0.8f;

    [Header("Optional : overlay fade (recommended)")]
    [SerializeField] private Image overlayImage;

    private Coroutine slideCorutine;

    private static void SetAlpha(Image img, float alpha)
    { 
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    private void Awake()
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null)
                images[i].raycastTarget = false;
        }

        if (overlayImage != null)
            overlayImage.raycastTarget = false;
    }

    private void OnEnable()
    {
        slideCorutine = StartCoroutine(SlideShowCoroutine());
    }

    private void OnDisable()
    {
        if (slideCorutine != null)
        {
            StopCoroutine(slideCorutine);
            slideCorutine = null;
        }
    }

    private IEnumerator SlideShowCoroutine()
    {
        if (null == images || 0 == images.Length)
        {
            yield break;
        }

        for (int i = 0; i < images.Length; ++i)
        {
            SetAlpha(images[i], i == 0 ? 1f : 0f);
        }

        if (overlayImage != null)
        {
            SetAlpha(overlayImage, 0f);
        }

        for (int i = 0; i < images.Length; ++i)
        {
            yield return new WaitForSeconds(holdTime);

            if (i + 1 < images.Length)
            {
                yield return CrossFade(images[i], images[i + 1], crossFadeTime);
            }
        }

        if (null != overlayImage)
        {
            yield return FadeTo(overlayImage, 1f, endDelayTime);
        }
        else
        {
            yield return FadeTo(images[images.Length - 1], 0f, endDelayTime);
        }


        Debug.Log("ChangeTitleScene");
        SceneManager.LoadScene("TitleScene");
    }

    private IEnumerator CrossFade(Image form, Image to, float sec)
    {
        if (null == form || null == to)
        {
            yield break;
        }

        float t = 0;
        float inv = sec > 0f ? 1f / sec : 0f;

        SetAlpha(to, 0f);

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * inv;
            float a = Mathf.Clamp01(t);

            SetAlpha(form, 1f - a);
            SetAlpha(to, a);
            yield return null;
        }

        SetAlpha(form, 0f);
        SetAlpha(to, 1f);
    }

    private IEnumerator FadeTo(Image img, float targetAlpha, float sec)
    {
        if (null == img) yield break;

        float start = img.color.a;
        float t = 0f;
        float inv = sec > 0f ? 1f / sec : 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * inv;
            float a = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(t));
            SetAlpha(img, a);
            yield return null;
        }

        SetAlpha(img, targetAlpha);
    }
}
