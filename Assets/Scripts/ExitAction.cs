using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitAction : MonoBehaviour
{
    public void Onclick()
    {
        // 빌드(Standalone, 모바일 등): 앱 종료
        Application.Quit();

#if UNITY_EDITOR
        // 에디터에서 테스트 시: Play 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
