using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public partial struct InitializePropertyForSaleSignSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ForSaleComponent>();
            state.RequireForUpdate<ForSaleSignTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            UnityEngine.Debug.Log("Initializing porperty for sale sign components");
            foreach (var (forSaleSign, _) in SystemAPI.Query<RefRW<ForSaleComponent>, RefRO<PropertySpaceTag>>())
            {
                if (SystemAPI.GetSingletonEntity<ForSaleSignTag>() == Entity.Null)
                {
                    UnityEngine.Debug.Log("No ForSaleSign entity");
                }
                else
                {
                    UnityEngine.Debug.Log("ForSaleSign entity exists!");
                }
                forSaleSign.ValueRW.entity = SystemAPI.GetSingletonEntity<ForSaleSignTag>();
            }
        }
    }
}
