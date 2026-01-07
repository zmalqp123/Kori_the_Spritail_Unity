using UnityEngine;

public enum WeaponSlotKind
{
    Default = 0,   // 0번 기본무기(고정)
    Melee = 1,   // 근거리
    Ranged = 2,   // 원거리
    Bomb = 3    // 폭탄
}

/// <summary>
/// 무기 슬롯 UI에 필요한 스프라이트 묶음.
/// (Normal/Active/게이지/텍스트BG + 빈슬롯 아이콘)
/// </summary>
[CreateAssetMenu(menuName = "UI/Weapon/Weapon Icon Set", fileName = "WeaponIconSet_")]
public sealed class WeaponIconSet : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private WeaponSlotKind kind;

    [Header("Weapon Icons")]
    [SerializeField] private Sprite normalIcon;   // 예: 2_MeleeWeapon.png
    [SerializeField] private Sprite activeIcon;   // 예: 2_MeleeWeapon_Active.png

    [Header("UI Overlays")]
    [SerializeField] private Sprite gaugeSprite;  // 예: 2_MeleeWeaponGage.png (Image Filled에 사용)
    [SerializeField] private Sprite textBgSprite; // 예: 2_MeleeWeaponTextBg.png (슬롯 번호 배경)
    [SerializeField] private Sprite selectedOutlineSprite; // ★ 추가: 선택 외곽선

    [Header("Empty Slot")]
    [SerializeField] private Sprite emptySlotIcon; // 빈 슬롯 아이콘 (공통이어도 OK)

    [Header("Optional")]
    [Tooltip("아이콘이 없음/활성 없음 같은 특수 케이스를 허용할지")]
    [SerializeField] private bool allowMissingSprites = false;

    public WeaponSlotKind Kind => kind;

    public Sprite NormalIcon => normalIcon;
    public Sprite ActiveIcon => activeIcon;

    public Sprite GaugeSprite => gaugeSprite;
    public Sprite TextBgSprite => textBgSprite;
    public Sprite SelectedOutlineSprite => selectedOutlineSprite;

    public Sprite EmptySlotIcon => emptySlotIcon;

    /// <summary>
    /// UI가 바로 사용할 수 있는 최소 완결성 검사.
    /// </summary>
    public bool IsValid(out string reason)
    {
        // 빈 슬롯 아이콘은 "전체 공통 세트"로 별도 관리할 수도 있으므로,
        // 여기서는 allowMissingSprites가 false일 때만 강제합니다.
        if (!allowMissingSprites)
        {
            if (normalIcon == null) { reason = "NormalIcon이 비어 있습니다."; return false; }
            if (activeIcon == null) { reason = "ActiveIcon이 비어 있습니다."; return false; }
            if (gaugeSprite == null) { reason = "GaugeSprite가 비어 있습니다."; return false; }
            if (textBgSprite == null) { reason = "TextBgSprite가 비어 있습니다."; return false; }
            if (emptySlotIcon == null) { reason = "EmptySlotIcon이 비어 있습니다."; return false; }
            if (selectedOutlineSprite == null) { reason = "SelectedOutlineSprite가 비어 있습니다."; return false; }
        }

        reason = null;
        return true;
    }
}
