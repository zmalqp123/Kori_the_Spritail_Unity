using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneChange : MonoBehaviour
{
    public void OnClick()
    {
        Debug.Log("Start");
        SceneManager.LoadScene("BossScene");
    }
}
