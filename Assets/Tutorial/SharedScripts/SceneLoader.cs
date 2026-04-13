using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public void LoadNextScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex + 1 < SceneManager.sceneCountInBuildSettings)
            LoadScene(currentSceneIndex + 1);
        else LoadScene(0);
    }
    private void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
