using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    public void DoOnContinueButtonClick()
    {
        Debug.Log("Continue stub");
    }

    public void DoOnNewButtonClick()
    {
        SceneManager.LoadScene("Intro");
    }

    public void DoOnOptionsButtonClick()
    {
        Debug.Log("Settings stub");
    }

    public void DoOnExitButtonClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
