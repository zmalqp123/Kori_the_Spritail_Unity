using UnityEngine;
using UnityEngine.UI;

/// HP 제공 인터페이스 (원하면 엔티티 스크립트에서 구현)
public interface IHasHP
{
    int CurrentHP { get; }
    int MaxHP { get; }
}

/// <summary>
/// UGUI HPBar:
/// - 월드 타겟을 따라다니며 World->Screen->CanvasLocal로 변환해 anchoredPosition 갱신
/// - Image.fillAmount로 HP 비율 표시
/// - (옵션) Warning 이미지 알파를 사인 블링크 + 스무딩으로 표시
///
/// 오른쪽으로 밀리는 문제 방지:
/// - 변환 기준 RectTransform은 반드시 Canvas
/// - UI 카메라는 Canvas.renderMode에 따라 정확히 선택
/// </summary>
public sealed class HPBarScript : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [Tooltip("캐릭터 머리 위로 띄우기 위한 월드 오프셋(미터 단위)")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.8f, 0f);

    [Header("UI Position")]
    [Tooltip("캔버스 로컬 좌표에서 추가로 더할 픽셀 오프셋")]
    [SerializeField] private Vector2 screenOffset = Vector2.zero;

    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform rect; // HPBar 루트 RectTransform
    [SerializeField] private Image hpFill;       // 전면 HP Fill
    [SerializeField] private Image warningImage; // 경고 오버레이(선택)

    [Header("Cameras")]
    [Tooltip("월드->스크린 투영에 사용할 월드 카메라 (미지정 시 Camera.main)")]
    [SerializeField] private Camera worldCamera;

    [Tooltip("Canvas가 ScreenSpace-Camera/WorldSpace일 때 사용할 UI 카메라. 보통 canvas.worldCamera로 자동 사용")]
    [SerializeField] private Camera overrideUiCamera;

    [Header("HP Source")]
    [Tooltip("타겟에서 HP를 읽을 컴포넌트(IHasHP). 비워두면 target에서 자동 GetComponent 시도")]
    [SerializeField] private MonoBehaviour hpSource; // IHasHP 기대

    [Header("HP Fallback (IHasHP 없을 때)")]
    [SerializeField] private int currentHP = 100;
    [SerializeField] private int maxHP = 100;

    [Header("Warning (Optional)")]
    [Range(0f, 1f)]
    [SerializeField] private float warningPercent = 0.2f;
    [SerializeField] private float blinkHz = 0.5f;
    [SerializeField] private float minWarnAlpha = 0.15f;
    [SerializeField] private float maxWarnAlpha = 1.0f;
    [SerializeField] private float alphaSmoothing = 0.2f;

    private IHasHP hp;
    private float blinkPhase01;

    private void Reset()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void Awake()
    {
        if (!rect) rect = GetComponent<RectTransform>();
        if (!canvas) canvas = GetComponentInParent<Canvas>();

        // HP 소스 연결
        hp = hpSource as IHasHP;
        if (hp == null && target != null)
            hp = target.GetComponent<IHasHP>();

        // 경고 알파 초기화
        if (warningImage != null)
        {
            var c = warningImage.color;
            c.a = 0f;
            warningImage.color = c;
        }
    }

    private void LateUpdate()
    {
        // 필수 조건
        if (target == null || rect == null || hpFill == null || canvas == null)
            return;

        // 월드 카메라 확보
        if (worldCamera == null)
            worldCamera = Camera.main;
        if (worldCamera == null)
            return;

        // 1) HP 계산
        int cur, max;
        if (hp != null)
        {
            cur = hp.CurrentHP;
            max = hp.MaxHP;
        }
        else
        {
            cur = currentHP;
            max = maxHP;
        }

        if (max <= 0) max = 1;
        cur = Mathf.Clamp(cur, 0, max);
        float ratio = Mathf.Clamp01((float)cur / max);

        // 2) Fill 반영
        hpFill.fillAmount = ratio;

        // 3) 위치 갱신 (중요: 변환 파이프라인 정확히)
        UpdateAnchoredPosition();

        // 4) Warning 갱신 (선택)
        UpdateWarningAlpha(ratio, Time.unscaledDeltaTime);
    }

    private void UpdateAnchoredPosition()
    {
        Vector3 sp = worldCamera.WorldToScreenPoint(target.position + worldOffset);
        if (sp.z <= 0f) return;

        RectTransform canvasRect = (RectTransform)canvas.transform;

        // Canvas 모드에 맞는 UI 카메라만 사용
        Camera uiCam = null;
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCam = canvas.worldCamera;
            if (uiCam == null) return; // ScreenSpace-Camera/WorldSpace면 반드시 있어야 함
        }

        // 핵심: localPoint는 canvasRect 기준 로컬좌표
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, sp, uiCam, out Vector2 lp))
        {
            rect.anchoredPosition = lp + screenOffset; // 현재 screenOffset=0,0이면 lp 그대로
        }
    }


    private void UpdateWarningAlpha(float ratio01, float dt)
    {
        if (warningImage == null) return;

        float threshold = Mathf.Clamp01(warningPercent);

        float shortage = Mathf.Max(0f, threshold - ratio01);
        float baseAlpha = (threshold > 0f) ? (shortage / threshold) : 0f;

        float currentA = warningImage.color.a;

        if (baseAlpha > 0f)
        {
            blinkPhase01 += dt * blinkHz;
            blinkPhase01 -= Mathf.Floor(blinkPhase01);

            float blink01 = 0.5f + 0.5f * Mathf.Sin(Mathf.PI * 2f * blinkPhase01);
            float warnFactor = baseAlpha * blink01;

            float targetA = Mathf.Lerp(minWarnAlpha, maxWarnAlpha, Mathf.Clamp01(warnFactor));
            float newA = currentA + (targetA - currentA) * alphaSmoothing;

            SetWarningAlpha(newA);
        }
        else
        {
            float newA = currentA + (0f - currentA) * alphaSmoothing;
            SetWarningAlpha(newA);
            blinkPhase01 = 0f;
        }
    }

    private void SetWarningAlpha(float a)
    {
        Color c = warningImage.color;
        c.a = Mathf.Clamp01(a);
        warningImage.color = c;
    }

    // ---------- Public API ----------

    public void SetTarget(Transform t, MonoBehaviour hpProvider = null)
    {
        target = t;
        hp = hpProvider as IHasHP;
        if (hp == null && target != null)
            hp = target.GetComponent<IHasHP>();
    }

    public void SetHP(int cur, int max)
    {
        maxHP = Mathf.Max(1, max);
        currentHP = Mathf.Clamp(cur, 0, maxHP);
    }

    public void SetWorldOffset(Vector3 offset) => worldOffset = offset;
    public void SetScreenOffset(Vector2 offset) => screenOffset = offset;

    /// 디버깅용: 현재 캔버스/카메라 설정 상태를 로그로 확인
    [ContextMenu("Debug Print Canvas Camera State")]
    private void DebugPrint()
    {
        string mode = canvas ? canvas.renderMode.ToString() : "NULL";
        string cw = (canvas && canvas.worldCamera) ? canvas.worldCamera.name : "NULL";
        string ow = overrideUiCamera ? overrideUiCamera.name : "NULL";
        string ww = worldCamera ? worldCamera.name : "NULL";
        Debug.Log($"[HPBar] CanvasMode={mode}, canvas.worldCamera={cw}, overrideUiCamera={ow}, worldCamera={ww}");
    }
}
