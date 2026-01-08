using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIInventorySceneLoader : MonoBehaviour
{
    [SerializeField] private string uiSceneName = "UI_inventory";

    private void Awake()
    {
        // якщо цей лоадер має жити завжди — можна теж DontDestroyOnLoad
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(LoadUIAdditiveIfNeeded());
    }

    private IEnumerator LoadUIAdditiveIfNeeded()
    {
        if (IsSceneLoaded(uiSceneName))
            yield break;

        var op = SceneManager.LoadSceneAsync(uiSceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;
    }

    private bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.name == sceneName) return true;
        }
        return false;
    }
}
