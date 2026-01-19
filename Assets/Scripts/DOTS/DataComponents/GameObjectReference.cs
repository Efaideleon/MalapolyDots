using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.DataComponents
{
    public class GameObjectReference : IComponentData, System.IDisposable
    {
        public GameObject Instance;
        public void Dispose()
        {
            if (Instance != null)
            {
                UnityEngine.Debug.Log($"[GameObjectReference] | Destroying GameObject");
                Object.Destroy(Instance);
            }
        }
    }
}
