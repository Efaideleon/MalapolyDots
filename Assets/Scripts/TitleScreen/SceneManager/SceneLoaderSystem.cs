using UnityEngine.SceneManagement;
using UnityEngine;

public enum SceneID
{
    Menu = 0,
    Game = 1,
}

public class SceneLoadersSystem : MonoBehaviour
{
    public void LoadSceneBySceneIDEnum(SceneID sceneID)
    {
        SceneManager.LoadSceneAsync((int)sceneID);
    }
}
