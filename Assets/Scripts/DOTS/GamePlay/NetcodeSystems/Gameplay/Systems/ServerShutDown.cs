using Unity.NetCode;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    public class ServerShutDown : MonoBehaviour
    {
        public void Update()
        {
            if (ServerLifecycleBridge.RequestShutdown)
            {
            }
        }
    }
}
