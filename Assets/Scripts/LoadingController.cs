using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LoadingController : MonoBehaviour
{
    [Header("Auto-wired if empty")]
    [SerializeField] private RawImage loadingImage;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private RawImage koriIcon;

    [Header("Tuning")]
    [SerializeField] private float rotateDegreePerSec = 180f;
    [SerializeField] private float charInterval = 0.1f;

    private float timer;
    private int idx;

    private static readonly string Base = "Loading...";
    private static readonly List<string> Frames = new();

    private void Awake()
    {
        if (Frames.Count == 0)
        {
            Frames.Capacity = Base.Length + 1;
            for (int i = 0; i <= Base.Length; ++i)
                Frames.Add(Base.Substring(0, i));
        }
    }

    private void Start()
    {
        // C++: GameManager 찾아서 NextSceneIndex 설정 후 LoadNextScene 호출
        if (GameManager.Instance != null)
        {
            // 이미 외부에서 SetNextSceneIndex 해두었다는 가정(가장 깔끔)
            GameManager.Instance.LoadNextSceneAsync();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsLoadSceneComplete)
        {
            if (loadingImage)
                loadingImage.rectTransform.Rotate(0f, 0f, -rotateDegreePerSec * Time.deltaTime);

            if (loadingText)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    timer += charInterval;
                    idx = (idx + 1) % Frames.Count;
                    loadingText.text = Frames[idx];
                }
            }
        }
        else
        {
            if (loadingImage) loadingImage.enabled = false;
            if (koriIcon) koriIcon.enabled = false;
            if (loadingText) loadingText.text = "Load Complete";
        }
    }
}
