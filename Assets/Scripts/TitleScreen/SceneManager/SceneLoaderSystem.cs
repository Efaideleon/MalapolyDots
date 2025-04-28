using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneID
{
    Default = -1,
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
