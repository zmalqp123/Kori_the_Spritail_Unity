using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class WeaponSlotItemView : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private RectTransform root;

    [Header("Icons (stacked)")]
    [SerializeField] private Image iconNormal;     // Normal icon layer
    [SerializeField] private Image iconActive;     // Active icon layer (enabled when selected)

    [Header("Durability")]
    [SerializeField] private Image durability;     // Image Type=Filled, Radial360, Clockwise=true

    [Header("Empty")]
    [SerializeField] private GameObject emptyGroup; // or Image only (use GO for easy toggle)
    [SerializeField] private Image emptyIcon;       // Empty slot icon

    [Header("Text BG + Slot Index")]
    [SerializeField] private Image textBg;
    [SerializeField] private TMP_Text slotIndexText; // optional

    [Header("Optional Visuals")]
    [SerializeField] private GameObject selectedFx; // optional highlight/glow
    [SerializeField] private Image selectedOutline;
    // State
    private WeaponIconSet iconSet;
    private int slotIndex = -1;
    private bool hasWeapon = false;
    private bool isSelected = false;

    /// <summary>
    /// 슬롯 인덱스(0~3) 설정. 텍스트가 있으면 표시합니다.
    /// </summary>
    public void SetSlotIndex(int index)
    {
        slotIndex = index;

        if (slotIndexText != null)
            slotIndexText.text = index.ToString();
    }

    /// <summary>
    /// 이 슬롯이 사용할 IconSet(근거리/원거리/폭탄/기본무기)을 지정합니다.
    /// 슬롯이 "무기 있음" 상태일 때 이 세트가 적용됩니다.
    /// </summary>
    public void SetIconSet(WeaponIconSet set)
    {
        iconSet = set;
        ApplyStaticSprites();   // gauge/text bg/empty 같은 고정 요소 적용
        RefreshVisual();
    }

    /// <summary>
    /// 무기 보유 여부(빈 슬롯/무기 있음)
    /// </summary>
    public void SetHasWeapon(bool value)
    {
        hasWeapon = value;
        RefreshVisual();
    }

    /// <summary>
    /// 선택 상태(활성 아이콘/강조)
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        RefreshVisual();
    }

    /// <summary>
    /// 내구도 비율(0~1). 무기 없는 슬롯이면 자동으로 숨김 처리됩니다.
    /// </summary>
    public void SetDurability01(float value01)
    {
        if (durability == null) return;

        float v = Mathf.Clamp01(value01);
        durability.fillAmount = v;

        // 무기 있을 때만 보이게(원하면 always on으로 바꿔도 됨)
        if (hasWeapon)
            durability.gameObject.SetActive(true);
    }

    /// <summary>
    /// 내구도 무한대 등: 게이지를 아예 끄고 싶을 때 사용(0번 슬롯 등)
    /// </summary>
    public void SetDurabilityVisible(bool visible)
    {
        if (durability != null)
            durability.gameObject.SetActive(visible);
    }

    /// <summary>
    /// 외부에서 한 번에 업데이트하기 좋은 스냅샷 API (선택)
    /// </summary>
    public void ApplySnapshot(bool hasWeapon, bool selected, float durability01, WeaponIconSet set)
    {
        iconSet = set;
        this.hasWeapon = hasWeapon;
        isSelected = selected;

        ApplyStaticSprites();
        RefreshVisual();

        // RefreshVisual 후 게이지 반영
        if (this.hasWeapon)
            SetDurability01(durability01);
        else
            SetDurabilityVisible(false);
    }

    private void Awake()
    {
        if (root == null) root = transform as RectTransform;

        // 안전: 게이지 Image 타입이 Filled가 아니라면 최소한 경고(런타임 망가짐 방지)
        // (원하면 로그 제거 가능)
        if (durability != null && durability.type != Image.Type.Filled)
        {
            Debug.LogWarning($"[{name}] Durability Image 타입이 Filled가 아닙니다. (현재: {durability.type})");
        }

        RefreshVisual();
    }

    /// <summary>
    /// IconSet에서 변하지 않는 스프라이트(게이지/텍스트배경/빈 슬롯) 적용
    /// </summary>
    private void ApplyStaticSprites()
    {
        if (iconSet == null) return;

        if (selectedOutline != null && iconSet.SelectedOutlineSprite != null)
            selectedOutline.sprite = iconSet.SelectedOutlineSprite;

        if (durability != null && iconSet.GaugeSprite != null)
            durability.sprite = iconSet.GaugeSprite;

        if (textBg != null && iconSet.TextBgSprite != null)
            textBg.sprite = iconSet.TextBgSprite;

        // empty 아이콘은 “공통”일 수 있으므로 세트에 있으면 적용
        if (emptyIcon != null && iconSet.EmptySlotIcon != null)
            emptyIcon.sprite = iconSet.EmptySlotIcon;
    }

    /// <summary>
    /// 현재 상태(hasWeapon/isSelected/iconSet)에 따라 레이어 토글
    /// </summary>
    private void RefreshVisual()
    {
        // 빈 슬롯 처리
        bool isEmpty = !hasWeapon;

        if (emptyGroup != null)
            emptyGroup.SetActive(isEmpty);

        if (iconNormal != null)
            iconNormal.gameObject.SetActive(!isEmpty && !isSelected);

        if (iconActive != null)
            iconActive.gameObject.SetActive(!isEmpty && isSelected);



        if (selectedFx != null)
            selectedFx.SetActive(!isEmpty && isSelected);

        if (textBg != null)
            textBg.gameObject.SetActive(!isEmpty);

        if (slotIndexText != null)
            slotIndexText.gameObject.SetActive(!isEmpty);

        // 아이콘 스프라이트 적용
        if (!isEmpty && iconSet != null)
        {
            if (iconNormal != null && iconSet.NormalIcon != null)
                iconNormal.sprite = iconSet.NormalIcon;

            if (iconActive != null && iconSet.ActiveIcon != null)
                iconActive.sprite = iconSet.ActiveIcon;
        }

        // 게이지 기본 토글: 무기 없으면 숨김
        if (durability != null)
        {
            if (isEmpty)
            {
                durability.gameObject.SetActive(false);
                durability.fillAmount = 0f;
            }
            else
            {
                // 무기 있으면 기본적으로 보이게 (0번 슬롯은 외부에서 SetDurabilityVisible(false)로 끄면 됨)
                durability.gameObject.SetActive(true);
            }
        }
    }

#if UNITY_EDITOR
    // 에디터에서 값 바꾸면 즉시 반영(프리팹 작업 편의)
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (root == null) root = transform as RectTransform;
            ApplyStaticSprites();
            RefreshVisual();
        }
    }
#endif
}
