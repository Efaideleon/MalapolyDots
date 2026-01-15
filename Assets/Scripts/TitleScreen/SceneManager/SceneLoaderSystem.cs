using UnityEngine;

namespace TitleScreen.SceneManager
{
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
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync((int)sceneID);
        }
    }
}
