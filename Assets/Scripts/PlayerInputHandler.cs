using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class ActivePadRouter : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    // 외부(플랫폼/하드웨어 레이어)에서 0~3이 넘어온다고 가정
    public void OnHardwareActiveIndexChanged(int activeIndex)
    {
        if (playerInput == null) return;
        if (activeIndex < 0 || activeIndex > 3) return;
        if (Gamepad.all.Count <= activeIndex) return;

        var targetPad = Gamepad.all[activeIndex];
        var user = playerInput.user;

        // 1) 기존 페어링 제거
        user.UnpairDevices();

        // 2) 새 패드 페어링
        InputUser.PerformPairingWithDevice(targetPad, user: user);

        // 3) 스킴 전환(스킴 이름은 프로젝트의 Control Schemes와 일치해야 함)
        playerInput.SwitchCurrentControlScheme("Gamepad", targetPad);

        // 4) 갱신
        playerInput.ActivateInput();
    }
}
