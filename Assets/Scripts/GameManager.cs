using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int NextSceneIndex { get; private set; } = -1;
    public int PrevSceneType { get; private set; } = -1;   // 필요 시
    public bool IsLoadSceneComplete { get; private set; }

    private AsyncOperation _loadOp;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetNextScene(int nextIndex, int prevSceneType = -1)
    {
        NextSceneIndex = nextIndex;
        PrevSceneType = prevSceneType;
        IsLoadSceneComplete = false;
    }

    // (로딩씬에서) 비동기 로드가 끝났다고 판단되면 true로 바꾸는 방식도 가능
    public void MarkLoadComplete() => IsLoadSceneComplete = true;

    public void LoadNextSceneAsync()
    {
        if (NextSceneIndex < 0) return;
        StartCoroutine(CoLoad());
    }

    private IEnumerator CoLoad()
    {
        IsLoadSceneComplete = false;
        _loadOp = SceneManager.LoadSceneAsync(NextSceneIndex, LoadSceneMode.Single);
        _loadOp.allowSceneActivation = true;

        while (!_loadOp.isDone)
            yield return null;

        IsLoadSceneComplete = true;
        // NextSceneIndex는 Switching에서 실제 SwitchNextScene 호출 시 쓰는 구조면 여기서 지우지 마세요.
    }

    public void SwitchNextScene()
    {
        // 질문 코드 상 "Switching" 단계에서 한 번만 호출되면 되므로 즉시 전환 버전
        if (NextSceneIndex < 0) return;
        SceneManager.LoadScene(NextSceneIndex, LoadSceneMode.Single);
    }
}
