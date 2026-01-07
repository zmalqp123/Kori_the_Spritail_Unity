using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SwitchingSceneTrigger : MonoBehaviour
{
    private enum SwitchPhase { Hidden, FadingIn, WaitingInput, FadingOut, Switching }
    [Header("UI (Inspector Binding Only)")]
    [SerializeField] private RawImage buttonIcon;        // 변경: 버튼 아이콘(스위치 버튼 이미지)
    [SerializeField] private TMP_Text switchingText;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private Image illustrationImage;

    [Header("Cutscene (Inspector Binding Only)")]
    [Tooltip("Cut1..CutN 순서대로 넣으세요.")]
    [SerializeField] private Image[] cutImages;

    [Header("Illustration Sprites (Optional)")]
    [SerializeField] private Sprite defaultIll;
    [SerializeField] private Sprite bossIll;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Header("Cutscene Options")]
    [SerializeField] private bool isTestMode = false;
    [SerializeField] private bool isTestBossStage = false;
    [SerializeField] private float autoPlayDelay = 1.0f;
    [SerializeField] private bool waitAtLastCut = false;

    private SwitchPhase phase = SwitchPhase.Hidden;
    private float timer;

    private int cutStart;
    private int cutEndExclusive;
    private int cutCursor;
    private bool cutRangeReady;
    private float autoTimer;

    private bool prevSubmit;

    private static float SmoothStep01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x);
    }

    private void OnValidate()
    {
        if (!buttonIcon) Debug.LogWarning($"{name}: buttonIcon(RawImage) not assigned.", this);
        if (!switchingText) Debug.LogWarning($"{name}: switchingText not assigned.", this);
    }

    private void Start()
    {
        SetAlphaAll(0f);
        if (loadingText) SetTextAlpha(loadingText, 1f);

        if (cutImages != null)
        {
            for (int i = 0; i < cutImages.Length; ++i)
                if (cutImages[i]) cutImages[i].enabled = false;
        }

        // 일러스트 분기(필요 시)
        if (illustrationImage && GameManager.Instance != null)
        {
            int next = GameManager.Instance.NextSceneIndex;
            int prev = GameManager.Instance.PrevSceneType;

            const int Scene_SelectChar = 1;
            const int Scene_Boss = 2;

            if (prev == Scene_SelectChar)
            {
                illustrationImage.enabled = true;
                if (defaultIll) illustrationImage.sprite = defaultIll;
            }
            else if (next == Scene_Boss)
            {
                illustrationImage.enabled = true;
                if (bossIll) illustrationImage.sprite = bossIll;
            }
            else
            {
                illustrationImage.enabled = false;
            }
        }

        phase = SwitchPhase.Hidden;
        timer = 0f;
        cutRangeReady = false;
        autoTimer = 0f;
        prevSubmit = false;
    }

    private void Update()
    {
        if (!switchingText) return;

        bool ready = GameManager.Instance != null && GameManager.Instance.IsLoadSceneComplete;

        if (!ready)
        {
            phase = SwitchPhase.Hidden;
            timer = 0f;
            SetAlphaAll(0f);
            prevSubmit = false;
            return;
        }

        switch (phase)
        {
            case SwitchPhase.Hidden:
                timer = 0f;
                phase = SwitchPhase.FadingIn;
                SetAlphaAll(0f);
                break;

            case SwitchPhase.FadingIn:
                {
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / Mathf.Max(0.0001f, fadeInDuration));
                    SetAlphaAll(SmoothStep01(t));

                    if (t >= 1f)
                    {
                        timer = 0f;
                        phase = SwitchPhase.WaitingInput;

                        if (HasCutsceneForNextSceneStrict())
                        {
                            SetupCutRangeForNextScene();
                            if (illustrationImage) illustrationImage.enabled = false;
                        }
                    }
                    break;
                }

            case SwitchPhase.WaitingInput:
                {
                    bool justPressed = IsSubmitJustPressed();
                    bool hasCutscene = HasCutsceneForNextSceneStrict();

                    if (justPressed)
                    {
                        if (!hasCutscene)
                        {
                            phase = SwitchPhase.FadingOut;
                            timer = 0f;
                            break;
                        }

                        if (cutCursor < cutEndExclusive)
                        {
                            ShowCut(cutCursor);
                            ++cutCursor;
                            autoTimer = 0f;
                        }
                        else
                        {
                            phase = SwitchPhase.FadingOut;
                            timer = 0f;
                        }
                    }
                    else
                    {
                        if (hasCutscene)
                        {
                            if (cutCursor < cutEndExclusive)
                            {
                                autoTimer += Time.deltaTime;
                                if (autoTimer >= autoPlayDelay)
                                {
                                    autoTimer = 0f;
                                    ShowCut(cutCursor);
                                    ++cutCursor;
                                }
                            }
                            else
                            {
                                if (!waitAtLastCut)
                                {
                                    phase = SwitchPhase.FadingOut;
                                    timer = 0f;
                                }
                            }
                        }
                        else
                        {
                            phase = SwitchPhase.FadingOut;
                            timer = 0f;
                        }
                    }
                    break;
                }

            case SwitchPhase.FadingOut:
                {
                    timer += Time.deltaTime;
                    float t = Mathf.Clamp01(timer / Mathf.Max(0.0001f, fadeOutDuration));
                    float a = 1f - SmoothStep01(t);
                    SetAlphaAll(a);
                    if (loadingText) SetTextAlpha(loadingText, a);

                    if (t >= 1f)
                        phase = SwitchPhase.Switching;

                    break;
                }

            case SwitchPhase.Switching:
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.SwitchNextScene();

                    phase = SwitchPhase.Hidden;
                    timer = 0f;
                    SetAlphaAll(0f);
                    if (loadingText) SetTextAlpha(loadingText, 0f);
                    break;
                }
        }
    }

    private bool IsSubmitJustPressed()
    {
        bool submit = Input.GetButton("Submit");
        bool just = submit && !prevSubmit;
        prevSubmit = submit;
        return just;
    }

    private void SetAlphaAll(float a)
    {
        // 변경: 버튼은 RawImage alpha로 제어
        if (buttonIcon) SetRawAlpha(buttonIcon, a);

        if (switchingText) SetTextAlpha(switchingText, a);

        // 원본: 일러스트 alpha 반전(1-a)
        if (illustrationImage)
        {
            var c = illustrationImage.color;
            c.a = 1f - a;
            illustrationImage.color = c;
        }
    }

    private static void SetTextAlpha(TMP_Text t, float a)
    {
        var c = t.color;
        c.a = a;
        t.color = c;
    }

    private static void SetRawAlpha(RawImage r, float a)
    {
        var c = r.color;
        c.a = a;
        r.color = c;
    }

    private void SetupCutRangeForNextScene()
    {
        if (cutRangeReady) return;

        int next = GameManager.Instance != null ? GameManager.Instance.NextSceneIndex : -1;

        int start, end;
        const int Scene_Boss = 2;

        if (next == Scene_Boss || isTestBossStage)
        {
            start = 3;
            end = 8;
        }
        else
        {
            start = 0;
            end = 3;
        }

        int n = (cutImages != null) ? cutImages.Length : 0;
        start = Mathf.Clamp(start, 0, n);
        end = Mathf.Clamp(end, start, n);

        cutStart = start;
        cutEndExclusive = end;
        cutCursor = cutStart;
        cutRangeReady = true;

        autoTimer = 0f;
    }

    private bool HasCutsceneForNextSceneStrict()
    {
        if (GameManager.Instance == null) return false;

        int next = GameManager.Instance.NextSceneIndex;
        int prev = GameManager.Instance.PrevSceneType;

        const int Scene_SelectChar = 1;
        const int Scene_Boss = 2;

        return prev == Scene_SelectChar || next == Scene_Boss || isTestMode;
    }

    private void ShowCut(int idx)
    {
        if (cutImages == null) return;
        if ((uint)idx >= (uint)cutImages.Length) return;

        var img = cutImages[idx];
        if (img) img.enabled = true;
    }
}
