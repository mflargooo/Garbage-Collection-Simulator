using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadScene(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }

    public void NextScene()
    {
        SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCount);
    }

    public void PrevScene()
    {
        SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex - 1 + SceneManager.sceneCount) % SceneManager.sceneCount);
    }
}
